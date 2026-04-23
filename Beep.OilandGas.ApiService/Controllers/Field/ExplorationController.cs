using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.LifeCycle;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.Models.Data;
using Microsoft.Extensions.Logging;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.ApiService.Attributes;

namespace Beep.OilandGas.ApiService.Controllers.Field
{
    /// <summary>
    /// API controller for Exploration phase business workflows, field-scoped
    /// 
    /// NOTE: This controller now owns the field-scoped prospect list/detail/create/delete boundary.
    /// More generic PPDM CRUD remains available through DataManagementController when needed.
    /// - Get seismic surveys: GET /api/datamanagement/SEIS_ACQTN_SURVEY
    /// - Create seismic survey: POST /api/datamanagement/SEIS_ACQTN_SURVEY
    /// - Get seismic lines: GET /api/datamanagement/SEIS_LINE
    /// - Get exploratory wells: GET /api/datamanagement/WELL with filters
    /// 
    /// This controller focuses on exploration workflow processes via ExplorationProcessService.
    /// </summary>
    [ApiController]
    [Route("api/field/current/exploration")]
    [RequireCurrentFieldAccess]
    public class ExplorationController : ControllerBase
    {
        private readonly IFieldOrchestrator _fieldOrchestrator;
        private readonly Beep.OilandGas.LifeCycle.Services.Exploration.Processes.ExplorationProcessService _explorationProcessService;
        private readonly Beep.OilandGas.LifeCycle.Services.Exploration.PPDMExplorationService _explorationService;
        private readonly Beep.OilandGas.ProductionAccounting.Services.ProductionAccountingService _productionAccountingService;
        private readonly ILogger<ExplorationController> _logger;

