// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Client
{
    using System;
    using System.Threading.Tasks;
    using Common;
    using Microsoft.ServiceFabric.Services.Remoting.Client;

    class Program
    {
        private static Uri ServiceName = new Uri("fabric:/ServiceRemotingCustomHeaders/Stateless1");

        static void Main(string[] args)
        {
            InvokeServiceMethod().GetAwaiter().GetResult();

            Console.ReadLine();
        }

        private static async Task InvokeServiceMethod()
        {
            ServiceProxyFactory proxyFactory = new ServiceProxyFactory(
                callbackClient => new MyServiceRemotingClientFactory(callbackClient));

            ITestService testServiceProxy = proxyFactory.CreateServiceProxy<ITestService>(ServiceName);

            string activityId = ActivityId.GetOrCreateActivityId();

            if (activityId == await testServiceProxy.GetCurrentActivityId())
            {
                Console.WriteLine("Activity ID is {0}", activityId);
            }
            else
            {
                Console.WriteLine("Local Activity id doesnt match activity id returned by remote method");
            }
        }
    }
}