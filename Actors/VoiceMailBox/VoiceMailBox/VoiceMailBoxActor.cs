// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.Service.Fabric.Samples.VoicemailBox
{
    using System;
    using System.Collections.Generic;
    using System.Fabric.Description;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Azure.Service.Fabric.Samples.VoicemailBox.Interfaces;
    using Microsoft.ServiceFabric.Actors.Runtime;
    using ServiceFabric.Actors;

    public class VoiceMailBoxActor : Actor, IVoicemailBoxActor
    {
        public VoiceMailBoxActor(ActorService actorService, ActorId actorId)
            : base (actorService, actorId)
        { }

        public async Task<List<Voicemail>> GetMessagesAsync()
        {
            VoicemailBox box = await this.StateManager.GetStateAsync<VoicemailBox>("State");

            return box.MessageList;
        }

        public async Task<string> GetGreetingAsync()
        {
            VoicemailBox box = await this.StateManager.GetStateAsync<VoicemailBox>("State");

            if (string.IsNullOrEmpty(box.Greeting))
            {
                ConfigurationSettings configSettings = this.ActorService.Context.CodePackageActivationContext.GetConfigurationPackageObject("Config").Settings;
                ConfigurationSection configSection = configSettings.Sections.FirstOrDefault(s => (s.Name == "GreetingConfig"));
                if (configSection != null)
                {
                    ConfigurationProperty defaultGreeting = configSection.Parameters.FirstOrDefault(p => (p.Name == "DefaultGreeting"));
                    if (defaultGreeting != null)
                    {
                        return defaultGreeting.Value;
                    }
                }

                return "No one is available, please leave a message after the beep.";
            }

            return box.Greeting;
        }

        public async Task LeaveMessageAsync(string message)
        {
            VoicemailBox box = await this.StateManager.GetStateAsync<VoicemailBox>("State");

            box.MessageList.Add(
                new Voicemail
                {
                    Id = Guid.NewGuid(),
                    Message = message,
                    ReceivedAt = DateTime.Now
                });

            await this.StateManager.SetStateAsync<VoicemailBox>("State", box);
        }

        public async Task SetGreetingAsync(string greeting)
        {
            VoicemailBox box = await this.StateManager.GetStateAsync<VoicemailBox>("State");

            box.Greeting = greeting;

            await this.StateManager.SetStateAsync<VoicemailBox>("State", box);
        }

        public async Task DeleteMessageAsync(Guid messageId)
        {
            VoicemailBox box = await this.StateManager.GetStateAsync<VoicemailBox>("State");

            box.MessageList.Remove(box.MessageList.Find(item => item.Id == messageId));

            await this.StateManager.SetStateAsync<VoicemailBox>("State", box);
        }

        public async Task DeleteAllMessagesAsync()
        {
            VoicemailBox box = await this.StateManager.GetStateAsync<VoicemailBox>("State");

            box.MessageList.Clear();

            await this.StateManager.SetStateAsync<VoicemailBox>("State", box);
        }

        protected override async Task OnActivateAsync()
        {
            ServiceEventSource.Current.ActorActivatedStart(this);

            await this.StateManager.TryAddStateAsync<VoicemailBox>("State", new VoicemailBox());

            await base.OnActivateAsync();

            ServiceEventSource.Current.ActorActivatedStop(this);
        }

        protected override async Task OnDeactivateAsync()
        {
            ServiceEventSource.Current.ActorDeactivatedStart(this);
            await base.OnDeactivateAsync();
            ServiceEventSource.Current.ActorDeactivatedStop(this);
        }
    }
}