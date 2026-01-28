using Microsoft.AspNetCore.Mvc;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.GasLift;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.Calculations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.ApiService.Controllers.Calculations
{
    /// <summary>
    /// API controller for gas lift operations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class GasLiftController : ControllerBase
    {
        private readonly IGasLiftService _service;
        private readonly ILogger<GasLiftController> _logger;

        public GasLiftController(IGasLiftService service, ILogger<GasLiftController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpPost("analyze-potential")]
        public ActionResult<GAS_LIFT_POTENTIAL_RESULT> AnalyzePotential([FromBody] AnalyzeGasLiftPotentialRequest request)
        {
            try
            {
                var result = _service.AnalyzeGasLiftPotential(
                    request.WellProperties,
                    request.MinGasInjectionRate,
                    request.MaxGasInjectionRate,
                    request.NumberOfPoints);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing gas lift potential");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("design-valves")]
        public ActionResult<GAS_LIFT_VALVE_DESIGN_RESULT> DesignValves([FromBody] DesignValvesRequest request)
        {
            try
            {
                var result = _service.DesignValves(
                    request.WellProperties,
                    request.GAS_INJECTION_PRESSURE,
                    request.NUMBER_OF_VALVES,
                    request.UseSIUnits);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error designing gas lift valves");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("design")]
        public async Task<ActionResult> SaveDesign([FromBody] GAS_LIFT_DESIGN design, [FromQuery] string? userId = null)
        {
            try
            {
                await _service.SaveGasLiftDesignAsync(design, userId ?? GetUserId());
                return Ok(new { message = "Gas lift design saved successfully", designId = design.DESIGN_ID });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving gas lift design");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("performance/{wellUWI}")]
        public async Task<ActionResult<GAS_LIFT_PERFORMANCE>> GetPerformance(string wellUWI)
        {
            try
            {
                var result = await _service.GetGasLiftPerformanceAsync(wellUWI);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting gas lift performance for well {WellUWI}", wellUWI);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        private string GetUserId() => User.FindFirst("sub")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "SYSTEM";
    }
}

