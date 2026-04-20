using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.LifeCycle;
using Microsoft.Extensions.Logging;
using Beep.OilandGas.PPDM39.Models;

namespace Beep.OilandGas.ApiService.Controllers.Field
{
    /// <summary>
    /// API controller for Exploration phase business workflows, field-scoped
    /// 
    /// NOTE: For CRUD operations (Create, Read, Update, Delete), please use DataManagementController:
    /// - Get prospects: GET /api/datamanagement/PROSPECT?filters=[{"field":"FIELD_ID","operator":"equals","value":"{fieldId}"}]
    /// - Get prospect: GET /api/datamanagement/PROSPECT/{id}
    /// - Create prospect: POST /api/datamanagement/PROSPECT
    /// - Update prospect: PUT /api/datamanagement/PROSPECT/{id}
    /// - Delete prospect: DELETE /api/datamanagement/PROSPECT/{id}
    /// - Get seismic surveys: GET /api/datamanagement/SEIS_ACQTN_SURVEY
    /// - Create seismic survey: POST /api/datamanagement/SEIS_ACQTN_SURVEY
    /// - Get seismic lines: GET /api/datamanagement/SEIS_LINE
    /// - Get exploratory wells: GET /api/datamanagement/WELL with filters
    /// 
    /// This controller focuses on exploration workflow processes via ExplorationProcessService.
    /// </summary>
    [ApiController]
    [Route("api/field/current/exploration")]
    public class ExplorationController : ControllerBase
    {
        private readonly IFieldOrchestrator _fieldOrchestrator;
        private readonly Beep.OilandGas.LifeCycle.Services.Exploration.Processes.ExplorationProcessService _explorationProcessService;
        private readonly Beep.OilandGas.LifeCycle.Services.Exploration.PPDMExplorationService _explorationService;
        private readonly ILogger<ExplorationController> _logger;

        public ExplorationController(
            IFieldOrchestrator fieldOrchestrator,
            Beep.OilandGas.LifeCycle.Services.Exploration.Processes.ExplorationProcessService explorationProcessService,
            Beep.OilandGas.LifeCycle.Services.Exploration.PPDMExplorationService explorationService,
            ILogger<ExplorationController> logger)
        {
            _fieldOrchestrator = fieldOrchestrator ?? throw new ArgumentNullException(nameof(fieldOrchestrator));
            _explorationProcessService = explorationProcessService ?? throw new ArgumentNullException(nameof(explorationProcessService));
            _explorationService = explorationService ?? throw new ArgumentNullException(nameof(explorationService));
            _logger = logger;
        }

        // ============================================
        // DATA QUERY ENDPOINTS
        // ============================================

