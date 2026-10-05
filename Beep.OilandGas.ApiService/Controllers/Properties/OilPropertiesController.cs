using Microsoft.AspNetCore.Mvc;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data;
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
            decimal gasSg = request.GasSpecificGravity is > 0m and <= 2.5m
                ? request.GasSpecificGravity
                : 0.65m;
            var result = _service.CalculateFormationVolumeFactor(
                request.Pressure,
                request.Temperature,
                request.GasOilRatio,
                request.OilGravity,
                request.Correlation,
                gasSg);
            return Ok(result);
        }

        [HttpPost("calculate-density")]
        public ActionResult<decimal> CalculateDensity([FromBody] CalculateDensityRequest request)
        {
            var result = _service.CalculateOilDensity(
                request.Pressure, request.Temperature, request.OilGravity, request.GasOilRatio);
            return Ok(result);
        }

        [HttpPost("calculate-viscosity")]
        public ActionResult<decimal> CalculateViscosity([FromBody] CalculateViscosityRequest request)
        {
            var result = _service.CalculateOilViscosity(
                request.Pressure, request.Temperature, request.OilGravity, request.GasOilRatio);
            return Ok(result);
        }

        [HttpPost("calculate-properties")]
        public async Task<ActionResult<OilPropertyResult>> CalculateProperties([FromBody] CalculateOilPropertiesRequest request)
        {
            var result = await _service.CalculateOilPropertiesAsync(request.Composition, request.Pressure, request.Temperature);
            return Ok(result);
        }

        [HttpPost("composition")]
        public async Task<ActionResult> SaveComposition([FromBody] OilComposition composition)
        {
            var userId = User.ActingUserId();
            await _service.SaveOilCompositionAsync(composition, userId);
            return Ok(new { message = "Composition saved successfully", compositionId = composition.CompositionId });
        }

        [HttpGet("composition/{compositionId}")]
        public async Task<ActionResult<OilComposition>> GetComposition(string compositionId)
        {
            if (string.IsNullOrWhiteSpace(compositionId))
                return BadRequest(new { error = "Composition ID is required." });
            var result = await _service.GetOilCompositionAsync(compositionId);
            if (result == null)
                    return NotFound(new { error = $"Composition {compositionId} not found." });
            return Ok(result);
        }

        [HttpGet("composition/{compositionId}/history")]
        public async Task<ActionResult<List<OilPropertyResult>>> GetPropertyHistory(string compositionId)
        {
            if (string.IsNullOrWhiteSpace(compositionId))
                return BadRequest(new { error = "Composition ID is required." });
            var result = await _service.GetOilPropertyHistoryAsync(compositionId);
            return Ok(result);
        }

        [HttpPost("result")]
        public async Task<ActionResult> SaveResult([FromBody] OilPropertyResult result)
        {
            var userId = User.ActingUserId();
            await _service.SaveOilPropertyResultAsync(result, userId);
            return Ok(new { message = "Result saved successfully", calculationId = result.CalculationId });
        }
    }
}

