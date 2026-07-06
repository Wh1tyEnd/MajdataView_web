using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Video;
using System.IO;
using System.Drawing;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class BGManager : MonoBehaviour
{
    public static SpriteRenderer spriteRender;
    SpriteRenderer BackgroundCover;
    public SettingsManager settings;
    public VideoPlayer videoPlayer;
    public AudioTimeProvider audioTimeProvider;
    public GameObject videoTarget;
    public bool isAnyErr = false;
    float desireSpeed = 1f;

    private bool showIdleVideoFrame = false;
    private bool lastHideStaticBackground = false;

    void Start()
    {
        spriteRender = GetComponent<SpriteRenderer>();
        BackgroundCover = GameObject.Find("BackgroundCover").GetComponent<SpriteRenderer>();
        audioTimeProvider = GameObject.Find("AudioTimeProvider").GetComponent<AudioTimeProvider>();
        videoPlayer.errorReceived += VideoPlayer_errorReceived;
        SetNewSpriteForVideo();
    }

    private void VideoPlayer_errorReceived(VideoPlayer source, string message)
    {
        Debug.LogWarning("[MJV][BGManager] LoadVideoFailed: " + message);
        UseStaticBackground("VideoPlayer.errorReceived");
    }

    public void UseStaticBackground(string reason)
    {
        isAnyErr = true;
        showIdleVideoFrame = false;

        if (spriteRender != null)
        {
            spriteRender.forceRenderingOff = false;
        }

        Debug.LogWarning("[MJV][BGManager] use static background reason=" + reason);
    }

    public void SetNewSpriteForVideo()
    {
        videoTarget.GetComponent<SpriteRenderer>().sprite =
                Sprite.Create(new Texture2D(480, 480), new Rect(0, 0, 480, 480), new Vector2(0.5f, 0.5f));
    }

    public void SetIdleVideoFrameVisible(bool visible, string reason)
    {
        showIdleVideoFrame = visible && !isAnyErr;

        Debug.Log(
            "[MJV][BGManager] SetIdleVideoFrameVisible visible=" +
            showIdleVideoFrame +
            " requested=" +
            visible +
            " reason=" +
            reason +
            " isAnyErr=" +
            isAnyErr
        );
    }

    public void UpdateVideoRatio()
    {
        if (videoPlayer == null ||
            videoTarget == null ||
            videoPlayer.width <= 0 ||
            videoPlayer.height <= 0)
        {
            Debug.LogWarning("[MJV][BGManager] UpdateVideoRatio skipped: invalid video size");
            return;
        }
        Debug.LogWarning("[MJV][BGManager] UpdateVideoRatio");
        var scale = videoPlayer.height / (float)videoPlayer.width;
        videoTarget.transform.localScale = new Vector3(2.25f, 2.25f * scale);
    }

    public void SetPlayBackSpeed(float speed)
    {
        desireSpeed = speed;
        if (videoPlayer.canSetPlaybackSpeed)
        {
            videoPlayer.playbackSpeed = desireSpeed;
        }
        else
        {
            Debug.Log("[MJV][BGManager] videoPlayer.canSetPlaybackSpeed is false");
        }
    }

    public void Update()
    {
        var hideStaticBackground = false;

        if (!isAnyErr && videoPlayer != null)
        {
            hideStaticBackground = videoPlayer.isPlaying || showIdleVideoFrame;
        }

        if (spriteRender != null)
        {
            spriteRender.forceRenderingOff = hideStaticBackground;
        }

        if (hideStaticBackground != lastHideStaticBackground)
        {
            lastHideStaticBackground = hideStaticBackground;

            Debug.Log(
                "[MJV][BGManager] staticBackgroundHidden=" +
                hideStaticBackground +
                " videoPlaying=" +
                (videoPlayer != null && videoPlayer.isPlaying) +
                " videoPrepared=" +
                (videoPlayer != null && videoPlayer.isPrepared) +
                " idleFrame=" +
                showIdleVideoFrame
            );
        }

        if (BackgroundCover != null && settings != null)
        {
            BackgroundCover.color = new UnityEngine.Color(0f, 0f, 0f, settings.bgCover);
        }

        if (videoPlayer.isPrepared && videoPlayer.isPlaying)
        {
            var delta = videoPlayer.time - audioTimeProvider.AudioTime;
            //Debug.Log("[MJV][BGManager] video time delta:" + delta);
            if (Math.Abs(delta) > 0.1f)
            {
                Debug.Log("[MJV][BGManager] video time is behind audio time, try speed up video");
                if (videoPlayer.canSetPlaybackSpeed)
                {
                    float speedchange = 1f + ((float)-delta * 0.8f);
                    videoPlayer.playbackSpeed = desireSpeed * speedchange;
                }
                else
                {
                    Debug.Log("[MJV][BGManager] videoPlayer.canSetPlaybackSpeed is false");
                }
            }
            else
            {
                if (videoPlayer.canSetPlaybackSpeed)
                {
                    videoPlayer.playbackSpeed = desireSpeed;
                }
            }
        }
    }
}
