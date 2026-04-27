using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.LifeCycle;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.Models.Data;
using Microsoft.Extensions.Logging;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.ApiService.Attributes;
using Beep.OilandGas.LifeCycle.Services.Exploration.Processes;
using Beep.OilandGas.ProspectIdentification;
using PROSPECT = Beep.OilandGas.Models.Data.ProspectIdentification.PROSPECT;

namespace Beep.OilandGas.ApiService.Controllers.Field
{
    /// <summary>
    /// API controller for Exploration phase business workflows, field-scoped
    /// 
    /// Process identifiers for exploration and gate flows are centralized in <see cref="ExplorationReferenceCodes"/>
    /// (e.g. <see cref="ExplorationReferenceCodes.ProcessIdLeadToProspect"/>,
    /// <see cref="ExplorationReferenceCodes.ProcessIdProspectToDiscovery"/>,
    /// <see cref="ExplorationReferenceCodes.ProcessIdGateExplorationReview"/>).
    /// 
    /// NOTE: This controller now owns the field-scoped prospect list/detail/create/delete boundary.
    /// More generic PPDM CRUD remains available through DataManagementController when needed.
    /// - Get seismic surveys: GET /api/datamanagement/SEIS_ACQTN_SURVEY
    /// - Create seismic survey: POST /api/datamanagement/SEIS_ACQTN_SURVEY
    /// - Get seismic lines: GET /api/datamanagement/SEIS_LINE
    /// - Get exploratory wells: GET /api/datamanagement/WELL with filters
    /// 
    /// This controller focuses on exploration workflow processes via ExplorationProcessService
    /// (lead funnel, prospect→discovery steps, discovery→development steps — see route summaries on actions).
    /// </summary>
    [ApiController]
    [Route("api/field/current/exploration")]
    [RequireCurrentFieldAccess]
    public class ExplorationController : ControllerBase
    {
        private readonly IFieldOrchestrator _fieldOrchestrator;
        private readonly Beep.OilandGas.LifeCycle.Services.Exploration.Processes.ExplorationProcessService _explorationProcessService;
        private readonly IFieldExplorationService _explorationService;
        private readonly Beep.OilandGas.ProductionAccounting.Services.ProductionAccountingService _productionAccountingService;
        private readonly ILogger<ExplorationController> _logger;

        public ExplorationController(
            IFieldOrchestrator fieldOrchestrator,
            Beep.OilandGas.LifeCycle.Services.Exploration.Processes.ExplorationProcessService explorationProcessService,
            IFieldExplorationService explorationService,
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
        public async Task<ActionResult<Beep.OilandGas.Models.Processes.ProcessInstance>> StartLeadToProspectProcess(
            [FromBody] StartLeadToProspectRequest request,
            CancellationToken cancellationToken)
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

                if (!await _explorationService.EnsureLeadInFieldForWorkflowStartAsync(
                        currentFieldId,
                        request.LeadId,
                        request.UserId).ConfigureAwait(false))
                {
                    return NotFound(new
                    {
                        error = $"Lead '{request.LeadId}' was not found for the current field."
                    });
                }

                var instance = await _explorationProcessService.StartLeadToProspectProcessAsync(
                    request.LeadId,
                    currentFieldId,
                    request.UserId,
                    cancellationToken);

                return Ok(instance);
            }
            catch (OperationCanceledException)
            {
                throw;
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
        public async Task<ActionResult<bool>> EvaluateLead(
            [FromBody] EvaluateLeadRequest request,
            CancellationToken cancellationToken)
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

                var scopeDenied = await EnsureWorkflowProcessMatchesCurrentFieldAsync(
                    request.InstanceId,
                    cancellationToken).ConfigureAwait(false);
                if (scopeDenied != null)
                    return scopeDenied;

                var result = await _explorationProcessService.EvaluateLeadAsync(
                    request.InstanceId,
                    request.EvaluationData ?? new Dictionary<string, object>(),
                    request.UserId,
                    cancellationToken);

                return Ok(result);
            }
            catch (OperationCanceledException)
            {
                throw;
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
        public async Task<ActionResult<bool>> ApproveLead(
            [FromBody] ApproveLeadRequest request,
            CancellationToken cancellationToken)
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

                var scopeDenied = await EnsureWorkflowProcessMatchesCurrentFieldAsync(
                    request.InstanceId,
                    cancellationToken).ConfigureAwait(false);
                if (scopeDenied != null)
                    return scopeDenied;

                var result = await _explorationProcessService.ApproveLeadAsync(
                    request.InstanceId,
                    request.UserId,
                    cancellationToken);
                return Ok(result);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving lead");
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        /// <summary>
        /// Reject lead (completes <c>LEAD_APPROVAL</c> with outcome <c>REJECTED</c>).
        /// </summary>
        [HttpPost("workflows/reject-lead")]
        public async Task<ActionResult<bool>> RejectLead(
            [FromBody] RejectLeadRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.InstanceId))
                    return BadRequest(new { error = "Instance ID is required." });
                if (string.IsNullOrWhiteSpace(request.UserId))
                    return BadRequest(new { error = "User ID is required." });

