using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace WebService.Controllers
{
    [Route("api/[controller]")]
    public class ActorBackendServiceController : Controller
    {
        // GET: api/actorbackendservice
        [HttpGet]
        public async Task<IActionResult> GetAsync()
        {
            var returnData = new
            {
                ActorCount = 12345
            };

            return new ContentResult
            {
                StatusCode = 200,
                Content = JsonConvert.SerializeObject(
                    returnData,
                    Formatting.Indented,
                    new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() }),
                ContentType = "application/json"
            };
        }

        // POST api/actorbackendservice
        [HttpPost]
        public async Task<IActionResult> PostAsync()
        {

            // Probably need some more info from the backend that just a bool, to pass on to the client
            bool result = await NewActor();

            // How do we pass on errors from the statefulbackend?
            if (result)
            {
                return new ContentResult
                {
                    StatusCode = 200,
                    Content = JsonConvert.SerializeObject(result),
                    ContentType = "application/json"
                };
            }
            else
            {
                return new ContentResult
                {
                    StatusCode = 500,
                    Content = JsonConvert.SerializeObject(result),
                    ContentType = "application/json"
                };
            }
        }

        #region Helper Methods
        private async Task<bool> NewActor()
        {
            var result = await Task.FromResult(true);

            return result;
        }

        #endregion

    }
}
