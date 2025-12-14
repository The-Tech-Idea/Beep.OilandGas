using System;
using Beep.OilandGas.ProductionAccounting.Models;

namespace Beep.OilandGas.ProductionAccounting.Trading
{
    /// <summary>
    /// Provides differential calculations for crude oil trading.
    /// </summary>
    public static class DifferentialCalculator
    {
        /// <summary>
        /// Calculates location differential.
        /// </summary>
        /// <param name="baseLocation">Base pricing location.</param>
        /// <param name="actualLocation">Actual delivery location.</param>
        /// <param name="locationDifferentialTable">Location differential table.</param>
        /// <returns>Location differential in dollars per barrel.</returns>
        public static decimal CalculateLocationDifferential(
            string baseLocation,
            string actualLocation,
            Dictionary<string, decimal> locationDifferentialTable)
        {
            if (locationDifferentialTable == null)
                throw new ArgumentNullException(nameof(locationDifferentialTable));

            if (baseLocation == actualLocation)
                return 0m;

            // Look up differential for actual location
            if (locationDifferentialTable.TryGetValue(actualLocation, out var differential))
                return differential;

            // If not found, return zero (no differential)
            return 0m;
        }

        /// <summary>
        /// Calculates quality differential based on API gravity.
        /// </summary>
        /// <param name="actualApiGravity">Actual API gravity.</param>
        /// <param name="referenceApiGravity">Reference API gravity.</param>
        /// <param name="differentialPerDegree">Differential per degree API gravity.</param>
        /// <returns>Quality differential in dollars per barrel.</returns>
        public static decimal CalculateQualityDifferential(
            decimal actualApiGravity,
            decimal referenceApiGravity,
            decimal differentialPerDegree)
        {
            decimal apiDifference = actualApiGravity - referenceApiGravity;
            return apiDifference * differentialPerDegree;
        }

        /// <summary>
        /// Calculates quality differential based on sulfur content.
        /// </summary>
        /// <param name="actualSulfurContent">Actual sulfur content (weight %).</param>
        /// <param name="referenceSulfurContent">Reference sulfur content (weight %).</param>
        /// <param name="differentialPerPoint">Differential per 0.1% sulfur.</param>
        /// <returns>Quality differential in dollars per barrel.</returns>
        public static decimal CalculateSulfurDifferential(
            decimal actualSulfurContent,
            decimal referenceSulfurContent,
            decimal differentialPerPoint)
        {
            decimal sulfurDifference = actualSulfurContent - referenceSulfurContent;
            decimal pointsDifference = sulfurDifference * 10m; // Convert to 0.1% points
            return pointsDifference * differentialPerPoint;
        }

        /// <summary>
        /// Calculates time differential (pricing date vs. delivery date).
        /// </summary>
        /// <param name="pricingDate">Pricing date.</param>
        /// <param name="deliveryDate">Delivery date.</param>
        /// <param name="dailyDifferential">Daily differential rate.</param>
        /// <returns>Time differential in dollars per barrel.</returns>
        public static decimal CalculateTimeDifferential(
            DateTime pricingDate,
            DateTime deliveryDate,
            decimal dailyDifferential)
        {
            int daysDifference = (deliveryDate - pricingDate).Days;
            return daysDifference * dailyDifferential;
        }

        /// <summary>
        /// Calculates total differential (location + quality + time).
        /// </summary>
        public static decimal CalculateTotalDifferential(
            decimal locationDifferential,
            decimal qualityDifferential,
            decimal timeDifferential)
        {
            return locationDifferential + qualityDifferential + timeDifferential;
        }

        /// <summary>
        /// Calculates quality differential from crude oil properties.
        /// </summary>
        public static decimal CalculateQualityDifferential(
            CrudeOilProperties actualProperties,
            CrudeOilProperties referenceProperties,
            decimal apiGravityDifferentialPerDegree,
            decimal sulfurDifferentialPerPoint)
        {
            decimal apiDifferential = CalculateQualityDifferential(
                actualProperties.ApiGravity,
                referenceProperties.ApiGravity,
                apiGravityDifferentialPerDegree);

            decimal sulfurDifferential = CalculateSulfurDifferential(
                actualProperties.SulfurContent,
                referenceProperties.SulfurContent,
                sulfurDifferentialPerPoint);

            return apiDifferential + sulfurDifferential;
        }
    }
}

