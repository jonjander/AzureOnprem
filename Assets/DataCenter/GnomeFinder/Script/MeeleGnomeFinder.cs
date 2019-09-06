using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeeleGnomeFinder : MonoBehaviour, IWeapon
{

    private Rigidbody rb;

    public void Fire()
    {

    }

    public Material GetMaterial()
    {
        throw new NotImplementedException();
    }

    public bool IsLocked()
    {
        throw new NotImplementedException();
    }

    public void MakeKinematic(bool kine = true)
    {
        rb = GetComponent<Rigidbody>();
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        rb.isKinematic = kine;
    }

    public void Reload()
    {
        
    }

    public void SetLock(bool enabled)
    {
        throw new NotImplementedException();
    }

    public void SetMaterial(Material material)
    {
        throw new NotImplementedException();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
