using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MenuManager : MonoBehaviour
{
    public int page = 0;
    public GameObject p1;
    public GameObject p2;
    public GameObject OverlayWindow;
    public Button pageToggle;
    public Button Upload;
    public Button PlayPause;
    public Button Stop;
    public TMP_Dropdown levelSelector;
    public Toggle recordMode;
    public Sprite ic_home;
    public Sprite ic_settings;
    public Sprite ic_play;
    public Sprite ic_pause;
    public Sprite ic_upload;
    public Sprite ic_reset;

    void Start()
    {
        pageToggle.GetComponent<Image>().sprite = ic_settings;
        SetInitMode();
        p1.SetActive(true);
        p2.SetActive(false);
    }

    public void TogglePage()
    {
        if(page == 0)
        {
            pageToggle.GetComponent<Image>().sprite = ic_home;
            page = 1;
            p1.SetActive(false);
            p2.SetActive(true);
        }
        else if (page == 1)
        {
            pageToggle.GetComponent<Image>().sprite = ic_settings;
            page = 0;
            p1.SetActive(true);
            p2.SetActive(false);
        }
    }

    public void SetInitMode()
    {
        Upload.interactable = false;
        PlayPause.interactable = false;
        Stop.interactable = false;
        levelSelector.interactable = false;
        recordMode.interactable = false;
        Upload.gameObject.GetComponentsInChildren<Image>()[1].sprite = ic_reset;
        PlayPause.gameObject.GetComponentInChildren<TMP_Text>().text = "Play";
        PlayPause.gameObject.GetComponentsInChildren<Image>()[1].sprite = ic_play;
        pageToggle.interactable = true;
    }

    public void SetPlayMode()
    {
        Upload.interactable = true;
        PlayPause.interactable = !recordMode.isOn;
        Stop.interactable = true;
        levelSelector.interactable = false;
        recordMode.interactable = false;
        PlayPause.gameObject.GetComponentInChildren<TMP_Text>().text = "Pause";
        PlayPause.gameObject.GetComponentsInChildren<Image>()[1].sprite = ic_pause;
        pageToggle.interactable = false;
    }

    public void SetPauseMode()
    {
        Upload.interactable = true;
        Stop.interactable = true;
        levelSelector.interactable = false;
        recordMode.interactable = false;
        PlayPause.gameObject.GetComponentInChildren<TMP_Text>().text = "Play";
        PlayPause.gameObject.GetComponentsInChildren<Image>()[1].sprite = ic_play;
        pageToggle.interactable = false;
    }

    public void SetReadyMode()
    {
        Upload.interactable = true;
        PlayPause.interactable = true;
        Stop.interactable = false;
        levelSelector.interactable = true;
        recordMode.interactable = true;
        PlayPause.gameObject.GetComponentInChildren<TMP_Text>().text = "Play";
        PlayPause.gameObject.GetComponentsInChildren<Image>()[1].sprite = ic_play;
        Upload.gameObject.GetComponentInChildren<TMP_Text>().text = "Back";
        Upload.gameObject.GetComponentsInChildren<Image>()[1].sprite = ic_upload;
        pageToggle.interactable = true;
    }

    public void DisablePlay()
    {
        Upload.interactable = true;
        PlayPause.interactable = false;
        Stop.interactable = false;
        levelSelector.interactable = true;
        recordMode.interactable = true;
    }

    public void SetUploadText(string text)
    {
        Upload.gameObject.GetComponentInChildren<TMP_Text>().text = text;
    }

    public int level {get { return levelSelector.value; } }
    public bool isRecord {get { return recordMode.isOn; } }
    public string uploadText {
        get { return Upload.gameObject.GetComponentInChildren<TMP_Text>().text; }
        set { Upload.gameObject.GetComponentInChildren<TMP_Text>().text = value; } 
    }

    public void ShowWindow(string fumen)
    {
        OverlayWindow.transform.Find("FumenView").GetComponent<maihighlight>().UpdateHighlight(fumen);
        OverlayWindow.SetActive(true);
    }

    public void ShowMenu() {gameObject.SetActive(true);}
    public void HideMenu() {gameObject.SetActive(false);}
    public void ShowWindow() {OverlayWindow.SetActive(true);}
    public void HideWindow() {OverlayWindow.SetActive(false);}
}
