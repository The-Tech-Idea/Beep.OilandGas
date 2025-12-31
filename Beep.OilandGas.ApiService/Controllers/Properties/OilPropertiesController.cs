using Microsoft.AspNetCore.Mvc;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.ApiService.Controllers.Properties
{
    /// <summary>
    /// API controller for oil property calculations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OilPropertiesController : ControllerBase
    {
        private readonly IOilPropertiesService _service;
        private readonly ILogger<OilPropertiesController> _logger;

        public OilPropertiesController(IOilPropertiesService service, ILogger<OilPropertiesController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpPost("calculate-fvf")]
        public ActionResult<decimal> CalculateFormationVolumeFactor([FromBody] CalculateFVFRequest request)
        {
            try
            {
                var result = _service.CalculateFormationVolumeFactor(
                    request.Pressure, request.Temperature, request.GasOilRatio, request.OilGravity, request.Correlation);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating formation volume factor");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("calculate-density")]
        public ActionResult<decimal> CalculateDensity([FromBody] CalculateDensityRequest request)
        {
            try
            {
                var result = _service.CalculateOilDensity(
                    request.Pressure, request.Temperature, request.OilGravity, request.GasOilRatio);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating oil density");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("calculate-viscosity")]
        public ActionResult<decimal> CalculateViscosity([FromBody] CalculateViscosityRequest request)
        {
            try
            {
                var result = _service.CalculateOilViscosity(
                    request.Pressure, request.Temperature, request.OilGravity, request.GasOilRatio);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating oil viscosity");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("calculate-properties")]
        public async Task<ActionResult<OilPropertyResultDto>> CalculateProperties([FromBody] CalculateOilPropertiesRequest request)
        {
            try
            {
                var result = await _service.CalculateOilPropertiesAsync(request.Composition, request.Pressure, request.Temperature);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating oil properties");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("composition")]
        public async Task<ActionResult> SaveComposition([FromBody] OilCompositionDto composition, [FromQuery] string userId)
        {
            try
            {
                await _service.SaveOilCompositionAsync(composition, userId ?? GetUserId());
                return Ok(new { message = "Composition saved successfully", compositionId = composition.CompositionId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving oil composition");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("composition/{compositionId}")]
        public async Task<ActionResult<OilCompositionDto>> GetComposition(string compositionId)
        {
            try
            {
                var result = await _service.GetOilCompositionAsync(compositionId);
                if (result == null)
                    return NotFound(new { error = $"Composition {compositionId} not found" });
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting oil composition");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("composition/{compositionId}/history")]
        public async Task<ActionResult<List<OilPropertyResultDto>>> GetPropertyHistory(string compositionId)
        {
            try
            {
                var result = await _service.GetOilPropertyHistoryAsync(compositionId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting oil property history");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("result")]
        public async Task<ActionResult> SaveResult([FromBody] OilPropertyResultDto result, [FromQuery] string userId)
        {
            try
            {
                await _service.SaveOilPropertyResultAsync(result, userId ?? GetUserId());
                return Ok(new { message = "Result saved successfully", calculationId = result.CalculationId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving oil property result");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        private string GetUserId() => User.FindFirst("sub")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "SYSTEM";
    }

    public class CalculateFVFRequest
    {
        public decimal Pressure { get; set; }
        public decimal Temperature { get; set; }
        public decimal GasOilRatio { get; set; }
        public decimal OilGravity { get; set; }
        public string Correlation { get; set; } = "Standing";
    }

    public class CalculateDensityRequest
    {
        public decimal Pressure { get; set; }
        public decimal Temperature { get; set; }
        public decimal OilGravity { get; set; }
        public decimal GasOilRatio { get; set; }
    }

    public class CalculateViscosityRequest
    {
        public decimal Pressure { get; set; }
        public decimal Temperature { get; set; }
        public decimal OilGravity { get; set; }
        public decimal GasOilRatio { get; set; }
    }

    public class CalculateOilPropertiesRequest
    {
        public OilCompositionDto Composition { get; set; } = null!;
        public decimal Pressure { get; set; }
        public decimal Temperature { get; set; }
    }
}

