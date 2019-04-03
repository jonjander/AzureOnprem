using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Assets.Tools;


public enum EarthquakeStates
{
    Off,
    Start,
    PreQuake,
    Blackout,
    Quaking,
    CoolDown,
    Restore
}

public class Earthquake : MonoBehaviour {

    public bool ManualStart = false;
    public float Magnitude;
    public AudioClip AlarmClip;
    public AudioClip QuakeClip;
    public AudioClip VoiceClip;
    public EarthquakeStates State;

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
    private float preQuake;
    private float preQuakeTimer;
    private bool preQuakeActive;
    private float quakeMaxVolume;
    private AudioSource[] audioSources;
    private bool alarmEnabled;

    // Use this for initialization
    void Start () {
        State = EarthquakeStates.Off;
        alarmEnabled = false;
        quakeMaxVolume = 1;
        preQuake = 1.2f;
        preQuakeTimer = 0;
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
        audioSources = GetComponentsInChildren<AudioSource>();
    }

    public void StartEarthQuake(float secunds)
    {
        quakeForSec = secunds;
        State = EarthquakeStates.Start;
    }
	
	// Update is called once per frame
	void Update () {

        switch (State)
        {
            case EarthquakeStates.Off:
                Magnitude = 0;
                break;
            case EarthquakeStates.Start:
                StartQuakeSound(0f);
                State = EarthquakeStates.PreQuake;
                break;
            case EarthquakeStates.PreQuake:
                PreQuake();
                Quake();
                break;
            case EarthquakeStates.Blackout:
                Blackout();
                Quake();
                State = EarthquakeStates.Quaking;
                break;
            case EarthquakeStates.Quaking:
                StartAlarmLight();
                Quake();
                Magnitude = startMagnitude;
                quakeForSec -= Time.deltaTime;
                if (quakeForSec <= 0)
                {
                    State = EarthquakeStates.CoolDown;
                    quakeForSec = 20;
                }
                break;
            case EarthquakeStates.CoolDown:
                CoolDownQuake();
                if (quakeCoolDownTimer < quakeCoolDown * 0.25f)
                {
                    RestoreLights();
                }
                if (quakeCoolDownTimer <= 0)
                {
                    quakeCoolDownTimer = quakeCoolDown;
                    State = EarthquakeStates.Restore;
                }
                break;
            case EarthquakeStates.Restore:
                StopSound();
                StopAlarmLight();
                State = EarthquakeStates.Off;
                break;
            default:
                break;
        }


        if (ManualStart)
        {
            ManualStart = false;
            StartEarthQuake(10f);
        }
	}

    private void PreQuake()
    {
        Debug.Log("PreQuake");
        preQuakeTimer += Time.deltaTime;
        if (preQuakeTimer >= preQuake)
        {
            preQuakeTimer = 0;
            State = EarthquakeStates.Blackout;
        }
        Magnitude = (float) Utils.Remap(preQuakeTimer, 0, preQuake, 0, startMagnitude);
        float vol = (float) Utils.Remap(preQuakeTimer, 0, preQuake, 0, quakeMaxVolume);
        SetQuakeVolume(vol);
    }

    private void SetQuakeVolume(float vol)
    {
        audioSources[1].volume = vol;
    }

    private void CoolDownQuake()
    {
        quakeCoolDownTimer -= Time.deltaTime;
        Magnitude = startMagnitude * (quakeCoolDownTimer / quakeCoolDown);
        SetQuakeVolume(quakeCoolDownTimer / quakeCoolDown);
    }

    private void StartAlarmLight()
    {
        if (!alarmEnabled)
        {
            alarmEnabled = true;
            SpecialLight alertLight = GetComponentInChildren<SpecialLight>();
            alertLight.Mode = SpecialLight.LightModes.Interval;
        }
    }

    private void StopAlarmLight()
    {
        alarmEnabled = false;
        SpecialLight alertLight = GetComponentInChildren<SpecialLight>();
        alertLight.Mode = SpecialLight.LightModes.Off;
    }

    private void StartQuakeSound(float quakeVolume)
    {
        //quake
        audioSources[1].clip = QuakeClip;
        audioSources[1].loop = true;
        audioSources[1].volume = quakeVolume;
        audioSources[1].spatialBlend = 0;
        audioSources[1].Play();
    }

    private void StartAlarmSound(float quakeVolume)
    {
        //Alarm
        audioSources[0].clip = AlarmClip;
        audioSources[0].loop = true;
        audioSources[0].spatialBlend = 1;
        audioSources[0].volume = 0.016f;
        audioSources[0].Play();

        //quake
        audioSources[2].clip = VoiceClip;
        audioSources[2].loop = true;
        audioSources[2].spatialBlend = 1;
        audioSources[2].Play();
    }

    private void StopSound()
    {
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

    private void Blackout()
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
        Debug.Log("wak");
        float mag = (float)Utils.Remap(Magnitude, 0, 1, 0, 0.05f);
        player.transform.rotation = Quaternion.Euler(player.transform.rotation.eulerAngles.x, player.transform.rotation.eulerAngles.y, SinusGen(80f * mag, 400f));

        Vector3 newPos = start - new Vector3(SinusGen(mag, 531f), 0, SinusGen(mag, 418f));
        rb.MovePosition(newPos);
    }
}