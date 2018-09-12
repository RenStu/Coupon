using Commands;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Offers.Commands
{
    public class RequestCouponHandler : RequestHandler<RequestCoupon>
    {
        protected override void Handle(RequestCoupon request)
        {
            throw new NotImplementedException();
        }
    }
}
