namespace Beep.OilandGas.ProductionAccounting.Constants
{
    /// <summary>
    /// Royalty status constants for tracking lifecycle of royalty calculations
    /// </summary>
    public static class RoyaltyStatus
    {
        /// <summary>
        /// Royalty amount has been calculated but not yet accrued
        /// </summary>
        public const string Calculated = "CALCULATED";

        /// <summary>
        /// Royalty amount has been accrued (recorded in accounts payable)
        /// </summary>
        public const string Accrued = "ACCRUED";

        /// <summary>
        /// Royalty payment has been made
        /// </summary>
        public const string Paid = "PAID";

        /// <summary>
        /// Royalty calculation has been reversed (adjustment/correction)
        /// </summary>
        public const string Reversed = "REVERSED";

        /// <summary>
        /// Royalty is in dispute with owner or regulatory authority
        /// </summary>
        public const string Disputed = "DISPUTED";

        /// <summary>
        /// Royalty is pending approval or verification
        /// </summary>
        public const string Pending = "PENDING";
    }
}
