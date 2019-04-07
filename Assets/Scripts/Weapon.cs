using System.Collections.Generic;
using UnityEngine;

public class Weapon
{
    public string Name;
    public GameObject ProjectilePrefab;
    public float WeaponPower;
    public float ProjectileMass;
    public Vector3 RotationVector;
    public float RateOfFire;
    public FireMode FireMode;
    public float FeedingTimer;
    public bool Feeding = true;
    public bool Reloading = true;
    public GameObject WeaponGameObject;
    public Vector3 WeaponLocalPosition;
    public Vector3 WeaponLocalRoration;
}

public enum FireMode
{
    SingleAction,
    FullAuto
}

public static class Weapons
{
    
    public static Weapon FloppyDiskAuto()
    {
        return new Weapon()
        {
            Name = "Floppy",
            ProjectilePrefab = Resources.Load<GameObject>("Floppy"),
            WeaponPower = 12,
            ProjectileMass = 0.3f,
            RotationVector = new Vector3(Random.Range(0.2f, 100f), 1000, Random.Range(0.2f, 100f)),
            FireMode = FireMode.FullAuto,
            RateOfFire = 0.2f,
            WeaponGameObject = Resources.Load<GameObject>("DisplayFloppy"),
            WeaponLocalPosition = new Vector3(0.16f, -0.148f, 0.345f),
            WeaponLocalRoration = new Vector3(42.175f, 97.394f, 100.96f)
        };
    }
    public static Weapon FloppyDisk()
    {
        return new Weapon()
        {
            Name = "Floppy",
            ProjectilePrefab = Resources.Load<GameObject>("Floppy"),
            WeaponPower = 12,
            ProjectileMass = 0.35f,
            RotationVector = new Vector3(Random.Range(0.2f, 100f), 1000, Random.Range(0.2f, 100f)),
            FireMode = FireMode.SingleAction,
            RateOfFire = 1f,
            WeaponGameObject = Resources.Load<GameObject>("DisplayFloppy"),
            WeaponLocalPosition = new Vector3(0, 0, 0),
            WeaponLocalRoration = new Vector3(0, 0, 0)
        };
    }
    public static Weapon Shotgun()
    {
        return new Weapon()
        {
            Name = "Shotgun",
            ProjectilePrefab = Resources.Load<GameObject>("Floppy"),
            WeaponPower = 400,
            ProjectileMass = 0.35f,
            RotationVector = Vector3.zero,
            FireMode = FireMode.SingleAction,
            RateOfFire = 1f,
            WeaponGameObject = Resources.Load<GameObject>("Shotgun"),
            WeaponLocalPosition = new Vector3(0.26f, -0.196f, 0.38f),
            WeaponLocalRoration = new Vector3(0, 180f, 0)
        };
    }

}