// 各种数据类型定义汇总，融合了MajData 和 SimaiProcess后半部分（
// source code from https://github.com/LingFeng-bbben/MajdataEdit/blob/master/SimaiProcess.cs
// Licensed under GPL-3.0

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;


public class SimaiTimingPoint 
{
    public double time;
    public bool havePlayed;
    public int rawTextPositionX;
    public int rawTextPositionY;
    public string notesContent;
    public float currentBpm = -1;
    public List<SimaiNote> noteList = new List<SimaiNote>(); //only used for json serialize
    public float HSpeed = 1f;
    public SimaiTimingPoint(double _time, int textposX = 0, int textposY = 0,string _content = "",float bpm=0f,float _hspeed = 1f)
    {
        time = _time;
        rawTextPositionX = textposX;
        rawTextPositionY = textposY;
        notesContent = _content.Replace("\n","").Replace(" ","");
        currentBpm = bpm;
        HSpeed = _hspeed;
    }

    public List<SimaiNote> getNotes()
    {
        if (noteList.Count != 0)
        {
            return noteList;
        }
        
        List<SimaiNote> simaiNotes = new List<SimaiNote>();
        if (notesContent == "") return simaiNotes;
        try
        {
            int dummy = 0;
            if (notesContent.Length == 2 && int.TryParse(notesContent, out dummy))//连写数字
            {
                simaiNotes.Add(getSingleNote(notesContent[0].ToString()));
                simaiNotes.Add(getSingleNote(notesContent[1].ToString()));
                return simaiNotes;
            }
            if (notesContent.Contains('/'))
            {
                var notes = notesContent.Split('/');
                foreach (var note in notes)
                {
                    if (note.Contains('*'))
                    {
                        simaiNotes.AddRange(getSameHeadSlide(note));
                    }
                    else
                    {
                        simaiNotes.Add(getSingleNote(note));
                    }
                }
                return simaiNotes;
            }   
            if (notesContent.Contains('*'))
            {
                simaiNotes.AddRange(getSameHeadSlide(notesContent));
                return simaiNotes;
            }
            simaiNotes.Add(getSingleNote(notesContent));
            noteList = simaiNotes;
            return simaiNotes;
        }
        catch
        {
            noteList = new List<SimaiNote>();
            return noteList;
        }

    }
    
    private List<SimaiNote> getSameHeadSlide(string content)
    {
        List<SimaiNote> simaiNotes = new List<SimaiNote>();
        var noteContents = content.Split('*');
        var note1 = getSingleNote(noteContents[0]);
        simaiNotes.Add(note1);
        var newNoteContent = noteContents.ToList();
        newNoteContent.RemoveAt(0);
        //删除第一个NOTE
        foreach (var item in newNoteContent)
        {
            var note2text = note1.startPosition + item;
            var note2 = getSingleNote(note2text);
            note2.isSlideNoHead = true;
            simaiNotes.Add(note2);
        }
        return simaiNotes;
    }

