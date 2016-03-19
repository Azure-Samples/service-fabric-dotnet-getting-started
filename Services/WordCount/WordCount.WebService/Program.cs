// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace WordCount.WebService
{
    using System;
    using System.Threading;
    using Microsoft.ServiceFabric.Services.Runtime;

    /// <summary>
    /// The service host is the executable that hosts the Service instances.
    /// </summary>
    public class Program
    {
        public static void Main(string[] args)
        {
            // Create Service Fabric runtime and register the service type.
            try
            {
                // This is the name of the ServiceType that is registered with FabricRuntime. 
                // This name must match the name defined in the ServiceManifest. If you change
                // this name, please change the name of the ServiceType in the ServiceManifest.
                ServiceRuntime.RegisterServiceAsync(
                    "WordCountWebServiceType",
                    context =>
                        new WordCountWebService(context)).GetAwaiter().GetResult();

                Thread.Sleep(Timeout.Infinite);
            }
            catch (Exception e)
            {
                ServiceEventSource.Current.ServiceHostInitializationFailed(e);
            }
        }
    }
}