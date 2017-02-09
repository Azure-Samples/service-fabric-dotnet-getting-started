// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Stateless1
{
    using System;
    using System.Collections.Generic;
    using System.Fabric;
    using System.Threading.Tasks;
    using Common;
    using Microsoft.ServiceFabric.Services.Communication.Runtime;
    using Microsoft.ServiceFabric.Services.Remoting;
    using Microsoft.ServiceFabric.Services.Remoting.FabricTransport.Runtime;
    using Microsoft.ServiceFabric.Services.Runtime;

    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class Stateless1 : StatelessService, ITestService
    {
        public Stateless1(StatelessServiceContext context)
            : base(context)
        {
        }

        public Task<string> GetCurrentActivityId()
        {
            string currentActivityId;
            if (!ActivityId.TryGetCurrentActivityId(out currentActivityId))
            {
                throw new Exception("Current activity id not found!");
            }

            ServiceEventSource.Current.Message("Invoked for activity id: {0}", currentActivityId);

            return Task.FromResult(currentActivityId);
        }

        /// <summary>
        /// Optional override to create listeners (e.g., TCP, HTTP) for this service replica to handle client or user requests.
        /// </summary>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new[]
            {
                new ServiceInstanceListener(context => this.GetMyRemotedCommunicationListener(context, this))
            };
        }

        /// <summary>
        /// This creates a custom message handler that examines the incoming message headers and sets up the call context.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="service"></param>
        /// <returns></returns>
        private ICommunicationListener GetMyRemotedCommunicationListener(ServiceContext context, IService service)
        {
            MyServiceRemotingMessageHandler customMessageHandler = new MyServiceRemotingMessageHandler(context, service);

            return new FabricTransportServiceRemotingListener(context, customMessageHandler);
        }
    }
}