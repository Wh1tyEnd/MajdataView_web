using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Newtonsoft.Json;
using JsonFormat;
using System.Linq;

public class SongInformation : MonoBehaviour
{
    public static int playID = 0;
    public static List<SongDetail> songlist = new List<SongDetail>();
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
   
    public static void sortSonglist()
    {
        if(songlist.Count > 0)
        {
            songlist = songlist.OrderBy(x => x.Id).ToList();
        }
    }
    public static string getChartID()
    {
        Debug.Log(playID);
        return songlist[playID].Id.ToString();
    }
}
