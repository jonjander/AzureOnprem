using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShellScript : MonoBehaviour
{
    private float lifeTime;
    private Rigidbody rb;
    // Start is called before the first frame update
    void Start()
    {
        lifeTime = 20f;
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!rb.isKinematic)
        {
            lifeTime -= Time.deltaTime;
            if (lifeTime <= 0)
            {
                Destroy(transform.gameObject);
            }
        }
        
    }
}
