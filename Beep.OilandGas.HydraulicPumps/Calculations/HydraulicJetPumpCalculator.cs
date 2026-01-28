using System;
using Beep.OilandGas.Models.Data.HydraulicPumps;
using Beep.OilandGas.GasProperties.Calculations;

namespace Beep.OilandGas.HydraulicPumps.Calculations
{
    /// <summary>
    /// Provides hydraulic jet pump calculations.
    /// </summary>
    public static class HydraulicJetPumpCalculator
    {
        /// <summary>
        /// Calculates hydraulic jet pump performance.
        /// </summary>
        /// <param name="wellProperties">Well properties.</param>
        /// <param name="pumpProperties">Jet pump properties.</param>
        /// <returns>Jet pump performance results.</returns>
        public static HYDRAULIC_JET_PUMP_RESULT CalculatePerformance(
            HYDRAULIC_PUMP_WELL_PROPERTIES wellProperties,
            HYDRAULIC_JET_PUMP_PROPERTIES pumpProperties)
        {
            if (wellProperties == null)
                throw new ArgumentNullException(nameof(wellProperties));

            if (pumpProperties == null)
                throw new ArgumentNullException(nameof(pumpProperties));

            var result = new HYDRAULIC_JET_PUMP_RESULT();

            // Calculate area ratios
            decimal nozzleArea = (decimal)Math.PI * pumpProperties.NOZZLE_DIAMETER * pumpProperties.NOZZLE_DIAMETER / 4m;
            decimal throatArea = (decimal)Math.PI * pumpProperties.THROAT_DIAMETER * pumpProperties.THROAT_DIAMETER / 4m;
            decimal areaRatio = nozzleArea / throatArea;

            // Calculate fluid properties
            decimal oilDensity = (141.5m / (131.5m + wellProperties.OIL_GRAVITY)) * 62.4m; // lb/ftÂ³
            decimal waterDensity = 62.4m; // lb/ftÂ³
            decimal liquidDensity = oilDensity * (1.0m - wellProperties.WATER_CUT) + waterDensity * wellProperties.WATER_CUT;
            decimal powerFluidDensity = pumpProperties.POWER_FLUID_SPECIFIC_GRAVITY * 62.4m;

            // Calculate production rate
            result.PRODUCTION_RATE = CalculateProductionRate(
                wellProperties, pumpProperties, areaRatio, liquidDensity, powerFluidDensity);

            // Calculate total flow rate
            result.TOTAL_FLOW_RATE = result.PRODUCTION_RATE + pumpProperties.POWER_FLUID_RATE;

            // Calculate production ratio
            if (pumpProperties.POWER_FLUID_RATE > 0)
            {
                result.PRODUCTION_RATIO = result.PRODUCTION_RATE / pumpProperties.POWER_FLUID_RATE;
            }
            else
            {
                result.PRODUCTION_RATIO = 0m;
            }

            // Calculate pump efficiency
            result.PUMP_EFFICIENCY = CalculatePumpEfficiency(
                areaRatio, result.PRODUCTION_RATIO, liquidDensity, powerFluidDensity);

            // Calculate pressures
            result.PUMP_INTAKE_PRESSURE = CalculatePumpIntakePressure(wellProperties);
            result.PUMP_DISCHARGE_PRESSURE = CalculatePumpDischargePressure(
                wellProperties, pumpProperties, result.PRODUCTION_RATE);

            // Calculate horsepower
            result.POWER_FLUID_HORSEPOWER = CalculatePowerFluidHorsepower(
                pumpProperties, powerFluidDensity);
            result.HYDRAULIC_HORSEPOWER = CalculateHydraulicHorsepower(
                result.PRODUCTION_RATE, result.PUMP_INTAKE_PRESSURE, result.PUMP_DISCHARGE_PRESSURE, liquidDensity);

            // System efficiency
            if (result.POWER_FLUID_HORSEPOWER > 0)
            {
                result.SYSTEM_EFFICIENCY = result.HYDRAULIC_HORSEPOWER / result.POWER_FLUID_HORSEPOWER;
            }
            else
            {
                result.SYSTEM_EFFICIENCY = 0m;
            }

            return result;
        }

        /// <summary>
        /// Calculates production rate.
        /// </summary>
        private static decimal CalculateProductionRate(
            HYDRAULIC_PUMP_WELL_PROPERTIES wellProperties,
            HYDRAULIC_JET_PUMP_PROPERTIES pumpProperties,
            decimal areaRatio,
            decimal liquidDensity,
            decimal powerFluidDensity)
        {
            // Jet pump production rate calculation
            // Based on momentum transfer from power fluid to production fluid

            // Nozzle velocity
            decimal nozzleArea = (decimal)Math.PI * pumpProperties.NOZZLE_DIAMETER * pumpProperties.NOZZLE_DIAMETER / 4m;
            decimal powerFluidRateFt3PerSec = pumpProperties.POWER_FLUID_RATE * 5.615m / 86400m; // ftÂ³/s
            decimal nozzleVelocity = powerFluidRateFt3PerSec / (nozzleArea / 144m); // ft/s

            // Pressure differential
            decimal pressureDifferential = pumpProperties.POWER_FLUID_PRESSURE - wellProperties.BOTTOM_HOLE_PRESSURE;

            // Production rate (simplified)
            decimal productionRate = pumpProperties.POWER_FLUID_RATE * areaRatio * 
                                   (liquidDensity / powerFluidDensity) * 
                                   (pressureDifferential / 100m); // Simplified

            // Apply efficiency factor
            productionRate *= 0.6m; // Typical jet pump efficiency

            return Math.Max(0m, productionRate);
        }

