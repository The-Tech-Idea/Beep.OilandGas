namespace Beep.OilandGas.ProductionAccounting.Constants
{
    /// <summary>
    /// ASC 606-style revenue recognition lifecycle codes; aligned with
    /// <c>R_PRODUCTION_ACCOUNTING_REFERENCE_CODE</c> REFERENCE_SET <c>REVENUE_RECOGNITION_STATUS</c>.
    /// </summary>
    public static class RevenueRecognitionStatusCodes
    {
        public const string Deferred = "DEFERRED";
        public const string Recognized = "RECOGNIZED";
        public const string Billed = "BILLED";
        public const string Collected = "COLLECTED";
    }
}
