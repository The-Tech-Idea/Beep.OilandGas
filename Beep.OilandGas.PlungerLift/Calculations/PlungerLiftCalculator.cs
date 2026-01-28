using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.Models.Data.PlungerLift;
using Beep.OilandGas.GasProperties.Calculations;

namespace Beep.OilandGas.PlungerLift.Calculations
{
    /// <summary>
    /// Provides plunger lift calculations.
    /// </summary>
    public static class PlungerLiftCalculator
    {
        /// <summary>
        /// Analyzes plunger lift cycle performance.
        /// </summary>
        /// <param name="wellProperties">Well properties.</param>
        /// <returns>Plunger lift cycle analysis results.</returns>
        public static PLUNGER_LIFT_CYCLE_RESULT AnalyzeCycle(
            PLUNGER_LIFT_WELL_PROPERTIES wellProperties)
        {
            if (wellProperties == null)
                throw new ArgumentNullException(nameof(wellProperties));

            var result = new PLUNGER_LIFT_CYCLE_RESULT();

            // Calculate plunger fall time
            result.FALL_TIME = CalculateFallTime(wellProperties);

            // Calculate plunger rise time
            result.RISE_TIME = CalculateRiseTime(wellProperties);

            // Calculate shut-in time (pressure build-up)
            result.SHUT_IN_TIME = CalculateShutInTime(wellProperties);

            // Total cycle time
            result.CYCLE_TIME = result.FALL_TIME + result.RISE_TIME + result.SHUT_IN_TIME;

            // Calculate velocities
            result.FALL_VELOCITY = wellProperties.WELL_DEPTH / result.FALL_TIME;
            result.RISE_VELOCITY = wellProperties.WELL_DEPTH / result.RISE_TIME;

            // Calculate liquid slug size
            result.LIQUID_SLUG_SIZE = CalculateLiquidSlugSize(wellProperties);

            // Production per cycle
            result.PRODUCTION_PER_CYCLE = result.LIQUID_SLUG_SIZE;

            // Cycles per day
            result.CYCLES_PER_DAY = 1440m / result.CYCLE_TIME; // 1440 minutes per day

            // Daily production rate
            result.DAILY_PRODUCTION_RATE = result.PRODUCTION_PER_CYCLE * result.CYCLES_PER_DAY;

            return result;
        }

        /// <summary>
        /// Calculates plunger fall time.
        /// </summary>
        private static decimal CalculateFallTime(PLUNGER_LIFT_WELL_PROPERTIES wellProperties)
        {
            // Plunger fall velocity depends on:
            // - Plunger weight
            // - Fluid properties
            // - Well conditions

            // Simplified calculation: fall velocity ~500-1000 ft/min
            decimal averageFallVelocity = 750m; // ft/min (typical)

            // Adjust for fluid properties
            decimal oilDensity = (141.5m / (131.5m + wellProperties.OIL_GRAVITY)) * 62.4m;
            decimal waterDensity = 62.4m;
            decimal liquidDensity = oilDensity * (1.0m - wellProperties.WATER_CUT) + waterDensity * wellProperties.WATER_CUT;

            // Heavier fluid = slower fall
            decimal densityFactor = 62.4m / liquidDensity;
            averageFallVelocity *= densityFactor;

            // Fall time
            decimal fallTime = wellProperties.WELL_DEPTH / averageFallVelocity; // minutes

            return Math.Max(1m, Math.Min(60m, fallTime)); // Clamp to 1-60 minutes
        }

        /// <summary>
        /// Calculates plunger rise time.
        /// </summary>
        private static decimal CalculateRiseTime(PLUNGER_LIFT_WELL_PROPERTIES wellProperties)
        {
            // Plunger rise velocity depends on:
            // - Gas pressure
            // - Liquid slug size
            // - Well depth

            // Simplified calculation: rise velocity ~200-500 ft/min
            decimal averageRiseVelocity = 350m; // ft/min (typical)

            // Adjust for pressure differential
            decimal pressureDifferential = wellProperties.CASING_PRESSURE - wellProperties.WELLHEAD_PRESSURE;
            decimal pressureFactor = pressureDifferential / 100m; // Normalize
            averageRiseVelocity *= (1.0m + pressureFactor * 0.2m); // Up to 20% increase

            // Adjust for liquid slug
            decimal liquidSlugSize = CalculateLiquidSlugSize(wellProperties);
            decimal slugFactor = 1.0m - (liquidSlugSize / 10m) * 0.1m; // Heavier slug = slower
            averageRiseVelocity *= Math.Max(0.5m, slugFactor);

            // Rise time
            decimal riseTime = wellProperties.WELL_DEPTH / averageRiseVelocity; // minutes

            return Math.Max(2m, Math.Min(30m, riseTime)); // Clamp to 2-30 minutes
        }

