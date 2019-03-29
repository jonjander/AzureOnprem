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
    NavMeshAgent agent;
    BoxCollider textureCollider;
    SpriteRenderer spriteRenderer;
    float checkSeen = 0f;
    float targetAlpha = 0f;
    Vector3 StartPos;
    public bool isVisible = false;
    public bool offScreenReset = false;
    public GnomeStates State;
    public float FreezPosition;

    public List<AudioClip> Voices;
    AudioSource audioSource;

    public FluorescentLamp SyncLamp;
    bool lampTest = false;
    public float lampHide = 0;

    private GameObject newHide;
    private float agentSpeed;

    public float RangeThreshold;
    public float cameraAngle;
    public float cameraDistance;
    private Earthquake eqScript;

    private FluorescentLamp currentLamp;
    private FluorescentLamp previusLamp;

    bool ImInCamera()
    {
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(Camera.main);
        if (GeometryUtility.TestPlanesAABB(planes, textureCollider.bounds))
            return true;
        else
            return false;
    }

    bool ImBehindObject()
    {

        var GnomePos = textureCollider.transform.position;

        Vector3 size = textureCollider.size;

        Vector3 center = new Vector3(textureCollider.center.x, textureCollider.center.y, textureCollider.center.z);

        var margin = 0.05f;
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

        var RayTargets = new List<Vector3>(){
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

        var CamPos = Camera.main.transform.position;

        var hits = RayTargets.Where(tar =>
        {
            RaycastHit hit;
            var heading = tar - CamPos;
            var isHit = Physics.Raycast(CamPos, heading, out hit);
            
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
                    Debug.DrawRay(CamPos, heading, Color.green);
                } else
                {
                    Debug.DrawRay(CamPos, heading, Color.red);
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
            isVisible = false;
        }
    }

    public float AngelAlpha(float CameraAngle)
    {
        var MappedAlpha = Utils.Remap(CameraAngle, 40, 60, 0, 1);
        return (float)Utils.flattern(MappedAlpha);
    }

    public float DistanceAlpha(float CameraDistance)
    {
        var DistanceAlpha = Utils.Remap(CameraDistance, 9, 7.9f, 1, 0);
        return (float)Utils.flattern(DistanceAlpha);
    }

    public float DistanceAlpha(float CameraDistance, float max, float min)
    {
        var DistanceAlpha = Utils.Remap(CameraDistance, max, min, 1, 0);
        return (float)Utils.flattern(DistanceAlpha);
    }

    public float CameraAngle()
    {
        Vector3 v3_Dir = transform.position - Camera.main.transform.position;
        float f_AngleBetween = Vector3.Angle(transform.forward, v3_Dir); // Returns an angle between 0 and 180
                                                                         
        return Mathf.Abs(f_AngleBetween - 90);
    }

    float CameraDistance()
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
        StartPos = transform.localPosition;
        textureCollider = GetComponent<BoxCollider>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        SyncLamp = FindClosestLamp().GetComponent<FluorescentLamp>();
        agent = GetComponent<NavMeshAgent>();
        State = GnomeStates.FindHide;
        agentSpeed = agent.speed;
        eqScript = GameObject.FindObjectOfType<Earthquake>();
    }

    private bool move()
    {
        try
        {
            List<GameObject> hidePlaces = new List<GameObject>();
            hidePlaces.AddRange(GameObject.FindGameObjectsWithTag("HideLeft").ToList());
            hidePlaces.AddRange(GameObject.FindGameObjectsWithTag("HideRight").ToList());

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

    float minAlpha(List<float> inputList)
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
        if (cLamp != currentLamp && !eqScript.activeEarthquake)
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

        cameraAngle = CameraAngle();
        cameraDistance = CameraDistance();

        if (newHide == null)
        {
            State = GnomeStates.Flee;
        }

        if (!audioSource.isPlaying)
        {
            audioSource.clip = Voices
                .OrderBy(s => Guid.NewGuid())
                .FirstOrDefault();
            if (isVisible)
            {
                audioSource.Play();
            }
            audioSource.panStereo = UnityEngine.Random.Range(0, 2) == 0 ? -1 : 1;
        }

        switch (State)
        {
            case GnomeStates.FindHide:
                if (!move())
                {
                    isVisible = false;
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

                if (FreezPosition - CameraDistance() > 0.1f)
                {
                    State = GnomeStates.FindHide;
                }
                break;
            case GnomeStates.Visible:
                isVisible = true;
                if (ImInCamera())
                {
                    FreezPosition = CameraDistance();
                    State = GnomeStates.PlayerContact;
                }
                break;
            case GnomeStates.Flee:
                //Become invisible
                if (!move())
                {
                    isVisible = false;
                    break;
                }
                var change = FreezPosition - CameraDistance();
                agent.speed += change;
                if (!ImInCamera() || !spriteRenderer.isVisible || ImBehindObject())
                {
                    isVisible = false;
                    State = GnomeStates.MoveHidden;
                    agent.speed = agentSpeed;
                }

                break;
            case GnomeStates.InHideHidden:

                if (!ImInCamera() && !isVisible && cameraDistance > RangeThreshold )
                {
                    if (!offScreenReset)
                    {
                        //Chans to spawn
                        if (UnityEngine.Random.Range(0,3) == 1)
                        {
                            isVisible = true;
                            State = GnomeStates.Visible;
                        }
                    }
                    offScreenReset = true;
                } else if (ImInCamera())
                {
                    offScreenReset = false;
                }
                break;
            default:
                break;
        }
        var a = 1;

        spriteRenderer.enabled = isVisible;
        var camRot = Camera.main.transform.rotation;
        transform.rotation = Quaternion.Euler(0, camRot.eulerAngles.y, 0);

        spriteRenderer.color = new Color(7f / 255, 6f / 255, 6f / 255, a);

    }
}
