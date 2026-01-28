using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.Models.Data.GasLift;
using Beep.OilandGas.GasProperties.Calculations;
using Beep.OilandGas.Models.Data.Calculations;

namespace Beep.OilandGas.GasLift.Calculations
{
    /// <summary>
    /// Provides gas lift potential analysis calculations.
    /// </summary>
    public static class GasLiftPotentialCalculator
    {
        /// <summary>
        /// Analyzes gas lift potential for a well.
        /// </summary>
        /// <param name="wellProperties">Well properties.</param>
        /// <param name="minGasInjectionRate">Minimum gas injection rate in Mscf/day.</param>
        /// <param name="maxGasInjectionRate">Maximum gas injection rate in Mscf/day.</param>
        /// <param name="numberOfPoints">Number of analysis points.</param>
        /// <returns>Gas lift potential analysis results.</returns>
        public static GAS_LIFT_WELL_PROPERTIES AnalyzeGasLiftPotential(
            GAS_LIFT_WELL_PROPERTIES wellProperties,
            decimal minGasInjectionRate = 100m,
            decimal maxGasInjectionRate = 5000m,
            int numberOfPoints = 50)
        {
            if (wellProperties == null)
                throw new ArgumentNullException(nameof(wellProperties));

            var result = new GAS_LIFT_WELL_PROPERTIES();

            decimal gasInjectionStep = (maxGasInjectionRate - minGasInjectionRate) / numberOfPoints;

            for (int i = 0; i <= numberOfPoints; i++)
            {
                decimal gasInjectionRate = minGasInjectionRate + i * gasInjectionStep;

                // Calculate production rate for this gas injection rate
                decimal productionRate = CalculateProductionRate(
                    wellProperties, gasInjectionRate);

                // Calculate gas-liquid ratio
                decimal totalGasRate = (decimal)(wellProperties.GAS_OIL_RATIO * productionRate / 1000m + gasInjectionRate);
                decimal gasLiquidRatio = totalGasRate * 1000m / productionRate;

                // Calculate bottom hole pressure
                decimal bottomHolePressure = CalculateBottomHolePressure(
                    wellProperties, gasInjectionRate, productionRate);

                result.PERFORMANCE_POINTS.Add(new GasLiftPerformancePoint
                {
                    GasInjectionRate = gasInjectionRate,
                    ProductionRate = productionRate,
                    GasLiquidRatio = gasLiquidRatio,
                    BottomHolePressure = bottomHolePressure
                });
            }

            // Find optimal point (maximum production rate)
            var optimalPoint = result.PERFORMANCE_POINTS
                .OrderByDescending(p => p.ProductionRate)
                .First();

            result.OPTIMAL_GAS_INJECTION_RATE = optimalPoint.GasInjectionRate;
            result.MAXIMUM_PRODUCTION_RATE = optimalPoint.ProductionRate;
            result.OPTIMAL_GAS_LIQUID_RATIO = optimalPoint.GasLiquidRatio;

            return result;
        }

        /// <summary>
        /// Calculates production rate for given gas injection rate.
        /// </summary>
        private static decimal CalculateProductionRate(
            GAS_LIFT_WELL_PROPERTIES wellProperties,
            decimal gasInjectionRate)
        {
            // Simplified gas lift production calculation
            // Based on gas lift performance curve

            // Base production without gas lift
            decimal baseProduction = (decimal)(wellProperties.DESIRED_PRODUCTION_RATE * 0.3m);

            // Gas lift effect (simplified)
            decimal gasLiftEffect = CalculateGasLiftEffect(
                wellProperties, gasInjectionRate);

            // Production rate with gas lift
            decimal productionRate = baseProduction + gasLiftEffect;

            // Apply diminishing returns at high gas rates
            if (gasInjectionRate > 2000m)
            {
                decimal excessGas = gasInjectionRate - 2000m;
                decimal penalty = excessGas * 0.1m; // Diminishing returns
                productionRate -= penalty;
            }

            return Math.Max(0m, productionRate);
        }

        /// <summary>
        /// Calculates gas lift effect on production.
        /// </summary>
        private static decimal CalculateGasLiftEffect(
            GAS_LIFT_WELL_PROPERTIES wellProperties,
            decimal gasInjectionRate)
        {
            // Gas lift effect increases production up to optimal point
            // Simplified model: effect = a * GLR - b * GLR²

            decimal gasLiquidRatio = (decimal)(gasInjectionRate * 1000m / wellProperties.DESIRED_PRODUCTION_RATE);

            decimal a = 2.0m; // Coefficient
            decimal b = 0.0001m; // Coefficient

            decimal effect = a * gasLiquidRatio - b * gasLiquidRatio * gasLiquidRatio;

            return Math.Max(0m, (decimal)(effect * wellProperties.DESIRED_PRODUCTION_RATE / 100m));
        }

        /// <summary>
        /// Calculates bottom hole pressure with gas lift.
        /// </summary>
        private static decimal CalculateBottomHolePressure(
            GAS_LIFT_WELL_PROPERTIES wellProperties,
            decimal gasInjectionRate,
            decimal productionRate)
        {
            // Calculate average pressure
            decimal averagePressure = (decimal)((wellProperties.WELLHEAD_PRESSURE + wellProperties.BOTTOM_HOLE_PRESSURE) / 2m);
            decimal averageTemperature = (decimal)((wellProperties.WELLHEAD_TEMPERATURE + wellProperties.BOTTOM_HOLE_TEMPERATURE) / 2m);

            // Calculate Z-factor
            decimal zFactor = ZFactorCalculator.CalculateBrillBeggs(
                averagePressure, averageTemperature, wellProperties.GAS_SPECIFIC_GRAVITY);

            // Calculate fluid density with gas
            decimal oilDensity = (decimal)(141.5m / (131.5m + wellProperties.GAS_SPECIFIC_GRAVITY) * 62.4m);
            decimal waterDensity = 62.4m;
            decimal liquidDensity = (decimal)(oilDensity * (1.0m - wellProperties.WATER_CUT) + waterDensity * wellProperties.WATER_CUT);

            // Gas density
            decimal gasDensity = (decimal)((averagePressure * wellProperties.GAS_SPECIFIC_GRAVITY * 28.9645m) /
                                (zFactor * 10.7316m * averageTemperature));

            // Total gas rate
            decimal totalGasRate = (decimal)(wellProperties.GAS_OIL_RATIO * productionRate / 1000m + gasInjectionRate);
            decimal gasVolumeFactor = totalGasRate * zFactor * averageTemperature / (averagePressure * 5.614m);

            // Mixture density
            decimal mixtureDensity = (liquidDensity + gasDensity * gasVolumeFactor) / (1.0m + gasVolumeFactor);

            // Bottom hole pressure with gas lift (reduced due to gas)
            decimal hydrostaticHead = (decimal)(mixtureDensity * wellProperties.WELL_DEPTH / 144m);
            decimal bottomHolePressure = (decimal)(wellProperties.WELLHEAD_PRESSURE + hydrostaticHead);

            return Math.Max((decimal)wellProperties.WELLHEAD_PRESSURE, bottomHolePressure);
        }
    }
}

