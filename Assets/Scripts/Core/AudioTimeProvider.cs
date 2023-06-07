﻿using System.Collections;
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

    public void SetStartTime(float _playStartTime, float _speed)
    {
        playStartTime = _playStartTime;
        AudioTime = playStartTime;
        speed = _speed;
        SE.generateSoundEffectList(playStartTime);
        startTime = Time.time + audioOffset;
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
        startTime = Time.time + audioOffset;
        playStartTime = AudioTime - audioOffset;
        isStart = true;
        bgm.time = AudioTime - audioOffset;
        bgm.Play();
    }

    public void ResetStartTime()
    {
        playStartTime = 0f;
        AudioTime = 0f;
        bgm.Stop();
        isStart = false;
    }

    void Update()
    {
        if (isStart)
        {
            AudioTime = (Time.time - startTime) * speed + playStartTime;
            var delta = AudioTime - audioOffset - bgm.time;
            //print(delta);
            if (AudioTime >= 0 && Mathf.Abs(delta) > 0.03)
            {
                Debug.LogError("bgm time delay > 0.03");
                if(AudioTime + audioOffset > bgm.clip.length) {
                    bgm.Stop();
                    isStart = false;
                }
                if (AudioTime + audioOffset > bgm.time)
                    startTime += Mathf.Abs(delta)*0.7f;
                else
                    startTime -= Mathf.Abs(delta)*0.7f;
            }
            SE.SoundEffectUpdate();
        }
    }
}
