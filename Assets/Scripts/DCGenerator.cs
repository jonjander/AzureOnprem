using Assets;
using Assets.Azure;
using Assets.Azure.Resource;
using Assets.Scripts;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Networking;

public class DCGenerator : MonoBehaviour {

    public List<GameObject> Racks;
    public GameObject RackTemplate;
    
    public string AccessToken = "";
    public string Subscription = "";
    public List<GameObject> ServerKinds;
    public bool DataIsLoaded = false;
    public GameObject Wall;
    public GameObject Door;
    public GameObject Light;
    public GameObject CableLadder;
    public NavMeshSurface NavMeshSurface;
    public GameObject DataCenterDoor;

    private AzureManagementAPIHelper azureManagementAPIHelper;
    private List<ResourceGroupObject> resoruceGroups;
    private int col;
    private int row;
    private float xOffset;
    private float zOffset;
    private int maximumNumberOfCables;
    private List<GameObject> dataCenterProps;
    private List<GameObject> cableList;
    private bool isClean;

    // Use this for initialization
    void Start () {
        dataCenterProps = new List<GameObject>();
        cableList = new List<GameObject>();
        maximumNumberOfCables = 20;
        azureManagementAPIHelper = new AzureManagementAPIHelper();
        AdminScreen.OnComputerLogin += GenerateDataCenterResources;
        col = 6;
        row = 0;
        xOffset = 1.04f;
        zOffset = 2.84f;
        isClean = true;
    }

    private Vector3[] GetRackBounds()
    {
        var maxX = Racks.Max(r => r.transform.position.x);
        var maxZ = Racks.Max(r => r.transform.position.z);
        var minX = Racks.Min(r => r.transform.position.x);
        var minZ = Racks.Min(r => r.transform.position.z);
        var bounds = new Vector3[2];
        bounds[0] = new Vector3(minX, 0, minZ);
        bounds[1] = new Vector3(maxX, 0, maxZ);
        return bounds;
    }

    public void DrawDatacenter()
    {
        var bounds = GetRackBounds();
        var offset = 3;
        var start = bounds[0] - new Vector3(offset, 0, offset);
        var xWall = Mathf.Round(bounds[1].x - bounds[0].x) + (offset * 2);
        var zWall = Mathf.Round(bounds[1].z - bounds[0].z) + (offset * 2);

        for (int x = 0; x < xWall; x++)
        {
            //Skip xWall
            start += new Vector3(1, 0, 0);
        }

        for (int x = 0; x < zWall; x++)
        {
            var tempWall = Instantiate(Wall);
            tempWall.transform.position = start;
            tempWall.transform.Rotate(Vector3.up, -90);
            start += new Vector3(0, 0, 1);
            dataCenterProps.Add(tempWall);
        }

        for (int x = 0; x < xWall; x++)
        {
            var tempWall = Instantiate(Wall);
            tempWall.transform.position = start;
            tempWall.transform.Rotate(Vector3.up, 180);
            start += new Vector3(-1, 0, 0);
            dataCenterProps.Add(tempWall);
        }

        for (int x = 0; x < zWall; x++)
        {
            //skip zwall
            start += new Vector3(0, 0, -1);
        }

        //Lights
        start = bounds[0]; //Reset start
        for (int z = 0; z < row + 2; z++)
        {
            var tempLight = Instantiate(Light);
            var x = (start.x + xWall + (offset * 2)) / 2;
            x = (bounds[1].x + bounds[0].x) / 2;
            Mesh mesh = tempLight.GetComponentInChildren<MeshFilter>().mesh;
            var exrents = mesh.bounds.extents;
            tempLight.transform.position = new Vector3(x + exrents.x, 3, start.z - zOffset + (zOffset / 2) + (zOffset / 4));
            var lampScript = tempLight.GetComponent<FluorescentLamp>();
            lampScript.isBroken = !!(UnityEngine.Random.Range(0, row + 3) == 1);
            
            start += new Vector3(0, 0, zOffset);
            dataCenterProps.Add(tempLight);
        }

        //CableLadder
        start = bounds[0] - new Vector3(offset, 0, offset); //Reset start
        while (start.z <= bounds[1].z)
        {
            var tempCableLadder = Instantiate(CableLadder);
            var clRenderer = tempCableLadder.GetComponentInChildren<MeshRenderer>();
            var clBounds = clRenderer.bounds.size;
            var x = (start.x + (offset * 2));
            tempCableLadder.transform.position = start + new Vector3(x, 0, 0);
            start += new Vector3(0, 0, clBounds.z);
            dataCenterProps.Add(tempCableLadder);
        }
    }
 
    public void DoLogin()
    {
        AccessToken = LoginHelper.GetToken();
        StartCoroutine(azureManagementAPIHelper.GetSubscriptions(AccessToken));
    }

