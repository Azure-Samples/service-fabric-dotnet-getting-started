// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace ActorBackendService
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using ActorBackendService.Interfaces;
    using Microsoft.ServiceFabric.Actors;
    using Microsoft.ServiceFabric.Actors.Runtime;
    using Microsoft.ApplicationInsights;
    using Microsoft.ApplicationInsights.ServiceFabric;
    using Microsoft.ApplicationInsights.Extensibility;
    using System.Collections.Generic;
    using Microsoft.ApplicationInsights.DataContracts;
    using System.Diagnostics;

    /// <remarks>
    /// This class represents an actor.
    /// Every ActorID maps to an instance of this class.
    /// The StatePersistence attribute determines persistence and replication of actor state:
    ///  - Persisted: State is written to disk and replicated.
    ///  - Volatile: State is kept in memory only and replicated.
    ///  - None: State is kept in memory only and not replicated.
    /// </remarks>
    [StatePersistence(StatePersistence.Persisted)]
    internal class MyActor : Actor, IMyActor, IRemindable
    {
        private const string ReminderName = "Reminder";
        private const string StateName = "Count";
        private TelemetryClient telemetryClient;

        /// <summary>
        /// Initializes a new instance of ActorBackendService
        /// </summary>
        /// <param name="actorService">The Microsoft.ServiceFabric.Actors.Runtime.ActorService that will host this actor instance.</param>
        /// <param name="actorId">The Microsoft.ServiceFabric.Actors.ActorId for this actor instance.</param>
        public MyActor(ActorService actorService, ActorId actorId)
            : base(actorService, actorId)
        {
            telemetryClient = new TelemetryClient(TelemetryConfiguration.Active);
            FabricTelemetryInitializer.SetServiceCallContext(actorService.Context);
        }

        public async Task StartProcessingAsync(string requestId, Dictionary<string, string> correlationContextHeader, CancellationToken cancellationToken)
        {
            var telemetry = new RequestTelemetry();

            try
            {
                // Create a new activity for this new request, and end it when we finish processing
                StartActivity("StartProcessingAsync", telemetry, requestId, correlationContextHeader);
                this.GetReminder(ReminderName);
            
                bool added = await this.StateManager.TryAddStateAsync<long>(StateName, 0);

                if (!added)
                {
                    // value already exists, which means processing has already started.
                    throw new InvalidOperationException("Processing for this actor has already started.");
                }
            }
            catch (ReminderNotFoundException)
            {
                await this.RegisterReminderAsync(ReminderName, null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(10));
            }
            finally
            {
                this.StopActivity(telemetry);
            }
        }

        public async Task ReceiveReminderAsync(string reminderName, byte[] context, TimeSpan dueTime, TimeSpan period)
        {
            if (reminderName.Equals(ReminderName, StringComparison.OrdinalIgnoreCase))
            {
                long currentValue = await this.StateManager.GetStateAsync<long>(StateName);

                ActorEventSource.Current.ActorMessage(this, $"Processing. Current value: {currentValue}");

                await this.StateManager.SetStateAsync<long>(StateName, ++currentValue);
            }
        }

        /// <summary>
        /// This method is called whenever an actor is activated.
        /// An actor is activated the first time any of its methods are invoked.
        /// </summary>
        protected override async Task OnActivateAsync()
        {
            ActorEventSource.Current.ActorMessage(this, "Actor activated.");

            // The StateManager is this actor's private state store.
            // Data stored in the StateManager will be replicated for high-availability for actors that use volatile or persisted state storage.
            // Any serializable object can be saved in the StateManager.
            // For more information, see https://aka.ms/servicefabricactorsstateserialization
            await base.OnActivateAsync();
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
                telemetry.Name = "POST " + "StartProcessingAsync";
            }

            telemetry.HttpMethod = "POST";
            telemetry.Url = new Uri("fabric:/GettingStartedApplication/ActorBackendService/StartProcessingAsync");
            telemetryClient.TrackRequest(telemetry);

            // Stop and conclude the activity
            Activity.Current.Stop();
        }
    }
}