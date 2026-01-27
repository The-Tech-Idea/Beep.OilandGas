using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.LifeCycle.Models.Processes;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.DataManagement.Services;
using Beep.OilandGas.PPDM39.Repositories;
using TheTechIdea.Beep.Editor;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using TheTechIdea.Beep.Report;
using Beep.OilandGas.Models.Data.Process;

namespace Beep.OilandGas.LifeCycle.Services.Processes
{
    /// <summary>
    /// Concrete implementation of process service that stores process data in database
    /// Note: Process workflow tables are in application database, not PPDM
    /// </summary>
    public class PPDMProcessService : ProcessServiceBase
    {
        private readonly string _applicationConnectionName; // Connection for application database (process tables)
        private PPDMGenericRepository? _processDefinitionRepository;
        private PPDMGenericRepository? _processInstanceRepository;
        private PPDMGenericRepository? _processStepInstanceRepository;
        private PPDMGenericRepository? _processHistoryRepository;
        private PPDMGenericRepository? _processApprovalRepository;

        public PPDMProcessService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            string connectionName = "PPDM39",
            string applicationConnectionName = "ApplicationDB",
            ILogger<ProcessServiceBase>? logger = null)
            : base(editor, commonColumnHandler, defaults, metadata, connectionName, logger)
        {
            _applicationConnectionName = applicationConnectionName ?? "ApplicationDB";
        }

        #region Process Definition Management

        public override async Task<ProcessDefinition> GetProcessDefinitionAsync(string processId)
        {
            try
            {
                var repo = await GetProcessDefinitionRepositoryAsync();
                var result = await repo.GetByIdAsync(processId);
                
                if (result == null)
                {
                    return null;
                }

                return ConvertToProcessDefinition(result);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error getting process definition: {processId}");
                throw;
            }
        }

        public override async Task<List<ProcessDefinition>> GetProcessDefinitionsByTypeAsync(string processType)
        {
            try
            {
                var repo = await GetProcessDefinitionRepositoryAsync();
                var filters = new List<AppFilter>
                {
                    new AppFilter
                    {
                        FieldName = "PROCESS_TYPE",
                        FilterValue = processType,
                        Operator = "="
                    },
                    new AppFilter
                    {
                        FieldName = "ACTIVE_IND",
                        FilterValue = "Y",
                        Operator = "="
                    }
                };

                var results = await repo.GetAsync(filters);
                return results.Select(r => ConvertToProcessDefinition(r)).ToList();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error getting process definitions by type: {processType}");
                throw;
            }
        }

        public override async Task<ProcessDefinition> CreateProcessDefinitionAsync(ProcessDefinition definition, string userId)
        {
            try
            {
                var repo = await GetProcessDefinitionRepositoryAsync();
                
                var entity = ConvertToProcessDefinitionEntity(definition);
                entity.PROCESS_DEFINITION_ID = definition.ProcessId ?? GenerateProcessDefinitionId();
                entity.ROW_CREATED_BY = userId;
                entity.ROW_CREATED_DATE = DateTime.UtcNow;

                var result = await repo.InsertAsync(entity, userId);
                return ConvertToProcessDefinition(result);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error creating process definition: {definition.ProcessName}");
                throw;
            }
        }

        public override async Task<ProcessDefinition> UpdateProcessDefinitionAsync(string processId, ProcessDefinition definition, string userId)
        {
            try
            {
                var repo = await GetProcessDefinitionRepositoryAsync();
                var entity = ConvertToProcessDefinitionEntity(definition);
                entity.PROCESS_DEFINITION_ID = processId;
                entity.ROW_CHANGED_BY = userId;
                entity.ROW_CHANGED_DATE = DateTime.UtcNow;

                var result = await repo.UpdateAsync(entity, userId);
                return ConvertToProcessDefinition(result);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error updating process definition: {processId}");
                throw;
            }
        }

