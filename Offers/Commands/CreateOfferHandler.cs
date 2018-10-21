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
    public class CreateOfferHandler : RequestHandler<CreateOffer>
    {
        protected override void Handle(CreateOffer request)
        {
            var offerObj = JsonConvert.DeserializeObject<Offer>(JsonConvert.SerializeObject(request));
            offerObj.CqrsType = Cqrs.Query;
            offerObj.ListProduct.ToList().ForEach(x =>
            {
                x.Guid = Guid.NewGuid().ToString();
            });

            if (offerObj.EffectiveStartDate < offerObj.EffectiveEndDate)
            {
                var dbOffers = new CouchClient(Couch.EndPoint).GetDatabaseAsync(Couch.DBOffers).Result;
                var dbUsers = new CouchClient(Couch.EndPoint).GetDatabaseAsync(Couch.DBUsers).Result;
                var dbUser = new CouchClient(Couch.EndPoint).GetDatabaseAsync(request.DbName).Result;
                var userObj = JsonConvert.DeserializeObject<User>(
                    dbUser.GetAsync("org.couchdb.user:" + request.DbName.ToUserName()).Result.Content);

                if (userObj.IsShopkeeper)
                {
                    dbOffers.ForceUpdateAsync(JToken.FromObject(offerObj));
                    dbUser.ForceUpdateAsync(JToken.FromObject(offerObj));

                    var users = dbUsers.SelectAsync(new FindBuilder().Selector("location", SelectorOperator.Equals, userObj.Location))
                        .Result.Docs.ToObject<List<User>>();
                    if (users.Any())
                    {
                        foreach (var user in users)
                        {
                            if (!user.Email.Equals(userObj.Email, StringComparison.InvariantCultureIgnoreCase))
                            {
                                dbUser = new CouchClient(Couch.EndPoint).GetDatabaseAsync(user.DbName).Result;
                                dbUser.ForceUpdateAsync(JToken.FromObject(offerObj));
                            }
                        }
                    }
                }
            }
        }
    }
}
