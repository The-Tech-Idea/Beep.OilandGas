namespace Beep.OilandGas.Accounting.Constants
{
    /// <summary>
    /// Default GL account IDs used for common postings.
    /// These align with the conventions used across Accounting services.
    /// </summary>
    public static class DefaultGlAccounts
    {
        public const string Cash = "1000";
        public const string AccountsReceivable = "1110";
        public const string Inventory = "1300";
        public const string AccountsPayable = "2000";
        public const string AccruedRoyalties = "2200";
        public const string RetainedEarnings = "3000";
        public const string Revenue = "4001";
        public const string CostOfGoodsSold = "5000";
        public const string RoyaltyExpense = "6100";
    }
}
