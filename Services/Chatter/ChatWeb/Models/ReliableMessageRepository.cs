// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace ChatWeb.Models
{
    using System;
    using System.Collections.Generic;
    using System.Fabric;
    using System.Threading.Tasks;
    using ChatWeb.Domain;
    using Microsoft.ServiceFabric.Services.Remoting.Client;
    using Microsoft.ServiceFabric.Services.Client;

    public class ReliableMessageRepository : IMessageRepository
    {
        private Uri chatServiceInstance = new Uri(FabricRuntime.GetActivationContext().ApplicationName + "/ChatService");
        
        public Task AddMessageAsync(Message message)
        {
            try
            {
                // The service only has a single partition. In order to access the partition need to provide a
                // value in the LowKey - HighKey range defined in the ApplicationManifest.xml
                IChatService proxy = ServiceProxy.Create<IChatService>(chatServiceInstance, new ServicePartitionKey(1));
                return proxy.AddMessageAsync(message);
            }
            catch (Exception e)
            {
                ServiceEventSource.Current.Message("AddMessageAsync failed because of error {0}", e.ToString());
                throw;
            }
        }

        public Task<IEnumerable<KeyValuePair<DateTime, Message>>> GetMessagesAsync()
        {
            try
            {
                IChatService proxy = ServiceProxy.Create<IChatService>(chatServiceInstance, new ServicePartitionKey(1));
                //return (await proxy.GetMessages()).Select(x => x.Value);
                return proxy.GetMessagesAsync();
            }
            catch (Exception e)
            {
                ServiceEventSource.Current.Message("GetMessage failed because of error {0}", e.ToString());
                throw;
            }
        }

        public Task ClearMessagesAsync()
        {
            try
            {
                IChatService proxy = ServiceProxy.Create<IChatService>(chatServiceInstance, new ServicePartitionKey(1));
                return proxy.ClearMessagesAsync();
            }
            catch (Exception e)
            {
                ServiceEventSource.Current.Message("ClearMessageAsync failed because of error {0}", e.ToString());
                throw;
            }
        }
    }
}