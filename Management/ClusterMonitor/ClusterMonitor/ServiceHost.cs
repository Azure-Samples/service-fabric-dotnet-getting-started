// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------


namespace ClusterMonitor
{
    using Microsoft.ServiceFabric.Services.Runtime;
    using System;
    using System.Diagnostics;
    using System.Fabric;
    using System.IO;
    using System.Threading;

    public static class ServiceHost
    {
        public static void Main(string[] args)
        {
            try
            {                
                using (TextWriterTraceListener trace = new TextWriterTraceListener(Path.Combine(FabricRuntime.GetActivationContext().LogDirectory, "out.log")))
                {
                    Trace.AutoFlush = true;
                    Trace.Listeners.Add(trace);

                    ServiceRuntime.RegisterServiceAsync(
                        Service.ServiceTypeName,
                        context => new Service(context)).GetAwaiter().GetResult();                                        

                    ServiceEventSource.Current.ServiceTypeRegistered(Process.GetCurrentProcess().Id, Service.ServiceTypeName);

                    Thread.Sleep(Timeout.Infinite);
                }
            }
            catch (Exception e)
            {
                Trace.WriteLine(e);
                ServiceEventSource.Current.ServiceHostInitializationFailed(e);
            }
        }
    }
}