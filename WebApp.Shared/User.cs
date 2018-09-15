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

        public string Location { get; set; }

        public string[] Roles { get; set; }

        public bool IsShopkeeper { get; set; }

        public string ShopName { get; set; }
    }
}
