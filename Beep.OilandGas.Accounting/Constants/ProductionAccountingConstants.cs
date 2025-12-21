namespace Beep.OilandGas.Accounting.Constants
{
    /// <summary>
    /// Constants for production accounting calculations and validations.
    /// </summary>
    public static class ProductionAccountingConstants
    {
        /// <summary>
        /// Standard barrels per cubic foot.
        /// </summary>
        public const decimal BarrelsPerCubicFoot = 0.178107606679035m;

        /// <summary>
        /// Standard temperature for measurements (60°F).
        /// </summary>
        public const decimal StandardTemperature = 60m;

        /// <summary>
        /// Standard pressure for measurements (14.696 psi).
        /// </summary>
        public const decimal StandardPressure = 14.696m;

        /// <summary>
        /// Minimum valid API gravity.
        /// </summary>
        public const decimal MinApiGravity = -50m;

        /// <summary>
        /// Maximum valid API gravity.
        /// </summary>
        public const decimal MaxApiGravity = 100m;

        /// <summary>
        /// Maximum valid BS&W percentage.
        /// </summary>
        public const decimal MaxBSWPercentage = 100m;

        /// <summary>
        /// Minimum working interest (0%).
        /// </summary>
        public const decimal MinWorkingInterest = 0m;

        /// <summary>
        /// Maximum working interest (100%).
        /// </summary>
        public const decimal MaxWorkingInterest = 1m;

        /// <summary>
        /// Minimum net revenue interest (0%).
        /// </summary>
        public const decimal MinNetRevenueInterest = 0m;

        /// <summary>
        /// Maximum net revenue interest (100%).
        /// </summary>
        public const decimal MaxNetRevenueInterest = 1m;

        /// <summary>
        /// Minimum royalty rate (0%).
        /// </summary>
        public const decimal MinRoyaltyRate = 0m;

        /// <summary>
        /// Maximum royalty rate (100%).
        /// </summary>
        public const decimal MaxRoyaltyRate = 1m;

        /// <summary>
        /// Standard gas-to-oil equivalent ratio (MCF per barrel).
        /// </summary>
        public const decimal GasToOilEquivalent = 6.0m;

        /// <summary>
        /// Minimum volume for measurement (barrels).
        /// </summary>
        public const decimal MinVolume = 0m;

        /// <summary>
        /// Maximum reasonable volume per day (barrels).
        /// </summary>
        public const decimal MaxVolumePerDay = 1000000m;
    }
}

