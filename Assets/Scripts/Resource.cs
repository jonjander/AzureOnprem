using Assets.Azure.Resource;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

public class Resource : MonoBehaviour {

    public int uSize = 2;
    public List<string> Types;
    public int Capacity = 1;
    public List<ResourceObject> refObject = new List<ResourceObject>();
    MeshRenderer MR;

    public List<TextMesh> DisplayText = new List<TextMesh>();
    private AudioSource audioSource;

    public List<AudioClip> ServerSounds;
    
    // Use this for initialization
    void Start () {
        DisplayText = GetComponentsInChildren<TextMesh>().ToList();
        var randomSound = ServerSounds
            .OrderBy(g => Guid.NewGuid())
            .FirstOrDefault();
        audioSource = GetComponentInChildren<AudioSource>();
        audioSource.clip = randomSound;

        audioSource.Play();
    }

    private void OnJointBreak(float breakForce)
    {
        var joints = GetComponents<FixedJoint>();
        if (joints.Count() == 1)
        {
            audioSource.loop = false;
            audioSource.Stop();

            var leds = GetComponentsInChildren<Light>();
            foreach (var led in leds)
            {
                Destroy(led.transform.gameObject);
            }
        }
    }

    void SetText()
    {
        try
        {
            DisplayText = GetComponentsInChildren<TextMesh>().ToList();
            for (int i = 0; i < DisplayText.Count(); i++)
            {
                if (refObject.Count < i + 1)
                {
                    DisplayText[i].text = "";
                } else 
                {
                    var maxLenFromExistingText = DisplayText[i].text.Length;
                    DisplayText[i].text = refObject[i].name.Substring(0, Math.Min(maxLenFromExistingText, refObject[i].name.Length));
                }
            }
        }
        catch {
            Debug.LogWarning("Name overflow");
        }
    }

    public void Load()
    {
        SetText();
        SetLeds();
    }

    private void SetLeds()
    {
        try
        {
            Regex ledRegex = new Regex("LED(\\w+)\\s?\\((\\d+)\\)");
            var Leds = GetComponentsInChildren<Light>()
                .Where(s => ledRegex.IsMatch(s.name));

            foreach (var item in Leds)
            {
                var match = ledRegex.Match(item.name);
                int ledIndex;
                int.TryParse(match.Groups[2].Value, out ledIndex);
                if (refObject.Count >= ledIndex + 1) {
                    //on now used untill refresh
                } else
                {
                    var script = item.GetComponent<SpecialLight>();
                    switch (match.Groups[1].Value.ToLower())
                    {
                        case "pwr":
                            script.ledColor = new Color(1, 1, 51/255f);
                            break;
                        case "hdd":
                            script.enabled = false;
                            item.enabled = false;
                            break;
                    }
                }
            }
        }
        catch {

        }
    }

    // Update is called once per frame
    void Update () {
        
    }
}
