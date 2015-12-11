// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace ClusterMonitor
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Fabric.Description;
    using System.Linq;
    using System.Net.Http;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// This is a simple proxy to allow JavaScript to make cross-origin requests to the Service Fabric HTTP Management API,
    /// which listens on a different port than the web page making the requests, until CORS support is available.
    /// </summary>
    public class ProxyHandler : DelegatingHandler
    {
        private readonly string secureClusterCertThumbprint;

        public ProxyHandler(ConfigurationSettings configSettings)
        {
            KeyedCollection<string, ConfigurationProperty> parameters = configSettings.Sections["WebServiceConfig"].Parameters;

            if (parameters.Contains("SecureClusterCertThumbprint"))
            {
                this.secureClusterCertThumbprint = parameters["SecureClusterCertThumbprint"].Value;
            }
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            IEnumerable<string> values;
            if (request.Headers.TryGetValues("X-ClusterMonitor-Proxy", out values))
            {
                if (values.Any())
                {
                    UriBuilder ub = new UriBuilder(request.RequestUri);
                    ub.Path = ub.Path.Replace("/cluster", String.Empty);
                    ub.Port = Int32.Parse(values.First());

                    WebRequestHandler handler = new WebRequestHandler();

                    if (!String.IsNullOrEmpty(this.secureClusterCertThumbprint))
                    {
                        X509Certificate2 cert = GetCertificate(this.secureClusterCertThumbprint);

                        if (cert != null)
                        {
                            if (!(handler.ClientCertificates.Contains(cert)))
                            {
                                handler.ClientCertificates.Add(cert);
                                handler.ServerCertificateValidationCallback += (sender, cert2, chain, sslPolicyErrors) => true;

                                ub.Scheme = "https";
                            }
                        }
                    }

                    HttpClient client = new HttpClient(handler);

                    return client.GetAsync(ub.Uri, HttpCompletionOption.ResponseContentRead, cancellationToken);
                }
            }

            return base.SendAsync(request, cancellationToken);
        }

        private static X509Certificate2 GetCertificate(string thumbprint)
        {
            X509Store store = new X509Store("My", StoreLocation.LocalMachine);
            store.Open(OpenFlags.OpenExistingOnly);
            foreach (X509Certificate2 cert in store.Certificates)
            {
                if (String.Equals(cert.Thumbprint, thumbprint, StringComparison.OrdinalIgnoreCase))
                {
                    return cert;
                }
            }

            return null;
        }
    }
}