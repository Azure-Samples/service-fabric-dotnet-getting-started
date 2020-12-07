using System;
using System.Collections.Generic;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;

namespace TenantService
{
    internal sealed class TenantService : StatefulService
    {
        public TenantService(StatefulServiceContext context)
            : base(context)
        { }

        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return new ServiceReplicaListener[0];
        }

        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            try
            {
                while (true)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    ServiceEventSource.Current.ServiceMessage(this.Context, "Working at full steam {0}", Context.ServiceName);

                    await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                ServiceEventSource.Current.ServiceMessage(this.Context, "Shutting down {0}", Context.ServiceName);
                throw;
            }
        }
    }
}
