using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Shotgun : MonoBehaviour, IWeapon
{
    private Rigidbody weaponRigidbody;
    private Rigidbody weaponRootRigidbody;
    private Transform gun;
    private Animator animator;
    private AudioSource soundSouce;
    public List<AudioClip> ShotSounds;
    public AudioClip ReloadSound;
    public GameObject Shell;

    private GameObject[] gunShells;
    private Vector3 localShellPosition1;
    private Vector3 localShellPosition2;
    private float soundOffSetShell1;
    private float soundOffSetShell2;
    private float ejectorOffsetShells;
    private Vector3 localShellRotation;

    // Start is called before the first frame update
    void Start()
    {
        gunShells = new GameObject[2];
        localShellPosition1 = new Vector3(0.00078f, -0.00212f, 0.00394f);
        localShellPosition2 = new Vector3(-0.00077f, -0.00212f, 0.00394f);
        soundOffSetShell1 = 0.682f;
        soundOffSetShell2 = 0.932f;
        ejectorOffsetShells = 0.417f;

        localShellRotation = new Vector3(0, -180f, 0);
        Shell = Resources.Load<GameObject>("Shell");

        GetRootRigidbody();
        weaponRigidbody.maxAngularVelocity = 20f;
        gun = GetComponentsInChildren<Transform>()
            .First(r => r.gameObject.name == "Pipe");

        animator = gameObject.GetComponentInChildren<Animator>();

        soundSouce = GetComponent<AudioSource>();
    }

    private void GetRootRigidbody()
    {
        weaponRigidbody = GetComponentsInChildren<Rigidbody>()
                    .Where(r => r.tag == "Weapon")
                    .FirstOrDefault();
        weaponRootRigidbody = GetComponentsInChildren<Rigidbody>()
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
        weaponRootRigidbody.isKinematic = kine;
    }

    public void Fire()
    {
        soundSouce.clip = ShotSounds
            .OrderBy(d => Guid.NewGuid())
            .FirstOrDefault();
        soundSouce.Play();
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

    public void Reload()
    {
        StartCoroutine(ReloadingSequence());
    }

    private IEnumerator ReloadingSequence()
    {
        float wait = 0.7f;
        while (wait > 0)
        {
            wait -= Time.deltaTime;
            yield return true;
        }
        soundSouce.clip = ReloadSound;
        soundSouce.Play();
        animator.Play("Reload");

        var shell1Done = false;
        var shell2Done = false;
        var ejected = false;
        float clipTime = 0;
        while (soundSouce.isPlaying)
        {
            clipTime += Time.deltaTime;
            if (clipTime > ejectorOffsetShells && !ejected)
            {
                foreach (var shell in gunShells)
                {
                    try
                    {
                        shell.transform.parent = null;
                        var shellRB = shell.GetComponent<Rigidbody>();
                        shellRB.isKinematic = false;
                        var ejectorForce = 0.22f;
                        shellRB.velocity = weaponRigidbody.velocity;
                        shellRB.AddForce(shellRB.transform.up.normalized * ejectorForce, ForceMode.Impulse);
                        shellRB.AddTorque(new Vector3(
                            0.0003f * UnityEngine.Random.Range(1f, 4f),
                            0.0006f * UnityEngine.Random.Range(1f, 4f),
                            0.002f * UnityEngine.Random.Range(1f, 4f)));
                    }
                    catch { }
                    ejected = true;
                }
            }
            if (clipTime > soundOffSetShell1 && !shell1Done)
            {
                shell1Done = true;
                loadShell(localShellPosition1, localShellRotation, Shell, 0);
            }
            if (clipTime > soundOffSetShell2 && !shell2Done)
            {
                shell2Done = true;
                loadShell(localShellPosition2, localShellRotation, Shell, 1);
            }
            yield return true;
        }

    }

    private void loadShell(Vector3 localShellPosition, Vector3 localShellRotation, GameObject shell, int index)
    {
        var pipe = GetComponentsInChildren<Transform>()
            .Where(s => s.name == "Pipe")
            .FirstOrDefault();
        var tmpShell = Instantiate(Shell, pipe.transform);
        tmpShell.transform.localPosition = localShellPosition;
        tmpShell.transform.localRotation = Quaternion.Euler(localShellRotation);
        //Destroy(gunShells[index]);
        tmpShell.GetComponent<Rigidbody>().isKinematic = true;
        gunShells[index] = tmpShell;
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
