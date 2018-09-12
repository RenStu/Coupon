using Commands;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Offers.Commands
{
    public class CreateOfferHandler : RequestHandler<CreateOffer>
    {
        protected override void Handle(CreateOffer request)
        {
            throw new NotImplementedException();
        }
    }
}
