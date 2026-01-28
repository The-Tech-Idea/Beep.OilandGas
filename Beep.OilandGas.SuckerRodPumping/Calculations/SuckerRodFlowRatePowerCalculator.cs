using System;
using Beep.OilandGas.Models.Data.SuckerRodPumping;
using Beep.OilandGas.GasProperties.Calculations;

namespace Beep.OilandGas.SuckerRodPumping.Calculations
{
    /// <summary>
    /// Provides sucker rod flow rate and power calculations.
    /// </summary>
    public static class SuckerRodFlowRatePowerCalculator
    {
        /// <summary>
        /// Calculates production rate and power requirements.
        /// </summary>
        /// <param name="systemProperties">Sucker rod system properties.</param>
        /// <param name="loadResult">Load analysis results.</param>
        /// <returns>Flow rate and power results.</returns>
        public static SUCKER_ROD_FLOW_RATE_POWER_RESULT CalculateFlowRateAndPower(
            SUCKER_ROD_SYSTEM_PROPERTIES systemProperties,
            SUCKER_ROD_LOAD_RESULT loadResult)
        {
            if (systemProperties == null)
                throw new ArgumentNullException(nameof(systemProperties));

            if (loadResult == null)
                throw new ArgumentNullException(nameof(loadResult));

            var result = new SUCKER_ROD_FLOW_RATE_POWER_RESULT();

            // Calculate pump displacement
            result.PUMP_DISPLACEMENT = CalculatePumpDisplacement(systemProperties);

            // Calculate volumetric efficiency
            result.VOLUMETRIC_EFFICIENCY = CalculateVolumetricEfficiency(systemProperties);

            // Calculate production rate
            result.PRODUCTION_RATE = result.PUMP_DISPLACEMENT * result.VOLUMETRIC_EFFICIENCY;

            // Calculate polished rod horsepower
            result.POLISHED_ROD_HORSEPOWER = CalculatePolishedRodHorsepower(
                systemProperties, loadResult);

            // Calculate hydraulic horsepower
            result.HYDRAULIC_HORSEPOWER = CalculateHydraulicHorsepower(
                systemProperties, result.PRODUCTION_RATE);

            // Calculate friction horsepower
            result.FRICTION_HORSEPOWER = CalculateFrictionHorsepower(
                systemProperties, loadResult);

            // Total horsepower
            result.TOTAL_HORSEPOWER = result.POLISHED_ROD_HORSEPOWER + 
                                   result.HYDRAULIC_HORSEPOWER + 
                                   result.FRICTION_HORSEPOWER;

            // Motor horsepower (with efficiency)
            decimal motorEfficiency = 0.9m;
            result.MOTOR_HORSEPOWER = result.TOTAL_HORSEPOWER / motorEfficiency;

            // System efficiency
            result.SYSTEM_EFFICIENCY = CalculateSystemEfficiency(
                result.HYDRAULIC_HORSEPOWER, result.TOTAL_HORSEPOWER);

            // Energy consumption
            result.ENERGY_CONSUMPTION = CalculateEnergyConsumption(
                result.MOTOR_HORSEPOWER);

            return result;
        }

        /// <summary>
        /// Calculates pump displacement.
        /// </summary>
        private static decimal CalculatePumpDisplacement(SUCKER_ROD_SYSTEM_PROPERTIES systemProperties)
        {
            // Pump displacement = (Ï€ * DÂ² / 4) * Stroke Length * SPM * 1440 minutes/day
            // Convert to bbl/day

            decimal pumpArea = (decimal)Math.PI * systemProperties.PUMP_DIAMETER * 
                             systemProperties.PUMP_DIAMETER / 4m; // square inches

            decimal strokeLengthFt = systemProperties.STROKE_LENGTH / 12m; // feet
            decimal volumePerStroke = pumpArea * strokeLengthFt / 144m; // cubic feet per stroke

            decimal strokesPerDay = systemProperties.STROKES_PER_MINUTE * 1440m; // strokes per day
            decimal volumePerDay = volumePerStroke * strokesPerDay; // cubic feet per day

            // Convert to bbl/day (1 bbl = 5.615 ftÂ³)
            decimal pumpDisplacement = volumePerDay / 5.615m; // bbl/day

            return pumpDisplacement;
        }

