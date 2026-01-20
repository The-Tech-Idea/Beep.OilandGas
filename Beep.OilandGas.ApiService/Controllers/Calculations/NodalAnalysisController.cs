using Microsoft.AspNetCore.Mvc;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.Calculations;
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
        private readonly INodalAnalysisService _service;
        private readonly ILogger<NodalAnalysisController> _logger;

        public NodalAnalysisController(INodalAnalysisService service, ILogger<NodalAnalysisController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpPost("analyze")]
        public async Task<ActionResult<NodalAnalysisRunResult>> PerformAnalysis([FromBody] PerformNodalAnalysisRequest request)
        {
            try
            {
                var result = await _service.PerformNodalAnalysisAsync(request.WellUWI, request.AnalysisParameters);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing nodal analysis for well {WellUWI}", request.WellUWI);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("optimize")]
        public async Task<ActionResult<OptimizationResult>> Optimize([FromBody] OptimizeSystemRequest request)
        {
            try
            {
                var result = await _service.OptimizeSystemAsync(request.WellUWI, request.OptimizationGoals);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error optimizing system for well {WellUWI}", request.WellUWI);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("result")]
        public async Task<ActionResult> SaveResult([FromBody] NodalAnalysisRunResult result, [FromQuery] string? userId = null)
        {
            try
            {
                await _service.SaveAnalysisResultAsync(result, userId ?? GetUserId());
                return Ok(new { message = "Nodal analysis result saved successfully", analysisId = result.AnalysisId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving nodal analysis result");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("history/{wellUWI}")]
        public async Task<ActionResult<List<NodalAnalysisRunResult>>> GetHistory(string wellUWI)
        {
            try
            {
                var result = await _service.GetAnalysisHistoryAsync(wellUWI);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting nodal analysis history for well {WellUWI}", wellUWI);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        private string GetUserId() => User.FindFirst("sub")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "SYSTEM";
    }
}

