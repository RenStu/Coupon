using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Services.Client;
using Newtonsoft.Json.Linq;

namespace Gateway.Controllers
{
    public class BaseController : Controller
    {
        #region protected

        protected string GetEndPoint(string address)
        {
            ServicePartitionResolver resolver = ServicePartitionResolver.GetDefault();
            ResolvedServicePartition partition =
                  resolver.ResolveAsync(new Uri(address), new ServicePartitionKey(), new CancellationToken()).Result;

            ResolvedServiceEndpoint endpoint = partition.GetEndpoint();

            JObject addresses = JObject.Parse(endpoint.Address);
            return (string)addresses["Endpoints"].First();
        }

        protected HttpClientHandler GetCookie(string endpoint)
        {
            var cookieContainer = new CookieContainer();
            HttpClientHandler handler = new HttpClientHandler
            {
                UseCookies = true,
                UseDefaultCredentials = true,
                CookieContainer = cookieContainer
            };

            foreach (var cookie in Request.Cookies)
            {
                cookieContainer.Add(new Cookie(cookie.Key, cookie.Value, "/", new Uri(endpoint).Host));
            }

            return handler;
        }
        #endregion
    }
}