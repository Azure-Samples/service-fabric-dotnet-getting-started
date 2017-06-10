using System;
using System.Fabric;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using TenantService.Interfaces;

namespace WebService.Controllers
{
    [Route("api/[controller]")]
    public class TenantBackendServiceController : Controller
    {
        private readonly ConfigSettings configSettings;
        private readonly StatelessServiceContext serviceContext;

        public TenantBackendServiceController(StatelessServiceContext serviceContext, ConfigSettings settings)
        {
            this.serviceContext = serviceContext;
            this.configSettings = settings;
        }

        [HttpPost]
        public async Task<IActionResult> DeployTenant(string tenantName)
        {
            string serviceUri = this.serviceContext.CodePackageActivationContext.ApplicationName + "/" + this.configSettings.TenantBackendServiceName;

            var proxy = ServiceProxy.Create<ITentantBackendService>(new Uri(serviceUri), new ServicePartitionKey(tenantName.GetHashCode()));

            await proxy.Deploy(tenantName);
            
            return this.Json(null);
        }
    }
}