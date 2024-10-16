using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MenuManager : MonoBehaviour
{
    public GameObject OverlayWindow;
    public GameObject OverlayMenu;
    public Button PlayPause;
    public Button Stop;
    public TMP_Dropdown speedSelector;
    public TMP_Text loadingText;
    public Sprite ic_home;
    public Sprite ic_settings;
    public Sprite ic_play;
    public Sprite ic_pause;
    public Sprite ic_upload;
    public Sprite ic_reset;

    void Start()
    {
        SetInitMode();
    }

    public void SetInitMode()
    {
        PlayPause.interactable = false;
        Stop.interactable = false;
        speedSelector.interactable = false;
        PlayPause.gameObject.GetComponentsInChildren<Image>()[1].sprite = ic_play;
        loadingText.gameObject.SetActive(false);
    }

    public void SetLoadingText(int step,float progress=0f)
    {
        loadingText.gameObject.SetActive(true);
        if (progress != 0f)
            loadingText.text = $"Loading {progress:P1}";
        else
            loadingText.text = $"Loading ({step}/4)";
    }

    public void SetPlayMode()
    {
        Stop.interactable = true;
        speedSelector.interactable = false;
        PlayPause.gameObject.GetComponentsInChildren<Image>()[1].sprite = ic_pause;
    }

    public void SetPauseMode()
    {
        Stop.interactable = true;
        speedSelector.interactable= false;
        PlayPause.gameObject.GetComponentsInChildren<Image>()[1].sprite = ic_play;
    }

    public void SetReadyMode()
    {
        loadingText.gameObject.SetActive(false);
        PlayPause.interactable = true;
        Stop.interactable = false;
        speedSelector.interactable = true;
        PlayPause.gameObject.GetComponentsInChildren<Image>()[1].sprite = ic_play;
    }

    public void DisablePlay()
    {
        PlayPause.interactable = false;
        Stop.interactable = false;
        speedSelector.interactable = true;
    }


    public void ShowWindow(string fumen)
    {
        OverlayWindow.transform.Find("FumenView").GetComponent<maihighlight>().UpdateHighlight(fumen);
        OverlayWindow.SetActive(true);
    }

    public void OnMenuButtonClick()
    {
        if(OverlayMenu.activeInHierarchy)
            OverlayMenu.SetActive(false);
        else
            OverlayMenu.SetActive(true);
    }
}
