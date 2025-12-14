namespace Beep.OilandGas.GasProperties.Constants
{
    /// <summary>
    /// Constants used in gas property calculations.
    /// </summary>
    public static class GasPropertiesConstants
    {
        /// <summary>
        /// Universal gas constant in psia·ft³/(lbmol·°R).
        /// </summary>
        public const decimal UniversalGasConstant = 10.7316m;

        /// <summary>
        /// Standard pressure in psia.
        /// </summary>
        public const decimal StandardPressure = 14.696m;

        /// <summary>
        /// Standard temperature in Rankine.
        /// </summary>
        public const decimal StandardTemperature = 519.67m; // 60°F

        /// <summary>
        /// Air molecular weight in lb/lbmol.
        /// </summary>
        public const decimal AirMolecularWeight = 28.9645m;

        /// <summary>
        /// Conversion factor: psia to atm.
        /// </summary>
        public const decimal PsiaToAtm = 0.068046m;

        /// <summary>
        /// Conversion factor: Rankine to Fahrenheit.
        /// </summary>
        public const decimal RankineToFahrenheit = -459.67m;

        /// <summary>
        /// Conversion factor: Fahrenheit to Rankine.
        /// </summary>
        public const decimal FahrenheitToRankine = 459.67m;

        /// <summary>
        /// Conversion factor: Rankine to Celsius.
        /// </summary>
        public const decimal RankineToCelsius = -491.67m;

        /// <summary>
        /// Conversion factor: Celsius to Rankine.
        /// </summary>
        public const decimal CelsiusToRankine = 491.67m;

        /// <summary>
        /// Minimum valid Z-factor.
        /// </summary>
        public const decimal MinimumZFactor = 0.1m;

        /// <summary>
        /// Maximum valid Z-factor.
        /// </summary>
        public const decimal MaximumZFactor = 2.0m;

        /// <summary>
        /// Minimum valid pressure in psia.
        /// </summary>
        public const decimal MinimumPressure = 0.1m;

        /// <summary>
        /// Maximum valid pressure in psia.
        /// </summary>
        public const decimal MaximumPressure = 15000m;

        /// <summary>
        /// Minimum valid temperature in Rankine.
        /// </summary>
        public const decimal MinimumTemperature = 100m;

        /// <summary>
        /// Maximum valid temperature in Rankine.
        /// </summary>
        public const decimal MaximumTemperature = 1000m;

        /// <summary>
        /// Minimum valid specific gravity.
        /// </summary>
        public const decimal MinimumSpecificGravity = 0.3m;

        /// <summary>
        /// Maximum valid specific gravity.
        /// </summary>
        public const decimal MaximumSpecificGravity = 1.5m;
    }
}

