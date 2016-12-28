using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace WebService.Controllers
{
    [Route("api/[controller]")]
    public class StatelessBackendServiceController : Controller
    {
        // GET: api/values
        [HttpGet]
        public async Task<IActionResult> GetAsync()
        {
            var result = new
            {
                Count = 12345
            };

            return Json(result);
        }

    }
}
