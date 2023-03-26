﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

public class ToggleFullScreen : MonoBehaviour
{
    public void ToggleFullscreen()
    {
        Debug.Log("ToggleFullScreen");
        var resolutions = Screen.resolutions;
        if (Screen.fullScreen)
        {
            var width = 300;
            var height = 300;
            Screen.SetResolution(width, height, false);
        }
        else
        {
            Screen.SetResolution(resolutions[resolutions.Length - 1].width, resolutions[resolutions.Length - 1].height, true);
        }
        Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
    }
    Dropdown dd;
    public void DisplayDropdown()
    {
        dd.value = 999;
        dd.gameObject.SetActive(true);
        
    }
    public void SetResolution()
    {
        var i = dd.value;
        Debug.Log(i);
        switch (i)
        {
            case 0:
                Screen.SetResolution(512, 512, false);
                break;
            case 1:
                Screen.SetResolution(1080, 1080,false);
                break;
            case 2:
                Screen.SetResolution(1280, 720, false);
                break;
            case 3:
                Screen.SetResolution(2560, 1440, false);
                break;
            case 4:
                Screen.SetResolution(3840, 2160, false);
                break;
        }
        dd.gameObject.SetActive(false);
    }
    public void Start()
    {
        // dd = GameObject.Find("ResoDropdown").GetComponent<Dropdown>();
        // dd.gameObject.SetActive(false);
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleFullscreen();
        }
    }
}
