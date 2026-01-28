using System;

using Beep.OilandGas.GasProperties.Calculations;

namespace Beep.OilandGas.HydraulicPumps.Calculations
{
    /// <summary>
    /// Provides hydraulic piston pump calculations.
    /// </summary>
    public static class HydraulicPistonPumpCalculator
    {
        /// <summary>
        /// Calculates hydraulic piston pump performance.
        /// </summary>
        /// <param name="wellProperties">Well properties.</param>
        /// <param name="pumpProperties">Piston pump properties.</param>
        /// <returns>Piston pump performance results.</returns>
        public static HYDRAULIC_PISTON_PUMP_RESULT CalculatePerformance(
            HYDRAULIC_PUMP_WELL_PROPERTIES wellProperties,
            HYDRAULIC_PISTON_PUMP_PROPERTIES pumpProperties)
        {
            if (wellProperties == null)
                throw new ArgumentNullException(nameof(wellProperties));

            if (pumpProperties == null)
                throw new ArgumentNullException(nameof(pumpProperties));

            var result = new HYDRAULIC_PISTON_PUMP_RESULT();

            // Calculate pump displacement
            result.PUMP_DISPLACEMENT = CalculatePumpDisplacement(pumpProperties);

            // Calculate volumetric efficiency
            result.VOLUMETRIC_EFFICIENCY = CalculateVolumetricEfficiency(
                wellProperties, pumpProperties);

            // Calculate production rate
            result.PRODUCTION_RATE = result.PUMP_DISPLACEMENT * result.VOLUMETRIC_EFFICIENCY;

            // Calculate power fluid consumption
            result.POWER_FLUID_CONSUMPTION = CalculatePowerFluidConsumption(
                pumpProperties, result.PRODUCTION_RATE);

            // Calculate pressures
            result.PUMP_INTAKE_PRESSURE = CalculatePumpIntakePressure(wellProperties);
            result.PUMP_DISCHARGE_PRESSURE = CalculatePumpDischargePressure(
                wellProperties, pumpProperties, result.PRODUCTION_RATE);

            // Calculate horsepower
            result.POWER_FLUID_HORSEPOWER = CalculatePowerFluidHorsepower(
                pumpProperties, result.POWER_FLUID_CONSUMPTION);
            result.HYDRAULIC_HORSEPOWER = CalculateHydraulicHorsepower(
                result.PRODUCTION_RATE, result.PUMP_INTAKE_PRESSURE, result.PUMP_DISCHARGE_PRESSURE, wellProperties);

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
        /// Calculates pump displacement.
        /// </summary>
        private static decimal CalculatePumpDisplacement(HYDRAULIC_PISTON_PUMP_PROPERTIES pumpProperties)
        {
            // Pump displacement = (Ï€ * DÂ² / 4) * Stroke Length * SPM * 1440 minutes/day
            // Convert to bbl/day

            decimal pistonArea = (decimal)Math.PI * pumpProperties.PISTON_DIAMETER * pumpProperties.PISTON_DIAMETER / 4m; // square inches
            decimal strokeLengthFt = pumpProperties.STROKE_LENGTH / 12m; // feet
            decimal volumePerStroke = pistonArea * strokeLengthFt / 144m; // cubic feet per stroke

            decimal strokesPerDay = pumpProperties.STROKES_PER_MINUTE * 1440m; // strokes per day
            decimal volumePerDay = volumePerStroke * strokesPerDay; // cubic feet per day

            // Convert to bbl/day (1 bbl = 5.615 ftÂ³)
            decimal pumpDisplacement = volumePerDay / 5.615m; // bbl/day

            return pumpDisplacement;
        }

        /// <summary>
        /// Calculates volumetric efficiency.
        /// </summary>
        private static decimal CalculateVolumetricEfficiency(
            HYDRAULIC_PUMP_WELL_PROPERTIES wellProperties,
            HYDRAULIC_PISTON_PUMP_PROPERTIES pumpProperties)
        {
            // Volumetric efficiency depends on:
            // - Gas-oil ratio
            // - Pump design
            // - Operating conditions

            decimal baseEfficiency = 0.85m; // Base efficiency

            // Gas effect (reduces efficiency)
            decimal gasEffect = 1.0m;
            if (wellProperties.GAS_OIL_RATIO > 0)
            {
                decimal gorFactor = wellProperties.GAS_OIL_RATIO / 1000m; // Normalize
                gasEffect = 1.0m - (gorFactor * 0.15m); // Up to 15% reduction
                gasEffect = Math.Max(0.5m, gasEffect); // Minimum 50%
            }

            // Water cut effect (slight reduction)
            decimal waterCutEffect = 1.0m - (wellProperties.WATER_CUT * 0.05m); // Up to 5% reduction

            // Pressure effect
            decimal pressureRatio = wellProperties.BOTTOM_HOLE_PRESSURE / pumpProperties.POWER_FLUID_PRESSURE;
            decimal pressureEffect = 1.0m - (decimal)Math.Abs((double)(pressureRatio - 0.5m)) * 0.2m; // Optimal at 0.5
            pressureEffect = Math.Max(0.6m, Math.Min(1.0m, pressureEffect));

            decimal volumetricEfficiency = baseEfficiency * gasEffect * waterCutEffect * pressureEffect;

            return Math.Max(0.3m, Math.Min(0.95m, volumetricEfficiency));
        }

        /// <summary>
        /// Calculates power fluid consumption.
        /// </summary>
        private static decimal CalculatePowerFluidConsumption(
            HYDRAULIC_PISTON_PUMP_PROPERTIES pumpProperties,
            decimal productionRate)
        {
            // Power fluid consumption is approximately equal to production rate
            // Plus losses due to efficiency

            decimal efficiency = 0.85m; // Typical efficiency
            decimal powerFluidConsumption = productionRate / efficiency;

            return Math.Max(productionRate, powerFluidConsumption);
        }

        /// <summary>
        /// Calculates pump intake pressure.
        /// </summary>
        private static decimal CalculatePumpIntakePressure(HYDRAULIC_PUMP_WELL_PROPERTIES wellProperties)
        {
            // Pump intake pressure is approximately bottom hole pressure
            return wellProperties.BOTTOM_HOLE_PRESSURE;
        }

        /// <summary>
        /// Calculates pump discharge pressure.
        /// </summary>
        private static decimal CalculatePumpDischargePressure(
            HYDRAULIC_PUMP_WELL_PROPERTIES wellProperties,
            HYDRAULIC_PISTON_PUMP_PROPERTIES pumpProperties,
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
            HYDRAULIC_PISTON_PUMP_PROPERTIES pumpProperties,
            decimal powerFluidConsumption)
        {
            // HHP = (Q * P) / 1714
            decimal flowRateGPM = powerFluidConsumption * 42m / 1440m; // GPM
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
            HYDRAULIC_PUMP_WELL_PROPERTIES wellProperties)
        {
            // HHP = (Q * Î”P) / 1714
            decimal flowRateGPM = productionRate * 42m / 1440m; // GPM
            decimal pressureDifferential = dischargePressure - intakePressure;
            decimal horsepower = (flowRateGPM * pressureDifferential) / 1714m;

            return Math.Max(0m, horsepower);
        }
    }
}