        /// <summary>
        /// Calculates shut-in time (pressure build-up).
        /// </summary>
        private static decimal CalculateShutInTime(PLUNGER_LIFT_WELL_PROPERTIES wellProperties)
        {
            // Shut-in time depends on:
            // - Required pressure build-up
            // - Gas flow rate
            // - Well characteristics

            // Simplified calculation: typical shut-in 5-30 minutes
            decimal baseShutInTime = 15m; // minutes

            // Adjust for pressure differential needed
            decimal requiredPressure = wellProperties.CASING_PRESSURE - wellProperties.WELLHEAD_PRESSURE;
            decimal pressureFactor = requiredPressure / 200m; // Normalize
            baseShutInTime *= (1.0m + pressureFactor * 0.5m); // Up to 50% increase

            // Adjust for gas availability
            decimal gasFactor = wellProperties.GAS_OIL_RATIO / 1000m; // Normalize
            baseShutInTime *= (1.0m - gasFactor * 0.2m); // More gas = faster build-up

            return Math.Max(5m, Math.Min(60m, baseShutInTime)); // Clamp to 5-60 minutes
        }

        /// <summary>
        /// Calculates liquid slug size.
        /// </summary>
        private static decimal CalculateLiquidSlugSize(PLUNGER_LIFT_WELL_PROPERTIES wellProperties)
        {
            // Liquid slug size depends on:
            // - Liquid production rate
            // - Cycle time
            // - Well conditions

            // Simplified: slug size based on production rate and cycle time
            decimal cycleTime = CalculateShutInTime(wellProperties) + 
                               CalculateFallTime(wellProperties) + 
                               CalculateRiseTime(wellProperties);

            // Liquid accumulated during shut-in
            decimal liquidSlugSize = wellProperties.LIQUID_PRODUCTION_RATE * cycleTime / 1440m; // bbl

            // Clamp to reasonable range
            return Math.Max(0.1m, Math.Min(5.0m, liquidSlugSize));
        }

        /// <summary>
        /// Calculates gas requirements for plunger lift.
        /// </summary>
        public static PLUNGER_LIFT_GAS_REQUIREMENTS CalculateGasRequirements(
            PLUNGER_LIFT_WELL_PROPERTIES wellProperties,
            PLUNGER_LIFT_CYCLE_RESULT cycleResult)
        {
            if (wellProperties == null)
                throw new ArgumentNullException(nameof(wellProperties));

            if (cycleResult == null)
                throw new ArgumentNullException(nameof(cycleResult));

            var result = new PLUNGER_LIFT_GAS_REQUIREMENTS();

            // Calculate required gas for lifting
            decimal requiredGasPerCycle = CalculateGasPerCycle(wellProperties, cycleResult);

            // Required gas injection rate
            result.REQUIRED_GAS_INJECTION_RATE = requiredGasPerCycle * cycleResult.CYCLES_PER_DAY / 1000m; // Mscf/day

            // Available gas from well
            result.AVAILABLE_GAS = wellProperties.GAS_OIL_RATIO * wellProperties.LIQUID_PRODUCTION_RATE / 1000m; // Mscf/day

            // Additional gas required
            result.ADDITIONAL_GAS_REQUIRED = Math.Max(0m, result.REQUIRED_GAS_INJECTION_RATE - result.AVAILABLE_GAS);

            // Required GLR
            if (wellProperties.LIQUID_PRODUCTION_RATE > 0)
            {
                result.REQUIRED_GAS_LIQUID_RATIO = result.REQUIRED_GAS_INJECTION_RATE * 1000m / wellProperties.LIQUID_PRODUCTION_RATE;
            }
            else
            {
                result.REQUIRED_GAS_LIQUID_RATIO = 0m;
            }

            // Minimum and maximum casing pressure
            result.MINIMUM_CASING_PRESSURE = wellProperties.WELLHEAD_PRESSURE + 50m; // Minimum 50 psi above
            result.MAXIMUM_CASING_PRESSURE = wellProperties.BOTTOM_HOLE_PRESSURE * 0.8m; // 80% of BHP

            return result;
        }

