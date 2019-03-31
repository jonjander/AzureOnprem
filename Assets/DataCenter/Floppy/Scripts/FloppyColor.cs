using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FloppyColor : MonoBehaviour
{
    public List<Material> Materials;

    // Start is called before the first frame update
    void Start()
    {
        List<MeshRenderer> plasitcBodys = GetComponentsInChildren<MeshRenderer>()
            .Where(s => s.name == "Body6" || s.name == "Body7")
            .ToList();

        var randomMaterial = Materials.OrderBy(s => Guid.NewGuid()).FirstOrDefault();
        foreach (var body in plasitcBodys)
        {
            body.material = randomMaterial;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
