using Microsoft.AspNetCore.Mvc;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.Calculations;
using Beep.OilandGas.Models.Data.Operations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.ApiService.Controllers.Operations
{
    /// <summary>
    /// API controller for enhanced recovery operations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class EnhancedRecoveryController : ControllerBase
    {
        private readonly IEnhancedRecoveryService _service;
        private readonly ILogger<EnhancedRecoveryController> _logger;

        public EnhancedRecoveryController(IEnhancedRecoveryService service, ILogger<EnhancedRecoveryController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpPost("analyze-eor")]
        public async Task<ActionResult<EnhancedRecoveryOperation>> AnalyzeEOR([FromBody] AnalyzeEORRequest request)
        {
            var userId = User.ActingUserId();
            var result = await _service.AnalyzeEORPotentialAsync(request.FieldId, request.EorMethod, userId);
            return Ok(result);
        }

        [HttpPost("recovery-factor")]
        public async Task<ActionResult<EnhancedRecoveryOperation>> CalculateRecoveryFactor([FromBody] CalculateRecoveryFactorRequest request)
        {
            var operationId = request.OperationId;
            if (string.IsNullOrWhiteSpace(operationId))
                operationId = request.ProjectId;

            if (string.IsNullOrWhiteSpace(operationId))
                return BadRequest(new { error = "Operation ID is required." });

            var result = await _service.CalculateRecoveryFactorAsync(operationId);
            return Ok(result);
        }

        [HttpGet("injection")]
        public async Task<ActionResult<List<InjectionOperation>>> GetInjectionOperations([FromQuery] string? wellUWI = null)
        {
            var result = await _service.GetInjectionOperationsAsync(wellUWI);
            return Ok(result);
        }

        [HttpPost("economics")]
        public async Task<ActionResult<EOREconomicAnalysis>> AnalyzeEconomics([FromBody] AnalyzeEOREconomicsRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.FieldId))
                return BadRequest(new { error = "Field ID is required." });
            if (request.EstimatedIncrementalOil <= 0)
                return BadRequest(new { error = "Estimated incremental oil must be greater than zero." });
            if (request.CapitalCostMm <= 0)
                return BadRequest(new { error = "Pilot CAPEX must be greater than zero." });
            if (request.ProjectLifeYears <= 0)
                return BadRequest(new { error = "Project life must be greater than zero." });

            var result = await _service.AnalyzeEOReconomicsAsync(
                request.FieldId,
                request.EstimatedIncrementalOil,
                request.OilPrice,
                request.CapitalCostMm * 1_000_000d,
                request.OperatingCostPerBarrel,
                request.ProjectLifeYears,
                request.DiscountRatePct / 100d);
            return Ok(result);
        }

        [HttpPost("injection")]
        public async Task<ActionResult<InjectionOperation>> ManageInjection([FromBody] ManageInjectionRequest request)
        {
            var userId = User.ActingUserId();
            var result = await _service.ManageInjectionAsync(request.InjectionWellId, request.InjectionRate, userId);
            return Ok(result);
        }
    }
}

