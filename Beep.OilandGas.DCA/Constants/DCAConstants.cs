namespace Beep.OilandGas.DCA.Constants
{
    /// <summary>
    /// Constants used throughout the DCA library.
    /// </summary>
    public static class DCAConstants
    {
        /// <summary>
        /// Default reference date for time calculations (January 1, 2000).
        /// </summary>
        public static readonly System.DateTime ReferenceDate = new System.DateTime(2000, 1, 1);

        /// <summary>
        /// Default maximum number of iterations for nonlinear regression.
        /// </summary>
        public const int DefaultMaxIterations = 100;

        /// <summary>
        /// Default convergence tolerance for nonlinear regression.
        /// </summary>
        public const double DefaultTolerance = 1E-8;

        /// <summary>
        /// Minimum value for initial production rate (qi).
        /// </summary>
        public const double MinInitialProductionRate = 0.0;

        /// <summary>
        /// Minimum value for initial decline rate (di).
        /// </summary>
        public const double MinInitialDeclineRate = 0.0;

        /// <summary>
        /// Maximum value for decline exponent (b) in hyperbolic decline.
        /// </summary>
        public const double MaxDeclineExponent = 1.0;

        /// <summary>
        /// Minimum value for decline exponent (b) in hyperbolic decline.
        /// </summary>
        public const double MinDeclineExponent = 0.0;

        /// <summary>
        /// Minimum number of data points required for analysis.
        /// </summary>
        public const int MinDataPoints = 2;

        /// <summary>
        /// Default initial production rate for curve fitting.
        /// </summary>
        public const double DefaultInitialProductionRate = 1000.0;

        /// <summary>
        /// Default initial decline rate for curve fitting.
        /// </summary>
        public const double DefaultInitialDeclineRate = 0.1;

        /// <summary>
        /// Default decline exponent (b) for hyperbolic decline.
        /// </summary>
        public const double DefaultDeclineExponent = 0.5;

        /// <summary>
        /// Small epsilon value for numerical comparisons.
        /// </summary>
        public const double Epsilon = 1E-10;

        /// <summary>
        /// Conversion factor: days per year (accounting for leap years).
        /// </summary>
        public const double DaysPerYear = 365.25;

        /// <summary>
        /// Conversion factor: Fahrenheit to Celsius.
        /// </summary>
        public const double FahrenheitToCelsius = 1.8;

        /// <summary>
        /// Conversion factor: psia to standard pressure.
        /// </summary>
        public const double PsiaToStandard = 14.7;

        /// <summary>
        /// Conversion factor: acres to square feet (for reservoir area calculations).
        /// </summary>
        public const double AcresToSquareFeet = 43560.0;

        /// <summary>
        /// Conversion factor: barrels per acre-foot (for volume calculations).
        /// </summary>
        public const double BarrelsPerAcreFoot = 7758.0;
    }
}

