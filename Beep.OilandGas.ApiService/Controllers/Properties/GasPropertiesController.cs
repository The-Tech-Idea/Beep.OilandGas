using Microsoft.AspNetCore.Mvc;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.GasProperties;
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
            var result = _service.CalculateZFactor(
                request.Pressure, request.Temperature, request.SpecificGravity, request.Correlation);
            return Ok(result);
        }

        [HttpPost("calculate-density")]
        public ActionResult<decimal> CalculateDensity([FromBody] CalculateGasDensityRequest request)
        {
            var result = _service.CalculateGasDensity(
                request.Pressure, request.Temperature, request.ZFactor, request.MolecularWeight);
            return Ok(result);
        }

        [HttpPost("calculate-fvf")]
        public ActionResult<decimal> CalculateFormationVolumeFactor([FromBody] CalculateGasFVFRequest request)
        {
            var result = _service.CalculateFormationVolumeFactor(
                request.Pressure, request.Temperature, request.ZFactor);
            return Ok(result);
        }

        [HttpPost("composition")]
        public async Task<ActionResult> SaveComposition([FromBody] GasComposition composition)
        {
            var userId = User.ActingUserId();
            await _service.SaveGasCompositionAsync(composition, userId);
            return Ok(new { message = "Composition saved successfully", compositionId = composition.CompositionId });
        }

        [HttpGet("composition/{compositionId}")]
        public async Task<ActionResult<GasComposition>> GetComposition(string compositionId)
        {
            if (string.IsNullOrWhiteSpace(compositionId))
                return BadRequest(new { error = "Composition ID is required." });
            var result = await _service.GetGasCompositionAsync(compositionId);
            if (result == null)
                    return NotFound(new { error = $"Composition {compositionId} not found." });
            return Ok(result);
        }
    }
}

