using System;
using System.Collections.Generic;
using System.Text;

namespace Shared
{
    public class Command : BaseDocument
    {

        public Change Change { get; set; }

    }

    public class Change : BaseDocument
    {
        public string Type { get; set; }

        public string Service { get; set; }

        public string CommandName { get; set; }

        public string CommandJSON { get; set; }
    }
}
