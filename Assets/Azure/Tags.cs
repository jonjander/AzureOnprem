using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace Assets.Azure
{
    [Serializable]
    public class Tags
    {
        [JsonProperty(PropertyName = "displayName")]
        public string DisplayName { get; set; }
        [JsonProperty(PropertyName = "environment")]
        public string Environment { get; set; }
        [JsonProperty(PropertyName = "iLo")]
        public string Ilo { get; set; }
    }
}
