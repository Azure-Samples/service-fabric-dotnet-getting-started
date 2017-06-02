// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace WebService.Controllers
{
    using ActorBackendService.Interfaces;
    using Microsoft.ApplicationInsights;
    using Microsoft.ApplicationInsights.DataContracts;
    using Microsoft.ApplicationInsights.Extensibility;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.ServiceFabric.Actors;
    using Microsoft.ServiceFabric.Actors.Client;
    using Microsoft.ServiceFabric.Actors.Query;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Fabric;
    using System.Fabric.Query;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    [Route("api/[controller]")]
    public class ActorBackendServiceController : Controller
    {
        private readonly FabricClient fabricClient;
        private readonly ConfigSettings configSettings;
        private readonly StatelessServiceContext serviceContext;

        public ActorBackendServiceController(StatelessServiceContext serviceContext, ConfigSettings settings, FabricClient fabricClient)
        {
            this.serviceContext = serviceContext;
            this.configSettings = settings;
            this.fabricClient = fabricClient;
        }

        // GET: api/actorbackendservice
        [HttpGet]
        public async Task<IActionResult> GetAsync()
        {
            string serviceUri = this.serviceContext.CodePackageActivationContext.ApplicationName + "/" + this.configSettings.ActorBackendServiceName;

            ServicePartitionList partitions = await this.fabricClient.QueryManager.GetPartitionListAsync(new Uri(serviceUri));

            long count = 0;
            foreach (Partition partition in partitions)
            {
                long partitionKey = ((Int64RangePartitionInformation)partition.PartitionInformation).LowKey;
                IActorService actorServiceProxy = ActorServiceProxy.Create(new Uri(serviceUri), partitionKey);

                ContinuationToken continuationToken = null;

                do
                {
                    PagedResult<ActorInformation> page = await actorServiceProxy.GetActorsAsync(continuationToken, CancellationToken.None);

                    count += page.Items.Where(x => x.IsActive).LongCount();

                    continuationToken = page.ContinuationToken;
                }
                while (continuationToken != null);
            }

            return this.Json(new CountViewModel() { Count = count } );
        }

        // POST api/actorbackendservice
        [HttpPost]
        public async Task<IActionResult> PostAsync()
        {
           
            string serviceUri = this.serviceContext.CodePackageActivationContext.ApplicationName + "/" + this.configSettings.ActorBackendServiceName;

            IMyActor proxy = ActorProxy.Create<IMyActor>(ActorId.CreateRandom(), new Uri(serviceUri));

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

            try
            {
                await proxy.StartProcessingAsync(requestId, correlationContextHeader, CancellationToken.None);
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
                    "POST " + serviceUri, // dependencyName
                    serviceUri, // data
                    startTime, // startTime
                    endTime - startTime, // duration
                    "OK", // resultCode
                    true); // success
                telemetry.Id = activity.Id;
                TelemetryClient client = new TelemetryClient(TelemetryConfiguration.Active);
                client.TrackDependency(telemetry);
            }

            return this.Json(true);
          
        }
    }
}