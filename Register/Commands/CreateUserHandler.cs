using Commands;
using CouchDB.Client;
using CouchDB.Client.FluentMango;
using MediatR;
using Newtonsoft.Json;
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
            var couchDB = new CouchClient(Couch.EndPoint);
            var dbUsers = couchDB.GetDatabaseAsync(Couch.DBUsers).Result;
            var user = JsonConvert.DeserializeObject<User>(dbUsers.GetAsync(request._id).Result.Content);

            if (user._id == null) {
                var userObj = JsonConvert.DeserializeObject<User>(JsonConvert.SerializeObject(request));
                userObj.CqrsType = Cqrs.Query;
                userObj.DbName = $"usersdb-{userObj.Email.StringToHex()}";
                userObj.Roles = new string[] { };
                userObj.type = "user";
                var result = dbUsers.InsertAsync(userObj).Result;
                if (result.StatusCode == HttpStatusCode.OK)
                {
                    var dbOffers = couchDB.GetDatabaseAsync(Couch.DBOffers).Result;
                    var offers = JsonConvert.DeserializeObject<List<Offer>>(
                        dbOffers.SelectAsync(new FindBuilder().Selector("Location", SelectorOperator.Equals, userObj.Location))
                    .Result.Content);

                    var dbUser = couchDB.GetDatabaseAsync(userObj.DbName).Result;
                    dbUser.InsertAsync(userObj);

                    if (offers.Any())
                        dbUser.BulkInsertAsync(offers.ToArray());
                }

                return result.StatusCode;
            } else {
                return HttpStatusCode.NotFound;
            }
        }
    }
}
