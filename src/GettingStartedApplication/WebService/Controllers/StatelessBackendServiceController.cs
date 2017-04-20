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
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Fabric;
    using System;
    using Microsoft.ApplicationInsights.DataContracts;
    using Microsoft.ApplicationInsights;
    using Microsoft.ApplicationInsights.Extensibility;

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

            // Create and start a new activity representing the beginning of this outgoing request
            Activity activity = new Activity("HttpOut");
            activity.Start();

            DateTimeOffset startTime = DateTimeOffset.UtcNow;

            // Extract the request id and correlation context headers so they can be passed to the callee, which
            // will create the correlation
            Activity currentActivity = Activity.Current;

            string requestId = currentActivity.Id;
            Dictionary<string, string> correlationContextHeader = new Dictionary<string, string>();
            foreach (var pair in currentActivity.Baggage)
            {
                correlationContextHeader.Add(pair.Key, pair.Value);
            }

            long result = 0;
            try
            {
                result = await proxy.GetCountAsync(requestId, correlationContextHeader).ConfigureAwait(false);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                //always stop activity if it was started
                if (activity != null)
                {
                    activity.Stop();
                }
                DateTimeOffset endTime = DateTimeOffset.UtcNow;
                DependencyTelemetry telemetry = new DependencyTelemetry(
                    "HTTP", // dependencyTypeName
                    serviceUri, // target
                    "GET " + serviceUri, // dependencyName
                    serviceUri, // data
                    startTime, // startTime
                    endTime - startTime, // duration
                    "OK", // resultCode
                    true); // success
                TelemetryClient client = new TelemetryClient(TelemetryConfiguration.Active);
                client.TrackDependency(telemetry);
            }

            return this.Json(new CountViewModel() { Count = result });
        }
    }
}