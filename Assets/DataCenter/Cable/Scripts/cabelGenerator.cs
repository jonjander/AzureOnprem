using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class cabelGenerator : MonoBehaviour
{

    public float segmentLen;
    public GameObject startPoint;
    public GameObject endPoint;
    public float CableLength = 0;
    public int NumberOfSegments;


    public GameObject segment;
    List<GameObject> Segments;

    HingeJoint EndJoint;

    // Use this for initialization
    void Start()
    {
        segmentLen = 0.187f;
        EndJoint = endPoint.GetComponent<HingeJoint>();
        Segments = new List<GameObject>();
        CableLength = Vector3.Distance(startPoint.transform.position, endPoint.transform.position);
        NumberOfSegments = (int)Mathf.Round(CableLength / segmentLen);
        segmentLen = (CableLength / NumberOfSegments) * 1.01f;

        var heading = endPoint.transform.position - startPoint.transform.position;
        var distance = heading.magnitude;
        var direction = heading / distance; // This is now the normalized direction.

        for (int i = 0; i < NumberOfSegments; i++)
        {
            var tmpSegment = Instantiate(segment, transform);
            tmpSegment.transform.position = startPoint.transform.position + (direction * (i * segmentLen));
            tmpSegment.transform.rotation = Quaternion.LookRotation(direction, Vector3.up) * Quaternion.Euler(0, 90, 0); ;
            var hj = tmpSegment.GetComponent<ConfigurableJoint>();
            if (Segments.Count == 0)
            {
                hj.connectedBody = startPoint.GetComponent<Rigidbody>();
            }
            else
            {
                hj.autoConfigureConnectedAnchor = false;
                hj.connectedBody = Segments[Segments.Count - 1].GetComponent<Rigidbody>();
                hj.connectedAnchor = new Vector3(-segmentLen, 0, 0);// tmpSegment.transform.position + 
            }
            var rb = tmpSegment.GetComponent<Rigidbody>();
            rb.maxDepenetrationVelocity = 0.001f;
            rb.maxAngularVelocity = 0.02f;
            Segments.Add(tmpSegment);
        }
        EndJoint.connectedBody = Segments[Segments.Count - 1].GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {

    }
}
