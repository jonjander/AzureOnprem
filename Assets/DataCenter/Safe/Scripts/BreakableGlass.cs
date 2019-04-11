using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakableGlass : MonoBehaviour
{

    public delegate void BreakableGlassAction();
    public event BreakableGlassAction OnGlassBreak;
    public float life;

    private void OnCollisionEnter(Collision collision)
    {
        life -= collision.relativeVelocity.magnitude;
        Debug.Log("life " + life);
        if (life <= 0)
        {
            OnGlassBreak();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        life = 200f;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
