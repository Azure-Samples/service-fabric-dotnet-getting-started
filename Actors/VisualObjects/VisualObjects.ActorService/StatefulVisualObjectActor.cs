// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace VisualObjects.ActorService
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.ServiceFabric.Actors;
    using VisualObjects.Common;

    [ActorService(Name = "VisualObjects.ActorService")]
    public class StatefulVisualObjectActor : StatefulActor<VisualObject>, IVisualObjectActor
    {
        private IActorTimer updateTimer;
        private string jsonString;

        [Readonly]
        public Task<string> GetStateAsJsonAsync()
        {
            return Task.FromResult(this.jsonString);
        }

        protected override Task OnActivateAsync()
        {
            if (this.State == null)
            {
                this.State = VisualObject.CreateRandom(this.Id.ToString(), new Random(this.Id.ToString().GetHashCode()));
            }

            this.jsonString = this.State.ToJson();

            // ACTOR MOVEMENT REFRESH
            this.updateTimer = this.RegisterTimer(this.MoveObject, null, TimeSpan.FromMilliseconds(10), TimeSpan.FromMilliseconds(10));
            return base.OnActivateAsync();
        }

        protected override Task OnDeactivateAsync()
        {
            if (this.updateTimer != null)
            {
                this.UnregisterTimer(this.updateTimer);
            }

            return base.OnDeactivateAsync();
        }

        private Task MoveObject(object state)
        {
            this.State.Move();

            // State.Move(true);

            this.jsonString = this.State.ToJson();

            return Task.FromResult(true);
        }
    }
}