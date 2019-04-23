using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialLight : MonoBehaviour {

    public Light Led;
    public Color LedColor;
    public LightModes Mode;
    public float RandomMin = 0.04f;
    public float RandomMax = 0.9f;
    public float IntervalOff = 0.9f;
    public float IntervalOn = 0.02f;
    public string TagName = "";
    public bool TagValue = false;
    public SpecialLight SyncLed;

    public float Flip = 0;
    public float LedFlip = 0;
    // Use this for initialization
    void Start () {
        Led = GetComponent<Light>();
        Led.color = LedColor;
        LedFlip = UnityEngine.Random.Range(RandomMin, RandomMax);
    }
	
	// Update is called once per frame
	void Update () {
        var microRandom = UnityEngine.Random.Range(0.001f, 0.04f);
        Flip += Time.deltaTime;
        switch (Mode)
        {
            case LightModes.Static:
                Led.enabled = true;
                Led.color = LedColor;
                break;
            case LightModes.Random:
                if (Flip > LedFlip)
                {
                    Flip = 0;
                    LedFlip = UnityEngine.Random.Range(RandomMin, RandomMax);
                    Led.enabled = !Led.enabled;
                }
                break;
            case LightModes.Interval:
                if (Flip > LedFlip)
                {
                    if (Led.enabled)
                    {
                        Led.enabled = false;
                        LedFlip = IntervalOff + microRandom;
                        Flip = 0;
                    } else
                    {
                        Led.enabled = true;
                        LedFlip = IntervalOn + microRandom;
                        Flip = 0;
                    }
                }
                break;
            case LightModes.Tag:
                Led.enabled = TagValue;
                break;
            case LightModes.Sync:
                Led.enabled = SyncLed.Led.enabled;
                break;
            case LightModes.Off:
                Led.enabled = false;
                break;
            default:
                break;
        }
    }

    public enum LightModes {Static, Random, Interval, Tag, Sync, Off};
}
