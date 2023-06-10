using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;
using Newtonsoft.Json;
using JsonFormat;
using API;
using System.Linq;
using TMPro;

public class SongSelect : MonoBehaviour
{
    public GameObject coverSheet;
    public GameObject parentConvas;
    public GameObject buttons;
    public GameObject loading;
    public GameObject loadingtext;

    
    public int idPointer = 0;
    
    const string SongListApiPath = ApiAccess.ROOT + "SongList";
    const string BGApiPath = ApiAccess.ROOT + "Image/{0}";

    int listlen = 9;
    GameObject[] showList;
    int centerNum = 4;
    float coverDist = 3.3f;
    RawImage rawImage;
    
    // Start is called before the first frame update
    void Start()
    {
        showList = new GameObject[listlen];
        Debug.Log(showList.Length);
        init();
        
        //initShowList();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.D))
        {
            moveRight();
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            moveLeft();
        }
        
    }
    
    public void init()
    {
        Action initAction = () =>
        {
            if (WebLoader.songlist.Count <= 0)
            {
                loadingtext.GetComponent<TMP_Text>().text = "Warning:Empty";
                loadingtext.GetComponent<TMP_Text>().color = new Color(255,0,0);
            }
            else
            {
                initShowList();
            }
            
        };
        StartCoroutine(WebLoader.getSongList(SongListApiPath, initAction));
    }


    

    public void initShowList()
    {
        idPointer = WebLoader.playID;
        if(WebLoader.songlist.Count < centerNum)
        {
            extentList();
        }
        for (int i = 0; i < listlen; i++)
        {
            Debug.Log(i + "     " + showList.Length);
            GameObject temp = Instantiate(coverSheet, new Vector2(0 + (i - centerNum) * coverDist, 0), Quaternion.identity);
            temp.transform.parent = parentConvas.transform;
            Debug.Log(transportID(i - centerNum));
            setInfo(temp, transportID(i - centerNum));
            showList[i] = temp;
            if (i != centerNum)
            {
                showList[i].gameObject.transform.DOScale(new Vector3(7.8f, 7.8f, 7.8f), 0.5f);
            }
            else
            {
                showList[i].gameObject.transform.DOScale(new Vector3(10.8f, 10.8f, 10.8f), 0.5f);
            }

        }
        
        buttons.SetActive(true);
        loading.SetActive(false);
        loadingtext.SetActive(false);
    }

    public void playChart()
    {
        WebLoader.playID = transportID(0);
        SceneManager.LoadScene("main");
    }

    public void moveRight()
    {
        idPointer++;
        idPointer = transportID(0);
        GameObject wastedCover = showList[0];
        for (int i = 0; i < listlen-1; i++)
        {
            showList[i] = showList[i + 1];
            //Debug.Log(showList[i].name);
            showList[i].gameObject.transform.DOMove(new Vector2(0 + (i - centerNum) * coverDist, 0), 0.5f);
            if(i != centerNum)
            {
                showList[i].gameObject.transform.DOScale(new Vector3(7.8f, 7.8f, 7.8f), 0.5f);
            }
            else
            {
                showList[i].gameObject.transform.DOScale(new Vector3(10.8f, 10.8f, 10.8f), 0.5f);
            }
        }
        GameObject temp = Instantiate(coverSheet, new Vector3(0 + (listlen - 1 - centerNum) * coverDist, 0, 0), Quaternion.identity);
        temp.transform.parent = parentConvas.transform;
        temp.transform.localScale = new Vector3(7.8f, 7.8f, 7.8f);
        setInfo(temp, transportID(centerNum));
        showList[listlen-1] = temp;
        Destroy(wastedCover);
        
    }

    public void moveLeft()
    {
        idPointer--;
        idPointer = transportID(0);
        GameObject wastedCover = showList[listlen-1];
        for (int i = listlen-1; i > 0; i--)
        {


            showList[i] = showList[i - 1];
            //Debug.Log(showList[i].name);
            showList[i].gameObject.transform.DOMove(new Vector2(0 + (i - centerNum) * coverDist, 0), 0.5f);
            if (i != centerNum)
            {
                showList[i].gameObject.transform.DOScale(new Vector3(7.8f, 7.8f, 7.8f), 0.5f);
            }
            else
            {
                showList[i].gameObject.transform.DOScale(new Vector3(10.8f, 10.8f, 10.8f), 0.5f);
            }


        }
        GameObject temp = Instantiate(coverSheet, new Vector3(0 + (0 - centerNum) * coverDist, 0, 0), Quaternion.identity);
        temp.transform.parent = parentConvas.transform;
        temp.transform.localScale = new Vector3(7.8f, 7.8f, 7.8f);
        setInfo(temp, transportID(-centerNum));
        showList[0] = temp;
        Destroy(wastedCover);
        
    }

    

    public int transportID(int i)
    {
        if (WebLoader.songlist.Count != 0)
        {
            return (idPointer + i + WebLoader.songlist.Count) % WebLoader.songlist.Count;
        }
        else return 0;
    }

    public void extentList()
    {
        WebLoader.songlist = WebLoader.songlist.Concat(WebLoader.songlist).ToList();
        if(WebLoader.songlist.Count < centerNum)
        {
            extentList();
        }
    }

    public void setInfo(GameObject obj, int id)
    {
        obj.transform.GetChild(0).GetChild(1).GetChild(1).GetComponent<Text>().text = WebLoader.songlist[id].Id.ToString();
        obj.transform.GetChild(0).GetChild(1).GetChild(0).GetComponent<TMP_Text>().text = WebLoader.songlist[id].Title;
        obj.transform.GetChild(0).GetChild(1).GetChild(2).GetComponent<TMP_Text>().text = WebLoader.songlist[id].Artist;
        obj.transform.GetChild(0).GetChild(1).GetChild(3).GetComponent<TMP_Text>().text = WebLoader.songlist[id].Designer;
        Debug.Log(obj.transform.GetChild(0).GetChild(0).name);
        if (WebLoader.textureCache.ContainsKey(WebLoader.songlist[id].Id.ToString()))
        {
            StartCoroutine(WebLoader.LoadBGFromCache(WebLoader.songlist[id].Id.ToString(), obj.transform.GetChild(0).GetChild(0).GetComponent<RawImage>()));
        }
        else
        {
            StartCoroutine(WebLoader.LoadBGFromWeb(WebLoader.songlist[id].Id.ToString(), obj.transform.GetChild(0).GetChild(0).GetComponent<RawImage>()));
        }
        
    }
}
