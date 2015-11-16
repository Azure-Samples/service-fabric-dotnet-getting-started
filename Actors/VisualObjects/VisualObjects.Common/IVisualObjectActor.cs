// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace VisualObjects.Common
{
    using System.Threading.Tasks;
    using Microsoft.ServiceFabric.Actors;

    public interface IVisualObjectActor : IActor
    {
        [Readonly]
        Task<string> GetStateAsJsonAsync();
    }
}