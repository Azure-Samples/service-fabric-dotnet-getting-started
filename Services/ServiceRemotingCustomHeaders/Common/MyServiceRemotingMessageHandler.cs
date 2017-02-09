// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Common
{
    using System.Fabric;
    using System.Threading.Tasks;
    using Microsoft.ServiceFabric.Services.Remoting;
    using Microsoft.ServiceFabric.Services.Remoting.Runtime;

    //
    // This implementation of the remoting message handler, adds the activity id sent by the client
    // in the message headers to the callcontext.
    //
    public class MyServiceRemotingMessageHandler : IServiceRemotingMessageHandler
    {
        private ServiceRemotingDispatcher innerRemotingMessageHandler;

        public MyServiceRemotingMessageHandler(ServiceContext context, IService service)
        {
            this.innerRemotingMessageHandler = new ServiceRemotingDispatcher(context, service);
        }

        public void HandleOneWay(IServiceRemotingRequestContext requestContext, ServiceRemotingMessageHeaders messageHeaders, byte[] requestBody)
        {
            ActivityId.UpdateCurrentActivityId(messageHeaders);
            this.innerRemotingMessageHandler.HandleOneWay(requestContext, messageHeaders, requestBody);
        }

        public Task<byte[]> RequestResponseAsync(
            IServiceRemotingRequestContext requestContext, ServiceRemotingMessageHeaders messageHeaders, byte[] requestBody)
        {
            ActivityId.UpdateCurrentActivityId(messageHeaders);
            return this.innerRemotingMessageHandler.RequestResponseAsync(requestContext, messageHeaders, requestBody);
        }
    }
}