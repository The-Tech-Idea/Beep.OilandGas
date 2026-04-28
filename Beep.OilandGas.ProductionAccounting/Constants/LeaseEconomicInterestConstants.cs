namespace Beep.OilandGas.ProductionAccounting.Constants
{
    /// <summary>Rules for validating summed <c>OWNERSHIP_INTEREST</c> / <c>ROYALTY_INTEREST</c> fractions on a lease.</summary>
    public static class LeaseEconomicInterestValidation
    {
        /// <summary>100% expressed as a unit fraction.</summary>
        public const decimal FullInterestFraction = 1.0m;

        /// <summary>Slack when comparing summed interests to <see cref="FullInterestFraction"/>.</summary>
        public const decimal SumToleranceEpsilon = 0.0001m;
    }

    /// <summary>Normalizing persisted WI/NRI/ORI values that may be stored as 0–1 fractions or as whole-number percents.</summary>
    public static class LeaseEconomicInterestFractionRules
    {
        /// <summary>Values strictly greater than this are divided by <see cref="PercentScaleDivisor"/> (e.g. 50 → 0.50).</summary>
        public const decimal PercentVersusFractionThreshold = 1.0m;

        public const decimal PercentScaleDivisor = 100m;
    }
}
