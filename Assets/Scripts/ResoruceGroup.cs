using Assets.Azure;
using Assets.Azure.Resource;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class ResoruceGroup : MonoBehaviour
{
    public string Id;
    public string Location;
    public ResourceGroupProperties Properties;
    public Tags Tags;
    public List<List<ResourceObject>> Servers;
    public bool ResourcesObjectsLoaded = false;

    private List<GameObject> resources;
    private float top;
    private float step;
    private int maxUSize;
    private TextMesh textmesh;
    private Rigidbody resourceGroupRigidbody;
    public void SetInfo(ResourceGroupObject rg)
    {
        name = rg.Name;
        Id = rg.Id;
        Location = rg.Location;
    }

    public ResoruceGroup()
    {
        //non parameter constructor for json
    }

    public void SetText()
    {
        textmesh = GetComponentInChildren<TextMesh>();
        textmesh.text = name;
    }



    private void Start()
    {
        maxUSize = 25;
        top = 1.9f;
        step = 0.071f;
        resourceGroupRigidbody = GetComponent<Rigidbody>();
    }

    public void InstanciateResources(List<GameObject> resourceTemplates)
    {
        if (ResourcesObjectsLoaded)
        {
            resources = new List<GameObject>();

            var i = 0;
            foreach (var item in Servers)
            {
                GameObject resourceTemplate = FindServerTemplate(resourceTemplates, item);

                //Test if fit
                var templateSize = resourceTemplate.GetComponent<Resource>().USize;
                if (templateSize + i > maxUSize)
                {
                    Debug.LogError("Cannot fit server into rack");
                }
                else
                {
                    var tempResource = Instantiate(resourceTemplate);
                    var script = tempResource.GetComponent<Resource>();

                    tempResource.transform.position = transform.position + new Vector3(0, top - (script.USize * step) - (step * i), 0);
                    tempResource.transform.rotation = transform.rotation;
                    tempResource.name = item.First().Name;

                    script.RefObject.AddRange(item);
                    script.Load();

                    List<Rigidbody> poles = GetComponentsInChildren<Rigidbody>().Where(s => s.gameObject.tag == "Pole")
                        .ToList();
                    var polesFixedJoints = tempResource.GetComponentsInChildren<FixedJoint>();
                    for (int f = 0; f < polesFixedJoints.Length; f++)
                    {
                        polesFixedJoints[f].connectedBody = poles[f];
                    }

                    resources.Add(tempResource);
                    i += script.USize;
                }
            }
        }
        
    }

    private int GetTotalUSize(List<GameObject> resourceTemplates, List<List<ResourceObject>> resourceItems)
    {
        var totalUs = 0;
        foreach (var serverResources in resourceItems)
        {
            var serverTemplate = FindServerTemplate(resourceTemplates, serverResources);
            var serverScript = serverTemplate.GetComponent<Resource>();
            totalUs += serverScript.USize;
        }

        return totalUs;
    }

    private static GameObject FindServerTemplate(List<GameObject> resourceTemplates, List<ResourceObject> resourceItems)
    {
        //Find blades > 10

        //Select type
        GameObject resourceTemplate = resourceTemplates
            .Where(s =>
            {
                var resourceScript = s.GetComponent<Resource>();
                var matchingType = resourceScript.Types.Any(u => u == resourceItems.First().Type);
                var matchingCapacity = resourceScript.Capacity >= resourceItems.Count();
                return matchingType && matchingCapacity;
            })
            .OrderBy(s=> {
                var resourceScript = s.GetComponent<Resource>();
                return resourceScript.Capacity;
            })
            .FirstOrDefault();
        if (resourceTemplate == null) //If type not exist
        {
            resourceTemplate = resourceTemplates
            .Where(s =>
            {
                var resourceScript = s.GetComponent<Resource>();
                var matchingType = resourceScript.Types.Any(u => u == "Dummy");
                var matchingCapacity = resourceScript.Capacity >= resourceItems.Count();
                return matchingType && matchingCapacity;
            })
            .OrderBy(s => {
                var resourceScript = s.GetComponent<Resource>();
                return resourceScript.Capacity;
            })
            .FirstOrDefault();
        }
        if (resourceTemplate == null) //if still null take biggest
        {
            Debug.LogError("Groups to big");
            resourceTemplate = resourceTemplates
            .Where(s =>
            {
                var resourceScript = s.GetComponent<Resource>();
                var matchingType = resourceScript.Types.Any(u => u == "Dummy");
                return matchingType;
            })
            .OrderByDescending(s => {
                var resourceScript = s.GetComponent<Resource>();
                return resourceScript.Capacity;
            })
            .FirstOrDefault();
        }

        return resourceTemplate;
    }

    IEnumerator GetResoruceGroupResources(string userAccessToken, string subscription, List<GameObject> serverKinds)
    {
        string azureUrl = "https://management.azure.com/subscriptions/" + subscription + "/resourceGroups/" + name + "/resources?api-version=2017-05-10";
        UnityWebRequest request = UnityWebRequest.Get(azureUrl);

        request.method = UnityWebRequest.kHttpVerbGET;
        request.SetRequestHeader("Content-Type", "application/json; utf-8");
        request.SetRequestHeader("Authorization", userAccessToken);

        yield return request.SendWebRequest();
        var jsonString = request.downloadHandler.text;
        var resources = JsonConvert.DeserializeObject<ResourceObjectRootObject>(jsonString).Value;
        Servers = ConvertResourcesToServers(serverKinds, resources);

        ResourcesObjectsLoaded = true;
        InstanciateResources(serverKinds);
    }

    private List<List<ResourceObject>> ConvertResourcesToServers(List<GameObject> serverKinds, List<ResourceObject> resourceObects)
    {
        List<string> groupableResources = new List<string>()
        {
            "Microsoft.Compute/disks",
            "Microsoft.Automation/automationAccounts/runbooks",
            "Microsoft.Web/certificates",
            "microsoft.insights/actiongroups",
            "microsoft.insights/scheduledqueryrules",
            "Microsoft.OperationsManagement/solutions"
        };

        var tmpServerResources = new List<List<ResourceObject>>();

        var groupedResources = resourceObects
            .Where(r => groupableResources.Any(s => s == r.Type))
            .GroupBy(t=>t.Type, (key, g) => new { KeyType = key, Resources = g.ToList() });

        var normalResources = resourceObects
            .Where(r => !groupableResources.Any(s => s == r.Type));
        
        var extractedGroupedResources = groupedResources
            .Select(g => g.Resources).ToList();

        var extractedNormalResources = normalResources.Select(res => {
            return new List<ResourceObject>
            {
                res
            };
        });

        //Add grouped lists
        tmpServerResources.AddRange(extractedGroupedResources);
        tmpServerResources.AddRange(extractedNormalResources);

        //Test max size
        if (GetTotalUSize(serverKinds, tmpServerResources) > maxUSize)
        {
            //Group all
            var allTypesGrouped = resourceObects
            .GroupBy(t => t.Type, (key, g) => new { KeyType = key, Resources = g.ToList() });
            extractedGroupedResources = groupedResources
                .Select(g => g.Resources).ToList();
            var tempReturn = new List<List<ResourceObject>>();
            tempReturn.AddRange(extractedGroupedResources);
            return tempReturn;
        }
        else
        {
            return tmpServerResources;
        }
    }

    public void RemoveHides()
    {
        var hides = GetComponentsInChildren<HideArea>().ToList();
        foreach (var item in hides)
        {
            item.DestroyHide();
        }
    }

    private void Update()
    {
                
    }

    internal void Load(string accessToken, string subscription, List<GameObject> serverKinds)
    {
        StartCoroutine(GetResoruceGroupResources(accessToken, subscription, serverKinds));
    }
}

