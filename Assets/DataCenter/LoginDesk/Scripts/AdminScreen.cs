using Assets.Azure.Subscription;
using Assets.Scripts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AdminScreen : MonoBehaviour {

    TextMesh ComputerOutputText;
    List<Subscription> AvalibleSubscriptions;
    string selectedSubscriptionId;
    private bool loggedIn;

    public bool IsCrashed { get; private set; }

    public delegate void LoginAction(string selectedSubscriptionId);
    public static event LoginAction OnComputerLogin;

    float promptBlinker = 0;

    // Use this for initialization
    void Start () {
        ComputerOutputText = GetComponentInChildren<TextMesh>();
        ComputerOutputText.text = ">_";
        AzureManagementAPIHelper.OnSubscriptionLoaded += DisplaySubscriptions;
        PlayerScript.OnComputerScreenInput += KeyInput;
    }

    void DisplaySubscriptions(List<Subscription> userSubscriptions)
    {
        selectedSubscriptionId = userSubscriptions.FirstOrDefault().subscriptionId;
        AvalibleSubscriptions = userSubscriptions;
        loggedIn = true;
    }

    void KeyInput(KeyCode key)
    {
        var selectedSubscription = AvalibleSubscriptions
                .Where(o => o.subscriptionId == selectedSubscriptionId)
                .FirstOrDefault();
        int index = 0;
        if (key == KeyCode.Y)
        {
            index = AvalibleSubscriptions.IndexOf(selectedSubscription) - 1;
            index = LoopIndex(index);
            selectedSubscriptionId = AvalibleSubscriptions[index].subscriptionId;

        }
        else if (key == KeyCode.H)
        {
            index = AvalibleSubscriptions.IndexOf(selectedSubscription) + 1;
            index = LoopIndex(index);
            selectedSubscriptionId = AvalibleSubscriptions[index].subscriptionId;
        }
        else if (key == KeyCode.Return)
        {
            OnComputerLogin(selectedSubscriptionId);
            loggedIn = false;
            IsCrashed = true;
        }
    }

    private int LoopIndex(int index)
    {
        if (index < 0)
        {
            index = AvalibleSubscriptions.Count() - 1;
        }
        else if (index > AvalibleSubscriptions.Count() - 1)
        {
            index = 0;
        }

        return index;
    }


    // Update is called once per frame
    void Update () {
        if (loggedIn)
        {
            var SubscriptionOutput = AvalibleSubscriptions.Select(s =>
            {
                var shortDisplayName = s.displayName.Substring(0, Math.Min(24, s.displayName.Length));
                if (s.subscriptionId == selectedSubscriptionId)
                {
                    return shortDisplayName + " <-- (Y = Up, H = Down)";
                }
                else
                {
                    return shortDisplayName;
                }
            });
            ComputerOutputText.text = string.Join(Environment.NewLine, SubscriptionOutput);
            
        } else if (IsCrashed)
        {
            ComputerOutputText.text = ":(" + Environment.NewLine
                + "0x00000FF2";
        } else
        {
            promptBlinker += Time.deltaTime;
            if (promptBlinker > 0.5f)
            {
                promptBlinker = 0;
                if (ComputerOutputText.text == ">_") {
                    ComputerOutputText.text = ">";
                } else if (ComputerOutputText.text == ">")
                {
                    ComputerOutputText.text = ">_";
                }
            }
            
        }
    }
}
