using System;
using System.Collections.Generic;
using System.Text;

namespace Shared
{
    public class Offer : BaseDocument
    {
        public Offer()
        {
            ListProduct = new HashSet<Product>();
        }
        public string Name { get; set; }

        public string Location { get; set; }

        public DateTime EffectiveStartDate { get; set; }

        public DateTime EffectiveEndDate { get; set; }

        public string ShopName { get; set; }

        public string UserShopEmail { get; set; }

        public ICollection<Product> ListProduct { get; set; }
    }
}