        /// <summary>
        /// Calculates gas required per cycle.
        /// </summary>
        private static decimal CalculateGasPerCycle(
            PLUNGER_LIFT_WELL_PROPERTIES wellProperties,
            PLUNGER_LIFT_CYCLE_RESULT cycleResult)
        {
            // Gas required to lift liquid slug
            // Based on pressure and volume needed

            // Calculate average pressure
            decimal averagePressure = (wellProperties.CASING_PRESSURE + wellProperties.WELLHEAD_PRESSURE) / 2m;
            decimal averageTemperature = (wellProperties.WELLHEAD_TEMPERATURE + wellProperties.BOTTOM_HOLE_TEMPERATURE) / 2m;

            // Calculate Z-factor
            decimal zFactor = ZFactorCalculator.CalculateBrillBeggs(
                averagePressure, averageTemperature, wellProperties.GAS_SPECIFIC_GRAVITY);

            // Liquid slug volume in cubic feet
            decimal liquidSlugVolume = cycleResult.LIQUID_SLUG_SIZE * 5.615m; // ftÂ³

            // Gas volume needed (simplified)
            // Gas must displace liquid and provide lifting force
            decimal gasVolumeNeeded = liquidSlugVolume * 2m; // Simplified: 2x liquid volume

            // Convert to standard conditions
            decimal gasVolumeSCF = gasVolumeNeeded * averagePressure * 520m / (14.7m * zFactor * averageTemperature);

            return gasVolumeSCF; // scf per cycle
        }

        /// <summary>
        /// Performs complete plunger lift performance analysis.
        /// </summary>
        public static PlungerLiftPerformanceResult AnalyzePerformance(
            PLUNGER_LIFT_WELL_PROPERTIES wellProperties)
        {
            if (wellProperties == null)
                throw new ArgumentNullException(nameof(wellProperties));

            var result = new PlungerLiftPerformanceResult();

            // Analyze cycle
            result.CycleResult = AnalyzeCycle(wellProperties);

            // Calculate gas requirements
            result.GasRequirements = CalculateGasRequirements(wellProperties, result.CycleResult);

            // Check feasibility
            result.IsFeasible = CheckFeasibility(wellProperties, result, result.FeasibilityReasons);

            // Calculate system efficiency
            result.SystemEfficiency = CalculateSystemEfficiency(wellProperties, result);

            return result;
        }

        /// <summary>
        /// Checks plunger lift feasibility.
        /// </summary>
        private static bool CheckFeasibility(
            PLUNGER_LIFT_WELL_PROPERTIES wellProperties,
            PlungerLiftPerformanceResult performanceResult,
            List<string> reasons)
        {
            bool isFeasible = true;

            // Check gas availability
            if (performanceResult.GasRequirements.ADDITIONAL_GAS_REQUIRED > performanceResult.GasRequirements.AVAILABLE_GAS * 0.5m)
            {
                isFeasible = false;
                reasons.Add($"Insufficient gas: Additional gas required ({performanceResult.GasRequirements.ADDITIONAL_GAS_REQUIRED:F2} Mscf/day) exceeds 50% of available gas.");
            }

            // Check pressure differential
            decimal pressureDifferential = wellProperties.CASING_PRESSURE - wellProperties.WELLHEAD_PRESSURE;
            if (pressureDifferential < 50m)
            {
                isFeasible = false;
                reasons.Add($"Insufficient pressure differential: {pressureDifferential:F2} psi is less than minimum 50 psi.");
            }

            // Check cycle time
            if (performanceResult.CycleResult.CYCLE_TIME > 60m)
            {
                isFeasible = false;
                reasons.Add($"Cycle time too long: {performanceResult.CycleResult.CYCLE_TIME:F2} minutes exceeds maximum 60 minutes.");
            }

            // Check production rate
            if (performanceResult.CycleResult.DAILY_PRODUCTION_RATE < wellProperties.LIQUID_PRODUCTION_RATE * 0.5m)
            {
                isFeasible = false;
                reasons.Add($"Production rate too low: {performanceResult.CycleResult.DAILY_PRODUCTION_RATE:F2} bbl/day is less than 50% of desired rate.");
            }

            // Check GLR
            if (performanceResult.GasRequirements.REQUIRED_GAS_LIQUID_RATIO > 5000m)
            {
                isFeasible = false;
                reasons.Add($"Gas-liquid ratio too high: {performanceResult.GasRequirements.REQUIRED_GAS_LIQUID_RATIO:F2} scf/bbl exceeds maximum 5000 scf/bbl.");
            }

            return isFeasible;
        }

        /// <summary>
        /// Calculates system efficiency.
        /// </summary>
        private static decimal CalculateSystemEfficiency(
            PLUNGER_LIFT_WELL_PROPERTIES wellProperties,
            PlungerLiftPerformanceResult performanceResult)
        {
            // System efficiency = actual production / potential production
            decimal potentialProduction = wellProperties.LIQUID_PRODUCTION_RATE;
            decimal actualProduction = performanceResult.CycleResult.DAILY_PRODUCTION_RATE;

            if (potentialProduction <= 0)
                return 0m;

            decimal efficiency = actualProduction / potentialProduction;

            // Adjust for gas efficiency
            decimal gasEfficiency = 1.0m;
            if (performanceResult.GasRequirements.ADDITIONAL_GAS_REQUIRED > 0)
            {
                gasEfficiency = 0.8m; // Reduced efficiency if external gas needed
            }

            return efficiency * gasEfficiency;
        }
    }
}

