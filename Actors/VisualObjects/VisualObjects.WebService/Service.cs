// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace VisualObjects.WebService
{
    using System;
    using System.Fabric;
    using System.Fabric.Description;
    using Microsoft.ServiceFabric.Services;
    using System.Collections.Generic;
    using Microsoft.ServiceFabric.Services.Runtime;
    using Microsoft.ServiceFabric.Services.Communication.Runtime;
    public class Service : StatelessService
    {
        public const string ServiceTypeName = "VisualObjects.WebServiceType";

        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            // get some configuration values from the service's config package (PackageRoot\Config\Settings.xml)
            ConfigurationPackage config = this.ServiceInitializationParameters.CodePackageActivationContext.GetConfigurationPackageObject("Config");
            ConfigurationSection section = config.Settings.Sections["VisualObjectsBoxSettings"];

            int numObjects = int.Parse(section.Parameters["ObjectCount"].Value);
            string serviceName = section.Parameters["ServiceName"].Value;
            string appName = this.ServiceInitializationParameters.CodePackageActivationContext.ApplicationName;

            return new[] 
            {
                new ServiceInstanceListener(
                    initParams => new WebCommunicationListener(new VisualObjectsBox(new Uri(appName + "/" + serviceName), numObjects), "visualobjects", "data", initParams))
            };
        }
    }
}