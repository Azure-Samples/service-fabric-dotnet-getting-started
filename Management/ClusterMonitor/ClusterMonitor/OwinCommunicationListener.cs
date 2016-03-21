// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace ClusterMonitor
{
    using System;
    using System.Diagnostics;
    using System.Fabric;
    using System.Fabric.Description;
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Owin.Hosting;
    using Microsoft.ServiceFabric.Services;
    using Microsoft.ServiceFabric.Services.Communication.Runtime;

    public class OwinCommunicationListener : ICommunicationListener
    {
        /// <summary>
        /// OWIN server handle.
        /// </summary>
        private IDisposable serverHandle;

        private IOwinAppBuilder startup;
        private string publishAddress;
        private string listeningAddress;
        private string appRoot;
        private readonly ServiceContext serviceContext;

        public OwinCommunicationListener(IOwinAppBuilder startup, ServiceContext serviceContext)
            : this(null, startup, serviceContext)
        {
        }

        public OwinCommunicationListener(string appRoot, IOwinAppBuilder startup, ServiceContext serviceContext)
        {
            this.startup = startup;
            this.appRoot = appRoot;
            this.serviceContext = serviceContext;
        }

        public bool ListenOnSecondary { get; set; }
                
        public Task<string> OpenAsync(CancellationToken cancellationToken)
        {
            Trace.WriteLine("Initialize");

            EndpointResourceDescription serviceEndpoint = this.serviceContext.CodePackageActivationContext.GetEndpoint("ServiceEndpoint");
            int port = serviceEndpoint.Port;

            if (serviceContext is StatefulServiceContext)
            {                
                this.listeningAddress = String.Format(
                    CultureInfo.InvariantCulture,
                    "http://+:{0}/{1}/{2}/{3}",
                    port,
                    this.serviceContext.PartitionId,
                    this.serviceContext.ReplicaOrInstanceId,
                    Guid.NewGuid());
            }
            else if (serviceContext is StatelessServiceContext)
            {
                this.listeningAddress = String.Format(
                    CultureInfo.InvariantCulture,
                    "http://+:{0}/{1}",
                    port,
                    String.IsNullOrWhiteSpace(this.appRoot)
                        ? String.Empty
                        : this.appRoot.TrimEnd('/') + '/');
            }
            else
            {
                throw new InvalidOperationException();
            }

            this.publishAddress = this.listeningAddress.Replace("+", FabricRuntime.GetNodeContext().IPAddressOrFQDN);

            Trace.WriteLine("Opening on " + this.listeningAddress);

            try
            {
                Trace.WriteLine("Starting web server on " + this.listeningAddress);

                this.serverHandle = WebApp.Start(this.listeningAddress, appBuilder => this.startup.Configuration(appBuilder));

                return Task.FromResult(this.publishAddress);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);

                this.StopWebServer();

                throw;
            }
        }

        public Task CloseAsync(CancellationToken cancellationToken)
        {
            Trace.WriteLine("Close");

            this.StopWebServer();

            return Task.FromResult(true);
        }

        public void Abort()
        {
            Trace.WriteLine("Abort");

            this.StopWebServer();
        }

        private void StopWebServer()
        {
            if (this.serverHandle != null)
            {
                try
                {
                    this.serverHandle.Dispose();
                }
                catch (ObjectDisposedException)
                {
                    // no-op
                }
            }
        }
    }
}