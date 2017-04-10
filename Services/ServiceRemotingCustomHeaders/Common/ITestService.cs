// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Common
{
    using System.Threading.Tasks;
    using Microsoft.ServiceFabric.Services.Remoting;

    public interface ITestService : IService
    {
        Task<string> GetCurrentActivityId();
    }
}