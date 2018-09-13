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
            if (offerObj.EffectiveStartDate < offerObj.EffectiveEndDate && offerObj.EffectiveEndDate > DateTime.Now)
            {
                var couchDB = new CouchClient("");
                var dbOffers = couchDB.GetDatabaseAsync("Offers").Result;
                var dbUsers = couchDB.GetDatabaseAsync("_users").Result;
                var userDB_ = couchDB.GetDatabaseAsync(request.DbName).Result;
                var userObj = JsonConvert.DeserializeObject<User>(
                    userDB_.GetAsync("org.couchdb.user:" + request.DbName.Substring(7).HexToString()).Result.Content);
                userDB_.InsertAsync(offerObj);

                var users = JsonConvert.DeserializeObject<List<User>>(
                    dbUsers.SelectAsync(new FindBuilder().Selector("Location", SelectorOperator.Equals, userObj.Location))
                .Result.Content);
                if (users.Any())
                {
                    foreach (var user in users)
                    {
                        userDB_ = couchDB.GetDatabaseAsync(user.DbName).Result;
                        userDB_.InsertAsync(offerObj);
                    }
                }
            }
        }
    }
}
