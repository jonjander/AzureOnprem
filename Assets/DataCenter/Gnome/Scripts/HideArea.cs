using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HideArea : MonoBehaviour {

    List<HideParent> hideParents { get; set; }

	// Use this for initialization
	void Start () {
        hideParents = new List<HideParent>();
        hideParents.AddRange(transform.parent.GetComponentsInChildren<HideParent>());
        hideParents.AddRange(transform.parent.GetComponents<HideParent>());
        foreach (var item in hideParents)
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
