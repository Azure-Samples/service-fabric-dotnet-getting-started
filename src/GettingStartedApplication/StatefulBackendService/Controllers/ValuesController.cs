// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace StatefulBackendService.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Fabric;
    using System.Threading;
    using System.Threading.Tasks;
    using global::StatefulBackendService.ViewModels;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.ServiceFabric.Data;
    using Microsoft.ServiceFabric.Data.Collections;

    [Route("api/[controller]")]
    public class ValuesController : Controller
    {
        private static readonly Uri ValuesDictionaryName = new Uri("store:/values");

        private readonly IReliableStateManager stateManager;

        public ValuesController(IReliableStateManager stateManager)
        {
            this.stateManager = stateManager;
        }

        // GET api/values
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                List<KeyValuePair<string, string>> result = new List<KeyValuePair<string, string>>();

                ConditionalValue<IReliableDictionary<string, string>> tryGetResult =
                    await this.stateManager.TryGetAsync<IReliableDictionary<string, string>>(ValuesDictionaryName);

                if (tryGetResult.HasValue)
                {
                    IReliableDictionary<string, string> dictionary = tryGetResult.Value;

                    using (ITransaction tx = this.stateManager.CreateTransaction())
                    {
                        IAsyncEnumerable<KeyValuePair<string, string>> enumerable = await dictionary.CreateEnumerableAsync(tx);
                        IAsyncEnumerator<KeyValuePair<string, string>> enumerator = enumerable.GetAsyncEnumerator();

                        while (await enumerator.MoveNextAsync(CancellationToken.None))
                        {
                            result.Add(enumerator.Current);
                        }
                    }
                }
                return this.Json(result);
            }
            catch (FabricException)
            {
                return new ContentResult {StatusCode = 503, Content = "The service was unable to process the request. Please try again."};
            }
        }

        // GET api/values/name
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string name)
        {
            try
            {
                IReliableDictionary<string, string> dictionary =
                    await this.stateManager.GetOrAddAsync<IReliableDictionary<string, string>>(ValuesDictionaryName);

                using (ITransaction tx = this.stateManager.CreateTransaction())
                {
                    ConditionalValue<string> result = await dictionary.TryGetValueAsync(tx, name);

                    if (result.HasValue)
                    {
                        return this.Ok(result.Value);
                    }

                    return this.NotFound();
                }
            }
            catch (FabricNotPrimaryException)
            {
                return new ContentResult {StatusCode = 410, Content = "The primary replica has moved. Please re-resolve the service."};
            }
            catch (FabricException)
            {
                return new ContentResult {StatusCode = 503, Content = "The service was unable to process the request. Please try again."};
            }
        }

        // POST api/values/name
        [HttpPost("{name}")]
        public async Task<IActionResult> Post(string name, [FromBody] ValueViewModel value)
        {
            try
            {
                IReliableDictionary<string, string> dictionary =
                    await this.stateManager.GetOrAddAsync<IReliableDictionary<string, string>>(ValuesDictionaryName);

                using (ITransaction tx = this.stateManager.CreateTransaction())
                {
                    await dictionary.SetAsync(tx, name, value.Value);
                    await tx.CommitAsync();
                }

                return this.Ok();
            }
            catch (FabricNotPrimaryException)
            {
                return new ContentResult {StatusCode = 410, Content = "The primary replica has moved. Please re-resolve the service."};
            }
            catch (FabricException)
            {
                return new ContentResult {StatusCode = 503, Content = "The service was unable to process the request. Please try again."};
            }
        }

        // PUT api/values/5
        [HttpPut("{name}")]
        public async Task<IActionResult> Put(string name, [FromBody] ValueViewModel value)
        {
            try
            {
                IReliableDictionary<string, string> dictionary =
                    await this.stateManager.GetOrAddAsync<IReliableDictionary<string, string>>(ValuesDictionaryName);

                using (ITransaction tx = this.stateManager.CreateTransaction())
                {
                    await dictionary.AddAsync(tx, name, value.Value);
                    await tx.CommitAsync();
                }
            }
            catch (ArgumentException)
            {
                return new ContentResult {StatusCode = 400, Content = $"A value with name {name} already exists."};
            }
            catch (FabricNotPrimaryException)
            {
                return new ContentResult {StatusCode = 410, Content = "The primary replica has moved. Please re-resolve the service."};
            }
            catch (FabricException)
            {
                return new ContentResult {StatusCode = 503, Content = "The service was unable to process the request. Please try again."};
            }

            return this.Ok();
        }

        // DELETE api/valuesname
        [HttpDelete("{name}")]
        public async Task<IActionResult> Delete(string name)
        {
            IReliableDictionary<string, string> dictionary =
                await this.stateManager.GetOrAddAsync<IReliableDictionary<string, string>>(ValuesDictionaryName);

            try
            {
                using (ITransaction tx = this.stateManager.CreateTransaction())
                {
                    ConditionalValue<string> result = await dictionary.TryRemoveAsync(tx, name);

                    await tx.CommitAsync();

                    if (result.HasValue)
                    {
                        return this.Ok();
                    }

                    return new ContentResult {StatusCode = 400, Content = $"A value with name {name} doesn't exist."};
                }
            }
            catch (FabricNotPrimaryException)
            {
                return new ContentResult {StatusCode = 503, Content = "The primary replica has moved. Please re-resolve the service."};
            }
        }
    }
}