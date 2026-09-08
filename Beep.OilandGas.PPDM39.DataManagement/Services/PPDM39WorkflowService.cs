using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;
using Beep.OilandGas.Models.Data.DataManagement;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.DataManagement.Core.Common;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;
using Beep.OilandGas.PPDM39.Core;

namespace Beep.OilandGas.PPDM39.DataManagement.Services
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

        public const string ExecutionUnavailableMessage =
            "Workflow execution is not implemented. Import, validation, quality-check, and version handlers are unavailable.";

        /// <summary>Rejects execution until real step handlers and governed orchestration are implemented.</summary>
        public Task<WorkflowExecutionResult> ExecuteWorkflowAsync(WorkflowDefinition workflow, string? operationId = null)
        {
            ArgumentNullException.ThrowIfNull(workflow);
            var now = DateTime.UtcNow;
            return Task.FromResult(new WorkflowExecutionResult
            {
                WorkflowId = workflow.WorkflowId,
                OperationId = operationId ?? workflow.WorkflowId,
                Success = false,
                ErrorMessage = ExecutionUnavailableMessage,
                StepResults = new Dictionary<string, object>(),
                StartedAt = now,
                CompletedAt = now,
                Duration = TimeSpan.Zero
            });
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
                    (ILogger<PPDMGenericRepository>?)_logger);
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
                    (ILogger<PPDMGenericRepository>?)_logger);
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
