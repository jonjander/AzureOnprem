using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : MonoBehaviour {

    //public GameObject Projectile;
    public AudioClip UseSoundHit;
    public AudioClip UseSoundMiss;
    private AudioSource audioSource;
    public DCGenerator GenerationScript;
    public Weapon CurrentWeapon;
    
    public delegate void ComputerScreenInput(KeyCode key);
    public static event ComputerScreenInput OnComputerScreenInput;
    
    // Use this for initialization
    void Start () {
        CurrentWeapon = Weapons.FloppyDisk();
        audioSource = GetComponent<AudioSource>();
	}

    IEnumerator Login()
    {
        yield return new WaitForSeconds(UseSoundHit.length);
        GenerationScript.doLogin();
    }

    // Update is called once per frame
    void Update () {

        if (Input.GetMouseButton(0))
        {
            bool shoot = false;
            if (CurrentWeapon.fireMode == FireMode.FullAuto)
            {
                CurrentWeapon.feedingTimer += Time.deltaTime;
                if (CurrentWeapon.feedingTimer >= CurrentWeapon.rateOfFire)
                {
                    //Fire
                    CurrentWeapon.feeding = false;
                    CurrentWeapon.feedingTimer = 0;
                    shoot = true;
                } else
                {
                    CurrentWeapon.feeding = true;
                }
            } else if (CurrentWeapon.fireMode == FireMode.SingleAction && CurrentWeapon.feeding) {
                CurrentWeapon.feeding = false;
                shoot = true;
            }

            if (shoot) { 
                var bullet = Instantiate(CurrentWeapon.projectilePrefab);

                bullet.transform.position = Camera.main.transform.position - new Vector3(0, 0.2f, 0);
                if (UnityEngine.Random.Range(0, 2) == 1) //randomly flip projectile
                {
                    bullet.transform.rotation = Quaternion.Euler(180, 0, 0);
                }
                var rg = bullet.GetComponent<Rigidbody>();
                rg.mass = CurrentWeapon.projectileMass;
                Vector3 pushDir = Camera.main.transform.forward;

                rg.maxAngularVelocity = 180;
                rg.AddForce(pushDir.normalized * rg.mass * CurrentWeapon.weaponPower, ForceMode.Impulse);
                rg.AddTorque(CurrentWeapon.rotationVector, ForceMode.Impulse);
            }
        } else
        {
            CurrentWeapon.feeding = true;
        }
        
        // Does the ray intersect any objects excluding the player layer

        if (Input.GetKeyDown("e") && IsNearComputerScreen())
        {
            StartCoroutine(Login());
        }
        else if (Input.GetKeyDown(KeyCode.Y) && IsNearComputerScreen())
        {
            OnComputerScreenInput(KeyCode.Y);
        }
        else if (Input.GetKeyDown(KeyCode.H) && IsNearComputerScreen())
        {
            OnComputerScreenInput(KeyCode.H);
        }
        else if (Input.GetKeyDown(KeyCode.Return) && IsNearComputerScreen())
        {
            OnComputerScreenInput(KeyCode.Return);
        }
        else if (Input.GetKeyDown("1"))
        {
            CurrentWeapon = Weapons.FloppyDiskAuto();
        }
        else if (Input.GetKeyDown("2"))
        {
            CurrentWeapon = Weapons.FloppyDisk();
        }
        else if (Input.GetKeyDown("3"))
        {
            CurrentWeapon = Weapons.FloppyDiskAuto();
            CurrentWeapon.rateOfFire = 0.1f;
        }





    }

    private bool IsNearComputerScreen()
    {
        int layerMask = 1 << LayerMask.NameToLayer("Login");
        RaycastHit hit;
        var RayIsHit = Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, Mathf.Infinity, layerMask);
        
        var Result = RayIsHit && hit.distance < 1.6f;
        if (!Result)
        {
            audioSource.clip = UseSoundMiss;
            audioSource.Play();
        } else
        {
            audioSource.clip = UseSoundHit;
            audioSource.Play();
        }
        return Result;
    }
}
