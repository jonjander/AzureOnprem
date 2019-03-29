using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Azure.Resource
{
    [Serializable]
    public class ResourceObjectRootObject
    {
        public List<ResourceObject> value { get; set; }
    }
}
