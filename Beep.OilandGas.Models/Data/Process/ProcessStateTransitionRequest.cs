using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data.Validation;
using Beep.OilandGas.Models.Data.Process;

namespace Beep.OilandGas.Models.Data
{
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
        public PROCESS_DATA TransitionData { get; set; } = new PROCESS_DATA();
    }
}
