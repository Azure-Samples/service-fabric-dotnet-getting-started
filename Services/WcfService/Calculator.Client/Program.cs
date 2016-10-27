// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Calculator.Client
{
    using System;
    using System.ServiceModel;
    using System.Threading.Tasks;
    using Calculator.Common;
    using Microsoft.ServiceFabric.Services.Client;
    using Microsoft.ServiceFabric.Services.Communication.Client;
    using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;

    internal class Program
    {
        private static void Main(string[] args)
        {
            var client = new CalculatorClient(new Uri("fabric:/CalculatorApp/CalculatorService")) as ICalculator;
            var iteration = 0;
            while (true)
            {
                iteration++;
                Console.WriteLine("({1}) 2 + 3 = {0}", client.Add(2, 3).GetAwaiter().GetResult(), iteration);
            }
        }
    }

    ///<summary>
    /// It uses ClientFactoryBase which in turn provides various features like resolving endpoints during Service Failover  , ExceptionHandling and maintains a cache of communication
    /// clients and attempts to reuse the clients for requests to the same service endpoint.
    /// Its using BasicHttpBinding.
    ///  </summary>
    public class CalculatorClient : ServicePartitionClient<WcfCommunicationClient<ICalculator>>, ICalculator
    {
        private static ICommunicationClientFactory<WcfCommunicationClient<ICalculator>> communicationClientFactory;

        static CalculatorClient()
        {
            communicationClientFactory = new WcfCommunicationClientFactory<ICalculator>(
                clientBinding: new BasicHttpBinding());
        }

        public CalculatorClient(Uri serviceUri)
            : this(serviceUri, ServicePartitionKey.Singleton)
        {
        }

        public CalculatorClient(
            Uri serviceUri,
            ServicePartitionKey partitionKey)
            : base(
                communicationClientFactory,
                serviceUri,
                partitionKey)
        {
        }

        public Task<double> Add(double n1, double n2)
        {
            return this.InvokeWithRetry(
                (c) => c.Channel.Add(n1, n2));
        }
    }
}