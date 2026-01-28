using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.LifeCycle
{
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
}
