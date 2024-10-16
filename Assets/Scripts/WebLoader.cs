using static System.Net.WebRequestMethods;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;

namespace API
{
    class WebLoader : MonoBehaviour
    {        
        //For inner main viewer
        public static IEnumerator LoadBGFromWeb(string path, Action callback)
        {
            if (path == string.Empty) { Debug.LogError("empty bg path!"); yield break; }
            UnityWebRequest bgreq = UnityWebRequest.Get(path);
            bgreq.downloadHandler = new DownloadHandlerTexture();
            yield return bgreq.SendWebRequest();
            if (bgreq.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error downloading bg: " + bgreq.error + bgreq.downloadHandler.error);
                callback.Invoke();
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

    }


}
