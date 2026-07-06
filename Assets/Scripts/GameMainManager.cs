using System;
using System.Runtime;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using API;

public class GameMainManager : MonoBehaviour
{
    [Header("Manager")]
    public SimaiDataLoader simailoader;
    public AudioTimeProvider timeProvider;
    public BGManager bgManager;
    public SpriteRenderer bgCover;
    public MultTouchHandler multTouchHandler;
    public ObjectCounter objectCounter;
    public Transform Notes;
    public SoundEffect SE;
    public MenuManager menuManager;
    public SettingsManager settings;

    [Space(10)]
    [Header("AudioRef")]
    public AudioSource bgm;

    [Space(10)]
    [Header("Settings")]
    public float startTime = 0f;
    public float audioSpeed = 1f;
    public float offset;

    [Space(10)]
    [Header("Debug")]
    public string editorInitPath;

    private bool inited = false;
    private int status = 0;
    private Coroutine videoSeekCoroutine;
    private int videoSeekGeneration = 0;

    public void Play()
    {
        simailoader.noteSpeed = settings.noteSpeed;
        simailoader.touchSpeed = settings.touchSpeed;
        simailoader.PlayLevel(startTime);
        timeProvider.SetStartTime(startTime - offset, audioSpeed);
        objectCounter.ComboSetActive(settings.combo);
        multTouchHandler.clearSlots();
        Notes.GetComponent<PlayAllPerfect>().enabled = false;
        inited = true;
        menuManager.SetPlayMode();

        QueueVideoSeek(startTime, true, "Play");
    }

    public void OnPlayPauseButtonClick()
    {
        if (!inited)
        {
            Play();
            return;
        }

        if (timeProvider.isStart)
        {
            startTime = timeProvider.AudioTime;
            timeProvider.playStartTime = startTime;
            timeProvider.Pause();

            if (bgManager != null && bgManager.videoPlayer != null && bgManager.videoPlayer.isPrepared)
            {
                bgManager.videoPlayer.Pause();
                bgManager.SetIdleVideoFrameVisible(true, "Pause");
            }

            menuManager.SetPauseMode();
        }
        else
        {
            timeProvider.Resume();
            QueueVideoSeek(startTime, true, "Resume");
            menuManager.SetPlayMode();
        }
    }

    public void OnStopButtonClick()
    {
        OnStopButtonClick(true);
    }

    public void OnStopButtonClick(bool stopVideo)
    {
        if (bgCover != null)
        {
            bgCover.color = new Color(0f, 0f, 0f, 0f);
        }

        if (timeProvider != null)
        {
            timeProvider.ResetStartTime();
            timeProvider.AudioTime = 0f;
            timeProvider.playStartTime = 0f;
        }

        startTime = 0f;

        if (Notes != null)
        {
            foreach (Transform child in Notes.transform)
            {
                GameObject.Destroy(child.gameObject);
            }
        }

        inited = false;

        if (objectCounter != null)
        {
            objectCounter.Reset();
        }

        if (menuManager != null)
        {
            menuManager.SetReadyMode();
        }

        if (stopVideo)
        {
            SafeStopVideoPlayer("OnStopButtonClick");
        }
    }

    private void SafeStopVideoPlayer(string reason)
    {
        if (bgManager == null || bgManager.videoPlayer == null)
        {
            return;
        }

        var videoPlayer = bgManager.videoPlayer;

        if (!videoPlayer.isPrepared && !videoPlayer.isPlaying)
        {
            bgManager.SetIdleVideoFrameVisible(false, reason + ":NotPrepared");
            return;
        }

        try
        {
            videoPlayer.Pause();
            videoPlayer.time = 0d;
            bgManager.SetIdleVideoFrameVisible(true, reason + ":ResetToFirstFrame");
        }
        catch (Exception e)
        {
            Debug.LogError("[MJV][Stop] SafeStopVideoPlayer exception: " + e);
        }
    }

