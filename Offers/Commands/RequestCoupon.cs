using MediatR;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Commands
{
    public class RequestCoupon : Command, IRequest
    {
        public string IdOffer { get; set; }

        public string GuidProduct { get; set; }

        public string UserEmail { get; set; }
    }
}
