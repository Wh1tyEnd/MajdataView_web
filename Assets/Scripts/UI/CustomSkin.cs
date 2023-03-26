﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class CustomSkin : MonoBehaviour
{
    SpriteRenderer Outline;

    public bool loadFromDisk;

    public Sprite Tap;
    public Sprite Tap_Each;
    public Sprite Tap_Break;
    public Sprite Tap_Ex;

    public Sprite Slide;
    public Sprite Slide_Each;
    public Sprite Slide_Break;
    public Sprite[] Wifi = new Sprite[11];
    public Sprite[] Wifi_Each = new Sprite[11];
    public Sprite[] Wifi_Break = new Sprite[11];

    public Sprite Star;
    public Sprite Star_Double;
    public Sprite Star_Each;
    public Sprite Star_Each_Double;
    public Sprite Star_Break;
    public Sprite Star_Break_Double;
    public Sprite Star_Ex;
    public Sprite Star_Ex_Double;

    public Sprite Hold;
    public Sprite Hold_Each;
    public Sprite Hold_Ex;
    public Sprite Hold_Break;

    public Sprite[] Just = new Sprite[6];
    public Sprite JudgeText_Normal;
    public Sprite JudgeText_Break;

    public Sprite Touch;
    public Sprite Touch_Each;
    public Sprite TouchPoint;
    public Sprite TouchPoint_Each;
    public Sprite TouchJust;
    public Sprite[] TouchBorder = new Sprite[2];
    public Sprite[] TouchBorder_Each = new Sprite[2];

    public Sprite[] TouchHold = new Sprite[5];

    public Texture2D test;

    // Start is called before the first frame update
    void Start()
    {
        if (loadFromDisk) 
        {
            var path = new DirectoryInfo(Application.dataPath).Parent.FullName + "/Skin/";
            Outline = gameObject.GetComponent<SpriteRenderer>();
            print(path);
            
            Outline.sprite = SpriteLoader.LoadSpriteFromFile(path + "/outline.png");

            Tap = SpriteLoader.LoadSpriteFromFile(path + "/tap.png");
            Tap_Each = SpriteLoader.LoadSpriteFromFile(path + "/tap_each.png");
            Tap_Break = SpriteLoader.LoadSpriteFromFile(path + "/tap_break.png");
            Tap_Ex = SpriteLoader.LoadSpriteFromFile(path + "/tap_ex.png");

            Slide = SpriteLoader.LoadSpriteFromFile(path + "/slide.png");
            Slide_Each = SpriteLoader.LoadSpriteFromFile(path + "/slide_each.png");
            Slide_Break = SpriteLoader.LoadSpriteFromFile(path + "/slide_break.png");
            for (int i = 0; i < 11; i++)
            {
                Wifi[i] = SpriteLoader.LoadSpriteFromFile(path + "/wifi_" + i + ".png");
                Wifi_Each[i] = SpriteLoader.LoadSpriteFromFile(path + "/wifi_each_" + i + ".png");
                Wifi_Break[i] = SpriteLoader.LoadSpriteFromFile(path + "/wifi_break_" + i + ".png");
            }

            Star = SpriteLoader.LoadSpriteFromFile(path + "/star.png");
            Star_Double = SpriteLoader.LoadSpriteFromFile(path + "/star_double.png");
            Star_Each = SpriteLoader.LoadSpriteFromFile(path + "/star_each.png");
            Star_Each_Double = SpriteLoader.LoadSpriteFromFile(path + "/star_each_double.png");
            Star_Break = SpriteLoader.LoadSpriteFromFile(path + "/star_break.png");
            Star_Break_Double = SpriteLoader.LoadSpriteFromFile(path + "/star_break_double.png");
            Star_Ex = SpriteLoader.LoadSpriteFromFile(path + "/star_ex.png");
            Star_Ex_Double = SpriteLoader.LoadSpriteFromFile(path + "/star_ex_double.png");

            var border = new Vector4(0, 58, 0, 58);
            Hold = SpriteLoader.LoadSpriteFromFile(path + "/hold.png", border);
            Hold_Each = SpriteLoader.LoadSpriteFromFile(path + "/hold_each.png", border);
            Hold_Ex = SpriteLoader.LoadSpriteFromFile(path + "/hold_ex.png", border);
            Hold_Break = SpriteLoader.LoadSpriteFromFile(path + "/hold_break.png", border);

            Just[0] = SpriteLoader.LoadSpriteFromFile(path + "/just_curv_r.png");
            Just[1] = SpriteLoader.LoadSpriteFromFile(path + "/just_str_r.png");
            Just[2] = SpriteLoader.LoadSpriteFromFile(path + "/just_wifi_u.png");
            Just[3] = SpriteLoader.LoadSpriteFromFile(path + "/just_curv_l.png");
            Just[4] = SpriteLoader.LoadSpriteFromFile(path + "/just_str_l.png");
            Just[5] = SpriteLoader.LoadSpriteFromFile(path + "/just_wifi_d.png");

            JudgeText_Normal = SpriteLoader.LoadSpriteFromFile(path + "/judge_text_normal.png");
            JudgeText_Break = SpriteLoader.LoadSpriteFromFile(path + "/judge_text_break.png");

            Touch = SpriteLoader.LoadSpriteFromFile(path + "/touch.png");
            Touch_Each = SpriteLoader.LoadSpriteFromFile(path + "/touch_each.png");
            TouchPoint = SpriteLoader.LoadSpriteFromFile(path + "/touch_point.png");
            TouchPoint_Each = SpriteLoader.LoadSpriteFromFile(path + "/touch_point_each.png");

            TouchJust = SpriteLoader.LoadSpriteFromFile(path + "/touch_just.png");

            TouchBorder[0] = SpriteLoader.LoadSpriteFromFile(path + "/touch_border_2.png");
            TouchBorder[1] = SpriteLoader.LoadSpriteFromFile(path + "/touch_border_3.png");
            TouchBorder_Each[0] = SpriteLoader.LoadSpriteFromFile(path + "/touch_border_2_each.png");
            TouchBorder_Each[1] = SpriteLoader.LoadSpriteFromFile(path + "/touch_border_3_each.png");

            for (int i = 0; i < 4; i++)
            {
                TouchHold[i] = SpriteLoader.LoadSpriteFromFile(path + "/touchhold_" + i +".png");
            }
            TouchHold[4] = SpriteLoader.LoadSpriteFromFile(path + "/touchhold_border.png");
        }
    }
}
