// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace WordCount.WebService
{
    using Microsoft.ServiceFabric.Services;
    using Microsoft.ServiceFabric.Services.Communication.Runtime;
    using Microsoft.ServiceFabric.Services.Runtime;
    using System.Collections.Generic;
    using System.Fabric;
    using WordCount.Common;

    /// <summary>
    /// Service that handles front-end web requests and acts as a proxy to the back-end data for the UI web page.
    /// It is a stateless service that hosts a Web API application on OWIN.
    /// </summary>
    public class WordCountWebService : StatelessService
    {
        public WordCountWebService(StatelessServiceContext context)
            : base(context)
        { }

        /// <summary>
        /// Creates a listener for Web API with websockets.
        /// </summary>
        /// <returns>The OWIN communication listener.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new[]
            {
                new ServiceInstanceListener(initParams => new OwinCommunicationListener("wordcount", new Startup(), initParams))
            };
        }
    }
}