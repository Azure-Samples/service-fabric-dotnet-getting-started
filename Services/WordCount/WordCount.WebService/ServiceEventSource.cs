// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace WordCount.WebService
{
    using System;
    using System.Diagnostics.Tracing;

    [EventSource(Name = "MyCompany-WordCount-WordCount.WebService")]
    internal sealed class ServiceEventSource : EventSource
    {
        public static ServiceEventSource Current = new ServiceEventSource();

        [Event(1, Level = EventLevel.Verbose)]
        public void MessageEvent(string message)
        {
            if (this.IsEnabled())
            {
                this.WriteEvent(1, message);
            }
        }

        [Event(2, Level = EventLevel.Informational, Message = "Service host {0} registered service type {1}")]
        public void ServiceTypeRegistered(int hostProcessId, string serviceType)
        {
            this.WriteEvent(2, hostProcessId, serviceType);
        }

        [NonEvent]
        public void ServiceHostInitializationFailed(Exception e)
        {
            this.ServiceHostInitializationFailed(e.ToString());
        }

        [Event(3, Level = EventLevel.Error, Message = "Service host initialization failed")]
        private void ServiceHostInitializationFailed(string exception)
        {
            this.WriteEvent(3, exception);
        }

        [Event(4, Level = EventLevel.Error, Message = "Encountered exception {0} during {1}")]
        public void OperationFailed(string exception, string operation)
        {
            this.WriteEvent(4, exception, operation);
        }
    }
}