using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data.Validation;
using Beep.OilandGas.Models.Data.Process;

namespace Beep.OilandGas.Models.Data
{
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
}
