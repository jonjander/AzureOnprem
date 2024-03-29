using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CabelGenerator : MonoBehaviour
{
    public GameObject StartPoint;
    public GameObject EndPoint;
    public float CableLength = 0;
    public int NumberOfSegments;
    public GameObject Segment;

    private List<GameObject> segments;
    private ConfigurableJoint endJoint;
    private float segmentLen;
    private List<ConfigurableJoint> endJoints;

    // Use this for initialization
    private void Start()
    {
        segmentLen = 0.187f;
        endJoints = EndPoint.GetComponentsInChildren<ConfigurableJoint>()
            .ToList();
        endJoint = endJoints[0];
        segments = new List<GameObject>();
        CableLength = Vector3.Distance(StartPoint.transform.position, EndPoint.transform.position);
        NumberOfSegments = (int)Mathf.Round(CableLength / segmentLen);
        segmentLen = (CableLength / NumberOfSegments) * 1.01f;

        var heading = EndPoint.transform.position - StartPoint.transform.position;
        var distance = heading.magnitude;
        var direction = heading / distance; // This is now the normalized direction.

        for (int i = 0; i < NumberOfSegments; i++)
        {
            var tmpSegment = Instantiate(Segment, transform);
            tmpSegment.GetComponent<Rigidbody>().isKinematic = true;
            tmpSegment.transform.position = StartPoint.transform.position + (direction * (i * segmentLen));
            tmpSegment.transform.rotation = Quaternion.LookRotation(direction, Vector3.up) * Quaternion.Euler(0, 90, 0); ;
            var hj = tmpSegment.GetComponent<ConfigurableJoint>();
            if (segments.Count == 0)
            {
                hj.connectedBody = StartPoint.GetComponent<Rigidbody>();
            }
            else
            {
                hj.autoConfigureConnectedAnchor = false;
                hj.connectedBody = segments[segments.Count - 1].GetComponent<Rigidbody>();
                hj.connectedAnchor = new Vector3(-segmentLen, 0, 0);// tmpSegment.transform.position + 
            }
            var rb = tmpSegment.GetComponent<Rigidbody>();
            rb.maxDepenetrationVelocity = 0.001f;
            rb.maxAngularVelocity = 0.02f;
            segments.Add(tmpSegment);
        }
        endJoint.connectedBody = segments[segments.Count - 1].GetComponent<Rigidbody>();
        foreach (GameObject item in segments)
        {
            item.GetComponent<Rigidbody>().isKinematic = false;
        }
    }

    public void DetachCable()
    {
        EndPoint.transform.parent = transform;
    }

    // Update is called once per frame
    private void Update()
    {

    }
}
