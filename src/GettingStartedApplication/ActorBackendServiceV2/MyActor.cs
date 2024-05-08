// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;
using ActorBackendService.Interfaces;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;

namespace ActorBackendService
{
    /// <remarks>
    /// This class represents an actor.
    /// Every ActorID maps to an instance of this class.
    /// The StatePersistence attribute determines persistence and replication of actor state:
    ///  - Persisted: State is written to disk and replicated.
    ///  - Volatile: State is kept in memory only and replicated.
    ///  - None: State is kept in memory only and not replicated.
    /// </remarks>
    /// <summary>
    /// Initializes a new instance of ActorBackendService
    /// </summary>
    /// <param name="actorService">The Microsoft.ServiceFabric.Actors.Runtime.ActorService that will host this actor instance.</param>
    /// <param name="actorId">The Microsoft.ServiceFabric.Actors.ActorId for this actor instance.</param>
    [StatePersistence(StatePersistence.Persisted)]
    internal class MyActor(ActorService actorService, ActorId actorId) : Actor(actorService, actorId), IMyActor, IRemindable
    {
        private const string ReminderName = "Reminder";
        private const string StateName = "Count";

        public async Task StartProcessingAsync(CancellationToken cancellationToken)
        {
            try
            {
                GetReminder(ReminderName);
            }
            catch (ReminderNotFoundException)
            {
                await RegisterReminderAsync(ReminderName, null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(10));
            }

            bool added = await StateManager.TryAddStateAsync<long>(StateName, 0, cancellationToken);

            if (!added)
            {
                // value already exists, which means processing has already started.
                throw new InvalidOperationException("Processing for this actor has already started.");
            }

        }

        public async Task ReceiveReminderAsync(string reminderName, byte[] context, TimeSpan dueTime, TimeSpan period, CancellationToken cancellationToken)
        {
            if (reminderName.Equals(ReminderName, StringComparison.OrdinalIgnoreCase))
            {
                long currentValue = await StateManager.GetStateAsync<long>(StateName, cancellationToken);
                ActorEventSource.Current.ActorMessage(this, $"Processing actorID: {Id}. Current value: {currentValue}");
                
                await StateManager.SetStateAsync(StateName, ++currentValue, cancellationToken);

                long newValue = await StateManager.GetStateAsync<long>(StateName, cancellationToken);
                ActorEventSource.Current.ActorMessage(this, $"ActorID: {Id}. New value: {newValue}");
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

        public Task ReceiveReminderAsync(string reminderName, byte[] state, TimeSpan dueTime, TimeSpan period)
        {
            return ReceiveReminderAsync(reminderName, state, dueTime, period, CancellationToken.None);
        }
    }
}
