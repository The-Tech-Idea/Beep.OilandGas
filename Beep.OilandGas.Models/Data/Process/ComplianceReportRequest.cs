using System;

namespace Beep.OilandGas.Models.Data
{
    public class ComplianceReportRequest : ModelEntityBase
    {
        private string JurisdictionTagValue = string.Empty;

        /// <summary>Jurisdiction code: USA, CANADA, INTERNATIONAL</summary>
        public string JurisdictionTag
        {
            get { return this.JurisdictionTagValue; }
            set { SetProperty(ref JurisdictionTagValue, value); }
        }

        private string ObligationTypeValue = string.Empty;

        public string ObligationType
        {
            get { return this.ObligationTypeValue; }
            set { SetProperty(ref ObligationTypeValue, value); }
        }

        private DateTime PeriodStartValue = DateTime.UtcNow;

        public DateTime PeriodStart
        {
            get { return this.PeriodStartValue; }
            set { SetProperty(ref PeriodStartValue, value); }
        }

        private DateTime PeriodEndValue = DateTime.UtcNow;

        public DateTime PeriodEnd
        {
            get { return this.PeriodEndValue; }
            set { SetProperty(ref PeriodEndValue, value); }
        }

        private string ReportDataValue = string.Empty;

        /// <summary>Serialized report payload or file reference</summary>
        public string ReportData
        {
            get { return this.ReportDataValue; }
            set { SetProperty(ref ReportDataValue, value); }
        }

        private string SubmissionReferenceValue = string.Empty;

        /// <summary>External regulator submission reference number</summary>
        public string SubmissionReference
        {
            get { return this.SubmissionReferenceValue; }
            set { SetProperty(ref SubmissionReferenceValue, value); }
        }
    }
}
