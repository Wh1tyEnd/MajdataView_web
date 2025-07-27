using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TouchHoldDrop : MonoBehaviour
{
    public float time;
    public float lastFor = 1f;
    public float speed = 1;
    public bool isFirework;
    public bool isEach;

    public GameObject tapEffect;
    public GameObject holdEffect;

    AudioTimeProvider timeProvider;

    GameObject firework;
    Animator fireworkEffect;

    public Sprite[] TouchHoldSprite = new Sprite[5];
    public Sprite TouchPointSprite;
    public Sprite TouchPointEachSprite;

    public GameObject[] fans;
    SpriteRenderer[] fansSprite = new SpriteRenderer[6];
    public SpriteMask mask;

    private float wholeDuration;
    private float moveDuration;
    private float displayDuration;

    public char areaPosition;
    public int startPosition;

    // Start is called before the first frame update
    void Start()
    {
        wholeDuration = 3.209385682f * Mathf.Pow(speed, -0.9549621752f);
        moveDuration = 0.8f * wholeDuration;
        displayDuration = 0.2f * wholeDuration;

        var notes = GameObject.Find("Notes").transform;
        holdEffect = Instantiate(holdEffect, notes);
        holdEffect.SetActive(false);

        timeProvider = GameObject.Find("AudioTimeProvider").GetComponent<AudioTimeProvider>();

        firework = GameObject.Find("Firework_effect");
        fireworkEffect = firework.GetComponent<Animator>();

        for (int i = 0; i < 6; i++)
        {
            fansSprite[i] = fans[i].GetComponent<SpriteRenderer>();
        }

        for (int i = 0; i < 4; i++)
        {
            fansSprite[i].sprite = TouchHoldSprite[i];
        }
        fansSprite[5].sprite = TouchHoldSprite[4];      // TouchHold Border
        if (isEach)
        {
            fansSprite[4].sprite = TouchPointEachSprite;
        }
        else { 
            fansSprite[4].sprite = TouchPointSprite; 
        }
            

        transform.position = GetAreaPos(startPosition, areaPosition);

        SetfanColor(new Color(1f, 1f, 1f, 0f));
        mask.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        var timing = timeProvider.AudioTime - time;
        //var pow = Mathf.Pow(-timing * speed, 0.1f) - 0.4f;
        var pow = -Mathf.Exp(8 * (timing * 0.4f / moveDuration) - 0.85f) + 0.42f;
        var distance = Mathf.Clamp(pow, 0f, 0.4f);

        if (timing > lastFor)
        {
            Instantiate(tapEffect, transform.position, transform.rotation);
            GameObject.Find("ObjectCounter").GetComponent<ObjectCounter>().holdCount++;
            if (isFirework)
            {
                fireworkEffect.SetTrigger("Fire");
                firework.transform.position = transform.position;
            }

            Destroy(holdEffect);
            Destroy(gameObject);
        }

        if (-timing <= wholeDuration && -timing > moveDuration)
        {
            SetfanColor(new Color(1f, 1f, 1f, Mathf.Clamp((wholeDuration + timing) / displayDuration, 0f, 1f)));
            fans[5].SetActive(false);
            mask.enabled = false;
        }
        else if (-timing < moveDuration)
        {
            fans[5].SetActive(true);
            mask.enabled = true;
            SetfanColor(Color.white);
            mask.alphaCutoff = Mathf.Clamp(0.91f * (1 - ((lastFor - timing) / lastFor)), 0f, 1f);
        }
        
        if (float.IsNaN(distance))
        {
            distance = 0f;
            
        }
        if(distance==0f)
        {
            holdEffect.SetActive(true);
            holdEffect.transform.position = transform.position;
        }
        for (int i = 0; i < 4; i++)
        {
            var pos = (0.226f + distance) * GetAngle(i);
            fans[i].transform.localPosition = pos;
        }

        

    }

    Vector3 GetAngle(int index)
    {
        var angle = (Mathf.PI / 4) + (index * (Mathf.PI / 2));
        return new Vector3(Mathf.Sin(angle), Mathf.Cos(angle));
    }

    Vector3 GetAreaPos(int index, char area)
    {
        /// <summary>
        /// AreaDistance: 
        /// C:   0
        /// E:   3.1
        /// B:   2.21
        /// A,D: 4.8
        /// </summary>
        if (area == 'C') return Vector3.zero;
        if (area == 'B')
        {
            var angle = (-index * (Mathf.PI / 4)) + ((Mathf.PI * 5) / 8);
            return new Vector3(Mathf.Cos(angle), Mathf.Sin(angle)) * 2.3f;
        }
        if (area == 'A')
        {
            var angle = (-index * (Mathf.PI / 4)) + ((Mathf.PI * 5) / 8);
            return new Vector3(Mathf.Cos(angle), Mathf.Sin(angle)) * 4.1f;
        }
        if (area == 'E')
        {
            var angle = (-index * (Mathf.PI / 4)) + ((Mathf.PI * 6) / 8);
            return new Vector3(Mathf.Cos(angle), Mathf.Sin(angle)) * 3.0f;
        }
        if (area == 'D')
        {
            var angle = (-index * (Mathf.PI / 4)) + ((Mathf.PI * 6) / 8);
            return new Vector3(Mathf.Cos(angle), Mathf.Sin(angle)) * 4.1f;
        }
        return Vector3.zero;
    }

    void SetfanColor(Color color)
    {
        foreach (var fan in fansSprite)
        {
            fan.color =color;
        }
    }
}
