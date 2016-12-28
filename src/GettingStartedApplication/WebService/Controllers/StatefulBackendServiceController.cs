using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace WebService.Controllers
{
    [Route("api/[controller]")]
    public class StatefulBackendServiceController : Controller
    {
        // GET: api/values
        [HttpGet]
        public async Task<IActionResult> GetAsync()
        {
            List<KeyValuePair<string, string>> result = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("key1", "value1"),
                new KeyValuePair<string, string>("key2", "value2")
            };

            return Json(result);
        }

        // PUT api/values
        [HttpPut]
        public async Task<IActionResult> PostAsync([FromBody]KeyValuePair<string, string> keyValuePair)
        {
            int partitionKeyNumber;

            try
            {
                string key = keyValuePair.Key;

                // Should we validate this in the UI or here in the controller?
                if (!String.IsNullOrEmpty(key))
                {
                    partitionKeyNumber = GetPartitionKey(key);
                }
                else
                {
                    throw new ArgumentException("No key provided");
                }
            }
            catch (Exception ex)
            {
                return new ContentResult { StatusCode = 400, Content = ex.Message };
            }

            // Probably need some more info from the backend that just a bool, to pass on to the client
            bool result = await AddKeyValuePairToStatefulBackendService(partitionKeyNumber, keyValuePair);

            // How do we pass on errors from the statefulbackend?
            if (result)
            {
                return Json(result);
            }
            else
            {
                return new ContentResult { StatusCode = 503, Content = "Something went wrong." };
            }
        }

        #region Helper Methods
        private static int GetPartitionKey(string key)
        {
            // The partitioning scheme of the processing service is a range of integers from 0 - 25.
            // This generates a partition key within that range by converting the first letter of the input name
            // into its numerica position in the alphabet.
            char firstLetterOfKey = key.First();
            int partitionKeyInt = Char.ToUpper(firstLetterOfKey) - 'A';

            if (partitionKeyInt < 0 || partitionKeyInt > 25)
            {
                throw new ArgumentException("The key must begin with a letter between A and Z");
            }

            return partitionKeyInt;
        }

        private async Task<bool> AddKeyValuePairToStatefulBackendService(int partitionKey, KeyValuePair<string, string> keyValuePair)
        {
            var result = await Task.FromResult(true);

            return result;
        }

        #endregion

        #region NotImplemented HTTPMethods

        // GET api/values/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            throw new NotImplementedException("No method implemented to get a specific key/value pair from the Stateful Backend Service");
        }

        // POST api/values/5
        [HttpPost("{id}")]
        public void Put(int id, [FromBody]string value)
        {
            throw new NotImplementedException("No method implemented to update the entire dictionary of key/value pairs in the Stateful Backend Service");
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
            throw new NotImplementedException("No method implemented to delete a specified key/value pair in the Stateful Backend Service");
        }

        #endregion

    }
}
