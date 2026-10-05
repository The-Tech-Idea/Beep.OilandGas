using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
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
    /// Business process lifecycle endpoints — definitions, instances, transitions, and history.
    /// All operations are scoped to the current active field via IFieldOrchestrator.
    /// </summary>
    [ApiController]
    [Route("api/field/current/process")]
    [Authorize]
    [RequireCurrentFieldAccess]
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

        /// <summary>Get a single process definition by ID.</summary>
        [HttpGet("definitions/{processId}")]
        [ProducesResponseType(typeof(ProcessDefinition), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<ProcessDefinition>> GetDefinitionAsync(string processId)
        {
            if (string.IsNullOrWhiteSpace(processId))
                    return BadRequest(new { error = "Process ID is required." });

            var def = await _processService.GetProcessDefinitionAsync(processId);
            if (def == null)
                return NotFound(new { error = $"Process definition '{processId}' not found." });
            return Ok(def);
        }

        /// <summary>List process definitions filtered by category/type.</summary>
        [HttpGet("definitions/category/{categoryId}")]
        [ProducesResponseType(typeof(List<ProcessDefinition>), 200)]
        public async Task<ActionResult<List<ProcessDefinition>>> GetDefinitionsByCategoryAsync(string categoryId)
        {
            if (string.IsNullOrWhiteSpace(categoryId))
                    return BadRequest(new { error = "Category ID is required." });

            var defs = await _processService.GetProcessDefinitionsByTypeAsync(categoryId.ToUpperInvariant());
            return Ok(defs ?? new List<ProcessDefinition>());
        }

        /// <summary>List process definitions filtered by jurisdiction tag (USA, CANADA, INTERNATIONAL).</summary>
        [HttpGet("definitions/jurisdiction/{tag}")]
        [ProducesResponseType(typeof(List<ProcessDefinition>), 200)]
        public async Task<ActionResult<List<ProcessDefinition>>> GetDefinitionsByJurisdictionAsync(string tag)
        {
            if (string.IsNullOrWhiteSpace(tag))
                    return BadRequest(new { error = "Jurisdiction tag is required." });

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

            var userId = User.ActingUserId();

            var instance = await _processService.StartProcessAsync(
                request.ProcessId,
                request.EntityId,
                request.EntityType ?? "UNKNOWN",
                fieldId,
                userId);
            return CreatedAtAction(nameof(GetInstanceAsync), new { instanceId = instance.InstanceId }, instance);
        }

        /// <summary>List all active process instances for the current field.</summary>
        [HttpGet("instances")]
        [ProducesResponseType(typeof(List<ProcessInstanceSummary>), 200)]
        public async Task<ActionResult<List<ProcessInstanceSummary>>> GetInstancesAsync()
        {
            var fieldId = _fieldOrchestrator.CurrentFieldId ?? string.Empty;
            if (string.IsNullOrEmpty(fieldId))
                    return BadRequest(new { error = "No active field selected." });

            var summaries = new List<ProcessInstanceSummary>();
            var processService = await GetOperationServiceAsync();
                var instances = await processService.GetProcessInstancesForFieldAsync(fieldId);
                if (instances != null)
                {
                    foreach (var inst in instances.Where(IsCurrentField))
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

        /// <summary>Get a specific process instance with its current step state.</summary>
        [HttpGet("instances/{instanceId}")]
        [ProducesResponseType(typeof(ProcessInstance), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<ProcessInstance>> GetInstanceAsync(string instanceId)
        {
            if (string.IsNullOrWhiteSpace(instanceId))
                    return BadRequest(new { error = "Instance ID is required." });

            var instance = await _processService.GetProcessInstanceAsync(instanceId);
            if (instance == null)
                return NotFound(new { error = $"Process instance '{instanceId}' not found." });
            if (!IsCurrentField(instance)) return Forbid();
            return Ok(instance);
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

            var userId = User.ActingUserId();

            var processService = await GetOperationServiceAsync();
            var instance = await processService.GetProcessInstanceAsync(instanceId);
            if (instance is null) return NotFound();
            if (!await CanActOnStepAsync(processService, instance, instance.CurrentStepId, userId)) return Forbid();
            var actualFromState = instance.CurrentState;

            var canTransition = await processService.CanTransitionAsync(instanceId, request.ToStateId);
            if (!canTransition)
                return UnprocessableEntity(new { error = $"Transition to '{request.ToStateId}' is not allowed from current state." });

            var success = await processService.TransitionStateAsync(instanceId, request.ToStateId, userId);
            var result = new ProcessTransitionResult
            {
                Success = success,
                InstanceId = instanceId,
                TransitionName = request.Trigger,
                FromState = actualFromState,
                NewStepId = request.ToStateId,
                Message = success ? "Transition completed successfully." : "Transition failed.",
                TransitionedAt = DateTime.UtcNow
            };
            return Ok(result);
        }

        /// <summary>Get the full audit history for a process instance.</summary>
        [HttpGet("instances/{instanceId}/history")]
        [ProducesResponseType(typeof(List<ProcessHistoryEntry>), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<List<ProcessHistoryEntry>>> GetHistoryAsync(string instanceId)
        {
            if (string.IsNullOrWhiteSpace(instanceId))
                    return BadRequest(new { error = "Instance ID is required." });

            var processService = await GetOperationServiceAsync();
            var instance = await processService.GetProcessInstanceAsync(instanceId);
            if (instance is null) return NotFound();
            if (!IsCurrentField(instance)) return Forbid();
            var history = await processService.GetProcessHistoryAsync(instanceId);
            return Ok(history ?? new List<ProcessHistoryEntry>());
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

            var userId = User.ActingUserId();

            var processService = await GetOperationServiceAsync();
            var instance = await processService.GetProcessInstanceAsync(instanceId);
            if (instance is null) return NotFound();
            if (!await CanActOnStepAsync(processService, instance, stepId, userId)) return Forbid();
            var success = await processService.ExecuteStepAsync(instanceId, stepId, stepData, userId);
            if (!success)
                return BadRequest(new { error = "Step update failed. Verify the instance and step are in a valid state." });
            return NoContent();
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

            var userId = User.ActingUserId();
            var reason = request?.Reason ?? "Closed by user.";

            var processService = await GetOperationServiceAsync();
            var instance = await processService.GetProcessInstanceAsync(instanceId);
            if (instance is null) return NotFound();
            if (!IsCurrentField(instance) || (instance.StartedBy != userId && !User.IsInRole("Administrator"))) return Forbid();
            var success = await processService.CancelProcessAsync(instanceId, reason, userId);
            if (!success)
                return BadRequest(new { error = "Unable to close instance. It may already be closed or completed." });
            return NoContent();
        }

        private Task<IProcessService> GetOperationServiceAsync() => _processService is BoundProcessService bound
            ? bound.CreateBoundServiceAsync() : Task.FromResult(_processService);

        private bool IsCurrentField(ProcessInstance instance) =>
            !string.IsNullOrWhiteSpace(_fieldOrchestrator.CurrentFieldId) &&
            string.Equals(instance.FieldId, _fieldOrchestrator.CurrentFieldId, StringComparison.Ordinal);

        private async Task<bool> CanActOnStepAsync(IProcessService service, ProcessInstance instance, string stepId, string userId)
        {
            if (!IsCurrentField(instance)) return false;
            var step = instance.StepInstances?.SingleOrDefault(x => x.StepId == stepId);
            if (step is null || instance.CurrentStepId != stepId) return false;
            if (User.IsInRole("Administrator")) return true;
            var assigned = !string.IsNullOrWhiteSpace(step.AssignedTo);
            if (assigned && step.AssignedTo != userId && !User.IsInRole(step.AssignedTo!)) return false;
            var definition = await service.GetProcessDefinitionAsync(instance.ProcessId);
            var definitionStep = definition?.Steps?.SingleOrDefault(x => x.StepId == stepId);
            if (definitionStep is null) return false;
            var definitionRoles = definitionStep.RequiredRoles ?? new List<string>();
            if (!string.IsNullOrWhiteSpace(step.RequiredRole) && !User.IsInRole(step.RequiredRole)) return false;
            if (definitionRoles.Count > 0 && !definitionRoles.Any(User.IsInRole)) return false;
            return assigned || !string.IsNullOrWhiteSpace(step.RequiredRole) || definitionRoles.Count > 0;
        }

        /// <summary>List all available seed process templates.</summary>
        [HttpGet("templates")]
        [ProducesResponseType(typeof(List<ProcessDefinition>), 200)]
        public async Task<ActionResult<List<ProcessDefinition>>> GetTemplatesAsync()
        {
            var templates = await _processService.GetProcessDefinitionsByTypeAsync("TEMPLATE");
            return Ok(templates ?? new List<ProcessDefinition>());
        }
    }

    /// <summary>Minimal close/cancel request body.</summary>
    public class ProcessCloseRequest
    {
        public string Reason { get; set; } = string.Empty;
    }
}
