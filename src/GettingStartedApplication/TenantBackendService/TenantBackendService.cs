using System;
using System.Collections.Generic;
using System.Fabric;
using System.Fabric.Description;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.FabricTransport.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using TenantService.Interfaces;

namespace TenantService
{
    internal sealed class TenantBackendService : StatefulService, ITentantBackendService
    {
        const string TenantServiceType = "TenantServiceType";
        const string TenantServiceTypeVersion = "1.0.0";

        public TenantBackendService(StatefulServiceContext context)
            : base(context)
        { }

        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return new[]
            {
                new ServiceReplicaListener(this.CreateServiceRemotingListener)
            };
        }

        protected override Task RunAsync(CancellationToken cancellationToken)
        {
            ServiceEventSource.Current.ServiceMessage(this.Context, "Fetching tenants");
            return Task.FromResult(0);
        }

        public async Task Deploy(string tenantName)
        {
            bool result;
            using (var tx = StateManager.CreateTransaction())
            {
                var tenants = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, string>>("tenants");
                result = await tenants.TryAddAsync(tx, tenantName, tenantName);
                await tx.CommitAsync();

            }

            if (result)
            {
                try
                {
                    var tenantServiceName = Context.CodePackageActivationContext.ApplicationName + "/TenantService_" + tenantName;
                    using (FabricClient fc = new FabricClient())
                    {
                        var svcDescription = new StatefulServiceDescription
                        {
                            ApplicationName = new Uri(Context.CodePackageActivationContext.ApplicationName),
                            ServiceName = new Uri(tenantServiceName),
                            HasPersistedState = true,
                            ServiceTypeName = TenantServiceType,
                            PartitionSchemeDescription = new UniformInt64RangePartitionSchemeDescription
                            {
                                HighKey = long.MaxValue,
                                LowKey = long.MinValue,
                                PartitionCount = 2
                            },
                            TargetReplicaSetSize = 3,
                            MinReplicaSetSize = 3,
                        };

                        await fc.ServiceManager.CreateServiceAsync(svcDescription);
                    }
                }
                catch (Exception e)
                {
                    ServiceEventSource.Current.ServiceMessage(this.Context, $"TenantService.Deploy({tenantName}) : {e}");
                }
            }
            else
            {
                ServiceEventSource.Current.ServiceMessage(this.Context, $"TenantService.Deploy({tenantName}) already registered.");
            }
        }

        public async Task TearDown(string tenantName)
        {
            ConditionalValue<string> result;
            using (var tx = StateManager.CreateTransaction())
            {
                var tenants = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, string>>("tenants");
                result = await tenants.TryRemoveAsync(tx, tenantName);
                await tx.CommitAsync();
            }

            if (result.HasValue)
            {
                try
                {
                    var tenantServiceName = Context.CodePackageActivationContext.ApplicationName + "/TenantService_" +
                                            tenantName;
                    using (FabricClient fc = new FabricClient())
                    {
                        var deleteDescription = new DeleteServiceDescription(new Uri(tenantServiceName));
                        await fc.ServiceManager.DeleteServiceAsync(deleteDescription);
                    }
                }
                catch (Exception e)
                {
                    ServiceEventSource.Current.ServiceMessage(this.Context, $"TenantService.TearDown() : {e}");
                }
            }
            else
            {
                ServiceEventSource.Current.ServiceMessage(this.Context, $"TenantService.Deploy({tenantName}) already removed.");
            }
        }
    }
}
