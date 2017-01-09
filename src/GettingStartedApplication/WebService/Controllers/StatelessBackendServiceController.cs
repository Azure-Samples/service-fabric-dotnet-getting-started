// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

 // For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace WebService.Controllers
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;

    [Route("api/[controller]")]
    public class StatelessBackendServiceController : Controller
    {
        // GET: api/values
        [HttpGet]
        public async Task<IActionResult> GetAsync()
        {
            var result = new
            {
                Count = 12345
            };

            return this.Json(result);
        }
    }
}