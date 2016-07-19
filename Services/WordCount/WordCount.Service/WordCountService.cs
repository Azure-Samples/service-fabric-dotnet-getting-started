// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace WordCount.Service
{
    using System;
    using System.Collections.Generic;
    using System.Fabric;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.ServiceFabric.Data;
    using Microsoft.ServiceFabric.Data.Collections;
    using Microsoft.ServiceFabric.Services.Communication.Runtime;
    using Microsoft.ServiceFabric.Services.Runtime;
    using WordCount.Common;

    /// <summary>
    /// Sample Service Fabric persistent service for counting words.
    /// </summary>
    public class WordCountService : StatefulService
    {
        public const string ServiceEventSourceName = "WordCountService";

        /// <summary>
        /// Initializes a new instance of the <see cref="WordCountService"/> class. 
        /// </summary>
        public WordCountService(StatefulServiceContext context)
            : base(context)
        {
            ServiceEventSource.Current.ServiceInstanceConstructed(ServiceEventSourceName);
        }

        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            ServiceEventSource.Current.RunAsyncInvoked(ServiceEventSourceName);
            

            IReliableQueue<string> inputQueue = await this.StateManager.GetOrAddAsync<IReliableQueue<string>>("inputQueue");
            IReliableDictionary<string, long> wordCountDictionary =
                await this.StateManager.GetOrAddAsync<IReliableDictionary<string, long>>("wordCountDictionary");
            IReliableDictionary<string, long> statsDictionary = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, long>>("statsDictionary");


            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    using (ITransaction tx = this.StateManager.CreateTransaction())
                    {
                        ConditionalValue<string> dequeuReply = await inputQueue.TryDequeueAsync(tx);

                        if (dequeuReply.HasValue)
                        {
                            string word = dequeuReply.Value;

                            long count = await wordCountDictionary.AddOrUpdateAsync(
                                tx,
                                word,
                                1,
                                (key, oldValue) => oldValue + 1);

                            long numberOfProcessedWords = await statsDictionary.AddOrUpdateAsync(
                                tx,
                                "Number of Words Processed",
                                1,
                                (key, oldValue) => oldValue + 1);

                            long queueLength = await inputQueue.GetCountAsync(tx);

                            await tx.CommitAsync();

                            ServiceEventSource.Current.RunAsyncStatus(
                                this.Partition.PartitionInfo.Id,
                                numberOfProcessedWords,
                                queueLength,
                                word,
                                count);
                        }
                    }

                    await Task.Delay(TimeSpan.FromMilliseconds(100), cancellationToken);
                }
                catch (TimeoutException)
                {
                    //Service Fabric uses timeouts on collection operations to prevent deadlocks.
                    //If this exception is thrown, it means that this transaction was waiting the default
                    //amount of time (4 seconds) but was unable to acquire the lock. In this case we simply
                    //retry after a random backoff interval. You can also control the timeout via a parameter
                    //on the collection operation.
                    Thread.Sleep(TimeSpan.FromSeconds(new Random().Next(100, 300)));

                    continue;
                }
                catch (Exception exception)
                {
                    //For sample code only: simply trace the exception.
                    ServiceEventSource.Current.MessageEvent(exception.ToString());
                }
            }
        }

        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            ServiceEventSource.Current.CreateCommunicationListener(ServiceEventSourceName);

            return new[]
            {
                new ServiceReplicaListener(initParams => new OwinCommunicationListener("wordcountservice", new Startup(this.StateManager), initParams))
            };
        }
    }
}