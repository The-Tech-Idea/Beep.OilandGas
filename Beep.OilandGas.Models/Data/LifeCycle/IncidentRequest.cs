using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.LifeCycle
{
    public class IncidentRequest : ModelEntityBase
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
        private DateTime IncidentDateValue;

        public DateTime IncidentDate

        {

            get { return this.IncidentDateValue; }

            set { SetProperty(ref IncidentDateValue, value); }

        }
        private string IncidentTypeValue = string.Empty;

        public string IncidentType

        {

            get { return this.IncidentTypeValue; }

            set { SetProperty(ref IncidentTypeValue, value); }

        } // SAFETY, ENVIRONMENTAL, OPERATIONAL, EQUIPMENT
        private string SeverityValue = string.Empty;

        public string Severity

        {

            get { return this.SeverityValue; }

            set { SetProperty(ref SeverityValue, value); }

        } // LOW, MEDIUM, HIGH, CRITICAL
        private string? DescriptionValue;

        public string? Description

        {

            get { return this.DescriptionValue; }

            set { SetProperty(ref DescriptionValue, value); }

        }
        private string? ReportedByValue;

        public string? ReportedBy

        {

            get { return this.ReportedByValue; }

            set { SetProperty(ref ReportedByValue, value); }

        }
        public Dictionary<string, object>? IncidentData { get; set; }
    }
}
