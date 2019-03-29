using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialLight : MonoBehaviour {

    public Light led;
    public Color ledColor;
    public LightModes Mode;

    public float RandomMin = 0.04f;
    public float RandomMax = 0.9f;

    public float IntervalOff = 0.9f;
    public float IntervalOn = 0.02f;

    float flip = 0;
    float ledFlip = 0;
    public string TagName = "";
    public bool TagValue = false;

    public SpecialLight SyncLed;

    // Use this for initialization
    void Start () {
        led = GetComponent<Light>();
        led.color = ledColor;
        ledFlip = UnityEngine.Random.Range(RandomMin, RandomMax);
        
    }
	
	// Update is called once per frame
	void Update () {
        var microRandom = UnityEngine.Random.Range(0.001f, 0.04f);
        flip += Time.deltaTime;
        switch (Mode)
        {
            case LightModes.Static:
                led.enabled = true;
                led.color = ledColor;
                break;
            case LightModes.Random:
                if (flip > ledFlip)
                {
                    flip = 0;
                    ledFlip = UnityEngine.Random.Range(RandomMin, RandomMax);
                    led.enabled = !led.enabled;
                }
                break;
            case LightModes.Interval:
                if (flip > ledFlip)
                {
                    if (led.enabled)
                    {
                        led.enabled = false;
                        ledFlip = IntervalOff + microRandom;
                        flip = 0;
                    } else
                    {
                        led.enabled = true;
                        ledFlip = IntervalOn + microRandom;
                        flip = 0;
                    }
                }
                break;
            case LightModes.Tag:
                led.enabled = TagValue;
                break;
            case LightModes.Sync:
                led.enabled = SyncLed.led.enabled;
                break;
            case LightModes.Off:
                led.enabled = false;
                break;
            default:
                break;
        }
    }

    public enum LightModes {Static, Random, Interval, Tag, Sync, Off};
}
