using Microsoft.AspNetCore.Mvc;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.ApiService.Controllers.Properties
{
    /// <summary>
    /// API controller for gas property calculations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class GasPropertiesController : ControllerBase
    {
        private readonly IGasPropertiesService _service;
        private readonly ILogger<GasPropertiesController> _logger;

        public GasPropertiesController(IGasPropertiesService service, ILogger<GasPropertiesController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpPost("calculate-zfactor")]
        public ActionResult<decimal> CalculateZFactor([FromBody] CalculateZFactorRequest request)
        {
            try
            {
                var result = _service.CalculateZFactor(
                    request.Pressure, request.Temperature, request.SpecificGravity, request.Correlation);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating Z-factor");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("calculate-density")]
        public ActionResult<decimal> CalculateDensity([FromBody] CalculateGasDensityRequest request)
        {
            try
            {
                var result = _service.CalculateGasDensity(
                    request.Pressure, request.Temperature, request.ZFactor, request.MolecularWeight);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating gas density");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("calculate-fvf")]
        public ActionResult<decimal> CalculateFormationVolumeFactor([FromBody] CalculateGasFVFRequest request)
        {
            try
            {
                var result = _service.CalculateFormationVolumeFactor(
                    request.Pressure, request.Temperature, request.ZFactor);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating formation volume factor");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("composition")]
        public async Task<ActionResult> SaveComposition([FromBody] GasComposition composition, [FromQuery] string? userId = null)
        {
            try
            {
                await _service.SaveGasCompositionAsync(composition, userId ?? GetUserId());
                return Ok(new { message = "Composition saved successfully", compositionId = composition.CompositionId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving gas composition");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("composition/{compositionId}")]
        public async Task<ActionResult<GasComposition>> GetComposition(string compositionId)
        {
            try
            {
                var result = await _service.GetGasCompositionAsync(compositionId);
                if (result == null)
                    return NotFound(new { error = $"Composition {compositionId} not found" });
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting gas composition");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        private string GetUserId() => User.FindFirst("sub")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "SYSTEM";
    }
}

