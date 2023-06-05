using System;
using System.Runtime;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using DG.Tweening;


public class SongSelect : MonoBehaviour
{
    public GameObject coverSheet;
    public GameObject parentConvas;
    public GameObject buttons;
    public GameObject loading;
    public GameObject loadingtext;

    public Dictionary<string, Texture2D> textureCache= new Dictionary<string, Texture2D>();
    public int idPointer = 0;
    
    string MMFCpath = "https://www.maimaimfc.ink/_functions/contestList";
    int listlen = 9;
    public GameObject[] showList = new GameObject[9];
    int centerNum = 4;
    float coverDist = 3.3f;
    RawImage rawImage;
    SongList songlist;
    // Start is called before the first frame update
    void Start()
    {
        
        Debug.Log(showList.Length);
        StartCoroutine(getMMFCList(MMFCpath));
        
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

    [Serializable]
    public class SongList
    {
        public List<SongInfo> info;
    }
    [Serializable]
    public class SongInfo
    {
        public string Title;
        public string sequenceId;
    }
    public class audioUrl
    {
        public string downloadUrl;
    }

    public IEnumerator getMMFCList(string path)
    {
        
        if (path == string.Empty) { Debug.LogError("Empty path!"); yield break; }
        Debug.Log("Downloading song list from " + path);
        UnityWebRequest www = UnityWebRequest.Get(path);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error downloading data: " + www.error);
        }
        else
        {
            string songs = "{\"info\":" + www.downloadHandler.text + "}";
            Debug.Log(songs);
            songlist = JsonUtility.FromJson<SongList>(songs);
            Debug.Log(songlist.info[0].Title);
        }
        initShowList();
    }

    public IEnumerator LoadBGFromWeb(string id, RawImage rawImage)
    {
        string path = "https://www.maimaimfc.ink/_functions/contestEntry/" + id + "/3";
        Texture2D texture;
        if (path == string.Empty) { Debug.LogError("empty bg path!"); yield break; }
        Debug.Log("Downloading bg from " + path);
        UnityWebRequest previous = UnityWebRequest.Get(path);
        yield return previous.SendWebRequest();
        if (previous.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error downloading bg: " + previous.error);
        }
        else
        {
            string truePath = JsonUtility.FromJson<audioUrl>(previous.downloadHandler.text).downloadUrl;
            using (UnityWebRequest imageWeb = new UnityWebRequest(truePath, UnityWebRequest.kHttpVerbGET))
            {
                imageWeb.downloadHandler = new DownloadHandlerTexture();

                yield return imageWeb.SendWebRequest();

                texture = ((DownloadHandlerTexture)imageWeb.downloadHandler).texture;
            }
            Sprite sprite;
            sprite = Sprite.Create(
                texture,
                new Rect(0.0f, 0.0f, texture.width, texture.height),
                new Vector2(0.5f, 0.5f));

            rawImage.texture = texture;
            //spriteRender.sprite = sprite;
            var scale = 1080f / (float)sprite.texture.width;
            gameObject.transform.localScale = new Vector3(scale, scale, scale);
            textureCache.Add(id, texture);
        }

    }

    public IEnumerator LoadBGFromCache(string id, RawImage rawImage)
    {
        Texture2D texture = textureCache[id];
        Sprite sprite;
        sprite = Sprite.Create(
            texture,
            new Rect(0.0f, 0.0f, texture.width, texture.height),
            new Vector2(0.5f, 0.5f));

        rawImage.texture = texture;
        //spriteRender.sprite = sprite;
        var scale = 1080f / (float)sprite.texture.width;
        gameObject.transform.localScale = new Vector3(scale, scale, scale);
        yield return null;
    }

        public void initShowList()
    {
        
            for (int i = 0; i < listlen; i++)
            {
                Debug.Log(i + "     " + showList.Length);
                GameObject temp = Instantiate(coverSheet, new Vector2(0 + (i - centerNum) * coverDist, 0), Quaternion.identity);
                temp.transform.parent = parentConvas.transform;
                
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
        PlayerPrefs.SetString("id", songlist.info[transportID(0)].sequenceId);
        buttons.SetActive(true);
        loading.SetActive(false);
        loadingtext.SetActive(false);
    }

    public void playChart()
    {
        Debug.Log("play");
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
        PlayerPrefs.SetString("id", songlist.info[transportID(0)].sequenceId);
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
        PlayerPrefs.SetString("id", songlist.info[transportID(0)].sequenceId);
    }

    public int transportID(int i)
    {
        if (songlist.info.Count != 0)
        {
            return (idPointer + i + songlist.info.Count) % songlist.info.Count;
        }
        else return 0;
    }

    public void setInfo(GameObject obj, int id)
    {
        //obj.name = id.ToString();
        obj.transform.GetChild(0).GetChild(1).GetChild(1).GetComponent<Text>().text = songlist.info[id].sequenceId;
        obj.transform.GetChild(0).GetChild(1).GetChild(0).GetComponent<Text>().text = songlist.info[id].Title;
        //string bgPath = "https://www.maimaimfc.ink/_functions/contestEntry/" + songlist.info[id].sequenceId + "/3";
        Debug.Log(obj.transform.GetChild(0).GetChild(0).name);
        if (textureCache.ContainsKey(songlist.info[id].sequenceId))
        {
            StartCoroutine(LoadBGFromCache(songlist.info[id].sequenceId, obj.transform.GetChild(0).GetChild(0).GetComponent<RawImage>()));
        }
        else
        {
            StartCoroutine(LoadBGFromWeb(songlist.info[id].sequenceId, obj.transform.GetChild(0).GetChild(0).GetComponent<RawImage>()));
        }
        
    }
}
