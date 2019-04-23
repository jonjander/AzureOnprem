using Assets.Azure.Resource;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

public class Resource : MonoBehaviour {

    public int USize = 2;
    public List<string> Types;
    public int Capacity = 1;
    public List<ResourceObject> RefObject = new List<ResourceObject>();
    public List<AudioClip> ServerSounds;
    public List<TextMesh> DisplayText = new List<TextMesh>();

    private AudioSource audioSource;

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

    private int GetTextLength(TextMesh textMesh)
    {
        char[] arr = textMesh.text.ToCharArray();
        Font myFont = textMesh.font;
        int totalLength = 0;
        foreach (char c in arr)
        {
            myFont.GetCharacterInfo(c, out CharacterInfo characterInfo, textMesh.fontSize);

            totalLength += characterInfo.advance;
        }
        return totalLength;
    }

    private int GetTextLength(TextMesh textMesh, string alternativeText)
    {
        char[] arr = alternativeText.ToCharArray();
        Font myFont = textMesh.font;
        int totalLength = 0;
        foreach (char c in arr)
        {
            myFont.GetCharacterInfo(c, out CharacterInfo characterInfo, textMesh.fontSize);

            totalLength += characterInfo.advance;
        }
        return totalLength;
    }


    void SetText()
    {
        try
        {
            DisplayText = GetComponentsInChildren<TextMesh>().ToList();

            for (int i = 0; i < DisplayText.Count(); i++)
            {
                if (RefObject.Count < i + 1)
                {
                    DisplayText[i].text = "";
                } else 
                {
                    
                    int baseLen = GetTextLength(DisplayText[i]);
                    int targetLen = GetTextLength(DisplayText[i], RefObject[i].Name);
                    int orgCharCount = RefObject[i].Name.Length;
                    string newText = RefObject[i].Name;
                    int newTextLen = targetLen;
                    int newTextCount = orgCharCount;
                    if (targetLen > baseLen)
                    { //Text too long

                        while (newTextLen >= baseLen)
                        {
                            newTextCount--;
                            newText = RefObject[i].Name.Substring(0, Math.Min(newTextCount, RefObject[i].Name.Length));
                            newTextLen = GetTextLength(DisplayText[i], newText);
                        }
                        DisplayText[i].text = newText;
                    }
                    else
                    {
                        DisplayText[i].text = RefObject[i].Name;
                    }
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
        UpdateIlo();
    }

    private void UpdateIlo()
    {
        List<string> iloTags = RefObject
            .Where(t=>t.Tags != null)
            .Where(t=>t.Tags.Ilo != null)
            .Select(i=>i.Tags.Ilo)
            .ToList();

        var iloState = false;

        foreach (var item in iloTags)
        {
            iloState |= (item.ToLower() == "true" || item == "1");
        }

        var iloLEDs = GetComponentsInChildren<SpecialLight>()
            .Where(n => n.Mode == SpecialLight.LightModes.Tag)
            .Where(n => n.TagName == "Ilo")
            .ToList();
        foreach (var item in iloLEDs)
        {
            item.TagValue = iloState;
        }
    }

    private void SetLeds()
    {
        try
        {
            Regex ledRegex = new Regex("LED(\\w+)\\s?\\((\\d+)\\)");
            var leds = GetComponentsInChildren<Light>()
                .Where(s => ledRegex.IsMatch(s.name));

            foreach (var item in leds)
            {
                var match = ledRegex.Match(item.name);
                int.TryParse(match.Groups[2].Value, out int ledIndex);
                if (RefObject.Count >= ledIndex + 1) {
                    //on now used untill refresh
                } else
                {
                    var script = item.GetComponent<SpecialLight>();
                    switch (match.Groups[1].Value.ToLower())
                    {
                        case "pwr":
                            script.LedColor = new Color(1, 1, 51/255f);
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
