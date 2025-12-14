using System;
using System.Collections.Generic;
using Beep.OilandGas.GasLift.Models;
using Beep.OilandGas.GasProperties.Calculations;

namespace Beep.OilandGas.GasLift.Calculations
{
    /// <summary>
    /// Provides gas lift valve spacing calculations.
    /// </summary>
    public static class GasLiftValveSpacingCalculator
    {
        /// <summary>
        /// Calculates optimal gas lift valve spacing.
        /// </summary>
        /// <param name="wellProperties">Well properties.</param>
        /// <param name="gasInjectionPressure">Gas injection pressure in psia.</param>
        /// <param name="numberOfValves">Number of valves.</param>
        /// <returns>Valve spacing result.</returns>
        public static GasLiftValveSpacingResult CalculateValveSpacing(
            GasLiftWellProperties wellProperties,
            decimal gasInjectionPressure,
            int numberOfValves = 5)
        {
            if (wellProperties == null)
                throw new ArgumentNullException(nameof(wellProperties));

            if (numberOfValves <= 0)
                throw new ArgumentException("Number of valves must be greater than zero.", nameof(numberOfValves));

            if (gasInjectionPressure <= wellProperties.WellheadPressure)
                throw new ArgumentException("Gas injection pressure must be greater than wellhead pressure.", nameof(gasInjectionPressure));

            var result = new GasLiftValveSpacingResult
            {
                NumberOfValves = numberOfValves
            };

            // Calculate temperature gradient
            decimal temperatureGradient = (wellProperties.BottomHoleTemperature - wellProperties.WellheadTemperature) /
                                        wellProperties.WellDepth;

            // Calculate pressure gradient (simplified)
            decimal pressureGradient = CalculatePressureGradient(wellProperties);

            // Calculate valve depths and opening pressures
            decimal currentDepth = wellProperties.WellheadPressure * 144m / (62.4m * 0.433m); // Initial depth estimate
            decimal depthIncrement = (wellProperties.WellDepth - currentDepth) / numberOfValves;

            for (int i = 0; i < numberOfValves; i++)
            {
                decimal valveDepth = currentDepth + (i + 1) * depthIncrement;

                // Calculate opening pressure at valve depth
                decimal valveTemperature = wellProperties.WellheadTemperature + temperatureGradient * valveDepth;
                decimal openingPressure = CalculateOpeningPressure(
                    wellProperties, gasInjectionPressure, valveDepth, valveTemperature, pressureGradient);

                result.ValveDepths.Add(valveDepth);
                result.OpeningPressures.Add(openingPressure);
            }

            result.TotalDepthCoverage = result.ValveDepths.Last() - result.ValveDepths.First();

            return result;
        }

        /// <summary>
        /// Calculates pressure gradient in the well.
        /// </summary>
        private static decimal CalculatePressureGradient(GasLiftWellProperties wellProperties)
        {
            // Calculate fluid density
            decimal oilDensity = (141.5m / (131.5m + wellProperties.OilGravity)) * 62.4m;
            decimal waterDensity = 62.4m;
            decimal liquidDensity = oilDensity * (1.0m - wellProperties.WaterCut) + waterDensity * wellProperties.WaterCut;

            // Pressure gradient in psia/ft
            decimal pressureGradient = liquidDensity / 144m;

            return pressureGradient;
        }

        /// <summary>
        /// Calculates opening pressure for a valve at given depth.
        /// </summary>
        private static decimal CalculateOpeningPressure(
            GasLiftWellProperties wellProperties,
            decimal gasInjectionPressure,
            decimal valveDepth,
            decimal valveTemperature,
            decimal pressureGradient)
        {
            // Opening pressure should allow gas injection
            // Typically: Opening pressure = Gas injection pressure - (depth * gradient * factor)

            decimal depthFactor = 0.8m; // Factor to account for gas column
            decimal openingPressure = gasInjectionPressure - (valveDepth * pressureGradient * depthFactor);

            // Ensure opening pressure is reasonable
            decimal minimumPressure = wellProperties.WellheadPressure;
            decimal maximumPressure = gasInjectionPressure * 0.95m;

            openingPressure = Math.Max(minimumPressure, Math.Min(maximumPressure, openingPressure));

            // Adjust for temperature (gas expands with temperature)
            decimal zFactor = ZFactorCalculator.CalculateBrillBeggs(
                openingPressure, valveTemperature, wellProperties.GasSpecificGravity);

            // Temperature correction
            decimal temperatureCorrection = valveTemperature / wellProperties.WellheadTemperature;
            openingPressure = openingPressure / temperatureCorrection;

            return Math.Max(minimumPressure, openingPressure);
        }

