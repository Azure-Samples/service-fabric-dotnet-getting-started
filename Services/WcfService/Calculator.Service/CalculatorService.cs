// ----------------------------------------------------------------------
//  <copyright file="CalculatorService.cs" company="Microsoft">
//       Copyright (c) Microsoft Corporation. All rights reserved.
//  </copyright>
// ----------------------------------------------------------------------

namespace Calculator.Service
{
    using System.Fabric;
    using System.ServiceModel;
    using System.Threading.Tasks;
    using Calculator.Common;

    // Service class which implements the service contract.
    [ServiceBehavior(AddressFilterMode = AddressFilterMode.Any)]
    public class CalculatorService : ICalculator
    {
        private readonly ServiceContext context;

        public CalculatorService(ServiceContext context)
        {
            this.context = context;
        }

        public Task<double> Add(double n1, double n2)
        {
            var result = n1 + n2;
            ServiceEventSource.Current.ServiceMessage(this.context, "Received Add({0},{1})", n1, n2);
            ServiceEventSource.Current.ServiceMessage(this.context, "Return: {0}", result);
            return Task.FromResult(result);
        }
    }
}