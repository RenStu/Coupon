using Commands;
using CouchDB.Client;
using CouchDB.Client.FluentMango;
using MediatR;
using Newtonsoft.Json;
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

            if (offerObj.EffectiveStartDate < offerObj.EffectiveEndDate && offerObj.EffectiveEndDate > DateTime.Now)
            {
                var couchDB = new CouchClient(Couch.EndPoint);
                var dbOffers = couchDB.GetDatabaseAsync(Couch.DBOffers).Result;
                var dbUsers = couchDB.GetDatabaseAsync(Couch.DBUsers).Result;
                var dbUser = couchDB.GetDatabaseAsync(request.DbName).Result;
                var userObj = JsonConvert.DeserializeObject<User>(
                    dbUser.GetAsync("org.couchdb.user:" + request.DbName.ToUserName()).Result.Content);

                if (userObj.IsShopkeeper)
                {
                    dbUser.InsertAsync(offerObj);

                    var users = JsonConvert.DeserializeObject<List<User>>(
                        dbUsers.SelectAsync(new FindBuilder().Selector("Location", SelectorOperator.Equals, userObj.Location))
                    .Result.Content);
                    if (users.Any())
                    {
                        foreach (var user in users)
                        {
                            if (user.Email.Equals(userObj.Email, StringComparison.InvariantCultureIgnoreCase))
                            {
                                dbUser = couchDB.GetDatabaseAsync(user.DbName).Result;
                                dbUser.InsertAsync(offerObj);
                            }
                        }
                    }
                }
            }
        }
    }
}