    private SimaiNote getSingleNote(string noteText)
    {
        SimaiNote simaiNote = new SimaiNote();

        if (isTouchNote(noteText))
        {
            simaiNote.touchArea = noteText[0];
            if (simaiNote.touchArea != 'C') simaiNote.startPosition = int.Parse(noteText[1].ToString());
            else simaiNote.startPosition = 8;
            simaiNote.noteType = SimaiNoteType.Touch;
        }
        else
        {
            simaiNote.startPosition = int.Parse(noteText[0].ToString());
            simaiNote.noteType = SimaiNoteType.Tap; //if nothing happen in following if
        }
        if (noteText.Contains('f'))
        {
            simaiNote.isHanabi= true;
        }

        //hold
        if (noteText.Contains('h')) {
            if (isTouchNote(noteText)) {
                simaiNote.noteType = SimaiNoteType.TouchHold;
                simaiNote.holdTime = getTimeFromBeats(noteText);
                //Console.WriteLine("Hold:" +simaiNote.touchArea+ simaiNote.startPosition + " TimeLastFor:" + simaiNote.holdTime);
            }
            else
            {
                simaiNote.noteType = SimaiNoteType.Hold;
                if (noteText.Last() == 'h')
                {
                    simaiNote.holdTime = 0;
                }
                else
                {
                    simaiNote.holdTime = getTimeFromBeats(noteText);
                }
                //Console.WriteLine("Hold:" + simaiNote.startPosition + " TimeLastFor:" + simaiNote.holdTime);
            }
        }
        //slide
        if (isSlideNote(noteText)) {
            simaiNote.noteType = SimaiNoteType.Slide;
            simaiNote.slideTime = getTimeFromBeats(noteText);
            var timeStarWait = getStarWaitTime(noteText);
            simaiNote.slideStartTime = time + timeStarWait;
            if(noteText.Contains('!'))
            {
                simaiNote.isSlideNoHead = true;
                noteText = noteText.Replace("!", "");
            }else if(noteText.Contains('?'))
            {
                simaiNote.isSlideNoHead = true;
                noteText = noteText.Replace("?", "");
            }
            //Console.WriteLine("Slide:" + simaiNote.startPosition + " TimeLastFor:" + simaiNote.slideTime);
        }
        //break
        if (noteText.Contains('b'))
        {
            if (simaiNote.noteType == SimaiNoteType.Slide)
            {
                // 如果是Slide 则要检查这个b到底是星星头的还是Slide本体的

                // !!! **SHIT CODE HERE** !!!
                int startIndex = 0;
                while ((startIndex = noteText.IndexOf('b', startIndex)) != -1)
                {
                    if (startIndex < noteText.Length - 1)
                    {
                        // 如果b不是最后一个字符 我们就检查b之后一个字符是不是`[`符号：如果是 那么就是break slide
                        if (noteText[startIndex + 1] == '[')
                        {
                            simaiNote.isSlideBreak = true;
                        }
                        else
                        {
                            // 否则 那么不管这个break出现在slide的哪一个地方 我们都认为他是星星头的break
                            // SHIT CODE!
                            simaiNote.isBreak = true;
                        }
                    }
                    else
                    {
                        // 如果b符号是整个文本的最后一个字符 那么也是break slide（Simai语法）
                        simaiNote.isSlideBreak = true;
                    }
                    startIndex++;
                }
            }
            else
            {
                // 除此之外的Break就无所谓了
                simaiNote.isBreak = true;
            }
            noteText = noteText.Replace("b", "");
        }
        //mine, copy b
        if (noteText.Contains('m'))
        {
            if (simaiNote.noteType == SimaiNoteType.Slide)
            {
                int startIndex = 0;
                while ((startIndex = noteText.IndexOf('m', startIndex)) != -1)
                {
                    if (startIndex < noteText.Length - 1)
                    {
                        if (noteText[startIndex + 1] == '[')
                        {
                            simaiNote.isSlideMine = true;
                        }
                        else
                        {
                            simaiNote.isMine = true;
                        }
                    }
                    else
                    {
                        simaiNote.isSlideMine = true;
                    }
                    startIndex++;
                }
            }
            else
            {
                // 除此之外的Break就无所谓了
                simaiNote.isMine = true;
            }
            noteText = noteText.Replace("m", "");
        }
        //EX
        if (noteText.Contains('x'))
        {
            simaiNote.isEx = true;
            noteText = noteText.Replace("x", "");
        }
        //starHead
        if (noteText.Contains('$'))
        {
            simaiNote.isForceStar = true;
            if (noteText.Count(o=>o=='$') == 2)
                simaiNote.isFakeRotate = true;
            noteText = noteText.Replace("$", "");
        }
        simaiNote.noteContent = noteText;
        return simaiNote;
    }

    private bool isSlideNote(string noteText)
    {
        string SlideMarks = "-^v<>Vpqszw";
        foreach(var mark in SlideMarks)
        {
            if (noteText.Contains(mark)) return true;
        }
        return false;
    }
    private bool isTouchNote(string noteText)
    {
        string SlideMarks = "ABCDE";
        foreach (var mark in SlideMarks)
        {
            if (noteText.StartsWith(mark.ToString())) return true;
        }
        return false;
    }

