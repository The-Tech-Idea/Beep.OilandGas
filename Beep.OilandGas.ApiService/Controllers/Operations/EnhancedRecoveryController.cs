using Microsoft.AspNetCore.Mvc;
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
            try
            {
                var result = await _service.AnalyzeEORPotentialAsync(request.FieldId, request.EorMethod);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing EOR potential for field {FieldId}", request.FieldId);
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        [HttpPost("recovery-factor")]
        public async Task<ActionResult<EnhancedRecoveryOperation>> CalculateRecoveryFactor([FromBody] CalculateRecoveryFactorRequest request)
        {
            var operationId = request.OperationId;
            if (string.IsNullOrWhiteSpace(operationId))
                operationId = request.ProjectId;

            if (string.IsNullOrWhiteSpace(operationId))
                return BadRequest(new { error = "Operation ID is required." });

            try
            {
                var result = await _service.CalculateRecoveryFactorAsync(operationId);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Enhanced recovery operation {OperationId} was not found for recovery-factor calculation", operationId);
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating recovery factor for operation {OperationId}", operationId);
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        [HttpGet("injection")]
        public async Task<ActionResult<List<InjectionOperation>>> GetInjectionOperations([FromQuery] string? wellUWI = null)
        {
            try
            {
                var result = await _service.GetInjectionOperationsAsync(wellUWI);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting injection operations for well {InjectionWellId}", wellUWI);
                return StatusCode(500, new { error = "An internal error occurred." });
            }
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

            try
            {
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing EOR economics for field {FieldId}", request.FieldId);
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        [HttpPost("injection")]
        public async Task<ActionResult<InjectionOperation>> ManageInjection([FromBody] ManageInjectionRequest request)
        {
            try
            {
                var result = await _service.ManageInjectionAsync(request.InjectionWellId, request.InjectionRate);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Injection management request failed for well {InjectionWellId}", request.InjectionWellId);
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error managing injection for well {InjectionWellId}", request.InjectionWellId);
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        private string GetUserId() => User.FindFirst("sub")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "SYSTEM";
    }
}

