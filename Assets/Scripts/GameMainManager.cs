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
    public GameObject SongDetail;
    public SoundEffect SE;
    public MenuManager menuManager;
    public SettingsManager settings;
    
    [Space(10)]
    [Header("AudioRef")]
    public AudioSource bgm;

    [Space(10)]
    [Header("Settings")]
    public int difficulty = 0;
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
        if (WebLoader.songlist.Count <= 0)
        {
            menuManager.Upload.interactable = true;
            menuManager.Upload.gameObject.GetComponentInChildren<TMP_Text>().text = "Empty! Pleace go Back";
            menuManager.Upload.gameObject.GetComponentInChildren<TMP_Text>().color = Color.red;
        }
        else
        {
            id = WebLoader.getChartID();
        
        chartpath = ApiAccess.ROOT+"Maidata/" + id;
        audiopath = ApiAccess.ROOT + "Track/" + id;
        bgpath = ApiAccess.ROOT + "Image/" + id;
        WebUpload();
        }
            

    }
    // init loading & start playing method
    public void ChickStart()
    {
        startAt = System.DateTime.Now.Ticks;
        loader.noteSpeed = (float)(107.25 / (71.4184491 * Mathf.Pow(settings.noteSpeed + 0.9975f, -0.985558604f)));
        loader.touchSpeed = settings.touchSpeed;
        timeProvider.audioOffset = settings.offset;
        timeProvider.SetStartTime(startAt, startTime - offset, audioSpeed, menuManager.isRecord);
        objectCounter.ComboSetActive(settings.combo);
        loader.PlayLevel(menuManager.level, startTime);
        multTouchHandler.clearSlots();
        bgCover.color = new Color(0f, 0f, 0f, settings.bgCover);

        if (menuManager.isRecord)
        {
            Notes.GetComponent<PlayAllPerfect>().enabled = true;
            // disable playpause btn to prevent errors
            bgManager.PlaySongDetail();
        }
        else
        {
            Notes.GetComponent<PlayAllPerfect>().enabled = false;
        }
        inited = true;
        // set btn states
        menuManager.SetPlayMode();
        menuManager.HideMenu();
    }

    // callback of play/pause button
    public void TogglePlay()
    {
        if (!inited) {ChickStart();return;}
        if (timeProvider.isStart) {
            timeProvider.Pause();
            bgManager.PauseVideo();
            menuManager.SetPauseMode();
        } else {
            timeProvider.Resume();
            bgManager.ContinueVideo(audioSpeed);
            menuManager.SetPlayMode();
        }
    }

    // callback of stop button
    public void ChickStop()
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
        // disable songdetail
        SongDetail.SetActive(false);
        // re-init on next start
        inited = false;
        // reset counter
        objectCounter.Reset();
        // set btn states
        menuManager.SetReadyMode();
    }

    // callback of upload button
    public void ChickLoad()
    {
        /* #if UNITY_EDITOR
             EditorLoad();
         #elif UNITY_WEBGL
             WebUpload();
         #endif*/

        SceneManager.LoadScene("SongLstMenu");


    }

    void EditorLoad()
    {
        if (status == 0)
        {
            loader.initFromFile(editorInitPath);
            Action audioCallback = () =>
            {
                menuManager.SetReadyMode();
                UpdateLevel();
                status = 3;
            };
            StartCoroutine(SE.LoadWebAudio(editorInitPath+"\\track.mp3", audioCallback));
        } else if (status == 3)
        // reset
        {SceneManager.LoadScene(0, LoadSceneMode.Single);}
    }

    public void WebUpload()
    {
        if (status == 0)
        {
            // open maidata.txt
            Action successCallback = () =>
            {
                status = 1;
                WebUpload();

            };

            StartCoroutine(loader.initFromWeb(chartpath, successCallback));
        }
        else if(status == 1)
        {
            Action audioCallback = () =>
            {
                status = 2;
                WebUpload();
            };
   
            StartCoroutine(SE.LoadWebAudio(audiopath, audioCallback));
        }
        else if (status == 2)
        {
            Action bgCallback = () =>
            {
                menuManager.SetReadyMode();
                UpdateLevel();
                status = 3;

            };

            StartCoroutine(bgManager.LoadBGFromWeb(bgpath, bgCallback));
        }
        
            
            
            //WebFileUploaderHelper.RequestFile(uploadedCallback, ".txt");
        
        
        else if (status == 3)
        // reset

        {  Debug.Log(status); SceneManager.LoadScene("SongLstMenu");}

    }
    [Serializable]
    public class SongList
    {
        public List<SongInfo> info;
    }
    [Serializable]
    public class SongInfo
    {
        public string Title;
        public string sequenceID;
    }

    public IEnumerator getMMFCList(string path)
    {
        if (path == string.Empty) { Debug.LogError("Empty path!"); yield break; }
        Debug.Log("Downloading song list from " + path);
        UnityWebRequest www = UnityWebRequest.Get(path);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error downloading data: " + www.error);
        }
        else
        {
            string songs = "{\"info\":" + www.downloadHandler.text + "}";
            Debug.Log(songs);
            SongList songlist = JsonUtility.FromJson<SongList>(songs);
            Debug.Log(songlist.info[0].Title);
        }
    }

    // method that checks if level is empty
    public void UpdateLevel()
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
