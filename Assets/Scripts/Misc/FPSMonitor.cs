using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// 监视和设置帧数的小工具
public class FPSMonitor : MonoBehaviour
{
    public int frameRate = -1;
    
    private int _frame;
    private float _lastTime;
    private float _frameDeltaTime;
    private float _Fps;
    public float _frameCalcInterval = 1f;
    public Text displayer;

 
    void Start()
    {
        Application.targetFrameRate = -1;
        _lastTime = Time.realtimeSinceStartup;
    }
 
    void Update()
    {
        FrameCalculate();
        string msg = string.Format("FPS: {0:N1}\nDeltaTime: {1:N4}", _Fps, _frameDeltaTime);
        displayer.text = msg;
    }
 
    private void FrameCalculate()
    {
        _frame++;
        if (Time.realtimeSinceStartup - _lastTime < _frameCalcInterval) {return;}
 
        float time = Time.realtimeSinceStartup - _lastTime;
        _Fps = _frame / time;
        _frameDeltaTime = Time.deltaTime;
 
        _lastTime = Time.realtimeSinceStartup;
        _frame = 0;
    }
}
