using Beep.OilandGas.Models;

namespace Beep.OilandGas.Models.Data.Analytics
{
    // ── Date range filter ─────────────────────────────────────────────────────
    public record DateRangeFilter(DateTime From, DateTime To)
    {
        public static DateRangeFilter Last30Days  => new(DateTime.UtcNow.AddDays(-30),  DateTime.UtcNow);
        public static DateRangeFilter Last90Days  => new(DateTime.UtcNow.AddDays(-90),  DateTime.UtcNow);
        public static DateRangeFilter Last365Days => new(DateTime.UtcNow.AddDays(-365), DateTime.UtcNow);
        public static DateRangeFilter LastNDays(int n) => new(DateTime.UtcNow.AddDays(-n), DateTime.UtcNow);
    }

    // ── KPI result records ────────────────────────────────────────────────────

    public record WorkOrderKPISet(
        double CompletionRate,
        double OnTimeCompletionRate,
        double MeanTimeToCompleteHours,
        int    OverdueCount,
        double ContractorUtilizationRate);

    public record GateReviewKPISet(
        double AverageCycleTimeDays,
        double PassRate,
        double AverageApproverCount,
        int    PendingCount);

    public record HSEKPISet(
        double Tier1PSERate,
        double Tier2PSERate,
        double TRIR,
        double NearMissReportingRate,
        double MeanDaysToCloseCA);

    public record ComplianceKPISet(
        double ObligationOnTimeRate,
        int    OverdueObligations,
        double RoyaltyPaymentTimeliness);

    public record ProductionKPISet(
        double MonthlyGrossBOE,
        double ProductionDeclineRatePct,
        double AverageUptimeRatePct);

    public record AnalyticsDashboardSummary(
        WorkOrderKPISet  WorkOrder,
        GateReviewKPISet GateReview,
        HSEKPISet        HSE,
        ComplianceKPISet Compliance,
        ProductionKPISet Production,
        DateTime         GeneratedAt);

    // ── Trend data ────────────────────────────────────────────────────────────

    public class KPITrendPoint : ModelEntityBase
    {
        public string   PeriodLabel { get; set; } = string.Empty;   // e.g. "2025-Q1"
        public DateTime PeriodDate  { get; set; }
        public double   Value       { get; set; }
        public string   SeriesName  { get; set; } = string.Empty;
    }

    // ── Reserves maturation ───────────────────────────────────────────────────

    public class ReservesMaturationSummary : ModelEntityBase
    {
        public string FieldId            { get; set; } = string.Empty;
        public string FieldName          { get; set; } = string.Empty;
        public double ProspectiveMMBOE   { get; set; }
        public double ContingentMMBOE    { get; set; }
        public double ProvedMMBOE        { get; set; }
        public double MaturationRatePct  { get; set; }  // Contingent / (Prospective + Contingent) × 100
        public DateTime AsOfDate         { get; set; }
    }
}
