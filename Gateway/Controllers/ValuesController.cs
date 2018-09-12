using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Shared;

namespace Gateway.Controllers
{
    [Route("api/[controller]")]
    public class ValuesController : BaseController
    {
        // GET api/values
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            //return "value";

            var command = new Command
            {
                Change = new Change {
                    CommandName = "Commands.GoogleSearch",
                    CommandJSON = "{ \"Search\" : \"cats\" }",
                    Service = "PyGoogleImg"
                }
            };

            HttpResponseMessage response = null;
            using (var client = new HttpClient())
            {
                var serviceEndPoint = GetEndPoint($"fabric:/Coupon/{command.Change.Service}");
                response = client.PostAsJsonAsync($"{serviceEndPoint}/api/cmd", command).Result;
            }

            //var value = response.Content.ReadAsStringAsync().Result;
            return id.ToString();
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
