namespace Beep.OilandGas.FlashCalculations.Constants
{
    /// <summary>
    /// Constants used in flash calculations.
    /// </summary>
    public static class FlashConstants
    {
        /// <summary>
        /// Standard molecular weight of air in lb/lbmol.
        /// </summary>
        public const decimal AirMolecularWeight = 28.9645m;

        /// <summary>
        /// Gas constant in ft-lbf/(lbmol-R).
        /// </summary>
        public const decimal GasConstant = 10.7316m;

        /// <summary>
        /// Standard pressure in psia.
        /// </summary>
        public const decimal StandardPressure = 14.7m;

        /// <summary>
        /// Standard temperature in Rankine.
        /// </summary>
        public const decimal StandardTemperature = 520m;

        /// <summary>
        /// Maximum number of iterations for flash calculation.
        /// </summary>
        public const int MaximumIterations = 100;

        /// <summary>
        /// Convergence tolerance.
        /// </summary>
        public const decimal ConvergenceTolerance = 0.0001m;

        /// <summary>
        /// Minimum K-value.
        /// </summary>
        public const decimal MinimumKValue = 0.001m;

        /// <summary>
        /// Maximum K-value.
        /// </summary>
        public const decimal MaximumKValue = 1000m;

        /// <summary>
        /// Wilson correlation constant.
        /// </summary>
        public const decimal WilsonConstant = 5.37m;

        /// <summary>
        /// Standard liquid density factor.
        /// </summary>
        public const decimal StandardLiquidDensityFactor = 62.4m;

        /// <summary>
        /// Minimum denominator value to avoid division by zero.
        /// </summary>
        public const decimal MinimumDenominator = 0.0001m;
    }
}

