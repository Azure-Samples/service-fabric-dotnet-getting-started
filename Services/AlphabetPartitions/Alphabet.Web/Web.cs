// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Alphabet.WebApi
{
    using System;
    using System.Collections.Generic;
    using System.Fabric;
    using System.Fabric.Description;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.ServiceFabric.Services.Client;
    using Microsoft.ServiceFabric.Services.Communication.Runtime;
    using Microsoft.ServiceFabric.Services.Runtime;
    using Newtonsoft.Json.Linq;

    public class Web : StatelessService
    {
        private static readonly Uri alphabetServiceUri = new Uri(@"fabric:/AlphabetPartitions/Processing");
        private readonly ServicePartitionResolver servicePartitionResolver = ServicePartitionResolver.GetDefault();
        private readonly HttpClient httpClient = new HttpClient();

        public Web(StatelessServiceContext context): base(context)
        {
        }

        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new[] {new ServiceInstanceListener(context => this.CreateInputListener(context))};
        }

        private ICommunicationListener CreateInputListener(ServiceContext context)
        {
            // Service instance's URL is the node's IP & desired port
            EndpointResourceDescription inputEndpoint = context.CodePackageActivationContext.GetEndpoint("WebApiServiceEndpoint");

            // This is the public-facing URL that HTTP clients, e.g., web browsers, can connect to.
            // The "alphabetpartitions" path is a unique URL prefix for this service so that other
            // services that might be hosted on the same node can also use this port with their own unique URL prefix.
            string uriPrefix = String.Format("{0}://+:{1}/alphabetpartitions/", inputEndpoint.Protocol, inputEndpoint.Port);
            
            // The published URL is slightly different from the listening URL prefix.
            // The listening URL is given to HttpListener.
            // The published URL is the URL that is published to the Service Fabric Naming Service,
            // which is used for service discovery. Clients will ask for this address through that discovery service.
            // The address that clients get needs to have the actual IP or FQDN of the node in order to connect,
            // so we need to replace '+' with the node's IP or FQDN.
            string uriPublished = uriPrefix.Replace("+", FabricRuntime.GetNodeContext().IPAddressOrFQDN);

            return new HttpCommunicationListener(uriPrefix, uriPublished, this.ProcessInputRequest);
        }

        private async Task ProcessInputRequest(HttpListenerContext context, CancellationToken cancelRequest)
        {
            String output = null;

            try
            {
                string lastname = context.Request.QueryString["lastname"];

                // The partitioning scheme of the processing service is a range of integers from 0 - 25.
                // This generates a partition key within that range by converting the first letter of the input name
                // into its numerica position in the alphabet.
                char firstLetterOfLastName = lastname.First();
                ServicePartitionKey partitionKey = new ServicePartitionKey(Char.ToUpper(firstLetterOfLastName) - 'A');                

                // This contacts the Service Fabric Naming Services to get the addresses of the replicas of the processing service 
                // for the partition with the partition key generated above. 
                // Note that this gets the most current addresses of the partition's replicas,
                // however it is possible that the replicas have moved between the time this call is made and the time that the address is actually used
                // a few lines below.
                // For a complete solution, a retry mechanism is required.
                // For more information, see http://aka.ms/servicefabricservicecommunication
                ResolvedServicePartition partition = await this.servicePartitionResolver.ResolveAsync(alphabetServiceUri, partitionKey, cancelRequest);
                ResolvedServiceEndpoint ep = partition.GetEndpoint();
                
                JObject addresses = JObject.Parse(ep.Address);
                string primaryReplicaAddress = (string)addresses["Endpoints"].First();

                UriBuilder primaryReplicaUriBuilder = new UriBuilder(primaryReplicaAddress);
                primaryReplicaUriBuilder.Query = "lastname=" + lastname;

                string result = await this.httpClient.GetStringAsync(primaryReplicaUriBuilder.Uri);

                output = String.Format(
                    "Result: {0}. <p>Partition key: '{1}' generated from the first letter '{2}' of input value '{3}'. <br>Processing service partition ID: {4}. <br>Processing service replica address: {5}",
                    result,
                    partitionKey,
                    firstLetterOfLastName,
                    lastname,
                    partition.Info.Id,
                    primaryReplicaAddress);
            }
            catch (Exception ex)
            {
                output = ex.Message;
            }

            using (HttpListenerResponse response = context.Response)
            {
                if (output != null)
                {
                    response.ContentType = "text/html";

                    byte[] outBytes = Encoding.UTF8.GetBytes(output);
                    response.OutputStream.Write(outBytes, 0, outBytes.Length);
                }
            }
        }
    }
}