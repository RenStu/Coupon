using System;
using System.Collections.Generic;
using System.Linq;
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

        public int AmountCoupon { get; set; }

        public int RemainingCoupon
        {
            get
            {
                ListUserCoupon.Skip(AmountCoupon).ToList().ForEach(x => { x.IsOutOfRange = true; });
                return AmountCoupon - ListUserCoupon.Take(AmountCoupon).Count();
            }
        }

        public ICollection<UserCoupon> ListUserCoupon { get; set; }
    }
}
