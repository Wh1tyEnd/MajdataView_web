using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Video;
using System.IO;
using System.Drawing;
using UnityEngine.UI;

public class BGManager : MonoBehaviour
{
    public static SpriteRenderer spriteRender;
    SpriteRenderer BackgroundCover;
    public SettingsManager settings;
    public VideoPlayer videoPlayer;

    void Start()
    {
        spriteRender = GetComponent<SpriteRenderer>();
        BackgroundCover = GameObject.Find("BackgroundCover").GetComponent<SpriteRenderer>();
    }
    

    public void Update()
    {
        if(videoPlayer.isActiveAndEnabled) {
            spriteRender.forceRenderingOff = videoPlayer.isPlaying;
        }
        else
        {
            spriteRender.forceRenderingOff = false;
        }
        
        BackgroundCover.color = new UnityEngine.Color(0f, 0f, 0f, settings.bgCover);
    }
}
