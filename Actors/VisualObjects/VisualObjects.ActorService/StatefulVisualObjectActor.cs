// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace VisualObjects.ActorService
{
    using Microsoft.ServiceFabric.Actors.Runtime;
    using System;
    using System.Threading.Tasks;
    using VisualObjects.Common;

    [ActorService(Name = "VisualObjects.ActorService")]
    [StatePersistence(StatePersistence.Persisted)]
    public class StatefulVisualObjectActor : Actor, IVisualObjectActor
    {
        private IActorTimer updateTimer;
        private string jsonString;
        private static readonly string StatePropertyName = "VisualObject";

        public Task<string> GetStateAsJsonAsync()
        {
            return Task.FromResult(this.jsonString);
        }

        protected override async Task OnActivateAsync()
        {
            VisualObject newObject = VisualObject.CreateRandom(this.Id.ToString());

            ActorEventSource.Current.ActorMessage(this, "StateCheck {0}", (await this.StateManager.ContainsStateAsync(StatePropertyName)).ToString());

            var result = await this.StateManager.GetOrAddStateAsync<VisualObject>(StatePropertyName, newObject);

            this.jsonString = result.ToJson();

            // ACTOR MOVEMENT REFRESH
            this.updateTimer = this.RegisterTimer(this.MoveObject, null, TimeSpan.FromMilliseconds(10), TimeSpan.FromMilliseconds(20));
            return;
        }

        private async Task MoveObject(object obj)
        {

            var visualObject = await this.StateManager.GetStateAsync<VisualObject>(StatePropertyName);

            //alternate which lines are commented out
            //then do an upgrade to cause the
            //visual objects to start rotating

            visualObject.Move(false);
            //visualObject.Move(true);

            await this.StateManager.SetStateAsync<VisualObject>(StatePropertyName, visualObject);

            this.jsonString = visualObject.ToJson();

            return;
        }
    }
}