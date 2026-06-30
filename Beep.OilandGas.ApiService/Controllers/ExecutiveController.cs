using System.Threading.Tasks;
using Beep.OilandGas.ApiService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.ApiService.Controllers
{
    [ApiController]
    [Route("api/executive")]
    [Authorize]
    public class ExecutiveController : ControllerBase
    {
        private readonly ExecutiveAggregationService _service;
        private readonly ILogger<ExecutiveController> _logger;

        public ExecutiveController(ExecutiveAggregationService service, ILogger<ExecutiveController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet("kpi")]
        public async Task<IActionResult> GetKpi()
        {
            var result = await _service.GetExecutiveKpiAsync();
            return Ok(result);
        }

        [HttpGet("assets")]
        public async Task<IActionResult> GetAssets()
        {
            var result = await _service.GetAssetPerformanceAsync();
            return Ok(result);
        }
    }
}
