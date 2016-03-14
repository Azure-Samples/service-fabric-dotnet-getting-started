// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Alphabet.Processing
{
    using Microsoft.ServiceFabric.Services.Runtime;
    using System;
    using System.Diagnostics;
    using System.Fabric;
    using System.Threading;

    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {                
                // This is the name of the ServiceType that is registered with FabricRuntime. 
                // This name must match the name defined in the ServiceManifest. If you change
                // this name, please change the name of the ServiceType in the ServiceManifest.                    
                ServiceRuntime.RegisterServiceAsync("ProcessingType",
                context => new Processing(context)).GetAwaiter().GetResult();

                ServiceEventSource.Current.ServiceTypeRegistered(Process.GetCurrentProcess().Id, typeof(Processing).Name);

                Thread.Sleep(Timeout.Infinite);                
            }
            catch (Exception e)
            {
                ServiceEventSource.Current.ServiceHostInitializationFailed(e);
                throw;
            }
        }
    }
}