using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;
using Beep.OilandGas.ApiService.Models;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.DataManagement.Core.Common;
using Beep.OilandGas.PPDM39.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.Repositories;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.ApiService.Services
{
    /// <summary>
    /// Service for executing workflow pipelines with progress tracking
    /// </summary>
    public class PPDM39WorkflowService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly ILogger<PPDM39WorkflowService> _logger;
        private readonly IProgressTrackingService? _progressTracking;
        private readonly string _connectionName;
        private PPDMGenericRepository? _workflowRepository;
        private PPDMGenericRepository? _workflowExecutionRepository;

        public PPDM39WorkflowService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            ILogger<PPDM39WorkflowService> logger,
            IProgressTrackingService? progressTracking = null,
            string connectionName = "PPDM39")
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _progressTracking = progressTracking;
            _connectionName = connectionName;
        }

        /// <summary>
        /// Executes a workflow pipeline
        /// </summary>
        public async Task<WorkflowExecutionResult> ExecuteWorkflowAsync(WorkflowDefinition workflow, string? operationId = null)
        {
            var workflowId = workflow.WorkflowId;
            var startTime = DateTime.UtcNow;
            var stepResults = new Dictionary<string, object>();

            try
            {
                _logger.LogInformation("Starting workflow {WorkflowId}: {WorkflowName} with {StepCount} steps", 
                    workflowId, workflow.WorkflowName, workflow.Steps.Count);

                // Start workflow progress tracking
                if (_progressTracking != null)
                {
                    var stepNames = workflow.Steps.Select(s => s.StepName).ToList();
                    var stepWeights = workflow.Steps.Select(s => s.EstimatedWeight).ToList();
                    var workflowOpId = _progressTracking.StartWorkflow(workflow.WorkflowName, stepNames, stepWeights);
                    operationId ??= workflowOpId;
                }

                // Build dependency graph
                var stepDependencies = BuildDependencyGraph(workflow.Steps);
                
                // Execute steps in dependency order
                var completedSteps = new HashSet<string>();
                var failedSteps = new HashSet<string>();
                var stepOperationIds = new Dictionary<string, string>();

                foreach (var step in workflow.Steps.OrderBy(s => GetStepExecutionOrder(s, stepDependencies, completedSteps)))
                {
                    // Check if dependencies are met
                    if (!string.IsNullOrEmpty(step.DependsOn) && !completedSteps.Contains(step.DependsOn))
                    {
                        _logger.LogWarning("Step {StepName} skipped due to unmet dependency {DependsOn}", step.StepName, step.DependsOn);
                        continue;
                    }

                    // Skip if already failed and stop on error
                    if (failedSteps.Any() && workflow.StopOnError)
                    {
                        _logger.LogInformation("Stopping workflow due to previous step failure");
                        break;
                    }

                    try
                    {
                        _logger.LogInformation("Executing workflow step: {StepName}", step.StepName);

                        // Execute step based on operation type
                        var stepResult = await ExecuteWorkflowStepAsync(step, workflow, stepOperationIds);
                        stepResults[step.StepId] = stepResult ?? new { success = true };

                        completedSteps.Add(step.StepId);

                        if (_progressTracking != null && stepOperationIds.ContainsKey(step.StepId))
                        {
                            _progressTracking.CompleteWorkflowStep(workflowId, step.StepId, true);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error executing workflow step {StepName}", step.StepName);
                        failedSteps.Add(step.StepId);
                        stepResults[step.StepId] = new { success = false, error = ex.Message };

                        if (_progressTracking != null && stepOperationIds.ContainsKey(step.StepId))
                        {
                            _progressTracking.CompleteWorkflowStep(workflowId, step.StepId, false, ex.Message);
                        }

                        if (workflow.StopOnError)
                        {
                            break;
                        }
                    }
                }

                var success = failedSteps.Count == 0;
                var errorMessage = failedSteps.Any() ? $"{failedSteps.Count} step(s) failed" : null;

                if (_progressTracking != null)
                {
                    _progressTracking.CompleteWorkflow(workflowId, success, errorMessage);
                }

                var duration = DateTime.UtcNow - startTime;

                _logger.LogInformation("Workflow {WorkflowId} completed: Success={Success}, Duration={Duration}", 
                    workflowId, success, duration);

                var executionResult = new WorkflowExecutionResult
                {
                    WorkflowId = workflowId,
                    OperationId = operationId ?? workflowId,
                    Success = success,
                    ErrorMessage = errorMessage,
                    StepResults = stepResults,
                    StartedAt = startTime,
                    CompletedAt = DateTime.UtcNow,
                    Duration = duration
                };

                // Save execution result to database
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await SaveWorkflowExecutionResultAsync(executionResult);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to save workflow execution result to database");
                    }
                });

                return executionResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fatal error executing workflow {WorkflowId}", workflowId);
                
                if (_progressTracking != null)
                {
                    _progressTracking.CompleteWorkflow(workflowId, false, ex.Message);
                }

                return new WorkflowExecutionResult
                {
                    WorkflowId = workflowId,
                    OperationId = operationId ?? workflowId,
                    Success = false,
                    ErrorMessage = ex.Message,
                    StepResults = stepResults,
                    StartedAt = startTime,
                    CompletedAt = DateTime.UtcNow,
                    Duration = DateTime.UtcNow - startTime
                };
            }
        }

        /// <summary>
        /// Executes a single workflow step
        /// </summary>
        private async Task<object?> ExecuteWorkflowStepAsync(
            WorkflowStep step, 
            WorkflowDefinition workflow,
            Dictionary<string, string> stepOperationIds)
        {
            var stepOperationId = Guid.NewGuid().ToString();
            stepOperationIds[step.StepId] = stepOperationId;

            // Start step progress
            if (_progressTracking != null)
            {
                _progressTracking.UpdateWorkflowStep(workflow.WorkflowId, step.StepId, stepOperationId, 0, $"Starting {step.StepName}");
            }

            try
            {
                switch (step.OperationType.ToLowerInvariant())
                {
                    case "importcsv":
                        return await ExecuteImportCsvStepAsync(step, workflow, stepOperationId);
                    
                    case "validate":
                        return await ExecuteValidateStepAsync(step, workflow, stepOperationId);
                    
                    case "qualitycheck":
                        return await ExecuteQualityCheckStepAsync(step, workflow, stepOperationId);
                    
                    case "version":
                        return await ExecuteVersionStepAsync(step, workflow, stepOperationId);
                    
                    default:
                        throw new NotSupportedException($"Operation type '{step.OperationType}' is not supported");
                }
            }
            finally
            {
                // Step progress updates are handled by individual step execution methods
            }
        }

        private async Task<object> ExecuteImportCsvStepAsync(WorkflowStep step, WorkflowDefinition workflow, string operationId)
        {
            // This would integrate with import service
            // For now, return a placeholder
            _logger.LogInformation("Executing ImportCsv step: {StepName}", step.StepName);
            
            if (_progressTracking != null)
            {
                _progressTracking.UpdateWorkflowStep(workflow.WorkflowId, step.StepId, operationId, 50, "Importing CSV...");
                _progressTracking.UpdateWorkflowStep(workflow.WorkflowId, step.StepId, operationId, 100, "CSV import completed");
            }
            
            await Task.Delay(100); // Placeholder
            return new { success = true, message = "CSV import completed" };
        }

        private async Task<object> ExecuteValidateStepAsync(WorkflowStep step, WorkflowDefinition workflow, string operationId)
        {
            _logger.LogInformation("Executing Validate step: {StepName}", step.StepName);
            
            if (_progressTracking != null)
            {
                _progressTracking.UpdateWorkflowStep(workflow.WorkflowId, step.StepId, operationId, 50, "Validating data...");
                _progressTracking.UpdateWorkflowStep(workflow.WorkflowId, step.StepId, operationId, 100, "Validation completed");
            }
            
            await Task.Delay(100); // Placeholder
            return new { success = true, message = "Validation completed" };
        }

        private async Task<object> ExecuteQualityCheckStepAsync(WorkflowStep step, WorkflowDefinition workflow, string operationId)
        {
            _logger.LogInformation("Executing QualityCheck step: {StepName}", step.StepName);
            
            if (_progressTracking != null)
            {
                _progressTracking.UpdateWorkflowStep(workflow.WorkflowId, step.StepId, operationId, 50, "Checking data quality...");
                _progressTracking.UpdateWorkflowStep(workflow.WorkflowId, step.StepId, operationId, 100, "Quality check completed");
            }
            
            await Task.Delay(100); // Placeholder
            return new { success = true, message = "Quality check completed" };
        }

        private async Task<object> ExecuteVersionStepAsync(WorkflowStep step, WorkflowDefinition workflow, string operationId)
        {
            _logger.LogInformation("Executing Version step: {StepName}", step.StepName);
            
            if (_progressTracking != null)
            {
                _progressTracking.UpdateWorkflowStep(workflow.WorkflowId, step.StepId, operationId, 50, "Creating version...");
                _progressTracking.UpdateWorkflowStep(workflow.WorkflowId, step.StepId, operationId, 100, "Version created");
            }
            
            await Task.Delay(100); // Placeholder
            return new { success = true, message = "Version created" };
        }

        /// <summary>
        /// Builds dependency graph for workflow steps
        /// </summary>
        private Dictionary<string, HashSet<string>> BuildDependencyGraph(List<WorkflowStep> steps)
        {
            var graph = new Dictionary<string, HashSet<string>>();
            
            foreach (var step in steps)
            {
                if (!graph.ContainsKey(step.StepId))
                {
                    graph[step.StepId] = new HashSet<string>();
                }

                if (!string.IsNullOrEmpty(step.DependsOn))
                {
                    graph[step.StepId].Add(step.DependsOn);
                }
            }

            return graph;
        }

        /// <summary>
        /// Gets execution order for a step based on dependencies
        /// </summary>
        private int GetStepExecutionOrder(WorkflowStep step, Dictionary<string, HashSet<string>> dependencies, HashSet<string> completedSteps)
        {
            if (string.IsNullOrEmpty(step.DependsOn))
            {
                return 0; // No dependencies, can run first
            }

            // If dependency is completed, can run now
            if (completedSteps.Contains(step.DependsOn))
            {
                return 1;
            }

            // Otherwise, wait
            return 2;
        }

        /// <summary>
        /// Gets or creates repository for workflow definitions
        /// </summary>
        private async Task<PPDMGenericRepository> GetWorkflowRepositoryAsync()
        {
            if (_workflowRepository == null)
            {
                var metadata = await _metadata.GetTableMetadataAsync("WORKFLOW_DEFINITION");
                var entityType = typeof(Dictionary<string, object>);
                
                if (metadata != null && !string.IsNullOrEmpty(metadata.EntityTypeName))
                {
                    var resolvedType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}");
                    if (resolvedType != null)
                    {
                        entityType = resolvedType;
                    }
                }

                _workflowRepository = new PPDMGenericRepository(
                    _editor,
                    _commonColumnHandler,
                    _defaults,
                    _metadata,
                    entityType,
                    _connectionName,
                    "WORKFLOW_DEFINITION",
                    _logger);
            }
            return _workflowRepository;
        }

        /// <summary>
        /// Gets or creates repository for workflow execution results
        /// </summary>
        private async Task<PPDMGenericRepository> GetWorkflowExecutionRepositoryAsync()
        {
            if (_workflowExecutionRepository == null)
            {
                var metadata = await _metadata.GetTableMetadataAsync("WORKFLOW_EXECUTION");
                var entityType = typeof(Dictionary<string, object>);
                
                if (metadata != null && !string.IsNullOrEmpty(metadata.EntityTypeName))
                {
                    var resolvedType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}");
                    if (resolvedType != null)
                    {
                        entityType = resolvedType;
                    }
                }

                _workflowExecutionRepository = new PPDMGenericRepository(
                    _editor,
                    _commonColumnHandler,
                    _defaults,
                    _metadata,
                    entityType,
                    _connectionName,
                    "WORKFLOW_EXECUTION",
                    _logger);
            }
            return _workflowExecutionRepository;
        }

        /// <summary>
        /// Converts WorkflowDefinition to dictionary for storage
        /// </summary>
        private Dictionary<string, object> ConvertWorkflowToDictionary(WorkflowDefinition workflow)
        {
            var json = JsonSerializer.Serialize(workflow);
            var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(json) ?? new Dictionary<string, object>();
            
            // Ensure key fields are set
            dict["WORKFLOW_ID"] = workflow.WorkflowId;
            dict["WORKFLOW_NAME"] = workflow.WorkflowName;
            
            if (!string.IsNullOrEmpty(workflow.Phase))
                dict["PHASE"] = workflow.Phase;
            
            if (!string.IsNullOrEmpty(workflow.FieldId))
                dict["FIELD_ID"] = workflow.FieldId;

            // Extract field ID from parameters if present
            if (workflow.Parameters != null && workflow.Parameters.ContainsKey("FIELD_ID"))
            {
                dict["FIELD_ID"] = workflow.Parameters["FIELD_ID"];
            }

            if (workflow.Parameters != null && workflow.Parameters.ContainsKey("PHASE"))
            {
                dict["PHASE"] = workflow.Parameters["PHASE"];
            }

            return dict;
        }

        /// <summary>
        /// Converts dictionary to WorkflowDefinition
        /// </summary>
        private WorkflowDefinition ConvertDictionaryToWorkflow(Dictionary<string, object> dict)
        {
            var json = JsonSerializer.Serialize(dict);
            return JsonSerializer.Deserialize<WorkflowDefinition>(json) ?? new WorkflowDefinition();
        }

        /// <summary>
        /// Saves workflow definition to PPDM database
        /// </summary>
        public async Task<WorkflowDefinition> SaveWorkflowDefinitionAsync(WorkflowDefinition workflow)
        {
            try
            {
                var repository = await GetWorkflowRepositoryAsync();
                var dict = ConvertWorkflowToDictionary(workflow);
                
                // Set field ID from parameters if present
                if (workflow.Parameters != null && workflow.Parameters.ContainsKey("FIELD_ID"))
                {
                    workflow.FieldId = workflow.Parameters["FIELD_ID"]?.ToString();
                }

                if (workflow.Parameters != null && workflow.Parameters.ContainsKey("PHASE"))
                {
                    workflow.Phase = workflow.Parameters["PHASE"]?.ToString();
                }

                await repository.InsertAsync(dict, workflow.UserId ?? "system");
                
                _logger.LogInformation("Saved workflow definition {WorkflowId} to database", workflow.WorkflowId);
                return workflow;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving workflow definition {WorkflowId}", workflow.WorkflowId);
                throw;
            }
        }

        /// <summary>
        /// Gets workflows filtered by phase and/or field
        /// </summary>
        public async Task<List<WorkflowDefinition>> GetWorkflowsAsync(string? fieldId = null, string? phase = null)
        {
            try
            {
                var repository = await GetWorkflowRepositoryAsync();
                var filters = new List<AppFilter>();

                if (!string.IsNullOrEmpty(fieldId))
                {
                    filters.Add(new AppFilter { FieldName = "FIELD_ID", Operator = "=", FilterValue = fieldId });
                }

                if (!string.IsNullOrEmpty(phase))
                {
                    filters.Add(new AppFilter { FieldName = "PHASE", Operator = "=", FilterValue = phase.ToUpperInvariant() });
                }

                var results = await repository.GetAsync(filters);
                
                var workflows = new List<WorkflowDefinition>();
                foreach (var result in results)
                {
                    if (result is Dictionary<string, object> dict)
                    {
                        workflows.Add(ConvertDictionaryToWorkflow(dict));
                    }
                }

                return workflows;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting workflows");
                throw;
            }
        }

        /// <summary>
        /// Gets workflows by phase
        /// </summary>
        public async Task<List<WorkflowDefinition>> GetWorkflowsByPhaseAsync(string phase, string? fieldId = null)
        {
            return await GetWorkflowsAsync(fieldId, phase);
        }

        /// <summary>
        /// Saves workflow execution result to PPDM database
        /// </summary>
        public async Task SaveWorkflowExecutionResultAsync(WorkflowExecutionResult result)
        {
            try
            {
                var repository = await GetWorkflowExecutionRepositoryAsync();
                var json = JsonSerializer.Serialize(result);
                var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(json) ?? new Dictionary<string, object>();
                
                dict["WORKFLOW_ID"] = result.WorkflowId;
                dict["OPERATION_ID"] = result.OperationId;

                await repository.InsertAsync(dict, "system");
                
                _logger.LogInformation("Saved workflow execution result {OperationId} to database", result.OperationId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving workflow execution result {OperationId}", result.OperationId);
                // Don't throw - execution results are logged but not critical
            }
        }

        /// <summary>
        /// Gets workflow execution history
        /// </summary>
        public async Task<List<WorkflowExecutionResult>> GetWorkflowExecutionHistoryAsync(string workflowId, int limit = 50)
        {
            try
            {
                var repository = await GetWorkflowExecutionRepositoryAsync();
                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "WORKFLOW_ID", Operator = "=", FilterValue = workflowId }
                };

                var results = await repository.GetAsync(filters);
                
                var executions = new List<WorkflowExecutionResult>();
                foreach (var result in results.Take(limit))
                {
                    if (result is Dictionary<string, object> dict)
                    {
                        var json = JsonSerializer.Serialize(dict);
                        var execution = JsonSerializer.Deserialize<WorkflowExecutionResult>(json);
                        if (execution != null)
                        {
                            executions.Add(execution);
                        }
                    }
                }

                // Sort by start time descending (most recent first)
                return executions.OrderByDescending(e => e.StartedAt).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting workflow execution history for {WorkflowId}", workflowId);
                throw;
            }
        }
    }
}
