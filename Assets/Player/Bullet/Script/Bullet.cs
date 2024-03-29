using Assets.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour {

    private bool lifeTimer = false;
    private float life;
    private Rigidbody rb;
    private AudioSource audioSource;
    public ParticleSystem CollitionEffect;
    public float Damage = 0f;
    public AudioClip ImpactSound;

	// Use this for initialization
	public void Start () {
        life = 30f;
        audioSource = gameObject.AddComponent<AudioSource>();
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    public void Update () {
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
            Damage += collision.relativeVelocity.magnitude;
            rb.velocity = rb.velocity / 2;
            rb.angularVelocity = rb.angularVelocity / 2;

            float size = (float)Utils.Remap(collision.relativeVelocity.magnitude, 5, 20, 0.017f, 0.046f);
            ParticleSystem effect = Instantiate(CollitionEffect, transform.position, new Quaternion());
            effect.transform.localScale = new Vector3(size, size, size);

            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
            audioSource.spatialBlend = 1;
            audioSource.volume = 0.6f;
            float volume = (float) Utils.Remap(collision.relativeVelocity.magnitude, 5, 20, 0f, 0.45f);
            audioSource.volume = volume;
            audioSource.clip = ImpactSound;
            audioSource.Play();

            if (Damage >= 120)
            {
                life = 0.3f;
            }
            if (Damage > 1000)
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
