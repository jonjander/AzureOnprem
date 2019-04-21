using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Assets.Azure.Lock
{
    [Serializable]
    public class LockRoot
    {
        [JsonProperty(PropertyName = "value")]
        public List<Lock> Value { get; set; }
    }
}
