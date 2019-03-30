using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace Assets.Azure
{
    [Serializable]
    public class ResourceGroupProperties
    {
        [JsonProperty(PropertyName = "provisioningState")]
        public string ProvisioningState { get; set; }
    }
}
