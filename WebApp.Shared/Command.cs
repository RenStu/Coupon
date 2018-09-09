using System;
using System.Collections.Generic;
using System.Text;

namespace Shared
{
    public class Command
    {

        public Change Change { get; set; }

        public string DbName { get; set; }
    }

    public class Change
    {
        public string _id { get; set; }

        public string _rev { get; set; }

        public string Type { get; set; }

        public string Service { get; set; }

        public string CommandName { get; set; }

        public string CommandJSON { get; set; }
    }
}
