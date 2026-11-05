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
        public IActionResult GetKpi()
        {
            return StatusCode(501,new { error="Production/finance/HSE executive KPIs require approved unit, period, source and consolidation evidence. No measured totals are available from this endpoint." });
        }

        [HttpGet("portfolio")]
        public async Task<ActionResult<TheTechIdea.Data.OilGas.ExecutivePortfolioReview>> GetPortfolio()
        {Response.Headers.CacheControl="no-store";return Ok(await _service.GetPortfolioAsync(User));}

        [HttpGet("assets")]
        public async Task<IActionResult> GetAssets()
        {
            Response.Headers.CacheControl="no-store";
            var result = await _service.GetPortfolioAsync(User);
            return Ok(result.Assets);
        }
    }
}
