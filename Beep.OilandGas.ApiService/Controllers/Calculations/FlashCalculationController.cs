using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.FlashCalculations;
using Beep.OilandGas.Models.Data.Calculations;
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
        private readonly Beep.OilandGas.FlashCalculations.Services.FlashCalculationService _service;
        private readonly ILogger<FlashCalculationController> _logger;

        public FlashCalculationController(Beep.OilandGas.FlashCalculations.Services.FlashCalculationService service, ILogger<FlashCalculationController> logger)
        {
            _service = service;
            _logger = logger;
        }

        /// <summary>PT flash returning <see cref="FlashCalculationResult"/> (Wilson K + Rachford–Rice).</summary>
        [HttpPost("rigorous")]
        public async Task<ActionResult<FlashCalculationResult>> RunRigorousFlashAsync(
            [FromBody] FlashCalculationRequest request,
            CancellationToken cancellationToken)
        {
            if (request == null)
                return BadRequest(new { error = "Request body is required." });
            var result = await _service.RunRigorousFlashAsync(request, cancellationToken);
            return Ok(result);
        }

        [HttpPost("isothermal")]
        public ActionResult<FlashResult> PerformIsothermalFlash([FromBody] FLASH_CONDITIONS conditions)
        {
            var result = _service.PerformIsothermalFlash(conditions);
            return Ok(result);
        }

        [HttpPost("multi-stage")]
        public ActionResult<List<FlashResult>> PerformMultiStageFlash([FromBody] MultiStageFlashRequest request)
        {
            if (request.Conditions == null)
                    return BadRequest(new { error = "Conditions are required." });
            var result = _service.PerformMultiStageFlash(request.Conditions, request.Stages);
            return Ok(result);
        }

        [HttpPost("result")]
        public async Task<ActionResult> SaveResult([FromBody] FlashResult result)
        {
            var userId = User.ActingUserId();
            await _service.SaveFlashResultAsync(result, userId);
            return Ok(new { message = "Flash calculation result saved successfully" });
        }

        [HttpGet("history")]
        public async Task<ActionResult<List<FlashResult>>> GetHistory([FromQuery] string? componentId = null)
        {
            var result = await _service.GetFlashHistoryAsync(componentId);
            return Ok(result);
        }
    }
}
