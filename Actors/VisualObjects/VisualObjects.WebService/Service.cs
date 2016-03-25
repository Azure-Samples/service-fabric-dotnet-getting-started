// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace VisualObjects.WebService
{
    using Common;
    using Microsoft.ServiceFabric.Actors;
    using Microsoft.ServiceFabric.Actors.Client;
    using Microsoft.ServiceFabric.Services.Communication.Runtime;
    using Microsoft.ServiceFabric.Services.Runtime;
    using System;
    using System.Collections.Generic;
    using System.Fabric;
    using System.Fabric.Description;
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;
    public class Service : StatelessService
    {
        public const string ServiceTypeName = "VisualObjects.WebServiceType";
        private Uri ActorServiceUri = null;
        private IVisualObjectsBox objectBox;
        private IEnumerable<ActorId> actorIds;

        public Service(StatelessServiceContext serviceContext) : base(serviceContext)
        {
            ServiceContext context = serviceContext;

            ConfigurationPackage config = context.CodePackageActivationContext.GetConfigurationPackageObject("Config");
            ConfigurationSection section = config.Settings.Sections["VisualObjectsBoxSettings"];

            int numObjects = int.Parse(section.Parameters["ObjectCount"].Value);
            string serviceName = section.Parameters["ServiceName"].Value;
            string appName = context.CodePackageActivationContext.ApplicationName;

            this.ActorServiceUri = new Uri(appName + "/" + serviceName);
            this.objectBox = new VisualObjectsBox();
            this.actorIds = CreateVisualObjectActorIds(numObjects);
        }


        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new[]
            {
                new ServiceInstanceListener(
                    initParams => new WebCommunicationListener("visualobjects", initParams), "httpListener"),

                new ServiceInstanceListener(
                    initparams => new WebSocketApp(this.objectBox, "visualobjects", "data", initparams), "webSocketListener")
            };

        }

        protected override Task RunAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            List<Task> runners = new List<Task>();

            foreach (ActorId id in this.actorIds)
            {
                IVisualObjectActor actorProxy = ActorProxy.Create<IVisualObjectActor>(id, this.ActorServiceUri);

                Task t = Task.Run(async () =>
                {
                    while (true)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        try
                        {
                            this.objectBox.SetObjectString(id, await actorProxy.GetStateAsJsonAsync());
                        }
                        catch (Exception)
                        {
                            // ignore the exceptions
                            this.objectBox.SetObjectString(id, string.Empty);
                        }
                        finally
                        {
                            this.objectBox.computeJson();
                        }

                        await Task.Delay(TimeSpan.FromMilliseconds(10));
                    }
                }
                , cancellationToken);

                runners.Add(t);

            }

            return Task.WhenAll(runners);
        }

        private IEnumerable<ActorId> CreateVisualObjectActorIds(int numObjects)
        {
            ActorId[] actorIds = new ActorId[numObjects];
            for (int i = 0; i < actorIds.Length; i++)
            {
                actorIds[i] = new ActorId(string.Format(CultureInfo.InvariantCulture, "Visual Object # {0}", i));
                this.objectBox.SetObjectString(actorIds[i], string.Empty);
            }

            return actorIds;
        }
    }
}