                var scopeDenied = await EnsureWorkflowProcessMatchesCurrentFieldAsync(
                    request.InstanceId,
                    cancellationToken).ConfigureAwait(false);
                if (scopeDenied != null)
                    return scopeDenied;

                var result = await _explorationProcessService.RejectLeadAsync(
                    request.InstanceId,
                    request.Reason ?? string.Empty,
                    request.UserId,
                    cancellationToken);
                return Ok(result);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting lead");
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        /// <summary>
        /// Run <c>PROSPECT_CREATION</c>, complete the step on success, then persist field prospect / lead status via <c>ILeadExplorationService</c>.
        /// </summary>
        [HttpPost("workflows/promote-lead-to-prospect")]
        public async Task<ActionResult<bool>> PromoteLeadToProspect(
            [FromBody] PromoteLeadToProspectRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.InstanceId))
                    return BadRequest(new { error = "Instance ID is required." });
                if (string.IsNullOrWhiteSpace(request.UserId))
                    return BadRequest(new { error = "User ID is required." });

                var scopeDenied = await EnsureWorkflowProcessMatchesCurrentFieldAsync(
                    request.InstanceId,
                    cancellationToken).ConfigureAwait(false);
                if (scopeDenied != null)
                    return scopeDenied;

                var stepData = request.ProspectData ?? new Dictionary<string, object>();
                var ok = await _explorationProcessService.PromoteLeadToProspectAsync(
                    request.InstanceId,
                    stepData,
                    request.UserId,
                    cancellationToken);
                return Ok(ok);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error promoting lead to prospect");
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        /// <summary>
        /// Start Prospect to Discovery workflow
        /// </summary>
        [HttpPost("workflows/prospect-to-discovery")]
        public async Task<ActionResult<Beep.OilandGas.Models.Processes.ProcessInstance>> StartProspectToDiscoveryProcess(
            [FromBody] StartProspectToDiscoveryRequest request,
            CancellationToken cancellationToken)
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

                if (await _explorationService.GetProspectForFieldAsync(currentFieldId, request.ProspectId).ConfigureAwait(false) == null)
                {
                    return NotFound(new
                    {
                        error = $"Prospect '{request.ProspectId}' was not found for the current field."
                    });
                }

                var instance = await _explorationProcessService.StartProspectToDiscoveryProcessAsync(
                    request.ProspectId,
                    currentFieldId,
                    request.UserId,
                    cancellationToken);

                return Ok(instance);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting Prospect to Discovery process");
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        /// <summary>
        /// Complete the prospect-to-discovery entry step (<see cref="ExplorationReferenceCodes.StepProspectCreation"/>; seed label <c>Prospect Readiness</c>).
        /// For <see cref="ExplorationReferenceCodes.ProcessNameLeadToProspect"/> use <c>POST …/promote-lead-to-prospect</c> (runs <c>ILeadExplorationService</c>).
        /// Returns <c>200 OK</c> with <c>false</c> when the underlying process service declines execution/completion without throwing.
        /// </summary>
        [HttpPost("workflows/prospect-to-discovery/prospect-readiness")]
        public Task<ActionResult<bool>> ProspectToDiscoveryProspectReadiness(
            [FromBody] ExplorationWorkflowStepRequest request,
            CancellationToken cancellationToken) =>
            RunExplorationWorkflowStepAsync(
                request,
                cancellationToken,
                "Error completing prospect readiness step",
                (id, data, user, ct) => _explorationProcessService.CompleteProspectReadinessStepAsync(id, data, user, ct));

        /// <summary>Execute <c>RISK_ASSESSMENT</c> for an active Prospect-to-Discovery process instance.</summary>
        [HttpPost("workflows/prospect-to-discovery/risk-assessment")]
        public Task<ActionResult<bool>> ProspectToDiscoveryRiskAssessment(
            [FromBody] ExplorationWorkflowStepRequest request,
            CancellationToken cancellationToken) =>
            RunExplorationWorkflowStepAsync(
                request,
                cancellationToken,
                "Error executing risk assessment step",
                (id, data, user, ct) => _explorationProcessService.PerformRiskAssessmentAsync(id, data, user, ct));

