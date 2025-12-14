namespace Beep.OilandGas.Accounting.Constants
{
    /// <summary>
    /// Constants used in oil and gas accounting calculations.
    /// </summary>
    public static class AccountingConstants
    {
        /// <summary>
        /// Gas to oil equivalent conversion factor (MCF per barrel).
        /// </summary>
        public const decimal GasToOilEquivalent = 6.0m;

        /// <summary>
        /// Minimum interest rate (0%).
        /// </summary>
        public const decimal MinInterestRate = 0.0m;

        /// <summary>
        /// Maximum interest rate (100%).
        /// </summary>
        public const decimal MaxInterestRate = 1.0m;

        /// <summary>
        /// Minimum reserves (barrels or MCF).
        /// </summary>
        public const decimal MinReserves = 0.0m;

        /// <summary>
        /// Minimum production (barrels or MCF).
        /// </summary>
        public const decimal MinProduction = 0.0m;

        /// <summary>
        /// Minimum cost amount.
        /// </summary>
        public const decimal MinCost = 0.0m;

        /// <summary>
        /// Maximum cost amount.
        /// </summary>
        public const decimal MaxCost = 1000000000000m; // 1 trillion

        /// <summary>
        /// Minimum working interest (0%).
        /// </summary>
        public const decimal MinWorkingInterest = 0.0m;

        /// <summary>
        /// Maximum working interest (100%).
        /// </summary>
        public const decimal MaxWorkingInterest = 1.0m;

        /// <summary>
        /// Minimum net revenue interest (0%).
        /// </summary>
        public const decimal MinNetRevenueInterest = 0.0m;

        /// <summary>
        /// Maximum net revenue interest (100%).
        /// </summary>
        public const decimal MaxNetRevenueInterest = 1.0m;

        /// <summary>
        /// Deferred classification period for exploratory wells (months).
        /// </summary>
        public const int DeferredClassificationPeriodMonths = 12;

        /// <summary>
        /// Epsilon for decimal comparisons.
        /// </summary>
        public const decimal Epsilon = 0.0001m;
    }
}


