using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class HandleJSMessages : MonoBehaviour
{
    public GameMainManager gameMainManager;

    [DllImport("__Internal")]
    private static extern void UnityLoaded();

    private void Awake()
    {
        Application.targetFrameRate = 5;
        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            WebGLInput.captureAllKeyboardInput = false;
            Debug.Log("HandleJSMessages Activated");//look for this message in the browser to ensure its working, delete before production
            DontDestroyOnLoad(this);
            try
            {
                UnityLoaded();
            }
            catch (Exception e)
            {
                Debug.Log("UnityLoaded() failed: " + e.Message);
            }
        }

    }

    public void Start()
    {
        //StartCoroutine(startAfter());
    }

    IEnumerator startAfter()
    {
        yield return new WaitForSeconds(2);
        Application.targetFrameRate = -1;
        var apiroot = "https://majdata.net/api3/api/maichart/";
        var id = "bb18c48b-2f74-49f3-808c-fef6e0b694c2";
        var level = "lv4";
        Debug.Log("level:" + level);
        gameMainManager = GameObject.Find("GameMain").GetComponent<GameMainManager>();

        gameMainManager.WebLoad(apiroot + id + "/chart" ,
            apiroot + id +  "/image?fullImage=true",
            apiroot + id + "/Track",
            apiroot + id + "/Video",
            int.Parse(level[2].ToString()));
    }

    /// <summary>
    /// Receive message from the nextjs app that has webgl-nextjs package
    /// </summary>
    /// <param name="message"></param>
    public void ReceiveMessage(string message)
    {
        Application.targetFrameRate = -1;
        var parts = message.Split('\n');//type\ncontent
        var maidata = parts[0];
        var track = parts[1];
        var bg = parts[2];
        var mv = parts[3];
        var level = parts[4];
        Debug.Log("level:"+level);
        gameMainManager = GameObject.Find("GameMain").GetComponent<GameMainManager>();

        gameMainManager.WebLoad(maidata,
            bg,
            track,
            mv,
            int.Parse(level[2].ToString()));
        
    }
}