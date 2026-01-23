using System;
using System.Collections.Generic;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.Reporting
{
    public class GenerateOperationalReportRequest : ModelEntityBase
    {
        private DateTime PeriodStartValue;

        public DateTime PeriodStart

        {

            get { return this.PeriodStartValue; }

            set { SetProperty(ref PeriodStartValue, value); }

        }
        private DateTime PeriodEndValue;

        public DateTime PeriodEnd

        {

            get { return this.PeriodEndValue; }

            set { SetProperty(ref PeriodEndValue, value); }

        }
        private List<string>? WellIdsValue;

        public List<string>? WellIds

        {

            get { return this.WellIdsValue; }

            set { SetProperty(ref WellIdsValue, value); }

        }
        private List<string>? LeaseIdsValue;

        public List<string>? LeaseIds

        {

            get { return this.LeaseIdsValue; }

            set { SetProperty(ref LeaseIdsValue, value); }

        }
    }

    public class GenerateFinancialReportRequest : ModelEntityBase
    {
        private DateTime PeriodStartValue;

        public DateTime PeriodStart

        {

            get { return this.PeriodStartValue; }

            set { SetProperty(ref PeriodStartValue, value); }

        }
        private DateTime PeriodEndValue;

        public DateTime PeriodEnd

        {

            get { return this.PeriodEndValue; }

            set { SetProperty(ref PeriodEndValue, value); }

        }
        private string ReportTypeValue;

        public string ReportType

        {

            get { return this.ReportTypeValue; }

            set { SetProperty(ref ReportTypeValue, value); }

        }
        private List<string>? PropertyIdsValue;

        public List<string>? PropertyIds

        {

            get { return this.PropertyIdsValue; }

            set { SetProperty(ref PropertyIdsValue, value); }

        }
    }

    public class GenerateRoyaltyStatementRequest : ModelEntityBase
    {
        private string RoyaltyOwnerBaIdValue;

        public string RoyaltyOwnerBaId

        {

            get { return this.RoyaltyOwnerBaIdValue; }

            set { SetProperty(ref RoyaltyOwnerBaIdValue, value); }

        }
        private DateTime PeriodStartValue;

        public DateTime PeriodStart

        {

            get { return this.PeriodStartValue; }

            set { SetProperty(ref PeriodStartValue, value); }

        }
        private DateTime PeriodEndValue;

        public DateTime PeriodEnd

        {

            get { return this.PeriodEndValue; }

            set { SetProperty(ref PeriodEndValue, value); }

        }
        private string PropertyIdValue;

        public string PropertyId

        {

            get { return this.PropertyIdValue; }

            set { SetProperty(ref PropertyIdValue, value); }

        }
    }

    public class GenerateJIBStatementRequest : ModelEntityBase
    {
        private string LeaseIdValue;

        public string LeaseId

        {

            get { return this.LeaseIdValue; }

            set { SetProperty(ref LeaseIdValue, value); }

        }
        private DateTime PeriodStartValue;

        public DateTime PeriodStart

        {

            get { return this.PeriodStartValue; }

            set { SetProperty(ref PeriodStartValue, value); }

        }
        private DateTime PeriodEndValue;

        public DateTime PeriodEnd

        {

            get { return this.PeriodEndValue; }

            set { SetProperty(ref PeriodEndValue, value); }

        }
    }

    public class ReportResult : ModelEntityBase
    {
        private string ReportIdValue;

        public string ReportId

        {

            get { return this.ReportIdValue; }

            set { SetProperty(ref ReportIdValue, value); }

        }
        private string ReportTypeValue;

        public string ReportType

        {

            get { return this.ReportTypeValue; }

            set { SetProperty(ref ReportTypeValue, value); }

        }
        private DateTime GeneratedDateValue = DateTime.UtcNow;

        public DateTime GeneratedDate

        {

            get { return this.GeneratedDateValue; }

            set { SetProperty(ref GeneratedDateValue, value); }

        }
        private string GeneratedByValue;

        public string GeneratedBy

        {

            get { return this.GeneratedByValue; }

            set { SetProperty(ref GeneratedByValue, value); }

        }
        private object ReportDataValue;

        public object ReportData

        {

            get { return this.ReportDataValue; }

            set { SetProperty(ref ReportDataValue, value); }

        }
        private string FormatValue = "JSON";

        public string Format

        {

            get { return this.FormatValue; }

            set { SetProperty(ref FormatValue, value); }

        }
    }

    public class ScheduleReportRequest : ModelEntityBase
    {
        private string ReportTypeValue;

        public string ReportType

        {

            get { return this.ReportTypeValue; }

            set { SetProperty(ref ReportTypeValue, value); }

        }
        private string ScheduleFrequencyValue;

        public string ScheduleFrequency

        {

            get { return this.ScheduleFrequencyValue; }

            set { SetProperty(ref ScheduleFrequencyValue, value); }

        }
        private DateTime StartDateValue;

        public DateTime StartDate

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
        public Dictionary<string, object> ReportParameters { get; set; } = new();
    }

    public class ReportSchedule : ModelEntityBase
    {
        private string ScheduleIdValue;

        public string ScheduleId

        {

            get { return this.ScheduleIdValue; }

            set { SetProperty(ref ScheduleIdValue, value); }

        }
        private string ReportTypeValue;

        public string ReportType

        {

            get { return this.ReportTypeValue; }

            set { SetProperty(ref ReportTypeValue, value); }

        }
        private string ScheduleFrequencyValue;

        public string ScheduleFrequency

        {

            get { return this.ScheduleFrequencyValue; }

            set { SetProperty(ref ScheduleFrequencyValue, value); }

        }
        private DateTime NextRunDateValue;

        public DateTime NextRunDate

        {

            get { return this.NextRunDateValue; }

            set { SetProperty(ref NextRunDateValue, value); }

        }
        private DateTime? LastRunDateValue;

        public DateTime? LastRunDate

        {

            get { return this.LastRunDateValue; }

            set { SetProperty(ref LastRunDateValue, value); }

        }
        private string StatusValue;

        public string Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }
    }

    public class ReportDistributionRequest : ModelEntityBase
    {
        private List<string> RecipientEmailsValue = new();

        public List<string> RecipientEmails

        {

            get { return this.RecipientEmailsValue; }

            set { SetProperty(ref RecipientEmailsValue, value); }

        }
        private string DistributionMethodValue = "Email";

        public string DistributionMethod

        {

            get { return this.DistributionMethodValue; }

            set { SetProperty(ref DistributionMethodValue, value); }

        }
        private string FormatValue = "PDF";

        public string Format

        {

            get { return this.FormatValue; }

            set { SetProperty(ref FormatValue, value); }

        }
    }

    public class ReportDistributionResult : ModelEntityBase
    {
        private string ReportIdValue;

        public string ReportId

        {

            get { return this.ReportIdValue; }

            set { SetProperty(ref ReportIdValue, value); }

        }
        private bool IsDistributedValue;

        public bool IsDistributed

        {

            get { return this.IsDistributedValue; }

            set { SetProperty(ref IsDistributedValue, value); }

        }
        private DateTime DistributionDateValue = DateTime.UtcNow;

        public DateTime DistributionDate

        {

            get { return this.DistributionDateValue; }

            set { SetProperty(ref DistributionDateValue, value); }

        }
        private int RecipientCountValue;

        public int RecipientCount

        {

            get { return this.RecipientCountValue; }

            set { SetProperty(ref RecipientCountValue, value); }

        }
        private List<string> DistributionErrorsValue = new();

        public List<string> DistributionErrors

        {

            get { return this.DistributionErrorsValue; }

            set { SetProperty(ref DistributionErrorsValue, value); }

        }
    }

    public class ReportHistory : ModelEntityBase
    {
        private string ReportIdValue;

        public string ReportId

        {

            get { return this.ReportIdValue; }

            set { SetProperty(ref ReportIdValue, value); }

        }
        private string ReportTypeValue;

        public string ReportType

        {

            get { return this.ReportTypeValue; }

            set { SetProperty(ref ReportTypeValue, value); }

        }
        private DateTime GeneratedDateValue;

        public DateTime GeneratedDate

        {

            get { return this.GeneratedDateValue; }

            set { SetProperty(ref GeneratedDateValue, value); }

        }
        private string GeneratedByValue;

        public string GeneratedBy

        {

            get { return this.GeneratedByValue; }

            set { SetProperty(ref GeneratedByValue, value); }

        }
        private DateTime PeriodStartValue;

        public DateTime PeriodStart

        {

            get { return this.PeriodStartValue; }

            set { SetProperty(ref PeriodStartValue, value); }

        }
        private DateTime PeriodEndValue;

        public DateTime PeriodEnd

        {

            get { return this.PeriodEndValue; }

            set { SetProperty(ref PeriodEndValue, value); }

        }
        private string StatusValue;

        public string Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }
    }
}








