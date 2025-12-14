using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.PlungerLift.Models;
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
        public static PlungerLiftCycleResult AnalyzeCycle(
            PlungerLiftWellProperties wellProperties)
        {
            if (wellProperties == null)
                throw new ArgumentNullException(nameof(wellProperties));

            var result = new PlungerLiftCycleResult();

            // Calculate plunger fall time
            result.FallTime = CalculateFallTime(wellProperties);

            // Calculate plunger rise time
            result.RiseTime = CalculateRiseTime(wellProperties);

            // Calculate shut-in time (pressure build-up)
            result.ShutInTime = CalculateShutInTime(wellProperties);

            // Total cycle time
            result.CycleTime = result.FallTime + result.RiseTime + result.ShutInTime;

            // Calculate velocities
            result.FallVelocity = wellProperties.WellDepth / result.FallTime;
            result.RiseVelocity = wellProperties.WellDepth / result.RiseTime;

            // Calculate liquid slug size
            result.LiquidSlugSize = CalculateLiquidSlugSize(wellProperties);

            // Production per cycle
            result.ProductionPerCycle = result.LiquidSlugSize;

            // Cycles per day
            result.CyclesPerDay = 1440m / result.CycleTime; // 1440 minutes per day

            // Daily production rate
            result.DailyProductionRate = result.ProductionPerCycle * result.CyclesPerDay;

            return result;
        }

        /// <summary>
        /// Calculates plunger fall time.
        /// </summary>
        private static decimal CalculateFallTime(PlungerLiftWellProperties wellProperties)
        {
            // Plunger fall velocity depends on:
            // - Plunger weight
            // - Fluid properties
            // - Well conditions

            // Simplified calculation: fall velocity ~500-1000 ft/min
            decimal averageFallVelocity = 750m; // ft/min (typical)

            // Adjust for fluid properties
            decimal oilDensity = (141.5m / (131.5m + wellProperties.OilGravity)) * 62.4m;
            decimal waterDensity = 62.4m;
            decimal liquidDensity = oilDensity * (1.0m - wellProperties.WaterCut) + waterDensity * wellProperties.WaterCut;

            // Heavier fluid = slower fall
            decimal densityFactor = 62.4m / liquidDensity;
            averageFallVelocity *= densityFactor;

            // Fall time
            decimal fallTime = wellProperties.WellDepth / averageFallVelocity; // minutes

            return Math.Max(1m, Math.Min(60m, fallTime)); // Clamp to 1-60 minutes
        }

        /// <summary>
        /// Calculates plunger rise time.
        /// </summary>
        private static decimal CalculateRiseTime(PlungerLiftWellProperties wellProperties)
        {
            // Plunger rise velocity depends on:
            // - Gas pressure
            // - Liquid slug size
            // - Well depth

            // Simplified calculation: rise velocity ~200-500 ft/min
            decimal averageRiseVelocity = 350m; // ft/min (typical)

            // Adjust for pressure differential
            decimal pressureDifferential = wellProperties.CasingPressure - wellProperties.WellheadPressure;
            decimal pressureFactor = pressureDifferential / 100m; // Normalize
            averageRiseVelocity *= (1.0m + pressureFactor * 0.2m); // Up to 20% increase

            // Adjust for liquid slug
            decimal liquidSlugSize = CalculateLiquidSlugSize(wellProperties);
            decimal slugFactor = 1.0m - (liquidSlugSize / 10m) * 0.1m; // Heavier slug = slower
            averageRiseVelocity *= Math.Max(0.5m, slugFactor);

            // Rise time
            decimal riseTime = wellProperties.WellDepth / averageRiseVelocity; // minutes

            return Math.Max(2m, Math.Min(30m, riseTime)); // Clamp to 2-30 minutes
        }

        /// <summary>
        /// Calculates shut-in time (pressure build-up).
        /// </summary>
        private static decimal CalculateShutInTime(PlungerLiftWellProperties wellProperties)
        {
            // Shut-in time depends on:
            // - Required pressure build-up
            // - Gas flow rate
            // - Well characteristics

            // Simplified calculation: typical shut-in 5-30 minutes
            decimal baseShutInTime = 15m; // minutes

            // Adjust for pressure differential needed
            decimal requiredPressure = wellProperties.CasingPressure - wellProperties.WellheadPressure;
            decimal pressureFactor = requiredPressure / 200m; // Normalize
            baseShutInTime *= (1.0m + pressureFactor * 0.5m); // Up to 50% increase

            // Adjust for gas availability
            decimal gasFactor = wellProperties.GasOilRatio / 1000m; // Normalize
            baseShutInTime *= (1.0m - gasFactor * 0.2m); // More gas = faster build-up

            return Math.Max(5m, Math.Min(60m, baseShutInTime)); // Clamp to 5-60 minutes
        }

        /// <summary>
        /// Calculates liquid slug size.
        /// </summary>
        private static decimal CalculateLiquidSlugSize(PlungerLiftWellProperties wellProperties)
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
            decimal liquidSlugSize = wellProperties.LiquidProductionRate * cycleTime / 1440m; // bbl

            // Clamp to reasonable range
            return Math.Max(0.1m, Math.Min(5.0m, liquidSlugSize));
        }

        /// <summary>
        /// Calculates gas requirements for plunger lift.
        /// </summary>
        public static PlungerLiftGasRequirements CalculateGasRequirements(
            PlungerLiftWellProperties wellProperties,
            PlungerLiftCycleResult cycleResult)
        {
            if (wellProperties == null)
                throw new ArgumentNullException(nameof(wellProperties));

            if (cycleResult == null)
                throw new ArgumentNullException(nameof(cycleResult));

            var result = new PlungerLiftGasRequirements();

            // Calculate required gas for lifting
            decimal requiredGasPerCycle = CalculateGasPerCycle(wellProperties, cycleResult);

            // Required gas injection rate
            result.RequiredGasInjectionRate = requiredGasPerCycle * cycleResult.CyclesPerDay / 1000m; // Mscf/day

            // Available gas from well
            result.AvailableGas = wellProperties.GasOilRatio * wellProperties.LiquidProductionRate / 1000m; // Mscf/day

            // Additional gas required
            result.AdditionalGasRequired = Math.Max(0m, result.RequiredGasInjectionRate - result.AvailableGas);

            // Required GLR
            if (wellProperties.LiquidProductionRate > 0)
            {
                result.RequiredGasLiquidRatio = result.RequiredGasInjectionRate * 1000m / wellProperties.LiquidProductionRate;
            }
            else
            {
                result.RequiredGasLiquidRatio = 0m;
            }

            // Minimum and maximum casing pressure
            result.MinimumCasingPressure = wellProperties.WellheadPressure + 50m; // Minimum 50 psi above
            result.MaximumCasingPressure = wellProperties.BottomHolePressure * 0.8m; // 80% of BHP

            return result;
        }

        /// <summary>
        /// Calculates gas required per cycle.
        /// </summary>
        private static decimal CalculateGasPerCycle(
            PlungerLiftWellProperties wellProperties,
            PlungerLiftCycleResult cycleResult)
        {
            // Gas required to lift liquid slug
            // Based on pressure and volume needed

            // Calculate average pressure
            decimal averagePressure = (wellProperties.CasingPressure + wellProperties.WellheadPressure) / 2m;
            decimal averageTemperature = (wellProperties.WellheadTemperature + wellProperties.BottomHoleTemperature) / 2m;

            // Calculate Z-factor
            decimal zFactor = ZFactorCalculator.CalculateBrillBeggs(
                averagePressure, averageTemperature, wellProperties.GasSpecificGravity);

            // Liquid slug volume in cubic feet
            decimal liquidSlugVolume = cycleResult.LiquidSlugSize * 5.615m; // ft³

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
            PlungerLiftWellProperties wellProperties)
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
            PlungerLiftWellProperties wellProperties,
            PlungerLiftPerformanceResult performanceResult,
            List<string> reasons)
        {
            bool isFeasible = true;

            // Check gas availability
            if (performanceResult.GasRequirements.AdditionalGasRequired > performanceResult.GasRequirements.AvailableGas * 0.5m)
            {
                isFeasible = false;
                reasons.Add($"Insufficient gas: Additional gas required ({performanceResult.GasRequirements.AdditionalGasRequired:F2} Mscf/day) exceeds 50% of available gas.");
            }

            // Check pressure differential
            decimal pressureDifferential = wellProperties.CasingPressure - wellProperties.WellheadPressure;
            if (pressureDifferential < 50m)
            {
                isFeasible = false;
                reasons.Add($"Insufficient pressure differential: {pressureDifferential:F2} psi is less than minimum 50 psi.");
            }

            // Check cycle time
            if (performanceResult.CycleResult.CycleTime > 60m)
            {
                isFeasible = false;
                reasons.Add($"Cycle time too long: {performanceResult.CycleResult.CycleTime:F2} minutes exceeds maximum 60 minutes.");
            }

            // Check production rate
            if (performanceResult.CycleResult.DailyProductionRate < wellProperties.LiquidProductionRate * 0.5m)
            {
                isFeasible = false;
                reasons.Add($"Production rate too low: {performanceResult.CycleResult.DailyProductionRate:F2} bbl/day is less than 50% of desired rate.");
            }

            // Check GLR
            if (performanceResult.GasRequirements.RequiredGasLiquidRatio > 5000m)
            {
                isFeasible = false;
                reasons.Add($"Gas-liquid ratio too high: {performanceResult.GasRequirements.RequiredGasLiquidRatio:F2} scf/bbl exceeds maximum 5000 scf/bbl.");
            }

            return isFeasible;
        }

        /// <summary>
        /// Calculates system efficiency.
        /// </summary>
        private static decimal CalculateSystemEfficiency(
            PlungerLiftWellProperties wellProperties,
            PlungerLiftPerformanceResult performanceResult)
        {
            // System efficiency = actual production / potential production
            decimal potentialProduction = wellProperties.LiquidProductionRate;
            decimal actualProduction = performanceResult.CycleResult.DailyProductionRate;

            if (potentialProduction <= 0)
                return 0m;

            decimal efficiency = actualProduction / potentialProduction;

            // Adjust for gas efficiency
            decimal gasEfficiency = 1.0m;
            if (performanceResult.GasRequirements.AdditionalGasRequired > 0)
            {
                gasEfficiency = 0.8m; // Reduced efficiency if external gas needed
            }

            return efficiency * gasEfficiency;
        }
    }
}

