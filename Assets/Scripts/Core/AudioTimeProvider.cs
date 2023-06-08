using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AudioTimeProvider : MonoBehaviour
{
    public float AudioTime = 0f; //notes get this value

    float startTime;
    float speed;
    public bool isStart = false;
    public float playStartTime = 0f;
    public float audioOffset = 0f;
    public AudioSource bgm;
    public SoundEffect SE;
    public SettingsManager settings;

    public void SetStartTime(float _playStartTime, float _speed)
    {
        playStartTime = _playStartTime;
        AudioTime = playStartTime;
        speed = _speed;
        SE.generateSoundEffectList(playStartTime);
        startTime = Time.time;
        isStart = true;
        bgm.time = AudioTime;
        bgm.Play();
    }

    public void Pause()
    {
        isStart = false;
        bgm.Stop();
    }

    public void Resume()
    {
        startTime = Time.time;
        playStartTime = AudioTime;
        isStart = true;
        bgm.time = AudioTime;
        bgm.Play();
    }

    public void ResetStartTime()
    {
        isStart = false;
        AudioTime = playStartTime;
        bgm.Stop();
    }

    void Update()
    {
        if (isStart)
        {
            audioOffset = settings.offset;
            AudioTime = (Time.time - startTime) * speed + playStartTime;
            var delta = AudioTime - bgm.time;
            //print(delta);
            if (AudioTime >= 0 && Mathf.Abs(delta) > 0.03)
            {
                Debug.LogError("bgm time delay > 0.03");
                if(AudioTime > bgm.clip.length) {
                    bgm.Stop();
                    isStart = false;
                }
                if (AudioTime > bgm.time)
                    startTime += Mathf.Abs(delta)*0.7f;
                else
                    startTime -= Mathf.Abs(delta)*0.7f;
            }
            SE.SoundEffectUpdate(audioOffset);
        }
    }
}
