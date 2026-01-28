using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data.Validation;
using Beep.OilandGas.Models.Data.Process;

namespace Beep.OilandGas.Models.Data
{
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
}
