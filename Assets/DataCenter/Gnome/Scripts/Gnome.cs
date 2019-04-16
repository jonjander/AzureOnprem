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
    Peek,
    MovingToPeekLocation,
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
    private GameObject agentTargetDestination;
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
    private float updatePath;
    private GameObject gnomeImagination;

    bool ImInCamera()
    {
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(Camera.main);
        return GeometryUtility.TestPlanesAABB(planes, textureCollider.bounds) ? true : false;
    }


    

    bool ImBehindObject()
    {
        Vector3 size = textureCollider.size;
        Vector3 center = new Vector3(textureCollider.center.x, textureCollider.center.y, textureCollider.center.z);

        List<Vector3> rayTargets = CreateRayTargets(size, center, textureCollider);

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
                }
                else
                {
                    Debug.DrawRay(camPos, heading, Color.red);
                }
                return dwarfHit;
            }
        });

        return hits.Count() == 0;
    }

    private List<Vector3> CreateRayTargets(Vector3 size, Vector3 center, Collider collider)
    {
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
            collider.transform.TransformPoint(vertex1),
            collider.transform.TransformPoint(vertex2),
            collider.transform.TransformPoint(vertex3),
            collider.transform.TransformPoint(vertex4),
            collider.transform.TransformPoint(vertex5),
            collider.transform.TransformPoint(vertex6),
            collider.transform.TransformPoint(vertex7),
            collider.transform.TransformPoint(vertex8),
            collider.transform.TransformPoint(vertex9),
            collider.transform.TransformPoint(vertex10),
        };
        return rayTargets;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Bullet")
        {
            eqScript.StartEarthQuake(UnityEngine.Random.Range(2, 14));
            IsVisible = false;
            State = GnomeStates.FindHide;
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
        updatePath = 1f;

        gnomeImagination = Resources.Load<GameObject>("GnomeImagination");
    }

    private List<HideResult> ImagineGnomeLocations()
    {
        List<GameObject> allHidePlaces = GameObject.FindGameObjectsWithTag("Hide").ToList();
        var camPos = Camera.main.transform.position;
        List<HideResult> hideResults = new List<HideResult>();
        List<BoxCollider> gnomeImaginations = new List<BoxCollider>();


        Debug.Log("update");
        foreach (var hideP in allHidePlaces)
        {
            var tempGnome = Instantiate(gnomeImagination);
            BoxCollider tempImaginationGnome = tempGnome.GetComponent<BoxCollider>();
            tempImaginationGnome.transform.position = hideP.transform.position;
            Vector3 playerHeading = Camera.main.transform.position - hideP.transform.position;
            Vector3 lookRotation = Quaternion.LookRotation(playerHeading, Vector3.up).eulerAngles;
            tempImaginationGnome.transform.rotation = Quaternion.Euler(new Vector3(0, lookRotation.y, 0));

            Vector3 size = tempImaginationGnome.size;
            Vector3 center = new Vector3(tempImaginationGnome.center.x, tempImaginationGnome.center.y, tempImaginationGnome.center.z);

            List<Vector3> rayTargets = CreateRayTargets(size, center, tempImaginationGnome);


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
                    var dwarfHit = hit.collider.gameObject.layer == LayerMask.NameToLayer("GnomeImagination");
                    if (dwarfHit)
                    {
                        Debug.DrawRay(camPos, heading, Color.green);
                    }
                    else
                    {
                        Debug.DrawRay(camPos, heading, Color.red);
                    }
                    return dwarfHit;
                }
            });
            var distanceToHide = Vector3.Distance(hideP.transform.position, transform.position);
            hideResults.Add(new HideResult(hideP, hits.Count(), distanceToHide));
            Destroy(tempGnome);
        }
        

        return hideResults;
    }

    private bool MoveTo(bool near, bool blocked)
    {
        try
        {
            List<HideResult> newHideLocation;
            if (blocked)
            {
                newHideLocation = ImagineGnomeLocations()
                    .Where(s => s.NumberOfHits == 0)
                    .ToList();
                Debug.Log("move to blocked area" + newHideLocation.Count());
            }
            else
            {
                newHideLocation = ImagineGnomeLocations()
                    .Where(s => s.NumberOfHits > 3)
                    .ToList();
            }

            if (newHideLocation.Count() == 0)
            {
                Debug.LogError("Cannot find hide Blocked " + blocked);
            }

            if (near)
            {
                agentTargetDestination = newHideLocation
                   .OrderBy(s => s.Distance)
                   .Take(2)
                   .OrderBy(s => Guid.NewGuid())
                   .FirstOrDefault()
                   .Target;
            }
            else
            {
                agentTargetDestination = newHideLocation
                   .OrderByDescending(s => s.Distance)
                   .Take(2)
                   .OrderBy(s => Guid.NewGuid())
                   .FirstOrDefault()
                   .Target;
            }

            agent.SetDestination(agentTargetDestination.transform.position);
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

    public void HideNow()
    {
        if (State == GnomeStates.PlayerContact)
        {
            State = GnomeStates.FindHide;
        }
        
    }

    private void Update()
    {

        #region update path
        if (updatePath <= 0)
        {
            updatePath = 1f;
            if (State == GnomeStates.Flee)
            {
                NavMeshPath path = agent.path;
                var pathPossible = agent.CalculatePath(agentTargetDestination.transform.position, path);
                if (pathPossible)
                {
                    Debug.Log("Calculated new path");
                    agent.path = path;
                }
                else
                {
                    Debug.LogWarning("Failed to calculate path");
                    IsVisible = false;
                }
            }
            
        }
        updatePath -= Time.deltaTime;
        #endregion

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

        if (agentTargetDestination == null)
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
                if (!MoveTo(true, true))
                {
                    IsVisible = false;
                    Debug.LogError("Unable to find hide");
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
                var change = FreezPosition - GetCameraDistance();
                agent.speed += Time.deltaTime * Math.Abs(change);
                if (!ImInCamera() || !spriteRenderer.isVisible || ImBehindObject())
                {
                    IsVisible = false;
                    MoveTo(false, true);
                    State = GnomeStates.MoveHidden;
                    agent.speed = agentSpeed;
                }
                else
                {
                    if (agent.remainingDistance < 0.2f)
                    {
                        State = GnomeStates.FindHide;
                    }
                }
                break;
            case GnomeStates.InHideHidden:
                MoveTo(true, false);
                State = GnomeStates.MovingToPeekLocation;
                break;
            case GnomeStates.MovingToPeekLocation:
                if (agent.remainingDistance < 0.2f)
                {
                    State = GnomeStates.Peek;
                }
                break;
            case GnomeStates.Peek:
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

    private GameObject FindNearestPeek()
    {
        throw new NotImplementedException();
    }
}


public class HideResult {
    public GameObject Target;
    public int NumberOfHits;
    public float Distance;

    public HideResult(GameObject target, int numberOfHits, float distance)
    {
        Target = target;
        NumberOfHits = numberOfHits;
        Distance = distance;
    }
}
