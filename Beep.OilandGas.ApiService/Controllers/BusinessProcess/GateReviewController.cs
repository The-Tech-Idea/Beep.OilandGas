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
    /// Gate review workflow endpoints — submit, approve, reject, defer, and checklist retrieval.
    /// Approve/reject/defer require the GateApprover role.
    /// </summary>
    [ApiController]
    [Route("api/field/current/gates")]
    [Authorize]
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
                return BadRequest("gateId is required.");
            if (request == null || string.IsNullOrWhiteSpace(request.EntityId))
                return BadRequest("EntityId is required in the request body.");

            var fieldId = _fieldOrchestrator.CurrentFieldId;
            if (string.IsNullOrEmpty(fieldId))
                return BadRequest("No active field selected.");

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
                return StatusCode(500, "Error submitting gate review.");
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
                return BadRequest("gateId is required.");
            if (request == null)
                return BadRequest("Request body is required.");
            if (request.Decision != GateDecision.Approve)
                return BadRequest("Decision must be 'Approve' for this endpoint.");

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "system";

            try
            {
                var success = await _processService.ApproveStepAsync(gateId, userId, request.Comments, userId);
                if (!success)
                    return NotFound($"Gate '{gateId}' not found or not in a state that can be approved.");
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving gate {GateId}", gateId);
                return StatusCode(500, "Error approving gate.");
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
                return BadRequest("gateId is required.");
            if (request == null || string.IsNullOrWhiteSpace(request.Comments))
                return BadRequest("A rejection reason (Comments) is required.");
            if (request.Decision != GateDecision.Reject)
                return BadRequest("Decision must be 'Reject' for this endpoint.");

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "system";

            try
            {
                var success = await _processService.RejectStepAsync(gateId, userId, request.Comments, userId);
                if (!success)
                    return NotFound($"Gate '{gateId}' not found or not in a state that can be rejected.");
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting gate {GateId}", gateId);
                return StatusCode(500, "Error rejecting gate.");
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
                return BadRequest("gateId is required.");
            if (request == null)
                return BadRequest("Request body is required.");
            if (request.Decision != GateDecision.Defer)
                return BadRequest("Decision must be 'Defer' for this endpoint.");

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "system";
            var deferNote = request.DeferTargetDate.HasValue
                ? $"Deferred to {request.DeferTargetDate:yyyy-MM-dd}. {request.Comments}"
                : request.Comments;

            try
            {
                // Transition to a DEFERRED state via the process service
                var success = await _processService.TransitionStateAsync(gateId, "DEFERRED", userId);
                if (!success)
                    return NotFound($"Gate '{gateId}' not found or cannot be deferred.");

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
                return StatusCode(500, "Error deferring gate.");
            }
        }

        /// <summary>Retrieve the required document checklist for a gate.</summary>
        [HttpGet("{gateId}/checklist")]
        [ProducesResponseType(typeof(GateChecklistResponse), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<GateChecklistResponse>> GetChecklistAsync(string gateId)
        {
            if (string.IsNullOrWhiteSpace(gateId))
                return BadRequest("gateId is required.");

            try
            {
                var gateProcessId = $"GATE_{gateId.ToUpperInvariant()}";
                var definition = await _processService.GetProcessDefinitionAsync(gateProcessId);
                if (definition == null)
                    return NotFound($"Gate definition '{gateId}' not found.");

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
                return StatusCode(500, "Error retrieving gate checklist.");
            }
        }
    }

    public class GateChecklistResponse
    {
        public string GateId { get; set; } = string.Empty;
        public List<string> RequiredDocuments { get; set; } = new();
    }
}
