using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.DataManagement;
using Beep.OilandGas.PPDM39.DataManagement.Services;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.ApiService.Controllers.PPDM39
{
    /// <summary>
    /// API controller for PPDM39 workflow pipeline operations
    /// Supports phase-specific workflows (Exploration, Development, Production, Decommissioning)
    /// </summary>
    [ApiController]
    [Route("api/ppdm39/workflow")]
    public class PPDM39WorkflowController : ControllerBase
    {
        private readonly PPDM39WorkflowService _workflowService;
        private readonly ILogger<PPDM39WorkflowController> _logger;
        private readonly IProgressTrackingService? _progressTracking;
        private readonly IFieldOrchestrator? _fieldOrchestrator;

        public PPDM39WorkflowController(
            PPDM39WorkflowService workflowService,
            ILogger<PPDM39WorkflowController> logger,
            IProgressTrackingService? progressTracking = null,
            IFieldOrchestrator? fieldOrchestrator = null)
        {
            _workflowService = workflowService ?? throw new ArgumentNullException(nameof(workflowService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _progressTracking = progressTracking;
            _fieldOrchestrator = fieldOrchestrator;
        }

        /// <summary>
        /// Execute a workflow pipeline
        /// Supports phase-specific workflows scoped to current field
        /// </summary>
        [HttpPost("execute")]
        public async Task<ActionResult<OperationStartResponse>> ExecuteWorkflow([FromBody] WorkflowExecutionRequest request, [FromQuery] string? phase = null)
        {
            try
            {
                if (request?.Workflow == null)
                {
                    return BadRequest(new OperationStartResponse { OperationId = "", Message = "Workflow definition is required" });
                }

                // Set field context if available and workflow doesn't already have field ID
                if (_fieldOrchestrator != null && !string.IsNullOrEmpty(_fieldOrchestrator.CurrentFieldId))
                {
                    // Add field context to workflow if not already set
                    if (request.Workflow.Parameters == null)
                    {
                        request.Workflow.Parameters = new Dictionary<string, object>();
                    }
                    if (!request.Workflow.Parameters.ContainsKey("FIELD_ID"))
                    {
                        request.Workflow.Parameters["FIELD_ID"] = _fieldOrchestrator.CurrentFieldId;
                    }

                    // Set phase context if specified
                    if (!string.IsNullOrEmpty(phase) && !request.Workflow.Parameters.ContainsKey("PHASE"))
                    {
                        request.Workflow.Parameters["PHASE"] = phase.ToUpperInvariant();
                    }
                }

                var operationId = request.OperationId ?? _progressTracking?.StartOperation("Workflow", request.Workflow.WorkflowName);

                _logger.LogInformation("Starting workflow execution: {WorkflowName} (OperationId: {OperationId}, Phase: {Phase})", 
                    request.Workflow.WorkflowName, operationId, phase ?? "N/A");

                // Execute workflow asynchronously
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _workflowService.ExecuteWorkflowAsync(request.Workflow, operationId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error executing workflow {WorkflowId}", request.Workflow.WorkflowId);
                        if (_progressTracking != null && operationId != null)
                        {
                            _progressTracking.CompleteOperation(operationId, false, errorMessage: ex.Message);
                        }
                    }
                });

                return Ok(new OperationStartResponse 
                { 
                    OperationId = operationId ?? request.Workflow.WorkflowId, 
                    Message = "Workflow execution started" 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting workflow execution");
                return StatusCode(500, new OperationStartResponse 
                { 
                    OperationId = "", 
                    Message = $"Error starting workflow: {ex.Message}" 
                });
            }
        }

        /// <summary>
        /// Get workflow definitions for a specific phase (Exploration, Development, Production, Decommissioning)
        /// </summary>
        [HttpGet("phase/{phase}")]
        public async Task<ActionResult<List<WorkflowDefinition>>> GetWorkflowsByPhase(string phase, [FromQuery] string? fieldId = null)
        {
            try
            {
                // Use current field if available and fieldId not specified
                if (_fieldOrchestrator != null && string.IsNullOrEmpty(fieldId) && !string.IsNullOrEmpty(_fieldOrchestrator.CurrentFieldId))
                {
                    fieldId = _fieldOrchestrator.CurrentFieldId;
                }

                var workflows = await _workflowService.GetWorkflowsByPhaseAsync(phase.ToUpperInvariant(), fieldId);
                return Ok(workflows);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting workflows for phase {Phase}", phase);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get all workflow definitions for the current field
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<WorkflowDefinition>>> GetWorkflows([FromQuery] string? fieldId = null, [FromQuery] string? phase = null)
        {
            try
            {
                // Use current field if available and fieldId not specified
                if (_fieldOrchestrator != null && string.IsNullOrEmpty(fieldId) && !string.IsNullOrEmpty(_fieldOrchestrator.CurrentFieldId))
                {
                    fieldId = _fieldOrchestrator.CurrentFieldId;
                }

                var workflows = await _workflowService.GetWorkflowsAsync(fieldId, phase);
                return Ok(workflows);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting workflows");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Save a workflow definition to PPDM database
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<WorkflowDefinition>> CreateWorkflow([FromBody] WorkflowDefinition workflow, [FromQuery] string? phase = null)
        {
            try
            {
                // Set field context if available
                if (_fieldOrchestrator != null && !string.IsNullOrEmpty(_fieldOrchestrator.CurrentFieldId))
                {
                    if (workflow.Parameters == null)
                    {
                        workflow.Parameters = new Dictionary<string, object>();
                    }
                    workflow.Parameters["FIELD_ID"] = _fieldOrchestrator.CurrentFieldId;

                    if (!string.IsNullOrEmpty(phase))
                    {
                        workflow.Parameters["PHASE"] = phase.ToUpperInvariant();
                    }
                }

                var savedWorkflow = await _workflowService.SaveWorkflowDefinitionAsync(workflow);
                return Ok(savedWorkflow);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving workflow definition");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get workflow execution history
        /// </summary>
        [HttpGet("{workflowId}/history")]
        public async Task<ActionResult<List<WorkflowExecutionResult>>> GetWorkflowHistory(string workflowId, [FromQuery] int limit = 50)
        {
            try
            {
                var history = await _workflowService.GetWorkflowExecutionHistoryAsync(workflowId, limit);
                return Ok(history);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting workflow history for {WorkflowId}", workflowId);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get workflow progress
        /// </summary>
        [HttpGet("{workflowId}/progress")]
        public ActionResult<WorkflowProgress> GetWorkflowProgress(string workflowId)
        {
            try
            {
                var progress = _progressTracking?.GetWorkflowProgress(workflowId);
                if (progress == null)
                {
                    return NotFound(new { error = "Workflow not found" });
                }
                return Ok(progress);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting workflow progress for {WorkflowId}", workflowId);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Cancel a workflow
        /// </summary>
        [HttpPost("{workflowId}/cancel")]
        public ActionResult CancelWorkflow(string workflowId)
        {
            try
            {
                _progressTracking?.CancelOperation(workflowId);
                _logger.LogInformation("Cancelled workflow {WorkflowId}", workflowId);
                return Ok(new { message = "Workflow cancellation requested" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling workflow {WorkflowId}", workflowId);
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
