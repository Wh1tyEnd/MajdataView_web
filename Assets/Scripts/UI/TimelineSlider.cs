using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimelineSlider : MonoBehaviour
{
    AudioTimeProvider timeProvider;
    ExtendedSlider slider;
    GameMainManager gameMainManager;
    bool isDragging = false;
    // Start is called before the first frame update
    void Start()
    {
        timeProvider = GameObject.Find("AudioTimeProvider").GetComponent<AudioTimeProvider>();
        gameMainManager = GameObject.Find("GameMain").GetComponent<GameMainManager>();
        slider = GetComponent<ExtendedSlider>();
        slider.DragStart.AddListener(OnDragStart);
        slider.DragStop.AddListener(OnDragStop);
        slider.onValueChanged.AddListener(OnValueChanged);
    }

    // Update is called once per frame
    void Update()
    {
        if(timeProvider.bgm.clip)
            slider.maxValue = timeProvider.bgm.clip.length;
        if (!isDragging)
        {
            slider.value = timeProvider.AudioTime;
        }
    }
    void OnDragStart(float value)
    {
        isDragging = true;
        if(timeProvider.isStart)
            gameMainManager.OnStopButtonClick();
        gameMainManager.startTime = value;
        timeProvider.AudioTime = value;
    }
    void OnValueChanged(float value)
    {
        if (!isDragging) return;
        gameMainManager.startTime = value;
        timeProvider.AudioTime = value;
    }
    void OnDragStop(float value)
    {
        isDragging = false;
        gameMainManager.startTime = value;
        timeProvider.AudioTime = value;
    }
}
