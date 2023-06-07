using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Video;
using System.IO;
using System.Drawing;
using UnityEngine.UI;

public class BGManager : MonoBehaviour
{
    SpriteRenderer spriteRender;
    AudioTimeProvider provider;
    float playSpeed;

    void Start()
    {
        spriteRender = GetComponent<SpriteRenderer>();
        provider = GameObject.Find("AudioTimeProvider").GetComponent<AudioTimeProvider>();
    }
    public class audioUrl
    {
        public string downloadUrl;
    }
    public IEnumerator LoadBGFromWeb(string path, Action callback)
    {
        if (path == string.Empty) {Debug.LogError("empty bg path!"); yield break;}
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

            spriteRender.sprite = sprite;
            var scale = 1080f / (float)sprite.texture.width;
            gameObject.transform.localScale = new Vector3(scale, scale, scale);
            callback.Invoke();
        }
            
    }
}
