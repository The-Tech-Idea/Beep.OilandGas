using System;

using Beep.OilandGas.GasProperties.Constants;

namespace Beep.OilandGas.GasProperties.Calculations
{
    /// <summary>
    /// Provides gas viscosity calculation methods.
    /// </summary>
    public static class GasViscosityCalculator
    {
        /// <summary>
        /// Calculates gas viscosity using Carr-Kobayashi-Burrows correlation.
        /// </summary>
        /// <param name="pressure">Pressure in psia.</param>
        /// <param name="temperature">Temperature in Rankine.</param>
        /// <param name="specificGravity">Gas specific gravity (relative to air).</param>
        /// <param name="zFactor">Z-factor (compressibility factor).</param>
        /// <returns>Gas viscosity in centipoise.</returns>
        public static decimal CalculateCarrKobayashiBurrows(
            decimal pressure,
            decimal temperature,
            decimal specificGravity,
            decimal zFactor)
        {
            if (pressure <= 0)
                throw new ArgumentException("Pressure must be greater than zero.", nameof(pressure));

            if (temperature <= 0)
                throw new ArgumentException("Temperature must be greater than zero.", nameof(temperature));

            if (specificGravity <= 0)
                throw new ArgumentException("Specific gravity must be greater than zero.", nameof(specificGravity));

            // Calculate pseudo-critical properties
            decimal pseudoCriticalPressure = 756.8m - 131.0m * specificGravity - 3.6m * specificGravity * specificGravity;
            decimal pseudoCriticalTemperature = 169.2m + 349.5m * specificGravity - 74.0m * specificGravity * specificGravity;

            // Calculate pseudo-reduced properties
            decimal pseudoReducedPressure = pressure / pseudoCriticalPressure;
            decimal pseudoReducedTemperature = temperature / pseudoCriticalTemperature;

            // Atmospheric gas viscosity (at 1 atm and reservoir temperature)
            decimal atmosphericViscosity = CalculateAtmosphericViscosity(temperature, specificGravity);

            // Viscosity ratio (mu/mu1) from Carr-Kobayashi-Burrows correlation
            decimal viscosityRatio = CalculateViscosityRatio(pseudoReducedPressure, pseudoReducedTemperature);

            // Gas viscosity at reservoir conditions
            decimal gasViscosity = atmosphericViscosity * viscosityRatio;

            return Math.Max(0.001m, Math.Min(0.1m, gasViscosity)); // Clamp to reasonable range
        }

        /// <summary>
        /// Calculates atmospheric gas viscosity.
        /// </summary>
        private static decimal CalculateAtmosphericViscosity(decimal temperature, decimal specificGravity)
        {
            // Temperature in Rankine
            decimal tempF = temperature - GasPropertiesConstants.FahrenheitToRankine;

            // Atmospheric viscosity correlation
            decimal K = (9.4m + 0.02m * (specificGravity * 28.9645m)) * (decimal)Math.Pow((double)tempF, 1.5) /
                        (209m + 19m * (specificGravity * 28.9645m) + tempF);

            return K / 10000m; // Convert to centipoise
        }

        /// <summary>
        /// Calculates viscosity ratio from pseudo-reduced properties.
        /// </summary>
        private static decimal CalculateViscosityRatio(decimal pseudoReducedPressure, decimal pseudoReducedTemperature)
        {
            // Carr-Kobayashi-Burrows correlation for viscosity ratio
            decimal A = 0.001493m * pseudoReducedPressure + 0.000517m * pseudoReducedPressure * pseudoReducedPressure -
                       0.00001583m * pseudoReducedPressure * pseudoReducedPressure * pseudoReducedPressure;
            decimal B = 0.000777m * pseudoReducedPressure + 0.000372m * pseudoReducedPressure * pseudoReducedPressure +
                       0.00008783m * pseudoReducedPressure * pseudoReducedPressure * pseudoReducedPressure;
            decimal C = -0.0000611m * pseudoReducedPressure - 0.0000833m * pseudoReducedPressure * pseudoReducedPressure +
                       0.00002222m * pseudoReducedPressure * pseudoReducedPressure * pseudoReducedPressure;

            decimal viscosityRatio = 1.0m + A / pseudoReducedTemperature + B / (pseudoReducedTemperature * pseudoReducedTemperature) +
                                    C / (pseudoReducedTemperature * pseudoReducedTemperature * pseudoReducedTemperature);

            return Math.Max(0.5m, Math.Min(3.0m, viscosityRatio)); // Clamp to reasonable range
        }

        /// <summary>
        /// Calculates gas viscosity using Lee-Gonzalez-Eakin correlation.
        /// </summary>
        /// <param name="pressure">Pressure in psia.</param>
        /// <param name="temperature">Temperature in Rankine.</param>
        /// <param name="specificGravity">Gas specific gravity (relative to air).</param>
        /// <param name="zFactor">Z-factor (compressibility factor).</param>
        /// <returns>Gas viscosity in centipoise.</returns>
        public static decimal CalculateLeeGonzalezEakin(
            decimal pressure,
            decimal temperature,
            decimal specificGravity,
            decimal zFactor)
        {
            if (pressure <= 0)
                throw new ArgumentException("Pressure must be greater than zero.", nameof(pressure));

            if (temperature <= 0)
                throw new ArgumentException("Temperature must be greater than zero.", nameof(temperature));

            if (specificGravity <= 0)
                throw new ArgumentException("Specific gravity must be greater than zero.", nameof(specificGravity));

            // Calculate gas density
            decimal molecularWeight = specificGravity * GasPropertiesConstants.AirMolecularWeight;
            decimal gasDensity = (pressure * molecularWeight) / (zFactor * GasPropertiesConstants.UniversalGasConstant * temperature);

            // Lee-Gonzalez-Eakin correlation
            decimal K = ((9.4m + 0.02m * molecularWeight) * (decimal)Math.Pow((double)temperature, 1.5)) /
                        (209m + 19m * molecularWeight + temperature);

            decimal X = 3.5m + 986m / temperature + 0.01m * molecularWeight;
            decimal Y = 2.4m - 0.2m * X;

            decimal gasViscosity = K * (decimal)Math.Exp((double)(X * (decimal)Math.Pow((double)(gasDensity / 62.4m), (double)Y))) / 10000m;

            return Math.Max(0.001m, Math.Min(0.1m, gasViscosity)); // Clamp to reasonable range
        }
    }
}