    public void GenerateDataCenterResources(string selectedSubscriptionId)
    {
        DataIsLoaded = false;
        Subscription = selectedSubscriptionId;
        StartCoroutine(GetResourceGroups(AccessToken, Subscription));
    }

    // Update is called once per frame
    void Update () {
        
        if (DataIsLoaded && isClean)
        {
            DataIsLoaded = false;
            Racks = new List<GameObject>();
            int iCol = 0, iRow = 0;
            foreach (var item in resoruceGroups)
            {

                if (iCol == col)
                {
                    iCol = 0;
                    iRow++;
                    row = iRow;
                }
                var tmp = Instantiate(RackTemplate);
                tmp.transform.position = new Vector3(iCol * xOffset, 0, iRow * zOffset);


                if (iRow % 2 == 0)
                {
                    var center = tmp.GetComponentsInChildren<Collider>()
                        .Where(s => s.gameObject.tag == "RackFloor")
                        .FirstOrDefault().bounds.center;

                    tmp.transform.RotateAround(center, Vector3.up, 180);
                }
                
                var script = tmp.GetComponent<ResoruceGroup>();
                script.SetInfo(item);
                script.SetText();
                script.Load(AccessToken, Subscription, ServerKinds);
                Racks.Add(tmp);
                

                iCol++;
            }
            DestroyImmediate(DataCenterDoor);
            DrawDatacenter();
            ConnectCables(Racks);
            UpdateMavMesh();
            isClean = false;
        }
        if (DataIsLoaded && !isClean)
        {
            CleanDatacenter();
        }
    }

    private void CleanDatacenter()
    {
        foreach (var item in cableList)
        {
            item.GetComponent<CabelGenerator>().DetachCable();
            DestroyImmediate(item);
        }
        
        foreach (var item in dataCenterProps)
        {
            DestroyImmediate(item);
        }
        foreach (var item in Racks)
        {
            DestroyImmediate(item);
        }
        isClean = true;
    }

    private void UpdateMavMesh() => NavMeshSurface.BuildNavMesh();

    private void ConnectCables(List<GameObject> racks)
    {
        var totalCables = 0;
        var CablePrefab = Resources.Load<GameObject>("Cable");
        racks = racks.OrderBy(s => Guid.NewGuid()).Take(maximumNumberOfCables).ToList();
        foreach (GameObject item in racks)
        {
            if (totalCables <= maximumNumberOfCables)
            {
                //Connect Cables
                totalCables++;
                var tmpCable = Instantiate(CablePrefab);
                var cabelGeneratorScript = tmpCable.GetComponent<CabelGenerator>();
                cabelGeneratorScript.StartPoint.GetComponent<Rigidbody>().isKinematic = true;
                var closestConnector = FindClosestCableConnection(item.transform);
                cabelGeneratorScript.StartPoint.transform.position = closestConnector.transform.position;
                var endConnectionJoint = cabelGeneratorScript.EndPoint.GetComponents<ConfigurableJoint>()[1];
                GameObject topOfRack = new List<GameObject>(GameObject.FindGameObjectsWithTag("RackTop")).Find(g => g.transform.IsChildOf(item.transform));
                GameObject topOfRackConnector = new List<GameObject>(GameObject.FindGameObjectsWithTag("CabelConnector")).Find(g => g.transform.IsChildOf(item.transform));
                cabelGeneratorScript.EndPoint.transform.position = topOfRackConnector.transform.position;
                cabelGeneratorScript.EndPoint.transform.parent = topOfRackConnector.transform;
                endConnectionJoint.connectedBody = topOfRack.GetComponent<Rigidbody>();
                endConnectionJoint.anchor = Vector3.zero;
                cableList.Add(tmpCable);
            }
        }
    }

    IEnumerator GetResourceGroups(string userAccessToken, string subscription)
    {
        string azureUrl = "https://management.azure.com/subscriptions/" + subscription + "/resourceGroups?api-version=2014-04-01";
        UnityWebRequest request = UnityWebRequest.Get(azureUrl);

        request.method = UnityWebRequest.kHttpVerbGET;
        request.SetRequestHeader("Content-Type", "application/json; utf-8");
        request.SetRequestHeader("Authorization", userAccessToken);

        yield return request.SendWebRequest();
        var jsonString = request.downloadHandler.text;
        resoruceGroups = JsonConvert.DeserializeObject<ResourceGroupRootObject>(jsonString).Value;
        DataIsLoaded = true;
    }

    public GameObject FindClosestCableConnection(Transform origin)
    {
        GameObject[] gos;
        gos = GameObject.FindGameObjectsWithTag("CableConnector");
        Vector3 position = origin.position;

        var closestIsh = gos.Select(gObject =>
        {
            var distance = (gObject.transform.position - position).sqrMagnitude;
            return new { gObject, distance };
        }).OrderBy(s => s.distance)
        .Take(3)
        .OrderBy(s => Guid.NewGuid())
        .FirstOrDefault();

        return closestIsh.gObject;
    }


}