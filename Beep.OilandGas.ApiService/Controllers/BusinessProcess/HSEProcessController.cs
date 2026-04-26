using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.HSE;
using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.LifeCycle.Services.Processes;
using Beep.OilandGas.LifeCycle.Models.Processes;
using Beep.OilandGas.ApiService.Attributes;

namespace Beep.OilandGas.ApiService.Controllers.BusinessProcess
{
    /// <summary>
    /// HSE incident and safety PROCESS WORKFLOW endpoints (IProcessService layer).
    /// Separate from the domain HSEController at api/field/current/hse.
    /// RCA submission and incident close require SafetyOfficer or Manager role.
    /// </summary>
    [ApiController]
    [Route("api/field/current/process/hse")]
    [Authorize]
    [RequireCurrentFieldAccess]
    public class HSEProcessController : ControllerBase
    {
        private const string HseIncidentEntityType = "HSE_INCIDENT";
        private static readonly string[] IncidentWorkflowSteps =
        {
            "INC_REPORT",
            "INC_CLASSIFICATION",
            "INC_NOTIFICATION",
            "INC_INVESTIGATION",
            "INC_RCA",
            "INC_CORRECTIVE_ACTIONS",
            "INC_VERIFICATION",
            "INC_CLOSURE"
        };

        private readonly IFieldOrchestrator _fieldOrchestrator;
        private readonly IProcessService _processService;
        private readonly ILogger<HSEProcessController> _logger;

        public HSEProcessController(
            IFieldOrchestrator fieldOrchestrator,
            IProcessService processService,
            ILogger<HSEProcessController> logger)
        {
            _fieldOrchestrator = fieldOrchestrator ?? throw new ArgumentNullException(nameof(fieldOrchestrator));
            _processService = processService ?? throw new ArgumentNullException(nameof(processService));
            _logger = logger;
        }

        private IFieldHSEService Hse => _fieldOrchestrator.GetHSEService();

