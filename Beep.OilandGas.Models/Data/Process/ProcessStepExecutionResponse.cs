using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data.Validation;
using Beep.OilandGas.Models.Data.Process;

namespace Beep.OilandGas.Models.Data
{
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
}
