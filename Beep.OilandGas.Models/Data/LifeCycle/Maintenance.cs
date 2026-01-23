using System;
using System.Collections.Generic;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.LifeCycle
{
    /// <summary>
    /// DTOs for Maintenance Management
    /// </summary>
    
    public class MaintenanceScheduleRequest : ModelEntityBase
    {
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

        } // WELL, FACILITY, PIPELINE, EQUIPMENT
        private string MaintenanceTypeValue = string.Empty;

        public string MaintenanceType

        {

            get { return this.MaintenanceTypeValue; }

            set { SetProperty(ref MaintenanceTypeValue, value); }

        } // PREVENTIVE, CORRECTIVE, EMERGENCY
        private DateTime ScheduledDateValue;

        public DateTime ScheduledDate

        {

            get { return this.ScheduledDateValue; }

            set { SetProperty(ref ScheduledDateValue, value); }

        }
        private string? DescriptionValue;

        public string? Description

        {

            get { return this.DescriptionValue; }

            set { SetProperty(ref DescriptionValue, value); }

        }
        private string? AssignedToValue;

        public string? AssignedTo

        {

            get { return this.AssignedToValue; }

            set { SetProperty(ref AssignedToValue, value); }

        }
        private bool? CreateWorkOrderValue;

        public bool? CreateWorkOrder

        {

            get { return this.CreateWorkOrderValue; }

            set { SetProperty(ref CreateWorkOrderValue, value); }

        } // Whether to create a work order for this maintenance
        public Dictionary<string, object>? ScheduleData { get; set; }
    }

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

    public class MaintenanceRequest : ModelEntityBase
    {
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
        private string PriorityValue = string.Empty;

        public string Priority

        {

            get { return this.PriorityValue; }

            set { SetProperty(ref PriorityValue, value); }

        } // LOW, MEDIUM, HIGH, URGENT
        private string? DescriptionValue;

        public string? Description

        {

            get { return this.DescriptionValue; }

            set { SetProperty(ref DescriptionValue, value); }

        }
        private string? RequestedByValue;

        public string? RequestedBy

        {

            get { return this.RequestedByValue; }

            set { SetProperty(ref RequestedByValue, value); }

        }
        public Dictionary<string, object>? RequestData { get; set; }
    }

    public class MaintenanceHistoryRequest : ModelEntityBase
    {
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
        private DateTime? StartDateValue;

        public DateTime? StartDate

        {

            get { return this.StartDateValue; }

            set { SetProperty(ref StartDateValue, value); }

        }
        private DateTime? EndDateValue;

        public DateTime? EndDate

        {

            get { return this.EndDateValue; }

            set { SetProperty(ref EndDateValue, value); }

        }
    }

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








