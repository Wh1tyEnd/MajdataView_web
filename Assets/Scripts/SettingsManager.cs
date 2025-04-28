using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using TMPro;
using System.Runtime.InteropServices;
using Newtonsoft.Json.Linq;
using System.Xml.Linq;
using System;

public static class ExtensionFindMethod
{
    public static Transform FindObject(this Transform parent, string name)
    {
        Transform[] trs= parent.GetComponentsInChildren<Transform>(true);
        foreach(Transform t in trs){
            if(t.name == name){
                return t;
            }
        }
        return null;
    }
}

public class SettingsManager : MonoBehaviour
{
    List<string> sliders = new List<string>{
        //"TargetFPS",
        "Offset",
        "NoteSpeed",
        "TouchSpeed",
        "BGCover"
    };
    List<string> channels = new List<string>{
        "BGM",
        "Answer",
        "Judge",
        "Slide",
        "Break",
        "EX",
        "Touch",
        "Hanabi",
        "Others"
    };
    //public FPSMonitor fpsMonitor;
    public GameObject volumePrefab;
    public AudioMixer masterMixer;
    
    public Slider GetSlider(string sliderName)
    {
        Transform sliderParent = transform.FindObject(sliderName);
        Slider slider = sliderParent.FindObject(sliderName + "Slider").GetComponent<Slider>();
        return slider;
    }

    public TMP_Text GetSliderDisplay(string sliderName)
    {
        Transform sliderParent = transform.FindObject(sliderName);
        TMP_Text sliderDisplay = sliderParent.FindObject(sliderName + "Display").GetComponent<TMP_Text>();
        return sliderDisplay;
    }

    public void UpdateSliders()
    {
        //int fps = (int)Mathf.Pow(2, GetSlider("TargetFPS").value) * 30;
        //GetSliderDisplay("TargetFPS").text = fps.ToString();
        //fpsMonitor.setTargetFPS(fps);
        GetSliderDisplay("Offset").text = GetSlider("Offset").value.ToString() + "MS";
        GetSliderDisplay("NoteSpeed").text = (GetSlider("NoteSpeed").value / 10).ToString();
        GetSliderDisplay("TouchSpeed").text = (GetSlider("TouchSpeed").value / 10).ToString();
        GetSliderDisplay("BGCover").text = (GetSlider("BGCover").value / 10).ToString();
    }

    public float offset {get {
        return GetSlider("Offset").value / 1000;
    }}

    public float noteSpeed {get {
        return (float)(107.25 / (71.4184491 * Mathf.Pow(GetSlider("NoteSpeed").value / 10 + 0.9975f, -0.985558604f)));
    }}

    public float touchSpeed {get {
        return GetSlider("TouchSpeed").value / 10;
    }}

    public float bgCover {get {
        return GetSlider("BGCover").value / 10;
    }}
    
    public bool combo
    {
        get
        {
            return transform.FindObject("Combo").GetComponent<Toggle>().isOn;
        }
        set 
        { 
            transform.FindObject("Combo").GetComponent<Toggle>().isOn = value; 
        }
    }
    
    public void UpdateVolume()
    {
        foreach(string channel in channels)
        {
            float volume = GetSlider(channel).value;
            GetSliderDisplay(channel).text = volume.ToString() + "dB";
            masterMixer.SetFloat(channel, volume);
        }
    }

    void Awake()
    {
        Transform audioRoot = transform.FindObject("Content");
        UIGroup VolumeGroup = transform.FindObject("Volume").GetComponent<UIGroup>();
        foreach (string channel in channels)
        {
            GameObject volumeItem = Instantiate(volumePrefab);
            volumeItem.transform.SetParent(audioRoot);
            volumeItem.transform.localScale = new Vector3(1, 1, 1);
            volumeItem.transform.FindObject("Text").GetComponent<TMP_Text>().text = channel;
            volumeItem.name = channel;
            GameObject volumeSlider = volumeItem.transform.FindObject("VolumeSlider").gameObject;
            volumeSlider.name = channel + "Slider";
            GameObject volumeDisplay = volumeItem.transform.FindObject("VolumeDisplay").gameObject;
            volumeDisplay.name = channel + "Display";
            float vol;
            bool result = masterMixer.GetFloat(channel, out vol);
            volumeSlider.GetComponent<Slider>().value = vol;
            volumeDisplay.GetComponent<TMP_Text>().text = vol.ToString() + "dB";
            VolumeGroup.childComponents.Add(volumeItem);
        }
        VolumeGroup.Apply();

        try
        {
            string values = GetLocalStorge("MajdataSettings");
            if (values != null)
            {
                SetSliderValues(values);
                UpdateSliders();
                UpdateVolume();
            }
        }
        catch (Exception e)
        {
            Debug.Log("GetLocalStorge() failed: " + e.Message);
        }

        foreach (string slider in sliders)
        {
            GetSlider(slider).onValueChanged.AddListener(delegate { UpdateSliders(); });
            GetSlider(slider).onValueChanged.AddListener(delegate { SaveSliderSettings(); });
        }
        foreach (string channel in channels)
        {
            GetSlider(channel).onValueChanged.AddListener(delegate { UpdateVolume(); });
            GetSlider(channel).onValueChanged.AddListener(delegate { SaveSliderSettings(); });
        }
        transform.FindObject("Combo").GetComponent<Toggle>().onValueChanged.AddListener(delegate { SaveSliderSettings(); });
    }
    [DllImport("__Internal")]
    private static extern void SetLocalStorge(string name, string value);
    [DllImport("__Internal")]
    private static extern string GetLocalStorge(string name);

    void SaveSliderSettings()
    {
        string values = GetSliderValues();
        try
        {
            SetLocalStorge("MajdataSettings", values);
        }
        catch (Exception e)
        {
            Debug.Log("SetLocalStorge() failed: " + e.Message);
        }
    }

    string GetSliderValues()
    {
        string values = "";
        foreach (string slider in sliders)
        {
            values += GetSlider(slider).value.ToString() + ",";
        }
        foreach (string channel in channels)
        {
            values += GetSlider(channel).value.ToString() + ",";
        }
        values += combo ? "1" : "0";
        return values;
    }

    void SetSliderValues(string values)
    {
        string[] valueArray = values.Split(',');
        for (int i = 0; i < sliders.Count; i++)
        {
            GetSlider(sliders[i]).value = float.Parse(valueArray[i]);
        }
        for (int i = 0; i < channels.Count; i++)
        {
            GetSlider(channels[i]).value = float.Parse(valueArray[i + sliders.Count]);
        }
        combo = valueArray[sliders.Count + channels.Count] == "1" ? true : false;
    }   
}
