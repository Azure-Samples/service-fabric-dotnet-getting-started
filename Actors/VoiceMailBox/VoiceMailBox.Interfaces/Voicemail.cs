// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.Service.Fabric.Samples.VoicemailBox.Interfaces
{
    using System;
    using System.Runtime.Serialization;

    [DataContract]
    public class Voicemail
    {
        [DataMember]
        public Guid Id { get; set; }

        [DataMember]
        public string Message { get; set; }

        [DataMember]
        public DateTime ReceivedAt { get; set; }
    }
}