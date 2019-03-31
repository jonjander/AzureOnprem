using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace Assets.Azure
{
    [Serializable]
    public class ResourceGroupRootObject
    {
        [JsonProperty(PropertyName = "value")]
        public List<ResourceGroupObject> Value { get; set; }
    }
}
