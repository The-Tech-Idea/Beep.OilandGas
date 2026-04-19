using System;

namespace Beep.OilandGas.Models.Data
{
    public class ProcessInstanceSummary : ModelEntityBase
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

        private string ProcessTypeValue = string.Empty;

        public string ProcessType
        {
            get { return this.ProcessTypeValue; }
            set { SetProperty(ref ProcessTypeValue, value); }
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
        }

        private DateTime StartedAtValue = DateTime.UtcNow;

        public DateTime StartedAt
        {
            get { return this.StartedAtValue; }
            set { SetProperty(ref StartedAtValue, value); }
        }

        private string StartedByValue = string.Empty;

        public string StartedBy
        {
            get { return this.StartedByValue; }
            set { SetProperty(ref StartedByValue, value); }
        }

        private DateTime? CompletedAtValue;

        public DateTime? CompletedAt
        {
            get { return this.CompletedAtValue; }
            set { SetProperty(ref CompletedAtValue, value); }
        }
    }
}
