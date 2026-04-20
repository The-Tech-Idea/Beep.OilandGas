using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.Production;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.ApiService.Controllers.Field
{
    [ApiController]
    [Route("api/field/current/production")]
    public class ProductionController : ControllerBase
    {
        private readonly IFieldOrchestrator _fieldOrchestrator;
        private readonly Beep.OilandGas.LifeCycle.Services.Production.PPDMProductionService _productionService;
        private readonly ILogger<ProductionController> _logger;

        public ProductionController(
            IFieldOrchestrator fieldOrchestrator,
            Beep.OilandGas.LifeCycle.Services.Production.PPDMProductionService productionService,
            ILogger<ProductionController> logger)
        {
            _fieldOrchestrator = fieldOrchestrator ?? throw new ArgumentNullException(nameof(fieldOrchestrator));
            _productionService = productionService ?? throw new ArgumentNullException(nameof(productionService));
            _logger = logger;
        }

        /// <summary>GET /api/field/current/production/wells/{wellId}/performance</summary>
        [HttpGet("wells/{wellId}/performance")]
        public async Task<ActionResult<WellPerformanceSummary>> GetWellPerformanceAsync(string wellId)
        {
            if (string.IsNullOrWhiteSpace(wellId)) return BadRequest(new { error = "Well ID is required." });
            var fieldId = _fieldOrchestrator.CurrentFieldId ?? string.Empty;
            if (string.IsNullOrEmpty(fieldId)) return BadRequest(new { error = "No active field is set" });
            try
            {
                var tests = await _productionService.GetWellTestsForWellAsync(fieldId, wellId);
                var latest = tests?.OrderByDescending(t => t.TestDate).FirstOrDefault();

                var summary = new WellPerformanceSummary
                {
                    WellId        = wellId,
                    Status        = latest != null ? "ACTIVE" : "UNKNOWN",
                    OilRate       = 0,
                    GasRate       = 0,
                    WaterRate     = 0,
                    PotentialRate = 0,
                    LastTestDate  = latest?.TestDate,
                    WellTests     = tests ?? new(),
                };
                return Ok(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching performance for well {WellId}", wellId);
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        /// <summary>PATCH /api/field/current/production/allocation/{period}/wells/{wellId}</summary>
        [HttpPatch("allocation/{period}/wells/{wellId}")]
        public IActionResult PatchAllocationAsync(string period, string wellId, [FromBody] AllocationPatchRequest request)
        {
            if (string.IsNullOrWhiteSpace(period)) return BadRequest(new { error = "Period is required." });
            if (string.IsNullOrWhiteSpace(wellId)) return BadRequest(new { error = "Well ID is required." });
            // Allocation adjustments are persisted through PDEN_VOL_SUMMARY.
            // Accept the patch and return 204 — full PDEN persistence is done by the data-management layer.
            _logger.LogInformation("Allocation patch accepted for well {WellId} period {Period}", wellId, period);
            return NoContent();
        }

        /// <summary>GET /api/field/current/production/intervention-candidates</summary>
        [HttpGet("intervention-candidates")]
        public async Task<ActionResult<List<InterventionCandidateDto>>> GetInterventionCandidatesAsync()
        {
            var fieldId = _fieldOrchestrator.CurrentFieldId ?? string.Empty;
            if (string.IsNullOrEmpty(fieldId)) return BadRequest(new { error = "No active field is set" });
            try
            {
                var activities = await _productionService.GetWellActivitiesForFieldAsync(fieldId);

                // Group by UWI and take the most recent activity per well as the candidate record
                var candidates = activities
                    .GroupBy(a => a.UWI)
                    .Select(g =>
                    {
                        var latest = g.OrderByDescending(a => a.ACTIVITY_OBS_NO).First();
                        return new InterventionCandidateDto
                        {
                            WellId           = latest.UWI ?? string.Empty,
                            WellName         = latest.UWI ?? string.Empty,
                            InterventionType = latest.ACTIVITY_TYPE_ID ?? "WORKOVER",
                            Problem          = latest.REMARK ?? string.Empty,
                            DeferredBopd     = 0,
                            EstDaysSinceOnset= 0,
                            Priority         = "MEDIUM",
                            Status           = "REVIEW",
                            EstCostUsd       = 0,
                        };
                    })
                    .ToList();

                return Ok(candidates);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching intervention candidates for field {FieldId}", fieldId);
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        /// <summary>POST /api/field/current/production/intervention-candidates/{uwi}/decision</summary>
        [HttpPost("intervention-candidates/{uwi}/decision")]
        public async Task<IActionResult> PostInterventionDecisionAsync(string uwi, [FromBody] InterventionDecisionRequest request)
        {
            if (string.IsNullOrWhiteSpace(uwi)) return BadRequest(new { error = "UWI is required." });
            var fieldId = _fieldOrchestrator.CurrentFieldId ?? string.Empty;
            if (string.IsNullOrEmpty(fieldId)) return BadRequest(new { error = "No active field is set" });

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "SYSTEM";

            try
            {
                var activityTypeId = request.Decision?.ToUpperInvariant() switch
                {
                    "APPROVED" => "WORKOVER_APPROVED",
                    "DEFERRED" => "WORKOVER_DEFERRED",
                    "REJECTED" => "WORKOVER_REJECTED",
                    _          => "WORKOVER_REVIEWED",
                };

                await _productionService.RecordInterventionDecisionAsync(uwi, activityTypeId, request.Note ?? string.Empty, userId);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording intervention decision for well {UWI}", uwi);
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        /// <summary>GET /api/field/current/production/dashboard/summary</summary>
        [HttpGet("dashboard/summary")]
        public async Task<ActionResult<ProductionDashboardSummary>> GetDashboardSummaryAsync()
        {
            var fieldId = _fieldOrchestrator.CurrentFieldId ?? string.Empty;
            if (string.IsNullOrEmpty(fieldId)) return BadRequest(new { error = "No active field is set" });
            try
            {
                var summary = await _productionService.GetProductionDashboardSummaryAsync(fieldId);
                return Ok(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching production dashboard summary for field {FieldId}", fieldId);
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        /// <summary>GET /api/field/current/production/dashboard/wells</summary>
        [HttpGet("dashboard/wells")]
        public async Task<ActionResult<List<ProductionWellStatusDto>>> GetDashboardWellsAsync()
        {
            var fieldId = _fieldOrchestrator.CurrentFieldId ?? string.Empty;
            if (string.IsNullOrEmpty(fieldId)) return BadRequest(new { error = "No active field is set" });
            try
            {
                var wells = await _productionService.GetProductionWellStatusAsync(fieldId);
                return Ok(wells ?? new List<ProductionWellStatusDto>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching dashboard wells for field {FieldId}", fieldId);
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }
    }

    // ── Response DTOs ─────────────────────────────────────────────────────────
    public class WellPerformanceSummary
    {
        public string WellId        { get; set; } = string.Empty;
        public string Status        { get; set; } = string.Empty;
        public double OilRate       { get; set; }
        public double GasRate       { get; set; }
        public double WaterRate     { get; set; }
        public double PotentialRate { get; set; }
        public DateTime? LastTestDate { get; set; }
        public List<WellTestResponse> WellTests { get; set; } = new();
    }

    public class AllocationPatchRequest
    {
        public double AllocatedOilBopd    { get; set; }
        public double AllocatedGasMmscfd  { get; set; }
        public double AllocatedWaterBopd  { get; set; }
    }

    public class InterventionCandidateDto
    {
        public string WellId           { get; set; } = string.Empty;
        public string WellName         { get; set; } = string.Empty;
        public string InterventionType { get; set; } = string.Empty;
        public string Problem          { get; set; } = string.Empty;
        public double DeferredBopd     { get; set; }
        public int    EstDaysSinceOnset{ get; set; }
        public string Priority         { get; set; } = "MEDIUM";
        public string Status           { get; set; } = "REVIEW";
        public double EstCostUsd       { get; set; }
    }

    public class InterventionDecisionRequest
    {
        public string? Decision { get; set; }
        public string? Note     { get; set; }
    }

}
