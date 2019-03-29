using Assets.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour {

    bool lifeTimer = false;
    float life;
    public ParticleSystem CollitionEffect;
    public float damage = 0f;

	// Use this for initialization
	void Start () {
        life = 20f;

    }
	
	// Update is called once per frame
	void Update () {
		if (lifeTimer)
        {
            life -= Time.deltaTime;
            if (life < 0)
            {
                
                Destroy(transform.gameObject);
            }
        }
	}

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.relativeVelocity.magnitude > 5f)
        {
            damage += collision.relativeVelocity.magnitude;

            var Size = (float)Utils.Remap(collision.relativeVelocity.magnitude, 5, 20, 0.017f, 0.046f);
            var effect = Instantiate(CollitionEffect, transform.position, new Quaternion());
            effect.transform.localScale = new Vector3(Size, Size, Size);
            if (damage > 80f)
            {
                Destroy(transform.gameObject);
            }
        }

        //Donot destroy on layers
        var layerMask = LayerMask.GetMask(
            new string[3]{
                "Floor",
                "Roof",
                "Player"
        });

        if ((1 << collision.gameObject.layer & layerMask) == 0)
        {
            lifeTimer = true;
        }

        
    }
}
