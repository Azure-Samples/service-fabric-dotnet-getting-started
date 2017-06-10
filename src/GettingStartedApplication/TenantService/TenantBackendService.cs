using System;
using System.Collections.Generic;
using System.Fabric;
using System.Fabric.Description;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using TenantService.Interfaces;

namespace TenantService
{
    internal sealed class TenantBackendService : StatefulService, ITentantBackendService
    {
        private IReliableDictionary<string, string> tenants;

        public TenantBackendService(StatefulServiceContext context)
            : base(context)
        { }

        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return new ServiceReplicaListener[]
            {
                new ServiceReplicaListener(this.CreateServiceRemotingListener)
            };
        }

        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            tenants = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, string>>("tenants");
        }

        public async Task Deploy(string tenantName)
        {
            bool result;
            using (var tx = StateManager.CreateTransaction())
            {
                result = await tenants.TryAddAsync(tx, tenantName, tenantName);
                await tx.CommitAsync();

            }

            if (result)
            {
                var tenantServiceName = Context.ServiceName + "_" + tenantName;
                using (FabricClient fc = new FabricClient())
                {
                    var svcDescription = new StatefulServiceDescription
                    {
                        ApplicationName = new Uri(Context.CodePackageActivationContext.ApplicationName),
                        ServiceName = new Uri(tenantServiceName),
                        HasPersistedState = true,
                        ServiceTypeName = Context.CodePackageActivationContext.ApplicationTypeName,
                        TargetReplicaSetSize = 3,
                        MinReplicaSetSize = 1,
                        PartitionSchemeDescription = new NamedPartitionSchemeDescription
                        {
                            PartitionNames = {"a", "b" }
                        }
                    };

                    await fc.ServiceManager.CreateServiceAsync(svcDescription);
                }
            }
            else
            {
                throw new InvalidOperationException("Tenant already registered.");
            }
        }

        public Task TearDown(string tenantName)
        {
            throw new NotImplementedException();
        }
    }
}
