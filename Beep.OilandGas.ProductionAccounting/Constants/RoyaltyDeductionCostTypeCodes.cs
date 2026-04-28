namespace Beep.OilandGas.ProductionAccounting.Constants
{
    /// <summary>
    /// <c>ACCOUNTING_COST.COST_TYPE</c> values (and substring tokens) used when netting transportation and production taxes from royalty bases.
    /// Rows are seeded under <c>COST_TYPE</c> via <see cref="ProductionAccountingReferenceCodeSeed"/>.
    /// </summary>
    public static class RoyaltyDeductionCostTypeCodes
    {
        public const string Transportation = "TRANSPORTATION";
        public const string AdValorem = "AD_VALOREM";
        public const string Severance = "SEVERANCE";

        /// <summary>Case-insensitive <c>Contains</c> token for transport-related cost types.</summary>
        public const string TransportContainsToken = "TRANSPORT";

        /// <summary>Case-insensitive <c>Contains</c> token for ad valorem-related cost types.</summary>
        public const string AdValoremContainsToken = "VALOREM";

        /// <summary>Case-insensitive <c>Contains</c> token for severance-related cost types.</summary>
        public const string SeveranceContainsToken = "SEVERANCE";
    }
}
