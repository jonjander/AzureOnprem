using Assets.Azure.Resource;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Azure
{
    [Serializable]
    public class ResourceGroupObject
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
        [JsonProperty(PropertyName = "location")]
        public string Location { get; set; }
        [JsonProperty(PropertyName = "properties")]
        public ResourceGroupProperties Properties { get; set; }
        [JsonProperty(PropertyName = "tags")]
        public Tags Tags { get; set; }

        
        public ResourceGroupObject()
        {

        }
    }
}
