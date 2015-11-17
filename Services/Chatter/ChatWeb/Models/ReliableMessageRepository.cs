// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace ChatWeb.Models
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using ChatWeb.Domain;    
    using Microsoft.ServiceFabric.Services.Remoting.Client;

    public class ReliableMessageRepository : IMessageRepository
    {
        // The service only has a single partition. In order to access the partition need to provide a
        // value in the LowKey - HighKey range defined in the ApplicationManifest.xml
        private long defaultPartitionID = 1;

        public async Task AddMessageAsync(Message message)
        {
            Uri serviceName = new Uri("fabric:/Chatter/ChatService");
            try
            {
                IChatService proxy = ServiceProxy.Create<IChatService>(this.defaultPartitionID, serviceName);
                await proxy.AddMessageAsync(message);
            }
            catch (Exception e)
            {
                ServiceEventSource.Current.Message("AddMessageAsync failed because of error {0}", e.ToString());
                throw;
            }
        }

        public Task<IEnumerable<KeyValuePair<DateTime, Message>>> GetMessages()
        {
            Uri serviceName = new Uri("fabric:/Chatter/ChatService");
            try
            {
                IChatService proxy = ServiceProxy.Create<IChatService>(this.defaultPartitionID, serviceName);
                //return (await proxy.GetMessages()).Select(x => x.Value);
                return proxy.GetMessages();
            }
            catch (Exception e)
            {
                ServiceEventSource.Current.Message("GetMessage failed because of error {0}", e.ToString());
                throw;
            }
        }

        public async Task ClearMessagesAsync()
        {
            Uri serviceName = new Uri("fabric:/Chatter/ChatService");
            try
            {
                IChatService proxy = ServiceProxy.Create<IChatService>(this.defaultPartitionID, serviceName);
                await proxy.ClearMessagesAsync();
            }
            catch (Exception e)
            {
                ServiceEventSource.Current.Message("ClearMessageAsync failed because of error {0}", e.ToString());
                throw;
            }
        }
    }
}