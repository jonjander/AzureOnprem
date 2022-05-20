using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Assets.Scripts;
using Microsoft.Identity.Client;
using UnityEngine;

public class PlayerScript : MonoBehaviour {

    //public GameObject Projectile;
    public AudioClip UseSoundHit;
    public AudioClip UseSoundMiss;
    public DCGenerator GenerationScript;
    public delegate bool ComputerScreenInput(KeyCode key);
    public static event ComputerScreenInput OnComputerScreenInput;

    public delegate void UserCodeInput(string userCode);
    public static event UserCodeInput OnUserCodeGenerated;


    private AudioSource audioSource;
    private GameObject currentWeaponGameObject;
    private Weapon currentWeapon;
    private IWeapon currentWeaponScript;

    private GameObject exit;

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
        CurrentWeapon = Weapons.FloppyDisk;
        audioSource = GetComponent<AudioSource>();
        exit = GameObject.FindGameObjectWithTag("Exit");
    }

    private IEnumerator Login()
    {
        yield return new WaitForSeconds(UseSoundHit.length);
        var task = DoLogin();
        while (!task.IsCanceled && !task.IsCompleted && !task.IsFaulted)
        {
            yield return new WaitForSeconds(0.01f);
        }
    }
    public async Task DoLogin()
    {
        //AccessToken = LoginHelper.GetToken();
        IPublicClientApplication publicClientApplication = PublicClientApplicationBuilder
            .Create("1950a258-227b-4e31-a9cf-717495945fc2")
            .WithTenantId("common")
            .WithRedirectUri("urn:ietf:wg:oauth:2.0:oob")
            .Build();
        var accounts = await publicClientApplication.GetAccountsAsync();
        string[] scopes = new string[] { "https://management.azure.com/.default" };

        var deviceCode = await publicClientApplication.AcquireTokenWithDeviceCode(scopes,
         callback =>
         {
             OnUserCodeGenerated(callback.UserCode);
             return Task.FromResult(0);
         }
        ).ExecuteAsync();


        StartCoroutine(AzureManagementAPIHelper.GetSubscriptions(deviceCode.AccessToken));
    }


    // Update is called once per frame
    void Update () {
        CheckExitGame();

        GunState gunState = CurrentWeapon.UpdateTrigger(Input.GetMouseButton(0));
        //Debug.Log(gunState);
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

                        rg.maxAngularVelocity = 120;
                        rg.mass = CurrentWeapon.ProjectileMass;
                        rg.AddTorque(CurrentWeapon.RotationVector, ForceMode.Impulse);
                        rg.AddForce(pushDir.normalized * rg.mass * CurrentWeapon.WeaponPower, ForceMode.Impulse);
                        Destroy(currentWeaponGameObject);

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
                if (CurrentWeapon.Type == WeaponType.Throwable)
                {
                    ChangeWeapon();
                }
                break;
            case GunState.Reloading:
                
                break;
            default:
                break;
        }

        // Does the ray intersect any objects excluding the player layer

        if (Input.GetKeyDown("e") && IsNearComputerScreen())
        {
            if (OnComputerScreenInput(KeyCode.E))
            {
                StartCoroutine(Login());
            }
        } else if (Input.GetKeyDown("e") && IsNearShotgun())
        {
            if (!Weapons.Shotgun.IsPicked)
            {
                PickUpWeapon(Weapons.Shotgun);
            }
        }
        else if (Input.GetKeyDown("e") && IsNearGnomeFinder())
        {
            if (!Weapons.GnomeFinder.IsPicked)
            {
                PickUpWeapon(Weapons.GnomeFinder);
            }
        } else {
            audioSource.clip = UseSoundMiss;
            audioSource.Play();
        }

        if (Input.GetKeyDown(KeyCode.Y) && IsNearComputerScreen())
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
            CurrentWeapon = Weapons.FloppyDisk;
        }
        else if (Input.GetKeyDown("2"))
        {
            CurrentWeapon = Weapons.FloppyDiskAuto;
        }
        else if (Input.GetKeyDown("3"))
        {
            if (Weapons.Shotgun.IsPicked)
            {
                CurrentWeapon = Weapons.Shotgun;
            }
        }
        else if (Input.GetKeyDown("4"))
        {
            if (Weapons.GnomeFinder.IsPicked)
            {
                CurrentWeapon = Weapons.GnomeFinder;
            }
        }

    }

    private void PickUpWeapon(Weapon selectedWeapon)
    {
        var weapon = GameObject.FindGameObjectsWithTag("WeaponRoot")
            .Where(s => s.name == selectedWeapon.Name)
            .Select(g => new { wep = g, distance = (g.transform.position - transform.position).magnitude })
            .OrderBy(f=> f.distance)
            .FirstOrDefault();

        var weaponInRange = weapon.wep;
        var weaponScript = weaponInRange.GetComponent<IWeapon>();
        if (weaponInRange && !weaponScript.IsLocked())
        {
            selectedWeapon.IsPicked = true;
            SetCurrentWeapon(selectedWeapon);
            ChangeWeapon(true, weaponInRange);
        }
    }



    private void ChangeWeapon(bool skipInstantiate = false, GameObject newWeaponGameObject = null)
    {
        if (currentWeaponGameObject != null)
        {
            Destroy(currentWeaponGameObject);
        }
        if (skipInstantiate)
        {
            currentWeaponGameObject = newWeaponGameObject;
            currentWeaponGameObject.transform.parent = Camera.main.transform;
        } else 
        {
            currentWeaponGameObject = Instantiate(CurrentWeapon.WeaponGameObject, Camera.main.transform);
        }
        currentWeaponScript = currentWeaponGameObject.GetComponent<IWeapon>();
        currentWeaponScript.MakeKinematic();
        currentWeaponGameObject.transform.localPosition = CurrentWeapon.WeaponLocalPosition;
        currentWeaponGameObject.transform.localRotation = Quaternion.Euler(CurrentWeapon.WeaponLocalRoration);

    }

    private void CheckExitGame()
    {
        if (Vector3.Distance(transform.position, exit.transform.position) < 0.85f)
        {
            #if UNITY_EDITOR
                        UnityEditor.EditorApplication.isPlaying = false;
            #elif UNITY_WEBPLAYER
                             Application.OpenURL(webplayerQuitURL);
            #else
                             Application.Quit();
            #endif
        }
    }

    private bool IsNearComputerScreen()
    {
        int layerMask = 1 << LayerMask.NameToLayer("Login");
        bool rayIsHit = Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out RaycastHit hit, Mathf.Infinity, layerMask);

        bool result = rayIsHit && hit.distance < 1.6f;
        if (result)
        {
            audioSource.clip = UseSoundHit;
            audioSource.Play();
        }
        return result;
    }

    private bool IsNearShotgun()
    {
       var vectorLengths = GameObject.FindGameObjectsWithTag("WeaponRoot")
            .Where(s => s.name == "Shotgun")
            .Select(g => (g.transform.position - transform.position).magnitude)
            .ToList();
        return vectorLengths.Count > 0 ? vectorLengths.Min() < 1.5 : false;
    }

    private bool IsNearGnomeFinder()
    {
        var vectorLengths = GameObject.FindGameObjectsWithTag("WeaponRoot")
            .Where(s => s.name == "GnomeFinder")
            .Select(g => (g.transform.position - transform.position).magnitude)
            .ToList();
        return vectorLengths.Count > 0 ? vectorLengths.Min() < 1.5 : false;
    }

    public void SetCurrentWeapon(Weapon weapon)
    {
        currentWeapon = weapon;
    }
}
