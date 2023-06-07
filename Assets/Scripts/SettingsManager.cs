using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using TMPro;

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
        "TargetFPS",
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
    public FPSMonitor fpsMonitor;
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
        int fps = (int)Mathf.Pow(2, GetSlider("TargetFPS").value) * 30;
        GetSliderDisplay("TargetFPS").text = fps.ToString();
        fpsMonitor.setTargetFPS(fps);
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
    
    public bool combo {get {
        return transform.FindObject("Combo").GetComponent<Toggle>().isOn;
    }}
    
    public void UpdateVolume()
    {
        foreach(string channel in channels)
        {
            float volume = GetSlider(channel).value;
            GetSliderDisplay(channel).text = volume.ToString() + "dB";
            masterMixer.SetFloat(channel, volume);
        }
    }

    void Start()
    {
        foreach(string slider in sliders)
        {
            GetSlider(slider).onValueChanged.AddListener (delegate {UpdateSliders();});
        }
        Transform audioRoot = transform.FindObject("Content");
        UIGroup VolumeGroup = transform.FindObject("Volume").GetComponent<UIGroup>();
        foreach(string channel in channels)
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
            bool result =  masterMixer.GetFloat(channel, out vol);
            volumeSlider.GetComponent<Slider>().value = vol;
            volumeSlider.GetComponent<Slider>().onValueChanged.AddListener (delegate {UpdateVolume();});
            volumeDisplay.GetComponent<TMP_Text>().text = vol.ToString() + "dB";
            VolumeGroup.childComponents.Add(volumeItem);
        }
        VolumeGroup.Apply();
    }
}