    public void WebLoad(string chartpath, string bgpath, string audiopath, string videopath, int level)
    {
        StopAllCoroutines();
        OnStopButtonClick(false);
        timeProvider.AudioTime = 0f;
        timeProvider.playStartTime = 0f;
        menuManager.SetInitMode();
        bgManager.isAnyErr = false;
        bgManager.SetIdleVideoFrameVisible(false, "WebLoadReset");

        var hasVideo = !string.IsNullOrWhiteSpace(videopath);
        if (hasVideo)
        {
            bgManager.videoPlayer.url = videopath;
        }
        else
        {
            bgManager.videoPlayer.url = string.Empty;
            bgManager.UseStaticBackground("NoVideo");
        }

        status = 0;

        void checkReady()
        {
            menuManager.SetLoadingText(status);
            if (status >= 4f)
            {
                menuManager.SetReadyMode();
                string fumens = SimaiProcess.fumens[level];
                if (fumens == null)
                {
                    Debug.Log("Null level!");
                    menuManager.DisablePlay();
                    return;
                }
                if (SimaiProcess.Serialize(fumens) == -1)
                {
                    menuManager.DisablePlay();
                    return;
                }
                if (SimaiProcess.notelist.Count <= 0)
                {
                    Debug.Log("Empty level!");
                    menuManager.DisablePlay();
                    return;
                }
                else
                {
                    menuManager.SetReadyMode();
                }
            }
        }

        Action videoCallback = () =>
        {
            status += 1;
            checkReady();
        };

        if (hasVideo)
        {
            StartCoroutine(WaitVideoPrepare(videoCallback));
        }
        else
        {
            videoCallback.Invoke();
        }

        Action successCallback = () =>
        {
            status += 1;
            checkReady();
        };

        StartCoroutine(simailoader.initFromWeb(chartpath, successCallback));

        Action audioCallback = () =>
        {
            status += 1;
            checkReady();
        };

        Action<float> progressCallback = (float progress) =>
        {
            menuManager.SetLoadingText(status, progress);
        };

        StartCoroutine(SE.LoadWebAudio(audiopath, progressCallback, audioCallback));

        Action bgCallback = () =>
        {
            status += 1;
            checkReady();
        };

        StartCoroutine(WebLoader.LoadBGFromWeb(bgpath, bgCallback));
    }

    IEnumerator WaitVideoPrepare(Action callback)
    {
        bgManager.videoPlayer.Prepare();
        var prepareStartTime = Time.time;
        while (!bgManager.videoPlayer.isPrepared)
        {
            yield return new WaitForEndOfFrame();
            if (Time.time - prepareStartTime > 2f)
            {
                Debug.LogWarning("[MJV][Video] prepare timeout");
                bgManager.UseStaticBackground("PrepareTimeout");
                callback.Invoke();
                StartCoroutine(SeeIfitisDoneLater());
                yield break;
            }
        }

        bgManager.UpdateVideoRatio();
        yield return StartCoroutine(ShowPreparedVideoFirstFrame());
        callback.Invoke();
    }

    IEnumerator ShowPreparedVideoFirstFrame()
    {
        if (bgManager == null || bgManager.videoPlayer == null || !bgManager.videoPlayer.isPrepared)
        {
            yield break;
        }

        try
        {
            bgManager.videoPlayer.time = 0d;
            bgManager.videoPlayer.Play();
        }
        catch (Exception e)
        {
            Debug.LogError("[MJV][FirstFrame] Play exception: " + e);
            yield break;
        }

        yield return null;
        yield return new WaitForEndOfFrame();

        try
        {
            bgManager.videoPlayer.Pause();
            bgManager.videoPlayer.time = 0d;
            bgManager.SetIdleVideoFrameVisible(true, "ShowPreparedVideoFirstFrame");
        }
        catch (Exception e)
        {
            Debug.LogError("[MJV][FirstFrame] Pause/reset exception: " + e);
        }
    }

    IEnumerator SeeIfitisDoneLater()
    {
        while (!bgManager.videoPlayer.isPrepared)
        {
           //Debug.Log("[MJV][FirstFrame] Still waiting for prepare");
            yield return new WaitForEndOfFrame();
        }
        bgManager.UpdateVideoRatio();
        bgManager.isAnyErr = false;
        yield return StartCoroutine(ShowPreparedVideoFirstFrame());
    }

    private void QueueVideoSeek(float timelineSeconds, bool playAfterSeek, string reason)
    {
        if (bgManager == null || bgManager.videoPlayer == null)
        {
            return;
        }

        if (!bgManager.videoPlayer.isPrepared)
        {
            return;
        }

        if (videoSeekCoroutine != null)
        {
            StopCoroutine(videoSeekCoroutine);
        }

        videoSeekGeneration++;
        videoSeekCoroutine = StartCoroutine(SeekVideoToTimelineCoroutine(videoSeekGeneration, timelineSeconds, playAfterSeek, reason));
    }

