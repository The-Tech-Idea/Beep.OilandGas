using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data.Validation;
using Beep.OilandGas.Models.Data.Process;

namespace Beep.OilandGas.Models.Data
{
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
        public PROCESS_STEP_DATA StepData { get; set; } = new PROCESS_STEP_DATA();
    }
}
