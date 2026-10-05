using Microsoft.AspNetCore.Mvc;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.Calculations;
using Beep.OilandGas.Models.Data.NodalAnalysis;
using Beep.OilandGas.Models.Data.ProductionForecasting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.ApiService.Controllers.Calculations
{
    /// <summary>
    /// API controller for nodal analysis operations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NodalAnalysisController : ControllerBase
    {
        private readonly Beep.OilandGas.NodalAnalysis.Core.Interfaces.INodalAnalysisService _service;
        private readonly ILogger<NodalAnalysisController> _logger;

        public NodalAnalysisController(Beep.OilandGas.NodalAnalysis.Core.Interfaces.INodalAnalysisService service, ILogger<NodalAnalysisController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpPost("analyze")]
        public async Task<ActionResult<NodalAnalysisRunResult>> PerformAnalysis([FromBody] PerformNodalAnalysisRequest? request)
        {
            if (request is null)
                return BadRequest(new { error = "Request body is required." });
            if (string.IsNullOrWhiteSpace(request.WellUWI))
                return BadRequest(new { error = "Well UWI is required." });
            if (request.AnalysisParameters == null)
                return BadRequest(new { error = "Analysis parameters are required." });
            var result = await _service.PerformNodalAnalysisAsync(request.WellUWI, request.AnalysisParameters);
            return Ok(result);
        }

        [HttpPost("optimize")]
        public async Task<ActionResult<OptimizationResult>> Optimize([FromBody] OptimizeSystemRequest? request)
        {
            if (request is null)
                return BadRequest(new { error = "Request body is required." });
            if (string.IsNullOrWhiteSpace(request.WellUWI))
                return BadRequest(new { error = "Well UWI is required." });
            if (request.OptimizationGoals is null)
                return BadRequest(new { error = "Optimization goals are required." });
            var result = await _service.OptimizeSystemAsync(request.WellUWI, request.OptimizationGoals);
            return Ok(result);
        }

        [HttpPost("result")]
        public async Task<ActionResult> SaveResult([FromBody] NodalAnalysisRunResult? result)
        {
            var userId = User.ActingUserId();
            if (result is null)
                return BadRequest(new { error = "Result body is required." });
            if (string.IsNullOrWhiteSpace(result.WellUWI))
                return BadRequest(new { error = "Well UWI is required." });
            await _service.SaveAnalysisResultAsync(result, userId);
            return Ok(new { message = "Nodal analysis result saved successfully", analysisId = result.AnalysisId });
        }

        [HttpGet("history/{wellUWI}")]
        public async Task<ActionResult<List<NodalAnalysisRunResult>>> GetHistory(string wellUWI)
        {
            if (string.IsNullOrWhiteSpace(wellUWI)) return BadRequest(new { error = "Well UWI is required." });
            var result = await _service.GetAnalysisHistoryAsync(wellUWI);
            return Ok(result);
        }

        [HttpPost("performance-matching")]
        public async Task<ActionResult<PerformanceMatchingAnalysis>> PerformanceMatching([FromBody] PerformNodalAnalysisRequest? request)
        {
            if (request is null)
                return BadRequest(new { error = "Request body is required." });
            if (string.IsNullOrWhiteSpace(request.WellUWI))
                return BadRequest(new { error = "Well UWI is required." });
            if (request.AnalysisParameters == null)
                return BadRequest(new { error = "Analysis parameters are required." });
            var result = await _service.AnalyzePerformanceMatchingAsync(request.WellUWI, request.AnalysisParameters);
            return Ok(result);
        }

        [HttpPost("sensitivity")]
        public async Task<ActionResult<EconomicSensitivityAnalysisResult>> Sensitivity([FromBody] NodalSensitivityAnalysisRequest? request)
        {
            if (request is null)
                return BadRequest(new { error = "Request body is required." });
            if (string.IsNullOrWhiteSpace(request.WellUWI))
                return BadRequest(new { error = "Well UWI is required." });
            if (request.BaselineParameters == null)
                return BadRequest(new { error = "Baseline analysis parameters are required." });
            var result = await _service.PerformSensitivityAnalysisAsync(
                request.WellUWI,
                request.BaselineParameters,
                request.ParametersToVary ?? new System.Collections.Generic.List<string>());
            return Ok(result);
        }

        [HttpPost("artificial-lift")]
        public async Task<ActionResult<ArtificialLiftRecommendation>> ArtificialLift([FromBody] NodalArtificialLiftRequest? request)
        {
            if (request is null)
                return BadRequest(new { error = "Request body is required." });
            if (string.IsNullOrWhiteSpace(request.WellUWI))
                return BadRequest(new { error = "Well UWI is required." });
            var result = await _service.RecommendArtificialLiftAsync(
                request.WellUWI,
                request.CurrentProduction,
                request.TargetProduction,
                request.WellDepth,
                request.WaterCut);
            return Ok(result);
        }

        [HttpPost("diagnostics")]
        public async Task<ActionResult<WellDiagnosticsResult>> Diagnostics([FromBody] NodalWellDiagnosticsRequest? request)
        {
            if (request is null)
                return BadRequest(new { error = "Request body is required." });
            if (string.IsNullOrWhiteSpace(request.WellUWI))
                return BadRequest(new { error = "Well UWI is required." });
            var result = await _service.DiagnoseWellPerformanceAsync(
                request.WellUWI,
                request.ExpectedProduction,
                request.ActualProduction,
                request.WellheadPressure,
                request.BottomholePressure);
            return Ok(result);
        }

        [HttpPost("production-forecast")]
        public async Task<ActionResult<PRODUCTION_FORECAST>> ProductionForecast([FromBody] NodalProductionForecastRequest? request)
        {
            if (request is null)
                return BadRequest(new { error = "Request body is required." });
            if (string.IsNullOrWhiteSpace(request.WellUWI))
                return BadRequest(new { error = "Well UWI is required." });
            if (request.ForecastMonths <= 0)
                return BadRequest(new { error = "Forecast months must be positive." });
            if (request.DeclineRate < 0m || request.DeclineRate > 1m)
                return BadRequest(new { error = "Decline rate must be between 0 and 1 (annual fraction)." });
            var result = await _service.ForecastProductionAsync(
                request.WellUWI,
                request.CurrentProduction,
                request.DeclineRate,
                request.ForecastMonths);
            return Ok(result);
        }

        [HttpPost("pressure-maintenance")]
        public async Task<ActionResult<PressureMaintenanceStrategy>> PressureMaintenance([FromBody] NodalPressureMaintenanceRequest? request)
        {
            if (request is null)
                return BadRequest(new { error = "Request body is required." });
            if (string.IsNullOrWhiteSpace(request.WellUWI))
                return BadRequest(new { error = "Well UWI is required." });
            if (request.ProductivityIndex < 0m)
                return BadRequest(new { error = "Productivity index cannot be negative." });
            var result = await _service.AnalyzePressureMaintenanceAsync(
                request.WellUWI,
                request.CurrentReservoirPressure,
                request.BubblePointPressure,
                request.ProductivityIndex);
            return Ok(result);
        }
    }
}
