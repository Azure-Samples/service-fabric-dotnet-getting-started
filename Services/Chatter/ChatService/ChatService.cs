// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace ChatWeb
{
    using System;
    using System.Collections.Generic;
    using System.Fabric;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using ChatWeb.Domain;
    using Microsoft.ServiceFabric.Data;
    using Microsoft.ServiceFabric.Data.Collections;
    using Microsoft.ServiceFabric.Services;
    using Microsoft.ServiceFabric.Services.Runtime;
    using Microsoft.ServiceFabric.Services.Communication.Runtime;
    using Microsoft.ServiceFabric.Services.Remoting.Runtime;
    public class ChatService : StatefulService, IChatService
    {
        private IReliableDictionary<DateTime, Message> messageDictionary;
        private const int MessagesToKeep = 3;

        public async Task AddMessageAsync(Message message)
        {
            DateTime time = DateTime.Now.ToLocalTime();
            IReliableDictionary<DateTime, Message> messagesDictionary = await this.GetMessageDictionaryAsync();

            using (ITransaction tx = this.StateManager.CreateTransaction())
            {
                await messagesDictionary.AddAsync(tx, time, message);
                await tx.CommitAsync();
            }
        }

        public async Task<IEnumerable<KeyValuePair<DateTime, Message>>> GetMessages()
        {
            IReliableDictionary<DateTime, Message> messagesDictionary = await this.GetMessageDictionaryAsync();
            return this.messageDictionary.CreateEnumerable(EnumerationMode.Ordered);
        }

        public async Task ClearMessagesAsync()
        {
            IReliableDictionary<DateTime, Message> messagesDictionary = await this.GetMessageDictionaryAsync();
            await messagesDictionary.ClearAsync();
        }

        protected async Task<IReliableDictionary<DateTime, Message>> GetMessageDictionaryAsync()
        {
            if (this.messageDictionary == null)
            {
                this.messageDictionary = await this.StateManager.GetOrAddAsync<IReliableDictionary<DateTime, Message>>("messages");
            }
            return this.messageDictionary;
        }

        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return new List<ServiceReplicaListener> {
                new ServiceReplicaListener(initParams => new ServiceRemotingListener<IChatService>(initParams, this))
            };
        }

        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            TimeSpan timeSpan = new TimeSpan(0, 0, 30);
            ServiceEventSource.Current.ServiceMessage(
                this,
                "Partition {0} started processing messages.",
                this.ServicePartition.PartitionInfo.Id);

            //Use this method to periodically clean up messages in the messagesDictionary
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    if (this.messageDictionary != null)
                    {
                        // Remove all the messages that are older than 30 seconds keeping the last 3 messages
                        IEnumerable<KeyValuePair<DateTime, Message>> oldMessages = from t in this.messageDictionary
                            where t.Key < (DateTime.Now - timeSpan) orderby t.Key ascending 
                            select t;

                        using (ITransaction tx = this.StateManager.CreateTransaction())
                        {
                            foreach (KeyValuePair<DateTime, Message> item in oldMessages.Take(this.messageDictionary.Count() - MessagesToKeep))
                            {
                                await this.messageDictionary.TryRemoveAsync(tx, item.Key);
                            }
                            await tx.CommitAsync();
                        }
                    }
                }
                catch (Exception e)
                {
                    if (!this.HandleException(e))
                    {
                        ServiceEventSource.Current.ServiceMessage(
                            this,
                            "Partition {0} stopped processing because of error {1}",
                            this.ServicePartition.PartitionInfo.Id,
                            e);
                        break;
                    }
                }
            }
        }

        private bool HandleException(Exception e)
        {
            {
                if ((e is FabricNotPrimaryException) || // replica is no longer writable
                    (e is FabricObjectClosedException) || // replica is closed
                    (e is FabricNotReadableException)) // replica is not readable
                {
                    return false;
                }
                if (e is TimeoutException)
                {
                    // Service Fabric uses timeouts on collection operations to prevent deadlocks.
                    // If this exception is thrown, it means that this transaction was waiting the default
                    // amount of time (4 seconds) but was unable to acquire the lock. In this case we simply
                    // retry after a random backoff interval. You can also control the timeout via a parameter
                    // on the collection operation.
                    return true;
                }

                return false;
            }
        }
    }
}