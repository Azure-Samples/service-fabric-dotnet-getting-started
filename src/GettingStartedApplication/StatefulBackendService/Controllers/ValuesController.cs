// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Fabric;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using StatefulBackendService.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;

namespace StatefulBackendService.Controllers
{
    [Route("api/[controller]")]
    public class ValuesController(IReliableStateManager stateManager) : Controller
    {
        private static readonly Uri ValuesDictionaryName = new("store:/values");
        private readonly IReliableStateManager stateManager = stateManager;

        // GET api/values
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                List<KeyValuePair<string, string>> result = [];
                ConditionalValue<IReliableDictionary<string, string>> tryGetResult =
                    await stateManager.TryGetAsync<IReliableDictionary<string, string>>(ValuesDictionaryName);

                if (tryGetResult.HasValue)
                {
                    IReliableDictionary<string, string> dictionary = tryGetResult.Value;

                    using (ITransaction tx = stateManager.CreateTransaction())
                    {
                        Microsoft.ServiceFabric.Data.IAsyncEnumerable<KeyValuePair<string, string>> enumerable = await dictionary.CreateEnumerableAsync(tx);
                        Microsoft.ServiceFabric.Data.IAsyncEnumerator<KeyValuePair<string, string>> enumerator = enumerable.GetAsyncEnumerator();

                        while (await enumerator.MoveNextAsync(CancellationToken.None))
                        {
                            result.Add(enumerator.Current);
                        }
                    }
                }
                return Json(result);
            }
            catch (FabricException)
            {
                return new ContentResult {StatusCode = (int)System.Net.HttpStatusCode.ServiceUnavailable, Content = "The service was unable to process the request. Please try again."};
            }
        }

        // GET api/values/name
        [HttpGet("{name}")]
        public async Task<IActionResult> Get(string name)
        {
            try
            {
                IReliableDictionary<string, string> dictionary =
                    await stateManager.GetOrAddAsync<IReliableDictionary<string, string>>(ValuesDictionaryName);

                using (ITransaction tx = stateManager.CreateTransaction())
                {
                    ConditionalValue<string> result = await dictionary.TryGetValueAsync(tx, name);

                    if (result.HasValue)
                    {
                        return Ok(result.Value);
                    }

                    return NotFound();
                }
            }
            catch (FabricNotPrimaryException)
            {
                return new ContentResult {StatusCode = (int)System.Net.HttpStatusCode.Gone, Content = "The primary replica has moved. Please re-resolve the service."};
            }
            catch (FabricException)
            {
                return new ContentResult {StatusCode = (int)System.Net.HttpStatusCode.ServiceUnavailable, Content = "The service was unable to process the request. Please try again."};
            }
        }

        // POST api/values/name
        [HttpPost("{name}")]
        public async Task<IActionResult> Post(string name, [FromBody] ValueViewModel input)
        {
            if (input == null)
            {
                using (StreamReader reader = new(Request.Body, Encoding.UTF8))
                {
                    string jsonString = await reader.ReadToEndAsync();

                    if (!string.IsNullOrWhiteSpace(jsonString))
                    {
                        try
                        {
                            input = JsonSerializer.Deserialize<ValueViewModel>(jsonString);
                        }
                        catch (JsonException je)
                        {
                            return new ContentResult { StatusCode = (int)System.Net.HttpStatusCode.BadRequest, Content = $"Unabled to process request: {je.Message}" };
                        }
                    }
                }
            }

            try
            {
                IReliableDictionary<string, string> dictionary =
                    await stateManager.GetOrAddAsync<IReliableDictionary<string, string>>(ValuesDictionaryName);

                using (ITransaction tx = stateManager.CreateTransaction())
                {
                    await dictionary.SetAsync(tx, name, input.Value);
                    await tx.CommitAsync();
                }

                return Ok();
            }
            catch (FabricNotPrimaryException)
            {
                return new ContentResult {StatusCode = (int)System.Net.HttpStatusCode.Gone, Content = "The primary replica has moved. Please re-resolve the service."};
            }
            catch (FabricException)
            {
                return new ContentResult {StatusCode = (int)System.Net.HttpStatusCode.ServiceUnavailable, Content = "The service was unable to process the request. Please try again."};
            }
        }

        // PUT api/values/5
        [HttpPut("{name}")]
        public async Task<IActionResult> Put(string name, [FromBody] ValueViewModel input)
        {
            if (input == null)
            {
                using (StreamReader reader = new(Request.Body, Encoding.UTF8))
                {
                    string jsonString = await reader.ReadToEndAsync();

                    if (!string.IsNullOrWhiteSpace(jsonString))
                    {
                        try
                        {
                            input = JsonSerializer.Deserialize<ValueViewModel>(jsonString);
                        }
                        catch (JsonException je)
                        {
                            return new ContentResult { StatusCode = (int)System.Net.HttpStatusCode.BadRequest, Content = $"Unabled to process request: {je.Message}" };
                        }
                    }
                }
            }

            try
            {
                if (input == null)
                {
                    return new ContentResult { StatusCode = (int)System.Net.HttpStatusCode.BadRequest, Content = $"Unabled to process request: {typeof(ValueViewModel).FullName} is null." };
                }

                IReliableDictionary<string, string> dictionary =
                    await stateManager.GetOrAddAsync<IReliableDictionary<string, string>>(ValuesDictionaryName);

                using (ITransaction tx = stateManager.CreateTransaction())
                {
                    await dictionary.AddAsync(tx, name.Trim(), input.Value);
                    await tx.CommitAsync();
                }
            }
            catch (ArgumentException)
            {
                return new ContentResult { StatusCode = (int)System.Net.HttpStatusCode.BadRequest, Content = $"A key with name {name} already exists. Keys must be unique." };
            }
            catch (FabricNotPrimaryException)
            {
                return new ContentResult { StatusCode = (int)System.Net.HttpStatusCode.Gone, Content = "The primary replica has moved. Please re-resolve the service." };
            }
            catch (FabricException)
            {
                return new ContentResult { StatusCode = (int)System.Net.HttpStatusCode.ServiceUnavailable, Content = "The service was unable to process the request. Please try again." };
            }

            return Ok();
        }

        // DELETE api/valuesname
        [HttpDelete("{name}")]
        public async Task<IActionResult> Delete(string name)
        {
            IReliableDictionary<string, string> dictionary =
                await stateManager.GetOrAddAsync<IReliableDictionary<string, string>>(ValuesDictionaryName);

            try
            {
                using (ITransaction tx = stateManager.CreateTransaction())
                {
                    ConditionalValue<string> result = await dictionary.TryRemoveAsync(tx, name);

                    await tx.CommitAsync();

                    if (result.HasValue)
                    {
                        return Ok();
                    }

                    return new ContentResult {StatusCode = (int)System.Net.HttpStatusCode.BadRequest, Content = $"A value with name {name} doesn't exist."};
                }
            }
            catch (FabricNotPrimaryException)
            {
                return new ContentResult {StatusCode = (int)System.Net.HttpStatusCode.ServiceUnavailable, Content = "The primary replica has moved. Please re-resolve the service."};
            }
        }
    }
}