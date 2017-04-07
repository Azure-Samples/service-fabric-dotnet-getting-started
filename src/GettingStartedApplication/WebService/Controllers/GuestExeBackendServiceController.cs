// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace WebService.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using System;
    using System.Fabric;
    using System.Net.Http;
    using System.Threading.Tasks;

    [Route("api/[controller]")]
    public class GuestExeBackendServiceController : Controller
    {
        private readonly HttpClient httpClient;
        private readonly StatelessServiceContext serviceContext;
        private readonly ConfigSettings configSettings;
        private readonly FabricClient fabricClient;

        public GuestExeBackendServiceController(StatelessServiceContext serviceContext, HttpClient httpClient, FabricClient fabricClient, ConfigSettings settings)
        {
            this.serviceContext = serviceContext;
            this.httpClient = httpClient;
            this.configSettings = settings;
            this.fabricClient = fabricClient;
        }

        // GET: api/values
        [HttpGet]
        public async Task<IActionResult> GetAsync()
        {
            string serviceUri = $"{this.serviceContext.CodePackageActivationContext.ApplicationName}/{this.configSettings.GuestExeBackendServiceName}".Replace("fabric:/", "");

            string proxyUrl = $"http://localhost:{this.configSettings.ReverseProxyPort}/{serviceUri}?cmd=instance";

            HttpResponseMessage response = await this.httpClient.GetAsync(proxyUrl);

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                return this.StatusCode((int)response.StatusCode);
            }

            return this.Ok(await response.Content.ReadAsStringAsync());
        }


        // GET api/values/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            throw new NotImplementedException("No method implemented to get a specific key/value pair from the Stateful Backend Service");
        }


        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
            throw new NotImplementedException("No method implemented to delete a specified key/value pair in the Stateful Backend Service");
        }

    }
}