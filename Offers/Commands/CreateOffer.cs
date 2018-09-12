using MediatR;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Commands
{
    public class CreateOffer : Command, IRequest
    {
        public CreateOffer()
        {
            ListProduct = new HashSet<Shared.Product>();
        }
        public string Name { get; set; }

        public DateTime EffectiveStartDate { get; set; }

        public DateTime EffectiveEndDate { get; set; }

        public ICollection<Product> ListProduct { get; set; }
    }
}
