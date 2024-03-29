using System.Collections.Generic;
using UnityEngine;

public class Weapon
{
    public WeaponType Type;
    public GameObject ProjectilePrefab;
    public float WeaponPower;
    public float ProjectileMass;
    public Vector3 RotationVector;
    public float RateOfFire;
    public FireMode FireMode;
    public float FeedingTime;
    public float ReloadTime;
    public int MagSize;
    public int Ammo;
    public bool IsPicked;
    public string Name;
    
    public GameObject WeaponGameObject;
    public Vector3 WeaponLocalPosition;
    public Vector3 WeaponLocalRoration;

    private GunState currentGunState;
    private float feedingTimer;
    private float reloadTimer;
    private bool lastTriggerState;

    public Weapon()
    {
        currentGunState = GunState.Normal;
        Ammo = MagSize;
        feedingTimer = 0;
        reloadTimer = 0;
        lastTriggerState = false;
    }


    public GunState UpdateTrigger(bool trigger)
    {
        switch (currentGunState)
        {
            case GunState.Normal:
                bool triggerEffective = false;
                if (FireMode == FireMode.FullAuto)
                {
                    triggerEffective = trigger;
                }
                else if (FireMode == FireMode.SingleAction)
                {
                    if (!lastTriggerState && trigger)
                    {
                        triggerEffective = true;
                    }
                }
                lastTriggerState = trigger;
                if (Ammo == 0)
                {
                    currentGunState = GunState.Reload;
                }
                if (triggerEffective)
                {

                    if (Ammo != 0)
                    {
                        Ammo--;
                        currentGunState = GunState.Fire;
                    }
                }
                break;
            case GunState.Fire:
                //Send fire event
                currentGunState = GunState.Feeding;
                break;
            case GunState.Feeding:
                if (FeedingTime == 0) //Feeding disabled
                {
                    currentGunState = GunState.Normal;
                    break;
                }
                feedingTimer += Time.deltaTime;
                if (feedingTimer >= FeedingTime)
                {
                    feedingTimer = 0;
                    currentGunState = GunState.Normal;
                }
                break;
            case GunState.Reload:
                currentGunState = GunState.Reloading;
                break;
            case GunState.Reloading:
                if (ReloadTime == 0) //Reloading disabled
                {
                    Ammo = MagSize;
                    currentGunState = GunState.Feeding;
                    break;
                }
                reloadTimer += Time.deltaTime;
                if (reloadTimer >= ReloadTime)
                {
                    Ammo = MagSize;
                    reloadTimer = 0;
                    currentGunState = GunState.Feeding;
                }
                break;
            default:
                break;
        }
        return currentGunState;
    }
}

public enum GunState
{
    Normal,
    Fire,
    Feeding,
    Reloading,
    Reload
}

public enum FireMode
{
    SingleAction,
    FullAuto
}

public enum WeaponType
{
    Melee,
    Throwable,
    Gun
}

public static class Weapons
{
    public static Weapon FloppyDiskAuto = new Weapon()
    {
        Name = "DisplayFloppy",
            MagSize = 1,
            ReloadTime = 0,
            FeedingTime = 0.05f,
            Type = WeaponType.Throwable,
            ProjectilePrefab = Resources.Load<GameObject>("Floppy"),
            WeaponPower = 12,
            ProjectileMass = 0.45f,
            RotationVector = new Vector3(Random.Range(0.2f, 100f), 1000, Random.Range(0.2f, 100f)),
            FireMode = FireMode.FullAuto,
            RateOfFire = 0.2f,
            WeaponGameObject = Resources.Load<GameObject>("DisplayFloppy"),
            WeaponLocalPosition = new Vector3(0.16f, -0.148f, 0.345f),
            WeaponLocalRoration = new Vector3(42.175f, 97.394f, 100.96f),
            IsPicked = true
        };
    public static Weapon FloppyDisk = new Weapon()
        {
            Name = "DisplayFloppy",
            MagSize = 1,
            ReloadTime = 0,
            FeedingTime = 0.15f,
            Type = WeaponType.Throwable,
            ProjectilePrefab = Resources.Load<GameObject>("Floppy"),
            WeaponPower = 12,
            ProjectileMass = 0.45f,
            RotationVector = new Vector3(Random.Range(0.2f, 100f), 1000, Random.Range(0.2f, 100f)),
            FireMode = FireMode.SingleAction,
            RateOfFire = 1f,
            WeaponGameObject = Resources.Load<GameObject>("DisplayFloppy"),
            WeaponLocalPosition = new Vector3(0.16f, -0.148f, 0.345f),
            WeaponLocalRoration = new Vector3(42.175f, 97.394f, 100.96f),
            IsPicked = true
        };
    
    public static Weapon Shotgun = new Weapon()
        {
            Name = "Shotgun",
            MagSize = 2,
            ReloadTime = 2f,
            FeedingTime = 0.1f,
            Type = WeaponType.Gun,
            ProjectilePrefab = Resources.Load<GameObject>("ShotgunSwarm"),
            WeaponPower = 150,
            ProjectileMass = 0.55f,
            RotationVector = Vector3.zero,
            FireMode = FireMode.SingleAction,
            RateOfFire = 1f,
            WeaponGameObject = Resources.Load<GameObject>("Shotgun"),
            WeaponLocalPosition = new Vector3(0.26f, -0.196f, 0.38f),
            WeaponLocalRoration = new Vector3(0, 180f, 0),
            IsPicked = false
        };
    

    public static Weapon GnomeFinder = new Weapon()
        {
            Name = "GnomeFinder",
            MagSize = 0,
            ReloadTime = 1,
            FeedingTime = 1f,
            Type = WeaponType.Melee,
            ProjectilePrefab = null,
            WeaponPower = 12,
            ProjectileMass = 0f,
            RotationVector = Vector3.zero,
            FireMode = FireMode.SingleAction,
            RateOfFire = 2f,
            WeaponGameObject = Resources.Load<GameObject>("GnomeFinder"),
            WeaponLocalPosition = new Vector3(0.0286f, -0.723f, 0.4932f),
            WeaponLocalRoration = new Vector3(5.895f, 1.685f, -2.192f),
            IsPicked = false
        };
    

    
}