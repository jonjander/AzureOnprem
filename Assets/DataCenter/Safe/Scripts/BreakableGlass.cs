using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakableGlass : MonoBehaviour
{

    public delegate void BreakableGlassAction();
    public event BreakableGlassAction OnGlassBreak;
    public float Life;

    private void OnCollisionEnter(Collision collision)
    {
        Life -= collision.relativeVelocity.magnitude;
        Debug.Log("life " + Life);
        if (Life <= 0)
        {
            OnGlassBreak();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        Life = 3000f;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
