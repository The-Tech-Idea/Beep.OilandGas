namespace Beep.OilandGas.ProductionAccounting.Constants
{
    /// <summary>ASC 932 numeric fallbacks when proved-reserve PV or volumes are missing (policy, not LOV-seeded).</summary>
    public static class FullCostCalculationFallbacks
    {
        /// <summary>Multiplier on capitalized cost when reserve PV cannot be estimated for the ceiling test.</summary>
        public const decimal FairValueMultipleOfCapitalizedCostsWhenPvUnavailable = 1.2m;

        /// <summary>Fraction of capitalized cost used as depletion when proved reserves denominator is zero.</summary>
        public const decimal DepletionFractionOfCapitalizedCostsWhenReservesMissing = 0.08m;

        /// <summary>Single-step discount factor placeholder in simplified reserve PV helper.</summary>
        public const decimal SimpleReservePvDiscountFactor = 0.9091m;
    }

    /// <summary>Default <c>COST_TYPE</c> / <c>COST_CATEGORY</c> for generic full-cost pool capitalization rows (seeded LOVs).</summary>
    public static class FullCostRecordedCostDefaults
    {
        public static readonly string DefaultCostType = CostTypes.Exploration;

        /// <summary>Exploratory G&amp;G–style default; callers may supply a more specific category.</summary>
        public static readonly string DefaultCostCategory = CostCategories.Seismic;
    }
}
