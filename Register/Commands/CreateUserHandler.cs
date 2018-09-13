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
    public class CreateUserHandler : RequestHandler<CreateUser>
    {
        protected override void Handle(CreateUser request)
        {
            var couchDB = new CouchClient("");
            var dbUsers = couchDB.GetDatabaseAsync("_users").Result;
            var user = JsonConvert.DeserializeObject<User>(dbUsers.GetAsync(request._id).Result.Content);
            if (user == null)
            {
                var userObj = JsonConvert.DeserializeObject<User>(JsonConvert.SerializeObject(request));
                userObj.DbName = $"usersdb-{userObj.Email.StringToHex()}";
                var result = dbUsers.InsertAsync(userObj).Result;
                if (result.StatusCode == HttpStatusCode.OK)
                {
                    var userDB_ = couchDB.GetDatabaseAsync(userObj.DbName).Result;
                    userDB_.InsertAsync(userObj);

                    var dbOffers = couchDB.GetDatabaseAsync("Offers").Result;
                    var offers = JsonConvert.DeserializeObject<List<Offer>>(
                        dbOffers.SelectAsync(new FindBuilder().Selector("Location", SelectorOperator.Equals, userObj.Location))
                    .Result.Content);
                    if (offers.Any())
                        userDB_.BulkInsertAsync(offers.ToArray());
                }
            }
        }
    }
}
