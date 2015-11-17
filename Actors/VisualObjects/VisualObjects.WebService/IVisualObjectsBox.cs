// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace VisualObjects.WebService
{
    using System.Threading;
    using System.Threading.Tasks;

    public interface IVisualObjectsBox
    {
        Task<string> GetObjectsAsync(CancellationToken cancellationToken);
    }
}