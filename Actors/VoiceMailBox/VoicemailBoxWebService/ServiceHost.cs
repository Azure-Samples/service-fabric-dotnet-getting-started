// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.Service.Fabric.Samples.VoicemailBoxWebService
{
    using System;
    using System.Threading;
    using Microsoft.ServiceFabric.Services.Runtime;

    /// <summary>
    /// The service host is the executable that hosts the Service instances.
    /// </summary>
    public static class ServiceHost
    {
        public static void Main(string[] args)
        {
            try
            {
                ServiceRuntime.RegisterServiceAsync(
                    Service.ServiceTypeName,
                    context =>
                        new Service(context)).GetAwaiter().GetResult();

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