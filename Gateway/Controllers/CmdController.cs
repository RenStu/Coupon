using API.BasicAuth.Attributes;
using Microsoft.AspNetCore.Mvc;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Gateway.Controllers
{
    [Route("api/[controller]")]
    public class CmdController : BaseController
    {
        // POST api/cmd
        [BasicAuthorize("PoC")]
        [HttpPost]
        public HttpResponseMessage Post([FromBody]Command command)
        {
            HttpResponseMessage response = null;
            using (var client = new HttpClient())
            {
                try
                {
                    var serviceEndPoint = GetEndPoint($"fabric:/Coupon/{command.Change.Service}");
                    response = client.PostAsJsonAsync($"{serviceEndPoint}/api/cmd", command).Result;
                }
                catch (Exception)
                {
                    //log
                }
                
            }

            return new HttpResponseMessage(response != null ? response.StatusCode : HttpStatusCode.InternalServerError);
        }
    }
}
