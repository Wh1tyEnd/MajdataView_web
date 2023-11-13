using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class HandleJSMessages : MonoBehaviour
{
    public GameMainManager gameMainManager;
    private void Awake()
    {
        Application.targetFrameRate = 5;
        WebGLInput.captureAllKeyboardInput = false;
        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            Debug.Log("HandleJSMessages Activated");//look for this message in the browser to ensure its working, delete before production
            DontDestroyOnLoad(this);
        }
    }

    public void Start()
    {
        //StartCoroutine(startAfter());
    }

    IEnumerator startAfter()
    {
        yield return new WaitForSeconds(2);
        var apiroot = "http://101.132.193.53:5002/api";
        var id = "148";
        var level = "lv0";
        Debug.Log("level:" + level);
        gameMainManager = GameObject.Find("GameMain").GetComponent<GameMainManager>();

        gameMainManager.WebLoad(apiroot + "/Maidata/" + id,
            apiroot + "/ImageFull/" + id,
            apiroot + "/Track/" + id);
    }

    /// <summary>
    /// Receive message from the nextjs app that has webgl-nextjs package
    /// </summary>
    /// <param name="message"></param>
    public void ReceiveMessage(string message)
    {
        Application.targetFrameRate = -1;
        var parts = message.Split('\n');//type\ncontent
        var type = parts[0];
        var apiroot = parts[1];
        var id = parts[2];
        var level = parts[3].Substring(1);
        Debug.Log("level:"+level);
        gameMainManager = GameObject.Find("GameMain").GetComponent<GameMainManager>();

        gameMainManager.WebLoad(apiroot + "/Maidata/" + id,
            apiroot + "/ImageFull/" + id,
            apiroot + "/Track/" + id);
        
    }
}