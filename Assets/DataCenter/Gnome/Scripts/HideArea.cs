using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HideArea : MonoBehaviour {

    List<HideParent> HideParents { get; set; }

	// Use this for initialization
	void Start () {
        HideParents = new List<HideParent>();
        HideParents.AddRange(transform.parent.GetComponentsInChildren<HideParent>());
        HideParents.AddRange(transform.parent.GetComponents<HideParent>());
        foreach (var item in HideParents)
        {
            item.OnHideDestroyed += DestroyHide; 
        }
    }

    public void DestroyHide()
    {
        Destroy(gameObject);
    }

    // Update is called once per frame
    void Update () {
		
	}
}
