using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideParent : MonoBehaviour {


    public delegate void HideDestryedAction();
    public event HideDestryedAction OnHideDestroyed;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.tag == "Bullet")
        {
            try
            {
                //RemoveHide
                OnHideDestroyed();
            }
            catch { }
        }
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
