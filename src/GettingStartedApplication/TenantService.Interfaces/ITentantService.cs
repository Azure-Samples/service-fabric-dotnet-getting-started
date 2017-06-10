using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Remoting;

namespace TenantService.Interfaces
{
    public interface ITentantService : IService
    {
        Task Deploy(string tenantName);

        Task TearDown(string tenantName);
    }
}
