using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.Models.SuckerRodPumping;
using Beep.OilandGas.GasProperties.Calculations;

namespace Beep.OilandGas.SuckerRodPumping.Calculations
{
    /// <summary>
    /// Provides sucker rod load calculations.
    /// </summary>
    public static class SuckerRodLoadCalculator
    {
        /// <summary>
        /// Calculates sucker rod loads for a pumping system.
        /// </summary>
        /// <param name="systemProperties">Sucker rod system properties.</param>
        /// <param name="rodString">Rod string configuration.</param>
        /// <returns>Sucker rod load analysis results.</returns>
        public static SuckerRodLoadResult CalculateLoads(
            SuckerRodSystemProperties systemProperties,
            SuckerRodString rodString)
        {
            if (systemProperties == null)
                throw new ArgumentNullException(nameof(systemProperties));

            if (rodString == null)
                throw new ArgumentNullException(nameof(rodString));

            var result = new SuckerRodLoadResult();

            // Calculate rod string weight
            result.RodStringWeight = CalculateRodStringWeight(rodString);

            // Calculate fluid load
            result.FluidLoad = CalculateFluidLoad(systemProperties);

            // Calculate dynamic load
            result.DynamicLoad = CalculateDynamicLoad(systemProperties, rodString);

            // Calculate peak load (upstroke)
            result.PeakLoad = result.RodStringWeight + result.FluidLoad + result.DynamicLoad;

            // Calculate minimum load (downstroke)
            result.MinimumLoad = result.RodStringWeight - result.DynamicLoad;

            // Polished rod load (average)
            result.PolishedRodLoad = (result.PeakLoad + result.MinimumLoad) / 2m;

            // Load range
            result.LoadRange = result.PeakLoad - result.MinimumLoad;

            // Calculate stress
            decimal rodArea = (decimal)Math.PI * systemProperties.RodDiameter * systemProperties.RodDiameter / 4m;
            result.MaximumStress = result.PeakLoad / rodArea;
            result.StressRange = result.LoadRange / rodArea;

            // Load factor (safety factor)
            decimal yieldStrength = 100000m; // psi (typical for sucker rod steel)
            result.LoadFactor = yieldStrength / result.MaximumStress;

            return result;
        }

        /// <summary>
        /// Calculates rod string weight.
        /// </summary>
        private static decimal CalculateRodStringWeight(SuckerRodString rodString)
        {
            decimal totalWeight = 0m;

            foreach (var section in rodString.Sections)
            {
                // Calculate section weight
                decimal rodArea = (decimal)Math.PI * section.Diameter * section.Diameter / 4m; // square inches
                decimal rodVolume = rodArea * section.Length * 12m; // cubic inches
                decimal rodVolumeFt3 = rodVolume / 1728m; // cubic feet
                decimal sectionWeight = rodVolumeFt3 * section.Density; // pounds

                section.Weight = sectionWeight;
                totalWeight += sectionWeight;
            }

            rodString.TotalWeight = totalWeight;
            return totalWeight;
        }

        /// <summary>
        /// Calculates fluid load.
        /// </summary>
        private static decimal CalculateFluidLoad(SuckerRodSystemProperties systemProperties)
        {
            // Calculate fluid density
            decimal oilDensity = (141.5m / (131.5m + systemProperties.OilGravity)) * 62.4m; // lb/ft³
            decimal waterDensity = 62.4m; // lb/ft³
            decimal liquidDensity = oilDensity * (1.0m - systemProperties.WaterCut) + 
                                   waterDensity * systemProperties.WaterCut;

            // Account for gas
            if (systemProperties.GasOilRatio > 0)
            {
                // Calculate average pressure
                decimal averagePressure = (systemProperties.WellheadPressure + systemProperties.BottomHolePressure) / 2m;
                decimal averageTemperature = 540m; // Rankine (simplified)

                // Calculate Z-factor
                decimal zFactor = ZFactorCalculator.CalculateBrillBeggs(
                    averagePressure, averageTemperature, systemProperties.GasSpecificGravity);

                // Gas density
                decimal gasDensity = (averagePressure * systemProperties.GasSpecificGravity * 28.9645m) /
                                    (zFactor * 10.7316m * averageTemperature);

                // Gas volume factor
                decimal gasVolumeFactor = systemProperties.GasOilRatio * zFactor * averageTemperature /
                                        (averagePressure * 5.614m);

                // Adjust liquid density for gas
                liquidDensity = (liquidDensity + gasDensity * gasVolumeFactor) / (1.0m + gasVolumeFactor);
            }

            // Pump area
            decimal pumpArea = (decimal)Math.PI * systemProperties.PumpDiameter * systemProperties.PumpDiameter / 4m; // square inches

            // Fluid column height (simplified - using well depth)
            decimal fluidColumnHeight = systemProperties.WellDepth; // feet

            // Fluid load = pressure * area
            decimal fluidPressure = liquidDensity * fluidColumnHeight / 144m; // psia
            decimal fluidLoad = fluidPressure * pumpArea; // pounds

            return fluidLoad;
        }

