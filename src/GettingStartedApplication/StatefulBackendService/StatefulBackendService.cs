// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace StatefulBackendService
{
    using System.Collections.Generic;
    using System.Diagnostics.Tracing;
    using System.Fabric;
    using System.IO;
    using Microsoft.ApplicationInsights.ServiceFabric;
    using Microsoft.ApplicationInsights.Extensibility;
    using Microsoft.ApplicationInsights.EventSourceListener;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.ServiceFabric.Data;
    using Microsoft.ServiceFabric.Services.Communication.AspNetCore;
    using Microsoft.ServiceFabric.Services.Communication.Runtime;
    using Microsoft.ServiceFabric.Services.Runtime;

    /// <summary>
    /// The FabricRuntime creates an instance of this class for each service type instance. 
    /// </summary>
    internal sealed class StatefulBackendService : StatefulService
    {
        public StatefulBackendService(StatefulServiceContext context)
            : base(context)
        {
            var telemetryConfig = TelemetryConfiguration.Active;
            FabricTelemetryInitializerExtension.SetServiceCallContext(context);
            var config = context.CodePackageActivationContext.GetConfigurationPackageObject("Config");
            var appInsights = config.Settings.Sections["ApplicationInsights"];
            telemetryConfig.InstrumentationKey = appInsights.Parameters["InstrumentationKey"].Value;
        }

        /// <summary>
        /// Optional override to create listeners (like tcp, http) for this service instance.
        /// </summary>
        /// <returns>The collection of listeners.</returns>
        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return new ServiceReplicaListener[]
            {
                new ServiceReplicaListener(
                    serviceContext =>
                        new KestrelCommunicationListener(
                            serviceContext,
                            (url, listener) =>
                            {
                                ServiceEventSource.Current.ServiceMessage(serviceContext, $"Starting Kestrel on {url}");

                                return new WebHostBuilder()
                                    .UseKestrel()
                                    .ConfigureServices(
                                        services => services
                                            .AddSingleton<IReliableStateManager>(this.StateManager)
                                            .AddSingleton<StatefulServiceContext>(serviceContext)
                                            .AddSingleton<ITelemetryInitializer>((serviceProvider) => FabricTelemetryInitializerExtension.CreateFabricTelemetryInitializer(serviceContext))
                                            .AddSingleton<ITelemetryModule>((serviceProvider) => CreateEventSourceTelemetryModule()))
                                    .UseContentRoot(Directory.GetCurrentDirectory())
                                    .UseServiceFabricIntegration(listener, ServiceFabricIntegrationOptions.UseUniqueServiceUrl)
                                    .UseStartup<Startup>()
                                    .UseApplicationInsights()
                                    .UseUrls(url)
                                    .Build();
                            }))
            };
        }

        private EventSourceTelemetryModule CreateEventSourceTelemetryModule()
        {
            var module = new EventSourceTelemetryModule();
            module.Sources.Add(new EventSourceListeningRequest() { Name = "Microsoft-ServiceFabric-Services", Level = EventLevel.Verbose });
            module.Sources.Add(new EventSourceListeningRequest() { Name = "MyCompany-GettingStartedApplication-StatefulBackendService", Level = EventLevel.Verbose });
            return module;
        }
    }
}