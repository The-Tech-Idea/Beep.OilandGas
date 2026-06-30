using System.Threading.Tasks;
using Beep.OilandGas.ApiService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Beep.OilandGas.ApiService.Controllers
{
    [ApiController][Route("api/ppdm/modules")][Authorize(Roles="Admin,Administrator")]
    public class PpdmModuleSeedingController:ControllerBase
    {
        private readonly PpdmModuleSeedingService _svc;
        public PpdmModuleSeedingController(PpdmModuleSeedingService svc)=>_svc=svc;

        [HttpPost("seed-all")]
        public async Task<IActionResult> SeedAll([FromQuery]string userId="SYSTEM")
        {
            var report=await _svc.SeedAllAsync(userId);
            return Ok(report);
        }

        [HttpGet("status")]
        public IActionResult GetStatus()
        {
            var modules=_svc.Modules;
            return Ok(new{count=modules.Count,modules=modules.Select(m=>new{m.ModuleId,m.ModuleName,m.Order,m.EntityTypes.Count})});
        }
    }
}
