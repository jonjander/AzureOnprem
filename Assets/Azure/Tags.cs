using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Azure
{
    [Serializable]
    public class Tags
    {
        public string displayName { get; set; }
        public string Environment { get; set; }
        public string iLo { get; set; }
    }
}
