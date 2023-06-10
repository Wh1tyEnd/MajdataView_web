// source code from https://github.com/LingFeng-bbben/MajdataEdit/blob/master/SoundEffect.cs
// Licensed under GPL-3.0
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Text;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class SoundEffectTiming
{
    public double time;
    public bool hasAnswer = false;
    public bool hasJudge = false;
    public bool hasJudgeBreak = false;
    public bool hasBreak = false;
    public bool hasTouch = false;
    public bool hasHanabi = false;
    public bool hasJudgeEx = false;
    public bool hasTouchHold = false;
    public bool hasTouchHoldEnd = false;
    public bool hasSlide = false;
    public bool hasAllPerfect = false;
    public bool hasClock = false;
    public bool hasTrackStart = false;

    public SoundEffectTiming(double _time, bool _hasAnswer = false, bool _hasJudge = false, bool _hasJudgeBreak = false,
                                bool _hasBreak = false, bool _hasTouch = false, bool _hasHanabi = false,
                                bool _hasJudgeEx = false, bool _hasTouchHold = false, bool _hasSlide = false,
                                bool _hasTouchHoldEnd = false, bool _hasAllPerfect = false, bool _hasClock = false, bool _hasTrackStart = false)
    {
        time = _time;
        hasAnswer = _hasAnswer;
        hasJudge = _hasJudge;
        hasJudgeBreak = _hasJudgeBreak; // 我是笨蛋
        hasBreak = _hasBreak;
        hasTouch = _hasTouch;
        hasHanabi = _hasHanabi;
        hasJudgeEx = _hasJudgeEx;
        hasTouchHold = _hasTouchHold;
        hasSlide = _hasSlide;
        hasTouchHoldEnd = _hasTouchHoldEnd;
        hasAllPerfect = _hasAllPerfect;
        hasClock = _hasClock;
        hasTrackStart = _hasTrackStart;
    }
}

public class SoundEffect: MonoBehaviour
{
    double extraTime4AllPerfect;     // 需要在播放完后等待All Perfect特效的秒数

    private double songLength = 0f;

    public AudioSource bgmStream;
    public AudioSource answerStream;
    public AudioSource judgeStream;
    public AudioSource judgeBreakStream;   // 这个是break的判定音效 不是欢呼声
    public AudioSource breakStream;        // 这个才是欢呼声
    public AudioSource judgeExStream;
    public AudioSource hanabiStream;
    public AudioSource holdRiserStream;
    public AudioSource trackStartStream;
    public AudioSource slideStream;
    public AudioSource touchStream;
    public AudioSource allperfectStream;
    public AudioSource fanfareStream;
    public AudioSource clockStream;

    public AudioTimeProvider timeProvider;

    List<SoundEffectTiming> waitToBePlayed;
    public class audioUrl
    {
        public string downloadUrl;
    }
    // Download audio and assign to bgm after uploaded
    public IEnumerator LoadWebAudio(string path, Action<float> callback = null, Action successcallback = null)
    {
        if (path == string.Empty) {Debug.LogError("Empty path!"); yield break;}
        Debug.Log("Downloading audio from " + path);
        UnityWebRequest trackreq = UnityWebRequest.Get(path);
        trackreq.downloadHandler = new DownloadHandlerAudioClip(path,AudioType.MPEG);
        trackreq.SendWebRequest();
        while (!trackreq.isDone) {
            callback.Invoke(trackreq.downloadProgress);
            yield return new WaitForSecondsRealtime(0.1f);
        }
        if (trackreq.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error downloading audio: " + trackreq.error);
        }
        else
        {
            var clip = DownloadHandlerAudioClip.GetContent(trackreq);
            if (clip != null)
            {
                bgmStream.clip = clip;
                if (successcallback != null)
                    successcallback.Invoke();
            }
            else { Debug.LogError("AudioClip is null!"); }
        }
        
    }

