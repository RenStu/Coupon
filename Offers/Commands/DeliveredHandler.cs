using Commands;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Offers.Commands
{
    public class DeliveredHandler : RequestHandler<Delivered>
    {
        protected override void Handle(Delivered request)
        {
            throw new NotImplementedException();
        }
    }
}