        /// <summary>
        /// Calculates volumetric efficiency.
        /// </summary>
        private static decimal CalculateVolumetricEfficiency(SUCKER_ROD_SYSTEM_PROPERTIES systemProperties)
        {
            // Volumetric efficiency depends on:
            // - Gas-oil ratio
            // - Pump efficiency
            // - Fluid properties

            decimal baseEfficiency = systemProperties.PUMP_EFFICIENCY;

            // Gas effect (reduces efficiency)
            decimal gasEffect = 1.0m;
            if (systemProperties.GAS_OIL_RATIO > 0)
            {
                // Simplified: efficiency decreases with GOR
                decimal gorFactor = systemProperties.GAS_OIL_RATIO / 1000m; // Normalize
                gasEffect = 1.0m - (gorFactor * 0.1m); // Up to 10% reduction
                gasEffect = Math.Max(0.5m, gasEffect); // Minimum 50%
            }

            // Water cut effect (slight reduction)
            decimal waterCutEffect = 1.0m - (systemProperties.WATER_CUT * 0.05m); // Up to 5% reduction

            decimal volumetricEfficiency = baseEfficiency * gasEffect * waterCutEffect;

            return Math.Max(0.3m, Math.Min(0.95m, volumetricEfficiency));
        }

        /// <summary>
        /// Calculates polished rod horsepower.
        /// </summary>
        private static decimal CalculatePolishedRodHorsepower(
            SUCKER_ROD_SYSTEM_PROPERTIES systemProperties,
            SUCKER_ROD_LOAD_RESULT loadResult)
        {
            // PRHP = (Peak Load - Min Load) * Stroke Length * SPM / 33000
            // Simplified: use load range

            decimal strokeLengthFt = systemProperties.STROKE_LENGTH / 12m; // feet
            decimal loadRange = loadResult.LOAD_RANGE; // pounds

            decimal prhp = loadRange * strokeLengthFt * systemProperties.STROKES_PER_MINUTE / 33000m;

            return Math.Max(0m, prhp);
        }

        /// <summary>
        /// Calculates hydraulic horsepower.
        /// </summary>
        private static decimal CalculateHydraulicHorsepower(
            SUCKER_ROD_SYSTEM_PROPERTIES systemProperties,
            decimal productionRate)
        {
            // HHP = (Q * Î”P) / (1714 * efficiency)
            // Where Q = flow rate in GPM, Î”P = pressure differential in psi

            // Convert production rate to GPM
            decimal flowRateGPM = productionRate * 42m / 1440m; // bbl/day to GPM

            // Pressure differential
            decimal pressureDifferential = systemProperties.BOTTOM_HOLE_PRESSURE - 
                                         systemProperties.WELLHEAD_PRESSURE; // psi

            // Hydraulic horsepower
            decimal hhp = (flowRateGPM * pressureDifferential) / (1714m * systemProperties.PUMP_EFFICIENCY);

            return Math.Max(0m, hhp);
        }

        /// <summary>
        /// Calculates friction horsepower.
        /// </summary>
        private static decimal CalculateFrictionHorsepower(
            SUCKER_ROD_SYSTEM_PROPERTIES systemProperties,
            SUCKER_ROD_LOAD_RESULT loadResult)
        {
            // Friction horsepower is typically 10-20% of polished rod horsepower
            // Simplified calculation

            // Calculate PRHP first
            decimal prhp = CalculatePolishedRodHorsepower(systemProperties, loadResult);
            
            decimal frictionFactor = 0.15m; // 15% of PRHP
            decimal frictionHorsepower = prhp * frictionFactor;

            return Math.Max(0m, frictionHorsepower);
        }

        /// <summary>
        /// Calculates system efficiency.
        /// </summary>
        private static decimal CalculateSystemEfficiency(
            decimal hydraulicHorsepower,
            decimal totalHorsepower)
        {
            if (totalHorsepower <= 0)
                return 0m;

            return hydraulicHorsepower / totalHorsepower;
        }

        /// <summary>
        /// Calculates energy consumption.
        /// </summary>
        private static decimal CalculateEnergyConsumption(decimal motorHorsepower)
        {
            // Energy = Power * Time
            // Convert HP to kW: 1 HP = 0.746 kW
            decimal powerKW = motorHorsepower * 0.746m;

            // Daily energy consumption (24 hours)
            decimal energyConsumption = powerKW * 24m; // kWh/day

            return Math.Max(0m, energyConsumption);
        }

        /// <summary>
        /// Calculates production rate for given system properties.
        /// </summary>
        public static decimal CalculateProductionRate(SUCKER_ROD_SYSTEM_PROPERTIES systemProperties)
        {
            // Calculate pump displacement
            decimal pumpDisplacement = CalculatePumpDisplacement(systemProperties);

            // Calculate volumetric efficiency
            decimal volumetricEfficiency = CalculateVolumetricEfficiency(systemProperties);

            // Production rate
            return pumpDisplacement * volumetricEfficiency;
        }

        /// <summary>
        /// Calculates power requirements for given system properties.
        /// </summary>
        public static decimal CalculatePowerRequirements(
            SUCKER_ROD_SYSTEM_PROPERTIES systemProperties,
            SUCKER_ROD_LOAD_RESULT loadResult)
        {
            var flowRatePowerResult = CalculateFlowRateAndPower(systemProperties, loadResult);
            return flowRatePowerResult.MOTOR_HORSEPOWER;
        }
    }
}

