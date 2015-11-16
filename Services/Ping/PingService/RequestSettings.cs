// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace PingService
{
    using System;

    /// <summary>
    /// Global request settings for the load test.
    /// This is a data model for the configuration settings in our Config Package.
    /// </summary>
    public class RequestSettings
    {
        public TimeSpan Timeout { get; set; }

        public bool KeepAlive { get; set; }
    }
}