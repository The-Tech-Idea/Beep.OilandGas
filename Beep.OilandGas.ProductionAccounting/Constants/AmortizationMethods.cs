namespace Beep.OilandGas.ProductionAccounting.Constants
{
    /// <summary>
    /// Amortization / depletion calculation methods (seeded under <c>AMORTIZATION_METHOD</c>).
    /// </summary>
    public static class AmortizationMethods
    {
        public const string UnitOfProduction = "UNIT-OF-PRODUCTION";
        public const string StraightLine = "STRAIGHT-LINE";
        public const string DoubleDeclining = "DOUBLE-DECLINING";

        /// <summary>All methods emitted to <c>R_PRODUCTION_ACCOUNTING_REFERENCE_CODE</c> for this family.</summary>
        public static readonly string[] AllSeeded = { UnitOfProduction, StraightLine, DoubleDeclining };
    }

    /// <summary>Numeric fallbacks when reserve-based UOP cannot be computed (policy, not LOV-seeded).</summary>
    public static class AmortizationCalculationFallbacks
    {
        /// <summary>Applied to capitalized cost when proved reserves are missing or depletion is still zero.</summary>
        public const decimal DepletionFactorOfCapitalizedCost = 0.06m;
    }
}

