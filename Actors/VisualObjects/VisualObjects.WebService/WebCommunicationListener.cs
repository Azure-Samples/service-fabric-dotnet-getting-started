// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace VisualObjects.WebService
{
    using Microsoft.Owin.Hosting;
    using Microsoft.ServiceFabric.Services.Communication.Runtime;
    using System;
    using System.Fabric;
    using System.Fabric.Description;
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;

    public class WebCommunicationListener : ICommunicationListener
    {

        private readonly string appRoot;
        private string listeningAddress;
        private string publishAddress;

        // OWIN server handle.
        private IDisposable webApp;
        private readonly ServiceInitializationParameters serviceInitializationParameters;

        public WebCommunicationListener(string appRoot, ServiceInitializationParameters serviceInitializationParameters)
        {
            this.appRoot = appRoot;
            this.serviceInitializationParameters = serviceInitializationParameters;
        }

        public Task<string> OpenAsync(CancellationToken cancellationToken)
        {
            ServiceEventSource.Current.Message("Initialize");

            EndpointResourceDescription serviceEndpoint = serviceInitializationParameters.CodePackageActivationContext.GetEndpoint("ServiceEndpoint");
            int port = serviceEndpoint.Port;

            this.listeningAddress = string.Format(
                CultureInfo.InvariantCulture,
                "http://+:{0}/{1}",
                port,
                string.IsNullOrWhiteSpace(this.appRoot)
                    ? string.Empty
                    : this.appRoot.TrimEnd('/') + '/');

            this.publishAddress = this.listeningAddress.Replace("+", FabricRuntime.GetNodeContext().IPAddressOrFQDN);

            ServiceEventSource.Current.Message("Starting web server on {0}", this.listeningAddress);

            this.webApp = WebApp.Start<Startup>(this.listeningAddress);

            return Task.FromResult(this.publishAddress);
        }

        public Task CloseAsync(CancellationToken cancellationToken)
        {
            this.StopAll();
            return Task.FromResult(true);
        }

        public void Abort()
        {
            this.StopAll();
        }

        /// <summary>
        /// Stops, cancels, and disposes everything.
        /// </summary>
        private void StopAll()
        {
            try
            {
                if (this.webApp != null)
                {
                    ServiceEventSource.Current.Message("Stopping web server.");
                    this.webApp.Dispose();
                }
            }
            catch (ObjectDisposedException)
            {
            }
        }
    }
}