using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.DTOs
{
    #region Process Definition DTOs

    /// <summary>
    /// Request for creating or updating a process definition
    /// </summary>
    public class ProcessDefinitionRequest
    {
        public string ProcessName { get; set; } = string.Empty;
        public string ProcessType { get; set; } = string.Empty; // EXPLORATION, DEVELOPMENT, PRODUCTION, DECOMMISSIONING
        public string EntityType { get; set; } = string.Empty; // WELL, FIELD, RESERVOIR, PROSPECT, POOL, FACILITY
        public string Description { get; set; } = string.Empty;
        public List<ProcessStepDefinitionRequest> Steps { get; set; } = new List<ProcessStepDefinitionRequest>();
        public Dictionary<string, ProcessTransitionRequest> Transitions { get; set; } = new Dictionary<string, ProcessTransitionRequest>();
        public Dictionary<string, object> Configuration { get; set; } = new Dictionary<string, object>();
        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// Response containing process definition
    /// </summary>
    public class ProcessDefinitionResponse
    {
        public string ProcessId { get; set; } = string.Empty;
        public string ProcessName { get; set; } = string.Empty;
        public string ProcessType { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<ProcessStepDefinitionResponse> Steps { get; set; } = new List<ProcessStepDefinitionResponse>();
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request for process step definition
    /// </summary>
    public class ProcessStepDefinitionRequest
    {
        public string StepId { get; set; } = string.Empty;
        public string StepName { get; set; } = string.Empty;
        public int SequenceNumber { get; set; }
        public string StepType { get; set; } = string.Empty; // ACTION, APPROVAL, VALIDATION, NOTIFICATION
        public bool IsRequired { get; set; } = true;
        public bool RequiresApproval { get; set; } = false;
        public List<string> RequiredRoles { get; set; } = new List<string>();
        public string NextStepId { get; set; } = string.Empty;
    }

    /// <summary>
    /// Response for process step definition
    /// </summary>
    public class ProcessStepDefinitionResponse
    {
        public string StepId { get; set; } = string.Empty;
        public string StepName { get; set; } = string.Empty;
        public int SequenceNumber { get; set; }
        public string StepType { get; set; } = string.Empty;
        public bool IsRequired { get; set; }
        public bool RequiresApproval { get; set; }
    }

    /// <summary>
    /// Request for process transition
    /// </summary>
    public class ProcessTransitionRequest
    {
        public string FromStateId { get; set; } = string.Empty;
        public string ToStateId { get; set; } = string.Empty;
        public string Trigger { get; set; } = string.Empty;
        public bool RequiresApproval { get; set; } = false;
    }

    #endregion

    #region Process Instance DTOs

    /// <summary>
    /// Request for starting a process
    /// </summary>
    public class ProcessInstanceRequest
    {
        public string ProcessId { get; set; } = string.Empty;
        public string EntityId { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public string FieldId { get; set; } = string.Empty;
        public Dictionary<string, object> InitialData { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// Response containing process instance
    /// </summary>
    public class ProcessInstanceResponse
    {
        public string InstanceId { get; set; } = string.Empty;
        public string ProcessId { get; set; } = string.Empty;
        public string ProcessName { get; set; } = string.Empty;
        public string EntityId { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public string FieldId { get; set; } = string.Empty;
        public string CurrentState { get; set; } = string.Empty;
        public string CurrentStepId { get; set; } = string.Empty;
        public string CurrentStepName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty; // NOT_STARTED, IN_PROGRESS, COMPLETED, FAILED, CANCELLED
        public DateTime StartDate { get; set; }
        public DateTime? CompletionDate { get; set; }
        public string StartedBy { get; set; } = string.Empty;
        public int CompletedStepsCount { get; set; }
        public int TotalStepsCount { get; set; }
        public decimal ProgressPercentage { get; set; }
        public List<ProcessStepInstanceResponse> Steps { get; set; } = new List<ProcessStepInstanceResponse>();
    }

    #endregion

    #region Process Step Execution DTOs

    /// <summary>
    /// Request for executing a process step
    /// </summary>
    public class ProcessStepExecutionRequest
    {
        public string InstanceId { get; set; } = string.Empty;
        public string StepId { get; set; } = string.Empty;
        public Dictionary<string, object> StepData { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// Response from step execution
    /// </summary>
    public class ProcessStepExecutionResponse
    {
        public string StepInstanceId { get; set; } = string.Empty;
        public string StepId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty; // PENDING, IN_PROGRESS, COMPLETED, FAILED, SKIPPED
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<ValidationResultResponse> ValidationResults { get; set; } = new List<ValidationResultResponse>();
    }

    /// <summary>
    /// Request for completing a step
    /// </summary>
    public class ProcessStepCompletionRequest
    {
        public string InstanceId { get; set; } = string.Empty;
        public string StepId { get; set; } = string.Empty;
        public string Outcome { get; set; } = string.Empty; // SUCCESS, FAILED, CONDITIONAL
        public string Notes { get; set; } = string.Empty;
    }

    #endregion

    #region Process State Transition DTOs

    /// <summary>
    /// Request for state transition
    /// </summary>
    public class ProcessStateTransitionRequest
    {
        public string InstanceId { get; set; } = string.Empty;
        public string TargetState { get; set; } = string.Empty;
        public Dictionary<string, object> TransitionData { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// Response from state transition
    /// </summary>
    public class ProcessStateTransitionResponse
    {
        public string InstanceId { get; set; } = string.Empty;
        public string PreviousState { get; set; } = string.Empty;
        public string NewState { get; set; } = string.Empty;
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    #endregion

    #region Process History DTOs

    /// <summary>
    /// Request for process history
    /// </summary>
    public class ProcessHistoryRequest
    {
        public string InstanceId { get; set; } = string.Empty;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string ActionFilter { get; set; } = string.Empty;
    }

    /// <summary>
    /// Response containing process history
    /// </summary>
    public class ProcessHistoryResponse
    {
        public string HistoryId { get; set; } = string.Empty;
        public string InstanceId { get; set; } = string.Empty;
        public string StepInstanceId { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string PreviousState { get; set; } = string.Empty;
        public string NewState { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string PerformedBy { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
    }

    #endregion

    #region Process Status DTOs

    /// <summary>
    /// Process status information
    /// </summary>
    public class ProcessStatusResponse
    {
        public string InstanceId { get; set; } = string.Empty;
        public string ProcessName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string CurrentState { get; set; } = string.Empty;
        public string CurrentStepName { get; set; } = string.Empty;
        public int CompletedStepsCount { get; set; }
        public int TotalStepsCount { get; set; }
        public decimal ProgressPercentage { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? CompletionDate { get; set; }
        public List<string> AvailableTransitions { get; set; } = new List<string>();
    }

    /// <summary>
    /// Process step instance response
    /// </summary>
    public class ProcessStepInstanceResponse
    {
        public string StepInstanceId { get; set; } = string.Empty;
        public string StepId { get; set; } = string.Empty;
        public string StepName { get; set; } = string.Empty;
        public int SequenceNumber { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool IsCurrentStep { get; set; }
        public bool CanExecute { get; set; }
        public bool RequiresApproval { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? CompletionDate { get; set; }
        public string Outcome { get; set; } = string.Empty;
        public List<ApprovalRecordResponse> Approvals { get; set; } = new List<ApprovalRecordResponse>();
    }

    /// <summary>
    /// Approval record response
    /// </summary>
    public class ApprovalRecordResponse
    {
        public string ApprovalId { get; set; } = string.Empty;
        public string ApprovalType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty; // PENDING, APPROVED, REJECTED
        public DateTime RequestedDate { get; set; }
        public string RequestedBy { get; set; } = string.Empty;
        public DateTime? ApprovedDate { get; set; }
        public string ApprovedBy { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
    }

    /// <summary>
    /// Validation result response
    /// </summary>
    public class ValidationResultResponse
    {
        public string ValidationId { get; set; } = string.Empty;
        public string RuleId { get; set; } = string.Empty;
        public bool IsValid { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public DateTime ValidatedDate { get; set; }
    }

    #endregion

    #region Entity Lifecycle DTOs

    /// <summary>
    /// Request for entity state transition
    /// </summary>
    public class EntityStateTransitionRequest
    {
        public string EntityId { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty; // WELL, FIELD, RESERVOIR
        public string TargetState { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
    }

    /// <summary>
    /// Response from entity state transition
    /// </summary>
    public class EntityStateTransitionResponse
    {
        public string EntityId { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public string PreviousState { get; set; } = string.Empty;
        public string NewState { get; set; } = string.Empty;
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<string> AvailableTransitions { get; set; } = new List<string>();
    }

    /// <summary>
    /// Entity state history response
    /// </summary>
    public class EntityStateHistoryResponse
    {
        public string EntityId { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public string CurrentState { get; set; } = string.Empty;
        public List<StateHistoryEntry> History { get; set; } = new List<StateHistoryEntry>();
    }

    /// <summary>
    /// State history entry
    /// </summary>
    public class StateHistoryEntry
    {
        public DateTime Date { get; set; }
        public string Status { get; set; } = string.Empty;
        public string ChangedBy { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
    }

    #endregion

    #region Process Database Entity DTOs

    /// <summary>
    /// Process definition database entity (DTO - used when PPDM model class doesn't exist)
    /// </summary>
    public class PROCESS_DEFINITION
    {
        public string? PROCESS_DEFINITION_ID { get; set; }
        public string? PROCESS_NAME { get; set; }
        public string? PROCESS_TYPE { get; set; }
        public string? ENTITY_TYPE { get; set; }
        public string? DESCRIPTION { get; set; }
        public string? PROCESS_CONFIG_JSON { get; set; }
        public string? ACTIVE_IND { get; set; }
        public string? REMARK { get; set; }
        public string? SOURCE { get; set; }
        public string? ROW_QUALITY { get; set; }
        public DateTime? ROW_CREATED_DATE { get; set; }
        public string? ROW_CREATED_BY { get; set; }
        public DateTime? ROW_CHANGED_DATE { get; set; }
        public string? ROW_CHANGED_BY { get; set; }
    }

    /// <summary>
    /// Process instance database entity (DTO - used when PPDM model class doesn't exist)
    /// </summary>
    public class PROCESS_INSTANCE
    {
        public string? PROCESS_INSTANCE_ID { get; set; }
        public string? PROCESS_DEFINITION_ID { get; set; }
        public string? ENTITY_ID { get; set; }
        public string? ENTITY_TYPE { get; set; }
        public string? FIELD_ID { get; set; }
        public string? CURRENT_STATE { get; set; }
        public string? CURRENT_STEP_ID { get; set; }
        public string? STATUS { get; set; }
        public DateTime? START_DATE { get; set; }
        public DateTime? COMPLETION_DATE { get; set; }
        public string? STARTED_BY { get; set; }
        public string? PROCESS_DATA_JSON { get; set; }
        public string? ACTIVE_IND { get; set; }
        public string? REMARK { get; set; }
        public string? SOURCE { get; set; }
        public string? ROW_QUALITY { get; set; }
        public DateTime? ROW_CREATED_DATE { get; set; }
        public string? ROW_CREATED_BY { get; set; }
        public DateTime? ROW_CHANGED_DATE { get; set; }
        public string? ROW_CHANGED_BY { get; set; }
    }

    /// <summary>
    /// Process step instance database entity (DTO - used when PPDM model class doesn't exist)
    /// </summary>
    public class PROCESS_STEP_INSTANCE
    {
        public string? PROCESS_STEP_INSTANCE_ID { get; set; }
        public string? PROCESS_INSTANCE_ID { get; set; }
        public string? STEP_ID { get; set; }
        public int? SEQUENCE_NUMBER { get; set; }
        public string? STATUS { get; set; }
        public DateTime? START_DATE { get; set; }
        public DateTime? COMPLETION_DATE { get; set; }
        public string? COMPLETED_BY { get; set; }
        public string? STEP_DATA_JSON { get; set; }
        public string? OUTCOME { get; set; }
        public string? NOTES { get; set; }
        public string? ACTIVE_IND { get; set; }
        public string? REMARK { get; set; }
        public string? SOURCE { get; set; }
        public string? ROW_QUALITY { get; set; }
        public DateTime? ROW_CREATED_DATE { get; set; }
        public string? ROW_CREATED_BY { get; set; }
        public DateTime? ROW_CHANGED_DATE { get; set; }
        public string? ROW_CHANGED_BY { get; set; }
    }

    /// <summary>
    /// Process history database entity (DTO - used when PPDM model class doesn't exist)
    /// </summary>
    public class PROCESS_HISTORY
    {
        public string? PROCESS_HISTORY_ID { get; set; }
        public string? PROCESS_INSTANCE_ID { get; set; }
        public string? PROCESS_STEP_INSTANCE_ID { get; set; }
        public string? ACTION { get; set; }
        public string? PREVIOUS_STATE { get; set; }
        public string? NEW_STATE { get; set; }
        public DateTime? ACTION_DATE { get; set; }
        public string? PERFORMED_BY { get; set; }
        public string? NOTES { get; set; }
        public string? ACTION_DATA_JSON { get; set; }
        public string? ACTIVE_IND { get; set; }
        public string? REMARK { get; set; }
        public string? SOURCE { get; set; }
        public string? ROW_QUALITY { get; set; }
        public DateTime? ROW_CREATED_DATE { get; set; }
        public string? ROW_CREATED_BY { get; set; }
        public DateTime? ROW_CHANGED_DATE { get; set; }
        public string? ROW_CHANGED_BY { get; set; }
    }

    /// <summary>
    /// Process approval database entity (DTO - used when PPDM model class doesn't exist)
    /// </summary>
    public class PROCESS_APPROVAL
    {
        public string? PROCESS_APPROVAL_ID { get; set; }
        public string? PROCESS_STEP_INSTANCE_ID { get; set; }
        public string? APPROVAL_TYPE { get; set; }
        public string? APPROVAL_STATUS { get; set; }
        public DateTime? REQUESTED_DATE { get; set; }
        public string? REQUESTED_BY { get; set; }
        public DateTime? APPROVED_DATE { get; set; }
        public string? APPROVED_BY { get; set; }
        public string? APPROVAL_NOTES { get; set; }
        public string? ACTIVE_IND { get; set; }
        public string? REMARK { get; set; }
        public string? SOURCE { get; set; }
        public string? ROW_QUALITY { get; set; }
        public DateTime? ROW_CREATED_DATE { get; set; }
        public string? ROW_CREATED_BY { get; set; }
        public DateTime? ROW_CHANGED_DATE { get; set; }
        public string? ROW_CHANGED_BY { get; set; }
    }

    #endregion
}




