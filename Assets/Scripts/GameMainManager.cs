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
    public SimaiDataLoader loader;
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

    string id;
    string chartpath;
    string bgpath;
    string audiopath;


    void Start()
    {
        id = WebLoader.getChartID();
        chartpath = ApiAccess.ROOT + "Maidata/" + id;
        audiopath = ApiAccess.ROOT + "Track/" + id;
        bgpath = ApiAccess.ROOT + "Image/" + id;
        WebLoad();
    }

    public void OnBackButtonClick()
    {
        SceneManager.LoadScene("SongLstMenu");
    }

    // init loading & start playing method
    public void Play()
    {
        startAt = System.DateTime.Now.Ticks;
        loader.noteSpeed = settings.noteSpeed;
        loader.touchSpeed = settings.touchSpeed;
        SimaiProcess.Serialize(SimaiProcess.fumens[menuManager.level]);
        timeProvider.SetStartTime(startTime - offset, audioSpeed);
        objectCounter.ComboSetActive(settings.combo);
        loader.PlayLevel(startTime);
        multTouchHandler.clearSlots();
        Notes.GetComponent<PlayAllPerfect>().enabled = false;
        inited = true;
        // set btn states
        menuManager.SetPlayMode();
    }

    // callback of play/pause button
    public void OnPlayPauseButtonClick()
    {
        if (!inited) {Play();return;}
        if (timeProvider.isStart) {
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
        startTime = 0f;
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

    public void WebLoad()
    {
        //载入各种资源，完成后准备菜单
        void checkReady()
        {
            menuManager.SetLoadingText(status);
            if(status ==3 ) {
                menuManager.SetReadyMode();
                OnDropDownClick();
            }
        }
        // open maidata.txt
        Action successCallback = () =>
        {
            status += 1;
            checkReady();
        };

        StartCoroutine(loader.initFromWeb(chartpath, successCallback));

        Action audioCallback = () =>
        {
            status += 1;
            checkReady();
        };

        StartCoroutine(SE.LoadWebAudio(audiopath, audioCallback));

        Action bgCallback = () =>
        {
            status += 1;
            checkReady();
        };

        StartCoroutine(bgManager.LoadBGFromWeb(bgpath, bgCallback));
    }

    // method that checks if level is empty
    public void OnDropDownClick()
    {
        int levelIndex = menuManager.level;
        Debug.Log("finding level: " + levelIndex);
        string fumens = SimaiProcess.fumens[levelIndex];
        if (fumens == null)
        {
            Debug.Log("Null level!");
            menuManager.DisablePlay();
        }
        else
        {
            if (SimaiProcess.Serialize(fumens) == -1) 
            {
            menuManager.DisablePlay();
                return;
            }
            Debug.Log("Total notes: "+SimaiProcess.notelist.Count);
            if (SimaiProcess.notelist.Count <= 0)
            {
                Debug.Log("Empty level!");
                menuManager.DisablePlay();
            }
            else {menuManager.SetReadyMode();}
        }
    }
}