        public override async Task<bool> DeleteProcessDefinitionAsync(string processId, string userId)
        {
            try
            {
                var repo = await GetProcessDefinitionRepositoryAsync();
                return await repo.SoftDeleteAsync(processId, userId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error deleting process definition: {processId}");
                throw;
            }
        }

        #endregion

        #region Process Instance Management

        public override async Task<ProcessInstance> GetProcessInstanceAsync(string instanceId)
        {
            try
            {
                return await LoadProcessInstanceAsync(instanceId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error getting process instance: {instanceId}");
                throw;
            }
        }

        public override async Task<List<ProcessInstance>> GetProcessInstancesForEntityAsync(string entityId, string entityType)
        {
            try
            {
                var repo = await GetProcessInstanceRepositoryAsync();
                var filters = new List<AppFilter>
                {
                    new AppFilter
                    {
                        FieldName = "ENTITY_ID",
                        FilterValue = entityId,
                        Operator = "="
                    },
                    new AppFilter
                    {
                        FieldName = "ENTITY_TYPE",
                        FilterValue = entityType,
                        Operator = "="
                    }
                };

                var results = await repo.GetAsync(filters);
                var instances = new List<ProcessInstance>();

                foreach (var result in results)
                {
                    var processInstance = result as PROCESS_INSTANCE;
                    if (processInstance != null && !string.IsNullOrEmpty(processInstance.PROCESS_INSTANCE_ID))
                    {
                        var instance = await LoadProcessInstanceAsync(processInstance.PROCESS_INSTANCE_ID);
                        if (instance != null)
                        {
                            instances.Add(instance);
                        }
                    }
                }

                return instances;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error getting process instances for entity: {entityType} {entityId}");
                throw;
            }
        }

        public override async Task<ProcessInstance> GetCurrentProcessForEntityAsync(string entityId, string entityType)
        {
            try
            {
                var instances = await GetProcessInstancesForEntityAsync(entityId, entityType);
                return instances
                    .Where(i => i.Status == ProcessStatus.IN_PROGRESS)
                    .OrderByDescending(i => i.StartDate)
                    .FirstOrDefault();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error getting current process for entity: {entityType} {entityId}");
                throw;
            }
        }

        public override async Task<bool> CancelProcessAsync(string instanceId, string reason, string userId)
        {
            try
            {
                var instance = await LoadProcessInstanceAsync(instanceId);
                if (instance == null)
                {
                    return false;
                }

                instance.Status = ProcessStatus.CANCELLED;
                instance.CompletionDate = DateTime.UtcNow;
                await SaveProcessInstanceAsync(instance);

                await AddHistoryEntryAsync(instanceId, new ProcessHistoryEntry
                {
                    HistoryId = GenerateHistoryId(),
                    InstanceId = instanceId,
                    Action = "PROCESS_CANCELLED",
                    NewState = "CANCELLED",
                    Timestamp = DateTime.UtcNow,
                    PerformedBy = userId,
                    Notes = reason
                });

                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error cancelling process instance: {instanceId}");
                throw;
            }
        }

        #endregion

        #region Process Execution

        public override async Task<bool> SkipStepAsync(string instanceId, string stepId, string reason, string userId)
        {
            try
            {
                var instance = await LoadProcessInstanceAsync(instanceId);
                if (instance == null)
                {
                    return false;
                }

                var stepInstance = instance.StepInstances.FirstOrDefault(s => s.StepId == stepId);
                if (stepInstance == null)
                {
                    return false;
                }

                stepInstance.Status = StepStatus.SKIPPED;
                stepInstance.CompletionDate = DateTime.UtcNow;
                stepInstance.CompletedBy = userId;
                stepInstance.Notes = reason;

                await SaveProcessInstanceAsync(instance);

                await AddHistoryEntryAsync(instanceId, new ProcessHistoryEntry
                {
                    HistoryId = GenerateHistoryId(),
                    InstanceId = instanceId,
                    StepInstanceId = stepInstance.StepInstanceId,
                    Action = "STEP_SKIPPED",
                    Timestamp = DateTime.UtcNow,
                    PerformedBy = userId,
                    Notes = reason
                });

                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error skipping step {stepId} for process instance: {instanceId}");
                throw;
            }
        }

        public override async Task<bool> RollbackStepAsync(string instanceId, string stepId, string reason, string userId)
        {
            try
            {
                var instance = await LoadProcessInstanceAsync(instanceId);
                if (instance == null)
                {
                    return false;
                }

                var stepInstance = instance.StepInstances.FirstOrDefault(s => s.StepId == stepId);
                if (stepInstance == null)
                {
                    return false;
                }

                stepInstance.Status = StepStatus.PENDING;
                stepInstance.StartDate = null;
                stepInstance.CompletionDate = null;
                stepInstance.CompletedBy = string.Empty;
                stepInstance.Notes = reason;

                await SaveProcessInstanceAsync(instance);

                await AddHistoryEntryAsync(instanceId, new ProcessHistoryEntry
                {
                    HistoryId = GenerateHistoryId(),
                    InstanceId = instanceId,
                    StepInstanceId = stepInstance.StepInstanceId,
                    Action = "STEP_ROLLED_BACK",
                    Timestamp = DateTime.UtcNow,
                    PerformedBy = userId,
                    Notes = reason
                });

                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error rolling back step {stepId} for process instance: {instanceId}");
                throw;
            }
        }

        #endregion

        #region State Management

        public override async Task<bool> TransitionStateAsync(string instanceId, string targetState, string userId)
        {
            try
            {
                var instance = await LoadProcessInstanceAsync(instanceId);
                if (instance == null)
                {
                    return false;
                }

                var previousState = instance.CurrentState;
                instance.CurrentState = targetState;

                await SaveProcessInstanceAsync(instance);

                await AddHistoryEntryAsync(instanceId, new ProcessHistoryEntry
                {
                    HistoryId = GenerateHistoryId(),
                    InstanceId = instanceId,
                    Action = "STATE_CHANGE",
                    PreviousState = previousState,
                    NewState = targetState,
                    Timestamp = DateTime.UtcNow,
                    PerformedBy = userId
                });

                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error transitioning state for process instance: {instanceId}");
                throw;
            }
        }

        public override async Task<List<string>> GetAvailableTransitionsAsync(string instanceId)
        {
            try
            {
                var instance = await LoadProcessInstanceAsync(instanceId);
                if (instance == null)
                {
                    return new List<string>();
                }

                var processDef = await GetProcessDefinitionAsync(instance.ProcessId);
                if (processDef == null)
                {
                    return new List<string>();
                }

                if (processDef.Transitions.ContainsKey(instance.CurrentState))
                {
                    var transition = processDef.Transitions[instance.CurrentState];
                    return new List<string> { transition.ToStateId };
                }

                return new List<string>();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error getting available transitions for process instance: {instanceId}");
                throw;
            }
        }

        public override async Task<bool> CanTransitionAsync(string instanceId, string targetState)
        {
            try
            {
                var availableTransitions = await GetAvailableTransitionsAsync(instanceId);
                return availableTransitions.Contains(targetState);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error checking transition for process instance: {instanceId}");
                throw;
            }
        }

        #endregion

        #region Process History

        public override async Task<List<ProcessHistoryEntry>> GetProcessHistoryAsync(string instanceId)
        {
            try
            {
                var repo = await GetProcessHistoryRepositoryAsync();
                var filters = new List<AppFilter>
                {
                    new AppFilter
                    {
                        FieldName = "PROCESS_INSTANCE_ID",
                        FilterValue = instanceId,
                        Operator = "="
                    }
                };

                var results = await repo.GetAsync(filters);
                return results.Select(r => ConvertToProcessHistoryEntry(r)).OrderBy(h => h.Timestamp).ToList();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error getting process history for instance: {instanceId}");
                throw;
            }
        }

        public override async Task<ProcessHistoryEntry> AddHistoryEntryAsync(string instanceId, ProcessHistoryEntry entry)
        {
            try
            {
                var repo = await GetProcessHistoryRepositoryAsync();
                var entity = ConvertToProcessHistoryEntity(entry);
                entity.PROCESS_HISTORY_ID = entry.HistoryId ?? GenerateHistoryId();
                entity.PROCESS_INSTANCE_ID = instanceId;
                entity.ROW_CREATED_BY = entry.PerformedBy;
                entity.ROW_CREATED_DATE = DateTime.UtcNow;

                await repo.InsertAsync(entity, entry.PerformedBy);
                return entry;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error adding history entry for instance: {instanceId}");
                throw;
            }
        }

        #endregion

        #region Validation

        public override async Task<ValidationResult> ValidateStepAsync(string instanceId, string stepId, PROCESS_STEP_DATA stepData)
        {
            try
            {
                var instance = await LoadProcessInstanceAsync(instanceId);
                if (instance == null)
                {
                    return new ValidationResult
                    {
                        IsValid = false,
                        ErrorMessage = "Process instance not found"
                    };
                }

                var processDef = await GetProcessDefinitionAsync(instance.ProcessId);
                if (processDef == null)
                {
                    return new ValidationResult
                    {
                        IsValid = false,
                        ErrorMessage = "Process definition not found"
                    };
                }

                var stepDef = processDef.Steps.FirstOrDefault(s => s.StepId == stepId);
                if (stepDef == null)
                {
                    return new ValidationResult
                    {
                        IsValid = false,
                        ErrorMessage = "Step definition not found"
                    };
                }

                var validator = new ProcessValidator(null);
                // Note: Need to check if validator needs update too
                return await validator.ValidateStepDataAsync(stepDef, stepData);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error validating step {stepId} for instance: {instanceId}");
                return new ValidationResult
                {
                    IsValid = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public override async Task<bool> ValidateProcessCompletionAsync(string instanceId)
        {
            try
            {
                var instance = await LoadProcessInstanceAsync(instanceId);
                if (instance == null)
                {
                    return false;
                }

                var processDef = await GetProcessDefinitionAsync(instance.ProcessId);
                if (processDef == null)
                {
                    return false;
                }

                var validator = new ProcessValidator(null);
                return validator.ValidateProcessCompletion(instance, processDef);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error validating process completion for instance: {instanceId}");
                return false;
            }
        }

        #endregion

        #region Approvals

        public override async Task<bool> RequestApprovalAsync(string stepInstanceId, string approvalType, string requestedBy, string userId)
        {
            try
            {
                var repo = await GetProcessApprovalRepositoryAsync();
                var entity = new PROCESS_APPROVAL
                {
                    PROCESS_APPROVAL_ID = GenerateApprovalId(),
                    PROCESS_STEP_INSTANCE_ID = stepInstanceId,
                    APPROVAL_TYPE = approvalType,
                    REQUESTED_DATE = DateTime.UtcNow,
                    REQUESTED_BY = requestedBy,
                    APPROVAL_STATUS = "PENDING",
                    ROW_CREATED_BY = userId,
                    ROW_CREATED_DATE = DateTime.UtcNow
                };

                await repo.InsertAsync(entity, userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error requesting approval for step instance: {stepInstanceId}");
                throw;
            }
        }

        public override async Task<bool> ApproveStepAsync(string approvalId, string approvedBy, string notes, string userId)
        {
            try
            {
                var repo = await GetProcessApprovalRepositoryAsync();
                var approval = await repo.GetByIdAsync(approvalId);
                if (approval == null)
                {
                    return false;
                }

                var entity = approval as PROCESS_APPROVAL;
                if (entity == null)
                {
                    return false;
                }
                entity.APPROVAL_STATUS = "APPROVED";
                entity.APPROVED_DATE = DateTime.UtcNow;
                entity.APPROVED_BY = approvedBy;
                entity.APPROVAL_NOTES = notes;
                entity.ROW_CHANGED_BY = userId;
                entity.ROW_CHANGED_DATE = DateTime.UtcNow;

                await repo.UpdateAsync(entity, userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error approving step: {approvalId}");
                throw;
            }
        }

        public override async Task<bool> RejectStepAsync(string approvalId, string rejectedBy, string reason, string userId)
        {
            try
            {
                var repo = await GetProcessApprovalRepositoryAsync();
                var approval = await repo.GetByIdAsync(approvalId);
                if (approval == null)
                {
                    return false;
                }

                var entity = approval as PROCESS_APPROVAL;
                if (entity == null)
                {
                    return false;
                }
                entity.APPROVAL_STATUS = "REJECTED";
                entity.APPROVED_DATE = DateTime.UtcNow;
                entity.APPROVED_BY = rejectedBy;
                entity.APPROVAL_NOTES = reason;
                entity.ROW_CHANGED_BY = userId;
                entity.ROW_CHANGED_DATE = DateTime.UtcNow;

                await repo.UpdateAsync(entity, userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error rejecting step: {approvalId}");
                throw;
            }
        }

        #endregion

        #region Protected Methods

        protected override async Task SaveProcessInstanceAsync(ProcessInstance instance)
        {
            try
            {
                var repo = await GetProcessInstanceRepositoryAsync();
                var entity = ConvertToProcessInstanceEntity(instance);
                
                var existing = await repo.GetByIdAsync(instance.InstanceId);
                if (existing == null)
                {
                    entity.ROW_CREATED_BY = instance.StartedBy;
                    entity.ROW_CREATED_DATE = DateTime.UtcNow;
                    await repo.InsertAsync(entity, instance.StartedBy);
                }
                else
                {
                    entity.ROW_CHANGED_BY = instance.StartedBy;
                    entity.ROW_CHANGED_DATE = DateTime.UtcNow;
                    await repo.UpdateAsync(entity, instance.StartedBy);
                }

                // Save step instances
                var stepRepo = await GetProcessStepInstanceRepositoryAsync();
                foreach (var stepInstance in instance.StepInstances)
                {
                    var stepEntity = ConvertToProcessStepInstanceEntity(stepInstance);
                    var existingStep = await stepRepo.GetByIdAsync(stepInstance.StepInstanceId);
                    
                    if (existingStep == null)
                    {
                        stepEntity.ROW_CREATED_BY = stepInstance.CompletedBy ?? instance.StartedBy;
                        stepEntity.ROW_CREATED_DATE = DateTime.UtcNow;
                        await stepRepo.InsertAsync(stepEntity, stepInstance.CompletedBy ?? instance.StartedBy);
                    }
                    else
                    {
                        stepEntity.ROW_CHANGED_BY = stepInstance.CompletedBy ?? instance.StartedBy;
                        stepEntity.ROW_CHANGED_DATE = DateTime.UtcNow;
                        await stepRepo.UpdateAsync(stepEntity, stepInstance.CompletedBy ?? instance.StartedBy);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error saving process instance: {instance.InstanceId}");
                throw;
            }
        }

        protected override async Task<ProcessInstance> LoadProcessInstanceAsync(string instanceId)
        {
            try
            {
                var repo = await GetProcessInstanceRepositoryAsync();
                var result = await repo.GetByIdAsync(instanceId);
                
                if (result == null)
                {
                    return null;
                }

                var instance = ConvertToProcessInstance(result);

                // Load step instances
                var stepRepo = await GetProcessStepInstanceRepositoryAsync();
                var stepFilters = new List<AppFilter>
                {
                    new AppFilter
                    {
                        FieldName = "PROCESS_INSTANCE_ID",
                        FilterValue = instanceId,
                        Operator = "="
                    }
                };

                var stepResults = await stepRepo.GetAsync(stepFilters);
                instance.StepInstances = stepResults
                    .Select(r => ConvertToProcessStepInstance(r))
                    .OrderBy(s => s.SequenceNumber)
                    .ToList();

                // Load history
                instance.History = await GetProcessHistoryAsync(instanceId);

                return instance;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error loading process instance: {instanceId}");
                throw;
            }
        }

        #endregion

        #region Repository Helpers

        private async Task<PPDMGenericRepository> GetProcessDefinitionRepositoryAsync()
        {
            if (_processDefinitionRepository == null)
            {
                _processDefinitionRepository = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    typeof(PROCESS_DEFINITION), _applicationConnectionName, "PROCESS_DEFINITION");
            }
            return _processDefinitionRepository;
        }

        private async Task<PPDMGenericRepository> GetProcessInstanceRepositoryAsync()
        {
            if (_processInstanceRepository == null)
            {
                _processInstanceRepository = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    typeof(PROCESS_INSTANCE), _applicationConnectionName, "PROCESS_INSTANCE");
            }
            return _processInstanceRepository;
        }

        private async Task<PPDMGenericRepository> GetProcessStepInstanceRepositoryAsync()
        {
            if (_processStepInstanceRepository == null)
            {
                _processStepInstanceRepository = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    typeof(PROCESS_STEP_INSTANCE), _applicationConnectionName, "PROCESS_STEP_INSTANCE");
            }
            return _processStepInstanceRepository;
        }

        private async Task<PPDMGenericRepository> GetProcessHistoryRepositoryAsync()
        {
            if (_processHistoryRepository == null)
            {
                _processHistoryRepository = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    typeof(PROCESS_HISTORY), _applicationConnectionName, "PROCESS_HISTORY");
            }
            return _processHistoryRepository;
        }

        private async Task<PPDMGenericRepository> GetProcessApprovalRepositoryAsync()
        {
            if (_processApprovalRepository == null)
            {
                _processApprovalRepository = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    typeof(PROCESS_APPROVAL), _applicationConnectionName, "PROCESS_APPROVAL");
            }
            return _processApprovalRepository;
        }

        #endregion

        #region Conversion Helpers

        private ProcessDefinition ConvertToProcessDefinition(object entity)
        {
            var processDef = entity as PROCESS_DEFINITION;
            if (processDef == null)
            {
                throw new ArgumentException("Entity is not a PROCESS_DEFINITION", nameof(entity));
            }
            
            var definition = new ProcessDefinition
            {
                ProcessId = processDef.PROCESS_DEFINITION_ID ?? string.Empty,
                ProcessName = processDef.PROCESS_NAME ?? string.Empty,
                ProcessType = processDef.PROCESS_TYPE ?? string.Empty,
                EntityType = processDef.ENTITY_TYPE ?? string.Empty,
                Description = processDef.DESCRIPTION ?? string.Empty,
                IsActive = processDef.ACTIVE_IND == "Y",
                CreatedDate = processDef.ROW_CREATED_DATE ?? DateTime.UtcNow,
                CreatedBy = processDef.ROW_CREATED_BY ?? string.Empty,
                Steps = new List<ProcessStepDefinition>(),
                Transitions = new Dictionary<string, ProcessTransition>(),
                Configuration = new Dictionary<string, object>()
            };

            // Deserialize JSON fields
            var configJson = processDef.PROCESS_CONFIG_JSON;
            if (!string.IsNullOrEmpty(configJson))
            {
                try
                {
                    // Try to deserialize stored configuration as a dictionary
                    var config = JsonSerializer.Deserialize<Dictionary<string, object>>(configJson);
                    if (config != null)
                    {
                        definition.Configuration = config;
                    }
                    else
                    {
                        // Fallback: attempt to deserialize into PROCESS_CONFIGURATION and convert
                        var legacy = JsonSerializer.Deserialize<PROCESS_CONFIGURATION>(configJson);
                        if (legacy != null)
                        {
                            // Convert legacy PROCESS_CONFIGURATION to a dictionary via serialization
                            var roundtrip = JsonSerializer.Serialize(legacy);
                            var converted = JsonSerializer.Deserialize<Dictionary<string, object>>(roundtrip);
                            if (converted != null)
                                definition.Configuration = converted;
                        }
                    }
                    // Extract steps and transitions from config if needed
                }
                catch (Exception ex)
                {
                    _logger?.LogWarning(ex, "Error deserializing process config JSON");
                }
            }

            return definition;
        }

        private PROCESS_DEFINITION ConvertToProcessDefinitionEntity(ProcessDefinition definition)
        {
            var entity = new PROCESS_DEFINITION
            {
                PROCESS_DEFINITION_ID = definition.ProcessId,
                PROCESS_NAME = definition.ProcessName,
                PROCESS_TYPE = definition.ProcessType,
                ENTITY_TYPE = definition.EntityType,
                DESCRIPTION = definition.Description,
                ACTIVE_IND = definition.IsActive ? "Y" : "N"
            };

            // Serialize steps, transitions, and configuration to JSON
            var config = new Dictionary<string, object>
            {
                ["Steps"] = definition.Steps,
                ["Transitions"] = definition.Transitions,
                ["Configuration"] = definition.Configuration
            };

            entity.PROCESS_CONFIG_JSON = JsonSerializer.Serialize(config);

            return entity;
        }

        private ProcessInstance ConvertToProcessInstance(object entity)
        {
            var processInst = entity as PROCESS_INSTANCE;
            if (processInst == null)
            {
                throw new ArgumentException("Entity is not a PROCESS_INSTANCE", nameof(entity));
            }
            
            return new ProcessInstance
            {
                InstanceId = processInst.PROCESS_INSTANCE_ID ?? string.Empty,
                ProcessId = processInst.PROCESS_DEFINITION_ID ?? string.Empty,
                EntityId = processInst.ENTITY_ID ?? string.Empty,
                EntityType = processInst.ENTITY_TYPE ?? string.Empty,
                FieldId = processInst.FIELD_ID ?? string.Empty,
                CurrentState = processInst.CURRENT_STATE ?? string.Empty,
                CurrentStepId = processInst.CURRENT_STEP_ID ?? string.Empty,
                Status = Enum.TryParse<ProcessStatus>(processInst.STATUS ?? "NOT_STARTED", out var status) ? status : ProcessStatus.NOT_STARTED,
                StartDate = processInst.START_DATE ?? DateTime.UtcNow,
                CompletionDate = processInst.COMPLETION_DATE,
                StartedBy = processInst.STARTED_BY ?? string.Empty,
                ProcessData = new PROCESS_DATA(),
                StepInstances = new List<ProcessStepInstance>(),
                History = new List<ProcessHistoryEntry>()
            };
        }

        private PROCESS_INSTANCE ConvertToProcessInstanceEntity(ProcessInstance instance)
        {
            return new PROCESS_INSTANCE
            {
                PROCESS_INSTANCE_ID = instance.InstanceId,
                PROCESS_DEFINITION_ID = instance.ProcessId,
                ENTITY_ID = instance.EntityId,
                ENTITY_TYPE = instance.EntityType,
                FIELD_ID = instance.FieldId,
                CURRENT_STATE = instance.CurrentState,
                CURRENT_STEP_ID = instance.CurrentStepId,
                STATUS = instance.Status.ToString(),
                START_DATE = instance.StartDate,
                COMPLETION_DATE = instance.CompletionDate,
                STARTED_BY = instance.StartedBy,
                PROCESS_DATA_JSON = JsonSerializer.Serialize(instance.ProcessData)
            };
        }

        private ProcessStepInstance ConvertToProcessStepInstance(object entity)
        {
            var stepInst = entity as PROCESS_STEP_INSTANCE;
            if (stepInst == null)
            {
                throw new ArgumentException("Entity is not a PROCESS_STEP_INSTANCE", nameof(entity));
            }
            
            return new ProcessStepInstance
            {
                StepInstanceId = stepInst.PROCESS_STEP_INSTANCE_ID ?? string.Empty,
                InstanceId = stepInst.PROCESS_INSTANCE_ID ?? string.Empty,
                StepId = stepInst.STEP_ID ?? string.Empty,
                SequenceNumber = stepInst.SEQUENCE_NUMBER ?? 0,
                Status = Enum.TryParse<StepStatus>(stepInst.STATUS ?? "PENDING", out var status) ? status : StepStatus.PENDING,
                StartDate = stepInst.START_DATE,
                CompletionDate = stepInst.COMPLETION_DATE,
                CompletedBy = stepInst.COMPLETED_BY ?? string.Empty,
                Outcome = stepInst.OUTCOME ?? string.Empty,
                Notes = stepInst.NOTES ?? string.Empty,
                StepData = new PROCESS_STEP_DATA(),
                Approvals = new List<ApprovalRecord>(),
                ValidationResults = new List<ValidationResult>()
            };
        }

        private PROCESS_STEP_INSTANCE ConvertToProcessStepInstanceEntity(ProcessStepInstance stepInstance)
        {
            return new PROCESS_STEP_INSTANCE
            {
                PROCESS_STEP_INSTANCE_ID = stepInstance.StepInstanceId,
                PROCESS_INSTANCE_ID = stepInstance.InstanceId,
                STEP_ID = stepInstance.StepId,
                SEQUENCE_NUMBER = stepInstance.SequenceNumber,
                STATUS = stepInstance.Status.ToString(),
                START_DATE = stepInstance.StartDate,
                COMPLETION_DATE = stepInstance.CompletionDate,
                COMPLETED_BY = stepInstance.CompletedBy,
                STEP_DATA_JSON = JsonSerializer.Serialize(stepInstance.StepData),
                OUTCOME = stepInstance.Outcome,
                NOTES = stepInstance.Notes
            };
        }

        private ProcessHistoryEntry ConvertToProcessHistoryEntry(object entity)
        {
            var history = entity as PROCESS_HISTORY;
            if (history == null)
            {
                throw new ArgumentException("Entity is not a PROCESS_HISTORY", nameof(entity));
            }
            
            return new ProcessHistoryEntry
            {
                HistoryId = history.PROCESS_HISTORY_ID ?? string.Empty,
                InstanceId = history.PROCESS_INSTANCE_ID ?? string.Empty,
                StepInstanceId = history.PROCESS_STEP_INSTANCE_ID ?? string.Empty,
                Action = history.ACTION ?? string.Empty,
                PreviousState = history.PREVIOUS_STATE ?? string.Empty,
                NewState = history.NEW_STATE ?? string.Empty,
                Timestamp = history.ACTION_DATE ?? DateTime.UtcNow,
                PerformedBy = history.PERFORMED_BY ?? string.Empty,
                Notes = history.NOTES ?? string.Empty,
                // Deserialize ACTION_DATA_JSON into a dictionary for ActionData
                ActionData = string.IsNullOrEmpty(history.ACTION_DATA_JSON)
                    ? new Dictionary<string, object>()
                    : JsonSerializer.Deserialize<Dictionary<string, object>>(history.ACTION_DATA_JSON) ?? new Dictionary<string, object>()
            };
        }

        private PROCESS_HISTORY ConvertToProcessHistoryEntity(ProcessHistoryEntry entry)
        {
            return new PROCESS_HISTORY
            {
                PROCESS_HISTORY_ID = entry.HistoryId,
                PROCESS_INSTANCE_ID = entry.InstanceId,
                PROCESS_STEP_INSTANCE_ID = entry.StepInstanceId,
                ACTION = entry.Action,
                PREVIOUS_STATE = entry.PreviousState,
                NEW_STATE = entry.NewState,
                ACTION_DATE = entry.Timestamp,
                PERFORMED_BY = entry.PerformedBy,
                NOTES = entry.Notes,
                ACTION_DATA_JSON = JsonSerializer.Serialize(entry.ActionData)
            };
        }

        #endregion

        #region Helper Methods

        private string GetStringValue(Dictionary<string, object> dict, string key)
        {
            return dict.ContainsKey(key) ? dict[key]?.ToString() ?? string.Empty : string.Empty;
        }

        private DateTime? GetDateTimeValue(Dictionary<string, object> dict, string key)
        {
            if (!dict.ContainsKey(key))
                return null;

            var value = dict[key];
            if (value is DateTime dt)
                return dt;
            if (value is string str && DateTime.TryParse(str, out var parsed))
                return parsed;
            return null;
        }

        private int GetIntValue(Dictionary<string, object> dict, string key)
        {
            if (!dict.ContainsKey(key))
                return 0;

            var value = dict[key];
            if (value is int i)
                return i;
            if (value is string str && int.TryParse(str, out var parsed))
                return parsed;
            return 0;
        }

        private string GenerateProcessDefinitionId() => $"PD_{Guid.NewGuid():N}";

        #endregion
    }
}

