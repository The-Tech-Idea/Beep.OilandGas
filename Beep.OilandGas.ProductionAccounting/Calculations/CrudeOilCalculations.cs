using System;
using Beep.OilandGas.ProductionAccounting.Models;

namespace Beep.OilandGas.ProductionAccounting.Calculations
{
    /// <summary>
    /// Provides calculations for crude oil properties and conversions.
    /// </summary>
    public static class CrudeOilCalculations
    {
        /// <summary>
        /// Standard temperature for API gravity measurement (60°F).
        /// </summary>
        public const decimal StandardTemperature = 60m;

        /// <summary>
        /// Calculates API gravity from specific gravity.
        /// </summary>
        /// <param name="specificGravity">Specific gravity at 60°F/60°F.</param>
        /// <returns>API gravity in degrees.</returns>
        public static decimal CalculateApiGravity(decimal specificGravity)
        {
            if (specificGravity <= 0)
                throw new ArgumentException("Specific gravity must be greater than zero.", nameof(specificGravity));

            return (141.5m / specificGravity) - 131.5m;
        }

        /// <summary>
        /// Calculates specific gravity from API gravity.
        /// </summary>
        /// <param name="apiGravity">API gravity in degrees.</param>
        /// <returns>Specific gravity at 60°F/60°F.</returns>
        public static decimal CalculateSpecificGravity(decimal apiGravity)
        {
            if (apiGravity <= -131.5m)
                throw new ArgumentException("API gravity must be greater than -131.5.", nameof(apiGravity));

            return 141.5m / (apiGravity + 131.5m);
        }

        /// <summary>
        /// Calculates density in pounds per gallon from API gravity.
        /// </summary>
        /// <param name="apiGravity">API gravity in degrees.</param>
        /// <returns>Density in pounds per gallon.</returns>
        public static decimal CalculateDensity(decimal apiGravity)
        {
            decimal specificGravity = CalculateSpecificGravity(apiGravity);
            return specificGravity * 8.345404m; // Water density at 60°F
        }

        /// <summary>
        /// Calculates net volume from gross volume and BS&W.
        /// </summary>
        /// <param name="grossVolume">Gross volume in barrels.</param>
        /// <param name="bswPercentage">BS&W as percentage (0-100).</param>
        /// <returns>Net volume in barrels.</returns>
        public static decimal CalculateNetVolume(decimal grossVolume, decimal bswPercentage)
        {
            if (grossVolume < 0)
                throw new ArgumentException("Gross volume cannot be negative.", nameof(grossVolume));

            if (bswPercentage < 0 || bswPercentage > 100)
                throw new ArgumentException("BS&W percentage must be between 0 and 100.", nameof(bswPercentage));

            return grossVolume * (1m - bswPercentage / 100m);
        }

        /// <summary>
        /// Calculates gross volume from net volume and BS&W.
        /// </summary>
        /// <param name="netVolume">Net volume in barrels.</param>
        /// <param name="bswPercentage">BS&W as percentage (0-100).</param>
        /// <returns>Gross volume in barrels.</returns>
        public static decimal CalculateGrossVolume(decimal netVolume, decimal bswPercentage)
        {
            if (netVolume < 0)
                throw new ArgumentException("Net volume cannot be negative.", nameof(netVolume));

            if (bswPercentage < 0 || bswPercentage > 100)
                throw new ArgumentException("BS&W percentage must be between 0 and 100.", nameof(bswPercentage));

            if (bswPercentage >= 100)
                throw new ArgumentException("BS&W percentage cannot be 100% or greater.", nameof(bswPercentage));

            return netVolume / (1m - bswPercentage / 100m);
        }

        /// <summary>
        /// Applies temperature correction to volume (ASTM Table 6).
        /// </summary>
        /// <param name="volume">Volume at observed temperature.</param>
        /// <param name="observedTemperature">Observed temperature in °F.</param>
        /// <param name="apiGravity">API gravity at 60°F.</param>
        /// <returns>Volume corrected to 60°F.</returns>
        public static decimal ApplyTemperatureCorrection(decimal volume, decimal observedTemperature, decimal apiGravity)
        {
            // Simplified temperature correction - full implementation would use ASTM Table 6
            decimal temperatureDifference = observedTemperature - StandardTemperature;
            decimal correctionFactor = 1m - (temperatureDifference * 0.00036m); // Approximate
            return volume * correctionFactor;
        }

        /// <summary>
        /// Calculates shrinkage factor for crude oil.
        /// </summary>
        /// <param name="apiGravity">API gravity.</param>
        /// <param name="temperature">Temperature in °F.</param>
        /// <returns>Shrinkage factor (typically 0.98-1.0).</returns>
        public static decimal CalculateShrinkageFactor(decimal apiGravity, decimal temperature)
        {
            // Simplified calculation - full implementation would use industry tables
            decimal baseShrinkage = 0.99m;
            decimal temperatureEffect = (temperature - StandardTemperature) * 0.0001m;
            return baseShrinkage - temperatureEffect;
        }

        /// <summary>
        /// Calculates value adjustment based on quality differentials.
        /// </summary>
        /// <param name="basePrice">Base price per barrel.</param>
        /// <param name="apiGravityDifferential">API gravity differential per degree.</param>
        /// <param name="sulfurDifferential">Sulfur content differential per 0.1%.</param>
        /// <param name="properties">Crude oil properties.</param>
        /// <param name="referenceApiGravity">Reference API gravity.</param>
        /// <param name="referenceSulfurContent">Reference sulfur content.</param>
        /// <returns>Adjusted price per barrel.</returns>
        public static decimal CalculateQualityAdjustedPrice(
            decimal basePrice,
            decimal apiGravityDifferential,
            decimal sulfurDifferential,
            CrudeOilProperties properties,
            decimal referenceApiGravity = 40m,
            decimal referenceSulfurContent = 0.5m)
        {
            decimal apiAdjustment = (properties.ApiGravity - referenceApiGravity) * apiGravityDifferential;
            decimal sulfurAdjustment = (properties.SulfurContent - referenceSulfurContent) * 10m * sulfurDifferential;

            return basePrice + apiAdjustment + sulfurAdjustment;
        }
    }
}

