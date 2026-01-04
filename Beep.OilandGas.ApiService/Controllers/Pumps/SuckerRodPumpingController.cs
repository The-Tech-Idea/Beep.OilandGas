using Microsoft.AspNetCore.Mvc;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.DTOs.Pumps;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.ApiService.Controllers.Pumps
{
    /// <summary>
    /// API controller for sucker rod pumping operations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SuckerRodPumpingController : ControllerBase
    {
        private readonly ISuckerRodPumpingService _service;
        private readonly ILogger<SuckerRodPumpingController> _logger;

        public SuckerRodPumpingController(ISuckerRodPumpingService service, ILogger<SuckerRodPumpingController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpPost("design")]
        public async Task<ActionResult<SuckerRodPumpDesignDto>> DesignPumpSystem([FromBody] DesignPumpSystemRequest request)
        {
            try
            {
                var result = await _service.DesignPumpSystemAsync(request.WellUWI, request.WellProperties);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error designing sucker rod pump system for well {WellUWI}", request.WellUWI);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("analyze-performance")]
        public async Task<ActionResult<SuckerRodPumpPerformanceDto>> AnalyzePerformance([FromBody] AnalyzePerformanceRequest request)
        {
            try
            {
                var result = await _service.AnalyzePerformanceAsync(request.PumpId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing sucker rod pump performance for pump {PumpId}", request.PumpId);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("design/save")]
        public async Task<ActionResult> SavePumpDesign([FromBody] SuckerRodPumpDesignDto design, [FromQuery] string? userId = null)
        {
            try
            {
                await _service.SavePumpDesignAsync(design, userId ?? GetUserId());
                return Ok(new { message = "Sucker rod pump design saved successfully", designId = design.DesignId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving sucker rod pump design");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        private string GetUserId() => User.FindFirst("sub")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "SYSTEM";
    }
}

