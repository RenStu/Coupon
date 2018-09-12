using System;
using System.Collections.Generic;
using System.Text;

namespace Shared
{
    public class User : BaseDocument
    {
        public string Name { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }

        public string Type { get { return "user"; } }

        public string[] Roles { get; set; }

        public string IsShopkeeper { get; set; }

        public string ShopName { get; set; }
    }
}
