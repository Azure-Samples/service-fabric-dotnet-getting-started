// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace VisualObjects.ActorService
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics.Tracing;
    using Microsoft.ServiceFabric.Actors;

    [EventSource(Name = "MyCompany-VisualObjectsApplication-StatefulVisualObjectActor")]
    internal sealed class ActorEventSource : EventSource
    {
        public static ActorEventSource Current = new ActorEventSource();

        [NonEvent]
        public void Message(string message, params object[] args)
        {
            if (this.IsEnabled())
            {
                string finalMessage = string.Format(message, args);
                this.Message(finalMessage);
            }
        }

        [NonEvent]
        public void Message(StatelessActor actor, string message, params object[] args)
        {
            if (this.IsEnabled())
            {
                string finalMessage = string.Format(message, args);
                this.ActorMessage(
                    actor.GetType().ToString(),
                    actor.Id.ToString(),
                    actor.ActorService.ServiceInitializationParameters.CodePackageActivationContext.ApplicationTypeName,
                    actor.ActorService.ServiceInitializationParameters.CodePackageActivationContext.ApplicationName,
                    actor.ActorService.ServiceInitializationParameters.ServiceTypeName,
                    actor.ActorService.ServiceInitializationParameters.ServiceName.ToString(),
                    actor.ActorService.ServiceInitializationParameters.PartitionId,
                    actor.ActorService.ServiceInitializationParameters.InstanceId,
                    finalMessage);
            }
        }

        [NonEvent]
        public void Message<TState>(StatefulActor<TState> actor, string message, params object[] args) where TState : class
        {
            if (this.IsEnabled())
            {
                string finalMessage = string.Format(message, args);
                this.ActorMessage(
                    actor.GetType().ToString(),
                    actor.Id.ToString(),
                    actor.ActorService.ServiceInitializationParameters.CodePackageActivationContext.ApplicationTypeName,
                    actor.ActorService.ServiceInitializationParameters.CodePackageActivationContext.ApplicationName,
                    actor.ActorService.ServiceInitializationParameters.ServiceTypeName,
                    actor.ActorService.ServiceInitializationParameters.ServiceName.ToString(),
                    actor.ActorService.ServiceInitializationParameters.PartitionId,
                    actor.ActorService.ServiceInitializationParameters.ReplicaId,
                    finalMessage);
            }
        }

        [NonEvent]
        public void ActorRegistrationFailed(Type actorType, Exception e)
        {
            if (this.IsEnabled())
            {
                this.ActorRegistrationFailed(actorType.ToString(), e.ToString());
            }
        }

        [Event(1, Level = EventLevel.Informational, Message = "{0}")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void Message(string message)
        {
            if (this.IsEnabled())
            {
                this.WriteEvent(1, message);
            }
        }

        [Event(2, Level = EventLevel.Informational, Message = "{8}")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void ActorMessage(
            string actorType,
            string actorId,
            string applicationTypeName,
            string applicationName,
            string serviceTypeName,
            string serviceName,
            Guid partitionId,
            long replicaOrInstanceId,
            string message)
        {
            if (this.IsEnabled())
            {
                this.WriteEvent(
                    2,
                    actorType,
                    actorId,
                    applicationTypeName,
                    applicationName,
                    serviceTypeName,
                    serviceName,
                    partitionId,
                    replicaOrInstanceId,
                    message);
            }
        }

        [Event(3, Level = EventLevel.Error, Message = "Registration of Actor {0} failed.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void ActorRegistrationFailed(string actorType, string exception)
        {
            if (this.IsEnabled())
            {
                this.WriteEvent(3, actorType, exception);
            }
        }
    }
}