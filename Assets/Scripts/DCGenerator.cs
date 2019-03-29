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
    public int col = 4;
    public int row = 0;
    public float xOffset = 4f;
    public float zOffset = 3f;
    public string AccessToken = "";
    public string Subscription = "";
    public List<GameObject> ServerKinds;

    List<ResourceGroupObject> ResoruceGroups;
    public bool dataIsLoaded = false;
    public GameObject Wall;
    public GameObject Door;
    public GameObject Light;
    public GameObject Cable;
    public GameObject CableLadder;
    public NavMeshSurface navMeshSurface;

    private AzureManagementAPIHelper azureManagementAPIHelper;

    // Use this for initialization
    void Start () {
        azureManagementAPIHelper = new AzureManagementAPIHelper();
        AdminScreen.OnComputerLogin += GenerateDataCenterResources;
    }

    Vector3[] GetRackBounds()
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
        var Bounds = GetRackBounds();
        var offset = 3;
        var start = Bounds[0] - new Vector3(offset, 0, offset);
        var XWall = Mathf.Round(Bounds[1].x - Bounds[0].x) + (offset * 2);
        var ZWall = Mathf.Round(Bounds[1].z - Bounds[0].z) + (offset * 2);

        for (int x = 0; x < XWall; x++)
        {
            var tempWall = Instantiate(Wall);
            tempWall.transform.position = start;
            start += new Vector3(1, 0, 0);
        }

        for (int x = 0; x < ZWall; x++)
        {
            var tempWall = Instantiate(Wall);
            tempWall.transform.position = start;
            tempWall.transform.Rotate(Vector3.up, -90);
            start += new Vector3(0, 0, 1);
        }

        for (int x = 0; x < XWall; x++)
        {
            var tempWall = Instantiate(Wall);
            tempWall.transform.position = start;
            tempWall.transform.Rotate(Vector3.up, 180);
            start += new Vector3(-1, 0, 0);
        }

        for (int x = 0; x < ZWall; x++)
        {
            GameObject tempWall;
            if (x == Mathf.Floor(ZWall/2) || x == Mathf.Floor(ZWall / 2) + 1)
            {
                tempWall = Instantiate(Door);
            }
            else {
                tempWall = Instantiate(Wall);
            }
            tempWall.transform.position = start;
            tempWall.transform.Rotate(Vector3.up, 90);
            start += new Vector3(0, 0, -1);
        }

        //Lights
        start = Bounds[0]; //Reset start
        for (int z = 0; z < row + 2; z++)
        {
            if (z % 1 == 0)
            {
                var tempLight = Instantiate(Light);
                var x = (start.x + XWall + (offset * 2)) / 2;
                x = (Bounds[1].x + Bounds[0].x) / 2;
                Mesh mesh = tempLight.GetComponentInChildren<MeshFilter>().mesh;
                var exrents = mesh.bounds.extents;
                tempLight.transform.position = new Vector3(x + exrents.x, 3, start.z + (zOffset / 2) + (zOffset / 4));
                var LampScript = tempLight.GetComponent<FluorescentLamp>();
                LampScript.isBroken = !!(UnityEngine.Random.Range(0, row + 3) == 1);
            }
            start += new Vector3(0, 0, zOffset);
        }

        //CableLadder
        start = Bounds[0] - new Vector3(offset, 0, offset); //Reset start
        while (start.z <= Bounds[1].z)
        {
            var tempCableLadder = Instantiate(CableLadder);
            var clRenderer = tempCableLadder.GetComponentInChildren<MeshRenderer>();
            var clBounds = clRenderer.bounds.size;
            var x = (start.x + (offset * 2));
            tempCableLadder.transform.position = start + new Vector3(x, 0, 0);
            start += new Vector3(0, 0, clBounds.z);
        }
    }
 
    public void doLogin()
    {
        AccessToken = LoginHelper.GetToken();
        StartCoroutine(azureManagementAPIHelper.GetSubscriptions(AccessToken));
    }

    public void GenerateDataCenterResources(string selectedSubscriptionId)
    {
        dataIsLoaded = false;
        Subscription = selectedSubscriptionId;
        StartCoroutine(GetResourceGroups(AccessToken, Subscription));
    }

    // Update is called once per frame
    void Update () {
        
        if (dataIsLoaded)
        {
            dataIsLoaded = false;
            Racks = new List<GameObject>();
            int iCol = 0, iRow = 0;
            foreach (var item in ResoruceGroups)
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
                    var Center = tmp.GetComponentsInChildren<Collider>()
                        .Where(s => s.gameObject.tag == "RackFloor")
                        .FirstOrDefault().bounds.center;

                    tmp.transform.RotateAround(Center, Vector3.up, 180);
                }
                
                var script = tmp.GetComponent<ResoruceGroup>();
                script.SetInfo(item);
                script.SetText();
                script.Load(AccessToken, Subscription, ServerKinds);
                Racks.Add(tmp);
                

                iCol++;
            }
            DrawDatacenter();
            ConnectCables(Racks);
            UpdateMavMesh();
        }
    }

    private void UpdateMavMesh()
    {
        navMeshSurface.BuildNavMesh();
    }

    private void ConnectCables(List<GameObject> racks)
    {
        racks = racks.OrderBy(s => Guid.NewGuid()).Take((int)(racks.Count() / 2.5f)).ToList();
        foreach (var item in racks)
        {
            //Connect Cables
            var cable = Instantiate(Cable);
            var cabelGeneratorScript = cable.GetComponent<cabelGenerator>();
            cabelGeneratorScript.startPoint.GetComponent<Rigidbody>().isKinematic = true;
            var closestConnector = FindClosestCableConnection(item.transform);
            cabelGeneratorScript.startPoint.transform.position = closestConnector.transform.position;
            var endConnectionJoint = cabelGeneratorScript.endPoint.GetComponent<ConfigurableJoint>();
            GameObject TopOfRack = new List<GameObject>(GameObject.FindGameObjectsWithTag("RackTop")).Find(g => g.transform.IsChildOf(item.transform));
            GameObject TopOfRackConnector = new List<GameObject>(GameObject.FindGameObjectsWithTag("CabelConnector")).Find(g => g.transform.IsChildOf(item.transform));
            cabelGeneratorScript.endPoint.transform.position = TopOfRackConnector.transform.position;
            endConnectionJoint.connectedBody = TopOfRack.GetComponent<Rigidbody>();
            endConnectionJoint.anchor = Vector3.zero;
        }
    }

    IEnumerator GetResourceGroups(string UserAccessToken, string subscription)
    {
        string AzureUrl = "https://management.azure.com/subscriptions/" + subscription + "/resourceGroups?api-version=2014-04-01";
        UnityWebRequest Request = UnityWebRequest.Get(AzureUrl);

        Request.method = UnityWebRequest.kHttpVerbGET;
        Request.SetRequestHeader("Content-Type", "application/json; utf-8");
        Request.SetRequestHeader("Authorization", UserAccessToken);

        yield return Request.SendWebRequest();
        var JsonString = Request.downloadHandler.text;
        ResoruceGroups = JsonConvert.DeserializeObject<ResourceGroupRootObject>(JsonString).value;
        dataIsLoaded = true;
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


public static class DB{
    public static string MocData()
    {
        return "{  \"value\": [    {      \"id\": \"/subscriptions/09b5a73e-6bc4-4a9c-a0a3-0656665ae3b1/resourceGroups/BroadcastBotBot\",      \"name\": \"BroadcastBotBot\",      \"location\": \"westeurope\",      \"properties\": {        \"provisioningState\": \"Succeeded\"      }    },    {      \"id\": \"/subscriptions/09b5a73e-6bc4-4a9c-a0a3-0656665ae3b1/resourceGroups/CADYC2\",      \"name\": \"CADYC2\",      \"location\": \"northeurope\",      \"properties\": {        \"provisioningState\": \"Succeeded\"      }    },    {      \"id\": \"/subscriptions/09b5a73e-6bc4-4a9c-a0a3-0656665ae3b1/resourceGroups/casperpos\",      \"name\": \"casperpos\",      \"location\": \"westeurope\",      \"properties\": {        \"provisioningState\": \"Succeeded\"      }    },    {      \"id\": \"/subscriptions/09b5a73e-6bc4-4a9c-a0a3-0656665ae3b1/resourceGroups/cloud-shell-storage-westeurope\",      \"name\": \"cloud-shell-storage-westeurope\",      \"location\": \"westeurope\",      \"properties\": {        \"provisioningState\": \"Succeeded\"      }    },    {      \"id\": \"/subscriptions/09b5a73e-6bc4-4a9c-a0a3-0656665ae3b1/resourceGroups/Default-Storage-CentralUS\",      \"name\": \"Default-Storage-CentralUS\",      \"location\": \"centralus\",      \"properties\": {        \"provisioningState\": \"Succeeded\"      }    },    {      \"id\": \"/subscriptions/09b5a73e-6bc4-4a9c-a0a3-0656665ae3b1/resourceGroups/fettonu\",      \"name\": \"fettonu\",      \"location\": \"westeurope\",      \"properties\": {        \"provisioningState\": \"Succeeded\"      }    },    {      \"id\": \"/subscriptions/09b5a73e-6bc4-4a9c-a0a3-0656665ae3b1/resourceGroups/RQII\",      \"name\": \"RQII\",      \"location\": \"westeurope\",      \"properties\": {        \"provisioningState\": \"Succeeded\"      }    },    {      \"id\": \"/subscriptions/09b5a73e-6bc4-4a9c-a0a3-0656665ae3b1/resourceGroups/securitydata\",      \"name\": \"securitydata\",      \"location\": \"eastus\",      \"tags\": {},      \"properties\": {        \"provisioningState\": \"Succeeded\"      }    },    {      \"id\": \"/subscriptions/09b5a73e-6bc4-4a9c-a0a3-0656665ae3b1/resourceGroups/TBN\",      \"name\": \"TBN\",      \"location\": \"westeurope\",      \"properties\": {        \"provisioningState\": \"Succeeded\"      }    },    {      \"id\": \"/subscriptions/09b5a73e-6bc4-4a9c-a0a3-0656665ae3b1/resourceGroups/tweetmeasure\",      \"name\": \"tweetmeasure\",      \"location\": \"westeurope\",      \"properties\": {        \"provisioningState\": \"Succeeded\"      }    },    {      \"id\": \"/subscriptions/09b5a73e-6bc4-4a9c-a0a3-0656665ae3b1/resourceGroups/VS-lanmat-Group\",      \"name\": \"VS-lanmat-Group\",      \"location\": \"westeurope\",      \"properties\": {        \"provisioningState\": \"Succeeded\"      }    },    {      \"id\": \"/subscriptions/09b5a73e-6bc4-4a9c-a0a3-0656665ae3b1/resourceGroups/VSTS\",      \"name\": \"VSTS\",      \"location\": \"westeurope\",      \"properties\": {        \"provisioningState\": \"Succeeded\"      }    }  ]}";
    }
}