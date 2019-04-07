using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Shotgun : MonoBehaviour, IWeapon
{
    private Rigidbody weaponRigidbody;
    private Transform gun;

    // Start is called before the first frame update
    void Start()
    {
        GetRootRigidbody();
        weaponRigidbody.maxAngularVelocity = 20f;
        gun = GetComponentsInChildren<Transform>()
            .First(r => r.gameObject.name == "Pipe");
    }

    private void GetRootRigidbody()
    {
        weaponRigidbody = GetComponentsInChildren<Rigidbody>()
                    .Where(r => r.tag == "WeaponRoot")
                    .FirstOrDefault();
    }

    // Update is called once per frame
    void Update()
    {
    }



    public void MakeKinematic(bool kine = true)
    {
        GetRootRigidbody();
        weaponRigidbody.isKinematic = kine;
    }

    public void Fire()
    {
        var power = 2000f;
        var flash = Resources.Load<ParticleSystem>("MuzzleFlash");
        var muzzelflash = Instantiate(flash);
        //fix this!
        var diff = new Vector3(90, -180, 0);
        muzzelflash.transform.rotation = Quaternion.Euler(gun.rotation.eulerAngles);
        var offsetUp = gun.forward * 0.07f;
        var offsetForward = gun.up * 0.6256f;
        Debug.DrawLine(gun.position + offsetUp, gun.position + offsetUp - offsetForward, Color.red, 10f);
        muzzelflash.transform.position = gun.position + offsetUp - offsetForward;
        weaponRigidbody.AddTorque(transform.position.normalized - gun.right * power, ForceMode.Impulse);
    }
    public Material GetMaterial()
    {
        throw new NotImplementedException();
    }
    public void SetMaterial(Material material)
    {
        throw new NotImplementedException();
    }
}
