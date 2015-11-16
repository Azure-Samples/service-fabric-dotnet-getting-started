// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace PingService
{
    using System;

    /// <summary>
    /// A load test target.
    /// This is a data model for the information in the Data Package.
    /// </summary>
    public class RequestTarget
    {
        public Uri Url { get; set; }

        public string Method { get; set; }

        public string Payload { get; set; }
    }
}