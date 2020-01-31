using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FluorescentLamp : MonoBehaviour {

    public bool isBroken = false;
    Light _light;

    public States LampState;
    public States NextState;
    public float NextStepTimer = 0;
    public float StepTimer = 0;

    public List<MeshRenderer> LampTubes;
    public enum States {
        Normal,
        Off,
        Reignite,
        FastFlicker,
        Flicker
    }

    public AudioClip NormalSound;
    public AudioClip BrokenSound;
    AudioSource audioSource;

    public bool CustomTime;

    // Use this for initialization
    void Start () {
        CustomTime = false;
        audioSource = GetComponent<AudioSource>();
        audioSource.loop = true;
        _light = GetComponentInChildren<Light>();
        LampTubes = GetComponentsInChildren<MeshRenderer>()
            .Where(r => r.tag == "LampTube")
            .ToList();
    }
	
	// Update is called once per frame
	void Update () {
        StepTimer += Time.deltaTime;
        if (StepTimer > NextStepTimer)
        {
            StepTimer = 0;
            LampState = NextState;
            CustomTime = false;
        }

      

        if (!isBroken)
        {
            _light.intensity = 2.4f;
            if (audioSource.clip != NormalSound)
            {
                audioSource.clip = NormalSound;
                audioSource.Play();
            }
            if (_light.enabled != true)
            {
                SetTubesState(true);
                _light.enabled = true;
            }
            if (_light.color != Color.white) {
                _light.color = Color.white;
            }
            NextState = States.Normal;
            LampState = States.Normal;
        }
        else
        {
            _light.intensity = 1.5f;
            if (audioSource.clip != BrokenSound)
            {
                audioSource.clip = BrokenSound;
                audioSource.Play();
            }
            switch (LampState)
            {
                case States.Normal:
                    SetTubesState(true);
                    _light.color = Color.white;
                    _light.enabled = true;
                    if (!CustomTime)
                    {
                        NextStepTimer = Random.Range(2.1f, 4.2f);
                    }
                    NextState = States.Off;
                    break;
                case States.Off:
                    SetTubesState(false);
                    _light.color = Color.white;
                    _light.enabled = false;
                    if (!CustomTime)
                    {
                        NextStepTimer = Random.Range(1.4f, 1.7f);
                    }
                    NextState = States.Reignite;
                    break;
                case States.Reignite:
                    SetTubesState(true);
                    _light.color = Color.cyan;
                    _light.enabled = true;
                    _light.intensity = 1.2f;
                    NextStepTimer = 0.03f;
                    NextState = States.FastFlicker;
                    break;
                case States.FastFlicker:
                    _light.color = Color.white;
                    _light.intensity = 0.7f;
                    var onStage = fastFlicker();
                    _light.enabled = onStage;
                    SetTubesState(onStage);
                    if (!CustomTime)
                    {
                        NextStepTimer = Random.Range(0.3f, 2f);
                    }
                    NextState = States.Normal;
                    break;
                case States.Flicker:
                    break;
                default:
                    break;
            }
        }

    }

    void SetTubesState(bool on)
    {
        foreach (var lamp in LampTubes)
        {
            lamp.enabled = on;
        }
    }
    

    bool fastFlicker()
    {
        var f = 1000;
        var flicker = Mathf.Sin(2 * (float)System.Math.PI * Time.deltaTime * f);
        if (flicker >= 0)
        {
            return true;
        } else
        {
            return false;
        }
         
    }
}
