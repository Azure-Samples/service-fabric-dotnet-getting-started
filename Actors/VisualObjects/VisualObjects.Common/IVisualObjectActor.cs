// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace VisualObjects.Common
{
    using Microsoft.ServiceFabric.Actors;
    using System.Threading.Tasks;

    public interface IVisualObjectActor : IActor
    {
        Task<string> GetStateAsJsonAsync();
    }
}