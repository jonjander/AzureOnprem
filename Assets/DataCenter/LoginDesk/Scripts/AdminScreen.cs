using Assets.Azure.Subscription;
using Assets.Scripts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AdminScreen : MonoBehaviour {
    public delegate void LoginAction(string selectedSubscriptionId);
    public static event LoginAction OnComputerLogin;

    public bool IsCrashed;
 
    private bool loggedIn;
    private float promptBlinker = 0;
    private TextMesh computerOutputText;
    private List<Subscription> avalibleSubscriptions;
    private string selectedSubscriptionId;
    // Use this for initialization
    void Start () {
        computerOutputText = GetComponentInChildren<TextMesh>();
        computerOutputText.text = ">_";
        AzureManagementAPIHelper.OnSubscriptionLoaded += DisplaySubscriptions;
        PlayerScript.OnComputerScreenInput += KeyInput;
    }

    void DisplaySubscriptions(List<Subscription> userSubscriptions)
    {
        selectedSubscriptionId = userSubscriptions.FirstOrDefault().SubscriptionId;
        avalibleSubscriptions = userSubscriptions;
        loggedIn = true;
    }

    void KeyInput(KeyCode key)
    {
        var selectedSubscription = avalibleSubscriptions
                .Where(o => o.SubscriptionId == selectedSubscriptionId)
                .FirstOrDefault();
        int index = 0;
        if (key == KeyCode.Y)
        {
            index = avalibleSubscriptions.IndexOf(selectedSubscription) - 1;
            index = LoopIndex(index);
            selectedSubscriptionId = avalibleSubscriptions[index].SubscriptionId;

        }
        else if (key == KeyCode.H)
        {
            index = avalibleSubscriptions.IndexOf(selectedSubscription) + 1;
            index = LoopIndex(index);
            selectedSubscriptionId = avalibleSubscriptions[index].SubscriptionId;
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
            index = avalibleSubscriptions.Count() - 1;
        }
        else if (index > avalibleSubscriptions.Count() - 1)
        {
            index = 0;
        }

        return index;
    }


    // Update is called once per frame
    void Update () {
        if (loggedIn)
        {
            var subscriptionOutput = avalibleSubscriptions.Select(s =>
            {
                var shortDisplayName = s.DisplayName.Substring(0, Math.Min(24, s.DisplayName.Length));
                if (s.SubscriptionId == selectedSubscriptionId)
                {
                    return shortDisplayName + " <-- (Y = Up, H = Down)";
                }
                else
                {
                    return shortDisplayName;
                }
            });
            computerOutputText.text = string.Join(Environment.NewLine, subscriptionOutput);
            
        } else if (IsCrashed)
        {
            computerOutputText.text = ":(" + Environment.NewLine
                + "0x00000FF2";
        } else
        {
            promptBlinker += Time.deltaTime;
            if (promptBlinker > 0.5f)
            {
                promptBlinker = 0;
                if (computerOutputText.text.Substring(1,1) == "_") {
                    computerOutputText.text = "Press E to login" + Environment.NewLine;
                    computerOutputText.text += ">";
                } else
                {
                    computerOutputText.text = "Press E to login" + Environment.NewLine;
                    computerOutputText.text += ">_";
                }
            }
            
        }
    }
}
