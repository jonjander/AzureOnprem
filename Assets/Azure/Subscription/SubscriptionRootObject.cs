using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Assets.Azure.Subscription
{
    
    public class SubscriptionRootObject
    {
        [JsonProperty(PropertyName = "value")]
        public List<Subscription> Value { get; set; }
    }
}
