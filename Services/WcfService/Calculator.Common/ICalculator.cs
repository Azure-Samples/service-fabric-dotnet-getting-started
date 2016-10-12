// ----------------------------------------------------------------------
//  <copyright file="ICalculator.cs" company="Microsoft">
//       Copyright (c) Microsoft Corporation. All rights reserved.
//  </copyright>
// ----------------------------------------------------------------------

namespace Calculator.Common
{
    using System.ServiceModel;

    // Define a service contract.
    [ServiceContract]
    public interface ICalculator
    {
        [OperationContract]
        double Add(double n1, double n2);
    }
}