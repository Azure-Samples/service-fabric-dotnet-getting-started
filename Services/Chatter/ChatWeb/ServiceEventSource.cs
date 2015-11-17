// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace ChatWeb
{
    using System.Diagnostics.Tracing;
    using System.Fabric;
    using Microsoft.AspNet.Mvc;

    [EventSource(Name = "ServiceFabricSamples-Chatter-ChatWeb")]
    internal sealed class ServiceEventSource : EventSource
    {
        public static ServiceEventSource Current = new ServiceEventSource();

        [NonEvent]
        public void Message(string message, params object[] args)
        {
            if (this.IsEnabled())
            {
                string finalMessage = string.Format(message, args);
                this.Message(finalMessage);
            }
        }

        [Event(1, Level = EventLevel.Informational, Message = "{0}")]
        public void Message(string message)
        {
            if (this.IsEnabled())
            {
                this.WriteEvent(1, message);
            }
        }

        [NonEvent]
        public void WebControllerMessage(Controller controller, string message, params object[] args)
        {
            if (this.IsEnabled())
            {                
                string finalMessage = string.Format(message, args);
                this.ServiceMessage(
                    controller.ActionContext.HttpContext.Connection.LocalPort,
                    FabricRuntime.GetNodeContext().NodeName,
                    finalMessage);
            }
        }

        [Event(2, Level = EventLevel.Informational, Message = "{2}")]
        private void ServiceMessage(int localPort, string nodeName, string message)
        {
            if (this.IsEnabled())
            {
                this.WriteEvent(2, localPort, nodeName, message);
            }
        }
    }
}