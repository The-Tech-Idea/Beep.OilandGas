using Microsoft.AspNetCore.Mvc;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.EconomicAnalysis;
using Beep.OilandGas.Models.Data.Calculations;
using Beep.OilandGas.ApiService.Controllers.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.ApiService.Controllers.Calculations
{
    /// <summary>
    /// API controller for economic analysis operations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class EconomicAnalysisController : ControllerBase
    {
        private readonly IEconomicAnalysisService _service;
        private readonly ILogger<EconomicAnalysisController> _logger;

        public EconomicAnalysisController(IEconomicAnalysisService service, ILogger<EconomicAnalysisController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpPost("npv")]
        public ActionResult<double> CalculateNPV([FromBody] CalculateNPVRequest request)
        {
            if (request == null)
                return BadRequest(new { error = "Request payload is required." });
            try
            {
                var result = _service.CalculateNPV(EconomicAnalysisControllerHelpers.ToCashFlows(request.CashFlows), request.DISCOUNT_RATE);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid request for NPV calculation");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating NPV");
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        [HttpPost("irr")]
        public ActionResult<double> CalculateIRR([FromBody] CalculateIRRRequest request)
        {
            if (request == null)
                return BadRequest(new { error = "Request payload is required." });
            try
            {
                var result = _service.CalculateIRR(EconomicAnalysisControllerHelpers.ToCashFlows(request.CashFlows), request.InitialGuess);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid request for IRR calculation");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating IRR");
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        [HttpPost("analyze")]
        public ActionResult<EconomicResult> Analyze([FromBody] AnalyzeRequest request)
        {
            if (request == null)
                return BadRequest(new { error = "Request payload is required." });
            try
            {
                var result = _service.Analyze(
                    EconomicAnalysisControllerHelpers.ToCashFlows(request.CashFlows),
                    request.DISCOUNT_RATE,
                    request.FinanceRate,
                    request.ReinvestRate);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid request for economic analysis");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing economic analysis");
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        [HttpPost("npv-profile")]
        public ActionResult<List<NPV_PROFILE_POINT>> GenerateNPVProfile([FromBody] GenerateNPVProfileRequest request)
        {
            if (request == null)
                return BadRequest(new { error = "Request payload is required." });
            try
            {
                var result = _service.GenerateNPVProfile(
                    EconomicAnalysisControllerHelpers.ToCashFlows(request.CashFlows),
                    request.MinRate,
                    request.MaxRate,
                    request.Points);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid request for NPV profile generation");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating NPV profile");
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        [HttpPost("result")]
        public async Task<ActionResult> SaveResult([FromBody] SaveAnalysisResultRequest request, [FromQuery] string? userId = null)
        {
            if (!EconomicAnalysisControllerHelpers.TryValidateSaveRequest(request, out var validationError))
                return BadRequest(new { error = validationError });
            try
            {
                await _service.SaveAnalysisResultAsync(request.AnalysisId, request.Result, userId ?? GetUserId());
                return Ok(new { message = "Economic analysis result saved successfully", analysisId = request.AnalysisId });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid request when saving economic analysis result");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving economic analysis result");
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        [HttpGet("result/{analysisId}")]
        public async Task<ActionResult<EconomicResult>> GetResult(string analysisId)
        {
            if (string.IsNullOrWhiteSpace(analysisId)) return BadRequest(new { error = "Analysis ID is required." });
            try
            {
                var result = await _service.GetAnalysisResultAsync(analysisId);
                if (result == null)
                        return NotFound(new { error = $"Analysis {analysisId} not found." });
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid request when retrieving economic analysis result");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting economic analysis result {AnalysisId}", analysisId);
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        private string GetUserId() => User.FindFirst("sub")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "SYSTEM";

    }
}

