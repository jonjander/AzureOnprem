using Assets.Azure.Subscription;
using Assets.Scripts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public enum AdminScreenMenu
{
    LoginPrompt,
    Loggingin,
    Loggedin,
    Error
}

public class AdminScreen : MonoBehaviour {
    public delegate void LoginAction(string selectedSubscriptionId);
    public static event LoginAction OnComputerLogin;
    public AdminScreenMenu CurrentMenu;

    private float promptBlinker = 0;
    private TextMesh computerOutputText;
    private List<Subscription> avalibleSubscriptions;
    private string selectedSubscriptionId;
    private bool cursorFlip;
    

    // Use this for initialization
    void Start () {
        CurrentMenu = AdminScreenMenu.LoginPrompt;
        computerOutputText = GetComponentInChildren<TextMesh>();
        computerOutputText.text = ">_";
        AzureManagementAPIHelper.OnSubscriptionLoaded += DisplaySubscriptions;
        PlayerScript.OnComputerScreenInput += KeyInput;
    }

    private void DisplaySubscriptions(List<Subscription> userSubscriptions)
    {
        selectedSubscriptionId = userSubscriptions.FirstOrDefault().SubscriptionId;
        avalibleSubscriptions = userSubscriptions;
        CurrentMenu = AdminScreenMenu.Loggedin;
    }

    private void KeyInput(KeyCode key)
    {
        int index = 0;
        if (key == KeyCode.Y && CurrentMenu == AdminScreenMenu.Loggedin)
        {
            index = avalibleSubscriptions.IndexOf(GetSelectedSubscription()) - 1;
            index = LoopIndex(index);
            selectedSubscriptionId = avalibleSubscriptions[index].SubscriptionId;

        }
        else if (key == KeyCode.H && CurrentMenu == AdminScreenMenu.Loggedin)
        {
            index = avalibleSubscriptions.IndexOf(GetSelectedSubscription()) + 1;
            index = LoopIndex(index);
            selectedSubscriptionId = avalibleSubscriptions[index].SubscriptionId;
        }
        else if (key == KeyCode.Return && CurrentMenu == AdminScreenMenu.Loggedin)
        {
            OnComputerLogin(selectedSubscriptionId);
            CurrentMenu = AdminScreenMenu.Error;
        }
        else if (key == KeyCode.E && CurrentMenu == AdminScreenMenu.LoginPrompt)
        {
            CurrentMenu = AdminScreenMenu.Loggingin;
        }
    }

    private Subscription GetSelectedSubscription()
    {
        return avalibleSubscriptions
                        .Where(o => o.SubscriptionId == selectedSubscriptionId)
                        .FirstOrDefault();
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

        switch (CurrentMenu)
        {
            case AdminScreenMenu.LoginPrompt:
                promptBlinker += Time.deltaTime;
                if (promptBlinker > 0.5f)
                {
                    promptBlinker = 0;
                    if (cursorFlip)
                    {
                        computerOutputText.text = "Press E to login" + Environment.NewLine;
                        computerOutputText.text += ">";
                    }
                    else
                    {
                        computerOutputText.text = "Press E to login" + Environment.NewLine;
                        computerOutputText.text += ">_";
                    }
                    cursorFlip = !cursorFlip;
                }
                break;
            case AdminScreenMenu.Loggingin:
                computerOutputText.text = ">_ E";
                break;
            case AdminScreenMenu.Loggedin:
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
                break;
            case AdminScreenMenu.Error:
                computerOutputText.text = ":(" + Environment.NewLine
                + "0x00000FF2";
                break;
            default:
                break;
        }

    }
}
