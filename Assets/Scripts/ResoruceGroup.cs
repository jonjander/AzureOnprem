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
    public ResourceGroupProperties properties;
    private TextMesh Textmesh;
    public Tags tags { get; set; }

    private List<GameObject> Resources;
    public List<List<ResourceObject>> Servers { get; set; }

    public bool ResourcesObjectsLoaded = false;

    float Top = 1.9f;
    float Step = 0.071f;

    int maxUSize = 25;

    private Rigidbody RGRigidbody;
    public void SetInfo(ResourceGroupObject rg)
    {
        name = rg.name;
        Id = rg.id;
        Location = rg.location;
    }

    public ResoruceGroup()
    {

    }

    public void SetText()
    {
        Textmesh = GetComponentInChildren<TextMesh>();
        Textmesh.text = name;
    }



    private void Start()
    {
        RGRigidbody = GetComponent<Rigidbody>();
    }

    public void InstanciateResources(List<GameObject> ResourceTemplates)
    {
        if (ResourcesObjectsLoaded)
        {
            Resources = new List<GameObject>();

            var i = 0;
            foreach (var item in Servers)
            {
                GameObject ResourceTemplate = FindServerTemplate(ResourceTemplates, item);

                //Test if fit
                var templateSize = ResourceTemplate.GetComponent<Resource>().uSize;
                if (templateSize + i > maxUSize)
                {
                    Debug.LogError("Cannot fit server into rack");
                }
                else
                {
                    var tempResource = Instantiate(ResourceTemplate);
                    var script = tempResource.GetComponent<Resource>();

                    tempResource.transform.position = transform.position + new Vector3(0, Top - (script.uSize * Step) - (Step * i), 0);
                    tempResource.transform.rotation = transform.rotation;
                    tempResource.name = item.First().name;

                    script.refObject.AddRange(item);
                    script.Load();

                    List<Rigidbody> poles = GetComponentsInChildren<Rigidbody>().Where(s => s.gameObject.tag == "Pole")
                        .ToList();
                    var FJs = tempResource.GetComponentsInChildren<FixedJoint>();
                    for (int f = 0; f < FJs.Length; f++)
                    {
                        FJs[f].connectedBody = poles[f];
                    }

                    Resources.Add(tempResource);
                    i += script.uSize;
                }
            }
        }
        
    }

    private int GetTotalUSize(List<GameObject> ResourceTemplates, List<List<ResourceObject>> ResourceItems)
    {
        var totalUs = 0;
        foreach (var serverResources in ResourceItems)
        {
            var serverTemplate = FindServerTemplate(ResourceTemplates, serverResources);
            var serverScript = serverTemplate.GetComponent<Resource>();
            totalUs += serverScript.uSize;
        }

        return totalUs;
    }

    private static GameObject FindServerTemplate(List<GameObject> ResourceTemplates, List<ResourceObject> ResourceItems)
    {
        //Find blades > 10

        //Select type
        var ResourceTemplate = ResourceTemplates
            .Where(s =>
            {
                var resourceScript = s.GetComponent<Resource>();
                var matchingType = resourceScript.Types.Any(u => u == ResourceItems.First().type);
                var matchingCapacity = resourceScript.Capacity >= ResourceItems.Count();
                return matchingType && matchingCapacity;
            })
            .OrderBy(s=> {
                var resourceScript = s.GetComponent<Resource>();
                return resourceScript.Capacity;
            })
            .FirstOrDefault();
        if (ResourceTemplate == null) //If type not exist
        {
            ResourceTemplate = ResourceTemplates
            .Where(s =>
            {
                var resourceScript = s.GetComponent<Resource>();
                var matchingType = resourceScript.Types.Any(u => u == "Dummy");
                var matchingCapacity = resourceScript.Capacity >= ResourceItems.Count();
                return matchingType && matchingCapacity;
            })
            .OrderBy(s => {
                var resourceScript = s.GetComponent<Resource>();
                return resourceScript.Capacity;
            })
            .FirstOrDefault();
        }
        if (ResourceTemplate == null) //if still null take biggest
        {
            Debug.LogError("Groups to big");
            ResourceTemplate = ResourceTemplates
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

        return ResourceTemplate;
    }

    IEnumerator GetResoruceGroupResources(string UserAccessToken, string subscription, List<GameObject> ServerKinds)
    {
        string AzureUrl = "https://management.azure.com/subscriptions/" + subscription + "/resourceGroups/" + name + "/resources?api-version=2017-05-10";
        UnityWebRequest Request = UnityWebRequest.Get(AzureUrl);

        Request.method = UnityWebRequest.kHttpVerbGET;
        Request.SetRequestHeader("Content-Type", "application/json; utf-8");
        Request.SetRequestHeader("Authorization", UserAccessToken);

        yield return Request.SendWebRequest();
        var JsonString = Request.downloadHandler.text;
        var resources = JsonConvert.DeserializeObject<ResourceObjectRootObject>(JsonString).value;
        Servers = ConvertResourcesToServers(ServerKinds, resources);

        ResourcesObjectsLoaded = true;
        InstanciateResources(ServerKinds);
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

        var GroupedResources = resourceObects
            .Where(r => groupableResources.Any(s => s == r.type))
            .GroupBy(t=>t.type, (key, g) => new { KeyType = key, Resources = g.ToList() });

        var NormalResources = resourceObects
            .Where(r => !groupableResources.Any(s => s == r.type));
        
        var ExtractedGroupedResources = GroupedResources
            .Select(g => g.Resources).ToList();

        var ExtractedNormalResources = NormalResources.Select(res => {
            return new List<ResourceObject>
            {
                res
            };
        });

        //Add grouped lists
        tmpServerResources.AddRange(ExtractedGroupedResources);
        tmpServerResources.AddRange(ExtractedNormalResources);

        //Test max size
        if (GetTotalUSize(serverKinds, tmpServerResources) > maxUSize)
        {
            //Group all
            var allTypesGrouped = resourceObects
            .GroupBy(t => t.type, (key, g) => new { KeyType = key, Resources = g.ToList() });
            ExtractedGroupedResources = GroupedResources
                .Select(g => g.Resources).ToList();
            var tempReturn = new List<List<ResourceObject>>();
            tempReturn.AddRange(ExtractedGroupedResources);
            return tempReturn;
        }
        else
        {
            return tmpServerResources;
        }
    }

    public void RemoveHides()
    {
        var Hides = GetComponentsInChildren<HideArea>().ToList();
        foreach (var item in Hides)
        {
            item.DestroyHide();
        }
    }

    private void Update()
    {
                
    }

    internal void Load(string AccessToken, string Subscription, List<GameObject> ServerKinds)
    {
        StartCoroutine(GetResoruceGroupResources(AccessToken, Subscription, ServerKinds));
    }
}

