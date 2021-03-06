﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Shared;

namespace Offers.Controllers
{
    [Route("api/[controller]")]
    public class CmdController : Controller
    {
        private readonly IMediator mediator;

        public CmdController(IMediator mediator)
        {
            this.mediator = mediator;
        }


        // POST api/values
        [HttpPost]
        public void Post([FromBody]Command command)
        {

            JObject commandOject = JObject.Parse(JsonConvert.SerializeObject(command));
            JObject newCommandOject = JObject.Parse(command.Change.CommandJSON);

            commandOject.Merge(newCommandOject, new JsonMergeSettings
            {
                MergeArrayHandling = MergeArrayHandling.Union
            });

            var commandType = Type.GetType(command.Change.CommandName);
            var commandObj = commandOject.ToObject(commandType);

            var response = mediator.Send((IRequest)commandObj);

        }

    }
}
