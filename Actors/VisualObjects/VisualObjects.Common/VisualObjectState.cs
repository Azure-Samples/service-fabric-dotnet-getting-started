// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace VisualObjects.Common
{
    using System.Runtime.Serialization;

    [DataContract]
    internal class VisualObjectActorState
    {
        [DataMember] public VisualObject ObjectState = null;

        [DataMember] public string JsonState = null;
    }
}