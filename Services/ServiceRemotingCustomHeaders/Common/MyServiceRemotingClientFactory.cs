// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Common
{
    using System;
    using System.Collections.Generic;
    using System.Fabric;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.ServiceFabric.Services.Client;
    using Microsoft.ServiceFabric.Services.Communication.Client;
    using Microsoft.ServiceFabric.Services.Communication.FabricTransport.Common;
    using Microsoft.ServiceFabric.Services.Remoting;
    using Microsoft.ServiceFabric.Services.Remoting.Client;
    using Microsoft.ServiceFabric.Services.Remoting.FabricTransport.Client;

    public class MyServiceRemotingClientFactory : IServiceRemotingClientFactory
    {
        private IServiceRemotingClientFactory innerRemotingClientFactory;

        public MyServiceRemotingClientFactory(
            IServiceRemotingCallbackClient callbackClient,
            IServicePartitionResolver resolver = null,
            IEnumerable<IExceptionHandler> exceptionHandlers = null)
        {
            this.innerRemotingClientFactory = new FabricTransportServiceRemotingClientFactory(
                new FabricTransportSettings(),
                callbackClient,
                resolver,
                exceptionHandlers);

            this.innerRemotingClientFactory.ClientConnected += this.ClientConnected;
            this.innerRemotingClientFactory.ClientDisconnected += this.ClientDisconnected;
        }

        public event EventHandler<CommunicationClientEventArgs<IServiceRemotingClient>> ClientConnected;

        public event EventHandler<CommunicationClientEventArgs<IServiceRemotingClient>> ClientDisconnected;

        public async Task<IServiceRemotingClient> GetClientAsync(
            ResolvedServicePartition previousRsp, TargetReplicaSelector targetReplicaSelector, string listenerName, OperationRetrySettings retrySettings,
            CancellationToken cancellationToken)
        {
            IServiceRemotingClient remotingClient = await this.innerRemotingClientFactory.GetClientAsync(
                previousRsp,
                targetReplicaSelector,
                listenerName,
                retrySettings,
                cancellationToken);

            return new MyServiceRemotingClient(remotingClient);
        }

        public async Task<IServiceRemotingClient> GetClientAsync(
            Uri serviceUri, ServicePartitionKey partitionKey, TargetReplicaSelector targetReplicaSelector, string listenerName,
            OperationRetrySettings retrySettings, CancellationToken cancellationToken)
        {
            IServiceRemotingClient remotingClient = await this.innerRemotingClientFactory.GetClientAsync(
                serviceUri,
                partitionKey,
                targetReplicaSelector,
                listenerName,
                retrySettings,
                cancellationToken);

            return new MyServiceRemotingClient(remotingClient);
        }

        public Task<OperationRetryControl> ReportOperationExceptionAsync(
            IServiceRemotingClient client, ExceptionInformation exceptionInformation, OperationRetrySettings retrySettings, CancellationToken cancellationToken)
        {
            return this.innerRemotingClientFactory.ReportOperationExceptionAsync(
                ((MyServiceRemotingClient) client).InnerClient,
                exceptionInformation,
                retrySettings,
                cancellationToken);
        }
    }
}