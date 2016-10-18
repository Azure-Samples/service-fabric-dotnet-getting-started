// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Calculator.Service
{
    using System.Collections.Generic;
    using System.Fabric;
    using System.ServiceModel;
    using Calculator.Common;
    using Microsoft.ServiceFabric.Services.Communication.Runtime;
    using Microsoft.ServiceFabric.Services.Communication.Wcf.Runtime;
    using Microsoft.ServiceFabric.Services.Runtime;

    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class Service : StatelessService
    {
        public Service(StatelessServiceContext context)
            : base(context)
        {
        }

        //Using SOAP Http Binding. It reads the port from EndpointReseourceName specified in ServiceManifest file.

        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            // create an return WCF based communication listener that hosts WCF Calculator Service.
            return new[]
            {
                new ServiceInstanceListener(
                    c => new WcfCommunicationListener<ICalculator>(
                        c,
                        new CalculatorService(c),
                        new BasicHttpBinding(),
                        endpointResourceName: "ServiceEndpoint"))
            };
        }
    }
}