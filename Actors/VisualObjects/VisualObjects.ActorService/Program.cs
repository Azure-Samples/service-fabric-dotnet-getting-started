// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace VisualObjects.ActorService
{
    using Microsoft.ServiceFabric.Actors.Runtime;
    using System;
    using System.Threading;
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                ActorRuntime.RegisterActorAsync<StatefulVisualObjectActor>().GetAwaiter().GetResult();

                Thread.Sleep(Timeout.Infinite);

            }
            catch (Exception e)
            {
                ActorEventSource.Current.ActorHostInitializationFailed(e.ToString());
                throw;
            }
        }
    }
}