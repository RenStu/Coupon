using System;
using System.Collections.Generic;
using System.Text;

namespace Shared
{
    public class Product
    {
        public Product()
        {
            ListUserCoupon = new HashSet<UserCoupon>();
        }

        public string Guid { get; set; }

        public string Name { get; set; }

        public Decimal Value { get; set; }

        public bool IsCoupon { get; set; }

        public ICollection<UserCoupon> ListUserCoupon { get; set; }
    }
}
