using Microsoft.AspNetCore.Mvc;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.FlashCalculations;
using Beep.OilandGas.Models.DTOs.Calculations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.ApiService.Controllers.Calculations
{
    /// <summary>
    /// API controller for flash calculation operations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FlashCalculationController : ControllerBase
    {
        private readonly IFlashCalculationService _service;
        private readonly ILogger<FlashCalculationController> _logger;

        public FlashCalculationController(IFlashCalculationService service, ILogger<FlashCalculationController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpPost("isothermal")]
        public ActionResult<FlashResult> PerformIsothermalFlash([FromBody] FlashConditions conditions)
        {
            try
            {
                var result = _service.PerformIsothermalFlash(conditions);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing isothermal flash calculation");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("multi-stage")]
        public ActionResult<List<FlashResult>> PerformMultiStageFlash([FromBody] MultiStageFlashRequest request)
        {
            try
            {
                var result = _service.PerformMultiStageFlash(request.Conditions, request.Stages);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing multi-stage flash calculation");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("result")]
        public async Task<ActionResult> SaveResult([FromBody] FlashResult result, [FromQuery] string? userId = null)
        {
            try
            {
                await _service.SaveFlashResultAsync(result, userId ?? GetUserId());
                return Ok(new { message = "Flash calculation result saved successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving flash calculation result");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("history")]
        public async Task<ActionResult<List<FlashResult>>> GetHistory([FromQuery] string? componentId = null)
        {
            try
            {
                var result = await _service.GetFlashHistoryAsync(componentId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting flash calculation history");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        private string GetUserId() => User.FindFirst("sub")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "SYSTEM";
    }
}

