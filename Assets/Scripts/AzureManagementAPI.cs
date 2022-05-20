using Assets.Azure.Subscription;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Assets.Scripts
{
    public static class AzureManagementAPIHelper
    {
        public static List<Subscription> Subscriptions = new List<Subscription>();
        public delegate void SubscriptionLoadedAction(List<Subscription> loadedSubscriptions);
        public static event SubscriptionLoadedAction OnSubscriptionLoaded;
        public static IEnumerator GetSubscriptions(string userAccessToken)
        {
            var apiUrl = "https://management.azure.com/subscriptions?api-version=2014-04-01";

            UnityWebRequest request = UnityWebRequest.Get(apiUrl);

            request.method = UnityWebRequest.kHttpVerbGET;
            request.SetRequestHeader("Content-Type", "application/json; utf-8");
            request.SetRequestHeader("Authorization", userAccessToken);

            yield return request.SendWebRequest();
            var jsonString = request.downloadHandler.text;
            Subscriptions = JsonConvert.DeserializeObject<SubscriptionRootObject>(jsonString).Value;
            OnSubscriptionLoaded(Subscriptions);
        }
    }
}
