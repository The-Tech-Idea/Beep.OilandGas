using System;
using System.Threading.Tasks;
using Beep.OilandGas.ApiService.Models;
using Beep.OilandGas.ApiService.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.ApiService.Controllers.PPDM39
{
    /// <summary>
    /// API controller for PPDM39 workflow pipeline operations
    /// </summary>
    [ApiController]
    [Route("api/ppdm39/workflow")]
    public class PPDM39WorkflowController : ControllerBase
    {
        private readonly PPDM39WorkflowService _workflowService;
        private readonly ILogger<PPDM39WorkflowController> _logger;
        private readonly IProgressTrackingService? _progressTracking;

        public PPDM39WorkflowController(
            PPDM39WorkflowService workflowService,
            ILogger<PPDM39WorkflowController> logger,
            IProgressTrackingService progressTracking)
        {
            _workflowService = workflowService ?? throw new ArgumentNullException(nameof(workflowService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _progressTracking = progressTracking;
        }

        /// <summary>
        /// Execute a workflow pipeline
        /// </summary>
        [HttpPost("execute")]
        public async Task<ActionResult<OperationStartResponse>> ExecuteWorkflow([FromBody] WorkflowExecutionRequest request)
        {
            try
            {
                if (request?.Workflow == null)
                {
                    return BadRequest(new OperationStartResponse { OperationId = "", Message = "Workflow definition is required" });
                }

                var operationId = request.OperationId ?? _progressTracking?.StartOperation("Workflow", request.Workflow.WorkflowName);

                _logger.LogInformation("Starting workflow execution: {WorkflowName} (OperationId: {OperationId})", 
                    request.Workflow.WorkflowName, operationId);

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
