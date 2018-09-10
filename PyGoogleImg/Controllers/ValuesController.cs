using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PyGoogleImg.Commands;
using Shared;

namespace PyGoogleImg.Controllers
{
    [Route("api/[controller]")]
    public class ValuesController : Controller
    {
        private readonly IMediator mediator;

        public ValuesController(IMediator mediator)
        {
            this.mediator = mediator;
        }

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
            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody]Command command)
        {

            JObject existentOject = JObject.Parse(JsonConvert.SerializeObject(command));
            JObject newOject = JObject.Parse(command.Change.CommandJSON);

            existentOject.Merge(newOject, new JsonMergeSettings
            {
                // union array values together to avoid duplicates
                MergeArrayHandling = MergeArrayHandling.Union
            });

            var commandType = Type.GetType(command.Change.CommandName);
            
            //var commandObj = JsonConvert.DeserializeObject(command.Change.CommandJSON, commandType);
            var commandObj = existentOject.ToObject(commandType);

            //var response = mediator.Send((IRequest<IEnumerable<string>>)commandObj);
            var response = mediator.Send((IRequest)commandObj);

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
