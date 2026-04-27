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
using Beep.OilandGas.Models.Processes;
using Beep.OilandGas.ApiService.Attributes;

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

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "system";

            try
            {
                // Determine the gate process definition from gateId
                var gateProcessId = $"GATE_{gateId.ToUpperInvariant()}";
                var instance = await _processService.StartProcessAsync(
                    gateProcessId,
                    request.EntityId,
                    request.EntityType ?? "UNKNOWN",
                    fieldId,
                    userId);

                // Record submission in process history
                await _processService.AddHistoryEntryAsync(instance.InstanceId, new ProcessHistoryEntry
                {
                    HistoryId = Guid.NewGuid().ToString(),
                    InstanceId = instance.InstanceId,
                    Action = "GATE_SUBMITTED",
                    Notes = request.Comments,
                    PerformedBy = userId,
                    Timestamp = DateTime.UtcNow
                });

                return CreatedAtAction(null, new { gateId }, instance);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting gate review {GateId} for entity {EntityId}", gateId, request.EntityId);
                return StatusCode(500, new { error = "Error submitting gate review." });
            }
        }

        /// <summary>Approve a gate — requires GateApprover role.</summary>
        [HttpPost("{gateId}/approve")]
        [Authorize(Roles = "GateApprover")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> ApproveGateAsync(
            string gateId,
            [FromBody] GateReviewDecisionRequest request)
        {
            if (string.IsNullOrWhiteSpace(gateId))
                    return BadRequest(new { error = "Gate ID is required." });
            if (request == null)
                    return BadRequest(new { error = "Request body is required." });
            if (request.Decision != GateDecision.Approve)
                    return BadRequest(new { error = "Decision must be 'Approve' for this endpoint." });

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "system";

            try
            {
                var success = await _processService.ApproveStepAsync(gateId, userId, request.Comments, userId);
                if (!success)
                    return NotFound(new { error = $"Gate '{gateId}' not found or not in a state that can be approved." });
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving gate {GateId}", gateId);
                return StatusCode(500, new { error = "Error approving gate." });
            }
        }

        /// <summary>Reject a gate — requires GateApprover role.</summary>
        [HttpPost("{gateId}/reject")]
        [Authorize(Roles = "GateApprover")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> RejectGateAsync(
            string gateId,
            [FromBody] GateReviewDecisionRequest request)
        {
            if (string.IsNullOrWhiteSpace(gateId))
                    return BadRequest(new { error = "Gate ID is required." });
                if (request == null)
                    return BadRequest(new { error = "Request body is required." });
                if (string.IsNullOrWhiteSpace(request.Comments))
                    return BadRequest(new { error = "A rejection reason (Comments) is required." });
            if (request.Decision != GateDecision.Reject)
                    return BadRequest(new { error = "Decision must be 'Reject' for this endpoint." });

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "system";

            try
            {
                var success = await _processService.RejectStepAsync(gateId, userId, request.Comments, userId);
                if (!success)
                    return NotFound(new { error = $"Gate '{gateId}' not found or not in a state that can be rejected." });
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting gate {GateId}", gateId);
                return StatusCode(500, new { error = "Error rejecting gate." });
            }
        }

        /// <summary>Defer a gate with a target date — requires GateApprover role.</summary>
        [HttpPost("{gateId}/defer")]
        [Authorize(Roles = "GateApprover")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeferGateAsync(
            string gateId,
            [FromBody] GateReviewDecisionRequest request)
        {
            if (string.IsNullOrWhiteSpace(gateId))
                    return BadRequest(new { error = "Gate ID is required." });
            if (request == null)
                    return BadRequest(new { error = "Request body is required." });
            if (request.Decision != GateDecision.Defer)
                    return BadRequest(new { error = "Decision must be 'Defer' for this endpoint." });

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "system";
            var deferNote = request.DeferTargetDate.HasValue
                ? $"Deferred to {request.DeferTargetDate:yyyy-MM-dd}. {request.Comments}"
                : request.Comments;

            try
            {
                // Transition to a DEFERRED state via the process service
                var success = await _processService.TransitionStateAsync(gateId, "DEFERRED", userId);
                if (!success)
                    return NotFound(new { error = $"Gate '{gateId}' not found or cannot be deferred." });

                // Record deferral in history
                var instance = await _processService.GetProcessInstanceAsync(gateId);
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deferring gate {GateId}", gateId);
                return StatusCode(500, new { error = "Error deferring gate." });
            }
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

            try
            {
                var instances = await _processService.GetProcessInstancesForEntityAsync(fieldId, "GATE_REVIEW");
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listing gate reviews for field {FieldId}", fieldId);
                return StatusCode(500, new { error = "Error retrieving gate reviews." });
            }
        }

        /// <summary>Get a specific gate review process instance by ID.</summary>
        [HttpGet("{gateId}")]
        [ProducesResponseType(typeof(ProcessInstanceSummary), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<ProcessInstanceSummary>> GetGateAsync(string gateId)
        {
            if (string.IsNullOrWhiteSpace(gateId))
                    return BadRequest(new { error = "Gate ID is required." });

            try
            {
                var instance = await _processService.GetProcessInstanceAsync(gateId);
                if (instance == null)
                    return NotFound(new { error = $"Gate review '{gateId}' not found." });

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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving gate {GateId}", gateId);
                return StatusCode(500, new { error = "Error retrieving gate review." });
            }
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

            try
            {
                var instances = await _processService.GetProcessInstancesForEntityAsync(fieldId, "GATE_REVIEW");
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving pending gates for field {FieldId}", fieldId);
                return StatusCode(500, new { error = "Error retrieving pending gate reviews." });
            }
        }

        /// <summary>Retrieve the required document checklist for a gate.</summary>
        [HttpGet("{gateId}/checklist")]
        [ProducesResponseType(typeof(GateChecklistResponse), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<GateChecklistResponse>> GetChecklistAsync(string gateId)
        {
            if (string.IsNullOrWhiteSpace(gateId))
                    return BadRequest(new { error = "Gate ID is required." });

            try
            {
                var instance = await _processService.GetProcessInstanceAsync(gateId);
                if (instance == null)
                    return NotFound(new { error = $"Gate review '{gateId}' not found." });

                if (string.IsNullOrWhiteSpace(instance.ProcessId))
                    return NotFound(new { error = $"Gate review '{gateId}' does not have a process definition." });

                var definition = await _processService.GetProcessDefinitionAsync(instance.ProcessId);
                if (definition == null)
                    return NotFound(new { error = $"Gate definition '{instance.ProcessId}' not found." });

                // Extract checklist items from the gate process configuration
                var checklist = new GateChecklistResponse { GateId = gateId };
                if (definition.Configuration != null &&
                    definition.Configuration.TryGetValue("RequiredDocuments", out var docs) &&
                    docs != null)
                {
                    foreach (var item in docs.ToString()!.Split(',', StringSplitOptions.RemoveEmptyEntries))
                        checklist.RequiredDocuments.Add(item.Trim());
                }

                return Ok(checklist);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving checklist for gate {GateId}", gateId);
                return StatusCode(500, new { error = "Error retrieving gate checklist." });
            }
        }
    }
}