        /// <summary>GET /api/field/current/exploration/prospects</summary>
        [HttpGet("prospects")]
        public async Task<ActionResult<List<PROSPECT>>> GetProspectsAsync()
        {
            var fieldId = _fieldOrchestrator.CurrentFieldId ?? string.Empty;
            if (string.IsNullOrEmpty(fieldId)) return BadRequest(new { error = "No active field is set" });
            try
            {
                var prospects = await _explorationService.GetProspectsForFieldAsync(fieldId);
                return Ok(prospects ?? new List<PROSPECT>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching prospects for field {FieldId}", fieldId);
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        /// <summary>GET /api/field/current/exploration/prospects/{id}</summary>
        [HttpGet("prospects/{id}")]
        public async Task<ActionResult<PROSPECT>> GetProspectAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return BadRequest(new { error = "Prospect ID is required." });
            var fieldId = _fieldOrchestrator.CurrentFieldId ?? string.Empty;
            if (string.IsNullOrEmpty(fieldId)) return BadRequest(new { error = "No active field is set" });
            try
            {
                var prospect = await _explorationService.GetProspectForFieldAsync(fieldId, id);
                if (prospect == null) return NotFound();
                return Ok(prospect);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching prospect {Id}", id);
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        /// <summary>GET /api/field/current/exploration/seismic-surveys</summary>
        [HttpGet("seismic-surveys")]
        public async Task<ActionResult<List<SEIS_ACQTN_SURVEY>>> GetSeismicSurveysAsync()
        {
            var fieldId = _fieldOrchestrator.CurrentFieldId ?? string.Empty;
            if (string.IsNullOrEmpty(fieldId)) return BadRequest(new { error = "No active field is set" });
            try
            {
                var surveys = await _explorationService.GetSeismicSurveysForFieldAsync(fieldId);
                return Ok(surveys ?? new List<SEIS_ACQTN_SURVEY>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching seismic surveys for field {FieldId}", fieldId);
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        /// <summary>GET /api/field/current/exploration/dashboard-summary</summary>
        [HttpGet("dashboard-summary")]
        public async Task<ActionResult<ExplorationDashboardSummary>> GetDashboardSummaryAsync()
        {
            var fieldId = _fieldOrchestrator.CurrentFieldId ?? string.Empty;
            if (string.IsNullOrEmpty(fieldId)) return BadRequest(new { error = "No active field is set" });
            try
            {
                var prospects = await _explorationService.GetProspectsForFieldAsync(fieldId);
                var surveys   = await _explorationService.GetSeismicSurveysForFieldAsync(fieldId);
                var wells     = await _explorationService.GetExploratoryWellsForFieldAsync(fieldId);

                var pending = (prospects ?? new())
                    .Where(p => p.PROSPECT_STATUS == "REVIEW" || p.PROSPECT_STATUS == "IN_PROGRESS" || p.PROSPECT_STATUS == "SCREENING")
                    .Select(p => new PendingDecisionDto(
                        p.PROSPECT_NAME ?? p.PROSPECT_ID,
                        $"Status: {p.PROSPECT_STATUS}",
                        p.PROSPECT_STATUS ?? "UNKNOWN"))
                    .Take(5)
                    .ToList();

                var upcoming = (prospects ?? new())
                    .Where(p => p.ACTIVE_IND == "Y")
                    .Take(3)
                    .Select(p => new WellProgramDto(
                        p.PROSPECT_NAME ?? p.PROSPECT_ID,
                        p.DISCOVERY_DATE.HasValue ? p.DISCOVERY_DATE.Value.ToString("Q? yyyy") : "TBD",
                        p.PROSPECT_STATUS ?? "PLANNED"))
                    .ToList();

                var summary = new ExplorationDashboardSummary
                {
                    LeadCount      = prospects?.Count(p => p.PROSPECT_TYPE == "LEAD") ?? 0,
                    ProspectCount  = prospects?.Count(p => p.PROSPECT_TYPE != "LEAD") ?? 0,
                    WellCount      = wells?.Count ?? 0,
                    SurveyCount    = surveys?.Count ?? 0,
                    SuccessRatePct = wells?.Count > 0
                        ? wells.Count(w => w.CURRENT_STATUS != null && w.CURRENT_STATUS.Contains("PRODUCER", StringComparison.OrdinalIgnoreCase)) * 100.0 / wells.Count
                        : 0,
                    PendingDecisions = pending,
                    UpcomingPrograms = upcoming,
                };
                return Ok(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching exploration dashboard summary for field {FieldId}", fieldId);
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        // ============================================
        // EXPLORATION WORKFLOW ENDPOINTS
        // ============================================

        /// <summary>
        /// Start Lead to Prospect workflow
        /// </summary>
        [HttpPost("workflows/lead-to-prospect")]
        public async Task<ActionResult<Beep.OilandGas.LifeCycle.Models.Processes.ProcessInstance>> StartLeadToProspectProcess(
            [FromBody] StartLeadToProspectRequest request)
        {
            try
            {
                var currentFieldId = _fieldOrchestrator.CurrentFieldId;
                if (string.IsNullOrEmpty(currentFieldId))
                {
                    return BadRequest(new { error = "No active field is set" });
                }

                if (string.IsNullOrWhiteSpace(request.LeadId))
                {
                    return BadRequest(new { error = "LeadId is required" });
                }

                if (string.IsNullOrWhiteSpace(request.UserId))
                {
                    return BadRequest(new { error = "UserId is required" });
                }

                var instance = await _explorationProcessService.StartLeadToProspectProcessAsync(
                    request.LeadId, 
                    currentFieldId, 
                    request.UserId);
                
                return Ok(instance);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting Lead to Prospect process");
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        /// <summary>
        /// Evaluate lead
        /// </summary>
        [HttpPost("workflows/evaluate-lead")]
        public async Task<ActionResult<bool>> EvaluateLead([FromBody] EvaluateLeadRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.InstanceId))
                {
                    return BadRequest(new { error = "InstanceId is required" });
                }

                if (string.IsNullOrWhiteSpace(request.UserId))
                {
                    return BadRequest(new { error = "UserId is required" });
                }

                var result = await _explorationProcessService.EvaluateLeadAsync(
                    request.InstanceId, 
                    request.EvaluationData ?? new Dictionary<string, object>(), 
                    request.UserId);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error evaluating lead");
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        /// <summary>
        /// Approve lead
        /// </summary>
        [HttpPost("workflows/approve-lead")]
        public async Task<ActionResult<bool>> ApproveLead([FromBody] ApproveLeadRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.InstanceId))
                {
                    return BadRequest(new { error = "InstanceId is required" });
                }

                if (string.IsNullOrWhiteSpace(request.UserId))
                {
                    return BadRequest(new { error = "UserId is required" });
                }

                var result = await _explorationProcessService.ApproveLeadAsync(request.InstanceId, request.UserId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving lead");
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        /// <summary>
        /// Start Prospect to Discovery workflow
        /// </summary>
        [HttpPost("workflows/prospect-to-discovery")]
        public async Task<ActionResult<Beep.OilandGas.LifeCycle.Models.Processes.ProcessInstance>> StartProspectToDiscoveryProcess(
            [FromBody] StartProspectToDiscoveryRequest request)
        {
            try
            {
                var currentFieldId = _fieldOrchestrator.CurrentFieldId;
                if (string.IsNullOrEmpty(currentFieldId))
                {
                    return BadRequest(new { error = "No active field is set" });
                }

                if (string.IsNullOrWhiteSpace(request.ProspectId))
                {
                    return BadRequest(new { error = "ProspectId is required" });
                }

                if (string.IsNullOrWhiteSpace(request.UserId))
                {
                    return BadRequest(new { error = "UserId is required" });
                }

                var instance = await _explorationProcessService.StartProspectToDiscoveryProcessAsync(
                    request.ProspectId, 
                    currentFieldId, 
                    request.UserId);
                
                return Ok(instance);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting Prospect to Discovery process");
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        /// <summary>
        /// Start Discovery to Development workflow
        /// </summary>
        [HttpPost("workflows/discovery-to-development")]
        public async Task<ActionResult<Beep.OilandGas.LifeCycle.Models.Processes.ProcessInstance>> StartDiscoveryToDevelopmentProcess(
            [FromBody] StartDiscoveryToDevelopmentRequest request)
        {
            try
            {
                var currentFieldId = _fieldOrchestrator.CurrentFieldId;
                if (string.IsNullOrEmpty(currentFieldId))
                {
                    return BadRequest(new { error = "No active field is set" });
                }

                if (string.IsNullOrWhiteSpace(request.DiscoveryId))
                {
                    return BadRequest(new { error = "DiscoveryId is required" });
                }

                if (string.IsNullOrWhiteSpace(request.UserId))
                {
                    return BadRequest(new { error = "UserId is required" });
                }

                var instance = await _explorationProcessService.StartDiscoveryToDevelopmentProcessAsync(
                    request.DiscoveryId, 
                    currentFieldId, 
                    request.UserId);
                
                return Ok(instance);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting Discovery to Development process");
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        /// <summary>POST /api/field/current/exploration/prospects/{id}/decision</summary>
        /// Records an approval gate decision (Approved / Deferred / Rejected) for an exploration well program.
        /// Updates PROSPECT_STATUS on the prospect record.
        [HttpPost("prospects/{id}/decision")]
        public async Task<IActionResult> PostProspectDecisionAsync(string id, [FromBody] ProspectDecisionRequest request)
        {
            if (string.IsNullOrWhiteSpace(id)) return BadRequest(new { error = "Prospect ID is required." });
            var fieldId = _fieldOrchestrator.CurrentFieldId ?? string.Empty;
            if (string.IsNullOrEmpty(fieldId)) return BadRequest(new { error = "No active field is set" });
            if (string.IsNullOrWhiteSpace(request.Decision))
                return BadRequest(new { error = "Decision is required" });

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "SYSTEM";
            try
            {
                var prospect = await _explorationService.GetProspectForFieldAsync(fieldId, id);
                if (prospect == null) return NotFound(new { error = $"Prospect {id} not found in field {fieldId}" });

                var newStatus = request.Decision?.ToUpperInvariant() switch
                {
                    "APPROVED" => "APPROVED",
                    "DEFERRED" => "DEFERRED",
                    "REJECTED" => "REJECTED",
                    _          => "REVIEWED"
                };

                await _explorationService.UpdateProspectStatusAsync(fieldId, id, newStatus, userId);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording decision for prospect {ProspectId}", id);
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

    }

    // ── Response DTOs ─────────────────────────────────────────────────────────
    public class ExplorationDashboardSummary
    {
        public int LeadCount       { get; set; }
        public int ProspectCount   { get; set; }
        public int WellCount       { get; set; }
        public int SurveyCount     { get; set; }
        public double SuccessRatePct { get; set; }
        public List<PendingDecisionDto> PendingDecisions  { get; set; } = new();
        public List<WellProgramDto>     UpcomingPrograms  { get; set; } = new();
    }

    public record PendingDecisionDto(string Name, string Description, string Status);
    public record WellProgramDto(string Name, string TargetDate, string Status);

    public class ProspectDecisionRequest
    {
        public string? Decision { get; set; }
        public string? Comments { get; set; }
    }
}
