using Microsoft.AspNetCore.Mvc;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.ApiService.Controllers.Pumps
{
    /// <summary>
    /// API controller for hydraulic pump operations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class HydraulicPumpController : ControllerBase
    {
        private readonly IHydraulicPumpService _service;
        private readonly ILogger<HydraulicPumpController> _logger;

        public HydraulicPumpController(IHydraulicPumpService service, ILogger<HydraulicPumpController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpPost("design")]
        public async Task<ActionResult<HydraulicPumpDesignDto>> DesignPumpSystem([FromBody] DesignPumpSystemRequest request)
        {
            try
            {
                var result = await _service.DesignPumpSystemAsync(
                    request.WellUWI,
                    request.PumpType,
                    request.WellDepth,
                    request.DesiredFlowRate);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error designing hydraulic pump system for well {WellUWI}", request.WellUWI);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("analyze-performance")]
        public async Task<ActionResult<PumpPerformanceAnalysisDto>> AnalyzePerformance([FromBody] AnalyzePerformanceRequest request)
        {
            try
            {
                var result = await _service.AnalyzePumpPerformanceAsync(request.PumpId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing pump performance for pump {PumpId}", request.PumpId);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("design/save")]
        public async Task<ActionResult> SavePumpDesign([FromBody] HydraulicPumpDesignDto design, [FromQuery] string? userId = null)
        {
            try
            {
                await _service.SavePumpDesignAsync(design, userId ?? GetUserId());
                return Ok(new { message = "Hydraulic pump design saved successfully", designId = design.DesignId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving hydraulic pump design");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("performance-history/{pumpId}")]
        public async Task<ActionResult<List<PumpPerformanceHistoryDto>>> GetPerformanceHistory(string pumpId)
        {
            try
            {
                var result = await _service.GetPumpPerformanceHistoryAsync(pumpId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting performance history for pump {PumpId}", pumpId);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        private string GetUserId() => User.FindFirst("sub")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "SYSTEM";
    }

    public class DesignPumpSystemRequest
    {
        public string WellUWI { get; set; } = string.Empty;
        public string PumpType { get; set; } = string.Empty;
        public decimal WellDepth { get; set; }
        public decimal DesiredFlowRate { get; set; }
    }

    public class AnalyzePerformanceRequest
    {
        public string PumpId { get; set; } = string.Empty;
    }
}

