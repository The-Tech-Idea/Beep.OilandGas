using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.LifeCycle.Services.Processes;
using Beep.OilandGas.Models.Processes;
using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.ApiService.Attributes;
using Beep.OilandGas.ApiService.Services;

namespace Beep.OilandGas.ApiService.Controllers.BusinessProcess
{
    /// <summary>
    /// Gate review workflow endpoints — submit, approve, reject, defer, and checklist retrieval.
    /// Approve/reject/defer require the GateApprover role.
    /// </summary>
    [ApiController]
    [Route("api/field/current/gates")]
    [Authorize]
    [RequireCurrentFieldAccess]
    public class GateReviewController : ControllerBase
    {
        private readonly IFieldOrchestrator _fieldOrchestrator;
        private readonly IProcessService _processService;
        private readonly ILogger<GateReviewController> _logger;

        public GateReviewController(
            IFieldOrchestrator fieldOrchestrator,
            IProcessService processService,
            ILogger<GateReviewController> logger)
        {
            _fieldOrchestrator = fieldOrchestrator ?? throw new ArgumentNullException(nameof(fieldOrchestrator));
            _processService = processService ?? throw new ArgumentNullException(nameof(processService));
            _logger = logger;
        }

        /// <summary>Submit an entity for gate review, starting a new gate process instance.</summary>
        [HttpPost("submit/{gateId}")]
        [ProducesResponseType(typeof(ProcessInstance), 201)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<ProcessInstance>> SubmitForGateReviewAsync(
            string gateId,
            [FromBody] GateReviewSubmitRequest request)
        {
            if (string.IsNullOrWhiteSpace(gateId))
                    return BadRequest(new { error = "Gate ID is required." });
                if (request == null)
                    return BadRequest(new { error = "Request body is required." });
                if (string.IsNullOrWhiteSpace(request.EntityId))
                    return BadRequest(new { error = "EntityId is required in the request body." });

            var fieldId = _fieldOrchestrator.CurrentFieldId ?? string.Empty;
            if (string.IsNullOrEmpty(fieldId))
                    return BadRequest(new { error = "No active field selected." });

            var userId = User.ActingUserId();

            // Determine the gate process definition from gateId
            var gateProcessId = $"GATE_{gateId.ToUpperInvariant()}";
            var instance = await _processService.StartProcessAsync(
                gateProcessId,
                request.EntityId,
                "GATE_REVIEW",
                fieldId,
                userId);

            // Record submission in process history
            await _processService.AddHistoryEntryAsync(instance.InstanceId, new ProcessHistoryEntry
            {
                HistoryId = Guid.NewGuid().ToString(),
                InstanceId = instance.InstanceId,
                Action = "GATE_SUBMITTED",
                Notes = $"Entity: {request.EntityId} ({request.EntityType ?? "UNKNOWN"}) — {request.Comments}",
                PerformedBy = userId,
                Timestamp = DateTime.UtcNow
            });

            return CreatedAtAction(nameof(GetGateAsync), new { instanceId = instance.InstanceId }, instance);
        }

        /// <summary>Approve a gate — requires GateApprover role.</summary>
        [HttpPost("{instanceId}/approve")]
        [Authorize(Roles = "GateApprover")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> ApproveGateAsync(
            string instanceId,
            [FromBody] GateReviewDecisionRequest request)
        {
            if (string.IsNullOrWhiteSpace(instanceId))
                    return BadRequest(new { error = "Instance ID is required." });
            if (request == null)
                    return BadRequest(new { error = "Request body is required." });
            if (request.Decision != GateDecision.Approve)
                    return BadRequest(new { error = "Decision must be 'Approve' for this endpoint." });

            var userId = User.ActingUserId();

            var instance = await _processService.GetProcessInstanceAsync(instanceId);
            if (instance == null)
                return NotFound(new { error = $"Gate instance '{instanceId}' not found." });

            var success = await _processService.CompleteStepAsync(instanceId, instance.CurrentStepId ?? string.Empty, "APPROVED", userId);
            if (!success)
                return NotFound(new { error = $"Gate instance '{instanceId}' not found or not in a state that can be approved." });

            await _processService.AddHistoryEntryAsync(instanceId, new ProcessHistoryEntry
            {
                HistoryId = Guid.NewGuid().ToString(),
                InstanceId = instanceId,
                Action = "GATE_APPROVED",
                Notes = request.Comments,
                PerformedBy = userId,
                Timestamp = DateTime.UtcNow
            });

            return NoContent();
        }

        /// <summary>Reject a gate — requires GateApprover role.</summary>
        [HttpPost("{instanceId}/reject")]
        [Authorize(Roles = "GateApprover")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> RejectGateAsync(
            string instanceId,
            [FromBody] GateReviewDecisionRequest request)
        {
            if (string.IsNullOrWhiteSpace(instanceId))
                    return BadRequest(new { error = "Instance ID is required." });
                if (request == null)
                    return BadRequest(new { error = "Request body is required." });
                if (string.IsNullOrWhiteSpace(request.Comments))
                    return BadRequest(new { error = "A rejection reason (Comments) is required." });
            if (request.Decision != GateDecision.Reject)
                    return BadRequest(new { error = "Decision must be 'Reject' for this endpoint." });

            var userId = User.ActingUserId();

            var instance = await _processService.GetProcessInstanceAsync(instanceId);
            if (instance == null)
                return NotFound(new { error = $"Gate instance '{instanceId}' not found." });

            var success = await _processService.CompleteStepAsync(instanceId, instance.CurrentStepId ?? string.Empty, "REJECTED", userId);
            if (!success)
                return NotFound(new { error = $"Gate instance '{instanceId}' not found or not in a state that can be rejected." });

            await _processService.AddHistoryEntryAsync(instanceId, new ProcessHistoryEntry
            {
                HistoryId = Guid.NewGuid().ToString(),
                InstanceId = instanceId,
                Action = "GATE_REJECTED",
                Notes = request.Comments,
                PerformedBy = userId,
                Timestamp = DateTime.UtcNow
            });

            return NoContent();
        }

        /// <summary>Defer a gate with a target date — requires GateApprover role.</summary>
        [HttpPost("{instanceId}/defer")]
        [Authorize(Roles = "GateApprover")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeferGateAsync(
            string instanceId,
            [FromBody] GateReviewDecisionRequest request)
        {
            if (string.IsNullOrWhiteSpace(instanceId))
                    return BadRequest(new { error = "Instance ID is required." });
            if (request == null)
                    return BadRequest(new { error = "Request body is required." });
            if (request.Decision != GateDecision.Defer)
                    return BadRequest(new { error = "Decision must be 'Defer' for this endpoint." });

            var userId = User.ActingUserId();
            var deferNote = request.DeferTargetDate.HasValue
                ? $"Deferred to {request.DeferTargetDate:yyyy-MM-dd}. {request.Comments}"
                : request.Comments;

            var success = await _processService.TransitionStateAsync(instanceId, "DEFERRED", userId);
            if (!success)
                return NotFound(new { error = $"Gate instance '{instanceId}' not found or cannot be deferred." });

            // Record deferral in history
            var instance = await _processService.GetProcessInstanceAsync(instanceId);
            if (instance != null)
            {
                await _processService.AddHistoryEntryAsync(instance.InstanceId, new ProcessHistoryEntry
                {
                    HistoryId = Guid.NewGuid().ToString(),
                    InstanceId = instance.InstanceId,
                    Action = "GATE_DEFERRED",
                    Notes = deferNote,
                    PerformedBy = userId,
                    Timestamp = DateTime.UtcNow
                });
            }

            return NoContent();
        }

        /// <summary>List all gate review process instances for the current field.</summary>
        [HttpGet]
        [ProducesResponseType(typeof(List<ProcessInstanceSummary>), 200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<List<ProcessInstanceSummary>>> GetGatesAsync(
            [FromQuery] string? status = null)
        {
            var fieldId = _fieldOrchestrator.CurrentFieldId ?? string.Empty;
            if (string.IsNullOrEmpty(fieldId))
                    return BadRequest(new { error = "No active field selected." });

            var instances = await FieldGateReviewsAsync(fieldId);
            var summaries = new List<ProcessInstanceSummary>();
            if (instances != null)
            {
                foreach (var inst in instances)
                {
                    var s = new ProcessInstanceSummary
                    {
                        InstanceId     = inst.InstanceId,
                        ProcessId      = inst.ProcessId,
                        ProcessType    = inst.EntityType,
                        EntityId       = inst.EntityId,
                        EntityType     = inst.EntityType,
                        CurrentStepId  = inst.CurrentStepId,
                        Status         = inst.Status.ToString(),
                        StartedAt      = inst.StartDate,
                        StartedBy      = inst.StartedBy
                    };
                    if (!string.IsNullOrWhiteSpace(status) &&
                        !string.Equals(s.Status, status, StringComparison.OrdinalIgnoreCase))
                        continue;
                    summaries.Add(s);
                }
            }
            return Ok(summaries);
        }

        /// <summary>Get a specific gate review process instance by ID.</summary>
        [HttpGet("{instanceId}")]
        [ProducesResponseType(typeof(ProcessInstanceSummary), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<ProcessInstanceSummary>> GetGateAsync(string instanceId)
        {
            if (string.IsNullOrWhiteSpace(instanceId))
                    return BadRequest(new { error = "Instance ID is required." });

            var instance = await _processService.GetProcessInstanceAsync(instanceId);
            if (instance == null)
                return NotFound(new { error = $"Gate review '{instanceId}' not found." });

            var summary = new ProcessInstanceSummary
            {
                InstanceId    = instance.InstanceId,
                ProcessId     = instance.ProcessId,
                ProcessType   = instance.EntityType,
                EntityId      = instance.EntityId,
                EntityType    = instance.EntityType,
                CurrentStepId = instance.CurrentStepId,
                Status        = instance.Status.ToString(),
                StartedAt     = instance.StartDate,
                StartedBy     = instance.StartedBy,
                CompletedAt   = instance.CompletionDate
            };
            return Ok(summary);
        }

        /// <summary>List gate reviews that are pending approval for the current field.</summary>
        [HttpGet("pending")]
        [ProducesResponseType(typeof(List<ProcessInstanceSummary>), 200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<List<ProcessInstanceSummary>>> GetPendingGatesAsync()
        {
            var fieldId = _fieldOrchestrator.CurrentFieldId ?? string.Empty;
            if (string.IsNullOrEmpty(fieldId))
                    return BadRequest(new { error = "No active field selected." });

            var instances = await FieldGateReviewsAsync(fieldId);
            var pending   = new List<ProcessInstanceSummary>();
            if (instances != null)
            {
                foreach (var inst in instances)
                {
                    var statusStr = inst.Status.ToString();
                    if (!string.Equals(statusStr, "IN_PROGRESS", StringComparison.OrdinalIgnoreCase) &&
                        !string.Equals(statusStr, "SUBMITTED",   StringComparison.OrdinalIgnoreCase) &&
                        !string.Equals(statusStr, "NOT_STARTED", StringComparison.OrdinalIgnoreCase))
                        continue;

                    pending.Add(new ProcessInstanceSummary
                    {
                        InstanceId    = inst.InstanceId,
                        ProcessId     = inst.ProcessId,
                        ProcessType   = inst.EntityType,
                        EntityId      = inst.EntityId,
                        EntityType    = inst.EntityType,
                        CurrentStepId = inst.CurrentStepId,
                        Status        = statusStr,
                        StartedAt     = inst.StartDate,
                        StartedBy     = inst.StartedBy
                    });
                }
            }
            return Ok(pending);
        }

        /// <summary>
        /// The current field's gate reviews, whatever entity each reviews: an instance belongs to the field it was started
        /// in (PROCESS_INSTANCE.FIELD_ID), and a gate review is an instance of a GATE_REVIEW process definition. They had
        /// been read as instances whose entity was the field itself, so a gate submitted for a well (WELL_DESIGN for a
        /// UWI) or the FDP gate never appeared in these lists.
        /// </summary>
        private async Task<List<ProcessInstance>> FieldGateReviewsAsync(string fieldId)
        {
            var gateDefinitions = await _processService.GetProcessDefinitionsByTypeAsync(GateReviewProcessType);
            var gateProcessIds = gateDefinitions.Select(d => d.ProcessId).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var instances = await _processService.GetProcessInstancesForFieldAsync(fieldId);
            return instances.Where(i => gateProcessIds.Contains(i.ProcessId)).ToList();
        }

        private const string GateReviewProcessType = "GATE_REVIEW";

        /// <summary>Retrieve the required document checklist for a gate.</summary>
        [HttpGet("{instanceId}/checklist")]
        [ProducesResponseType(typeof(GateChecklistResponse), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<GateChecklistResponse>> GetChecklistAsync(string instanceId)
        {
            if (string.IsNullOrWhiteSpace(instanceId))
                    return BadRequest(new { error = "Instance ID is required." });

            var instance = await _processService.GetProcessInstanceAsync(instanceId);
            if (instance == null)
                return NotFound(new { error = $"Gate review '{instanceId}' not found." });

            if (string.IsNullOrWhiteSpace(instance.ProcessId))
                return NotFound(new { error = $"Gate review '{instanceId}' does not have a process definition." });

            var definition = await _processService.GetProcessDefinitionAsync(instance.ProcessId);
            if (definition == null)
                return NotFound(new { error = $"Gate definition '{instance.ProcessId}' not found." });

            var checklist = new GateChecklistResponse { GateId = instanceId };
            if (definition.Configuration != null &&
                definition.Configuration.TryGetValue("RequiredDocuments", out var docs) &&
                docs != null)
            {
                if (docs is System.Text.Json.JsonElement je && je.ValueKind == System.Text.Json.JsonValueKind.Array)
                {
                    foreach (var item in je.EnumerateArray())
                        checklist.RequiredDocuments.Add(item.GetString() ?? string.Empty);
                }
                else if (docs is string str)
                {
                    foreach (var item in str.Split(',', StringSplitOptions.RemoveEmptyEntries))
                        checklist.RequiredDocuments.Add(item.Trim());
                }
            }

            return Ok(checklist);
        }
    }
}
