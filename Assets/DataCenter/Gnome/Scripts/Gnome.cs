using Assets.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

public enum GnomeStates
{
    FindHide,
    MoveHidden,
    PlayerContact,
    Visible,
    Flee,
    InHideHidden,
}

class Gnome : MonoBehaviour
{
    private NavMeshAgent agent;
    private BoxCollider textureCollider;
    private SpriteRenderer spriteRenderer;
    private float checkSeen = 0f;
    private float targetAlpha = 0f;
    private Vector3 startPos;
    private AudioSource audioSource;
    private GameObject newHide;
    private float agentSpeed;
    private Earthquake eqScript;
    private FluorescentLamp currentLamp;
    private FluorescentLamp previusLamp;

    public bool IsVisible = false;
    public bool OffScreenReset = false;
    public GnomeStates State;
    public float FreezPosition;
    public List<AudioClip> Voices;
    public FluorescentLamp SyncLamp;
    public float LampHide = 0;
    public float RangeThreshold;
    public float CameraAngle;
    public float CameraDistance;


    bool ImInCamera()
    {
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(Camera.main);
        return GeometryUtility.TestPlanesAABB(planes, textureCollider.bounds) ? true : false;
    }

    bool ImBehindObject()
    {

        Vector3 gnomePos = textureCollider.transform.position;

        Vector3 size = textureCollider.size;

        Vector3 center = new Vector3(textureCollider.center.x, textureCollider.center.y, textureCollider.center.z);

        float margin = 0.05f;
        Vector3 vertex1 = new Vector3(center.x + size.x / 2 - margin, center.y - size.y / 2 + margin, center.z + size.z / 2 - margin);
        Vector3 vertex2 = new Vector3(center.x - size.x / 2 + margin, center.y - size.y / 2 + margin, center.z - size.z / 2 + margin);
        Vector3 vertex3 = new Vector3(center.x + size.x / 2 - margin, center.y + size.y / 2 - margin, center.z + size.z / 2 - margin);
        Vector3 vertex4 = new Vector3(center.x - size.x / 2 + margin, center.y + size.y / 2 - margin, center.z - size.z / 2 + margin);
        Vector3 vertex5 = new Vector3(center.x + size.x / 4 - margin, center.y - size.y / 4 + margin, center.z + size.z / 4 - margin);
        Vector3 vertex6 = new Vector3(center.x - size.x / 4 + margin, center.y - size.y / 4 + margin, center.z - size.z / 4 + margin);
        Vector3 vertex7 = new Vector3(center.x + size.x / 4 - margin, center.y + size.y / 4 - margin, center.z + size.z / 4 - margin);
        Vector3 vertex8 = new Vector3(center.x - size.x / 4 + margin, center.y + size.y / 4 - margin, center.z - size.z / 4 + margin);
        Vector3 vertex9 = new Vector3(center.x + size.x / 2 - margin, center.y, center.z + size.z / 2 - margin);
        Vector3 vertex10 = new Vector3(center.x - size.x / 2 + margin, center.y, center.z - size.z / 2 + margin);

        float rayLength = size.y / 2;

        var rayTargets = new List<Vector3>(){
            textureCollider.transform.TransformPoint(vertex1),
            textureCollider.transform.TransformPoint(vertex2),
            textureCollider.transform.TransformPoint(vertex3),
            textureCollider.transform.TransformPoint(vertex4),
            textureCollider.transform.TransformPoint(vertex5),
            textureCollider.transform.TransformPoint(vertex6),
            textureCollider.transform.TransformPoint(vertex7),
            textureCollider.transform.TransformPoint(vertex8),
            textureCollider.transform.TransformPoint(vertex9),
            textureCollider.transform.TransformPoint(vertex10),
        };

        Vector3 camPos = Camera.main.transform.position;

        var hits = rayTargets.Where(tar =>
        {
            Vector3 heading = tar - camPos;
            bool isHit = Physics.Raycast(camPos, heading, out RaycastHit hit);
            
            if (!isHit)
            {
                //Debug.DrawRay(CamPos, heading, Color.red);
                return false;
            }
            else
            {
                var dwarfHit = hit.collider.gameObject.layer == LayerMask.NameToLayer("Gnome");
                if (dwarfHit)
                {
                    Debug.DrawRay(camPos, heading, Color.green);
                } else
                {
                    Debug.DrawRay(camPos, heading, Color.red);
                }
                return dwarfHit;
            }
        });
        
        return hits.Count() == 0;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Bullet")
        {
            eqScript.StartEarthQuake(UnityEngine.Random.Range(2, 14));
            IsVisible = false;
        }
    }

    public float AngelAlpha(float cameraAngle)
    {
        double mappedAlpha = Utils.Remap(cameraAngle, 40, 60, 0, 1);
        return (float)Utils.Flattern(mappedAlpha);
    }

    public float DistanceAlpha(float cameraDistance)
    {
        double distanceAlpha = Utils.Remap(cameraDistance, 9, 7.9f, 1, 0);
        return (float)Utils.Flattern(distanceAlpha);
    }

    public float DistanceAlpha(float cameraDistance, float max, float min)
    {
        var distanceAlpha = Utils.Remap(cameraDistance, max, min, 1, 0);
        return (float)Utils.Flattern(distanceAlpha);
    }

    private float GetCameraAngle()
    {
        Vector3 v3_Dir = transform.position - Camera.main.transform.position;
        float f_AngleBetween = Vector3.Angle(transform.forward, v3_Dir); // Returns an angle between 0 and 180
                                                                         
        return Mathf.Abs(f_AngleBetween - 90);
    }

