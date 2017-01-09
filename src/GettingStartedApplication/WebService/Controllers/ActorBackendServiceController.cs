// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

 // For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace WebService.Controllers
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json;

    [Route("api/[controller]")]
    public class ActorBackendServiceController : Controller
    {
        // GET: api/actorbackendservice
        [HttpGet]
        public async Task<IActionResult> GetAsync()
        {
            var result = new
            {
                ActorCount = 12345
            };

            return this.Json(result);
        }

        // POST api/actorbackendservice
        [HttpPost]
        public async Task<IActionResult> PostAsync()
        {
            // Probably need some more info from the backend that just a bool, to pass on to the client
            bool result = await this.NewActor();

            // How do we pass on errors from the statefulbackend?
            if (result)
            {
                return this.Json(result);
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
            bool result = await Task.FromResult(true);

            return result;
        }

        #endregion
    }
}