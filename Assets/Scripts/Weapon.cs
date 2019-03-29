using System.Collections.Generic;
using UnityEngine;

public class Weapon
{
    public GameObject projectilePrefab;
    public float weaponPower;
    public float projectileMass;
    public Vector3 rotationVector;
    public float rateOfFire;
    public FireMode fireMode;
    public float feedingTimer;
    public bool feeding = true;
    public bool reloading = true;
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
            projectilePrefab = Resources.Load<GameObject>("Floppy"),
            weaponPower = 12,
            projectileMass = 0.3f,
            rotationVector = new Vector3(Random.Range(0.2f, 100f), 1000, Random.Range(0.2f, 100f)),
            fireMode = FireMode.FullAuto,
            rateOfFire = 0.2f
        };
    }
    public static Weapon FloppyDisk()
    {
        return new Weapon()
        {
            projectilePrefab = Resources.Load<GameObject>("Floppy"),
            weaponPower = 12,
            projectileMass = 0.35f,
            rotationVector = new Vector3(Random.Range(0.2f, 100f), 1000, Random.Range(0.2f, 100f)),
            fireMode = FireMode.SingleAction,
            rateOfFire = 1f
        };
    }

}