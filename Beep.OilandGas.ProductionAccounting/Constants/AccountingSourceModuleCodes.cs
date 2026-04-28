namespace Beep.OilandGas.ProductionAccounting.Constants
{
    /// <summary>
    /// <c>JOURNAL_ENTRY.SOURCE_MODULE</c> and related provenance codes; aligned with
    /// <c>R_PRODUCTION_ACCOUNTING_REFERENCE_CODE</c> REFERENCE_SET <c>ACCOUNTING_SOURCE_MODULE</c>.
    /// </summary>
    public static class AccountingSourceModuleCodes
    {
        public const string PeriodClosing = "PERIOD_CLOSING";

        /// <summary>Provenance for farm-in / farm-out and property exchange journal pipelines.</summary>
        public const string AssetSwap = "ASSET_SWAP";
    }
}
