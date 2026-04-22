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
    /// Business process lifecycle endpoints — definitions, instances, transitions, and history.
    /// All operations are scoped to the current active field via IFieldOrchestrator.
    /// </summary>
    [ApiController]
    [Route("api/field/current/process")]
    [Authorize]
    public class BusinessProcessController : ControllerBase
    {
        private readonly IFieldOrchestrator _fieldOrchestrator;
        private readonly IProcessService _processService;
        private readonly ILogger<BusinessProcessController> _logger;

        public BusinessProcessController(
            IFieldOrchestrator fieldOrchestrator,
            IProcessService processService,
            ILogger<BusinessProcessController> logger)
        {
            _fieldOrchestrator = fieldOrchestrator ?? throw new ArgumentNullException(nameof(fieldOrchestrator));
            _processService = processService ?? throw new ArgumentNullException(nameof(processService));
            _logger = logger;
        }

        // ─── Process Definitions ────────────────────────────────────────────────

        /// <summary>List all process definitions available for the current field.</summary>
        [HttpGet("definitions")]
        [ProducesResponseType(typeof(List<ProcessDefinition>), 200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<List<ProcessDefinition>>> GetDefinitionsAsync()
        {
            var fieldId = _fieldOrchestrator.CurrentFieldId ?? string.Empty;
            if (string.IsNullOrEmpty(fieldId))
                    return BadRequest(new { error = "No active field selected." });

            try
            {
                // Return all definitions — type-based lookup covers all categories
                var allDefinitions = new List<ProcessDefinition>();
                var categories = new[]
                {
                    "WORK_ORDER", "GATE_REVIEW", "HSE", "COMPLIANCE",
                    "WELL_LIFECYCLE", "FACILITY_LIFECYCLE", "RESERVOIR", "PIPELINE"
                };
                foreach (var cat in categories)
                {
                    var defs = await _processService.GetProcessDefinitionsByTypeAsync(cat);
                    if (defs != null) allDefinitions.AddRange(defs);
                }
                return Ok(allDefinitions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving process definitions for field {FieldId}", fieldId);
                return StatusCode(500, new { error = "Error retrieving process definitions." });
            }
        }

        /// <summary>Get a single process definition by ID.</summary>
        [HttpGet("definitions/{processId}")]
        [ProducesResponseType(typeof(ProcessDefinition), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<ProcessDefinition>> GetDefinitionAsync(string processId)
        {
            if (string.IsNullOrWhiteSpace(processId))
                    return BadRequest(new { error = "Process ID is required." });

            try
            {
                var def = await _processService.GetProcessDefinitionAsync(processId);
                if (def == null)
                    return NotFound(new { error = $"Process definition '{processId}' not found." });
                return Ok(def);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving process definition {ProcessId}", processId);
                return StatusCode(500, new { error = "Error retrieving process definition." });
            }
        }

        /// <summary>List process definitions filtered by category/type.</summary>
        [HttpGet("definitions/category/{categoryId}")]
        [ProducesResponseType(typeof(List<ProcessDefinition>), 200)]
        public async Task<ActionResult<List<ProcessDefinition>>> GetDefinitionsByCategoryAsync(string categoryId)
        {
            if (string.IsNullOrWhiteSpace(categoryId))
                    return BadRequest(new { error = "Category ID is required." });

            try
            {
                var defs = await _processService.GetProcessDefinitionsByTypeAsync(categoryId.ToUpperInvariant());
                return Ok(defs ?? new List<ProcessDefinition>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving definitions for category {Category}", categoryId);
                return StatusCode(500, new { error = "Error retrieving process definitions." });
            }
        }

        /// <summary>List process definitions filtered by jurisdiction tag (USA, CANADA, INTERNATIONAL).</summary>
        [HttpGet("definitions/jurisdiction/{tag}")]
        [ProducesResponseType(typeof(List<ProcessDefinition>), 200)]
        public async Task<ActionResult<List<ProcessDefinition>>> GetDefinitionsByJurisdictionAsync(string tag)
        {
            if (string.IsNullOrWhiteSpace(tag))
                    return BadRequest(new { error = "Jurisdiction tag is required." });

            try
            {
                // Fetch all then filter by configuration/metadata tag
                var allDefinitions = new List<ProcessDefinition>();
                var categories = new[]
                {
                    "WORK_ORDER", "GATE_REVIEW", "HSE", "COMPLIANCE",
                    "WELL_LIFECYCLE", "FACILITY_LIFECYCLE", "RESERVOIR", "PIPELINE"
                };
                foreach (var cat in categories)
                {
                    var defs = await _processService.GetProcessDefinitionsByTypeAsync(cat);
                    if (defs != null) allDefinitions.AddRange(defs);
                }
                var filtered = allDefinitions.FindAll(d =>
                    d.Configuration != null &&
                    d.Configuration.TryGetValue("JurisdictionTag", out var jTag) &&
                    string.Equals(jTag?.ToString(), tag, StringComparison.OrdinalIgnoreCase));
                return Ok(filtered);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving definitions for jurisdiction {Tag}", tag);
                return StatusCode(500, new { error = "Error retrieving process definitions." });
            }
        }

        // ─── Process Instances ───────────────────────────────────────────────────

        /// <summary>Start a new process instance for an entity in the current field.</summary>
        [HttpPost("instances")]
        [ProducesResponseType(typeof(ProcessInstance), 201)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<ProcessInstance>> StartInstanceAsync(
            [FromBody] ProcessInstanceRequest request)
        {
            if (request == null)
                 return BadRequest(new { error = "Request body is required." });
            if (string.IsNullOrWhiteSpace(request.ProcessId))
                 return BadRequest(new { error = "ProcessId is required." });
            if (string.IsNullOrWhiteSpace(request.EntityId))
                 return BadRequest(new { error = "EntityId is required." });

            var fieldId = _fieldOrchestrator.CurrentFieldId ?? string.Empty;
            if (string.IsNullOrEmpty(fieldId))
                 return BadRequest(new { error = "No active field selected." });

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "system";

            try
            {
                var instance = await _processService.StartProcessAsync(
                    request.ProcessId,
                    request.EntityId,
                    request.EntityType ?? "UNKNOWN",
                    fieldId,
                    userId);
                return CreatedAtAction(nameof(GetInstanceAsync), new { instanceId = instance.InstanceId }, instance);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting process {ProcessId} for entity {EntityId}", request.ProcessId, request.EntityId);
                return StatusCode(500, new { error = "Error starting process instance." });
            }
        }

        /// <summary>List all active process instances for the current field.</summary>
        [HttpGet("instances")]
        [ProducesResponseType(typeof(List<ProcessInstanceSummary>), 200)]
        public async Task<ActionResult<List<ProcessInstanceSummary>>> GetInstancesAsync()
        {
            var fieldId = _fieldOrchestrator.CurrentFieldId ?? string.Empty;
            if (string.IsNullOrEmpty(fieldId))
                    return BadRequest(new { error = "No active field selected." });

            try
            {
                // Return instances for the field entity itself
                var instances = await _processService.GetProcessInstancesForEntityAsync(fieldId, "FIELD");
                var summaries = new List<ProcessInstanceSummary>();
                if (instances != null)
                {
                    foreach (var inst in instances)
                    {
                        summaries.Add(new ProcessInstanceSummary
                        {
                            InstanceId = inst.InstanceId,
                            ProcessId = inst.ProcessId,
                            EntityId = inst.EntityId,
                            EntityType = inst.EntityType,
                            CurrentStepId = inst.CurrentStepId,
                            Status = inst.Status.ToString(),
                            StartedAt = inst.StartDate
                        });
                    }
                }
                return Ok(summaries);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listing instances for field {FieldId}", fieldId);
                return StatusCode(500, new { error = "Error retrieving process instances." });
            }
        }

        /// <summary>Get a specific process instance with its current step state.</summary>
        [HttpGet("instances/{instanceId}")]
        [ProducesResponseType(typeof(ProcessInstance), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<ProcessInstance>> GetInstanceAsync(string instanceId)
        {
            if (string.IsNullOrWhiteSpace(instanceId))
                    return BadRequest(new { error = "Instance ID is required." });

            try
            {
                var instance = await _processService.GetProcessInstanceAsync(instanceId);
                if (instance == null)
                    return NotFound(new { error = $"Process instance '{instanceId}' not found." });
                return Ok(instance);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving process instance {InstanceId}", instanceId);
                return StatusCode(500, new { error = "Error retrieving process instance." });
            }
        }

        /// <summary>Execute a state transition on a process instance.</summary>
        [HttpPost("instances/{instanceId}/transitions")]
        [ProducesResponseType(typeof(ProcessTransitionResult), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(422)]
        public async Task<ActionResult<ProcessTransitionResult>> ExecuteTransitionAsync(
            string instanceId,
            [FromBody] ProcessTransitionRequest request)
        {
            if (string.IsNullOrWhiteSpace(instanceId))
                    return BadRequest(new { error = "Instance ID is required." });
                if (request == null)
                    return BadRequest(new { error = "Request body is required." });
                if (string.IsNullOrWhiteSpace(request.Trigger))
                    return BadRequest(new { error = "Transition trigger is required." });

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "system";

            try
            {
                var canTransition = await _processService.CanTransitionAsync(instanceId, request.ToStateId);
                if (!canTransition)
                    return UnprocessableEntity(new { error = $"Transition to '{request.ToStateId}' is not allowed from current state." });

                var success = await _processService.TransitionStateAsync(instanceId, request.ToStateId, userId);
                var result = new ProcessTransitionResult
                {
                    Success = success,
                    InstanceId = instanceId,
                    TransitionName = request.Trigger,
                    FromState = request.FromStateId,
                    NewStepId = request.ToStateId,
                    Message = success ? "Transition completed successfully." : "Transition failed.",
                    TransitionedAt = DateTime.UtcNow
                };
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing transition on instance {InstanceId}", instanceId);
                return StatusCode(500, new { error = "Error executing state transition." });
            }
        }

        /// <summary>Get the full audit history for a process instance.</summary>
        [HttpGet("instances/{instanceId}/history")]
        [ProducesResponseType(typeof(List<ProcessHistoryEntry>), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<List<ProcessHistoryEntry>>> GetHistoryAsync(string instanceId)
        {
            if (string.IsNullOrWhiteSpace(instanceId))
                    return BadRequest(new { error = "Instance ID is required." });

            try
            {
                var history = await _processService.GetProcessHistoryAsync(instanceId);
                if (history == null)
                    return NotFound(new { error = $"Process instance '{instanceId}' not found." });
                return Ok(history);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving history for instance {InstanceId}", instanceId);
                return StatusCode(500, new { error = "Error retrieving process history." });
            }
        }

        /// <summary>Update step data or attach documents to a step.</summary>
        [HttpPatch("instances/{instanceId}/steps/{stepId}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> UpdateStepAsync(
            string instanceId,
            string stepId,
            [FromBody] Beep.OilandGas.Models.Data.Process.PROCESS_STEP_DATA stepData)
        {
                if (string.IsNullOrWhiteSpace(instanceId))
                    return BadRequest(new { error = "Instance ID is required." });
                if (string.IsNullOrWhiteSpace(stepId))
                    return BadRequest(new { error = "Step ID is required." });

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "system";

            try
            {
                var success = await _processService.ExecuteStepAsync(instanceId, stepId, stepData, userId);
                if (!success)
                    return BadRequest(new { error = "Step update failed. Verify the instance and step are in a valid state." });
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating step {StepId} on instance {InstanceId}", stepId, instanceId);
                return StatusCode(500, new { error = "Error updating step." });
            }
        }

        /// <summary>Close or cancel a process instance.</summary>
        [HttpPost("instances/{instanceId}/close")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> CloseInstanceAsync(
            string instanceId,
            [FromBody] ProcessCloseRequest request)
        {
            if (string.IsNullOrWhiteSpace(instanceId))
                    return BadRequest(new { error = "Instance ID is required." });

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "system";
            var reason = request?.Reason ?? "Closed by user.";

            try
            {
                var success = await _processService.CancelProcessAsync(instanceId, reason, userId);
                if (!success)
                    return BadRequest(new { error = "Unable to close instance. It may already be closed or completed." });
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error closing process instance {InstanceId}", instanceId);
                return StatusCode(500, new { error = "Error closing process instance." });
            }
        }

        /// <summary>List all available seed process templates.</summary>
        [HttpGet("templates")]
        [ProducesResponseType(typeof(List<ProcessDefinition>), 200)]
        public async Task<ActionResult<List<ProcessDefinition>>> GetTemplatesAsync()
        {
            try
            {
                var templates = await _processService.GetProcessDefinitionsByTypeAsync("TEMPLATE");
                return Ok(templates ?? new List<ProcessDefinition>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving process templates");
                return StatusCode(500, new { error = "Error retrieving templates." });
            }
        }
    }

    /// <summary>Minimal close/cancel request body.</summary>
    public class ProcessCloseRequest
    {
        public string Reason { get; set; } = string.Empty;
    }
}
