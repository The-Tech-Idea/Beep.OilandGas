using Microsoft.AspNetCore.Mvc;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.HeatMap;
using Beep.OilandGas.HeatMap.Configuration;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.HeatMap;
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
        public async Task<ActionResult<HeatMapResult>> GenerateHeatMap([FromBody] GenerateHeatMapRequest request)
        {
            if (request.Configuration == null)
                    return BadRequest(new { error = "Configuration is required." });
            var result = await _service.GenerateHeatMapAsync(request.DataPoints, request.Configuration);
            return Ok(result);
        }

        [HttpPost("configuration")]
        public async Task<ActionResult<string>> SaveConfiguration([FromBody] HeatMapConfigurationRecord configuration)
        {
            var userId = User.ActingUserId();
            var heatMapId = await _service.SaveHeatMapConfigurationAsync(configuration, userId);
            return Ok(new { message = "Heat map configuration saved successfully", heatMapId });
        }

        [HttpGet("configuration/{heatMapId}")]
        public async Task<ActionResult<HeatMapConfigurationRecord>> GetConfiguration(string heatMapId)
        {
            if (string.IsNullOrWhiteSpace(heatMapId)) return BadRequest(new { error = "Heat map ID is required." });
            var result = await _service.GetHeatMapConfigurationAsync(heatMapId);
            if (result == null)
                    return NotFound(new { error = $"Heat map configuration {heatMapId} not found." });
            return Ok(result);
        }

        [HttpPost("production")]
        public async Task<ActionResult<HeatMapResult>> GenerateProductionHeatMap([FromBody] GenerateProductionHeatMapRequest request)
        {
            var startDate = request.StartDate ?? DateTime.UtcNow.AddDays(-30);
            var endDate = request.EndDate ?? DateTime.UtcNow;
            var result = await _service.GenerateProductionHeatMapAsync(request.FieldId, startDate, endDate);
            return Ok(result);
        }
    }
}

