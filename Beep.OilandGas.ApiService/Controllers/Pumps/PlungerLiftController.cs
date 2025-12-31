using Microsoft.AspNetCore.Mvc;
using Beep.OilandGas.Models.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.ApiService.Controllers.Pumps
{
    /// <summary>
    /// API controller for plunger lift operations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PlungerLiftController : ControllerBase
    {
        private readonly IPlungerLiftService _service;
        private readonly ILogger<PlungerLiftController> _logger;

        public PlungerLiftController(IPlungerLiftService service, ILogger<PlungerLiftController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpPost("design")]
        public async Task<ActionResult<PlungerLiftDesignDto>> DesignPlungerLiftSystem([FromBody] DesignPlungerLiftSystemRequest request)
        {
            try
            {
                var result = await _service.DesignPlungerLiftSystemAsync(request.WellUWI, request.WellProperties);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error designing plunger lift system for well {WellUWI}", request.WellUWI);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("analyze-performance")]
        public async Task<ActionResult<PlungerLiftPerformanceDto>> AnalyzePerformance([FromBody] AnalyzePerformanceRequest request)
        {
            try
            {
                var result = await _service.AnalyzePerformanceAsync(request.WellUWI);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing plunger lift performance for well {WellUWI}", request.WellUWI);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("design/save")]
        public async Task<ActionResult> SavePlungerLiftDesign([FromBody] PlungerLiftDesignDto design, [FromQuery] string? userId = null)
        {
            try
            {
                await _service.SavePlungerLiftDesignAsync(design, userId ?? GetUserId());
                return Ok(new { message = "Plunger lift design saved successfully", designId = design.DesignId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving plunger lift design");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        private string GetUserId() => User.FindFirst("sub")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "SYSTEM";
    }

    public class DesignPlungerLiftSystemRequest
    {
        public string WellUWI { get; set; } = string.Empty;
        public PlungerLiftWellPropertiesDto WellProperties { get; set; } = null!;
    }

    public class AnalyzePerformanceRequest
    {
        public string WellUWI { get; set; } = string.Empty;
    }
}