    private float GetCameraDistance()
    {
        return Vector3.Distance(transform.position, Camera.main.transform.position);
    }

    public FluorescentLamp FindClosestLamp()
    {
        GameObject[] gos;
        gos = GameObject.FindGameObjectsWithTag("Lamp");
        GameObject closest = null;
        float distance = Mathf.Infinity;
        Vector3 position = transform.position;
        foreach (GameObject go in gos)
        {
            Vector3 diff = go.transform.position - position;
            float curDistance = diff.sqrMagnitude;
            if (curDistance < distance)
            {
                closest = go;
                distance = curDistance;
            }
        }
        return closest.GetComponent<FluorescentLamp>();
    }

    private void Start()
    {
        startPos = transform.localPosition;
        textureCollider = GetComponent<BoxCollider>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        audioSource.spatialBlend = 1;
        audioSource.panStereo = 0;
        SyncLamp = FindClosestLamp().GetComponent<FluorescentLamp>();
        agent = GetComponent<NavMeshAgent>();
        State = GnomeStates.FindHide;
        agentSpeed = agent.speed;
        eqScript = GameObject.FindObjectOfType<Earthquake>();
    }

    private bool Move()
    {
        try
        {
            List<GameObject> hidePlaces = GameObject.FindGameObjectsWithTag("Hide").ToList();

            newHide = hidePlaces.Select(s =>
            {
                var distance = Vector3.Distance(Camera.main.transform.position, s.transform.position);
                return new { HidePlace = s, distance };
            })
                .OrderByDescending(s => s.distance)
                .FirstOrDefault()
                .HidePlace;

            agent.SetDestination(newHide.transform.position);
            return true;
        }
        catch {
            return false;
        }
    }

    float MinAlpha(List<float> inputList)
    {
        return inputList.Min();
    }

    float GetAlpha(float currentA)
    {
        var diff = targetAlpha - currentA;
        if (diff > 0)
        {
            targetAlpha -= Time.deltaTime;
        } else if (diff < 0)
        {
            targetAlpha += Time.deltaTime;
        }
        //targetAlpha = (targetAlpha + currentA) / 2;

        return targetAlpha;
    }

    private void Update()
    {
        var cLamp = FindClosestLamp();
        if (cLamp != currentLamp && eqScript.State == EarthquakeStates.Off)
        {
            //Change lamp
            if (currentLamp == null)
            {
                currentLamp = cLamp;
            } else
            {
                if (cLamp != null)
                {
                    currentLamp.isBroken = false;
                }
                cLamp.isBroken = true;
                currentLamp = cLamp;
            }
        }

        CameraAngle = GetCameraAngle();
        CameraDistance = GetCameraDistance();

        if (newHide == null)
        {
            State = GnomeStates.Flee;
        }

        if (!audioSource.isPlaying)
        {
            audioSource.clip = Voices
                .OrderBy(s => Guid.NewGuid())
                .FirstOrDefault();
            if (IsVisible || (agent.remainingDistance < 0.1f && State == GnomeStates.InHideHidden )) 
            {
                
                audioSource.Play();
            }
        }

        switch (State)
        {
            case GnomeStates.FindHide:
                if (!Move())
                {
                    IsVisible = false;
                    break;
                }
                State = GnomeStates.Flee;
                break;
            case GnomeStates.MoveHidden:
                if (agent.remainingDistance < 0.2f)
                {
                    State = GnomeStates.InHideHidden;
                }
                break;
            case GnomeStates.PlayerContact:
                if (!ImInCamera() || !spriteRenderer.isVisible || ImBehindObject())
                {
                    if (UnityEngine.Random.Range(0, 4) == 1)
                    {
                        State = GnomeStates.Flee;
                        agent.speed = agentSpeed;
                    }
                }

                if (FreezPosition - GetCameraDistance() > 0.1f)
                {
                    State = GnomeStates.FindHide;
                }
                break;
            case GnomeStates.Visible:
                IsVisible = true;
                if (ImInCamera())
                {
                    FreezPosition = GetCameraDistance();
                    State = GnomeStates.PlayerContact;
                }
                break;
            case GnomeStates.Flee:
                //Become invisible
                if (!Move())
                {
                    IsVisible = false;
                    break;
                }
                var change = FreezPosition - GetCameraDistance();
                agent.speed += change;
                if (!ImInCamera() || !spriteRenderer.isVisible || ImBehindObject())
                {
                    IsVisible = false;
                    State = GnomeStates.MoveHidden;
                    agent.speed = agentSpeed;
                }

                break;
            case GnomeStates.InHideHidden:

                if (!ImInCamera() && !IsVisible && CameraDistance > RangeThreshold )
                {
                    if (!OffScreenReset)
                    {
                        //Chans to spawn
                        if (UnityEngine.Random.Range(0,3) == 1)
                        {
                            IsVisible = true;
                            State = GnomeStates.Visible;
                        }
                    }
                    OffScreenReset = true;
                } else if (ImInCamera())
                {
                    OffScreenReset = false;
                }
                break;
            default:
                break;
        }
        var a = 1;

        spriteRenderer.enabled = IsVisible;
        var camRot = Camera.main.transform.rotation;
        transform.rotation = Quaternion.Euler(0, camRot.eulerAngles.y, 0);

        spriteRenderer.color = new Color(7f / 255, 6f / 255, 6f / 255, a);

    }
}
