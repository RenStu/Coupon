using System;
using System.Collections.Generic;
using System.Text;

namespace Shared
{
    public class UserCoupon
    {
        public string UserEmail { get; set; }

        public bool? InStock { get; set; }

        public bool IsDelivered { get; set; }

        public bool IsCancelled { get; set; }
    }
}
