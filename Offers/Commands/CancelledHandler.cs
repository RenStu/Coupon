using Commands;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Offers.Commands
{
    public class CancelledHandler : RequestHandler<Cancelled>
    {
        protected override void Handle(Cancelled request)
        {
            throw new NotImplementedException();
        }
    }
}
