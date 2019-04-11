using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Safe : MonoBehaviour
{

    private List<Transform> brokenGlass;
    private Transform glass;
    

    // Start is called before the first frame update
    void Start()
    {
        brokenGlass = GetComponentsInChildren<Transform>(true)
            .Where(t => t.tag == "BrokenGlass")
            .ToList();

        glass = GetComponentsInChildren<Transform>()
            .Where(t => t.tag == "Glass")
            .FirstOrDefault();

        glass.gameObject.GetComponent<BreakableGlass>().OnGlassBreak += BreakGlass;

    }


    // Update is called once per frame
    void Update()
    {
        
    }

    private void BreakGlass()
    {
        glass.gameObject.SetActive(false);
        foreach (var gp in brokenGlass)
        {
            gp.gameObject.SetActive(true);
        }

    }

}
