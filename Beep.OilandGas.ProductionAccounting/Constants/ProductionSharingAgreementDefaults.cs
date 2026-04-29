namespace Beep.OilandGas.ProductionAccounting.Constants
{
    /// <summary>
    /// Default unit fractions for PSA cost-oil / profit-oil entitlement when agreement percent columns are null,
    /// or when resolved government and contractor profit splits both net to zero (equal fallback).
    /// </summary>
    public static class PsaEntitlementCalculationDefaults
    {
        /// <summary>Default <c>COST_RECOVERY_LIMIT_PCT</c> when unspecified (60% as 0–1 fraction).</summary>
        public const decimal DefaultCostRecoveryLimitFraction = 0.6m;

        /// <summary>Default <c>GOVERNMENT_PROFIT_SPLIT_PCT</c> when unspecified (50% as 0–1 fraction).</summary>
        public const decimal DefaultGovernmentProfitSplitFraction = 0.5m;
    }
}
