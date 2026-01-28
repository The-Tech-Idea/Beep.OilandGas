using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data.Validation;
using Beep.OilandGas.Models.Data.Process;

namespace Beep.OilandGas.Models.Data
{
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
}
