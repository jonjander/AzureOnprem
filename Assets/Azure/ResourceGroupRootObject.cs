using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Azure
{
    [Serializable]
    public class ResourceGroupRootObject
    {
        public List<ResourceGroupObject> value { get; set; }
    }
}
