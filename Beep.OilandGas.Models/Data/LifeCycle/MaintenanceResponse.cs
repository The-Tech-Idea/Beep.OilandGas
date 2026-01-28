using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.LifeCycle
{
    public class MaintenanceResponse : ModelEntityBase
    {
        private string MaintenanceIdValue = string.Empty;

        public string MaintenanceId

        {

            get { return this.MaintenanceIdValue; }

            set { SetProperty(ref MaintenanceIdValue, value); }

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
        private string MaintenanceTypeValue = string.Empty;

        public string MaintenanceType

        {

            get { return this.MaintenanceTypeValue; }

            set { SetProperty(ref MaintenanceTypeValue, value); }

        }
        private DateTime? ScheduledDateValue;

        public DateTime? ScheduledDate

        {

            get { return this.ScheduledDateValue; }

            set { SetProperty(ref ScheduledDateValue, value); }

        }
        private DateTime? CompletedDateValue;

        public DateTime? CompletedDate

        {

            get { return this.CompletedDateValue; }

            set { SetProperty(ref CompletedDateValue, value); }

        }
        private string? StatusValue;

        public string? Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }
        private string? WorkOrderIdValue;

        public string? WorkOrderId

        {

            get { return this.WorkOrderIdValue; }

            set { SetProperty(ref WorkOrderIdValue, value); }

        } // Associated work order ID if created
        public Dictionary<string, object>? MaintenanceData { get; set; }
    }
}
