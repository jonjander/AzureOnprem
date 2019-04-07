using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : MonoBehaviour {

    //public GameObject Projectile;
    public AudioClip UseSoundHit;
    public AudioClip UseSoundMiss;
    public DCGenerator GenerationScript;
    public delegate void ComputerScreenInput(KeyCode key);
    public static event ComputerScreenInput OnComputerScreenInput;

    private AudioSource audioSource;
    private GameObject currentWeaponGameObject;
    private Weapon currentWeapon;
    private IWeapon currentWeaponScript;

    public Weapon CurrentWeapon
    {
        get => currentWeapon;
        set
        {
            currentWeapon = value;
            ChangeWeapon();
        }
    }

    // Use this for initialization
    void Start () {
        CurrentWeapon = Weapons.Shotgun();
        audioSource = GetComponent<AudioSource>();
	}

    private IEnumerator Login()
    {
        yield return new WaitForSeconds(UseSoundHit.length);
        GenerationScript.DoLogin();
    }

    // Update is called once per frame
    void Update () {

        if (Input.GetMouseButton(0))
        {
            bool shoot = false;
            if (CurrentWeapon.FireMode == FireMode.FullAuto)
            {
                CurrentWeapon.FeedingTimer += Time.deltaTime;
                if (CurrentWeapon.FeedingTimer >= CurrentWeapon.RateOfFire)
                {
                    //Fire
                    CurrentWeapon.Feeding = false;
                    CurrentWeapon.FeedingTimer = 0;
                    shoot = true;
                } else
                {
                    CurrentWeapon.Feeding = true;
                }
            } else if (CurrentWeapon.FireMode == FireMode.SingleAction && CurrentWeapon.Feeding) {
                CurrentWeapon.Feeding = false;
                shoot = true;
            }

            if (shoot) { 
                var bullet = Instantiate(CurrentWeapon.ProjectilePrefab);              
                bullet.transform.position = Camera.main.transform.position - new Vector3(0, 0.2f, 0);

                switch (CurrentWeapon.Name)
                {
                    case "Floppy":
                        if (UnityEngine.Random.Range(0, 2) == 1) //randomly flip projectile
                        {
                            bullet.transform.rotation = Quaternion.Euler(180, 0, 0);
                        }
                        var currentMaterial = currentWeaponScript.GetMaterial();
                        var Script = bullet.GetComponent<IWeapon>();
                        Script.SetMaterial(currentMaterial);

                        ChangeWeapon();

                        break;
                    default:
                        break;
                }

                #region bullet
                var rg = bullet.GetComponent<Rigidbody>();
                rg.mass = CurrentWeapon.ProjectileMass;
                Vector3 pushDir = Camera.main.transform.forward;
                rg.maxAngularVelocity = 180;
                rg.AddForce(pushDir.normalized * rg.mass * CurrentWeapon.WeaponPower, ForceMode.Impulse);
                rg.AddTorque(CurrentWeapon.RotationVector, ForceMode.Impulse);
                #endregion

                currentWeaponScript.Fire();
            }
        } else
        {
            CurrentWeapon.Feeding = true;
        }
        
        // Does the ray intersect any objects excluding the player layer

        if (Input.GetKeyDown("e") && IsNearComputerScreen())
        {
            OnComputerScreenInput(KeyCode.E);
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
            CurrentWeapon.RateOfFire = 0.1f;
        }
        else if (Input.GetKeyDown("4"))
        {
            CurrentWeapon = Weapons.Shotgun();
        }

    }

    private void ChangeWeapon()
    {
        if (currentWeaponGameObject != null)
        {
            Destroy(currentWeaponGameObject);
        }
        currentWeaponGameObject = Instantiate(CurrentWeapon.WeaponGameObject, Camera.main.transform);
        currentWeaponScript = currentWeaponGameObject.GetComponent<IWeapon>();
        currentWeaponScript.MakeKinematic();
        currentWeaponGameObject.transform.localPosition = CurrentWeapon.WeaponLocalPosition;
        currentWeaponGameObject.transform.localRotation = Quaternion.Euler(CurrentWeapon.WeaponLocalRoration);
        
    }

    private bool IsNearComputerScreen()
    {
        int layerMask = 1 << LayerMask.NameToLayer("Login");
        bool rayIsHit = Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out RaycastHit hit, Mathf.Infinity, layerMask);

        bool result = rayIsHit && hit.distance < 1.6f;
        if (!result)
        {
            audioSource.clip = UseSoundMiss;
            audioSource.Play();
        } else
        {
            audioSource.clip = UseSoundHit;
            audioSource.Play();
        }
        return result;
    }
}
