using MediatR;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Commands
{
    public class GoogleSearch : Command, IRequest<HttpStatusCode>
    {
        public string Search { get; set; }
    }
}
