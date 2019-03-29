using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Earthquake : MonoBehaviour {

    public bool activeEarthquake = false;
    public bool quaking = false;
    public float magnitude = 0.001f;
    private float startMagnitude = 0f;
    Vector3 start;
    float signal = 0f;

    Rigidbody rb;
    GameObject player;
    private bool blackout;
    private FluorescentLamp[] allStripLights;
    bool[] originalLampStates;

    public AudioClip AlarmClip;
    public AudioClip QuakeClip;
    public AudioClip VoiceClip;

    float quakeCoolDown = 25f;
    float quakeCoolDownTimer = 0f;
    float quakeForSec = 20;

    // Use this for initialization
    void Start () {
        quakeForSec = 20;
        blackout = false;
        startMagnitude = magnitude;
        quakeCoolDownTimer = quakeCoolDown;
        rb = GetComponent<Rigidbody>();
        start = transform.position;
        player = GameObject.FindGameObjectWithTag("PlayerHead");
        allStripLights = GameObject.FindObjectsOfType<FluorescentLamp>();
    }

    public void StartEarthQuake(float Secunds)
    {
        quakeForSec = Secunds;
        activeEarthquake = true;
    }
	
	// Update is called once per frame
	void Update () {
        quakeForSec -= Time.deltaTime;
        if (activeEarthquake && quakeForSec <= 0)
        {
            activeEarthquake = false;
            quakeForSec = 20;
        }


        if (activeEarthquake)
        {
            if (!quaking)
            {
                magnitude = startMagnitude;
                Blackout();
                StartSound();
                StartAlarmLight();
                quaking = true;
            } 
            Quake();
        } else
        {
            if (quaking)
            {
                quakeCoolDownTimer -= Time.deltaTime;
                coolDownQuake();
                Quake();
            }

            if (quakeCoolDownTimer < quakeCoolDown * 0.25f)
            {
                RestoreLights();
            }

            if (quakeCoolDownTimer <= 0 && quaking)
            {
                quakeCoolDownTimer = quakeCoolDown;
                
                StopSound();
                StopAlarmLight();
                quaking = false;
            }
        }
	}

    private void coolDownQuake()
    {
        magnitude = startMagnitude * (quakeCoolDownTimer / quakeCoolDown);
        var audioSources = GetComponentsInChildren<AudioSource>();
        audioSources[1].volume = (quakeCoolDownTimer / quakeCoolDown);
    }

    private void StartAlarmLight()
    {
        var AlertLight = GetComponentInChildren<SpecialLight>();
        AlertLight.Mode = SpecialLight.LightModes.Interval;
    }

    private void StopAlarmLight()
    {
        var AlertLight = GetComponentInChildren<SpecialLight>();
        AlertLight.Mode = SpecialLight.LightModes.Off;
    }

    private void StartSound()
    {
        var audioSources = GetComponentsInChildren<AudioSource>();
        //Alarm
        audioSources[0].clip = AlarmClip;
        audioSources[0].loop = true;
        audioSources[0].spatialBlend = 1;
        audioSources[0].Play();
        audioSources[0].volume = 0.12f;

        //quake
        audioSources[1].clip = QuakeClip;
        audioSources[1].loop = true;
        audioSources[1].volume = 1;
        audioSources[1].spatialBlend = 0;
        audioSources[1].Play();

        //quake
        audioSources[2].clip = VoiceClip;
        audioSources[2].loop = true;
        audioSources[2].spatialBlend = 1;
        audioSources[2].Play();
    }

    private void StopSound()
    {
        var audioSources = GetComponentsInChildren<AudioSource>();
        //Alarm
        audioSources[0].Stop();

        //quake
        audioSources[1].Stop();

        //quake
        audioSources[2].Stop();
    }

    float sinusGen(float magnitude, float frequency)
    {
        signal += Time.deltaTime;
        return magnitude * Mathf.Sin((float)System.Math.PI * signal * frequency);
    }

    void Blackout()
    {
        if (!blackout)
        {
            blackout = true;
            allStripLights = GameObject.FindObjectsOfType<FluorescentLamp>();
            originalLampStates = allStripLights.Select(l => l.isBroken).ToArray();
            for (int i = 0; i < allStripLights.Length; i++)
            {
                allStripLights[i].CustomTime = true;
                allStripLights[i].NextStepTimer = 4f;
                allStripLights[i].LampState = FluorescentLamp.States.Off;
                allStripLights[i].NextState = FluorescentLamp.States.FastFlicker;
                allStripLights[i].isBroken = true;
            }
        }
    }

    void RestoreLights()
    {
        if (blackout)
        {
            blackout = false;
            for (int i = 0; i < allStripLights.Length; i++)
            {
                allStripLights[i].isBroken = originalLampStates[i];
            }
        }
    }

    private void Quake()
    {
        var mag = (float)Assets.Tools.Utils.Remap(magnitude, 0, 1, 0, 0.05f);
        player.transform.rotation = Quaternion.Euler(player.transform.rotation.eulerAngles.x, player.transform.rotation.eulerAngles.y, sinusGen(80f * mag, 400f));

        var newPos = start - new Vector3(sinusGen(mag, 531f), 0, sinusGen(mag, 418f));
        rb.MovePosition(newPos);
    }
}