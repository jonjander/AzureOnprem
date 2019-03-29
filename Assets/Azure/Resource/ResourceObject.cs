using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Azure.Resource
{
    [Serializable]
    public class ResourceObject
    {
        public string id { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public string kind { get; set; }
        public string location { get; set; }
        public Tags tags { get; set; }
        
    }
}
