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
    public class FinishedStockHandler : RequestHandler<FinishedStock>
    {
        protected override void Handle(FinishedStock request)
        {
            var dbOffers = new CouchClient(Couch.EndPoint).GetDatabaseAsync(Couch.DBOffers).Result;
            var offer = JsonConvert.DeserializeObject<Offer>(dbOffers.GetAsync(request.IdOffer).Result.Content);
            if (offer._id != null)
            {
                offer.ListProduct.Where(x => x.Guid.Equals(request.GuidProduct)).ToList().ForEach(x =>
                {
                    x.ListUserCoupon.Where(y => y.UserEmail.Equals(request.UserEmail, StringComparison.InvariantCultureIgnoreCase)).ToList().ForEach(y =>
                    {
                        y.InStock = false;
                    });
                });

                var userDB_ = new CouchClient(Couch.EndPoint).GetDatabaseAsync(request.DbName).Result;
                userDB_.ForceUpdateAsync(JToken.FromObject(offer));

                var dbUsers = new CouchClient(Couch.EndPoint).GetDatabaseAsync(Couch.DBUsers).Result;
                var users = dbUsers.SelectAsync(new FindBuilder().Selector("Location", SelectorOperator.Equals, offer.Location))
                .Result.Dynamic.docs.ToObject<List<User>>();

                foreach (var user in users)
                {
                    if (user.Email.Equals(request.UserEmail, StringComparison.InvariantCultureIgnoreCase))
                    {
                        userDB_ = new CouchClient(Couch.EndPoint).GetDatabaseAsync(user.DbName).Result;
                        userDB_.ForceUpdateAsync(JToken.FromObject(offer));
                    }
                }
            }
        }
    }
}
