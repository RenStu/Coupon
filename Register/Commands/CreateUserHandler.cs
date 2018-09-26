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
using System.Net;
using System.Threading.Tasks;

namespace Register.Commands
{
    public class CreateUserHandler : RequestHandler<CreateUser, HttpStatusCode>
    {
        protected override HttpStatusCode Handle(CreateUser request)
        {
            var dbUsers = new CouchClient(Couch.EndPoint).GetDatabaseAsync(Couch.DBUsers).Result;
            var user = JsonConvert.DeserializeObject<User>(dbUsers.GetAsync(request._id).Result.Content);

            if (user._id == null) {
                var userObj = JsonConvert.DeserializeObject<User>(JsonConvert.SerializeObject(request));
                userObj.CqrsType = Cqrs.Query;
                userObj.DbName = $"userdb-{userObj.Email.StringToHex()}";
                userObj.Roles = new string[] { };
                userObj.Type = "user";
                var result = dbUsers.UpdateAsync(JToken.FromObject(userObj)).Result;

                if (result.StatusCode == HttpStatusCode.Created)
                {
                    var dbOffers = new CouchClient(Couch.EndPoint).GetDatabaseAsync(Couch.DBOffers).Result;
                    var offers = dbOffers.SelectAsync(new FindBuilder().Selector("Location", SelectorOperator.Equals, userObj.Location))
                    .Result.Dynamic.docs.ToObject<List<Offer>>();

                    var dbUser = new CouchClient(Couch.EndPoint).GetDatabaseAsync(userObj.DbName).Result;
                    dbUser.ForceUpdateAsync(JToken.FromObject(userObj));

                    if (offers.Count > 0)
                        dbUser.BulkInsertAsync(offers.ToArray());
                }

                return result.StatusCode;
            } else {
                return HttpStatusCode.NotFound;
            }
        }
    }
}
