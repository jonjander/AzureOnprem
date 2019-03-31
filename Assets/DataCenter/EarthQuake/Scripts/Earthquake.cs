using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Earthquake : MonoBehaviour {

    public bool ActiveEarthquake = false;
    public bool Quaking = false;
    public float Magnitude;
    public AudioClip AlarmClip;
    public AudioClip QuakeClip;
    public AudioClip VoiceClip;

    private float startMagnitude = 0f;
    private Vector3 start;
    private float signal = 0f;
    private Rigidbody rb;
    private GameObject player;
    private bool blackout;
    private FluorescentLamp[] allStripLights;
    private bool[] originalLampStates;
    private float quakeCoolDown;
    private float quakeCoolDownTimer;
    private float quakeForSec;

    // Use this for initialization
    void Start () {
        quakeCoolDown = 25f;
        quakeCoolDownTimer = 0f;
        quakeForSec = 20;
        Magnitude = 0.3f;
        quakeForSec = 20;
        blackout = false;
        startMagnitude = Magnitude;
        quakeCoolDownTimer = quakeCoolDown;
        rb = GetComponent<Rigidbody>();
        start = transform.position;
        player = GameObject.FindGameObjectWithTag("PlayerHead");
        allStripLights = GameObject.FindObjectsOfType<FluorescentLamp>();
    }

    public void StartEarthQuake(float secunds)
    {
        quakeForSec = secunds;
        ActiveEarthquake = true;
    }
	
	// Update is called once per frame
	void Update () {
        quakeForSec -= Time.deltaTime;
        if (ActiveEarthquake && quakeForSec <= 0)
        {
            ActiveEarthquake = false;
            quakeForSec = 20;
        }

        if (ActiveEarthquake)
        {
            if (!Quaking)
            {
                Magnitude = startMagnitude;
                Blackout();
                StartSound();
                StartAlarmLight();
                Quaking = true;
            } 
            Quake();
        } else
        {
            if (Quaking)
            {
                quakeCoolDownTimer -= Time.deltaTime;
                CoolDownQuake();
                Quake();
            }

            if (quakeCoolDownTimer < quakeCoolDown * 0.25f)
            {
                RestoreLights();
            }

            if (quakeCoolDownTimer <= 0 && Quaking)
            {
                quakeCoolDownTimer = quakeCoolDown;
                
                StopSound();
                StopAlarmLight();
                Quaking = false;
            }
        }
	}

    private void CoolDownQuake()
    {
        Magnitude = startMagnitude * (quakeCoolDownTimer / quakeCoolDown);
        AudioSource[] audioSources = GetComponentsInChildren<AudioSource>();
        audioSources[1].volume = (quakeCoolDownTimer / quakeCoolDown);
    }

    private void StartAlarmLight()
    {
        SpecialLight alertLight = GetComponentInChildren<SpecialLight>();
        alertLight.Mode = SpecialLight.LightModes.Interval;
    }

    private void StopAlarmLight()
    {
        SpecialLight alertLight = GetComponentInChildren<SpecialLight>();
        alertLight.Mode = SpecialLight.LightModes.Off;
    }

    private void StartSound()
    {
        AudioSource[] audioSources = GetComponentsInChildren<AudioSource>();
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
        AudioSource[] audioSources = GetComponentsInChildren<AudioSource>();
        //Alarm
        audioSources[0].Stop();

        //quake
        audioSources[1].Stop();

        //quake
        audioSources[2].Stop();
    }

    float SinusGen(float magnitude, float frequency)
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
        float mag = (float)Assets.Tools.Utils.Remap(Magnitude, 0, 1, 0, 0.05f);
        player.transform.rotation = Quaternion.Euler(player.transform.rotation.eulerAngles.x, player.transform.rotation.eulerAngles.y, SinusGen(80f * mag, 400f));

        Vector3 newPos = start - new Vector3(SinusGen(mag, 531f), 0, SinusGen(mag, 418f));
        rb.MovePosition(newPos);
    }
}