// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Calculator.Common
{
    using System.ServiceModel;
    using System.Threading.Tasks;

    // Define a service contract.
    [ServiceContract]
    public interface ICalculator
    {
        [OperationContract]
        Task<double> Add(double n1, double n2);
    }
}