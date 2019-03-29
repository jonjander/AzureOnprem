using Assets.Azure.Resource;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Azure
{
    [Serializable]
    public class ResourceGroupObject
    {
        public string id { get; set; }
        public string name { get; set; }
        public string location { get; set; }
        public ResourceGroupProperties properties { get; set; }
        public Tags tags { get; set; }

        
        public ResourceGroupObject()
        {

        }
    }
}
