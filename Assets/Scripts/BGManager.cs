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
    public GameObject videoTarget;
    public bool isAnyErr = false;

    private bool showIdleVideoFrame = false;
    private bool lastHideStaticBackground = false;

    void Start()
    {
        spriteRender = GetComponent<SpriteRenderer>();
        BackgroundCover = GameObject.Find("BackgroundCover").GetComponent<SpriteRenderer>();
        videoPlayer.errorReceived += VideoPlayer_errorReceived;
        SetNewSpriteForVideo();
    }

    private void VideoPlayer_errorReceived(VideoPlayer source, string message)
    {
        Debug.Log("LoadVideoFailed");
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
    }

    public void SetNewSpriteForVideo()
    {
        videoTarget.GetComponent<SpriteRenderer>().sprite =
                Sprite.Create(new Texture2D(480, 480), new Rect(0, 0, 480, 480), new Vector2(0.5f, 0.5f));
    }

    public void SetIdleVideoFrameVisible(bool visible, string reason)
    {
        showIdleVideoFrame = visible && !isAnyErr;
    }

    public void UpdateVideoRatio()
    {
        if (videoPlayer == null ||
            videoTarget == null ||
            videoPlayer.width <= 0 ||
            videoPlayer.height <= 0)
        {
            return;
        }

        var scale = videoPlayer.height / (float)videoPlayer.width;
        videoTarget.transform.localScale = new Vector3(2.25f, 2.25f * scale);
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
        }

        if (BackgroundCover != null && settings != null)
        {
            BackgroundCover.color = new UnityEngine.Color(0f, 0f, 0f, settings.bgCover);
        }
    }
}
