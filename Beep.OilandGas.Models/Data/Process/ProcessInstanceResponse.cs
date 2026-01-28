using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data.Validation;
using Beep.OilandGas.Models.Data.Process;

namespace Beep.OilandGas.Models.Data
{
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
}
