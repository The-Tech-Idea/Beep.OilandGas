using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data.Validation;
using Beep.OilandGas.Models.Data.Process;

namespace Beep.OilandGas.Models.Data
{
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
}
