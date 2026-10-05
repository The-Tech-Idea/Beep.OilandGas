using Microsoft.AspNetCore.Mvc;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.Calculations;
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
        public async Task<ActionResult<PipelineAnalysisResult>> AnalyzeFlow([FromBody] AnalyzePipelineFlowRequest request)
        {
            var result = await _service.AnalyzePipelineFlowAsync(
                request.PipelineId,
                request.FlowRate,
                request.InletPressure);
            return Ok(result);
        }

        [HttpPost("pressure-drop")]
        public async Task<ActionResult<PressureDropResult>> CalculatePressureDrop([FromBody] CalculatePressureDropRequest request)
        {
            var result = await _service.CalculatePressureDropAsync(
                request.PipelineId,
                request.FlowRate);
            return Ok(result);
        }

        [HttpPost("result")]
        public async Task<ActionResult> SaveResult([FromBody] PipelineAnalysisResult result)
        {
            var userId = User.ActingUserId();
            await _service.SaveAnalysisResultAsync(result, userId);
            return Ok(new { message = "Pipeline analysis result saved successfully", analysisId = result.AnalysisId });
        }
    }
}