        /// <summary>
        /// Calculates valve spacing using equal pressure drop method.
        /// </summary>
        public static GasLiftValveSpacingResult CalculateEqualPressureDropSpacing(
            GasLiftWellProperties wellProperties,
            decimal gasInjectionPressure,
            int numberOfValves)
        {
            if (wellProperties == null)
                throw new ArgumentNullException(nameof(wellProperties));

            var result = new GasLiftValveSpacingResult
            {
                NumberOfValves = numberOfValves
            };

            // Calculate pressure drop per valve
            decimal totalPressureDrop = gasInjectionPressure - wellProperties.WellheadPressure;
            decimal pressureDropPerValve = totalPressureDrop / numberOfValves;

            // Calculate temperature gradient
            decimal temperatureGradient = (wellProperties.BottomHoleTemperature - wellProperties.WellheadTemperature) /
                                        wellProperties.WellDepth;

            // Calculate pressure gradient
            decimal pressureGradient = CalculatePressureGradient(wellProperties);

            decimal currentDepth = 0m;
            decimal currentPressure = wellProperties.WellheadPressure;

            for (int i = 0; i < numberOfValves; i++)
            {
                // Calculate depth for next valve
                decimal targetPressure = currentPressure + pressureDropPerValve;
                decimal depthIncrement = (targetPressure - currentPressure) / pressureGradient;

                currentDepth += depthIncrement;
                currentPressure = targetPressure;

                // Ensure we don't exceed well depth
                if (currentDepth > wellProperties.WellDepth)
                    currentDepth = wellProperties.WellDepth;

                decimal valveTemperature = wellProperties.WellheadTemperature + temperatureGradient * currentDepth;

                result.ValveDepths.Add(currentDepth);
                result.OpeningPressures.Add(currentPressure);

                if (currentDepth >= wellProperties.WellDepth)
                    break;
            }

            result.NumberOfValves = result.ValveDepths.Count;
            if (result.ValveDepths.Count > 0)
            {
                result.TotalDepthCoverage = result.ValveDepths.Last() - result.ValveDepths.First();
            }

            return result;
        }

        /// <summary>
        /// Calculates valve spacing using equal depth spacing method.
        /// </summary>
        public static GasLiftValveSpacingResult CalculateEqualDepthSpacing(
            GasLiftWellProperties wellProperties,
            decimal gasInjectionPressure,
            int numberOfValves)
        {
            if (wellProperties == null)
                throw new ArgumentNullException(nameof(wellProperties));

            var result = new GasLiftValveSpacingResult
            {
                NumberOfValves = numberOfValves
            };

            // Equal depth spacing
            decimal depthSpacing = wellProperties.WellDepth / numberOfValves;

            // Calculate temperature gradient
            decimal temperatureGradient = (wellProperties.BottomHoleTemperature - wellProperties.WellheadTemperature) /
                                        wellProperties.WellDepth;

            // Calculate pressure gradient
            decimal pressureGradient = CalculatePressureGradient(wellProperties);

            for (int i = 0; i < numberOfValves; i++)
            {
                decimal valveDepth = (i + 1) * depthSpacing;
                decimal valveTemperature = wellProperties.WellheadTemperature + temperatureGradient * valveDepth;

                decimal openingPressure = CalculateOpeningPressure(
                    wellProperties, gasInjectionPressure, valveDepth, valveTemperature, pressureGradient);

                result.ValveDepths.Add(valveDepth);
                result.OpeningPressures.Add(openingPressure);
            }

            result.TotalDepthCoverage = result.ValveDepths.Last() - result.ValveDepths.First();

            return result;
        }
    }
}

