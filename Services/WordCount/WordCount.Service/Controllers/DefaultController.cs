// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace WordCountService.Controllers
{
    using System.Threading.Tasks;
    using System.Web.Http;
    using Microsoft.ServiceFabric.Data;
    using Microsoft.ServiceFabric.Data.Collections;
    using System;
    using System.Threading;
    using WordCount.Service;/// <summary>
                            /// Default controller.
                            /// </summary>
    public class DefaultController : ApiController
    {
        private readonly IReliableStateManager stateManager;
        private static string statsDictionaryName = "statsDictionary";
        private static string countKey = "Number of Words Processed";

        public DefaultController(IReliableStateManager stateManager)
        {
            this.stateManager = stateManager;
        }

        [HttpGet]
        public async Task<IHttpActionResult> Count()
        {
            IReliableDictionary<string, long> statsDictionary = 
                await this.stateManager.GetOrAddAsync<IReliableDictionary<string, long>>(statsDictionaryName);

            using (ITransaction tx = this.stateManager.CreateTransaction())
            {
                ConditionalResult<long> result = await statsDictionary.TryGetValueAsync(tx, countKey);

                if (result.HasValue)
                {
                    return this.Ok(result.Value);
                }
            }

            return this.Ok(0);
        }

        [HttpPut]
        public async Task<IHttpActionResult> AddWord(string word)
        {
            IReliableQueue<string> queue = await this.stateManager.GetOrAddAsync<IReliableQueue<string>>("inputQueue");

            using (ITransaction tx = this.stateManager.CreateTransaction())
            {
                await queue.EnqueueAsync(tx, word);

                await tx.CommitAsync();

                await UpdateStats();
            }

            return this.Ok();
        }

        private async Task<long> UpdateStats(int count = 1)
        {
            long numberOfProcessedWords = 0;
            var statsDictionary = await this.stateManager.GetOrAddAsync<IReliableDictionary<string, long>>(statsDictionaryName);

            try
            {
                using (var tx = this.stateManager.CreateTransaction())
                {
                    numberOfProcessedWords = await statsDictionary.AddOrUpdateAsync(
                                tx, countKey, 1,
                                (key, oldValue) => oldValue + count);
                    await tx.CommitAsync();
                }
            }
            catch (TimeoutException ex)
            {
                //Service Fabric uses timeouts on collection operations to prevent deadlocks.
                //If this exception is thrown, it means that this transaction was waiting the default
                //amount of time (4 seconds) but was unable to acquire the lock. In this case we simply
                //retry after a random backoff interval. You can also control the timeout via a parameter
                //on the collection operation.
                Thread.Sleep(TimeSpan.FromSeconds(new Random().Next(100, 300)));
                throw ex;
            }
            catch (Exception exception)
            {
                //For sample code only: simply trace the exception.
                ServiceEventSource.Current.MessageEvent(exception.ToString());
                throw exception;
            }

            return numberOfProcessedWords;
        }

    }
}