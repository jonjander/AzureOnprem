using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CableSegment : MonoBehaviour {

    Rigidbody Rigidbody;
    public float sleepTimer;
    public float cooldownTimer;
    public bool sleeping;

    // Use this for initialization
    void Start () {
        Rigidbody = GetComponent<Rigidbody>();
        sleepTimer = 0.0f;
        cooldownTimer = 0.0f;
    }
	
	// Update is called once per frame
	void Update () {
		if (Rigidbody.velocity.magnitude > 7f)
        {
            Destroy(gameObject);
        }

        
        if (Rigidbody.velocity.magnitude < 0.01f)
        {
            if (cooldownTimer > 0)
            {
                cooldownTimer -= Time.deltaTime;
            }
            else
            {
                cooldownTimer = 1.0f;
                sleepTimer = 0.5f;
            }
        }

        if (sleeping)
        {
            Rigidbody.Sleep();
        } else
        {
            Rigidbody.WakeUp();
        }

        if (sleepTimer > 0)
        {
            sleepTimer -= Time.deltaTime;
            sleeping = true;
        } else
        {
            sleeping = false;
        }
        
        
    }
}
