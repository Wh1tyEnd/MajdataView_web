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
    public long startAt;
    public float audioSpeed = 1f;
    public float offset;
    
    [Space(10)]
    [Header("Debug")]
    public string editorInitPath;

    private bool inited = false;
    private int status = 0;




    void Start()
    {
        /*id = SongInformation.getChartID();
        chartpath = ApiAccess.ROOT + "Maidata/" + id;
        audiopath = ApiAccess.ROOT + "Track/" + id;
        bgpath = ApiAccess.ROOT + "ImageFull/" + id;
        WebLoad();*/
    }

    // init loading & start playing method
    public void Play()
    {
        startAt = System.DateTime.Now.Ticks;
        simailoader.noteSpeed = settings.noteSpeed;
        simailoader.touchSpeed = settings.touchSpeed;
        //SimaiProcess.Serialize(SimaiProcess.fumens[menuManager.level]);
        simailoader.PlayLevel(startTime);
        timeProvider.SetStartTime(startTime - offset, audioSpeed);
        objectCounter.ComboSetActive(settings.combo);
        multTouchHandler.clearSlots();
        Notes.GetComponent<PlayAllPerfect>().enabled = false;
        inited = true;
        // set btn states
        menuManager.SetPlayMode();
    }

    // callback of play/pause button
    public void OnPlayPauseButtonClick()
    {
        if (!inited) {
            //startTime = timeProvider.AudioTime;
            Play();
            return;
        }
        if (timeProvider.isStart) {
            startTime = timeProvider.AudioTime;
            timeProvider.playStartTime = startTime;
            timeProvider.Pause();
            menuManager.SetPauseMode();
        } else {
            timeProvider.Resume();
            menuManager.SetPlayMode();
        }
    }

    // callback of stop button
    public void OnStopButtonClick()
    {
        // hide bgcover
        bgCover.color = new Color(0f, 0f, 0f, 0f);
        // reset audiotime
        timeProvider.ResetStartTime();
        // destroy all notes
        foreach (Transform child in Notes.transform) {
            GameObject.Destroy(child.gameObject);
        }
        // re-init on next start
        inited = false;
        // reset counter
        objectCounter.Reset();
        // set btn states
        menuManager.SetReadyMode();
    }

    public void WebLoad(string chartpath, string bgpath, string audiopath, int level)
    {
        OnStopButtonClick();
        menuManager.SetInitMode();
        status = 0;
        //载入各种资源，完成后准备菜单
        void checkReady()
        {
            menuManager.SetLoadingText(status);
            if(status >= 3f ) {
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
                Debug.Log("Total notes: " + SimaiProcess.notelist.Count);
                if (SimaiProcess.notelist.Count <= 0)
                {
                    Debug.Log("Empty level!");
                    menuManager.DisablePlay();
                    return;
                }
                else { menuManager.SetReadyMode(); }
            }
        }
        // open maidata.txt
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
            menuManager.SetLoadingText(status,progress);
        };

        StartCoroutine(SE.LoadWebAudio(audiopath, progressCallback, audioCallback));

        Action bgCallback = () =>
        {
            status += 1;
            checkReady();
        };

        StartCoroutine(WebLoader.LoadBGFromWeb(bgpath, bgCallback));
    }

    public void OnSpeedDropDownClick(int value)
    {
        audioSpeed = 1f-value*0.25f;
    }
}
