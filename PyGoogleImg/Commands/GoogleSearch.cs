using MediatR;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Commands
{
    public class GoogleSearch : Command, IRequest
    {
        public string Search { get; set; }
    }
}
