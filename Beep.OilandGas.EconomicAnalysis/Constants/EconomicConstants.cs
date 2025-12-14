namespace Beep.OilandGas.EconomicAnalysis.Constants
{
    /// <summary>
    /// Constants used in economic analysis calculations.
    /// </summary>
    public static class EconomicConstants
    {
        /// <summary>
        /// Minimum discount rate (0%).
        /// </summary>
        public const double MinDiscountRate = 0.0;

        /// <summary>
        /// Maximum discount rate (100%).
        /// </summary>
        public const double MaxDiscountRate = 1.0;

        /// <summary>
        /// Default discount rate (10%).
        /// </summary>
        public const double DefaultDiscountRate = 0.10;

        /// <summary>
        /// Minimum IRR tolerance for convergence.
        /// </summary>
        public const double MinIRRTolerance = 1e-6;

        /// <summary>
        /// Maximum IRR iterations.
        /// </summary>
        public const int MaxIRRIterations = 100;

        /// <summary>
        /// Default IRR initial guess (10%).
        /// </summary>
        public const double DefaultIRRGuess = 0.10;

        /// <summary>
        /// Minimum cash flow amount.
        /// </summary>
        public const double MinCashFlow = -1e10;

        /// <summary>
        /// Maximum cash flow amount.
        /// </summary>
        public const double MaxCashFlow = 1e10;

        /// <summary>
        /// Minimum number of periods.
        /// </summary>
        public const int MinPeriods = 1;

        /// <summary>
        /// Maximum number of periods.
        /// </summary>
        public const int MaxPeriods = 1000;

        /// <summary>
        /// Epsilon for floating point comparisons.
        /// </summary>
        public const double Epsilon = 1e-10;
    }
}

