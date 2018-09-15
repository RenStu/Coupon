using Microsoft.AspNetCore.Mvc;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Gateway.Controllers
{
    [Route("api/[controller]")]
    public class RegisterController : BaseController
    {
        // POST api/register
        [HttpPost]
        public HttpResponseMessage Post([FromBody]Command command)
        {
            HttpResponseMessage response = null;
            using (var client = new HttpClient())
            {
                var serviceEndPoint = GetEndPoint($"fabric:/Coupon/{command.Change.Service}");
                response = client.PostAsJsonAsync($"{serviceEndPoint}/api/cmd", command).Result;
            }

            return new HttpResponseMessage(response.StatusCode);
        }
    }
}
