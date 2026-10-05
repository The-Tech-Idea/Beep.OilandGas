using Microsoft.AspNetCore.Mvc;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.Pumps;
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
        public async Task<ActionResult<HydraulicPumpDesign>> DesignPumpSystem([FromBody] DesignPumpSystemRequest request)
        {
            var result = await _service.DesignPumpSystemAsync(
                request.WellUWI,
                request.PumpType,
                request.WellDepth,
                request.DesiredFlowRate);
            return Ok(result);
        }

        [HttpPost("analyze-performance")]
        public async Task<ActionResult<PumpPerformanceAnalysis>> AnalyzePerformance([FromBody] AnalyzePerformanceRequest request)
        {
            var result = await _service.AnalyzePumpPerformanceAsync(request.PumpId);
            return Ok(result);
        }

        [HttpPost("design/save")]
        public async Task<ActionResult> SavePumpDesign([FromBody] HydraulicPumpDesign design)
        {
            var userId = User.ActingUserId();
            await _service.SavePumpDesignAsync(design, userId);
            return Ok(new { message = "Hydraulic pump design saved successfully", designId = design.DesignId });
        }

        [HttpGet("performance-history/{pumpId}")]
        public async Task<ActionResult<List<PumpPerformanceHistory>>> GetPerformanceHistory(string pumpId)
        {
            if (string.IsNullOrWhiteSpace(pumpId)) return BadRequest(new { error = "Pump ID is required." });
            var result = await _service.GetPumpPerformanceHistoryAsync(pumpId);
            return Ok(result);
        }
    }
}

