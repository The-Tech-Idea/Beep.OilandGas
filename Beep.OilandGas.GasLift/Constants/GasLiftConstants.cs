namespace Beep.OilandGas.GasLift.Constants
{
    /// <summary>
    /// Constants used in gas lift calculations.
    /// </summary>
    public static class GasLiftConstants
    {
        /// <summary>
        /// Standard gas lift valve port sizes in inches.
        /// </summary>
        public static readonly decimal[] StandardPortSizes = { 0.25m, 0.375m, 0.5m, 0.625m, 0.75m, 1.0m };

        /// <summary>
        /// Standard gas lift valve closing pressure ratio (relative to opening).
        /// </summary>
        public const decimal StandardClosingPressureRatio = 0.9m;

        /// <summary>
        /// Minimum gas injection rate in Mscf/day.
        /// </summary>
        public const decimal MinimumGasInjectionRate = 50m;

        /// <summary>
        /// Maximum gas injection rate in Mscf/day.
        /// </summary>
        public const decimal MaximumGasInjectionRate = 10000m;

        /// <summary>
        /// Minimum number of valves.
        /// </summary>
        public const int MinimumNumberOfValves = 1;

        /// <summary>
        /// Maximum number of valves.
        /// </summary>
        public const int MaximumNumberOfValves = 20;

        /// <summary>
        /// Default number of valves.
        /// </summary>
        public const int DefaultNumberOfValves = 5;

        /// <summary>
        /// Minimum valve depth in feet.
        /// </summary>
        public const decimal MinimumValveDepth = 100m;

        /// <summary>
        /// Minimum valve spacing in feet.
        /// </summary>
        public const decimal MinimumValveSpacing = 200m;

        /// <summary>
        /// Gas lift efficiency factor.
        /// </summary>
        public const decimal GasLiftEfficiencyFactor = 0.7m;

        /// <summary>
        /// Conversion factor: Mscf to scf.
        /// </summary>
        public const decimal MscfToScf = 1000m;

        /// <summary>
        /// Conversion factor: scf to bbl (for gas).
        /// </summary>
        public const decimal ScfToBbl = 5.614m;
    }
}

