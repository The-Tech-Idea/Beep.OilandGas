using System;
using Beep.OilandGas.HydraulicPumps.Constants;
using Beep.OilandGas.HydraulicPumps.Validation;
using Beep.OilandGas.Models.Data.HydraulicPumps;

namespace Beep.OilandGas.HydraulicPumps.Calculations
{
    /// <summary>
    /// Hydraulic piston pump screening calculations (displacement, volumetric efficiency, HP).
    /// </summary>
    public static class HydraulicPistonPumpCalculator
    {
        /// <summary>
        /// Calculates hydraulic piston pump performance.
        /// </summary>
        /// <param name="wellProperties">Well properties.</param>
        /// <param name="pumpProperties">Piston pump properties.</param>
        /// <param name="validateInputs">When true, runs <see cref="HydraulicPumpValidator.ValidatePistonPumpCalculationParameters"/> before computing.</param>
        public static HYDRAULIC_PISTON_PUMP_RESULT CalculatePerformance(
            HYDRAULIC_PUMP_WELL_PROPERTIES wellProperties,
            HYDRAULIC_PISTON_PUMP_PROPERTIES pumpProperties,
            bool validateInputs = false)
        {
            if (wellProperties == null)
                throw new ArgumentNullException(nameof(wellProperties));

            if (pumpProperties == null)
                throw new ArgumentNullException(nameof(pumpProperties));

            if (validateInputs)
                HydraulicPumpValidator.ValidatePistonPumpCalculationParameters(wellProperties, pumpProperties);

            var result = new HYDRAULIC_PISTON_PUMP_RESULT();

            result.PUMP_DISPLACEMENT = CalculatePumpDisplacement(pumpProperties);
            result.VOLUMETRIC_EFFICIENCY = CalculateVolumetricEfficiency(wellProperties, pumpProperties);
            result.PRODUCTION_RATE = result.PUMP_DISPLACEMENT * result.VOLUMETRIC_EFFICIENCY;
            result.POWER_FLUID_CONSUMPTION = CalculatePowerFluidConsumption(pumpProperties, result.PRODUCTION_RATE);

            result.PUMP_INTAKE_PRESSURE = CalculatePumpIntakePressure(wellProperties);
            result.PUMP_DISCHARGE_PRESSURE = HydraulicPumpSharedCalculations.CalculateDischargePressurePsi(
                wellProperties, result.PRODUCTION_RATE);

            result.POWER_FLUID_HORSEPOWER = CalculatePowerFluidHorsepower(
                pumpProperties, result.POWER_FLUID_CONSUMPTION);
            result.HYDRAULIC_HORSEPOWER = CalculateHydraulicHorsepower(
                result.PRODUCTION_RATE, result.PUMP_INTAKE_PRESSURE, result.PUMP_DISCHARGE_PRESSURE);

            if (result.POWER_FLUID_HORSEPOWER > 0)
            {
                decimal raw = result.HYDRAULIC_HORSEPOWER / result.POWER_FLUID_HORSEPOWER;
                result.SYSTEM_EFFICIENCY = Math.Max(0m, Math.Min(1m, raw));
            }
            else
            {
                result.SYSTEM_EFFICIENCY = 0m;
            }

            return result;
        }

        private static decimal CalculatePumpDisplacement(HYDRAULIC_PISTON_PUMP_PROPERTIES pumpProperties)
        {
            decimal pistonArea = (decimal)Math.PI * pumpProperties.PISTON_DIAMETER * pumpProperties.PISTON_DIAMETER / 4m;
            decimal strokeLengthFt = pumpProperties.STROKE_LENGTH / HydraulicPumpConstants.InchesToFeet;
            decimal volumePerStroke = pistonArea * strokeLengthFt / HydraulicPumpConstants.SquareInchesPerSquareFoot;

            decimal strokesPerDay = pumpProperties.STROKES_PER_MINUTE * HydraulicPumpConstants.MinutesPerDay;
            decimal volumePerDay = volumePerStroke * strokesPerDay;

            return volumePerDay / HydraulicPumpConstants.BarrelToCubicFeet;
        }

        private static decimal CalculateVolumetricEfficiency(
            HYDRAULIC_PUMP_WELL_PROPERTIES wellProperties,
            HYDRAULIC_PISTON_PUMP_PROPERTIES pumpProperties)
        {
            decimal baseEfficiency = HydraulicPumpConstants.StandardPistonPumpEfficiency;

            decimal gasEffect = 1.0m;
            if (wellProperties.GAS_OIL_RATIO > 0)
            {
                decimal gorFactor = wellProperties.GAS_OIL_RATIO / 1000m;
                gasEffect = 1.0m - (gorFactor * 0.15m);
                gasEffect = Math.Max(0.5m, gasEffect);
            }

            decimal waterCutEffect = 1.0m - (wellProperties.WATER_CUT * 0.05m);

            decimal pressureRatio = wellProperties.BOTTOM_HOLE_PRESSURE / pumpProperties.POWER_FLUID_PRESSURE;
            decimal pressureEffect = 1.0m - (decimal)Math.Abs((double)(pressureRatio - 0.5m)) * 0.2m;
            pressureEffect = Math.Max(0.6m, Math.Min(1.0m, pressureEffect));

            decimal volumetricEfficiency = baseEfficiency * gasEffect * waterCutEffect * pressureEffect;

            return Math.Max(
                HydraulicPumpConstants.MinimumVolumetricEfficiency,
                Math.Min(HydraulicPumpConstants.MaximumVolumetricEfficiency, volumetricEfficiency));
        }

        private static decimal CalculatePowerFluidConsumption(
            HYDRAULIC_PISTON_PUMP_PROPERTIES pumpProperties,
            decimal productionRate)
        {
            _ = pumpProperties; // reserved for slip / rod-volume models
            decimal efficiency = HydraulicPumpConstants.StandardPistonPumpEfficiency;
            decimal powerFluidConsumption = productionRate / efficiency;
            return Math.Max(productionRate, powerFluidConsumption);
        }

        private static decimal CalculatePumpIntakePressure(HYDRAULIC_PUMP_WELL_PROPERTIES wellProperties) =>
            wellProperties.BOTTOM_HOLE_PRESSURE;

        private static decimal CalculatePowerFluidHorsepower(
            HYDRAULIC_PISTON_PUMP_PROPERTIES pumpProperties,
            decimal powerFluidConsumption)
        {
            decimal flowRateGpm = powerFluidConsumption * HydraulicPumpConstants.BarrelToGallons / HydraulicPumpConstants.MinutesPerDay;
            return Math.Max(0m, (flowRateGpm * pumpProperties.POWER_FLUID_PRESSURE) / HydraulicPumpConstants.HorsepowerConversionFactor);
        }

        private static decimal CalculateHydraulicHorsepower(
            decimal productionRate,
            decimal intakePressure,
            decimal dischargePressure)
        {
            decimal flowRateGpm = productionRate * HydraulicPumpConstants.BarrelToGallons / HydraulicPumpConstants.MinutesPerDay;
            decimal pressureDifferential = dischargePressure - intakePressure;
            return Math.Max(0m, (flowRateGpm * pressureDifferential) / HydraulicPumpConstants.HorsepowerConversionFactor);
        }
    }
}
