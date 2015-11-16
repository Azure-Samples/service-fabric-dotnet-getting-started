// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace WordCount.Service
{
    using System;
    using System.Fabric;
    using System.Threading;

    /// <summary>
    /// The service host is the executable that hosts the Service instances.
    /// </summary>
    public class Program
    {
        public static void Main(string[] args)
        {
            // Create a Service Fabric Runtime and register the service type.
            try
            {
                using (FabricRuntime fabricRuntime = FabricRuntime.Create())
                {
                    // This is the name of the ServiceType that is registered with FabricRuntime. 
                    // This name must match the name defined in the ServiceManifest. If you change
                    // this name, please change the name of the ServiceType in the ServiceManifest.
                    fabricRuntime.RegisterServiceType("WordCountServiceType", typeof(WordCountService));
                    Thread.Sleep(Timeout.Infinite);
                }
            }
            catch (Exception e)
            {
                ServiceEventSource.Current.ServiceHostInitializationFailed(e);
            }
        }
    }
}