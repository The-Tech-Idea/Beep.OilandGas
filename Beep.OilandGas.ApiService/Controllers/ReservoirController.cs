using System.Threading.Tasks;
using Beep.OilandGas.ApiService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Beep.OilandGas.ApiService.Controllers
{
    [ApiController] [Route("api/reservoir/aggregation")] [Authorize]
    public class ReservoirAggregationController : ControllerBase
    {
        private readonly ReservoirAggregationService _svc;
        public ReservoirAggregationController(ReservoirAggregationService svc) => _svc = svc;
        [HttpGet("pools")] public async Task<IActionResult> GetPools([FromQuery] string? fieldId = null) => Ok(await _svc.GetPoolSummaryAsync(fieldId));
    }
}
