using Commands;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Offers.Commands
{
    public class FinishedStockHandler : RequestHandler<FinishedStock>
    {
        protected override void Handle(FinishedStock request)
        {
            throw new NotImplementedException();
        }
    }
}