        /// <summary>
        /// Calculates dynamic load.
        /// </summary>
        private static decimal CalculateDynamicLoad(
            SuckerRodSystemProperties systemProperties,
            SuckerRodString rodString)
        {
            // Dynamic load due to acceleration
            // Simplified calculation based on stroke length and SPM

            decimal strokeLengthFt = systemProperties.StrokeLength / 12m; // feet
            decimal angularVelocity = 2m * (decimal)Math.PI * systemProperties.StrokesPerMinute / 60m; // rad/s

            // Acceleration at top of stroke
            decimal acceleration = angularVelocity * angularVelocity * strokeLengthFt / 2m; // ft/s²

            // Dynamic load = mass * acceleration
            decimal rodMass = rodString.TotalWeight / 32.174m; // slugs
            decimal dynamicLoad = rodMass * acceleration; // pounds

            // Add fluid mass effect
            decimal fluidDensity = (141.5m / (131.5m + systemProperties.OilGravity)) * 62.4m;
            decimal pumpArea = (decimal)Math.PI * systemProperties.PumpDiameter * systemProperties.PumpDiameter / 4m;
            decimal fluidVolume = pumpArea * strokeLengthFt / 144m; // cubic feet
            decimal fluidMass = fluidVolume * fluidDensity / 32.174m; // slugs
            decimal fluidDynamicLoad = fluidMass * acceleration;

            return dynamicLoad + fluidDynamicLoad;
        }

        /// <summary>
        /// Generates pump card (load vs position).
        /// </summary>
        public static PumpCard GeneratePumpCard(
            SuckerRodSystemProperties systemProperties,
            SuckerRodString rodString)
        {
            var loadResult = CalculateLoads(systemProperties, rodString);
            var pumpCard = new PumpCard
            {
                PeakLoad = loadResult.PeakLoad,
                MinimumLoad = loadResult.MinimumLoad
            };

            // Generate points for pump card
            int numberOfPoints = 100;
            for (int i = 0; i <= numberOfPoints; i++)
            {
                decimal position = (decimal)i / numberOfPoints; // 0 to 1

                // Simplified load curve (sinusoidal)
                decimal load = CalculateLoadAtPosition(
                    position, loadResult.MinimumLoad, loadResult.PeakLoad);

                pumpCard.Points.Add(new PumpCardPoint
                {
                    Position = position,
                    Load = load
                });
            }

            // Calculate net area (trapezoidal integration)
            decimal netArea = 0m;
            for (int i = 0; i < pumpCard.Points.Count - 1; i++)
            {
                decimal deltaX = pumpCard.Points[i + 1].Position - pumpCard.Points[i].Position;
                decimal avgY = (pumpCard.Points[i].Load + pumpCard.Points[i + 1].Load) / 2m;
                netArea += deltaX * avgY;
            }
            pumpCard.NetArea = netArea;

            return pumpCard;
        }

        /// <summary>
        /// Calculates load at given position.
        /// </summary>
        private static decimal CalculateLoadAtPosition(
            decimal position,
            decimal minimumLoad,
            decimal peakLoad)
        {
            // Simplified sinusoidal load curve
            decimal angle = position * 2m * (decimal)Math.PI;
            decimal load = minimumLoad + (peakLoad - minimumLoad) * (1m + (decimal)Math.Sin((double)angle)) / 2m;

            return load;
        }
    }
}