        /// <summary>Report a new HSE incident. Severity follows API RP 754 tiers (TIER1–TIER4).</summary>
        [HttpPost("incidents")]
        [ProducesResponseType(typeof(ProcessInstance), 201)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<ProcessInstance>> ReportIncidentAsync(
            [FromBody] HSEIncidentReportRequest request)
        {
            if (request == null)
                 return BadRequest(new { error = "Request body is required." });
            if (string.IsNullOrWhiteSpace(request.IncidentType))
                 return BadRequest(new { error = "IncidentType is required." });
            if (string.IsNullOrWhiteSpace(request.Severity))
                 return BadRequest(new { error = "Severity (API RP 754 tier) is required." });

            var fieldId = _fieldOrchestrator.CurrentFieldId ?? string.Empty;
            if (string.IsNullOrEmpty(fieldId))
                 return BadRequest(new { error = "No active field selected." });

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "system";

            try
            {
                var processId = $"HSE_INCIDENT_REPORTING";
                var incident = await Hse.ReportIncidentAsync(new ReportIncidentRequest(
                    FieldId: fieldId,
                    IncidentType: request.IncidentType,
                    Tier: MapSeverityToTier(request.Severity),
                    IncidentDate: request.IncidentDateTime,
                    Location: request.LocationDescription,
                    Description: request.Description,
                    Jurisdiction: "USA"), userId);

                var instance = await _processService.StartProcessAsync(
                    processId,
                    incident.IncidentId,
                    HseIncidentEntityType,
                    fieldId,
                    userId);

                await SyncProcessStateToIncidentAsync(instance.InstanceId, incident.IncidentId, userId);

                // Record the incident details in process history
                await _processService.AddHistoryEntryAsync(instance.InstanceId, new ProcessHistoryEntry
                {
                    HistoryId = Guid.NewGuid().ToString(),
                    InstanceId = instance.InstanceId,
                    Action = "INCIDENT_REPORTED",
                    Notes = $"Type: {request.IncidentType} | Severity: {request.Severity} | Location: {request.LocationDescription} | {request.Description}",
                    PerformedBy = userId,
                    Timestamp = request.IncidentDateTime,
                    ActionData = new Dictionary<string, object>
                    {
                        ["IncidentType"] = request.IncidentType,
                        ["Severity"] = request.Severity,
                        ["LocationDescription"] = request.LocationDescription,
                        ["InjuredPartyId"] = request.InjuredPartyId
                    }
                });

                return CreatedAtAction(nameof(GetIncidentAsync), new { incidentId = incident.IncidentId }, instance);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reporting HSE incident for field {FieldId}", fieldId);
                return StatusCode(500, new { error = "Error reporting incident." });
            }
        }

        /// <summary>List HSE incidents for the current field, optionally filtered by severity or status.</summary>
        [HttpGet("incidents")]
        [ProducesResponseType(typeof(List<ProcessInstanceSummary>), 200)]
        public async Task<ActionResult<List<ProcessInstanceSummary>>> GetIncidentsAsync(
            [FromQuery] string? severity = null,
            [FromQuery] string? status = null)
        {
            var fieldId = _fieldOrchestrator.CurrentFieldId ?? string.Empty;
            if (string.IsNullOrEmpty(fieldId))
                    return BadRequest(new { error = "No active field selected." });

            try
            {
                var incidents = await Hse.GetIncidentsAsync(null);
                var summaries = new List<ProcessInstanceSummary>();

                foreach (var incident in incidents)
                {
                    if (!MatchesSeverity(incident, severity))
                    {
                        continue;
                    }

                    var inst = await GetLatestProcessInstanceForIncidentAsync(incident.IncidentId);
                    if (!string.IsNullOrWhiteSpace(status) &&
                        (inst == null || !string.Equals(inst.Status.ToString(), status, StringComparison.OrdinalIgnoreCase)))
                    {
                        continue;
                    }

                    summaries.Add(new ProcessInstanceSummary
                    {
                        InstanceId = inst?.InstanceId ?? string.Empty,
                        ProcessId = inst?.ProcessId ?? string.Empty,
                        EntityId = incident.IncidentId,
                        EntityType = "HSE_INCIDENT",
                        CurrentStepId = inst?.CurrentStepId ?? string.Empty,
                        Status = inst?.Status.ToString() ?? incident.CurrentState,
                        StartedAt = inst?.StartDate ?? incident.IncidentDate,
                        StartedBy = inst?.StartedBy ?? string.Empty,
                        CompletedAt = inst?.CompletionDate
                    });
                }

                return Ok(summaries);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listing HSE incidents for field {FieldId}", fieldId);
                return StatusCode(500, new { error = "Error retrieving incidents." });
            }
        }

        /// <summary>Get a specific incident process instance.</summary>
        [HttpGet("incidents/{incidentId}")]
        [ProducesResponseType(typeof(ProcessInstance), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<ProcessInstance>> GetIncidentAsync(string incidentId)
        {
            if (string.IsNullOrWhiteSpace(incidentId))
                    return BadRequest(new { error = "Incident ID is required." });

            try
            {
                var instance = await GetLatestProcessInstanceForIncidentAsync(incidentId);
                if (instance == null)
                    return NotFound(new { error = $"Incident '{incidentId}' not found." });
                return Ok(instance);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving incident {IncidentId}", incidentId);
                return StatusCode(500, new { error = "Error retrieving incident." });
            }
        }

        /// <summary>Ensure an existing PPDM incident has an HSE workflow instance.</summary>
        [HttpPost("incidents/{incidentId}/ensure-workflow")]
        [ProducesResponseType(typeof(ProcessInstance), 200)]
        [ProducesResponseType(typeof(ProcessInstance), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<ProcessInstance>> EnsureWorkflowAsync(string incidentId)
        {
            if (string.IsNullOrWhiteSpace(incidentId))
                return BadRequest(new { error = "Incident ID is required." });

            var fieldId = _fieldOrchestrator.CurrentFieldId ?? string.Empty;
            if (string.IsNullOrEmpty(fieldId))
                return BadRequest(new { error = "No active field selected." });

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "system";

            try
            {
                var incident = await Hse.GetIncidentAsync(incidentId);
                if (incident == null)
                    return NotFound(new { error = $"Incident '{incidentId}' not found." });

                var existing = await GetLatestProcessInstanceForIncidentAsync(incidentId);
                if (existing != null)
                    return Ok(existing);

                var instance = await _processService.StartProcessAsync(
                    "HSE_INCIDENT_REPORTING",
                    incident.IncidentId,
                    HseIncidentEntityType,
                    fieldId,
                    userId);

                await SyncProcessStateToIncidentAsync(instance.InstanceId, incident.IncidentId, userId);

                await _processService.AddHistoryEntryAsync(instance.InstanceId, new ProcessHistoryEntry
                {
                    HistoryId = Guid.NewGuid().ToString(),
                    InstanceId = instance.InstanceId,
                    Action = "WORKFLOW_ENROLLED",
                    Notes = "Workflow created for an existing PPDM incident.",
                    PerformedBy = userId,
                    Timestamp = DateTime.UtcNow
                });

                return CreatedAtAction(nameof(GetIncidentAsync), new { incidentId = incident.IncidentId }, instance);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ensuring workflow for incident {IncidentId}", incidentId);
                return StatusCode(500, new { error = "Error ensuring workflow." });
            }
        }

        /// <summary>Submit a Root Cause Analysis for an incident. Requires SafetyOfficer or Manager role.</summary>
        [HttpPost("incidents/{incidentId}/rca")]
        [Authorize(Roles = "SafetyOfficer,Manager")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> SubmitRcaAsync(
            string incidentId,
            [FromBody] HSEIncidentUpdateRequest request)
        {
            if (string.IsNullOrWhiteSpace(incidentId))
                    return BadRequest(new { error = "Incident ID is required." });
                if (request == null)
                    return BadRequest(new { error = "Request body is required." });
                if (string.IsNullOrWhiteSpace(request.RcaSummary))
                    return BadRequest(new { error = "RcaSummary is required." });

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "system";

            try
            {
                var instance = await GetLatestProcessInstanceForIncidentAsync(incidentId);
                if (instance == null)
                    return NotFound(new { error = $"Incident '{incidentId}' not found." });

                await TransitionIncidentIfAvailableAsync(incidentId, "investigate", request.RcaSummary, userId);
                await TransitionIncidentIfAvailableAsync(incidentId, "rca_start", request.RcaSummary, userId);

                var causes = await Hse.GetCausesAsync(incidentId);
                if (!causes.Any(cause =>
                        string.Equals(cause.CauseType, CauseType.Root, StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(cause.Description, request.RcaSummary, StringComparison.Ordinal)))
                {
                    var nextSeq = causes.Count > 0 ? causes.Max(cause => cause.Seq) + 1 : 1;
                    await Hse.AddCauseAsync(incidentId, new AddCauseRequest(
                        CauseType: CauseType.Root,
                        CauseDesc: request.RcaSummary,
                        CauseCategory: string.IsNullOrWhiteSpace(request.RcaCauseCategory)
                            ? CauseCategory.Process
                            : request.RcaCauseCategory,
                        Seq: nextSeq), userId);
                }

                if (await Hse.IsRcaCompleteAsync(incidentId))
                {
                    await TransitionIncidentIfAvailableAsync(incidentId, "rca_complete", request.RcaSummary, userId);
                }

                await AdvanceWorkflowThroughStepAsync(instance.InstanceId, "INC_RCA", userId);
                await SyncProcessStateToIncidentAsync(instance.InstanceId, incidentId, userId);

                // Record RCA submission
                await _processService.AddHistoryEntryAsync(instance.InstanceId, new ProcessHistoryEntry
                {
                    HistoryId = Guid.NewGuid().ToString(),
                    InstanceId = instance.InstanceId,
                    Action = "RCA_SUBMITTED",
                    Notes = request.RcaSummary,
                    PerformedBy = userId,
                    Timestamp = DateTime.UtcNow
                });

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting RCA for incident {IncidentId}", incidentId);
                return StatusCode(500, new { error = "Error submitting RCA." });
            }
        }

        /// <summary>Raise corrective actions for an incident.</summary>
        [HttpPost("incidents/{incidentId}/actions")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> RaiseCorrectiveActionsAsync(
            string incidentId,
            [FromBody] HSEIncidentUpdateRequest request)
        {
            if (string.IsNullOrWhiteSpace(incidentId))
                    return BadRequest(new { error = "Incident ID is required." });
                if (request == null)
                    return BadRequest(new { error = "Request body is required." });
                var correctiveActions = request.CorrectiveActions?
                    .Where(action => !string.IsNullOrWhiteSpace(action))
                    .ToList() ?? new List<string>();

                if (correctiveActions.Count == 0)
                    return BadRequest(new { error = "At least one corrective action is required." });
                if (string.IsNullOrWhiteSpace(request.CorrectiveActionType))
                    return BadRequest(new { error = "CorrectiveActionType is required." });
                if (!request.CorrectiveActionDueDate.HasValue)
                    return BadRequest(new { error = "CorrectiveActionDueDate is required." });

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "system";

            try
            {
                var instance = await GetLatestProcessInstanceForIncidentAsync(incidentId);
                if (instance == null)
                    return NotFound(new { error = $"Incident '{incidentId}' not found." });

                await Hse.CreateCaPlanAsync(incidentId, userId);

                foreach (var action in correctiveActions)
                {
                    var actionId = await Hse.AddCorrectiveActionAsync(incidentId, new AddCARequest(
                        CADescription: action,
                        CAType: request.CorrectiveActionType,
                        DueDate: request.CorrectiveActionDueDate.Value,
                        ResponsibleBaId: request.AssignedToUserId ?? string.Empty), userId);

                    await _processService.AddHistoryEntryAsync(instance.InstanceId, new ProcessHistoryEntry
                    {
                        HistoryId = Guid.NewGuid().ToString(),
                        InstanceId = instance.InstanceId,
                        Action = "CORRECTIVE_ACTION_RAISED",
                        Notes = action,
                        PerformedBy = userId,
                        Timestamp = DateTime.UtcNow,
                        ActionData = new Dictionary<string, object>
                        {
                            ["ActionId"] = actionId,
                            ["ActionType"] = request.CorrectiveActionType,
                            ["AssignedTo"] = request.AssignedToUserId ?? userId,
                            ["DueDate"] = request.CorrectiveActionDueDate.Value
                        }
                    });
                }

                await AdvanceWorkflowThroughStepAsync(instance.InstanceId, "INC_CORRECTIVE_ACTIONS", userId);
                await SyncProcessStateToIncidentAsync(instance.InstanceId, incidentId, userId);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error raising corrective actions for incident {IncidentId}", incidentId);
                return StatusCode(500, new { error = "Error raising corrective actions." });
            }
        }

        /// <summary>Mark a corrective action as closed.</summary>
        [HttpPatch("incidents/{incidentId}/actions/{actionId}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> CloseCorrectiveActionAsync(
            string incidentId,
            string actionId,
            [FromBody] CorrectiveActionCloseRequest request)
        {
                if (string.IsNullOrWhiteSpace(incidentId))
                    return BadRequest(new { error = "Incident ID is required." });
                if (string.IsNullOrWhiteSpace(actionId))
                    return BadRequest(new { error = "Action ID is required." });

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "system";
            var notes = request?.Notes ?? "Closed.";

            try
            {
                if (!TryParseCorrectiveActionStepSeq(actionId, out var stepSeq))
                    return BadRequest(new { error = "Action ID must contain the corrective-action step sequence." });

                var instance = await GetLatestProcessInstanceForIncidentAsync(incidentId);
                if (instance == null)
                    return NotFound(new { error = $"Incident '{incidentId}' not found." });

                await Hse.RecordCompletionAsync(incidentId, stepSeq, notes, userId);

                var correctiveActions = await Hse.GetCorrectiveActionsAsync(incidentId);
                if (correctiveActions.Count > 0 && correctiveActions.All(action =>
                        string.Equals(action.Status, "COMPLETED", StringComparison.OrdinalIgnoreCase)))
                {
                    await TransitionIncidentIfAvailableAsync(incidentId, "ca_done", notes, userId);
                    await SyncProcessStateToIncidentAsync(instance.InstanceId, incidentId, userId);
                }

                await _processService.AddHistoryEntryAsync(instance.InstanceId, new ProcessHistoryEntry
                {
                    HistoryId = Guid.NewGuid().ToString(),
                    InstanceId = instance.InstanceId,
                    Action = "CORRECTIVE_ACTION_CLOSED",
                    Notes = notes,
                    PerformedBy = userId,
                    Timestamp = DateTime.UtcNow,
                    ActionData = new Dictionary<string, object> { ["ActionId"] = actionId }
                });

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error closing corrective action {ActionId} for incident {IncidentId}", actionId, incidentId);
                return StatusCode(500, new { error = "Error closing corrective action." });
            }
        }

        /// <summary>Close an incident. Requires SafetyOfficer or Manager role.</summary>
        [HttpPost("incidents/{incidentId}/close")]
        [Authorize(Roles = "SafetyOfficer,Manager")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> CloseIncidentAsync(
            string incidentId,
            [FromBody] IncidentCloseRequest request)
        {
            if (string.IsNullOrWhiteSpace(incidentId))
                    return BadRequest(new { error = "Incident ID is required." });

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "system";
            var reason = request?.Reason ?? "Closed.";

            try
            {
                var instance = await GetLatestProcessInstanceForIncidentAsync(incidentId);
                if (instance == null)
                    return NotFound(new { error = $"Incident '{incidentId}' not found or already closed." });

                var correctiveActions = await Hse.GetCorrectiveActionsAsync(incidentId);
                var hasOpenCorrectiveActions = correctiveActions.Any() && correctiveActions.Any(action =>
                    !string.Equals(action.Status, "COMPLETED", StringComparison.OrdinalIgnoreCase));
                if (hasOpenCorrectiveActions)
                    return BadRequest(new { error = "All corrective actions must be completed before the incident can be closed." });

                await TransitionIncidentIfAvailableAsync(incidentId, "ca_done", reason, userId);

                var incident = await Hse.GetIncidentAsync(incidentId);
                if (incident == null)
                    return NotFound(new { error = $"Incident '{incidentId}' not found or already closed." });

                var availableTriggers = await Hse.GetAvailableTriggersAsync(incidentId);
                if (!availableTriggers.Contains("close", StringComparer.OrdinalIgnoreCase) &&
                    !string.Equals(incident.CurrentState, IncidentState.Closed, StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest(new { error = $"Incident '{incidentId}' is not ready for closure from state '{incident.CurrentState}'." });
                }

                await TransitionIncidentIfAvailableAsync(incidentId, "close", reason, userId);
                await AdvanceWorkflowThroughStepAsync(instance.InstanceId, "INC_CLOSURE", userId);
                await SyncProcessStateToIncidentAsync(instance.InstanceId, incidentId, userId);

                await _processService.AddHistoryEntryAsync(instance.InstanceId, new ProcessHistoryEntry
                {
                    HistoryId = Guid.NewGuid().ToString(),
                    InstanceId = instance.InstanceId,
                    Action = "INCIDENT_CLOSED",
                    Notes = reason,
                    PerformedBy = userId,
                    Timestamp = DateTime.UtcNow
                });

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error closing incident {IncidentId}", incidentId);
                return StatusCode(500, new { error = "Error closing incident." });
            }
        }

        private async Task<ProcessInstance?> GetLatestProcessInstanceForIncidentAsync(string incidentId)
        {
            var current = await _processService.GetCurrentProcessForEntityAsync(incidentId, HseIncidentEntityType);
            if (current != null)
            {
                return current;
            }

            var instances = await _processService.GetProcessInstancesForEntityAsync(incidentId, HseIncidentEntityType);
            return instances
                .OrderByDescending(instance => instance.StartDate)
                .FirstOrDefault();
        }

        private async Task TransitionIncidentIfAvailableAsync(string incidentId, string trigger, string? reason, string userId)
        {
            var availableTriggers = await Hse.GetAvailableTriggersAsync(incidentId);
            if (availableTriggers.Contains(trigger, StringComparer.OrdinalIgnoreCase))
            {
                await Hse.TransitionAsync(incidentId, trigger, reason, userId);
            }
        }

        private async Task AdvanceWorkflowThroughStepAsync(string instanceId, string targetStepId, string userId)
        {
            foreach (var stepId in IncidentWorkflowSteps)
            {
                var instance = await _processService.GetProcessInstanceAsync(instanceId);
                var stepInstance = instance?.StepInstances.FirstOrDefault(step => step.StepId == stepId);
                if (stepInstance == null)
                {
                    continue;
                }

                if (stepInstance.Status == StepStatus.PENDING)
                {
                    await _processService.ExecuteStepAsync(instanceId, stepId, new PROCESS_STEP_DATA(), userId);
                    instance = await _processService.GetProcessInstanceAsync(instanceId);
                    stepInstance = instance?.StepInstances.FirstOrDefault(step => step.StepId == stepId);
                }

                if (stepInstance?.Status == StepStatus.IN_PROGRESS)
                {
                    await _processService.CompleteStepAsync(instanceId, stepId, "Completed", userId);
                }

                if (string.Equals(stepId, targetStepId, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }
            }
        }

        private async Task SyncProcessStateToIncidentAsync(string instanceId, string incidentId, string userId)
        {
            var incident = await Hse.GetIncidentAsync(incidentId);
            if (incident == null)
            {
                return;
            }

            var instance = await _processService.GetProcessInstanceAsync(instanceId);
            if (instance == null || string.Equals(instance.CurrentState, incident.CurrentState, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            await _processService.TransitionStateAsync(instanceId, incident.CurrentState, userId);
        }

        private static bool TryParseCorrectiveActionStepSeq(string actionId, out int stepSeq)
        {
            stepSeq = 0;
            if (string.IsNullOrWhiteSpace(actionId))
            {
                return false;
            }

            var endIndex = actionId.Length - 1;
            while (endIndex >= 0 && !char.IsDigit(actionId[endIndex]))
            {
                endIndex--;
            }

            if (endIndex < 0)
            {
                return false;
            }

            var startIndex = endIndex;
            while (startIndex >= 0 && char.IsDigit(actionId[startIndex]))
            {
                startIndex--;
            }

            var digits = actionId.Substring(startIndex + 1, endIndex - startIndex);
            return int.TryParse(digits, out stepSeq);
        }

        private static int MapSeverityToTier(string severity)
        {
            if (string.IsNullOrWhiteSpace(severity))
            {
                return 4;
            }

            var digits = new string(severity.Where(char.IsDigit).ToArray());
            return int.TryParse(digits, out var tier) && tier is >= 1 and <= 4 ? tier : 4;
        }

        private static bool MatchesSeverity(HSEIncidentRecord incident, string? severity)
        {
            if (string.IsNullOrWhiteSpace(severity))
            {
                return true;
            }

            return incident.Tier == MapSeverityToTier(severity);
        }
    }
}
