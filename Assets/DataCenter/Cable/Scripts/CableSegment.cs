using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CableSegment : MonoBehaviour {

    private Rigidbody rigidbody;
    public float SleepTimer;
    public float CooldownTimer;
    public bool Sleeping;

    // Use this for initialization
    void Start () {
        rigidbody = GetComponent<Rigidbody>();
        SleepTimer = 0.0f;
        CooldownTimer = 0.0f;
    }
	
	// Update is called once per frame
	void Update () {
		if (rigidbody.velocity.magnitude > 9f)
        {
            Destroy(gameObject);
        }

        
        if (rigidbody.velocity.magnitude < 0.01f)
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
            rigidbody.Sleep();
        } else
        {
            rigidbody.WakeUp();
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
