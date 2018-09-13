using System;
using System.Collections.Generic;
using System.Text;

namespace Shared
{
    public class BaseDocument
    {
        public string _id { get; set; }

        public string _rev { get; set; }

        public string DbName { get; set; }
    }
}
