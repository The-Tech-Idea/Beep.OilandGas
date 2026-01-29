using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.Models.Data.SuckerRodPumping;
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
        public static SUCKER_ROD_LOAD_RESULT CalculateLoads(
            SUCKER_ROD_SYSTEM_PROPERTIES systemProperties,
            SUCKER_ROD_STRING rodString)
        {
            if (systemProperties == null)
                throw new ArgumentNullException(nameof(systemProperties));

            if (rodString == null)
                throw new ArgumentNullException(nameof(rodString));

            var result = new SUCKER_ROD_LOAD_RESULT();

            // Calculate rod string weight
            result.ROD_STRING_WEIGHT = CalculateRodStringWeight(rodString);

            // Calculate fluid load
            result.FLUID_LOAD = CalculateFluidLoad(systemProperties);

            // Calculate dynamic load
            result.DYNAMIC_LOAD = CalculateDynamicLoad(systemProperties, rodString);

            // Calculate peak load (upstroke)
            result.PEAK_LOAD = result.ROD_STRING_WEIGHT + result.FLUID_LOAD + result.DYNAMIC_LOAD;

            // Calculate minimum load (downstroke)
            result.MINIMUM_LOAD = result.ROD_STRING_WEIGHT - result.DYNAMIC_LOAD;

            // Polished rod load (average)
            result.POLISHED_ROD_LOAD = (result.PEAK_LOAD + result.MINIMUM_LOAD) / 2m;

            // Load range
            result.LOAD_RANGE = result.PEAK_LOAD - result.MINIMUM_LOAD;

            // Calculate stress
            decimal rodArea = (decimal)Math.PI * systemProperties.ROD_DIAMETER * systemProperties.ROD_DIAMETER / 4m;
            result.MAXIMUM_STRESS = result.PEAK_LOAD / rodArea;
            result.STRESS_RANGE = result.LOAD_RANGE / rodArea;

            // Load factor (safety factor)
            decimal yieldStrength = 100000m; // psi (typical for sucker rod steel)
            result.LOAD_FACTOR = yieldStrength / result.MAXIMUM_STRESS;

            return result;
        }

        /// <summary>
        /// Calculates rod string weight.
        /// </summary>
        private static decimal CalculateRodStringWeight(SUCKER_ROD_STRING rodString)
        {
            decimal totalWeight = 0m;

            foreach (var section in rodString.SECTIONS)
            {
                // Calculate section weight
                decimal rodArea = (decimal)Math.PI * section.DIAMETER * section.DIAMETER / 4m; // square inches
                decimal rodVolume = rodArea * section.LENGTH * 12m; // cubic inches
                decimal rodVolumeFt3 = rodVolume / 1728m; // cubic feet
                decimal sectionWeight = rodVolumeFt3 * section.DENSITY; // pounds

                section.WEIGHT = sectionWeight;
                totalWeight += sectionWeight;
            }

            rodString.TOTAL_WEIGHT = totalWeight;
            return totalWeight;
        }

        /// <summary>
        /// Calculates fluid load.
        /// </summary>
        private static decimal CalculateFluidLoad(SUCKER_ROD_SYSTEM_PROPERTIES systemProperties)
        {
            // Calculate fluid density
            decimal oilDensity = (141.5m / (131.5m + systemProperties.OIL_GRAVITY)) * 62.4m; // lb/ftÂ³
            decimal waterDensity = 62.4m; // lb/ftÂ³
            decimal liquidDensity = oilDensity * (1.0m - systemProperties.WATER_CUT) + 
                                   waterDensity * systemProperties.WATER_CUT;

            // Account for gas
            if (systemProperties.GAS_OIL_RATIO > 0)
            {
                // Calculate average pressure
                decimal averagePressure = (systemProperties.WELLHEAD_PRESSURE + systemProperties.BOTTOM_HOLE_PRESSURE) / 2m;
                decimal averageTemperature = 540m; // Rankine (simplified)

                // Calculate Z-factor
                decimal zFactor = ZFactorCalculator.CalculateBrillBeggs(
                    averagePressure, averageTemperature, systemProperties.GAS_SPECIFIC_GRAVITY);

                // Gas density
                decimal gasDensity = (averagePressure * systemProperties.GAS_SPECIFIC_GRAVITY * 28.9645m) /
                                    (zFactor * 10.7316m * averageTemperature);

                // Gas volume factor
                decimal gasVolumeFactor = systemProperties.GAS_OIL_RATIO * zFactor * averageTemperature /
                                        (averagePressure * 5.614m);

                // Adjust liquid density for gas
                liquidDensity = (liquidDensity + gasDensity * gasVolumeFactor) / (1.0m + gasVolumeFactor);
            }

            // Pump area
            decimal pumpArea = (decimal)Math.PI * systemProperties.PUMP_DIAMETER * systemProperties.PUMP_DIAMETER / 4m; // square inches

            // Fluid column height (simplified - using well depth)
            decimal fluidColumnHeight = systemProperties.WELL_DEPTH; // feet

            // Fluid load = pressure * area
            decimal fluidPressure = liquidDensity * fluidColumnHeight / 144m; // psia
            decimal fluidLoad = fluidPressure * pumpArea; // pounds

            return fluidLoad;
        }

        /// <summary>
        /// Calculates dynamic load.
        /// </summary>
        private static decimal CalculateDynamicLoad(
            SUCKER_ROD_SYSTEM_PROPERTIES systemProperties,
            SUCKER_ROD_STRING rodString)
        {
            // Dynamic load due to acceleration
            // Simplified calculation based on stroke length and SPM

            decimal strokeLengthFt = systemProperties.STROKE_LENGTH / 12m; // feet
            decimal angularVelocity = 2m * (decimal)Math.PI * systemProperties.STROKES_PER_MINUTE / 60m; // rad/s

            // Acceleration at top of stroke
            decimal acceleration = angularVelocity * angularVelocity * strokeLengthFt / 2m; // ft/sÂ²

            // Dynamic load = mass * acceleration
            decimal rodMass = rodString.TOTAL_WEIGHT / 32.174m; // slugs
            decimal dynamicLoad = rodMass * acceleration; // pounds

            // Add fluid mass effect
            decimal fluidDensity = (141.5m / (131.5m + systemProperties.OIL_GRAVITY)) * 62.4m;
            decimal pumpArea = (decimal)Math.PI * systemProperties.PUMP_DIAMETER * systemProperties.PUMP_DIAMETER / 4m;
            decimal fluidVolume = pumpArea * strokeLengthFt / 144m; // cubic feet
            decimal fluidMass = fluidVolume * fluidDensity / 32.174m; // slugs
            decimal fluidDynamicLoad = fluidMass * acceleration;

            return dynamicLoad + fluidDynamicLoad;
        }

        /// <summary>
        /// Generates pump card (load vs position).
        /// </summary>
        public static PumpCard GeneratePumpCard(
            SUCKER_ROD_SYSTEM_PROPERTIES systemProperties,
            SUCKER_ROD_STRING rodString)
        {
            var loadResult = CalculateLoads(systemProperties, rodString);
            var pumpCard = new PumpCard
            {
                PeakLoad = loadResult.PEAK_LOAD,
                MinimumLoad = loadResult.MINIMUM_LOAD
            };

            // Generate points for pump card
            int numberOfPoints = 100;
            for (int i = 0; i <= numberOfPoints; i++)
            {
                decimal position = (decimal)i / numberOfPoints; // 0 to 1

                // Simplified load curve (sinusoidal)
                decimal load = CalculateLoadAtPosition(
                    position, loadResult.MINIMUM_LOAD, loadResult.PEAK_LOAD);

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