        /// <summary>Execute <c>VOLUME_ESTIMATION</c>.</summary>
        [HttpPost("workflows/prospect-to-discovery/volume-estimation")]
        public Task<ActionResult<bool>> ProspectToDiscoveryVolumeEstimation(
            [FromBody] ExplorationWorkflowStepRequest request,
            CancellationToken cancellationToken) =>
            RunExplorationWorkflowStepAsync(
                request,
                cancellationToken,
                "Error executing volume estimation step",
                (id, data, user, ct) => _explorationProcessService.PerformVolumeEstimationAsync(id, data, user, ct));

        /// <summary>Execute <c>ECONOMIC_EVALUATION</c>.</summary>
        [HttpPost("workflows/prospect-to-discovery/economic-evaluation")]
        public Task<ActionResult<bool>> ProspectToDiscoveryEconomicEvaluation(
            [FromBody] ExplorationWorkflowStepRequest request,
            CancellationToken cancellationToken) =>
            RunExplorationWorkflowStepAsync(
                request,
                cancellationToken,
                "Error executing economic evaluation step",
                (id, data, user, ct) => _explorationProcessService.PerformEconomicEvaluationAsync(id, data, user, ct));

        /// <summary>Execute <c>DRILLING_DECISION</c>.</summary>
        [HttpPost("workflows/prospect-to-discovery/drilling-decision")]
        public Task<ActionResult<bool>> ProspectToDiscoveryDrillingDecision(
            [FromBody] ExplorationWorkflowStepRequest request,
            CancellationToken cancellationToken) =>
            RunExplorationWorkflowStepAsync(
                request,
                cancellationToken,
                "Error executing drilling decision step",
                (id, data, user, ct) => _explorationProcessService.MakeDrillingDecisionAsync(id, data, user, ct));

        /// <summary>Execute <c>DISCOVERY_RECORDING</c> and complete the step on success.</summary>
        [HttpPost("workflows/prospect-to-discovery/discovery-recording")]
        public Task<ActionResult<bool>> ProspectToDiscoveryDiscoveryRecording(
            [FromBody] ExplorationWorkflowStepRequest request,
            CancellationToken cancellationToken) =>
            RunExplorationWorkflowStepAsync(
                request,
                cancellationToken,
                "Error executing discovery recording step",
                (id, data, user, ct) => _explorationProcessService.RecordDiscoveryAsync(id, data, user, ct));

        /// <summary>
        /// Start Discovery to Development workflow
        /// </summary>
        [HttpPost("workflows/discovery-to-development")]
        public async Task<ActionResult<Beep.OilandGas.Models.Processes.ProcessInstance>> StartDiscoveryToDevelopmentProcess(
            [FromBody] StartDiscoveryToDevelopmentRequest request,
            CancellationToken cancellationToken)
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

                if (!await _explorationService.IsProspectDiscoveryInFieldAsync(currentFieldId, request.DiscoveryId).ConfigureAwait(false))
                {
                    return NotFound(new
                    {
                        error = $"Discovery '{request.DiscoveryId}' was not found for the current field."
                    });
                }

                var instance = await _explorationProcessService.StartDiscoveryToDevelopmentProcessAsync(
                    request.DiscoveryId,
                    currentFieldId,
                    request.UserId,
                    cancellationToken);

                return Ok(instance);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting Discovery to Development process");
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        /// <summary>Execute <c>APPRAISAL</c> for an active Discovery-to-Development process instance.</summary>
        [HttpPost("workflows/discovery-to-development/appraisal")]
        public Task<ActionResult<bool>> DiscoveryToDevelopmentAppraisal(
            [FromBody] ExplorationWorkflowStepRequest request,
            CancellationToken cancellationToken) =>
            RunExplorationWorkflowStepAsync(
                request,
                cancellationToken,
                "Error executing appraisal step",
                (id, data, user, ct) => _explorationProcessService.PerformAppraisalAsync(id, data, user, ct));

        /// <summary>Execute <c>RESERVE_ESTIMATION</c>.</summary>
        [HttpPost("workflows/discovery-to-development/reserve-estimation")]
        public Task<ActionResult<bool>> DiscoveryToDevelopmentReserveEstimation(
            [FromBody] ExplorationWorkflowStepRequest request,
            CancellationToken cancellationToken) =>
            RunExplorationWorkflowStepAsync(
                request,
                cancellationToken,
                "Error executing reserve estimation step",
                (id, data, user, ct) => _explorationProcessService.EstimateReservesAsync(id, data, user, ct));

