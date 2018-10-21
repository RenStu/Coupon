using System;
using System.Collections.Generic;
using System.Text;

namespace Shared
{
    public class UserCoupon
    {
        public UserCoupon()
        {
            DateChange = DateTime.Now;
        }

        public DateTime DateChange { get; set; }

        string userEmail;

        public string UserEmail
        {
            get { return userEmail; }
            set { userEmail = value; DateChange = DateTime.Now; }
        }

        bool? inStock;

        public bool? InStock
        {
            get { return inStock; }
            set { inStock = value; DateChange = DateTime.Now; }
        }

        bool isDelivered;

        public bool IsDelivered
        {
            get { return isDelivered; }
            set { isDelivered = value; DateChange = DateTime.Now; }
        }

        bool isCancelled;

        public bool IsCancelled
        {
            get { return isCancelled; }
            set { isCancelled = value; DateChange = DateTime.Now; }
        }

        bool isOutOfRange;

        public bool IsOutOfRange
        {
            get { return isOutOfRange; }
            set { isOutOfRange = value; DateChange = DateTime.Now; }
        }

    }
}
