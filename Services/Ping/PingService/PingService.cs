// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace PingService
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Fabric;
    using System.Fabric.Description;
    using System.IO;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.ServiceFabric.Services;
    using Newtonsoft.Json;
    using Microsoft.ServiceFabric.Services.Runtime;

    /// <summary>
    /// Ping is a stateless service that takes a set of web site address, pings them with an HTTP request, and displays the results.
    /// The web site addresses and HTTP request settings are configured through the Data and Config Packages.
    /// This is the entry point for our service logic which is a stateless service, indicated by implementing IStatelessServiceInstance.
    /// </summary>
    public class PingService : StatelessService
    {
        /// <summary>
        /// The set of targets to ping. 
        /// This gets set up from the Data Package.
        /// </summary>
        private IEnumerable<RequestTarget> targets;

        /// <summary>
        /// Global HTTP request settings.
        /// This gets configured through the custom config file (RequestSettings.json) in the Configuration Package.
        /// </summary>
        private RequestSettings requestSettings;

        /// <summary>
        /// The ping frequency.
        /// This gets configured through the built-in settings file (Settings.xml).
        /// </summary>
        private TimeSpan frequency;

        /// <summary>
        /// Runs code that is intended to be run for the life of the service instance.
        /// </summary>
        /// <remarks>
        /// The RunAsync is considered the Main method for your service.
        /// If you override the RunAsync method, your code should block indefinitely.
        /// If the RunAsync method returns, the service instance continues to stay up.
        /// </remarks>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            ServiceEventSource.Current.ServiceMessage(this, "RunAsync");

            // First time we have to get the config and data packages from the initialization parameters.
            ConfigurationPackage configPackage = this.ServiceInitializationParameters.CodePackageActivationContext.GetConfigurationPackageObject("Config");
            DataPackage dataPackage = this.ServiceInitializationParameters.CodePackageActivationContext.GetDataPackageObject("Data");

            this.UpdateApplicationSettings(configPackage.Settings);
            this.UpdateRequestSettings(configPackage.Path);
            this.UpdateRequestData(dataPackage.Path);

            // After that, we can set up events that are triggered whenever the Config Package or Data Package is modified.
            // This allows us to ingest the new config and data without stopping the service.
            this.ServiceInitializationParameters.CodePackageActivationContext.ConfigurationPackageModifiedEvent +=
                this.CodePackageActivationContext_ConfigurationPackageModifiedEvent;
            this.ServiceInitializationParameters.CodePackageActivationContext.DataPackageModifiedEvent +=
                this.CodePackageActivationContext_DataPackageModifiedEvent;

            ServiceEventSource.Current.ServiceMessage(this, "Starting pinging.");

            // This loop generates HTTP requests as defined in our Data Package,
            // using the configuration settings we defined in our Config Package.
            while (true)
            {
                try
                {
                    foreach (RequestTarget target in this.targets)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        try
                        {
                            // Create a request based on the request data in our Data Package 
                            // and configure it using the settings from our Configuration Package

                            HttpWebRequest request = WebRequest.CreateHttp(target.Url);

                            request.Timeout = (int) this.requestSettings.Timeout.TotalMilliseconds;
                            request.KeepAlive = this.requestSettings.KeepAlive;
                            request.Method = target.Method;

                            ServiceEventSource.Current.ServiceMessage(this, "Sending {0} request to {1}", request.Method, target.Url);

                            if (request.Method.ToUpperInvariant() == "POST")
                            {
                                using (Stream requestStream = request.GetRequestStream())
                                using (StreamWriter writer = new StreamWriter(requestStream))
                                {
                                    writer.Write(target.Payload ?? String.Empty);
                                }
                            }

                            using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                            using (Stream responseStream = response.GetResponseStream())
                            using (StreamReader reader = new StreamReader(responseStream))
                            {
                                reader.ReadToEnd();

                                ServiceEventSource.Current.ServiceMessage(this, response.StatusDescription);
                            }
                        }
                        catch (Exception ex)
                        {
                            ServiceEventSource.Current.ServiceMessage(this, ex.ToString());
                        }
                    }

                    await Task.Delay(this.frequency, cancellationToken);
                }
                catch (TaskCanceledException)
                {
                    ServiceEventSource.Current.ServiceMessage(this, "Stopping pinging.");

                    return;
                }
            }
        }

        /// <summary>
        /// Updates the application settings with the given ConfigurationSettings object.
        /// </summary>
        /// <param name="applicationSettings"></param>
        private void UpdateApplicationSettings(ConfigurationSettings applicationSettings)
        {
            // For demonstration purposes, this configuration is using the built-in Settings.xml configuration option.
            // We could just as easily put these settings in our custom JSON configuration (RequestSettings.json), or vice-versa.

            try
            {
                KeyedCollection<string, ConfigurationProperty> parameters = applicationSettings.Sections["PingServiceConfiguration"].Parameters;

                this.frequency = TimeSpan.Parse(parameters["FrequencyTimespan"].Value);
            }
            catch (Exception e)
            {
                ServiceEventSource.Current.ServiceMessage(this, e.ToString());
            }
        }

        /// <summary>
        /// Updates the request settings with the config file at the given path.
        /// </summary>
        /// <param name="requestSettingsPath"></param>
        private void UpdateRequestSettings(string requestSettingsPath)
        {
            // We're using JSON to store the request settings.
            // As shown here, the config package is just a directory with some files that we put in.
            // The files can be anything whatever. In this sample, we chose to use a JSON file.
            // This config package is separate from the data package so that we can update them individually.

            try
            {
                using (StreamReader reader = new StreamReader(Path.Combine(requestSettingsPath, "RequestSettings.json")))
                {
                    this.requestSettings = JsonConvert.DeserializeObject<RequestSettings>(reader.ReadToEnd());
                }
            }
            catch (Exception e)
            {
                ServiceEventSource.Current.ServiceMessage(this, e.ToString());
            }
        }

        /// <summary>
        /// Updates the request data with the data file at the given path.
        /// </summary>
        /// <param name="requestDataPath"></param>
        private void UpdateRequestData(string requestDataPath)
        {
            // We're using JSON to store the request data.
            // As shown here, the data package is just a directory with some files that we put in.
            // The files can be anything whatever. In this sample, we chose to use a JSON file.
            // This data package is separate from the config package so that we can update them individually.

            try
            {
                using (StreamReader reader = new StreamReader(Path.Combine(requestDataPath, "RequestData.json")))
                {
                    this.targets = JsonConvert.DeserializeObject<IEnumerable<RequestTarget>>(reader.ReadToEnd());
                }
            }
            catch (Exception e)
            {
                ServiceEventSource.Current.ServiceMessage(this, e.ToString());
            }
        }

        /// <summary>
        /// This event handler is called whenever a new data package is available.
        /// We can do whatever we want with the event. 
        /// In this sample, we reconfigure the service without stopping anything.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CodePackageActivationContext_DataPackageModifiedEvent(object sender, PackageModifiedEventArgs<DataPackage> e)
        {
            // The Path property contains a path to our data file that we included with the data package.
            this.UpdateRequestData(e.NewPackage.Path);
        }

        /// <summary>
        /// This event handler is called whenever a new config package is available.
        /// We can do whatever we want with the event. 
        /// In this sample, we reconfigure the service without stopping anything.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CodePackageActivationContext_ConfigurationPackageModifiedEvent(object sender, PackageModifiedEventArgs<ConfigurationPackage> e)
        {
            // The Settings property contains the built-in Settings.xml config.
            this.UpdateApplicationSettings(e.NewPackage.Settings);

            // The Path property contains a path to our custom config file that we included with the config package.
            this.UpdateRequestSettings(e.NewPackage.Path);
        }
    }
}