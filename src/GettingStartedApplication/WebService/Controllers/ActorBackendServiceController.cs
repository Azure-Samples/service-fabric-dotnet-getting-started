// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

using ActorBackendService.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using Microsoft.ServiceFabric.Actors.Query;
using System;
using System.Fabric;
using System.Fabric.Query;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace WebService.Controllers
{
    [Route("api/[controller]")]
    public class ActorBackendServiceController(StatelessServiceContext serviceContext, ConfigSettings settings, FabricClient fabricClient) : Controller
    {
        private readonly FabricClient fabricClient = fabricClient;
        private readonly ConfigSettings configSettings = settings;
        private readonly StatelessServiceContext serviceContext = serviceContext;

        // GET: api/actorbackendservice
        [HttpGet]
        public async Task<IActionResult> GetAsync()
        {
            string serviceUri = serviceContext.CodePackageActivationContext.ApplicationName + "/" + configSettings.ActorBackendServiceName;
            ServicePartitionList partitions = await fabricClient.QueryManager.GetPartitionListAsync(new Uri(serviceUri));
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

            return Json(new CountViewModel() { Count = count } );
        }

        // POST api/actorbackendservice
        [HttpPost]
        public async Task<IActionResult> PostAsync()
        {
            string serviceUri = serviceContext.CodePackageActivationContext.ApplicationName + "/" + configSettings.ActorBackendServiceName;
            IMyActor proxy = ActorProxy.Create<IMyActor>(ActorId.CreateRandom(), new Uri(serviceUri));
            await proxy.StartProcessingAsync(CancellationToken.None);

            return Json(true);
        }
    }
}