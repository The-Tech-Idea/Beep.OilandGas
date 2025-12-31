using Microsoft.AspNetCore.Mvc;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.GasLift;
using Beep.OilandGas.Models.DTOs;
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
        public ActionResult<GasLiftPotentialResult> AnalyzePotential([FromBody] AnalyzeGasLiftPotentialRequest request)
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
        public ActionResult<GasLiftValveDesignResult> DesignValves([FromBody] DesignValvesRequest request)
        {
            try
            {
                var result = _service.DesignValves(
                    request.WellProperties,
                    request.GasInjectionPressure,
                    request.NumberOfValves,
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
        public async Task<ActionResult> SaveDesign([FromBody] GasLiftDesignDto design, [FromQuery] string? userId = null)
        {
            try
            {
                await _service.SaveGasLiftDesignAsync(design, userId ?? GetUserId());
                return Ok(new { message = "Gas lift design saved successfully", designId = design.DesignId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving gas lift design");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("performance/{wellUWI}")]
        public async Task<ActionResult<GasLiftPerformanceDto>> GetPerformance(string wellUWI)
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

    public class AnalyzeGasLiftPotentialRequest
    {
        public GasLiftWellProperties WellProperties { get; set; } = null!;
        public decimal MinGasInjectionRate { get; set; }
        public decimal MaxGasInjectionRate { get; set; }
        public int NumberOfPoints { get; set; } = 50;
    }

    public class DesignValvesRequest
    {
        public GasLiftWellProperties WellProperties { get; set; } = null!;
        public decimal GasInjectionPressure { get; set; }
        public int NumberOfValves { get; set; }
        public bool UseSIUnits { get; set; } = false;
    }
}

