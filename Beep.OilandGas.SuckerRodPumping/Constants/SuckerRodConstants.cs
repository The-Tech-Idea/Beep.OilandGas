namespace Beep.OilandGas.SuckerRodPumping.Constants
{
    /// <summary>
    /// Constants used in sucker rod pumping calculations.
    /// </summary>
    public static class SuckerRodConstants
    {
        /// <summary>
        /// Standard sucker rod diameters in inches.
        /// </summary>
        public static readonly decimal[] StandardRodDiameters = { 0.625m, 0.75m, 0.875m, 1.0m, 1.125m, 1.25m };

        /// <summary>
        /// Standard pump diameters in inches.
        /// </summary>
        public static readonly decimal[] StandardPumpDiameters = { 1.25m, 1.5m, 1.75m, 2.0m, 2.25m, 2.5m, 3.0m, 3.5m, 4.0m };

        /// <summary>
        /// Standard stroke lengths in inches.
        /// </summary>
        public static readonly decimal[] StandardStrokeLengths = { 12m, 18m, 24m, 30m, 36m, 42m, 48m, 54m, 60m, 72m, 84m, 96m, 108m, 120m, 144m, 168m };

        /// <summary>
        /// Standard strokes per minute (SPM).
        /// </summary>
        public static readonly decimal[] StandardSPM = { 5m, 6m, 8m, 10m, 12m, 14m, 16m, 18m, 20m, 22m, 24m, 26m, 28m, 30m };

        /// <summary>
        /// Rod material density (steel) in lb/ft³.
        /// </summary>
        public const decimal SteelDensity = 490m;

        /// <summary>
        /// Rod material yield strength (typical sucker rod steel) in psi.
        /// </summary>
        public const decimal RodYieldStrength = 100000m;

        /// <summary>
        /// Minimum safety factor for rod loading.
        /// </summary>
        public const decimal MinimumSafetyFactor = 1.5m;

        /// <summary>
        /// Standard pump efficiency.
        /// </summary>
        public const decimal StandardPumpEfficiency = 0.85m;

        /// <summary>
        /// Standard motor efficiency.
        /// </summary>
        public const decimal StandardMotorEfficiency = 0.9m;

        /// <summary>
        /// Conversion factor: HP to kW.
        /// </summary>
        public const decimal HorsepowerToKilowatts = 0.746m;

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
        /// Conversion factor: cubic inches to cubic feet.
        /// </summary>
        public const decimal CubicInchesToCubicFeet = 1728m;

        /// <summary>
        /// Standard acceleration due to gravity in ft/s².
        /// </summary>
        public const decimal Gravity = 32.174m;

        /// <summary>
        /// Horsepower conversion factor (for hydraulic calculations).
        /// </summary>
        public const decimal HorsepowerConversionFactor = 1714m;

        /// <summary>
        /// Polished rod horsepower conversion factor.
        /// </summary>
        public const decimal PRHPConversionFactor = 33000m;

        /// <summary>
        /// Minutes per day.
        /// </summary>
        public const decimal MinutesPerDay = 1440m;

        /// <summary>
        /// Hours per day.
        /// </summary>
        public const decimal HoursPerDay = 24m;
    }
}