        /// <summary>Execute <c>ECONOMIC_ANALYSIS</c> (discovery-to-development workflow step).</summary>
        [HttpPost("workflows/discovery-to-development/economic-analysis")]
        public Task<ActionResult<bool>> DiscoveryToDevelopmentEconomicAnalysis(
            [FromBody] ExplorationWorkflowStepRequest request,
            CancellationToken cancellationToken) =>
            RunExplorationWorkflowStepAsync(
                request,
                cancellationToken,
                "Error executing development economic analysis step",
                (id, data, user, ct) => _explorationProcessService.PerformDevelopmentEconomicAnalysisAsync(id, data, user, ct));

        /// <summary>Complete <c>DEVELOPMENT_APPROVAL</c> as <c>APPROVED</c>.</summary>
        [HttpPost("workflows/discovery-to-development/approve")]
        public Task<ActionResult<bool>> DiscoveryToDevelopmentApprove(
            [FromBody] ExplorationWorkflowStepRequest request,
            CancellationToken cancellationToken) =>
            RunApproveDevelopmentWorkflowAsync(request, cancellationToken);

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

        /// <summary>
        /// Returns a bad request when there is no current field or the process instance is not tied to it; otherwise <c>null</c>.
        /// </summary>
        private async Task<ActionResult<bool>?> EnsureWorkflowProcessMatchesCurrentFieldAsync(
            string instanceId,
            CancellationToken cancellationToken)
        {
            var fieldId = _fieldOrchestrator.CurrentFieldId;
            if (string.IsNullOrEmpty(fieldId))
                return new ActionResult<bool>(BadRequest(new { error = "No active field selected." }));

            if (!await _explorationProcessService.IsProcessInstanceInFieldAsync(
                    instanceId,
                    fieldId,
                    cancellationToken)
                .ConfigureAwait(false))
            {
                return new ActionResult<bool>(BadRequest(new
                {
                    error = "Process instance not found or is not tied to the current field."
                }));
            }

            return null;
        }

        /// <summary>
        /// Runs a workflow step for the current field-scoped process instance.
        /// Returns <c>200 OK</c> with the service boolean result (<c>true</c>/<c>false</c>);
        /// prerequisite violations are mapped to <c>409 Conflict</c>.
        /// </summary>
        private async Task<ActionResult<bool>> RunExplorationWorkflowStepAsync(
            ExplorationWorkflowStepRequest request,
            CancellationToken cancellationToken,
            string errorLogMessage,
            Func<string, PROCESS_STEP_DATA, string, CancellationToken, Task<bool>> execute)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.InstanceId))
                    return BadRequest(new { error = "Instance ID is required." });
                if (string.IsNullOrWhiteSpace(request.UserId))
                    return BadRequest(new { error = "User ID is required." });

                var scopeDenied = await EnsureWorkflowProcessMatchesCurrentFieldAsync(
                    request.InstanceId,
                    cancellationToken).ConfigureAwait(false);
                if (scopeDenied != null)
                    return scopeDenied;

                PROCESS_STEP_DATA stepData = request.StepData ?? new Dictionary<string, object>();
                var ok = await execute(request.InstanceId, stepData, request.UserId, cancellationToken);
                return Ok(ok);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (ExplorationWorkflowPrerequisiteException ex)
            {
                return Conflict(new
                {
                    error = "Workflow prerequisite not satisfied.",
                    ex.InstanceId,
                    attemptedStep = ex.AttemptedStepId,
                    prerequisiteStep = ex.PrerequisiteStepId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{Message} (instance {InstanceId})", errorLogMessage, request.InstanceId);
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        private async Task<ActionResult<bool>> RunApproveDevelopmentWorkflowAsync(
            ExplorationWorkflowStepRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.InstanceId))
                    return BadRequest(new { error = "Instance ID is required." });
                if (string.IsNullOrWhiteSpace(request.UserId))
                    return BadRequest(new { error = "User ID is required." });

                var scopeDenied = await EnsureWorkflowProcessMatchesCurrentFieldAsync(
                    request.InstanceId,
                    cancellationToken).ConfigureAwait(false);
                if (scopeDenied != null)
                    return scopeDenied;

                var ok = await _explorationProcessService.ApproveDevelopmentAsync(
                    request.InstanceId,
                    request.UserId,
                    cancellationToken);
                return Ok(ok);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (ExplorationWorkflowPrerequisiteException ex)
            {
                return Conflict(new
                {
                    error = "Workflow prerequisite not satisfied.",
                    ex.InstanceId,
                    attemptedStep = ex.AttemptedStepId,
                    prerequisiteStep = ex.PrerequisiteStepId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving development (instance {InstanceId})", request.InstanceId);
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
