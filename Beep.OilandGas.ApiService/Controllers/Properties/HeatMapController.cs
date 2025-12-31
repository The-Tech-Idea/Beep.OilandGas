using Microsoft.AspNetCore.Mvc;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.HeatMap;
using Beep.OilandGas.HeatMap.Configuration;
using Beep.OilandGas.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.ApiService.Controllers.Properties
{
    /// <summary>
    /// API controller for heat map operations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class HeatMapController : ControllerBase
    {
        private readonly IHeatMapService _service;
        private readonly ILogger<HeatMapController> _logger;

        public HeatMapController(IHeatMapService service, ILogger<HeatMapController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpPost("generate")]
        public async Task<ActionResult<HeatMapResultDto>> GenerateHeatMap([FromBody] GenerateHeatMapRequest request)
        {
            try
            {
                var result = await _service.GenerateHeatMapAsync(request.DataPoints, request.Configuration);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating heat map");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("configuration")]
        public async Task<ActionResult<string>> SaveConfiguration([FromBody] HeatMapConfigurationDto configuration, [FromQuery] string? userId = null)
        {
            try
            {
                var heatMapId = await _service.SaveHeatMapConfigurationAsync(configuration, userId ?? GetUserId());
                return Ok(new { message = "Heat map configuration saved successfully", heatMapId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving heat map configuration");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("configuration/{heatMapId}")]
        public async Task<ActionResult<HeatMapConfigurationDto>> GetConfiguration(string heatMapId)
        {
            try
            {
                var result = await _service.GetHeatMapConfigurationAsync(heatMapId);
                if (result == null)
                    return NotFound(new { error = $"Heat map configuration {heatMapId} not found" });
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting heat map configuration {HeatMapId}", heatMapId);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("production")]
        public async Task<ActionResult<HeatMapResultDto>> GenerateProductionHeatMap([FromBody] GenerateProductionHeatMapRequest request)
        {
            try
            {
                var result = await _service.GenerateProductionHeatMapAsync(request.FieldId, request.StartDate, request.EndDate);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating production heat map for field {FieldId}", request.FieldId);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        private string GetUserId() => User.FindFirst("sub")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "SYSTEM";
    }

    public class GenerateHeatMapRequest
    {
        public List<HeatMapDataPoint> DataPoints { get; set; } = new();
        public HeatMapConfiguration Configuration { get; set; } = null!;
    }

    public class GenerateProductionHeatMapRequest
    {
        public string FieldId { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}

