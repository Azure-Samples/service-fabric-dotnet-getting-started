// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

using System.Fabric;
using System.Fabric.Description;

namespace WebService
{
    public class ConfigSettings
    {
        public ConfigSettings(StatelessServiceContext context)
        {
            context.CodePackageActivationContext.ConfigurationPackageModifiedEvent += CodePackageActivationContext_ConfigurationPackageModifiedEvent;
            UpdateConfigSettings(context.CodePackageActivationContext.GetConfigurationPackageObject("Config").Settings);
        }

        public string GuestExeBackendServiceName { get; private set; }

        public string StatefulBackendServiceName { get; private set; }

        public string StatelessBackendServiceName { get; private set; }

        public string ActorBackendServiceName { get; private set; }

        public int ReverseProxyPort { get; private set; }


        private void CodePackageActivationContext_ConfigurationPackageModifiedEvent(object sender, PackageModifiedEventArgs<ConfigurationPackage> e)
        {
            UpdateConfigSettings(e.NewPackage.Settings);
        }

        private void UpdateConfigSettings(ConfigurationSettings settings)
        {
            ConfigurationSection section = settings.Sections["MyConfigSection"];
            GuestExeBackendServiceName = section.Parameters["GuestExeBackendServiceName"].Value;
            StatefulBackendServiceName = section.Parameters["StatefulBackendServiceName"].Value;
            StatelessBackendServiceName = section.Parameters["StatelessBackendServiceName"].Value;
            ActorBackendServiceName = section.Parameters["ActorBackendServiceName"].Value;
            ReverseProxyPort = int.Parse(section.Parameters["ReverseProxyPort"].Value);
        }
    }
}