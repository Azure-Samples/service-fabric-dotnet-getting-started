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

    /// <summary>
    /// Default controller.
    /// </summary>
    public class DefaultController : ApiController
    {
        private readonly IReliableStateManager stateManager;

        public DefaultController(IReliableStateManager stateManager)
        {
            this.stateManager = stateManager;
        }

        [HttpGet]
        public async Task<IHttpActionResult> Count()
        {
            IReliableDictionary<string, long> statsDictionary = await this.stateManager.GetOrAddAsync<IReliableDictionary<string, long>>("statsDictionary");

            using (ITransaction tx = this.stateManager.CreateTransaction())
            {
                ConditionalResult<long> result = await statsDictionary.TryGetValueAsync(tx, "Number of Words Processed");

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
            }

            return this.Ok();
        }
    }
}