        /// <summary>
        /// Calculates pump efficiency.
        /// </summary>
        private static decimal CalculatePumpEfficiency(
            decimal areaRatio,
            decimal productionRatio,
            decimal liquidDensity,
            decimal powerFluidDensity)
        {
            // Jet pump efficiency depends on area ratio and production ratio
            // Optimal efficiency typically at area ratio ~0.4-0.6

            decimal optimalAreaRatio = 0.5m;
            decimal areaRatioEfficiency = 1.0m - (decimal)Math.Abs((double)(areaRatio - optimalAreaRatio)) * 2m;
            areaRatioEfficiency = Math.Max(0.3m, Math.Min(1.0m, areaRatioEfficiency));

            // Production ratio effect
            decimal productionRatioEfficiency = 1.0m;
            if (productionRatio > 2m)
            {
                productionRatioEfficiency = 1.0m - (productionRatio - 2m) * 0.1m; // Decreases above 2:1
            }
            productionRatioEfficiency = Math.Max(0.3m, Math.Min(1.0m, productionRatioEfficiency));

            // Density effect
            decimal densityRatio = liquidDensity / powerFluidDensity;
            decimal densityEfficiency = 1.0m - (decimal)Math.Abs((double)(densityRatio - 1.0m)) * 0.2m;
            densityEfficiency = Math.Max(0.4m, Math.Min(1.0m, densityEfficiency));

            return areaRatioEfficiency * productionRatioEfficiency * densityEfficiency;
        }

        /// <summary>
        /// Calculates pump intake pressure.
        /// </summary>
        private static decimal CalculatePumpIntakePressure(HYDRAULIC_PUMP_WELL_PROPERTIES wellProperties)
        {
            // Pump intake pressure is approximately bottom hole pressure
            // Adjusted for depth and fluid column
            return wellProperties.BOTTOM_HOLE_PRESSURE;
        }

        /// <summary>
        /// Calculates pump discharge pressure.
        /// </summary>
        private static decimal CalculatePumpDischargePressure(
            HYDRAULIC_PUMP_WELL_PROPERTIES wellProperties,
            HYDRAULIC_JET_PUMP_PROPERTIES pumpProperties,
            decimal productionRate)
        {
            // Discharge pressure = wellhead pressure + friction + hydrostatic
            decimal wellheadPressure = wellProperties.WELLHEAD_PRESSURE;

            // Friction pressure (simplified)
            decimal frictionPressure = CalculateFrictionPressure(wellProperties, productionRate);

            // Hydrostatic head
            decimal oilDensity = (141.5m / (131.5m + wellProperties.OIL_GRAVITY)) * 62.4m;
            decimal waterDensity = 62.4m;
            decimal liquidDensity = oilDensity * (1.0m - wellProperties.WATER_CUT) + waterDensity * wellProperties.WATER_CUT;
            decimal hydrostaticPressure = liquidDensity * wellProperties.WELL_DEPTH / 144m;

            return wellheadPressure + frictionPressure + hydrostaticPressure;
        }

        /// <summary>
        /// Calculates friction pressure.
        /// </summary>
        private static decimal CalculateFrictionPressure(
            HYDRAULIC_PUMP_WELL_PROPERTIES wellProperties,
            decimal flowRate)
        {
            // Simplified friction calculation
            decimal tubingArea = (decimal)Math.PI * wellProperties.TUBING_DIAMETER * wellProperties.TUBING_DIAMETER / 4m;
            decimal velocity = (flowRate * 5.615m) / (86400m * tubingArea / 144m); // ft/s

            decimal reynoldsNumber = 62.4m * velocity * (wellProperties.TUBING_DIAMETER / 12m) / 0.001m;
            decimal frictionFactor = reynoldsNumber < 2000m
                ? 64m / reynoldsNumber
                : 0.3164m / (decimal)Math.Pow((double)reynoldsNumber, 0.25);

            decimal frictionPressure = frictionFactor * (wellProperties.WELL_DEPTH / (wellProperties.TUBING_DIAMETER / 12m)) *
                                      (velocity * velocity) / (2m * 32.174m) * 62.4m / 144m;

            return Math.Max(0m, frictionPressure);
        }

        /// <summary>
        /// Calculates power fluid horsepower.
        /// </summary>
        private static decimal CalculatePowerFluidHorsepower(
            HYDRAULIC_JET_PUMP_PROPERTIES pumpProperties,
            decimal powerFluidDensity)
        {
            // HHP = (Q * P) / (1714 * efficiency)
            decimal flowRateGPM = pumpProperties.POWER_FLUID_RATE * 42m / 1440m; // GPM
            decimal horsepower = (flowRateGPM * pumpProperties.POWER_FLUID_PRESSURE) / 1714m;

            return Math.Max(0m, horsepower);
        }

        /// <summary>
        /// Calculates hydraulic horsepower.
        /// </summary>
        private static decimal CalculateHydraulicHorsepower(
            decimal productionRate,
            decimal intakePressure,
            decimal dischargePressure,
            decimal liquidDensity)
        {
            // HHP = (Q * Î”P) / 1714
            decimal flowRateGPM = productionRate * 42m / 1440m; // GPM
            decimal pressureDifferential = dischargePressure - intakePressure;
            decimal horsepower = (flowRateGPM * pressureDifferential) / 1714m;

            return Math.Max(0m, horsepower);
        }
    }
}

