using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FloppyScript : MonoBehaviour, IWeapon
{
    public List<Material> Materials;
    private Rigidbody rb;

    private Material currentMaterial;
    private bool materialSet = false;

    // Start is called before the first frame update
    void Start()
    {
        if (!materialSet)
        {
            currentMaterial = Materials.OrderBy(s => Guid.NewGuid()).FirstOrDefault();
            SetMaterial(currentMaterial);
        }
    }

    public void MakeKinematic(bool kine=true)
    {
        rb = GetComponent<Rigidbody>();
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        rb.isKinematic = kine;
    }
    public Material GetMaterial() => currentMaterial;

    public void SetMaterial(Material material) {
        List<MeshRenderer> plasitcBodys = GetComponentsInChildren<MeshRenderer>()
                .Where(s => s.name == "Body6" || s.name == "Body7")
                .ToList();
        foreach (var body in plasitcBodys)
        {
            body.material = material;
        }
        materialSet = true;
    }

    public void Fire()
    {

    }

    public void Reload()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
