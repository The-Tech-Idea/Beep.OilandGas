using System;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.GasLift.Exceptions;
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
        public async Task<ActionResult<GAS_LIFT_POTENTIAL_RESULT>> AnalyzePotential(
            [FromBody] AnalyzeGasLiftPotentialRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                if (request == null)
                    return BadRequest(new { error = "Request body is required." });
                if (request.WellProperties == null)
                    return BadRequest(new { error = "WellProperties is required." });
                if (request.MinGasInjectionRate > request.MaxGasInjectionRate)
                    return BadRequest(new { error = "MinGasInjectionRate must not exceed MaxGasInjectionRate." });

                var result = await _service.AnalyzeGasLiftPotentialAsync(
                    request.WellProperties,
                    request.MinGasInjectionRate,
                    request.MaxGasInjectionRate,
                    request.NumberOfPoints,
                    cancellationToken);
                return Ok(result);
            }
            catch (OperationCanceledException)
            {
                return StatusCode(408, new { error = "Request was cancelled." });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid gas lift potential request");
                return BadRequest(new { error = ex.Message });
            }
            catch (GasLiftException ex)
            {
                _logger.LogWarning(ex, "Gas lift potential request rejected");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing gas lift potential");
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        [HttpPost("design-valves")]
        public async Task<ActionResult<GAS_LIFT_VALVE_DESIGN_RESULT>> DesignValves(
            [FromBody] DesignValvesRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                if (request == null)
                    return BadRequest(new { error = "Request body is required." });
                if (request.WellProperties == null)
                    return BadRequest(new { error = "WellProperties is required." });

                var result = await _service.DesignValvesAsync(
                    request.WellProperties,
                    request.GAS_INJECTION_PRESSURE,
                    request.NUMBER_OF_VALVES,
                    request.UseSIUnits,
                    cancellationToken);
                return Ok(result);
            }
            catch (OperationCanceledException)
            {
                return StatusCode(408, new { error = "Request was cancelled." });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid gas lift valve design request");
                return BadRequest(new { error = ex.Message });
            }
            catch (GasLiftException ex)
            {
                _logger.LogWarning(ex, "Gas lift valve design rejected");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error designing gas lift valves");
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        [HttpPost("design")]
        public async Task<ActionResult> SaveDesign(
            [FromBody] GAS_LIFT_DESIGN design,
            [FromQuery] string? userId = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (design == null)
                    return BadRequest(new { error = "Design payload is required." });
                cancellationToken.ThrowIfCancellationRequested();
                await _service.SaveGasLiftDesignAsync(design, userId ?? GetUserId());
                return Ok(new { message = "Gas lift design saved successfully", designId = design.DESIGN_ID });
            }
            catch (OperationCanceledException)
            {
                return StatusCode(408, new { error = "Request was cancelled." });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid gas lift design save request");
                return BadRequest(new { error = ex.Message });
            }
            catch (GasLiftException ex)
            {
                _logger.LogWarning(ex, "Gas lift design save rejected");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving gas lift design");
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        [HttpGet("performance/{wellUWI}")]
        public async Task<ActionResult<GAS_LIFT_PERFORMANCE>> GetPerformance(
            string wellUWI,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(wellUWI)) return BadRequest(new { error = "Well UWI is required." });
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                var result = await _service.GetGasLiftPerformanceAsync(wellUWI);
                return Ok(result);
            }
            catch (OperationCanceledException)
            {
                return StatusCode(408, new { error = "Request was cancelled." });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid gas lift performance request for {WellUWI}", wellUWI);
                return BadRequest(new { error = ex.Message });
            }
            catch (GasLiftException ex)
            {
                _logger.LogWarning(ex, "Gas lift performance request rejected for {WellUWI}", wellUWI);
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting gas lift performance for well {WellUWI}", wellUWI);
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        private string GetUserId() => User.FindFirst("sub")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "SYSTEM";
    }
}

