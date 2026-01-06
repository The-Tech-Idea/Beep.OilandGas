using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.DTOs.Reporting
{
    public class GenerateOperationalReportRequest
    {
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public List<string>? WellIds { get; set; }
        public List<string>? LeaseIds { get; set; }
    }

    public class GenerateFinancialReportRequest
    {
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public string ReportType { get; set; }
        public List<string>? PropertyIds { get; set; }
    }

    public class GenerateRoyaltyStatementRequest
    {
        public string RoyaltyOwnerBaId { get; set; }
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public string PropertyId { get; set; }
    }

    public class GenerateJIBStatementRequest
    {
        public string LeaseId { get; set; }
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
    }

    public class ReportResult
    {
        public string ReportId { get; set; }
        public string ReportType { get; set; }
        public DateTime GeneratedDate { get; set; } = DateTime.UtcNow;
        public string GeneratedBy { get; set; }
        public object ReportData { get; set; }
        public string Format { get; set; } = "JSON";
    }

    public class ScheduleReportRequest
    {
        public string ReportType { get; set; }
        public string ScheduleFrequency { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public Dictionary<string, object> ReportParameters { get; set; } = new();
    }

    public class ReportSchedule
    {
        public string ScheduleId { get; set; }
        public string ReportType { get; set; }
        public string ScheduleFrequency { get; set; }
        public DateTime NextRunDate { get; set; }
        public DateTime? LastRunDate { get; set; }
        public string Status { get; set; }
    }

    public class ReportDistributionRequest
    {
        public List<string> RecipientEmails { get; set; } = new();
        public string DistributionMethod { get; set; } = "Email";
        public string Format { get; set; } = "PDF";
    }

    public class ReportDistributionResult
    {
        public string ReportId { get; set; }
        public bool IsDistributed { get; set; }
        public DateTime DistributionDate { get; set; } = DateTime.UtcNow;
        public int RecipientCount { get; set; }
        public List<string> DistributionErrors { get; set; } = new();
    }

    public class ReportHistory
    {
        public string ReportId { get; set; }
        public string ReportType { get; set; }
        public DateTime GeneratedDate { get; set; }
        public string GeneratedBy { get; set; }
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public string Status { get; set; }
    }
}




