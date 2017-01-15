// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

 // For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace WebService.Controllers
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.ServiceFabric.Services.Remoting.Client;
    using StatelessBackendService.Interfaces;
    using System.Fabric;
    using System;

    [Route("api/[controller]")]
    public class StatelessBackendServiceController : Controller
    {
        private readonly ConfigSettings configSettings;
        private readonly StatelessServiceContext serviceContext;

        public StatelessBackendServiceController(StatelessServiceContext serviceContext, ConfigSettings settings)
        {
            this.serviceContext = serviceContext;
            this.configSettings = settings;
        }

        // GET: api/values
        [HttpGet]
        public async Task<IActionResult> GetAsync()
        {
            string serviceUri = this.serviceContext.CodePackageActivationContext.ApplicationName + "/" + this.configSettings.StatelessBackendServiceName;

            IStatelessBackendService proxy = ServiceProxy.Create<IStatelessBackendService>(new Uri(serviceUri));

            long result = await proxy.GetCountAsync();

            return this.Json(new CountViewModel() { Count = result });
        }
    }
}