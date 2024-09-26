using static System.Net.WebRequestMethods;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Newtonsoft.Json;
using JsonFormat;
using TMPro;

namespace API
{
    class ApiAccess
    {
        public static string ROOT = "http://101.132.193.53:5001/api/";
        public const string majROOT = "http://101.132.193.53:5002/api/";
        public const string mmfcROOT = "http://101.132.193.53:5001/api/";

        public static void changeROOT()
        {
            if (majROOT.Equals(ROOT))
            {
                ROOT = mmfcROOT;
            }else if (mmfcROOT.Equals(ROOT))
            {
                ROOT = majROOT;
            }
        }
    }


    class WebLoader : MonoBehaviour
    {        
        static string SongListApiPath = ApiAccess.ROOT + "SongList";
        static string BGApiPath = ApiAccess.ROOT + "Image/{0}";
       

        public static void updateApi()
        {
            SongListApiPath = ApiAccess.ROOT + "SongList";
            BGApiPath = ApiAccess.ROOT + "Image/{0}";
            
        }
        public static IEnumerator getSongList(string path, Action initShowList)
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
                SongInformation.songlist = JsonConvert.DeserializeObject<List<SongDetail>>(www.downloadHandler.text);
                Debug.Log($"Downloaded the songlist with {SongInformation.songlist.Count} songs");
                SongInformation.sortSonglist();
                initShowList.Invoke();
            }
            
        }



        public static IEnumerator LoadBGFromWeb(string id, RawImage rawImage)
        {
            string path = String.Format(BGApiPath, id);
            if (path == string.Empty) { Debug.LogError("empty bg path!"); yield break; }
            Debug.Log("Downloading bg from " + path);
            UnityWebRequest bgreq = UnityWebRequest.Get(path);
            bgreq.downloadHandler = new DownloadHandlerTexture();
            yield return bgreq.SendWebRequest();
            if (bgreq.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error downloading bg: " + bgreq.error);
            }
            else
            {
                var texture = DownloadHandlerTexture.GetContent(bgreq);
                Sprite sprite;
                sprite = Sprite.Create(
                    texture,
                    new Rect(0.0f, 0.0f, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f));

                rawImage.texture = texture;
                var scale = 250f / (float)sprite.texture.width;
                rawImage.gameObject.transform.localScale = new Vector3(scale, scale, scale);
                //textureCache.Add(id, texture);
            }

        }

        //For inner main viewer
        public static IEnumerator LoadBGFromWeb(string path, Action callback)
        {
            if (path == string.Empty) { Debug.LogError("empty bg path!"); yield break; }
            UnityWebRequest bgreq = UnityWebRequest.Get(path);
            bgreq.downloadHandler = new DownloadHandlerTexture();
            yield return bgreq.SendWebRequest();
            if (bgreq.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error downloading bg: " + bgreq.error);
            }
            else
            {
                var texture = DownloadHandlerTexture.GetContent(bgreq);
                var sprite = Sprite.Create(
                    texture,
                    new Rect(0.0f, 0.0f, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f));

                BGManager.spriteRender.sprite = sprite;
                var scale = 1080f / (float)sprite.texture.width;
                BGManager.spriteRender.transform.localScale = new Vector3(scale, scale, scale);
                callback.Invoke();
            }

        }




        /*public static IEnumerator LoadBGFromCache(string id, RawImage rawImage)
        {
            Texture2D texture = textureCache[id];
            Sprite sprite;
            sprite = Sprite.Create(
                texture,
                new Rect(0.0f, 0.0f, texture.width, texture.height),
                new Vector2(0.5f, 0.5f));

            rawImage.texture = texture;
            //spriteRender.sprite = sprite;
            var scale = 250f / (float)sprite.texture.width;
            rawImage.gameObject.transform.localScale = new Vector3(scale, scale, scale);
            yield return null;
        }*/


    }


}
