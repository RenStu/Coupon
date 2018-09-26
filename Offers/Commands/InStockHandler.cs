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
    public class InStockHandler : RequestHandler<InStock>
    {
        protected override void Handle(InStock request)
        {
            var dbOffers = new CouchClient(Couch.EndPoint).GetDatabaseAsync(Couch.DBOffers).Result;
            var jTOffer = dbOffers.GetAsync(request.IdOffer).Result.Json;
            var offer = jTOffer.ToObject<Offer>();

            if (offer._id != null)
            {
                offer.ListProduct.Where(x => x.Guid.Equals(request.GuidProduct)).ToList().ForEach(x =>
                {
                    x.ListUserCoupon.Where(y => y.UserEmail.Equals(request.UserEmail, StringComparison.InvariantCultureIgnoreCase)).ToList().ForEach(y =>
                    {
                        y.InStock = true;
                    });
                });

                var offerToken = JToken.FromObject(offer)["_rev"] = jTOffer["_rev"];
                UpdateWithMergeConflicts(offerToken, dbOffers);

                var userDB_ = new CouchClient(Couch.EndPoint).GetDatabaseAsync(request.DbName).Result;
                userDB_.ForceUpdateAsync(JToken.FromObject(offer));

                var dbUsers = new CouchClient(Couch.EndPoint).GetDatabaseAsync(Couch.DBUsers).Result;
                var users = dbUsers.SelectAsync(new FindBuilder().Selector("Location", SelectorOperator.Equals, offer.Location))
                    .Result.Docs.ToObject<List<User>>();

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

        protected void UpdateWithMergeConflicts(JToken offer, CouchDatabase dbOffers)
        {
            var result = dbOffers.UpdateAsync(offer).Result;

            if (result.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                var offerNewer = dbOffers.GetAsync(offer.GetString("_id")).Result.Json;
                var jOffer = JObject.Parse(JsonConvert.SerializeObject(offerNewer));
                var jOfferNewer = JObject.Parse(JsonConvert.SerializeObject(offerNewer));

                jOffer.Merge(jOfferNewer, new JsonMergeSettings
                {
                    MergeArrayHandling = MergeArrayHandling.Merge
                });

                UpdateWithMergeConflicts(offer, dbOffers);
            }
        }
    }
}
