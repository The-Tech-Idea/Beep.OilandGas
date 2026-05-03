using System;
using Beep.OilandGas.HydraulicPumps.Constants;
using Beep.OilandGas.HydraulicPumps.Validation;
using Beep.OilandGas.Models.Data.HydraulicPumps;

namespace Beep.OilandGas.HydraulicPumps.Calculations
{
    /// <summary>
    /// Hydraulic jet pump screening calculations (rates, pressures, HP, simplified efficiency).
    /// </summary>
    public static class HydraulicJetPumpCalculator
    {
        /// <summary>
        /// Calculates hydraulic jet pump performance.
        /// </summary>
        /// <param name="wellProperties">Well properties.</param>
        /// <param name="pumpProperties">Jet pump properties.</param>
        /// <param name="validateInputs">When true, runs <see cref="HydraulicPumpValidator.ValidateJetPumpCalculationParameters"/> before computing.</param>
        /// <returns>Jet pump performance results.</returns>
        public static HYDRAULIC_JET_PUMP_RESULT CalculatePerformance(
            HYDRAULIC_PUMP_WELL_PROPERTIES wellProperties,
            HYDRAULIC_JET_PUMP_PROPERTIES pumpProperties,
            bool validateInputs = false)
        {
            if (wellProperties == null)
                throw new ArgumentNullException(nameof(wellProperties));

            if (pumpProperties == null)
                throw new ArgumentNullException(nameof(pumpProperties));

            if (validateInputs)
                HydraulicPumpValidator.ValidateJetPumpCalculationParameters(wellProperties, pumpProperties);

            var result = new HYDRAULIC_JET_PUMP_RESULT();

            decimal nozzleArea = (decimal)Math.PI * pumpProperties.NOZZLE_DIAMETER * pumpProperties.NOZZLE_DIAMETER / 4m;
            decimal throatArea = (decimal)Math.PI * pumpProperties.THROAT_DIAMETER * pumpProperties.THROAT_DIAMETER / 4m;
            decimal areaRatio = nozzleArea / throatArea;

            decimal liquidDensity = HydraulicPumpSharedCalculations.LiquidDensityLbPerFt3(
                wellProperties.OIL_GRAVITY, wellProperties.WATER_CUT);
            decimal powerFluidDensity = pumpProperties.POWER_FLUID_SPECIFIC_GRAVITY * HydraulicPumpConstants.WaterDensityLbPerFt3;

            result.PRODUCTION_RATE = CalculateProductionRate(
                wellProperties, pumpProperties, areaRatio, liquidDensity, powerFluidDensity);

            result.TOTAL_FLOW_RATE = result.PRODUCTION_RATE + pumpProperties.POWER_FLUID_RATE;

            if (pumpProperties.POWER_FLUID_RATE > 0)
                result.PRODUCTION_RATIO = result.PRODUCTION_RATE / pumpProperties.POWER_FLUID_RATE;
            else
                result.PRODUCTION_RATIO = 0m;

            result.PUMP_EFFICIENCY = CalculatePumpEfficiency(
                areaRatio, result.PRODUCTION_RATIO, liquidDensity, powerFluidDensity);

            result.PUMP_INTAKE_PRESSURE = CalculatePumpIntakePressure(wellProperties);
            result.PUMP_DISCHARGE_PRESSURE = HydraulicPumpSharedCalculations.CalculateDischargePressurePsi(
                wellProperties, result.PRODUCTION_RATE);

            result.POWER_FLUID_HORSEPOWER = CalculatePowerFluidHorsepower(pumpProperties);
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

        private static decimal CalculateProductionRate(
            HYDRAULIC_PUMP_WELL_PROPERTIES wellProperties,
            HYDRAULIC_JET_PUMP_PROPERTIES pumpProperties,
            decimal areaRatio,
            decimal liquidDensity,
            decimal powerFluidDensity)
        {
            decimal pressureDifferential = pumpProperties.POWER_FLUID_PRESSURE - wellProperties.BOTTOM_HOLE_PRESSURE;

            decimal productionRate = pumpProperties.POWER_FLUID_RATE * areaRatio *
                                     (liquidDensity / powerFluidDensity) *
                                     (pressureDifferential / 100m);

            productionRate *= 0.6m;

            return Math.Max(0m, productionRate);
        }

        private static decimal CalculatePumpEfficiency(
            decimal areaRatio,
            decimal productionRatio,
            decimal liquidDensity,
            decimal powerFluidDensity)
        {
            decimal optimalAreaRatio = HydraulicPumpConstants.OptimalJetPumpAreaRatio;
            decimal areaRatioEfficiency = 1.0m - (decimal)Math.Abs((double)(areaRatio - optimalAreaRatio)) * 2m;
            areaRatioEfficiency = Math.Max(HydraulicPumpConstants.MinimumJetPumpEfficiency, Math.Min(1.0m, areaRatioEfficiency));

            decimal productionRatioEfficiency = 1.0m;
            if (productionRatio > 2m)
                productionRatioEfficiency = 1.0m - (productionRatio - 2m) * 0.1m;
            productionRatioEfficiency = Math.Max(0.3m, Math.Min(1.0m, productionRatioEfficiency));

            decimal densityRatio = liquidDensity / powerFluidDensity;
            decimal densityEfficiency = 1.0m - (decimal)Math.Abs((double)(densityRatio - 1.0m)) * 0.2m;
            densityEfficiency = Math.Max(0.4m, Math.Min(1.0m, densityEfficiency));

            return areaRatioEfficiency * productionRatioEfficiency * densityEfficiency;
        }

        private static decimal CalculatePumpIntakePressure(HYDRAULIC_PUMP_WELL_PROPERTIES wellProperties) =>
            wellProperties.BOTTOM_HOLE_PRESSURE;

        private static decimal CalculatePowerFluidHorsepower(HYDRAULIC_JET_PUMP_PROPERTIES pumpProperties)
        {
            decimal flowRateGpm = pumpProperties.POWER_FLUID_RATE * HydraulicPumpConstants.BarrelToGallons /
                                  HydraulicPumpConstants.MinutesPerDay;
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
