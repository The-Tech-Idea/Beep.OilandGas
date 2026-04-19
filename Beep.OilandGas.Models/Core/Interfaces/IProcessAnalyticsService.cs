using Beep.OilandGas.Models.Data.Analytics;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Field-level process performance analytics: KPI dashboards for cycle times,
    /// gate pass rates, HSE tier distribution, compliance fulfillment rates,
    /// and reserves maturation velocity.
    /// Standards: SPE PRMS §7, API RP 97, IOGP KPI Report 2022e, NI 51-101.
    /// </summary>
    public interface IProcessAnalyticsService
    {
        /// <summary>Work order completion, on-time rate, MTTC, overdue count.</summary>
        Task<WorkOrderKPISet> GetWorkOrderKPIsAsync(string fieldId, DateRangeFilter range);

        /// <summary>Gate review cycle time, pass rate, average approver count.</summary>
        Task<GateReviewKPISet> GetGateReviewKPIsAsync(string fieldId, DateRangeFilter range);

        /// <summary>
        /// HSE KPIs aligned to IOGP 2022e: Tier 1/2 PSE rates, TRIR,
        /// near-miss reporting rate, mean days to close CAs.
        /// </summary>
        Task<HSEKPISet> GetHSEKPIsAsync(string fieldId, DateRangeFilter range, double exposureHours);

        /// <summary>Compliance obligation on-time rate, overdue count, royalty timeliness.</summary>
        Task<ComplianceKPISet> GetComplianceKPIsAsync(string fieldId, DateRangeFilter range);

        /// <summary>Monthly gross BOE, decline rate, average uptime per well.</summary>
        Task<ProductionKPISet> GetProductionKPIsAsync(string fieldId, DateRangeFilter range);

        /// <summary>Combined dashboard — single call aggregating all five KPI sets.</summary>
        Task<AnalyticsDashboardSummary> GetDashboardSummaryAsync(
            string fieldId, DateRangeFilter range, double exposureHours);

        /// <summary>
        /// Monthly production trend data for charting.
        /// Returns series points for oil, gas (BOE), or combined as requested.
        /// </summary>
        Task<List<KPITrendPoint>> GetProductionTrendAsync(
            string fieldId, DateRangeFilter range, string seriesName = "BOE");

        /// <summary>
        /// PRMS reserves maturation summary: Prospective → Contingent → Proved volumes.
        /// Used for NI 51-101 / SEC 17 CFR 229 disclosures.
        /// </summary>
        Task<ReservesMaturationSummary> GetReservesMaturationAsync(string fieldId);
    }
}
