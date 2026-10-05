using Microsoft.AspNetCore.Mvc;
using Beep.OilandGas.ApiService.Services;
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
            var result = _service.CalculateNPV(EconomicAnalysisControllerHelpers.ToCashFlows(request.CashFlows), request.DISCOUNT_RATE);
            return Ok(result);
        }

        [HttpPost("irr")]
        public ActionResult<double> CalculateIRR([FromBody] CalculateIRRRequest request)
        {
            if (request == null)
                return BadRequest(new { error = "Request payload is required." });
            var result = _service.CalculateIRR(EconomicAnalysisControllerHelpers.ToCashFlows(request.CashFlows), request.InitialGuess);
            return Ok(result);
        }

        [HttpPost("analyze")]
        public ActionResult<EconomicResult> Analyze([FromBody] AnalyzeRequest request)
        {
            if (request == null)
                return BadRequest(new { error = "Request payload is required." });
            var result = _service.Analyze(
                EconomicAnalysisControllerHelpers.ToCashFlows(request.CashFlows),
                request.DISCOUNT_RATE,
                request.FinanceRate,
                request.ReinvestRate);
            return Ok(result);
        }

        [HttpPost("npv-profile")]
        public ActionResult<List<NPV_PROFILE_POINT>> GenerateNPVProfile([FromBody] GenerateNPVProfileRequest request)
        {
            if (request == null)
                return BadRequest(new { error = "Request payload is required." });
            var result = _service.GenerateNPVProfile(
                EconomicAnalysisControllerHelpers.ToCashFlows(request.CashFlows),
                request.MinRate,
                request.MaxRate,
                request.Points);
            return Ok(result);
        }

        [HttpPost("result")]
        public async Task<ActionResult> SaveResult([FromBody] SaveAnalysisResultRequest request)
        {
            var userId = User.ActingUserId();
            if (!EconomicAnalysisControllerHelpers.TryValidateSaveRequest(request, out var validationError))
                return BadRequest(new { error = validationError });
            await _service.SaveAnalysisResultAsync(request.AnalysisId, request.Result, userId);
            return Ok(new { message = "Economic analysis result saved successfully", analysisId = request.AnalysisId });
        }

        [HttpGet("result/{analysisId}")]
        public async Task<ActionResult<EconomicResult>> GetResult(string analysisId)
        {
            if (string.IsNullOrWhiteSpace(analysisId)) return BadRequest(new { error = "Analysis ID is required." });
            var result = await _service.GetAnalysisResultAsync(analysisId);
            if (result == null)
                    return NotFound(new { error = $"Analysis {analysisId} not found." });
            return Ok(result);
        }
    }
}