        public ExplorationController(
            IFieldOrchestrator fieldOrchestrator,
            Beep.OilandGas.LifeCycle.Services.Exploration.Processes.ExplorationProcessService explorationProcessService,
            Beep.OilandGas.LifeCycle.Services.Exploration.PPDMExplorationService explorationService,
            Beep.OilandGas.ProductionAccounting.Services.ProductionAccountingService productionAccountingService,
            ILogger<ExplorationController> logger)
        {
            _fieldOrchestrator = fieldOrchestrator ?? throw new ArgumentNullException(nameof(fieldOrchestrator));
            _explorationProcessService = explorationProcessService ?? throw new ArgumentNullException(nameof(explorationProcessService));
            _explorationService = explorationService ?? throw new ArgumentNullException(nameof(explorationService));
            _productionAccountingService = productionAccountingService ?? throw new ArgumentNullException(nameof(productionAccountingService));
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
                if (string.IsNullOrEmpty(fieldId)) return BadRequest(new { error = "No active field selected." });
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

        /// <summary>POST /api/field/current/exploration/prospects</summary>
        [HttpPost("prospects")]
        public async Task<ActionResult<PROSPECT>> CreateProspectAsync([FromBody] ProspectRequest request, [FromQuery] string? userId = null)
        {
            var fieldId = _fieldOrchestrator.CurrentFieldId ?? string.Empty;
            if (string.IsNullOrEmpty(fieldId)) return BadRequest(new { error = "No active field selected." });
            if (request == null) return BadRequest(new { error = "Prospect payload is required." });
            if (string.IsNullOrWhiteSpace(request.ProspectName)) return BadRequest(new { error = "Prospect name is required." });

            var effectiveUserId = userId ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "SYSTEM";

            try
            {
                var createdProspect = await _explorationService.CreateProspectForFieldAsync(fieldId, request, effectiveUserId);
                return Ok(createdProspect);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating prospect for field {FieldId}", fieldId);
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        /// <summary>GET /api/field/current/exploration/prospects/{id}</summary>
        [HttpGet("prospects/{id}")]
        public async Task<ActionResult<PROSPECT>> GetProspectAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return BadRequest(new { error = "Prospect ID is required." });
            var fieldId = _fieldOrchestrator.CurrentFieldId ?? string.Empty;
                if (string.IsNullOrEmpty(fieldId)) return BadRequest(new { error = "No active field selected." });
            try
            {
                var prospect = await _explorationService.GetProspectForFieldAsync(fieldId, id);
                if (prospect == null) return NotFound(new { error = $"Prospect {id} not found in field {fieldId}." });
                return Ok(prospect);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching prospect {Id}", id);
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        /// <summary>GET /api/field/current/exploration/prospects/{id}/afe-lines</summary>
        [HttpGet("prospects/{id}/afe-lines")]
        public async Task<ActionResult<List<ProspectAfeLineDto>>> GetProspectAfeLinesAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return BadRequest(new { error = "Prospect ID is required." });

            var fieldId = _fieldOrchestrator.CurrentFieldId ?? string.Empty;
            if (string.IsNullOrEmpty(fieldId)) return BadRequest(new { error = "No active field selected." });

            try
            {
                var prospect = await _explorationService.GetProspectForFieldAsync(fieldId, id);
                if (prospect == null) return NotFound(new { error = $"Prospect {id} not found in field {fieldId}." });

                var afes = (await _productionAccountingService.GetAfesAsync(propertyId: id))
                    .Where(afe => string.IsNullOrWhiteSpace(afe.FIELD_ID) || string.Equals(afe.FIELD_ID, fieldId, StringComparison.OrdinalIgnoreCase))
                    .Where(afe => !string.IsNullOrWhiteSpace(afe.AFE_ID))
                    .OrderByDescending(GetAfeSortDate)
                    .ToList();

                var results = new List<ProspectAfeLineDto>();

                foreach (var afe in afes)
                {
                    var afeId = afe.AFE_ID ?? string.Empty;
                    var lineItems = await _productionAccountingService.GetAfeLineItemsAsync(afeId);

                    results.AddRange(lineItems.Select(lineItem => new ProspectAfeLineDto(
                        afeId,
                        afe.AFE_NUMBER ?? afeId,
                        afe.AFE_NAME ?? afe.DESCRIPTION ?? afe.AFE_NUMBER ?? afeId,
                        lineItem.COST_CATEGORY ?? "Uncategorized",
                        string.IsNullOrWhiteSpace(lineItem.DESCRIPTION) ? afe.DESCRIPTION ?? "AFE line item" : lineItem.DESCRIPTION,
                        lineItem.BUDGET_AMOUNT ?? lineItem.ACTUAL_AMOUNT ?? 0m,
                        afe.STATUS ?? afe.ACTIVE_IND ?? string.Empty)));
                }

                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching AFE lines for prospect {ProspectId}", id);
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        /// <summary>PUT /api/field/current/exploration/prospects/{id}</summary>
        [HttpPut("prospects/{id}")]
        public async Task<ActionResult<PROSPECT>> UpdateProspectAsync(string id, [FromBody] ProspectRequest request, [FromQuery] string? userId = null)
        {
            if (string.IsNullOrWhiteSpace(id)) return BadRequest(new { error = "Prospect ID is required." });
            var fieldId = _fieldOrchestrator.CurrentFieldId ?? string.Empty;
            if (string.IsNullOrEmpty(fieldId)) return BadRequest(new { error = "No active field selected." });
            if (request == null) return BadRequest(new { error = "Prospect payload is required." });

            var effectiveUserId = userId ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "SYSTEM";

            try
            {
                var existing = await _explorationService.GetProspectForFieldAsync(fieldId, id);
                if (existing == null) return NotFound(new { error = $"Prospect {id} not found in field {fieldId}." });

                var updatedProspect = await _explorationService.UpdateProspectForFieldAsync(fieldId, id, request, effectiveUserId);
                return Ok(updatedProspect);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating prospect {ProspectId} for field {FieldId}", id, fieldId);
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        /// <summary>DELETE /api/field/current/exploration/prospects/{id}</summary>
        [HttpDelete("prospects/{id}")]
        public async Task<IActionResult> DeleteProspectAsync(string id, [FromQuery] string? userId = null)
        {
            if (string.IsNullOrWhiteSpace(id)) return BadRequest(new { error = "Prospect ID is required." });
            var fieldId = _fieldOrchestrator.CurrentFieldId ?? string.Empty;
            if (string.IsNullOrEmpty(fieldId)) return BadRequest(new { error = "No active field selected." });

            var effectiveUserId = userId ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "SYSTEM";

            try
            {
                var deleted = await _explorationService.DeleteProspectForFieldAsync(fieldId, id, effectiveUserId);
                if (!deleted) return NotFound(new { error = $"Prospect {id} not found in field {fieldId}." });
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting prospect {ProspectId} for field {FieldId}", id, fieldId);
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        /// <summary>GET /api/field/current/exploration/seismic-surveys</summary>
        [HttpGet("seismic-surveys")]
        public async Task<ActionResult<List<SEIS_ACQTN_SURVEY>>> GetSeismicSurveysAsync()
        {
            var fieldId = _fieldOrchestrator.CurrentFieldId ?? string.Empty;
                if (string.IsNullOrEmpty(fieldId)) return BadRequest(new { error = "No active field selected." });
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

        /// <summary>POST /api/field/current/exploration/seismic-surveys</summary>
        [HttpPost("seismic-surveys")]
        public async Task<ActionResult<SEIS_ACQTN_SURVEY>> CreateSeismicSurveyAsync([FromBody] SeismicSurveyRequest request, [FromQuery] string? userId = null)
        {
            var fieldId = _fieldOrchestrator.CurrentFieldId ?? string.Empty;
            if (string.IsNullOrEmpty(fieldId)) return BadRequest(new { error = "No active field selected." });
            if (request == null) return BadRequest(new { error = "Seismic survey payload is required." });
            if (string.IsNullOrWhiteSpace(request.SurveyName)) return BadRequest(new { error = "Survey name is required." });

            var effectiveUserId = userId ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "SYSTEM";

            try
            {
                var createdSurvey = await _explorationService.CreateSeismicSurveyForFieldAsync(fieldId, request, effectiveUserId);
                return Ok(createdSurvey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating seismic survey for field {FieldId}", fieldId);
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        /// <summary>GET /api/field/current/exploration/seismic-surveys/{surveyId}/lines</summary>
        [HttpGet("seismic-surveys/{surveyId}/lines")]
        public async Task<ActionResult<List<SEIS_LINE>>> GetSeismicLinesAsync(string surveyId)
        {
            if (string.IsNullOrWhiteSpace(surveyId)) return BadRequest(new { error = "Survey ID is required." });

            var fieldId = _fieldOrchestrator.CurrentFieldId ?? string.Empty;
            if (string.IsNullOrEmpty(fieldId)) return BadRequest(new { error = "No active field selected." });

            try
            {
                var surveys = await _explorationService.GetSeismicSurveysForFieldAsync(fieldId);
                if (!surveys.Any(s => string.Equals(s.SEIS_ACQTN_SURVEY_ID, surveyId, StringComparison.OrdinalIgnoreCase)))
                    return NotFound(new { error = $"Survey {surveyId} not found in field {fieldId}." });

                var lines = await _explorationService.GetSeismicLinesForSurveyAsync(surveyId);
                return Ok(lines ?? new List<SEIS_LINE>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching seismic lines for survey {SurveyId}", surveyId);
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        /// <summary>GET /api/field/current/exploration/dashboard-summary</summary>
        [HttpGet("dashboard-summary")]
        public async Task<ActionResult<ExplorationDashboardSummary>> GetDashboardSummaryAsync()
        {
            var fieldId = _fieldOrchestrator.CurrentFieldId ?? string.Empty;
                if (string.IsNullOrEmpty(fieldId)) return BadRequest(new { error = "No active field selected." });
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
                        return BadRequest(new { error = "No active field selected." });
                }

                if (string.IsNullOrWhiteSpace(request.LeadId))
                {
                        return BadRequest(new { error = "Lead ID is required." });
                }

                if (string.IsNullOrWhiteSpace(request.UserId))
                {
                        return BadRequest(new { error = "User ID is required." });
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
                        return BadRequest(new { error = "Instance ID is required." });
                }

                if (string.IsNullOrWhiteSpace(request.UserId))
                {
                        return BadRequest(new { error = "User ID is required." });
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
                        return BadRequest(new { error = "Instance ID is required." });
                }

                if (string.IsNullOrWhiteSpace(request.UserId))
                {
                        return BadRequest(new { error = "User ID is required." });
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
                        return BadRequest(new { error = "No active field selected." });
                }

                if (string.IsNullOrWhiteSpace(request.ProspectId))
                {
                        return BadRequest(new { error = "Prospect ID is required." });
                }

                if (string.IsNullOrWhiteSpace(request.UserId))
                {
                        return BadRequest(new { error = "User ID is required." });
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
                        return BadRequest(new { error = "No active field selected." });
                }

                if (string.IsNullOrWhiteSpace(request.DiscoveryId))
                {
                        return BadRequest(new { error = "Discovery ID is required." });
                }

                if (string.IsNullOrWhiteSpace(request.UserId))
                {
                        return BadRequest(new { error = "User ID is required." });
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
                if (string.IsNullOrEmpty(fieldId)) return BadRequest(new { error = "No active field selected." });
            if (string.IsNullOrWhiteSpace(request.Decision))
                    return BadRequest(new { error = "Decision is required." });

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "SYSTEM";
            try
            {
                var prospect = await _explorationService.GetProspectForFieldAsync(fieldId, id);
                    if (prospect == null) return NotFound(new { error = $"Prospect {id} not found in field {fieldId}." });

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

        private static DateTime GetAfeSortDate(AFE afe)
        {
            return afe.APPROVAL_DATE
                ?? afe.ROW_CHANGED_DATE
                ?? afe.ROW_EFFECTIVE_DATE
                ?? afe.ROW_CREATED_DATE
                ?? DateTime.MinValue;
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
    public record ProspectAfeLineDto(string AfeId, string AfeNumber, string AfeName, string Category, string Description, decimal CostUsd, string Status);

    public class ProspectDecisionRequest
    {
        public string? Decision { get; set; }
        public string? Comments { get; set; }
    }
}
