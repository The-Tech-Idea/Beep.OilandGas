using System;
using System.Threading.Tasks;
using Beep.OilandGas.ApiService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.ApiService.Controllers
{
    [ApiController]
    [Route("api/accounting/aggregation")]
    [Authorize]
    public class AccountingAggregationController : ControllerBase
    {
        private readonly AccountingAggregationService _service;
        private readonly ILogger<AccountingAggregationController> _logger;

        public AccountingAggregationController(
            AccountingAggregationService service,
            ILogger<AccountingAggregationController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet("revenue")]
        public async Task<IActionResult> GetRevenue(
            [FromQuery] string? fieldId = null,
            [FromQuery] DateTime? start = null,
            [FromQuery] DateTime? end = null)
        {
            var result = await _service.GetRevenueSummaryAsync(fieldId, start, end);
            return Ok(result);
        }

        [HttpGet("costs")]
        public async Task<IActionResult> GetCosts(
            [FromQuery] string? fieldId = null,
            [FromQuery] DateTime? start = null,
            [FromQuery] DateTime? end = null)
        {
            var result = await _service.GetCostSummaryAsync(fieldId, start, end);
            return Ok(result);
        }

        [HttpGet("royalties")]
        public async Task<IActionResult> GetRoyalties(
            [FromQuery] string? fieldId = null,
            [FromQuery] DateTime? start = null,
            [FromQuery] DateTime? end = null)
        {
            var result = await _service.GetRoyaltySummaryAsync(fieldId, start, end);
            return Ok(result);
        }

        [HttpGet("afe")]
        public async Task<IActionResult> GetAFEs([FromQuery] string? fieldId = null)
        {
            var result = await _service.GetAFESummaryAsync(fieldId);
            return Ok(result);
        }

        [HttpGet("period-close")]
        public async Task<IActionResult> GetPeriodClose([FromQuery] string? fieldId = null)
        {
            var result = await _service.GetPeriodCloseStatusAsync(fieldId);
            return Ok(result);
        }
    }
}
