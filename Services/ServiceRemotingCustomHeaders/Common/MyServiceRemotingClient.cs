// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Common
{
    using System.Fabric;
    using System.Threading.Tasks;
    using Microsoft.ServiceFabric.Services.Remoting;
    using Microsoft.ServiceFabric.Services.Remoting.Client;

    /// <summary>
    /// This implementation of the RemotingClient, flows the current activity id to the remote method call via
    /// message headers.
    /// </summary>
    public class MyServiceRemotingClient : IServiceRemotingClient
    {
        public IServiceRemotingClient InnerClient;

        public MyServiceRemotingClient(IServiceRemotingClient remotingClient)
        {
            this.InnerClient = remotingClient;
        }

        public ResolvedServiceEndpoint Endpoint
        {
            get { return this.InnerClient.Endpoint; }

            set { this.InnerClient.Endpoint = value; }
        }

        public string ListenerName
        {
            get { return this.InnerClient.ListenerName; }

            set { this.InnerClient.ListenerName = value; }
        }

        public ResolvedServicePartition ResolvedServicePartition
        {
            get { return this.InnerClient.ResolvedServicePartition; }

            set { this.InnerClient.ResolvedServicePartition = value; }
        }

        public Task<byte[]> RequestResponseAsync(ServiceRemotingMessageHeaders messageHeaders, byte[] requestBody)
        {
            ActivityId.SetActivityIdHeader(messageHeaders);
            return this.InnerClient.RequestResponseAsync(messageHeaders, requestBody);
        }

        public void SendOneWay(ServiceRemotingMessageHeaders messageHeaders, byte[] requestBody)
        {
            ActivityId.SetActivityIdHeader(messageHeaders);
            this.InnerClient.SendOneWay(messageHeaders, requestBody);
        }
    }
}