// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace WordCount.WebService.Controllers
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Fabric;
    using System.Fabric.Query;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using System.Web.Http;
    using Microsoft.ServiceFabric.Services;
    using Microsoft.ServiceFabric.Services.Communication.Client;
    using Microsoft.ServiceFabric.Services.Client;    /// <summary>
                                                      /// Default controller.
                                                      /// </summary>
    public class DefaultController : ApiController
    {
        private const string WordCountServiceName = "fabric:/WordCount/WordCountService";
        private const int MaxQueryRetryCount = 20;
        private static TimeSpan BackoffQueryDelay = TimeSpan.FromSeconds(3);
        private static FabricClient fabricClient = new FabricClient();

        private static CommunicationClientFactory clientFactory = new CommunicationClientFactory(
            ServicePartitionResolver.GetDefault(),
            TimeSpan.FromSeconds(10),
            TimeSpan.FromSeconds(3));

        [HttpGet]
        public HttpResponseMessage Index()
        {
            return this.View("WordCount.WebService.wwwroot.Index.html", "text/html");
        }

        [HttpGet]
        public async Task<HttpResponseMessage> Count()
        {
            // Get the list of representative service partition clients.
            IList<ServicePartitionClient<CommunicationClient>> partitionClients = await this.GetServicePartitionClientsAsync();

            // For each partition client, keep track of partition information and the number of words
            ConcurrentDictionary<Int64RangePartitionInformation, long> totals = new ConcurrentDictionary<Int64RangePartitionInformation, long>();
            IList<Task> tasks = new List<Task>(partitionClients.Count);
            foreach (ServicePartitionClient<CommunicationClient> partitionClient in partitionClients)
            {
                // partitionClient internally resolves the address and retries on transient errors based on the configured retry policy.
                tasks.Add(
                    partitionClient.InvokeWithRetryAsync(
                        client =>
                        {
                            Uri serviceAddress = new Uri(client.BaseAddress, "Count");

                            HttpWebRequest request = WebRequest.CreateHttp(serviceAddress);
                            request.Method = "GET";
                            request.Timeout = (int) client.OperationTimeout.TotalMilliseconds;
                            request.ReadWriteTimeout = (int) client.ReadWriteTimeout.TotalMilliseconds;

                            using (HttpWebResponse response = (HttpWebResponse) request.GetResponse())
                            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                            {
                                totals[client.ResolvedServicePartition.Info as Int64RangePartitionInformation] = Int64.Parse(reader.ReadToEnd().Trim());
                            }

                            return Task.FromResult(true);
                        }));
            }

            try
            {
                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                // Sample code: print exception
                ServiceEventSource.Current.OperationFailed(ex.Message, "Count - run web request");
            }

            StringBuilder sb = new StringBuilder();
            sb.Append("<h1> Total:");
            sb.Append(totals.Aggregate<KeyValuePair<Int64RangePartitionInformation, long>, long>(0, (total, next) => next.Value + total));
            sb.Append("</h1>");
            sb.Append("<table><tr><td>Partition ID</td><td>Key Range</td><td>Total</td></tr>");
            foreach (KeyValuePair<Int64RangePartitionInformation, long> partitionData in totals.OrderBy(partitionData => partitionData.Key.LowKey))
            {
                sb.Append("<tr><td>");
                sb.Append(partitionData.Key.Id);
                sb.Append("</td><td>");
                sb.AppendFormat("{0} - {1}", partitionData.Key.LowKey, partitionData.Key.HighKey);
                sb.Append("</td><td>");
                sb.Append(partitionData.Value);
                sb.Append("</td></tr>");
            }

            sb.Append("</table>");

            HttpResponseMessage message = new HttpResponseMessage();
            message.Content = new StringContent(sb.ToString(), Encoding.UTF8, "text/html");
            return message;
        }

        [HttpPost]
        public async Task<HttpResponseMessage> AddWord(string word)
        {
            // Determine the partition key that should handle the request
            long partitionKey = GetPartitionKey(word);

            // Use service partition client to resolve the service and partition key.
            // This determines the endpoint of the replica that should handle the request.
            // Internally, the service partition client handles exceptions and retries appropriately.
            ServicePartitionClient<CommunicationClient> servicePartitionClient = new ServicePartitionClient<CommunicationClient>(
                clientFactory,
                new Uri(WordCountServiceName),
                partitionKey);

            return await servicePartitionClient.InvokeWithRetryAsync(
                client =>
                {
                    Uri serviceAddress = new Uri(client.BaseAddress, string.Format("AddWord/{0}", word));

                    HttpWebRequest request = WebRequest.CreateHttp(serviceAddress);
                    request.Method = "PUT";
                    request.ContentLength = 0;
                    request.Timeout = (int) client.OperationTimeout.TotalMilliseconds;
                    request.ReadWriteTimeout = (int) client.ReadWriteTimeout.TotalMilliseconds;

                    using (HttpWebResponse response = (HttpWebResponse) request.GetResponse())
                    {
                        HttpResponseMessage message = new HttpResponseMessage();
                        message.Content = new StringContent(
                            String.Format("<h1>{0}</h1> added to partition <h2>{1}</h2> at {2}", word, client.ResolvedServicePartition.Info.Id, serviceAddress),
                            Encoding.UTF8,
                            "text/html");
                        return Task.FromResult<HttpResponseMessage>(message);
                    }
                });
        }

        /// <summary>
        /// Gets the partition key which serves the specified word.
        /// Note that the sample only accepts Int64 partition scheme. 
        /// </summary>
        /// <param name="word">The word that needs to be mapped to a service partition key.</param>
        /// <returns>A long representing the partition key.</returns>
        private static long GetPartitionKey(string word)
        {
            return ((long) char.ToUpper(word[0])) - 64;
        }

        /// <summary>
        /// Returns a list of service partition clients pointing to one key in each of the WordCount service partitions.
        /// The returned representative key is the min key served by each partition.
        /// </summary>
        /// <returns>The service partition clients pointing at a key in each of the WordCount service partitions.</returns>
        private async Task<IList<ServicePartitionClient<CommunicationClient>>> GetServicePartitionClientsAsync()
        {
            for (int i = 0; i < MaxQueryRetryCount; i++)
            {
                try
                {
                    // Get the list of partitions up and running in the service.
                    ServicePartitionList partitionList = await fabricClient.QueryManager.GetPartitionListAsync(new Uri(WordCountServiceName));

                    // For each partition, build a service partition client used to resolve the low key served by the partition.
                    IList<ServicePartitionClient<CommunicationClient>> partitionClients =
                        new List<ServicePartitionClient<CommunicationClient>>(partitionList.Count);
                    foreach (Partition partition in partitionList)
                    {
                        Int64RangePartitionInformation partitionInfo = partition.PartitionInformation as Int64RangePartitionInformation;
                        if (partitionInfo == null)
                        {
                            throw new InvalidOperationException(
                                string.Format(
                                    "The service {0} should have a uniform Int64 partition. Instead: {1}",
                                    WordCountServiceName,
                                    partition.PartitionInformation.Kind));
                        }

                        partitionClients.Add(
                            new ServicePartitionClient<CommunicationClient>(clientFactory, new Uri(WordCountServiceName), partitionInfo.LowKey));
                    }

                    return partitionClients;
                }
                catch (FabricTransientException ex)
                {
                    ServiceEventSource.Current.OperationFailed(ex.Message, "create representative partition clients");
                    if (i == MaxQueryRetryCount - 1)
                    {
                        throw;
                    }
                }

                await Task.Delay(BackoffQueryDelay);
            }

            throw new TimeoutException("Retry timeout is exhausted and creating representative partition clients wasn't successful");
        }
    }
}