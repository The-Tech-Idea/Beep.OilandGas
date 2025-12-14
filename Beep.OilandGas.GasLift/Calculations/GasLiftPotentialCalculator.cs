using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.GasLift.Models;
using Beep.OilandGas.GasProperties.Calculations;

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
        public static GasLiftPotentialResult AnalyzeGasLiftPotential(
            GasLiftWellProperties wellProperties,
            decimal minGasInjectionRate = 100m,
            decimal maxGasInjectionRate = 5000m,
            int numberOfPoints = 50)
        {
            if (wellProperties == null)
                throw new ArgumentNullException(nameof(wellProperties));

            var result = new GasLiftPotentialResult();

            decimal gasInjectionStep = (maxGasInjectionRate - minGasInjectionRate) / numberOfPoints;

            for (int i = 0; i <= numberOfPoints; i++)
            {
                decimal gasInjectionRate = minGasInjectionRate + i * gasInjectionStep;

                // Calculate production rate for this gas injection rate
                decimal productionRate = CalculateProductionRate(
                    wellProperties, gasInjectionRate);

                // Calculate gas-liquid ratio
                decimal totalGasRate = wellProperties.GasOilRatio * productionRate / 1000m + gasInjectionRate;
                decimal gasLiquidRatio = totalGasRate * 1000m / productionRate;

                // Calculate bottom hole pressure
                decimal bottomHolePressure = CalculateBottomHolePressure(
                    wellProperties, gasInjectionRate, productionRate);

                result.PerformancePoints.Add(new GasLiftPerformancePoint
                {
                    GasInjectionRate = gasInjectionRate,
                    ProductionRate = productionRate,
                    GasLiquidRatio = gasLiquidRatio,
                    BottomHolePressure = bottomHolePressure
                });
            }

            // Find optimal point (maximum production rate)
            var optimalPoint = result.PerformancePoints
                .OrderByDescending(p => p.ProductionRate)
                .First();

            result.OptimalGasInjectionRate = optimalPoint.GasInjectionRate;
            result.MaximumProductionRate = optimalPoint.ProductionRate;
            result.OptimalGasLiquidRatio = optimalPoint.GasLiquidRatio;

            return result;
        }

        /// <summary>
        /// Calculates production rate for given gas injection rate.
        /// </summary>
        private static decimal CalculateProductionRate(
            GasLiftWellProperties wellProperties,
            decimal gasInjectionRate)
        {
            // Simplified gas lift production calculation
            // Based on gas lift performance curve

            // Base production without gas lift
            decimal baseProduction = wellProperties.DesiredProductionRate * 0.3m;

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
            GasLiftWellProperties wellProperties,
            decimal gasInjectionRate)
        {
            // Gas lift effect increases production up to optimal point
            // Simplified model: effect = a * GLR - b * GLR²

            decimal gasLiquidRatio = gasInjectionRate * 1000m / wellProperties.DesiredProductionRate;

            decimal a = 2.0m; // Coefficient
            decimal b = 0.0001m; // Coefficient

            decimal effect = a * gasLiquidRatio - b * gasLiquidRatio * gasLiquidRatio;

            return Math.Max(0m, effect * wellProperties.DesiredProductionRate / 100m);
        }

        /// <summary>
        /// Calculates bottom hole pressure with gas lift.
        /// </summary>
        private static decimal CalculateBottomHolePressure(
            GasLiftWellProperties wellProperties,
            decimal gasInjectionRate,
            decimal productionRate)
        {
            // Calculate average pressure
            decimal averagePressure = (wellProperties.WellheadPressure + wellProperties.BottomHolePressure) / 2m;
            decimal averageTemperature = (wellProperties.WellheadTemperature + wellProperties.BottomHoleTemperature) / 2m;

            // Calculate Z-factor
            decimal zFactor = ZFactorCalculator.CalculateBrillBeggs(
                averagePressure, averageTemperature, wellProperties.GasSpecificGravity);

            // Calculate fluid density with gas
            decimal oilDensity = (141.5m / (131.5m + wellProperties.OilGravity)) * 62.4m;
            decimal waterDensity = 62.4m;
            decimal liquidDensity = oilDensity * (1.0m - wellProperties.WaterCut) + waterDensity * wellProperties.WaterCut;

            // Gas density
            decimal gasDensity = (averagePressure * wellProperties.GasSpecificGravity * 28.9645m) /
                                (zFactor * 10.7316m * averageTemperature);

            // Total gas rate
            decimal totalGasRate = wellProperties.GasOilRatio * productionRate / 1000m + gasInjectionRate;
            decimal gasVolumeFactor = totalGasRate * zFactor * averageTemperature / (averagePressure * 5.614m);

            // Mixture density
            decimal mixtureDensity = (liquidDensity + gasDensity * gasVolumeFactor) / (1.0m + gasVolumeFactor);

            // Bottom hole pressure with gas lift (reduced due to gas)
            decimal hydrostaticHead = mixtureDensity * wellProperties.WellDepth / 144m;
            decimal bottomHolePressure = wellProperties.WellheadPressure + hydrostaticHead;

            return Math.Max(wellProperties.WellheadPressure, bottomHolePressure);
        }
    }
}

