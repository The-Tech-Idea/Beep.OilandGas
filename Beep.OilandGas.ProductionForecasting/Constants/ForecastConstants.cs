namespace Beep.OilandGas.ProductionForecasting.Constants
{
    /// <summary>
    /// Constants used in production forecasting calculations.
    /// </summary>
    public static class ForecastConstants
    {
        /// <summary>
        /// Conversion factor: days to seconds.
        /// </summary>
        public const decimal DaysToSeconds = 86400m;

        /// <summary>
        /// Conversion factor: Mscf to scf.
        /// </summary>
        public const decimal MscfToScf = 1000m;

        /// <summary>
        /// Standard temperature in Rankine.
        /// </summary>
        public const decimal StandardTemperature = 519.67m; // 60°F

        /// <summary>
        /// Standard pressure in psia.
        /// </summary>
        public const decimal StandardPressure = 14.696m;

        /// <summary>
        /// Minimum forecast duration in days.
        /// </summary>
        public const decimal MinimumForecastDuration = 1m;

        /// <summary>
        /// Maximum forecast duration in days.
        /// </summary>
        public const decimal MaximumForecastDuration = 36500m; // 100 years

        /// <summary>
        /// Minimum time step in days.
        /// </summary>
        public const decimal MinimumTimeStep = 0.01m;

        /// <summary>
        /// Minimum number of time steps.
        /// </summary>
        public const int MinimumTimeSteps = 10;

        /// <summary>
        /// Maximum number of time steps.
        /// </summary>
        public const int MaximumTimeSteps = 10000;

        /// <summary>
        /// Default number of time steps.
        /// </summary>
        public const int DefaultTimeSteps = 100;
    }
}

