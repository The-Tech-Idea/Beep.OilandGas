using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.Production;
using Beep.OilandGas.ApiService.Attributes;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.ApiService.Controllers.Field
{
    /// <summary>
    /// API controller for Reservoir Performance dashboard — field-scoped.
    /// </summary>
    [ApiController]
    [Route("api/field/current/reservoir")]
    [RequireCurrentFieldAccess]
    public class ReservoirController : ControllerBase
    {
        private readonly IFieldOrchestrator _fieldOrchestrator;
        private readonly Beep.OilandGas.LifeCycle.Services.Development.PPDMDevelopmentService _developmentService;
        private readonly ILogger<ReservoirController> _logger;

        public ReservoirController(
            IFieldOrchestrator fieldOrchestrator,
            Beep.OilandGas.LifeCycle.Services.Development.PPDMDevelopmentService developmentService,
            ILogger<ReservoirController> logger)
        {
            _fieldOrchestrator = fieldOrchestrator ?? throw new ArgumentNullException(nameof(fieldOrchestrator));
            _developmentService = developmentService ?? throw new ArgumentNullException(nameof(developmentService));
            _logger = logger;
        }

        /// <summary>GET /api/field/current/reservoir/dashboard/summary</summary>
        [HttpGet("dashboard/summary")]
        public async Task<ActionResult<ReservoirDashboardSummary>> GetDashboardSummaryAsync()
        {
            var fieldId = _fieldOrchestrator.CurrentFieldId ?? string.Empty;
                if (string.IsNullOrEmpty(fieldId)) return BadRequest(new { error = "No active field selected." });
            try
            {
                var summary = await _developmentService.GetReservoirDashboardSummaryAsync(fieldId);
                return Ok(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching reservoir dashboard summary for field {FieldId}", fieldId);
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        /// <summary>GET /api/field/current/reservoir/dashboard/pools</summary>
        [HttpGet("dashboard/pools")]
        public async Task<ActionResult<List<ReservoirPoolDto>>> GetDashboardPoolsAsync()
        {
            var fieldId = _fieldOrchestrator.CurrentFieldId ?? string.Empty;
                if (string.IsNullOrEmpty(fieldId)) return BadRequest(new { error = "No active field selected." });
            try
            {
                var pools = await _developmentService.GetReservoirPoolsAsync(fieldId);
                return Ok(pools);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching reservoir pools for field {FieldId}", fieldId);
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }
    }
}