    private double getTimeFromBeats(string noteText)
    {
        if (noteText.Count((c) => { return c == '['; }) > 1)
        {
            // 组合slide 有多个时长
            double wholeTime = 0;

            int partStartIndex = 0;
            while (noteText.IndexOf('[', partStartIndex) >= 0)
            {
                var startIndex = noteText.IndexOf('[', partStartIndex);
                var overIndex = noteText.IndexOf(']', partStartIndex);
                partStartIndex = overIndex + 1;
                var innerString = noteText.Substring(startIndex + 1, overIndex - startIndex - 1);
                var timeOneBeat = 1d / (currentBpm / 60d);
                if (innerString.Count(o => o == '#') == 1)
                {
                    var times = innerString.Split('#');
                    if (times[1].Contains(':'))
                    {
                        innerString = times[1];
                        timeOneBeat = 1d / (double.Parse(times[0]) / 60d);
                    }
                    else
                    {
                        wholeTime += double.Parse(times[1]);
                        continue;
                    }
                }
                if (innerString.Count(o => o == '#') == 2)
                {
                    var times = innerString.Split('#');
                    wholeTime += double.Parse(times[2]);
                    continue;
                }
                var numbers = innerString.Split(':');
                var divide = int.Parse(numbers[0]);
                var count = int.Parse(numbers[1]);


                wholeTime += (timeOneBeat * 4d / (double)divide) * (double)count;
            }
            return wholeTime;
        }
        else
        {
            var startIndex = noteText.IndexOf('[');
            var overIndex = noteText.IndexOf(']');
            var innerString = noteText.Substring(startIndex + 1, overIndex - startIndex - 1);
            var timeOneBeat = 1d / (currentBpm / 60d);
            if (innerString.Count(o => o == '#') == 1)
            {
                var times = innerString.Split('#');
                if (times[1].Contains(':'))
                {
                    innerString = times[1];
                    timeOneBeat = 1d / (double.Parse(times[0]) / 60d);
                }
                else
                {
                    return double.Parse(times[1]);
                }
            }
            if (innerString.Count(o => o == '#') == 2)
            {
                var times = innerString.Split('#');
                return double.Parse(times[2]);
            }
            var numbers = innerString.Split(':');   //TODO:customBPM
            var divide = int.Parse(numbers[0]);
            var count = int.Parse(numbers[1]);


            return (timeOneBeat * 4d / (double)divide) * (double)count;
        }
    }

    private double getStarWaitTime(string noteText)
    {
        var startIndex = noteText.IndexOf('[');
        var overIndex = noteText.IndexOf(']');
        var innerString = noteText.Substring(startIndex + 1, overIndex - startIndex - 1);
        double bpm = currentBpm ;
        if (innerString.Count(o => o == '#') == 1)
        {
            var times = innerString.Split('#');
            bpm = double.Parse(times[0]);
        }
        if (innerString.Count(o => o == '#') == 2)
        {
            var times = innerString.Split('#');
            return double.Parse(times[0]);
        }
        return 1d / (bpm / 60d);
    }
}

public enum SimaiNoteType
{
    Tap,Slide,Hold,Touch,TouchHold
}

public class SimaiNote
{
    public SimaiNoteType noteType;
    public bool isMine = false;
    public bool isSlideMine = false;
    public bool isBreak = false;
    public bool isSlideBreak = false;
    public bool isHanabi = false;
    public bool isEx = false;
    public bool isSlideNoHead = false;
    public bool isForceStar = false;
    public bool isFakeRotate = false;

    public int startPosition = 1; //键位（1-8）
    public char touchArea = ' ';

    public double holdTime = 0d;

    public double slideStartTime = 0d;
    public double slideTime = 0d;

    public string noteContent; //used for star explain
}

public enum EditorComboIndicator
{
    None,
    // List of viable indicators that won't be a static content.
    // ScoreBorder, AchievementMaxDown, ScoreDownDeluxe are static.
    Combo, ScoreClassic, AchievementClassic, AchievementDownClassic,
    AchievementDeluxe = 11, AchievementDownDeluxe, ScoreDeluxe,
    // Please prefix custom indicator with C
    CScoreDedeluxe = 101, CScoreDownDedeluxe,
    MAX
}