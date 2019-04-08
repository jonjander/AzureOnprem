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

        GunState gunState = CurrentWeapon.UpdateTrigger(Input.GetMouseButton(0));
        Debug.Log(gunState);
        switch (gunState)
        {
            case GunState.Normal:
                break;
            case GunState.Fire:
                GameObject bullet;
                Rigidbody rg;
                Vector3 pushDir = Camera.main.transform.forward;

                switch (CurrentWeapon.Type)
                {
                    case WeaponType.Throwable:
                        bullet = Instantiate(CurrentWeapon.ProjectilePrefab);
                        bullet.transform.position = currentWeaponGameObject.transform.position;
                        bullet.transform.rotation = currentWeaponGameObject.transform.rotation;
                        Material currentMaterial = currentWeaponScript.GetMaterial();
                        IWeapon script = bullet.GetComponent<IWeapon>();
                        script.SetMaterial(currentMaterial);

                        rg = bullet.GetComponent<Rigidbody>();
                        rg.isKinematic = false;
                        if (UnityEngine.Random.Range(0, 2) == 1) //randomly flip projectile
                        {
                            bullet.transform.rotation = Quaternion.Euler(180, 0, 0);
                        }

                        rg.AddTorque(CurrentWeapon.RotationVector, ForceMode.Impulse);
                        rg.AddForce(pushDir.normalized * rg.mass * CurrentWeapon.WeaponPower, ForceMode.Impulse);
                        ChangeWeapon();

                        break;
                    case WeaponType.Gun:
                        bullet = Instantiate(CurrentWeapon.ProjectilePrefab);
                        rg = bullet.GetComponent<Rigidbody>();
                        bullet.transform.position = Camera.main.transform.position - new Vector3(0, 0.2f, 0);

                        rg.mass = CurrentWeapon.ProjectileMass;

                        rg.maxAngularVelocity = 180;
                        rg.AddForce(pushDir.normalized * rg.mass * CurrentWeapon.WeaponPower, ForceMode.Impulse);
                        break;
                    default:
                        break;
                }
                currentWeaponScript.Fire();
                break;
            case GunState.Feeding:
                break;
            case GunState.Reload:
                currentWeaponScript.Reload();
                break;
            case GunState.Reloading:
                
                break;
            default:
                break;
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
