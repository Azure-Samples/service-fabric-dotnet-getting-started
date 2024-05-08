// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Remoting;

namespace StatelessBackendService.Interfaces
{
    public interface IStatelessBackendService : IService
    {
        Task<long> GetCountAsync();
    }
}
