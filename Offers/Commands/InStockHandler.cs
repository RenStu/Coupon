using Commands;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Offers.Commands
{
    public class InStockHandler : RequestHandler<InStock>
    {
        protected override void Handle(InStock request)
        {
            throw new NotImplementedException();
        }
    }
}
