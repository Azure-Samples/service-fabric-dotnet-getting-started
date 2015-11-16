// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace WordCount.WebService
{
    using System;
    using System.Fabric;
    using Microsoft.ServiceFabric.Services;
    using Microsoft.ServiceFabric.Services.Communication.Client;
    /// <summary>
    /// Communication client that wraps the logic for talking to the service.
    /// Created by communication client factory.
    /// </summary>
    public class CommunicationClient : ICommunicationClient
    {
        public CommunicationClient(Uri baseAddress, TimeSpan operationTimeout, TimeSpan readWriteTimeout)
        {
            this.BaseAddress = baseAddress;
            this.OperationTimeout = operationTimeout;
            this.ReadWriteTimeout = readWriteTimeout;
        }

        /// <summary>
        /// The service base address.
        /// </summary>
        public Uri BaseAddress { get; private set; }

        /// <summary>
        /// Represents the value for operation timeout. Used for HttpWebRequest GetResponse and GetRequestStream methods.
        /// </summary>
        public TimeSpan OperationTimeout { get; set; }

        /// <summary>
        /// Represents the value for the timeout used to read/write from a stream.
        /// </summary>
        public TimeSpan ReadWriteTimeout { get; set; }

        /// <summary>
        /// The resolved service partition which contains the resolved service endpoints.
        /// </summary>
        public ResolvedServicePartition ResolvedServicePartition { get; set; }
    }
}