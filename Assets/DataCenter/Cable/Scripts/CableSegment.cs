using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CableSegment : MonoBehaviour {

    private Rigidbody cableRigidbody;
    public float SleepTimer;
    public float CooldownTimer;
    public bool Sleeping;

    // Use this for initialization
    private void Start () {
        cableRigidbody = GetComponent<Rigidbody>();
        SleepTimer = 0.0f;
        CooldownTimer = 0.0f;
    }

    // Update is called once per frame
    private void Update () {
		if (cableRigidbody.velocity.magnitude > 9f)
        {
            Destroy(gameObject);
        }

        
        if (cableRigidbody.velocity.magnitude < 0.01f)
        {
            if (CooldownTimer > 0)
            {
                CooldownTimer -= Time.deltaTime;
            }
            else
            {
                CooldownTimer = 1.0f;
                SleepTimer = 0.5f;
            }
        }

        if (Sleeping)
        {
            cableRigidbody.Sleep();
        } else
        {
            cableRigidbody.WakeUp();
        }

        if (SleepTimer > 0)
        {
            SleepTimer -= Time.deltaTime;
            Sleeping = true;
        } else
        {
            Sleeping = false;
        }
        
        
    }
}
