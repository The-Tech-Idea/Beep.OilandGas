using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data.Validation;

namespace Beep.OilandGas.Models.Data
{
    #region Process Definition DTOs

    /// <summary>
    /// Request for creating or updating a process definition
    /// </summary>
    public class ProcessDefinitionRequest : ModelEntityBase
    {
        private string ProcessNameValue = string.Empty;

        public string ProcessName

        {

            get { return this.ProcessNameValue; }

            set { SetProperty(ref ProcessNameValue, value); }

        }
        private string ProcessTypeValue = string.Empty;

        public string ProcessType

        {

            get { return this.ProcessTypeValue; }

            set { SetProperty(ref ProcessTypeValue, value); }

        } // EXPLORATION, DEVELOPMENT, PRODUCTION, DECOMMISSIONING
        private string EntityTypeValue = string.Empty;

        public string EntityType

        {

            get { return this.EntityTypeValue; }

            set { SetProperty(ref EntityTypeValue, value); }

        } // WELL, FIELD, RESERVOIR, PROSPECT, POOL, FACILITY
        private string DescriptionValue = string.Empty;

        public string Description

        {

            get { return this.DescriptionValue; }

            set { SetProperty(ref DescriptionValue, value); }

        }
        private List<ProcessStepDefinitionRequest> StepsValue = new List<ProcessStepDefinitionRequest>();

        public List<ProcessStepDefinitionRequest> Steps

        {

            get { return this.StepsValue; }

            set { SetProperty(ref StepsValue, value); }

        }
        public Dictionary<string, ProcessTransitionRequest> Transitions { get; set; } = new Dictionary<string, ProcessTransitionRequest>();
        public Dictionary<string, object> Configuration { get; set; } = new Dictionary<string, object>();
        private bool IsActiveValue = true;

        public bool IsActive

        {

            get { return this.IsActiveValue; }

            set { SetProperty(ref IsActiveValue, value); }

        }
    }

    /// <summary>
    /// Response containing process definition
    /// </summary>
    public class ProcessDefinitionResponse : ModelEntityBase
    {
        private string ProcessIdValue = string.Empty;

        public string ProcessId

        {

            get { return this.ProcessIdValue; }

            set { SetProperty(ref ProcessIdValue, value); }

        }
        private string ProcessNameValue = string.Empty;

        public string ProcessName

        {

            get { return this.ProcessNameValue; }

            set { SetProperty(ref ProcessNameValue, value); }

        }
        private string ProcessTypeValue = string.Empty;

        public string ProcessType

        {

            get { return this.ProcessTypeValue; }

            set { SetProperty(ref ProcessTypeValue, value); }

        }
        private string EntityTypeValue = string.Empty;

        public string EntityType

        {

            get { return this.EntityTypeValue; }

            set { SetProperty(ref EntityTypeValue, value); }

        }
        private string DescriptionValue = string.Empty;

        public string Description

        {

            get { return this.DescriptionValue; }

            set { SetProperty(ref DescriptionValue, value); }

        }
        private List<ProcessStepDefinitionResponse> StepsValue = new List<ProcessStepDefinitionResponse>();

        public List<ProcessStepDefinitionResponse> Steps

        {

            get { return this.StepsValue; }

            set { SetProperty(ref StepsValue, value); }

        }
        private bool IsActiveValue;

        public bool IsActive

        {

            get { return this.IsActiveValue; }

            set { SetProperty(ref IsActiveValue, value); }

        }
        private DateTime CreatedDateValue;

        public DateTime CreatedDate

        {

            get { return this.CreatedDateValue; }

            set { SetProperty(ref CreatedDateValue, value); }

        }
        private string CreatedByValue = string.Empty;

        public string CreatedBy

        {

            get { return this.CreatedByValue; }

            set { SetProperty(ref CreatedByValue, value); }

        }
    }

    /// <summary>
    /// Request for process step definition
    /// </summary>
    public class ProcessStepDefinitionRequest : ModelEntityBase
    {
        private string StepIdValue = string.Empty;

        public string StepId

        {

            get { return this.StepIdValue; }

            set { SetProperty(ref StepIdValue, value); }

        }
        private string StepNameValue = string.Empty;

        public string StepName

        {

            get { return this.StepNameValue; }

            set { SetProperty(ref StepNameValue, value); }

        }
        private int SequenceNumberValue;

        public int SequenceNumber

        {

            get { return this.SequenceNumberValue; }

            set { SetProperty(ref SequenceNumberValue, value); }

        }
        private string StepTypeValue = string.Empty;

        public string StepType

        {

            get { return this.StepTypeValue; }

            set { SetProperty(ref StepTypeValue, value); }

        } // ACTION, APPROVAL, VALIDATION, NOTIFICATION
        private bool IsRequiredValue = true;

        public bool IsRequired

        {

            get { return this.IsRequiredValue; }

            set { SetProperty(ref IsRequiredValue, value); }

        }
        private bool RequiresApprovalValue = false;

        public bool RequiresApproval

        {

            get { return this.RequiresApprovalValue; }

            set { SetProperty(ref RequiresApprovalValue, value); }

        }
        private List<string> RequiredRolesValue = new List<string>();

