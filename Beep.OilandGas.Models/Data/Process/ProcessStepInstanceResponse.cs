using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data.Validation;
using Beep.OilandGas.Models.Data.Process;

namespace Beep.OilandGas.Models.Data
{
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
}
