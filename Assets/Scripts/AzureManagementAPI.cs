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
    public class AzureManagementAPIHelper : MonoBehaviour
    {
        public List<Subscription> Subscriptions;
        public delegate void SubscriptionLoadedAction(List<Subscription> loadedSubscriptions);
        public static event SubscriptionLoadedAction OnSubscriptionLoaded;
        public IEnumerator GetSubscriptions(string UserAccessToken)
        {
            var apiUrl = "https://management.azure.com/subscriptions?api-version=2014-04-01";

            UnityWebRequest Request = UnityWebRequest.Get(apiUrl);

            Request.method = UnityWebRequest.kHttpVerbGET;
            Request.SetRequestHeader("Content-Type", "application/json; utf-8");
            Request.SetRequestHeader("Authorization", UserAccessToken);

            yield return Request.SendWebRequest();
            var JsonString = Request.downloadHandler.text;
            Subscriptions = JsonConvert.DeserializeObject<SubscriptionRootObject>(JsonString).value;
            OnSubscriptionLoaded(Subscriptions);
        }
    }
}
