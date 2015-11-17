// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.Service.Fabric.Samples.VoicemailBox
{
    using System;
    using System.Diagnostics;
    using System.Fabric;
    using System.Threading;
    using Microsoft.ServiceFabric.Actors;

    public class ServiceHost
    {
        public static void Main(string[] args)
        {
            try
            {
                using (FabricRuntime fabricRuntime = FabricRuntime.Create())
                {                    
                    fabricRuntime.RegisterActor<VoiceMailBoxActor>();
                    
                    ServiceEventSource.Current.ActorTypeRegistered(Process.GetCurrentProcess().Id, typeof(VoiceMailBoxActor).ToString());

                    Thread.Sleep(Timeout.Infinite);
                }
            }
            catch (Exception e)
            {
                ServiceEventSource.Current.ActorHostInitializationFailed(e);
                throw;
            }
        }
    }
}