        public List<string> RequiredRoles

        {

            get { return this.RequiredRolesValue; }

            set { SetProperty(ref RequiredRolesValue, value); }

        }
        private string NextStepIdValue = string.Empty;

        public string NextStepId

        {

            get { return this.NextStepIdValue; }

            set { SetProperty(ref NextStepIdValue, value); }

        }
    }

    /// <summary>
    /// Response for process step definition
    /// </summary>
    public class ProcessStepDefinitionResponse : ModelEntityBase
    {
        private string StepIdValue = string.Empty;

        public string StepId

        {

            get { return this.StepIdValue; }

            set { SetProperty(ref StepIdValue, value); }

        }
        private string StepNameValue = string.Empty;

        public string StepName

        {

            get { return this.StepNameValue; }

            set { SetProperty(ref StepNameValue, value); }

        }
        private int SequenceNumberValue;

        public int SequenceNumber

        {

            get { return this.SequenceNumberValue; }

            set { SetProperty(ref SequenceNumberValue, value); }

        }
        private string StepTypeValue = string.Empty;

        public string StepType

        {

            get { return this.StepTypeValue; }

            set { SetProperty(ref StepTypeValue, value); }

        }
        private bool IsRequiredValue;

        public bool IsRequired

        {

            get { return this.IsRequiredValue; }

            set { SetProperty(ref IsRequiredValue, value); }

        }
        private bool RequiresApprovalValue;

        public bool RequiresApproval

        {

            get { return this.RequiresApprovalValue; }

            set { SetProperty(ref RequiresApprovalValue, value); }

        }
    }

    /// <summary>
    /// Request for process transition
    /// </summary>
    public class ProcessTransitionRequest : ModelEntityBase
    {
        private string FromStateIdValue = string.Empty;

        public string FromStateId

        {

            get { return this.FromStateIdValue; }

            set { SetProperty(ref FromStateIdValue, value); }

        }
        private string ToStateIdValue = string.Empty;

        public string ToStateId

        {

            get { return this.ToStateIdValue; }

            set { SetProperty(ref ToStateIdValue, value); }

        }
        private string TriggerValue = string.Empty;

        public string Trigger

        {

            get { return this.TriggerValue; }

            set { SetProperty(ref TriggerValue, value); }

        }
        private bool RequiresApprovalValue = false;

        public bool RequiresApproval

        {

            get { return this.RequiresApprovalValue; }

            set { SetProperty(ref RequiresApprovalValue, value); }

        }
    }

    #endregion

    #region Process Instance DTOs

    /// <summary>
    /// Request for starting a process
    /// </summary>
    public class ProcessInstanceRequest : ModelEntityBase
    {
        private string ProcessIdValue = string.Empty;

        public string ProcessId

        {

            get { return this.ProcessIdValue; }

            set { SetProperty(ref ProcessIdValue, value); }

        }
        private string EntityIdValue = string.Empty;

        public string EntityId

        {

            get { return this.EntityIdValue; }

            set { SetProperty(ref EntityIdValue, value); }

        }
        private string EntityTypeValue = string.Empty;

        public string EntityType

        {

            get { return this.EntityTypeValue; }

            set { SetProperty(ref EntityTypeValue, value); }

        }
        private string FieldIdValue = string.Empty;

        public string FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        }
        public Dictionary<string, object> InitialData { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// Response containing process instance
    /// </summary>
    public class ProcessInstanceResponse : ModelEntityBase
    {
        private string InstanceIdValue = string.Empty;

        public string InstanceId

        {

            get { return this.InstanceIdValue; }

            set { SetProperty(ref InstanceIdValue, value); }

        }
        private string ProcessIdValue = string.Empty;

        public string ProcessId

        {

            get { return this.ProcessIdValue; }

            set { SetProperty(ref ProcessIdValue, value); }

        }
        private string ProcessNameValue = string.Empty;

        public string ProcessName

        {

            get { return this.ProcessNameValue; }

            set { SetProperty(ref ProcessNameValue, value); }

        }
        private string EntityIdValue = string.Empty;

        public string EntityId

        {

            get { return this.EntityIdValue; }

            set { SetProperty(ref EntityIdValue, value); }

        }
        private string EntityTypeValue = string.Empty;

        public string EntityType

        {

            get { return this.EntityTypeValue; }

            set { SetProperty(ref EntityTypeValue, value); }

        }
        private string FieldIdValue = string.Empty;

        public string FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        }
        private string CurrentStateValue = string.Empty;

        public string CurrentState

        {

            get { return this.CurrentStateValue; }

            set { SetProperty(ref CurrentStateValue, value); }

        }
        private string CurrentStepIdValue = string.Empty;

        public string CurrentStepId

        {

            get { return this.CurrentStepIdValue; }

            set { SetProperty(ref CurrentStepIdValue, value); }

        }
        private string CurrentStepNameValue = string.Empty;

        public string CurrentStepName

        {

            get { return this.CurrentStepNameValue; }

            set { SetProperty(ref CurrentStepNameValue, value); }

        }
        private string StatusValue = string.Empty;

        public string Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        } // NOT_STARTED, IN_PROGRESS, COMPLETED, FAILED, CANCELLED
        private DateTime StartDateValue;

