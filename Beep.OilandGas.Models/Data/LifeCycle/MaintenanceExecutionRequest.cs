using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.LifeCycle
{
    public class MaintenanceExecutionRequest : ModelEntityBase
    {
        private string MaintenanceIdValue = string.Empty;

        public string MaintenanceId

        {

            get { return this.MaintenanceIdValue; }

            set { SetProperty(ref MaintenanceIdValue, value); }

        }
        private DateTime ExecutionDateValue;

        public DateTime ExecutionDate

        {

            get { return this.ExecutionDateValue; }

            set { SetProperty(ref ExecutionDateValue, value); }

        }
        private string? ExecutedByValue;

        public string? ExecutedBy

        {

            get { return this.ExecutedByValue; }

            set { SetProperty(ref ExecutedByValue, value); }

        }
        private string? WorkPerformedValue;

        public string? WorkPerformed

        {

            get { return this.WorkPerformedValue; }

            set { SetProperty(ref WorkPerformedValue, value); }

        }
        private string? StatusValue;

        public string? Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }
        public Dictionary<string, object>? ExecutionData { get; set; }
    }
}
