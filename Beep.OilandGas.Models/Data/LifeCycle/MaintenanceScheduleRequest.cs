using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.LifeCycle
{
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
}
