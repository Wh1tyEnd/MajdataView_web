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
    public TMP_Dropdown levelSelector;
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
        levelSelector.interactable = false;
        PlayPause.gameObject.GetComponentsInChildren<Image>()[1].sprite = ic_play;
    }

    public void SetLoadingText(int step)
    {
        loadingText.text = $"Loading ({step}/3)";
    }

    public void SetPlayMode()
    {
        Stop.interactable = true;
        levelSelector.interactable = false;
        PlayPause.gameObject.GetComponentsInChildren<Image>()[1].sprite = ic_pause;
    }

    public void SetPauseMode()
    {
        Stop.interactable = true;
        levelSelector.interactable = false;
        PlayPause.gameObject.GetComponentsInChildren<Image>()[1].sprite = ic_play;
    }

    public void SetReadyMode()
    {
        if(loadingText)
            Destroy(loadingText.gameObject);
        PlayPause.interactable = true;
        Stop.interactable = false;
        levelSelector.interactable = true;
        PlayPause.gameObject.GetComponentsInChildren<Image>()[1].sprite = ic_play;
    }

    public void DisablePlay()
    {
        PlayPause.interactable = false;
        Stop.interactable = false;
        levelSelector.interactable = true;
    }


    public int level {get { return levelSelector.value; } }

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