        public DateTime StartDate

        {

            get { return this.StartDateValue; }

            set { SetProperty(ref StartDateValue, value); }

        }
        private DateTime? CompletionDateValue;

        public DateTime? CompletionDate

        {

            get { return this.CompletionDateValue; }

            set { SetProperty(ref CompletionDateValue, value); }

        }
        private string StartedByValue = string.Empty;

        public string StartedBy

        {

            get { return this.StartedByValue; }

            set { SetProperty(ref StartedByValue, value); }

        }
        private int CompletedStepsCountValue;

        public int CompletedStepsCount

        {

            get { return this.CompletedStepsCountValue; }

            set { SetProperty(ref CompletedStepsCountValue, value); }

        }
        private int TotalStepsCountValue;

        public int TotalStepsCount

        {

            get { return this.TotalStepsCountValue; }

            set { SetProperty(ref TotalStepsCountValue, value); }

        }
        private decimal ProgressPercentageValue;

        public decimal ProgressPercentage

        {

            get { return this.ProgressPercentageValue; }

            set { SetProperty(ref ProgressPercentageValue, value); }

        }
        private List<ProcessStepInstanceResponse> StepsValue = new List<ProcessStepInstanceResponse>();

        public List<ProcessStepInstanceResponse> Steps

        {

            get { return this.StepsValue; }

            set { SetProperty(ref StepsValue, value); }

        }
    }

    #endregion

    #region Process Step Execution DTOs

    /// <summary>
    /// Request for executing a process step
    /// </summary>
    public class ProcessStepExecutionRequest : ModelEntityBase
    {
        private string InstanceIdValue = string.Empty;

        public string InstanceId

        {

            get { return this.InstanceIdValue; }

            set { SetProperty(ref InstanceIdValue, value); }

        }
        private string StepIdValue = string.Empty;

        public string StepId

        {

            get { return this.StepIdValue; }

            set { SetProperty(ref StepIdValue, value); }

        }
        public Dictionary<string, object> StepData { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// Response from step execution
    /// </summary>
    public class ProcessStepExecutionResponse : ModelEntityBase
    {
        private string StepInstanceIdValue = string.Empty;

        public string StepInstanceId

        {

            get { return this.StepInstanceIdValue; }

            set { SetProperty(ref StepInstanceIdValue, value); }

        }
        private string StepIdValue = string.Empty;

        public string StepId

        {

            get { return this.StepIdValue; }

            set { SetProperty(ref StepIdValue, value); }

        }
        private string StatusValue = string.Empty;

        public string Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        } // PENDING, IN_PROGRESS, COMPLETED, FAILED, SKIPPED
        private bool SuccessValue;

        public bool Success

        {

            get { return this.SuccessValue; }

            set { SetProperty(ref SuccessValue, value); }

        }
        private string MessageValue = string.Empty;

        public string Message

        {

            get { return this.MessageValue; }

            set { SetProperty(ref MessageValue, value); }

        }
        private List<ValidationResultResponse> ValidationResultsValue = new List<ValidationResultResponse>();

        public List<ValidationResultResponse> ValidationResults

        {

            get { return this.ValidationResultsValue; }

            set { SetProperty(ref ValidationResultsValue, value); }

        }
    }

    /// <summary>
    /// Request for completing a step
    /// </summary>
    public class ProcessStepCompletionRequest : ModelEntityBase
    {
        private string InstanceIdValue = string.Empty;

        public string InstanceId

        {

            get { return this.InstanceIdValue; }

            set { SetProperty(ref InstanceIdValue, value); }

        }
        private string StepIdValue = string.Empty;

        public string StepId

        {

            get { return this.StepIdValue; }

            set { SetProperty(ref StepIdValue, value); }

        }
        private string OutcomeValue = string.Empty;

        public string Outcome

        {

            get { return this.OutcomeValue; }

            set { SetProperty(ref OutcomeValue, value); }

        } // SUCCESS, FAILED, CONDITIONAL
        private string NotesValue = string.Empty;

        public string Notes

        {

            get { return this.NotesValue; }

            set { SetProperty(ref NotesValue, value); }

        }
    }

    #endregion

    #region Process State Transition DTOs

    /// <summary>
    /// Request for state transition
    /// </summary>
    public class ProcessStateTransitionRequest : ModelEntityBase
    {
        private string InstanceIdValue = string.Empty;

        public string InstanceId

        {

            get { return this.InstanceIdValue; }

            set { SetProperty(ref InstanceIdValue, value); }

        }
        private string TargetStateValue = string.Empty;

        public string TargetState

        {

            get { return this.TargetStateValue; }

            set { SetProperty(ref TargetStateValue, value); }

        }
        public Dictionary<string, object> TransitionData { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// Response from state transition
    /// </summary>
    public class ProcessStateTransitionResponse : ModelEntityBase
    {
        private string InstanceIdValue = string.Empty;

        public string InstanceId

        {

            get { return this.InstanceIdValue; }

            set { SetProperty(ref InstanceIdValue, value); }

        }
        private string PreviousStateValue = string.Empty;

        public string PreviousState

        {

            get { return this.PreviousStateValue; }

            set { SetProperty(ref PreviousStateValue, value); }

        }
        private string NewStateValue = string.Empty;

        public string NewState

        {

            get { return this.NewStateValue; }

            set { SetProperty(ref NewStateValue, value); }

        }
        private bool SuccessValue;

        public bool Success

        {

            get { return this.SuccessValue; }

            set { SetProperty(ref SuccessValue, value); }

        }
        private string MessageValue = string.Empty;

        public string Message

        {

            get { return this.MessageValue; }

            set { SetProperty(ref MessageValue, value); }

        }
    }

    #endregion

    #region Process History DTOs

    /// <summary>
    /// Request for process history
    /// </summary>
    public class ProcessHistoryRequest : ModelEntityBase
    {
        private string InstanceIdValue = string.Empty;

        public string InstanceId

        {

            get { return this.InstanceIdValue; }

            set { SetProperty(ref InstanceIdValue, value); }

        }
        private DateTime? StartDateValue;

        public DateTime? StartDate

        {

            get { return this.StartDateValue; }

            set { SetProperty(ref StartDateValue, value); }

        }
        private DateTime? EndDateValue;

        public DateTime? EndDate

        {

            get { return this.EndDateValue; }

            set { SetProperty(ref EndDateValue, value); }

        }
        private string ActionFilterValue = string.Empty;

        public string ActionFilter

        {

            get { return this.ActionFilterValue; }

            set { SetProperty(ref ActionFilterValue, value); }

        }
    }

    /// <summary>
    /// Response containing process history
    /// </summary>
    public class ProcessHistoryResponse : ModelEntityBase
    {
        private string HistoryIdValue = string.Empty;

        public string HistoryId

        {

            get { return this.HistoryIdValue; }

            set { SetProperty(ref HistoryIdValue, value); }

        }
        private string InstanceIdValue = string.Empty;

        public string InstanceId

        {

            get { return this.InstanceIdValue; }

            set { SetProperty(ref InstanceIdValue, value); }

        }
        private string StepInstanceIdValue = string.Empty;

        public string StepInstanceId

        {

            get { return this.StepInstanceIdValue; }

            set { SetProperty(ref StepInstanceIdValue, value); }

        }
        private string ActionValue = string.Empty;

        public string Action

        {

            get { return this.ActionValue; }

            set { SetProperty(ref ActionValue, value); }

        }
        private string PreviousStateValue = string.Empty;

        public string PreviousState

        {

            get { return this.PreviousStateValue; }

            set { SetProperty(ref PreviousStateValue, value); }

        }
        private string NewStateValue = string.Empty;

        public string NewState

        {

            get { return this.NewStateValue; }

            set { SetProperty(ref NewStateValue, value); }

        }
        private DateTime TimestampValue;

        public DateTime Timestamp

        {

            get { return this.TimestampValue; }

            set { SetProperty(ref TimestampValue, value); }

        }
        private string PerformedByValue = string.Empty;

        public string PerformedBy

        {

            get { return this.PerformedByValue; }

            set { SetProperty(ref PerformedByValue, value); }

        }
        private string NotesValue = string.Empty;

        public string Notes

        {

            get { return this.NotesValue; }

            set { SetProperty(ref NotesValue, value); }

        }
    }

    #endregion

    #region Process Status DTOs

    /// <summary>
    /// Process status information
    /// </summary>
    public class ProcessStatusResponse : ModelEntityBase
    {
        private string InstanceIdValue = string.Empty;

        public string InstanceId

        {

            get { return this.InstanceIdValue; }

            set { SetProperty(ref InstanceIdValue, value); }

        }
        private string ProcessNameValue = string.Empty;

        public string ProcessName

        {

            get { return this.ProcessNameValue; }

            set { SetProperty(ref ProcessNameValue, value); }

        }
        private string StatusValue = string.Empty;

        public string Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }
        private string CurrentStateValue = string.Empty;

        public string CurrentState

        {

            get { return this.CurrentStateValue; }

            set { SetProperty(ref CurrentStateValue, value); }

        }
        private string CurrentStepNameValue = string.Empty;

        public string CurrentStepName

        {

            get { return this.CurrentStepNameValue; }

            set { SetProperty(ref CurrentStepNameValue, value); }

        }
        private int CompletedStepsCountValue;

        public int CompletedStepsCount

        {

            get { return this.CompletedStepsCountValue; }

            set { SetProperty(ref CompletedStepsCountValue, value); }

        }
        private int TotalStepsCountValue;

        public int TotalStepsCount

        {

            get { return this.TotalStepsCountValue; }

            set { SetProperty(ref TotalStepsCountValue, value); }

        }
        private decimal ProgressPercentageValue;

        public decimal ProgressPercentage

        {

            get { return this.ProgressPercentageValue; }

            set { SetProperty(ref ProgressPercentageValue, value); }

        }
        private DateTime StartDateValue;

        public DateTime StartDate

        {

            get { return this.StartDateValue; }

            set { SetProperty(ref StartDateValue, value); }

        }
        private DateTime? CompletionDateValue;

        public DateTime? CompletionDate

        {

            get { return this.CompletionDateValue; }

            set { SetProperty(ref CompletionDateValue, value); }

        }
        private List<string> AvailableTransitionsValue = new List<string>();

        public List<string> AvailableTransitions

        {

            get { return this.AvailableTransitionsValue; }

            set { SetProperty(ref AvailableTransitionsValue, value); }

        }
    }

    /// <summary>
    /// Process step instance response
    /// </summary>
    public class ProcessStepInstanceResponse : ModelEntityBase
    {
        private string StepInstanceIdValue = string.Empty;

        public string StepInstanceId

        {

            get { return this.StepInstanceIdValue; }

            set { SetProperty(ref StepInstanceIdValue, value); }

        }
        private string StepIdValue = string.Empty;

        public string StepId

        {

            get { return this.StepIdValue; }

            set { SetProperty(ref StepIdValue, value); }

        }
        private string StepNameValue = string.Empty;

        public string StepName

        {

            get { return this.StepNameValue; }

            set { SetProperty(ref StepNameValue, value); }

        }
        private int SequenceNumberValue;

        public int SequenceNumber

        {

            get { return this.SequenceNumberValue; }

            set { SetProperty(ref SequenceNumberValue, value); }

        }
        private string StatusValue = string.Empty;

        public string Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }
        private bool IsCurrentStepValue;

        public bool IsCurrentStep

        {

            get { return this.IsCurrentStepValue; }

            set { SetProperty(ref IsCurrentStepValue, value); }

        }
        private bool CanExecuteValue;

        public bool CanExecute

        {

            get { return this.CanExecuteValue; }

            set { SetProperty(ref CanExecuteValue, value); }

        }
        private bool RequiresApprovalValue;

        public bool RequiresApproval

        {

            get { return this.RequiresApprovalValue; }

            set { SetProperty(ref RequiresApprovalValue, value); }

        }
        private DateTime? StartDateValue;

        public DateTime? StartDate

        {

            get { return this.StartDateValue; }

            set { SetProperty(ref StartDateValue, value); }

        }
        private DateTime? CompletionDateValue;

        public DateTime? CompletionDate

        {

            get { return this.CompletionDateValue; }

            set { SetProperty(ref CompletionDateValue, value); }

        }
        private string OutcomeValue = string.Empty;

        public string Outcome

        {

            get { return this.OutcomeValue; }

            set { SetProperty(ref OutcomeValue, value); }

        }
        private List<ApprovalRecordResponse> ApprovalsValue = new List<ApprovalRecordResponse>();

        public List<ApprovalRecordResponse> Approvals

        {

            get { return this.ApprovalsValue; }

            set { SetProperty(ref ApprovalsValue, value); }

        }
    }

    /// <summary>
    /// Approval class response
    /// </summary>
    public class ApprovalRecordResponse : ModelEntityBase
    {
        private string ApprovalIdValue = string.Empty;

        public string ApprovalId

        {

            get { return this.ApprovalIdValue; }

            set { SetProperty(ref ApprovalIdValue, value); }

        }
        private string ApprovalTypeValue = string.Empty;

        public string ApprovalType

        {

            get { return this.ApprovalTypeValue; }

            set { SetProperty(ref ApprovalTypeValue, value); }

        }
        private string StatusValue = string.Empty;

        public string Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        } // PENDING, APPROVED, REJECTED
        private DateTime RequestedDateValue;

        public DateTime RequestedDate

        {

            get { return this.RequestedDateValue; }

            set { SetProperty(ref RequestedDateValue, value); }

        }
        private string RequestedByValue = string.Empty;

        public string RequestedBy

        {

            get { return this.RequestedByValue; }

            set { SetProperty(ref RequestedByValue, value); }

        }
        private DateTime? ApprovedDateValue;

        public DateTime? ApprovedDate

        {

            get { return this.ApprovedDateValue; }

            set { SetProperty(ref ApprovedDateValue, value); }

        }
        private string ApprovedByValue = string.Empty;

        public string ApprovedBy

        {

            get { return this.ApprovedByValue; }

            set { SetProperty(ref ApprovedByValue, value); }

        }
        private string NotesValue = string.Empty;

        public string Notes

        {

            get { return this.NotesValue; }

            set { SetProperty(ref NotesValue, value); }

        }
    }

    /// <summary>
    /// Validation result response
    /// </summary>
    // ValidationResultResponse moved to Beep.OilandGas.Models.Data.Validation namespace

    #endregion

    #region Entity Lifecycle DTOs

    /// <summary>
    /// Request for entity state transition
    /// </summary>
    public class EntityStateTransitionRequest : ModelEntityBase
    {
        private string EntityIdValue = string.Empty;

        public string EntityId

        {

            get { return this.EntityIdValue; }

            set { SetProperty(ref EntityIdValue, value); }

        }
        private string EntityTypeValue = string.Empty;

        public string EntityType

        {

            get { return this.EntityTypeValue; }

            set { SetProperty(ref EntityTypeValue, value); }

        } // WELL, FIELD, RESERVOIR
        private string TargetStateValue = string.Empty;

        public string TargetState

        {

            get { return this.TargetStateValue; }

            set { SetProperty(ref TargetStateValue, value); }

        }
        private string ReasonValue = string.Empty;

        public string Reason

        {

            get { return this.ReasonValue; }

            set { SetProperty(ref ReasonValue, value); }

        }
    }

    /// <summary>
    /// Response from entity state transition
    /// </summary>
    public class EntityStateTransitionResponse : ModelEntityBase
    {
        private string EntityIdValue = string.Empty;

        public string EntityId

        {

            get { return this.EntityIdValue; }

            set { SetProperty(ref EntityIdValue, value); }

        }
        private string EntityTypeValue = string.Empty;

        public string EntityType

        {

            get { return this.EntityTypeValue; }

            set { SetProperty(ref EntityTypeValue, value); }

        }
        private string PreviousStateValue = string.Empty;

        public string PreviousState

        {

            get { return this.PreviousStateValue; }

            set { SetProperty(ref PreviousStateValue, value); }

        }
        private string NewStateValue = string.Empty;

        public string NewState

        {

            get { return this.NewStateValue; }

            set { SetProperty(ref NewStateValue, value); }

        }
        private bool SuccessValue;

        public bool Success

        {

            get { return this.SuccessValue; }

            set { SetProperty(ref SuccessValue, value); }

        }
        private string MessageValue = string.Empty;

        public string Message

        {

            get { return this.MessageValue; }

            set { SetProperty(ref MessageValue, value); }

        }
        private List<string> AvailableTransitionsValue = new List<string>();

        public List<string> AvailableTransitions

        {

            get { return this.AvailableTransitionsValue; }

            set { SetProperty(ref AvailableTransitionsValue, value); }

        }
    }

    /// <summary>
    /// Entity state history response
    /// </summary>
    public class EntityStateHistoryResponse : ModelEntityBase
    {
        private string EntityIdValue = string.Empty;

        public string EntityId

        {

            get { return this.EntityIdValue; }

            set { SetProperty(ref EntityIdValue, value); }

        }
        private string EntityTypeValue = string.Empty;

        public string EntityType

        {

            get { return this.EntityTypeValue; }

            set { SetProperty(ref EntityTypeValue, value); }

        }
        private string CurrentStateValue = string.Empty;

        public string CurrentState

        {

            get { return this.CurrentStateValue; }

            set { SetProperty(ref CurrentStateValue, value); }

        }
        private List<StateHistoryEntry> HistoryValue = new List<StateHistoryEntry>();

        public List<StateHistoryEntry> History

        {

            get { return this.HistoryValue; }

            set { SetProperty(ref HistoryValue, value); }

        }
    }

    /// <summary>
    /// State history entry
    /// </summary>
    public class StateHistoryEntry : ModelEntityBase
    {
        private DateTime DateValue;

        public DateTime Date

        {

            get { return this.DateValue; }

            set { SetProperty(ref DateValue, value); }

        }
        private string StatusValue = string.Empty;

        public string Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }
        private string ChangedByValue = string.Empty;

        public string ChangedBy

        {

            get { return this.ChangedByValue; }

            set { SetProperty(ref ChangedByValue, value); }

        }
        private string NotesValue = string.Empty;

        public string Notes

        {

            get { return this.NotesValue; }

            set { SetProperty(ref NotesValue, value); }

        }
    }

    #endregion

    #region Process Database Entity DTOs

    /// <summary>
    /// Process definition database entity (DTO - used when PPDM model class doesn't exist)
    /// </summary>
    public class PROCESS_DEFINITION : ModelEntityBase
    {
        private string? PROCESS_DEFINITION_IDValue;

        public string? PROCESS_DEFINITION_ID

        {

            get { return this.PROCESS_DEFINITION_IDValue; }

            set { SetProperty(ref PROCESS_DEFINITION_IDValue, value); }

        }
        private string? PROCESS_NAMEValue;

        public string? PROCESS_NAME

        {

            get { return this.PROCESS_NAMEValue; }

            set { SetProperty(ref PROCESS_NAMEValue, value); }

        }
        private string? PROCESS_TYPEValue;

        public string? PROCESS_TYPE

        {

            get { return this.PROCESS_TYPEValue; }

            set { SetProperty(ref PROCESS_TYPEValue, value); }

        }
        private string? ENTITY_TYPEValue;

        public string? ENTITY_TYPE

        {

            get { return this.ENTITY_TYPEValue; }

            set { SetProperty(ref ENTITY_TYPEValue, value); }

        }
        private string? DESCRIPTIONValue;

        public string? DESCRIPTION

        {

            get { return this.DESCRIPTIONValue; }

            set { SetProperty(ref DESCRIPTIONValue, value); }

        }
        private string? PROCESS_CONFIG_JSONValue;

        public string? PROCESS_CONFIG_JSON

        {

            get { return this.PROCESS_CONFIG_JSONValue; }

            set { SetProperty(ref PROCESS_CONFIG_JSONValue, value); }

        }

    }

    /// <summary>
    /// Process instance database entity (DTO - used when PPDM model class doesn't exist)
    /// </summary>
    public class PROCESS_INSTANCE : ModelEntityBase
    {
        private string? PROCESS_INSTANCE_IDValue;

        public string? PROCESS_INSTANCE_ID

        {

            get { return this.PROCESS_INSTANCE_IDValue; }

            set { SetProperty(ref PROCESS_INSTANCE_IDValue, value); }

        }
        private string? PROCESS_DEFINITION_IDValue;

        public string? PROCESS_DEFINITION_ID

        {

            get { return this.PROCESS_DEFINITION_IDValue; }

            set { SetProperty(ref PROCESS_DEFINITION_IDValue, value); }

        }
        private string? ENTITY_IDValue;

        public string? ENTITY_ID

        {

            get { return this.ENTITY_IDValue; }

            set { SetProperty(ref ENTITY_IDValue, value); }

        }
        private string? ENTITY_TYPEValue;

        public string? ENTITY_TYPE

        {

            get { return this.ENTITY_TYPEValue; }

            set { SetProperty(ref ENTITY_TYPEValue, value); }

        }
        private string? FIELD_IDValue;

        public string? FIELD_ID

        {

            get { return this.FIELD_IDValue; }

            set { SetProperty(ref FIELD_IDValue, value); }

        }
        private string? CURRENT_STATEValue;

        public string? CURRENT_STATE

        {

            get { return this.CURRENT_STATEValue; }

            set { SetProperty(ref CURRENT_STATEValue, value); }

        }
        private string? CURRENT_STEP_IDValue;

        public string? CURRENT_STEP_ID

        {

            get { return this.CURRENT_STEP_IDValue; }

            set { SetProperty(ref CURRENT_STEP_IDValue, value); }

        }
        private string? STATUSValue;

        public string? STATUS

        {

            get { return this.STATUSValue; }

            set { SetProperty(ref STATUSValue, value); }

        }
        private DateTime? START_DATEValue;

        public DateTime? START_DATE

        {

            get { return this.START_DATEValue; }

            set { SetProperty(ref START_DATEValue, value); }

        }
        private DateTime? COMPLETION_DATEValue;

        public DateTime? COMPLETION_DATE

        {

            get { return this.COMPLETION_DATEValue; }

            set { SetProperty(ref COMPLETION_DATEValue, value); }

        }
        private string? STARTED_BYValue;

        public string? STARTED_BY

        {

            get { return this.STARTED_BYValue; }

            set { SetProperty(ref STARTED_BYValue, value); }

        }
        private string? PROCESS_DATA_JSONValue;

        public string? PROCESS_DATA_JSON

        {

            get { return this.PROCESS_DATA_JSONValue; }

            set { SetProperty(ref PROCESS_DATA_JSONValue, value); }

        }

    }

    /// <summary>
    /// Process step instance database entity (DTO - used when PPDM model class doesn't exist)
    /// </summary>
    public class PROCESS_STEP_INSTANCE : ModelEntityBase
    {
        private string? PROCESS_STEP_INSTANCE_IDValue;

        public string? PROCESS_STEP_INSTANCE_ID

        {

            get { return this.PROCESS_STEP_INSTANCE_IDValue; }

            set { SetProperty(ref PROCESS_STEP_INSTANCE_IDValue, value); }

        }
        private string? PROCESS_INSTANCE_IDValue;

        public string? PROCESS_INSTANCE_ID

        {

            get { return this.PROCESS_INSTANCE_IDValue; }

            set { SetProperty(ref PROCESS_INSTANCE_IDValue, value); }

        }
        private string? STEP_IDValue;

        public string? STEP_ID

        {

            get { return this.STEP_IDValue; }

            set { SetProperty(ref STEP_IDValue, value); }

        }
        private int? SEQUENCE_NUMBERValue;

        public int? SEQUENCE_NUMBER

        {

            get { return this.SEQUENCE_NUMBERValue; }

            set { SetProperty(ref SEQUENCE_NUMBERValue, value); }

        }
        private string? STATUSValue;

        public string? STATUS

        {

            get { return this.STATUSValue; }

            set { SetProperty(ref STATUSValue, value); }

        }
        private DateTime? START_DATEValue;

        public DateTime? START_DATE

        {

            get { return this.START_DATEValue; }

            set { SetProperty(ref START_DATEValue, value); }

        }
        private DateTime? COMPLETION_DATEValue;

        public DateTime? COMPLETION_DATE

        {

            get { return this.COMPLETION_DATEValue; }

            set { SetProperty(ref COMPLETION_DATEValue, value); }

        }
        private string? COMPLETED_BYValue;

        public string? COMPLETED_BY

        {

            get { return this.COMPLETED_BYValue; }

            set { SetProperty(ref COMPLETED_BYValue, value); }

        }
        private string? STEP_DATA_JSONValue;

        public string? STEP_DATA_JSON

        {

            get { return this.STEP_DATA_JSONValue; }

            set { SetProperty(ref STEP_DATA_JSONValue, value); }

        }
        private string? OUTCOMEValue;

        public string? OUTCOME

        {

            get { return this.OUTCOMEValue; }

            set { SetProperty(ref OUTCOMEValue, value); }

        }
        private string? NOTESValue;

        public string? NOTES

        {

            get { return this.NOTESValue; }

            set { SetProperty(ref NOTESValue, value); }

        }

    }

    /// <summary>
    /// Process history database entity (DTO - used when PPDM model class doesn't exist)
    /// </summary>
    public class PROCESS_HISTORY : ModelEntityBase
    {
        private string? PROCESS_HISTORY_IDValue;

        public string? PROCESS_HISTORY_ID

        {

            get { return this.PROCESS_HISTORY_IDValue; }

            set { SetProperty(ref PROCESS_HISTORY_IDValue, value); }

        }
        private string? PROCESS_INSTANCE_IDValue;

        public string? PROCESS_INSTANCE_ID

        {

            get { return this.PROCESS_INSTANCE_IDValue; }

            set { SetProperty(ref PROCESS_INSTANCE_IDValue, value); }

        }
        private string? PROCESS_STEP_INSTANCE_IDValue;

        public string? PROCESS_STEP_INSTANCE_ID

        {

            get { return this.PROCESS_STEP_INSTANCE_IDValue; }

            set { SetProperty(ref PROCESS_STEP_INSTANCE_IDValue, value); }

        }
        private string? ACTIONValue;

        public string? ACTION

        {

            get { return this.ACTIONValue; }

            set { SetProperty(ref ACTIONValue, value); }

        }
        private string? PREVIOUS_STATEValue;

        public string? PREVIOUS_STATE

        {

            get { return this.PREVIOUS_STATEValue; }

            set { SetProperty(ref PREVIOUS_STATEValue, value); }

        }
        private string? NEW_STATEValue;

        public string? NEW_STATE

        {

            get { return this.NEW_STATEValue; }

            set { SetProperty(ref NEW_STATEValue, value); }

        }
        private DateTime? ACTION_DATEValue;

        public DateTime? ACTION_DATE

        {

            get { return this.ACTION_DATEValue; }

            set { SetProperty(ref ACTION_DATEValue, value); }

        }
        private string? PERFORMED_BYValue;

        public string? PERFORMED_BY

        {

            get { return this.PERFORMED_BYValue; }

            set { SetProperty(ref PERFORMED_BYValue, value); }

        }
        private string? NOTESValue;

        public string? NOTES

        {

            get { return this.NOTESValue; }

            set { SetProperty(ref NOTESValue, value); }

        }
        private string? ACTION_DATA_JSONValue;

        public string? ACTION_DATA_JSON

        {

            get { return this.ACTION_DATA_JSONValue; }

            set { SetProperty(ref ACTION_DATA_JSONValue, value); }

        }

    }

    /// <summary>
    /// Process approval database entity (DTO - used when PPDM model class doesn't exist)
    /// </summary>
    public class PROCESS_APPROVAL : ModelEntityBase
    {
        private string? PROCESS_APPROVAL_IDValue;

        public string? PROCESS_APPROVAL_ID

        {

            get { return this.PROCESS_APPROVAL_IDValue; }

            set { SetProperty(ref PROCESS_APPROVAL_IDValue, value); }

        }
        private string? PROCESS_STEP_INSTANCE_IDValue;

        public string? PROCESS_STEP_INSTANCE_ID

        {

            get { return this.PROCESS_STEP_INSTANCE_IDValue; }

            set { SetProperty(ref PROCESS_STEP_INSTANCE_IDValue, value); }

        }
        private string? APPROVAL_TYPEValue;

        public string? APPROVAL_TYPE

        {

            get { return this.APPROVAL_TYPEValue; }

            set { SetProperty(ref APPROVAL_TYPEValue, value); }

        }
        private string? APPROVAL_STATUSValue;

        public string? APPROVAL_STATUS

        {

            get { return this.APPROVAL_STATUSValue; }

            set { SetProperty(ref APPROVAL_STATUSValue, value); }

        }
        private DateTime? REQUESTED_DATEValue;

        public DateTime? REQUESTED_DATE

        {

            get { return this.REQUESTED_DATEValue; }

            set { SetProperty(ref REQUESTED_DATEValue, value); }

        }
        private string? REQUESTED_BYValue;

        public string? REQUESTED_BY

        {

            get { return this.REQUESTED_BYValue; }

            set { SetProperty(ref REQUESTED_BYValue, value); }

        }
        private DateTime? APPROVED_DATEValue;

        public DateTime? APPROVED_DATE

        {

            get { return this.APPROVED_DATEValue; }

            set { SetProperty(ref APPROVED_DATEValue, value); }

        }
        private string? APPROVED_BYValue;

        public string? APPROVED_BY

        {

            get { return this.APPROVED_BYValue; }

            set { SetProperty(ref APPROVED_BYValue, value); }

        }
        private string? APPROVAL_NOTESValue;

        public string? APPROVAL_NOTES

        {

            get { return this.APPROVAL_NOTESValue; }

            set { SetProperty(ref APPROVAL_NOTESValue, value); }

        }

    }

    #endregion
}