    public void generateSoundEffectList(double startTime)
    {
        songLength = bgmStream.clip.length;
        waitToBePlayed = new List<SoundEffectTiming>();
        foreach (var noteGroup in SimaiProcess.notelist)
        {
            if (noteGroup.time < startTime) { continue; }

            SoundEffectTiming stobj;

            // 如果目前为止已经有一个SE了 那么就直接使用这个SE
            var combIndex = waitToBePlayed.FindIndex(o => Math.Abs(o.time - noteGroup.time) < 0.001f);
            if (combIndex != -1)
            {
                stobj = waitToBePlayed[combIndex];
            }
            else
            {
                stobj = new SoundEffectTiming(noteGroup.time);
            }

            var notes = noteGroup.getNotes();
            foreach (SimaiNote note in notes)
            {
                switch (note.noteType)
                {
                    case SimaiNoteType.Tap:
                        {
                            stobj.hasAnswer = true;
                            if (note.isBreak)
                            {
                                // 如果是Break 则有Break判定音和Break欢呼音（2600）
                                stobj.hasBreak = true;
                                stobj.hasJudgeBreak = true;
                            }
                            if (note.isEx)
                            {
                                // 如果是Ex 则有Ex判定音
                                stobj.hasJudgeEx = true;
                            }
                            if (!note.isBreak && !note.isEx)
                            {
                                // 如果二者皆没有 则是普通note 播放普通判定音
                                stobj.hasJudge = true;
                            }
                            break;
                        }
                    case SimaiNoteType.Hold:
                        {
                            stobj.hasAnswer = true;
                            // 类似于Tap 判断Break和Ex的音效 二者皆无则为普通
                            if (note.isBreak)
                            {
                                stobj.hasBreak = true;
                                stobj.hasJudgeBreak = true;
                            }
                            if (note.isEx)
                            {
                                stobj.hasJudgeEx = true;
                            }
                            if (!note.isBreak && !note.isEx)
                            {
                                stobj.hasJudge = true;
                            }

                            // 计算Hold尾部的音效
                            if (!(note.holdTime <= 0.00f))
                            {
                                // 如果是短hold（六角tap），则忽略尾部音效。否则，才会计算尾部音效
                                var targetTime = noteGroup.time + note.holdTime;
                                var nearIndex = waitToBePlayed.FindIndex(o => Math.Abs(o.time - targetTime) < 0.001f);
                                if (nearIndex != -1)
                                {
                                    waitToBePlayed[nearIndex].hasAnswer = true;
                                    if (!note.isBreak && !note.isEx)
                                    {
                                        waitToBePlayed[nearIndex].hasJudge = true;
                                    }
                                }
                                else
                                {
                                    // 只有最普通的Hold才有结尾的判定音 Break和Ex型则没有（Break没有为推定）
                                    SoundEffectTiming holdRelease = new SoundEffectTiming(targetTime, _hasAnswer: true, _hasJudge: !note.isBreak && !note.isEx);
                                    waitToBePlayed.Add(holdRelease);
                                }
                            }
                            break;
                        }
                    case SimaiNoteType.Slide:
                        {
                            if (!note.isSlideNoHead)
                            {
                                // 当Slide不是无头星星的时候 才有answer音和判定音
                                stobj.hasAnswer = true;
                                if (note.isBreak)
                                {
                                    stobj.hasBreak = true;
                                    stobj.hasJudgeBreak = true;
                                }
                                if (note.isEx)
                                {
                                    stobj.hasJudgeEx = true;
                                }
                                if (!note.isBreak && !note.isEx)
                                {
                                    stobj.hasJudge = true;
                                }
                            }

                            // Slide启动音效
                            var targetTime = note.slideStartTime;
                            var nearIndex = waitToBePlayed.FindIndex(o => Math.Abs(o.time - targetTime) < 0.001f);
                            if (nearIndex != -1)
                            {
                                waitToBePlayed[nearIndex].hasSlide = true;
                            }
                            else
                            {
                                SoundEffectTiming slide = new SoundEffectTiming(targetTime, _hasSlide: true);
                                waitToBePlayed.Add(slide);
                            }
                            // Slide尾巴 如果是Break Slide的话 就要添加一个Break音效
                            if (note.isSlideBreak)
                            {
                                targetTime = note.slideStartTime + note.slideTime;
                                nearIndex = waitToBePlayed.FindIndex(o => Math.Abs(o.time - targetTime) < 0.001f);
                                if (nearIndex != -1)
                                {
                                    waitToBePlayed[nearIndex].hasBreak = true;
                                }
                                else
                                {
                                    SoundEffectTiming slide = new SoundEffectTiming(targetTime, _hasBreak: true);
                                    waitToBePlayed.Add(slide);
                                }
                            }
                            break;
                        }
                    case SimaiNoteType.Touch:
                        {
                            stobj.hasAnswer = true;
                            stobj.hasTouch = true;
                            if (note.isHanabi)
                            {
                                stobj.hasHanabi = true;
                            }
                            break;
                        }
                    case SimaiNoteType.TouchHold:
                        {
                            stobj.hasAnswer = true;
                            stobj.hasTouch = true;
                            stobj.hasTouchHold = true;
                            // 计算TouchHold结尾
                            var targetTime = noteGroup.time + note.holdTime;
                            var nearIndex = waitToBePlayed.FindIndex(o => Math.Abs(o.time - targetTime) < 0.001f);
                            if (nearIndex != -1)
                            {
                                if (note.isHanabi)
                                {
                                    waitToBePlayed[nearIndex].hasHanabi = true;
                                }
                                waitToBePlayed[nearIndex].hasAnswer = true;
                                waitToBePlayed[nearIndex].hasTouchHoldEnd = true;
                            }
                            else
                            {
                                SoundEffectTiming tHoldRelease = new SoundEffectTiming(targetTime, _hasAnswer: true, _hasHanabi: note.isHanabi, _hasTouchHoldEnd: true);
                                waitToBePlayed.Add(tHoldRelease);
                            }
                            break;
                        }
                }
            }

            if (combIndex != -1)
            {
                waitToBePlayed[combIndex] = stobj;
            }
            else
            {
                waitToBePlayed.Add(stobj);
            }
        }
        waitToBePlayed.Sort((o1, o2) => o1.time < o2.time ? -1 : 1);

        double apTime = GetAllPerfectStartTime();
        if (songLength < apTime + 4.0)
        {
            // 如果BGM的时长不足以播放完AP特效 这里假设AP特效持续4秒
            extraTime4AllPerfect = apTime + 4.0 - songLength; // 预留给AP的额外时间（播放结束后）
        }
        else
        {
            // 如果足够播完 那么就等到BGM结束再停止
            extraTime4AllPerfect = -1;
        }
    }
    double GetAllPerfectStartTime()
    {
        // 获取All Perfect理论上播放的时间点 也就是最后一个被完成的note
        double latestNoteFinishTime = -1;
        double baseTime, noteTime;
        foreach (var noteGroup in SimaiProcess.notelist)
        {
            baseTime = noteGroup.time;
            foreach (var note in noteGroup.getNotes())
            {
                if (note.noteType == SimaiNoteType.Tap || note.noteType == SimaiNoteType.Touch)
                {
                    noteTime = baseTime;
                }
                else if (note.noteType == SimaiNoteType.Hold || note.noteType == SimaiNoteType.TouchHold)
                {
                    noteTime = baseTime + note.holdTime;
                }
                else if (note.noteType == SimaiNoteType.Slide)
                {
                    noteTime = note.slideStartTime + note.slideTime;
                }
                else
                {
                    noteTime = -1;
                }
                if (noteTime > latestNoteFinishTime)
                {
                    latestNoteFinishTime = noteTime;
                }
            }
        }
        return latestNoteFinishTime;
    }
    public void SoundEffectUpdate(float audioOffset)
    {
        try
        {
            var currentTime = timeProvider.AudioTime + audioOffset;
            // var waitToBePlayed = SimaiProcess.notelist.FindAll(o => o.havePlayed == false && o.time > currentTime);
            if (waitToBePlayed.Count < 1) return;
            var nearestTime = waitToBePlayed[0].time;
            //Console.WriteLine(nearestTime);
            // if (Math.Abs(currentTime - nearestTime) < 0.055)
            if (currentTime - nearestTime > 0)
            {
                SoundEffectTiming se = waitToBePlayed[0];
                waitToBePlayed.RemoveAt(0);

                if (se.hasTrackStart)
                {
                    trackStartStream.Play();
                }
                if (se.hasAnswer)
                {
                    answerStream.Play();
                }
                if (se.hasJudge)
                {
                    judgeStream.Play();
                }
                if (se.hasJudgeBreak)
                {
                    judgeBreakStream.Play();
                }
                if (se.hasJudgeEx)
                {
                    judgeExStream.Play();
                }
                if (se.hasBreak)
                {
                    breakStream.Play();
                }
                if (se.hasTouch)
                {
                    touchStream.Play();
                }
                if (se.hasHanabi) //may cause delay
                {
                    hanabiStream.Play();
                }
                if (se.hasTouchHold)
                {
                    holdRiserStream.Play();
                }
                if (se.hasTouchHoldEnd)
                {
                    holdRiserStream.Stop();
                }
                if (se.hasSlide)
                {
                    slideStream.Play();
                }
                if (se.hasAllPerfect)
                {
                    allperfectStream.Play();
                    fanfareStream.Play();
                }
                if (se.hasClock)
                {
                    clockStream.Play();
                }
            }
        }
        catch { }
    }
}
