namespace Beep.OilandGas.ProductionAccounting.Constants
{
    /// <summary>
    /// Heuristic leading digit of <c>GL_ACCOUNT_ID</c> when summarizing GL into revenue vs expense buckets
    /// for simplified <c>FINANCIAL_REPORT</c> rollups (common 4xx / 5xx / 6xx layout).
    /// </summary>
    public static class FinancialReportGlRollupAccountPrefixes
    {
        public const string Revenue = "4";
        public const string OperatingExpensePrimary = "5";
        public const string OperatingExpenseSecondary = "6";
    }

    /// <summary>
    /// Placeholder tax math for <c>FINANCIAL_REPORT</c> when <see cref="GeneratedReportTypeCodes.Tax"/> is selected (illustrative only, not jurisdiction-specific).
    /// </summary>
    public static class FinancialReportTaxVariantPlaceholders
    {
        public const decimal IllustrativeCorporateIncomeTaxRate = 0.21m;
    }

    /// <summary>User-facing validation text for reporting guard clauses.</summary>
    public static class ReportingServiceExceptionMessages
    {
        public const string RoyaltyOwnerBaIdRequired = "Royalty owner BA ID is required";
        public const string LeaseIdRequired = "Lease ID is required";
    }

    /// <summary><c>OPERATIONAL_REPORT.REMARK</c> invariant format: ticket count, total net volume.</summary>
    public static class ReportingRemarkFormats
    {
        public const string OperationalRunTicketSummaryFormat = "RunTickets={0}, TotalNetVolume={1}";
    }
}
