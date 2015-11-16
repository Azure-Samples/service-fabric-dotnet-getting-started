// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace VisualObjects.WebService
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.ServiceFabric.Actors;
    using VisualObjects.Common;

    /// <summary>
    /// This class encapsulates the logic for getting all of the visual object actors and joining their values into a JSON string.
    /// </summary>
    public class VisualObjectsBox : IVisualObjectsBox
    {
        private readonly Uri serviceUri;
        private readonly IEnumerable<ActorId> objectIds;

        public VisualObjectsBox(Uri serviceUri, int numObjects = 7)
        {
            this.serviceUri = serviceUri;
            this.objectIds = CreateVisualObjectActorIds(numObjects);
        }

        public async Task<string> GetObjectsAsync(CancellationToken cancellationToken)
        {
            List<Task<string>> tasks = this.objectIds.Select(objectId => this.GetObjectAsync(objectId, cancellationToken)).ToList();

            await Task.WhenAll(tasks);

            return "[" + String.Join(",", tasks.Select(task => task.Result)) + "]";
        }

        private Task<string> GetObjectAsync(ActorId objectId, CancellationToken cancellationToken)
        {
            IVisualObjectActor actorProxy = ActorProxy.Create<IVisualObjectActor>(objectId, this.serviceUri);

            try
            {
                return actorProxy.GetStateAsJsonAsync();
            }
            catch (Exception)
            {
                // ignore the exceptions
                return Task.FromResult(String.Empty);
            }
        }

        private static IEnumerable<ActorId> CreateVisualObjectActorIds(int numObjects)
        {
            ActorId[] actorIds = new ActorId[numObjects];
            for (int i = 0; i < actorIds.Length; i++)
            {
                actorIds[i] = new ActorId(string.Format(CultureInfo.InvariantCulture, "Visual Object # {0}", i));
            }

            return actorIds;
        }
    }
}