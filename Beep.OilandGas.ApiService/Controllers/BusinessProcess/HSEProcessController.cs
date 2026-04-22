using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.LifeCycle.Services.Processes;
using Beep.OilandGas.LifeCycle.Models.Processes;

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
    public class HSEProcessController : ControllerBase
    {
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
                // Map incident type to process definition id
                var processId = $"HSE_INCIDENT_REPORTING";
                var incidentEntityId = Guid.NewGuid().ToString();

                var instance = await _processService.StartProcessAsync(
                    processId,
                    incidentEntityId,
                    "HSE_INCIDENT",
                    fieldId,
                    userId);

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

                return CreatedAtAction(nameof(GetIncidentAsync), new { incidentId = instance.InstanceId }, instance);
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
                var instances = await _processService.GetProcessInstancesForEntityAsync(fieldId, "HSE_INCIDENT");
                var summaries = new List<ProcessInstanceSummary>();

                if (instances != null)
                {
                    foreach (var inst in instances)
                    {
                        var summary = new ProcessInstanceSummary
                        {
                            InstanceId = inst.InstanceId,
                            ProcessId = inst.ProcessId,
                            EntityId = inst.EntityId,
                            EntityType = inst.EntityType,
                            CurrentStepId = inst.CurrentStepId,
                            Status = inst.Status.ToString(),
                            StartedAt = inst.StartDate,
                            StartedBy = inst.StartedBy
                        };

                        // Apply optional filters
                        if (!string.IsNullOrWhiteSpace(status) &&
                            !string.Equals(summary.Status, status, StringComparison.OrdinalIgnoreCase))
                            continue;

                        summaries.Add(summary);
                    }
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
                var instance = await _processService.GetProcessInstanceAsync(incidentId);
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
                var instance = await _processService.GetProcessInstanceAsync(incidentId);
                if (instance == null)
                    return NotFound(new { error = $"Incident '{incidentId}' not found." });

                // Transition to RCA_IN_PROGRESS state
                await _processService.TransitionStateAsync(incidentId, "RCA_IN_PROGRESS", userId);

                // Record RCA submission
                await _processService.AddHistoryEntryAsync(incidentId, new ProcessHistoryEntry
                {
                    HistoryId = Guid.NewGuid().ToString(),
                    InstanceId = incidentId,
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
                if (request.CorrectiveActions == null || request.CorrectiveActions.Count == 0)
                    return BadRequest(new { error = "At least one corrective action is required." });

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "system";

            try
            {
                var instance = await _processService.GetProcessInstanceAsync(incidentId);
                if (instance == null)
                    return NotFound(new { error = $"Incident '{incidentId}' not found." });

                foreach (var action in request.CorrectiveActions)
                {
                    await _processService.AddHistoryEntryAsync(incidentId, new ProcessHistoryEntry
                    {
                        HistoryId = Guid.NewGuid().ToString(),
                        InstanceId = incidentId,
                        Action = "CORRECTIVE_ACTION_RAISED",
                        Notes = action,
                        PerformedBy = userId,
                        Timestamp = DateTime.UtcNow,
                        ActionData = new Dictionary<string, object>
                        {
                            ["AssignedTo"] = request.AssignedToUserId ?? userId
                        }
                    });
                }

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
                var instance = await _processService.GetProcessInstanceAsync(incidentId);
                if (instance == null)
                    return NotFound(new { error = $"Incident '{incidentId}' not found." });

                await _processService.AddHistoryEntryAsync(incidentId, new ProcessHistoryEntry
                {
                    HistoryId = Guid.NewGuid().ToString(),
                    InstanceId = incidentId,
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
                var success = await _processService.CancelProcessAsync(incidentId, reason, userId);
                if (!success)
                    return NotFound(new { error = $"Incident '{incidentId}' not found or already closed." });
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error closing incident {IncidentId}", incidentId);
                return StatusCode(500, new { error = "Error closing incident." });
            }
        }
    }
}
