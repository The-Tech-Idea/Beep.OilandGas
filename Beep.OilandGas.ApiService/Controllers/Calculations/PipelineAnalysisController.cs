using Microsoft.AspNetCore.Mvc;
using Beep.OilandGas.Models.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.ApiService.Controllers.Calculations
{
    /// <summary>
    /// API controller for pipeline analysis operations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PipelineAnalysisController : ControllerBase
    {
        private readonly IPipelineAnalysisService _service;
        private readonly ILogger<PipelineAnalysisController> _logger;

        public PipelineAnalysisController(IPipelineAnalysisService service, ILogger<PipelineAnalysisController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpPost("analyze-flow")]
        public async Task<ActionResult<PipelineAnalysisResultDto>> AnalyzeFlow([FromBody] AnalyzePipelineFlowRequest request)
        {
            try
            {
                var result = await _service.AnalyzePipelineFlowAsync(
                    request.PipelineId,
                    request.FlowRate,
                    request.InletPressure);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing pipeline flow for pipeline {PipelineId}", request.PipelineId);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("pressure-drop")]
        public async Task<ActionResult<PressureDropResultDto>> CalculatePressureDrop([FromBody] CalculatePressureDropRequest request)
        {
            try
            {
                var result = await _service.CalculatePressureDropAsync(
                    request.PipelineId,
                    request.FlowRate);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating pressure drop for pipeline {PipelineId}", request.PipelineId);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("result")]
        public async Task<ActionResult> SaveResult([FromBody] PipelineAnalysisResultDto result, [FromQuery] string? userId = null)
        {
            try
            {
                await _service.SaveAnalysisResultAsync(result, userId ?? GetUserId());
                return Ok(new { message = "Pipeline analysis result saved successfully", analysisId = result.AnalysisId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving pipeline analysis result");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        private string GetUserId() => User.FindFirst("sub")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "SYSTEM";
    }

    public class AnalyzePipelineFlowRequest
    {
        public string PipelineId { get; set; } = string.Empty;
        public decimal FlowRate { get; set; }
        public decimal InletPressure { get; set; }
    }

    public class CalculatePressureDropRequest
    {
        public string PipelineId { get; set; } = string.Empty;
        public decimal FlowRate { get; set; }
    }
}

