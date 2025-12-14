namespace Beep.OilandGas.HydraulicPumps.Constants
{
    /// <summary>
    /// Constants used in hydraulic pump calculations.
    /// </summary>
    public static class HydraulicPumpConstants
    {
        /// <summary>
        /// Standard nozzle diameters for jet pumps in inches.
        /// </summary>
        public static readonly decimal[] StandardNozzleDiameters = { 0.125m, 0.25m, 0.375m, 0.5m, 0.625m, 0.75m };

        /// <summary>
        /// Standard throat diameters for jet pumps in inches.
        /// </summary>
        public static readonly decimal[] StandardThroatDiameters = { 0.25m, 0.375m, 0.5m, 0.625m, 0.75m, 1.0m };

        /// <summary>
        /// Standard piston diameters for piston pumps in inches.
        /// </summary>
        public static readonly decimal[] StandardPistonDiameters = { 1.25m, 1.5m, 1.75m, 2.0m, 2.25m, 2.5m, 3.0m };

        /// <summary>
        /// Standard stroke lengths in inches.
        /// </summary>
        public static readonly decimal[] StandardStrokeLengths = { 12m, 18m, 24m, 30m, 36m, 42m, 48m, 54m, 60m };

        /// <summary>
        /// Standard strokes per minute (SPM).
        /// </summary>
        public static readonly decimal[] StandardSPM = { 5m, 6m, 8m, 10m, 12m, 14m, 16m, 18m, 20m, 22m, 24m };

        /// <summary>
        /// Optimal area ratio for jet pumps.
        /// </summary>
        public const decimal OptimalJetPumpAreaRatio = 0.5m;

        /// <summary>
        /// Minimum jet pump efficiency.
        /// </summary>
        public const decimal MinimumJetPumpEfficiency = 0.3m;

        /// <summary>
        /// Maximum jet pump efficiency.
        /// </summary>
        public const decimal MaximumJetPumpEfficiency = 0.7m;

        /// <summary>
        /// Standard piston pump efficiency.
        /// </summary>
        public const decimal StandardPistonPumpEfficiency = 0.85m;

        /// <summary>
        /// Minimum volumetric efficiency.
        /// </summary>
        public const decimal MinimumVolumetricEfficiency = 0.3m;

        /// <summary>
        /// Maximum volumetric efficiency.
        /// </summary>
        public const decimal MaximumVolumetricEfficiency = 0.95m;

        /// <summary>
        /// Conversion factor: bbl to ft³.
        /// </summary>
        public const decimal BarrelToCubicFeet = 5.615m;

        /// <summary>
        /// Conversion factor: bbl to gallons.
        /// </summary>
        public const decimal BarrelToGallons = 42m;

        /// <summary>
        /// Conversion factor: inches to feet.
        /// </summary>
        public const decimal InchesToFeet = 12m;

        /// <summary>
        /// Horsepower conversion factor (for hydraulic calculations).
        /// </summary>
        public const decimal HorsepowerConversionFactor = 1714m;

        /// <summary>
        /// Minutes per day.
        /// </summary>
        public const decimal MinutesPerDay = 1440m;

        /// <summary>
        /// Standard acceleration due to gravity in ft/s².
        /// </summary>
        public const decimal Gravity = 32.174m;
    }
}

