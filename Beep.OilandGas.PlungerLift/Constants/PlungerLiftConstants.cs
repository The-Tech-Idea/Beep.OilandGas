namespace Beep.OilandGas.PlungerLift.Constants
{
    /// <summary>
    /// Constants used in plunger lift calculations.
    /// </summary>
    public static class PlungerLiftConstants
    {
        /// <summary>
        /// Standard plunger diameters in inches.
        /// </summary>
        public static readonly decimal[] StandardPlungerDiameters = { 1.25m, 1.5m, 1.75m, 2.0m, 2.25m, 2.5m, 2.875m, 3.5m };

        /// <summary>
        /// Standard tubing diameters in inches.
        /// </summary>
        public static readonly decimal[] StandardTubingDiameters = { 1.25m, 1.5m, 1.75m, 2.0m, 2.25m, 2.5m, 2.875m, 3.5m, 4.0m };

        /// <summary>
        /// Minimum pressure differential in psia.
        /// </summary>
        public const decimal MinimumPressureDifferential = 50m;

        /// <summary>
        /// Maximum cycle time in minutes.
        /// </summary>
        public const decimal MaximumCycleTime = 60m;

        /// <summary>
        /// Minimum cycle time in minutes.
        /// </summary>
        public const decimal MinimumCycleTime = 5m;

        /// <summary>
        /// Typical plunger fall velocity in ft/min.
        /// </summary>
        public const decimal TypicalFallVelocity = 750m;

        /// <summary>
        /// Typical plunger rise velocity in ft/min.
        /// </summary>
        public const decimal TypicalRiseVelocity = 350m;

        /// <summary>
        /// Minimum fall velocity in ft/min.
        /// </summary>
        public const decimal MinimumFallVelocity = 500m;

        /// <summary>
        /// Maximum fall velocity in ft/min.
        /// </summary>
        public const decimal MaximumFallVelocity = 1000m;

        /// <summary>
        /// Minimum rise velocity in ft/min.
        /// </summary>
        public const decimal MinimumRiseVelocity = 200m;

        /// <summary>
        /// Maximum rise velocity in ft/min.
        /// </summary>
        public const decimal MaximumRiseVelocity = 500m;

        /// <summary>
        /// Minimum gas-liquid ratio in scf/bbl.
        /// </summary>
        public const decimal MinimumGasLiquidRatio = 200m;

        /// <summary>
        /// Maximum gas-liquid ratio in scf/bbl.
        /// </summary>
        public const decimal MaximumGasLiquidRatio = 5000m;

        /// <summary>
        /// Minimum liquid slug size in bbl.
        /// </summary>
        public const decimal MinimumLiquidSlugSize = 0.1m;

        /// <summary>
        /// Maximum liquid slug size in bbl.
        /// </summary>
        public const decimal MaximumLiquidSlugSize = 5.0m;

        /// <summary>
        /// Standard shut-in time in minutes.
        /// </summary>
        public const decimal StandardShutInTime = 15m;

        /// <summary>
        /// Minimum shut-in time in minutes.
        /// </summary>
        public const decimal MinimumShutInTime = 5m;

        /// <summary>
        /// Maximum shut-in time in minutes.
        /// </summary>
        public const decimal MaximumShutInTime = 60m;

        /// <summary>
        /// Conversion factor: bbl to ft³.
        /// </summary>
        public const decimal BarrelToCubicFeet = 5.615m;

        /// <summary>
        /// Conversion factor: Mscf to scf.
        /// </summary>
        public const decimal MscfToScf = 1000m;

        /// <summary>
        /// Standard pressure in psia.
        /// </summary>
        public const decimal StandardPressure = 14.7m;

        /// <summary>
        /// Standard temperature in Rankine.
        /// </summary>
        public const decimal StandardTemperature = 520m;

        /// <summary>
        /// Minutes per day.
        /// </summary>
        public const decimal MinutesPerDay = 1440m;
    }
}

