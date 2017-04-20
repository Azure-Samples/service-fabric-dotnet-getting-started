// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace StatelessBackendService
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Fabric;
    using System.Threading;
    using System.Threading.Tasks;
    using global::StatelessBackendService.Interfaces;
    using Microsoft.ServiceFabric.Services.Communication.Runtime;
    using Microsoft.ServiceFabric.Services.Remoting.Runtime;
    using Microsoft.ServiceFabric.Services.Runtime;
    using Microsoft.ApplicationInsights.Extensibility;
    using Microsoft.ApplicationInsights;
    using Microsoft.ApplicationInsights.DataContracts;
    using Microsoft.ApplicationInsights.ServiceFabric;

    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class StatelessBackendService : StatelessService, IStatelessBackendService
    {
        private long iterations = 0;
        private TelemetryClient telemetryClient;

        public StatelessBackendService(StatelessServiceContext context)
            : base(context)
        {
            telemetryClient = new TelemetryClient(TelemetryConfiguration.Active);
            FabricTelemetryInitializer.SetServiceCallContext(context);
        }

        public Task<long> GetCountAsync(string requestId, Dictionary<string, string> correlationContextHeader)
        {
            var telemetry = new RequestTelemetry();

            // Create a new activity for this new request, and end it when we finish processing
            StartActivity("GetCountAsync", telemetry, requestId, correlationContextHeader);
            return Task.FromResult(this.iterations).ContinueWith((task) => {
                this.StopActivity(telemetry);
                return task.Result;
            });
        }

        /// <summary>
        /// Optional override to create listeners (e.g., TCP, HTTP) for this service replica to handle client or user requests.
        /// </summary>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new ServiceInstanceListener[1]
            {
                new ServiceInstanceListener(this.CreateServiceRemotingListener)
            };
        }

        /// <summary>
        /// This is the main entry point for your service instance.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service instance.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            // TODO: Replace the following sample code with your own logic 
            //       or remove this RunAsync override if it's not needed in your service.

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                ++this.iterations;

                ServiceEventSource.Current.ServiceMessage(this.Context, "Working-{0}", this.iterations);

                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
        }

        private void StartActivity(string callerName, RequestTelemetry telemetry, string requestId, Dictionary<string, string> correlationContextHeader)
        {
            // Initialize all needed properties on the activity object
            var activity = new Activity(callerName);
            activity.SetParentId(requestId);
            telemetry.Context.Operation.ParentId = requestId;
            
            foreach (KeyValuePair<string, string> pair in correlationContextHeader)
            {
                activity.AddBaggage(pair.Key, pair.Value);
            }

            activity.Start();

            // Initialize all needed properties on the request telemetry object, and start tracking
            telemetry.Id = activity.Id;
            telemetry.Context.Operation.Id = activity.RootId;
            telemetry.Context.Operation.ParentId = activity.ParentId;

            this.telemetryClient.Initialize(telemetry);
            telemetry.Start(Stopwatch.GetTimestamp());

            //IHeaderDictionary responseHeaders = httpContext.Response?.Headers;
            //if (responseHeaders != null &&
            //    !string.IsNullOrEmpty(telemetry.Context.InstrumentationKey) &&
            //    (!responseHeaders.ContainsKey(RequestResponseHeaders.RequestContextHeader) || HttpHeadersUtilities.ContainsRequestContextKeyValue(responseHeaders, RequestResponseHeaders.RequestContextTargetKey)))
            //{
            //    string correlationId = null;
            //    if (this.correlationIdLookupHelper.TryGetXComponentCorrelationId(telemetry.Context.InstrumentationKey, out correlationId))
            //    {
            //        HttpHeadersUtilities.SetRequestContextKeyValue(responseHeaders, RequestResponseHeaders.RequestContextTargetKey, correlationId);
            //    }
            //}
        }

        private void StopActivity(RequestTelemetry telemetry)
        {
            // Stop the request telemetry tracking and log it with the telemetry client
            telemetry.Stop(Stopwatch.GetTimestamp());
            telemetry.Success = true;

            if (string.IsNullOrEmpty(telemetry.Name))
            {
                telemetry.Name = "GET " + "GetCountAsync";
            }

            telemetry.HttpMethod = "GET";
            telemetry.Url = new Uri("fabric:/GettingStartedApplication/StatelessBackendService/GetCountAsync");
            telemetryClient.TrackRequest(telemetry);

            // Stop and conclude the activity
            Activity.Current.Stop();
        }
    }
}