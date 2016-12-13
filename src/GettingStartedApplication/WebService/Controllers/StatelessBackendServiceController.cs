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
    public class StatelessBackendServiceController : Controller
    {
        // GET: api/values
        [HttpGet]
        public async Task<IActionResult> GetAsync()
        {
            var returnData = new {
                Count = 12345
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

    }
}
