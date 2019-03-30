using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Assets.Azure.Subscription
{
    public class SubscriptionPolicies
    {
        [JsonProperty(PropertyName = "locationPlacementId")]
        public string LocationPlacementId { get; set; }
        [JsonProperty(PropertyName = "quotaId")]
        public string QuotaId { get; set; }
        [JsonProperty(PropertyName = "spendingLimit")]
        public string SpendingLimit { get; set; }
    }
}
