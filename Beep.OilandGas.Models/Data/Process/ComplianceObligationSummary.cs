using System;

namespace Beep.OilandGas.Models.Data
{
    public class ComplianceObligationSummary : ModelEntityBase
    {
        private string ObligationIdValue = string.Empty;

        public string ObligationId
        {
            get { return this.ObligationIdValue; }
            set { SetProperty(ref ObligationIdValue, value); }
        }

        private string ObligationTypeValue = string.Empty;

        public string ObligationType
        {
            get { return this.ObligationTypeValue; }
            set { SetProperty(ref ObligationTypeValue, value); }
        }

        private string JurisdictionTagValue = string.Empty;

        public string JurisdictionTag
        {
            get { return this.JurisdictionTagValue; }
            set { SetProperty(ref JurisdictionTagValue, value); }
        }

        private DateTime DueDateValue = DateTime.UtcNow;

        public DateTime DueDate
        {
            get { return this.DueDateValue; }
            set { SetProperty(ref DueDateValue, value); }
        }

        private string StatusValue = string.Empty;

        public string Status
        {
            get { return this.StatusValue; }
            set { SetProperty(ref StatusValue, value); }
        }

        private string AssignedToValue = string.Empty;

        public string AssignedTo
        {
            get { return this.AssignedToValue; }
            set { SetProperty(ref AssignedToValue, value); }
        }

        private int DaysUntilDueValue;

        public int DaysUntilDue
        {
            get { return this.DaysUntilDueValue; }
            set { SetProperty(ref DaysUntilDueValue, value); }
        }
    }
}
