using Commands;
using CouchDB.Client;
using CouchDB.Client.FluentMango;
using MediatR;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Shared;
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
            var couchDB = new CouchClient(Couch.EndPoint);
            var dbOffers = couchDB.GetDatabaseAsync(Couch.DBOffers).Result;
            var offer = JsonConvert.DeserializeObject<Offer>(dbOffers.GetAsync(request.IdOffer).Result.Content);
            if (offer._id != null)
            {
                offer.ListProduct.Where(x => x.Guid.Equals(request.GuidProduct)).ToList().ForEach(x =>
                {
                    x.ListUserCoupon.Where(y => y.UserEmail.Equals(request.UserEmail, StringComparison.InvariantCultureIgnoreCase)).ToList().ForEach(y =>
                    {
                        y.IsCancelled = true;
                    });
                });

                var userDB_ = couchDB.GetDatabaseAsync(request.DbName).Result;
                userDB_.ForceUpdateAsync(JToken.FromObject(offer));

                var dbUsers = couchDB.GetDatabaseAsync(Couch.DBUsers).Result;
                var users = JsonConvert.DeserializeObject<List<User>>(
                    dbUsers.SelectAsync(new FindBuilder().Selector("Location", SelectorOperator.Equals, offer.Location))
                .Result.Content);

                foreach (var user in users)
                {
                    if (user.Email.Equals(request.UserEmail, StringComparison.InvariantCultureIgnoreCase))
                    {
                        userDB_ = couchDB.GetDatabaseAsync(user.DbName).Result;
                        userDB_.ForceUpdateAsync(JToken.FromObject(offer));
                    }
                }
            }
        }
    }
}
