using System;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.GasLift.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Beep.OilandGas.ApiService.Services;
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
        private readonly Beep.OilandGas.GasLift.Services.GasLiftService _service;
        private readonly ILogger<GasLiftController> _logger;

        public GasLiftController(Beep.OilandGas.GasLift.Services.GasLiftService service, ILogger<GasLiftController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpPost("analyze-potential")]
        public async Task<ActionResult<GAS_LIFT_POTENTIAL_RESULT>> AnalyzePotential(
            [FromBody] AnalyzeGasLiftPotentialRequest request,
            CancellationToken cancellationToken)
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

        [HttpPost("design-valves")]
        public async Task<ActionResult<GAS_LIFT_VALVE_DESIGN_RESULT>> DesignValves(
            [FromBody] DesignValvesRequest request,
            CancellationToken cancellationToken)
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

        [HttpPost("design")]
        public async Task<ActionResult> SaveDesign(
            [FromBody] GAS_LIFT_DESIGN design,
            CancellationToken cancellationToken = default)
        {
            var userId = User.ActingUserId();
            if (design == null)
                return BadRequest(new { error = "Design payload is required." });
            cancellationToken.ThrowIfCancellationRequested();
            await _service.SaveGasLiftDesignAsync(design, userId);
            return Ok(new { message = "Gas lift design saved successfully", designId = design.DESIGN_ID });
        }

        [HttpGet("performance/{wellUWI}")]
        public async Task<ActionResult<GAS_LIFT_PERFORMANCE>> GetPerformance(
            string wellUWI,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(wellUWI)) return BadRequest(new { error = "Well UWI is required." });
            cancellationToken.ThrowIfCancellationRequested();
            var result = await _service.GetGasLiftPerformanceAsync(wellUWI);
            return Ok(result);
        }
    }
}
