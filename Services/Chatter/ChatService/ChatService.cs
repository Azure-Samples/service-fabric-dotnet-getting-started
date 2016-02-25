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
    using Microsoft.ServiceFabric.Services.Communication.Runtime;
    using Microsoft.ServiceFabric.Services.Remoting.Runtime;
    using Microsoft.ServiceFabric.Services.Runtime;

    public class ChatService : StatefulService, IChatService
    {
        private const int MessagesToKeep = 50;

        public async Task AddMessageAsync(Message message)
        {
            DateTime time = DateTime.Now.ToLocalTime();

            IReliableDictionary<DateTime, Message> messagesDictionary =
                await this.StateManager.GetOrAddAsync<IReliableDictionary<DateTime, Message>>("messages");

            using (ITransaction tx = this.StateManager.CreateTransaction())
            {
                await messagesDictionary.AddAsync(tx, time, message);
                await tx.CommitAsync();
            }
        }

        public async Task<IEnumerable<KeyValuePair<DateTime, Message>>> GetMessagesAsync()
        {
            IReliableDictionary<DateTime, Message> messagesDictionary =
                await this.StateManager.GetOrAddAsync<IReliableDictionary<DateTime, Message>>("messages");

            using (ITransaction tx = this.StateManager.CreateTransaction())
            {                
                var messagesEnumerable = await messagesDictionary.CreateEnumerableAsync(tx, EnumerationMode.Ordered);
                                
                return messagesEnumerable.ToList();
            }
        }

        public async Task ClearMessagesAsync()
        {
            IReliableDictionary<DateTime, Message> messagesDictionary =
                await this.StateManager.GetOrAddAsync<IReliableDictionary<DateTime, Message>>("messages");

            await messagesDictionary.ClearAsync();
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

            IReliableDictionary<DateTime, Message> messagesDictionary =
                await this.StateManager.GetOrAddAsync<IReliableDictionary<DateTime, Message>>("messages");

            //Use this method to periodically clean up messages in the messagesDictionary
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    IEnumerable<KeyValuePair<DateTime, Message>> messagesEnumerable = await GetMessagesAsync();

                    // Remove all the messages that are older than 30 seconds keeping the last 50 messages
                    IEnumerable<KeyValuePair<DateTime, Message>> oldMessages = from t in messagesEnumerable
                                                                               where t.Key < (DateTime.Now - timeSpan)
                                                                               orderby t.Key ascending
                                                                               select t;

                    using (ITransaction tx = this.StateManager.CreateTransaction())
                    {
                        int messagesCount = (int)await messagesDictionary.GetCountAsync(tx);

                        foreach (KeyValuePair<DateTime, Message> item in oldMessages.Take(messagesCount - MessagesToKeep))
                        {                            
                            await messagesDictionary.TryRemoveAsync(tx, item.Key);
                        }
                        await tx.CommitAsync();
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

                await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
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