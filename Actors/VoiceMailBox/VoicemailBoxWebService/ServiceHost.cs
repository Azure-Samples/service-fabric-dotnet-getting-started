// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.Service.Fabric.Samples.VoicemailBoxWebService
{
    using System;
    using System.Diagnostics;
    using System.Fabric;
    using System.Threading;

    /// <summary>
    /// The service host is the executable that hosts the Service instances.
    /// </summary>
    public static class ServiceHost
    {
        public static void Main(string[] args)
        {
            try
            {
                // Create a Service Fabric Runtime
                using (FabricRuntime fabricRuntime = FabricRuntime.Create())
                {
                    fabricRuntime.RegisterServiceType(Service.ServiceTypeName, typeof(Service));
                    Thread.Sleep(Timeout.Infinite);
                }
            }
            catch (Exception e)
            {
                ServiceEventSource.Current.ServiceHostInitializationFailed(e);
                throw;
            }
        }
    }
}