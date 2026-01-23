using System;
using System.Collections.Generic;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.DataManagement
{
    /// <summary>
    /// Workflow step definition
    /// </summary>
    public class WorkflowStep : ModelEntityBase
    {
        private string StepIdValue = Guid.NewGuid().ToString();

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
        private string? DependsOnValue;

        public string? DependsOn

        {

            get { return this.DependsOnValue; }

            set { SetProperty(ref DependsOnValue, value); }

        } // StepId this step depends on
        private bool CanRunInParallelValue = false;

        public bool CanRunInParallel

        {

            get { return this.CanRunInParallelValue; }

            set { SetProperty(ref CanRunInParallelValue, value); }

        }
        private int EstimatedWeightValue = 1;

        public int EstimatedWeight

        {

            get { return this.EstimatedWeightValue; }

            set { SetProperty(ref EstimatedWeightValue, value); }

        } // Weight for progress calculation
        private string OperationTypeValue = string.Empty;

        public string OperationType

        {

            get { return this.OperationTypeValue; }

            set { SetProperty(ref OperationTypeValue, value); }

        } // "ImportCsv", "Validate", "QualityCheck", etc.
        public Dictionary<string, object>? Parameters { get; set; }
    }

    /// <summary>
    /// Workflow definition
    /// </summary>
    public class WorkflowDefinition : ModelEntityBase
    {
        private string WorkflowIdValue = Guid.NewGuid().ToString();

        public string WorkflowId

        {

            get { return this.WorkflowIdValue; }

            set { SetProperty(ref WorkflowIdValue, value); }

        }
        private string WorkflowNameValue = string.Empty;

        public string WorkflowName

        {

            get { return this.WorkflowNameValue; }

            set { SetProperty(ref WorkflowNameValue, value); }

        }
        private string? DescriptionValue;

        public string? Description

        {

            get { return this.DescriptionValue; }

            set { SetProperty(ref DescriptionValue, value); }

        }
        private List<WorkflowStep> StepsValue = new List<WorkflowStep>();

        public List<WorkflowStep> Steps

        {

            get { return this.StepsValue; }

            set { SetProperty(ref StepsValue, value); }

        }
        private bool StopOnErrorValue = true;

        public bool StopOnError

        {

            get { return this.StopOnErrorValue; }

            set { SetProperty(ref StopOnErrorValue, value); }

        }
        private string? ConnectionNameValue;

        public string? ConnectionName

        {

            get { return this.ConnectionNameValue; }

            set { SetProperty(ref ConnectionNameValue, value); }

        }
        private string UserIdValue = string.Empty;

        public string UserId

        {

            get { return this.UserIdValue; }

            set { SetProperty(ref UserIdValue, value); }

        }
        private string? PhaseValue;

        public string? Phase

        {

            get { return this.PhaseValue; }

            set { SetProperty(ref PhaseValue, value); }

        } // EXPLORATION, DEVELOPMENT, PRODUCTION, DECOMMISSIONING
        private string? FieldIdValue;

        public string? FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        } // Field this workflow belongs to
        public Dictionary<string, object>? Parameters { get; set; } // Additional parameters including FIELD_ID, PHASE
    }

    /// <summary>
    /// Workflow execution request
    /// </summary>
    public class WorkflowExecutionRequest : ModelEntityBase
    {
        private WorkflowDefinition WorkflowValue = new WorkflowDefinition();

        public WorkflowDefinition Workflow

        {

            get { return this.WorkflowValue; }

            set { SetProperty(ref WorkflowValue, value); }

        }
        private string? OperationIdValue;

        public string? OperationId

        {

            get { return this.OperationIdValue; }

            set { SetProperty(ref OperationIdValue, value); }

        }
    }

    /// <summary>
    /// Workflow execution result
    /// </summary>
    public class WorkflowExecutionResult : ModelEntityBase
    {
        private string WorkflowIdValue = string.Empty;

        public string WorkflowId

        {

            get { return this.WorkflowIdValue; }

            set { SetProperty(ref WorkflowIdValue, value); }

        }
        private string OperationIdValue = string.Empty;

        public string OperationId

        {

            get { return this.OperationIdValue; }

            set { SetProperty(ref OperationIdValue, value); }

        }
        private bool SuccessValue;

        public bool Success

        {

            get { return this.SuccessValue; }

            set { SetProperty(ref SuccessValue, value); }

        }
        private string? ErrorMessageValue;

        public string? ErrorMessage

        {

            get { return this.ErrorMessageValue; }

            set { SetProperty(ref ErrorMessageValue, value); }

        }
        public Dictionary<string, object>? StepResults { get; set; }
        private DateTime StartedAtValue;

        public DateTime StartedAt

        {

            get { return this.StartedAtValue; }

            set { SetProperty(ref StartedAtValue, value); }

        }
        private DateTime? CompletedAtValue;

        public DateTime? CompletedAt

        {

            get { return this.CompletedAtValue; }

            set { SetProperty(ref CompletedAtValue, value); }

        }
        private TimeSpan? DurationValue;

        public TimeSpan? Duration

        {

            get { return this.DurationValue; }

            set { SetProperty(ref DurationValue, value); }

        }
    }
}