    private IEnumerator SeekVideoToTimelineCoroutine(int generation, float timelineSeconds, bool playAfterSeek, string reason)
    {
        var player = bgManager.videoPlayer;
        var videoTime = Mathf.Max(0f, timelineSeconds - offset);

        if (player.length > 0d)
        {
            videoTime = Mathf.Min(videoTime, (float)player.length);
        }

        var seekDone = false;

        void OnSeekCompleted(UnityEngine.Video.VideoPlayer source)
        {
            seekDone = true;
        }

        player.seekCompleted += OnSeekCompleted;

        try
        {
            player.Pause();
            bgManager.SetIdleVideoFrameVisible(true, reason + ":SeekStart");
            player.time = videoTime;
        }
        catch (Exception e)
        {
            player.seekCompleted -= OnSeekCompleted;
            Debug.LogError("[MJV][VideoSeek] request exception reason=" + reason + " error=" + e);
            yield break;
        }

        var waitStart = Time.realtimeSinceStartup;
        while (!seekDone && generation == videoSeekGeneration && Time.realtimeSinceStartup - waitStart < 2f)
        {
            yield return null;
        }

        player.seekCompleted -= OnSeekCompleted;

        if (generation != videoSeekGeneration)
        {
            yield break;
        }

        if (playAfterSeek)
        {
            if (player.canSetPlaybackSpeed)
            {
                player.playbackSpeed = audioSpeed;
            }
            player.Play();
            bgManager.SetIdleVideoFrameVisible(false, reason + ":PlayAfterSeek");
        }
        else
        {
            player.Play();
            yield return new WaitForEndOfFrame();

            if (generation != videoSeekGeneration)
            {
                yield break;
            }

            player.Pause();
            bgManager.SetIdleVideoFrameVisible(true, reason + ":PausedFrame");
        }

        videoSeekCoroutine = null;
    }

    public void WebSeek(float seconds)
    {
        var target = Mathf.Max(0f, seconds);
        var wasPlaying = inited && timeProvider != null && timeProvider.isStart;

        startTime = target;

        if (wasPlaying)
        {
            timeProvider.Pause();
        }

        if (inited)
        {
            foreach (Transform child in Notes.transform)
            {
                GameObject.Destroy(child.gameObject);
            }
            simailoader.PlayLevel(target);
        }

        QueueVideoSeek(target, wasPlaying, "WebSeek");

        if (wasPlaying)
        {
            timeProvider.SetStartTime(Mathf.Max(0f, target - offset), audioSpeed);
            menuManager.SetPlayMode();
        }
        else
        {
            timeProvider.AudioTime = target;
            timeProvider.playStartTime = target;
            menuManager.SetPauseMode();
        }
    }

    public void WebSetPlaybackSpeed(string rawSpeed)
    {
        float speed;
        if (!float.TryParse(rawSpeed, out speed))
        {
            return;
        }

        audioSpeed = Mathf.Clamp(speed, 0.25f, 2f);

        var timeline = startTime;
        var wasPlaying = false;

        if (timeProvider != null)
        {
            timeline = Mathf.Max(0f, timeProvider.AudioTime);
            wasPlaying = inited && timeProvider.isStart;

            if (wasPlaying)
            {
                timeProvider.Pause();
                startTime = timeline;
                timeProvider.SetStartTime(Mathf.Max(0f, timeline - offset), audioSpeed);
            }
        }

        if (bgManager != null && bgManager.videoPlayer != null && bgManager.videoPlayer.isPrepared)
        {
            bgManager.SetPlayBackSpeed(audioSpeed);

            if (wasPlaying)
            {
                QueueVideoSeek(timeline, true, "PlaybackSpeed");
            }
        }
    }

    public void WebRefreshTimelineAfterVisualSpeedChange(string reason)
    {
        if (!inited || timeProvider == null)
        {
            return;
        }

        var timeline = Mathf.Max(0f, timeProvider.AudioTime);
        WebSeek(timeline);
    }

    public void OnSpeedDropDownClick(int value)
    {
        audioSpeed = 1f - value * 0.25f;

        if (bgManager != null && bgManager.videoPlayer != null && bgManager.videoPlayer.isPrepared && bgManager.videoPlayer.canSetPlaybackSpeed)
        {
            bgManager.videoPlayer.playbackSpeed = audioSpeed;
        }
    }
}
