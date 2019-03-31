using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace Assets.Azure.Resource
{
    [Serializable]
    public class ResourceObjectRootObject
    {
        [JsonProperty(PropertyName = "value")]
        public List<ResourceObject> Value { get; set; }
    }
}
