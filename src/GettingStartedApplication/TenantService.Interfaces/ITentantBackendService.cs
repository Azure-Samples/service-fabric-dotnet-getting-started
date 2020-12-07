using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Remoting;

namespace TenantService.Interfaces
{
    public interface ITentantBackendService : IService
    {
        Task Deploy(string tenantName);

        Task TearDown(string tenantName);
    }
}
