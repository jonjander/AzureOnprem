using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Assets.Azure.Lock
{
    [Serializable]
    public class Lock
    {
        [JsonProperty(PropertyName = "id")]
        public string Id
        {
            get; set;
        }
        [JsonProperty(PropertyName = "name")]
        public string Name
        {
            get; set;
        }
        [JsonProperty(PropertyName = "type")]
        public string Type
        {
            get; set;
        }
        [JsonProperty(PropertyName = "kind")]
        public string Kind
        {
            get; set;
        }
        [JsonProperty(PropertyName = "location")]
        public string Location
        {
            get; set;
        }
    }
}
