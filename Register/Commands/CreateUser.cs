using MediatR;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Commands
{
    public class CreateUser : Command, IRequest
    {
        public string Name { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }

        public string Location { get; set; }

        public string Type { get { return "user"; } }

        public string[] Roles { get; set; }

        public string IsShopkeeper { get; set; }

        public string ShopName { get; set; }
    }
}
