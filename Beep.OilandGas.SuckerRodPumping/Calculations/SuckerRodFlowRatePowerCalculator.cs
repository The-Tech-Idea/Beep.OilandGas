using System;
using Beep.OilandGas.Models.SuckerRodPumping;
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
        public static SuckerRodFlowRatePowerResult CalculateFlowRateAndPower(
            SuckerRodSystemProperties systemProperties,
            SuckerRodLoadResult loadResult)
        {
            if (systemProperties == null)
                throw new ArgumentNullException(nameof(systemProperties));

            if (loadResult == null)
                throw new ArgumentNullException(nameof(loadResult));

            var result = new SuckerRodFlowRatePowerResult();

            // Calculate pump displacement
            result.PumpDisplacement = CalculatePumpDisplacement(systemProperties);

            // Calculate volumetric efficiency
            result.VolumetricEfficiency = CalculateVolumetricEfficiency(systemProperties);

            // Calculate production rate
            result.ProductionRate = result.PumpDisplacement * result.VolumetricEfficiency;

            // Calculate polished rod horsepower
            result.PolishedRodHorsepower = CalculatePolishedRodHorsepower(
                systemProperties, loadResult);

            // Calculate hydraulic horsepower
            result.HydraulicHorsepower = CalculateHydraulicHorsepower(
                systemProperties, result.ProductionRate);

            // Calculate friction horsepower
            result.FrictionHorsepower = CalculateFrictionHorsepower(
                systemProperties, loadResult);

            // Total horsepower
            result.TotalHorsepower = result.PolishedRodHorsepower + 
                                   result.HydraulicHorsepower + 
                                   result.FrictionHorsepower;

            // Motor horsepower (with efficiency)
            decimal motorEfficiency = 0.9m;
            result.MotorHorsepower = result.TotalHorsepower / motorEfficiency;

            // System efficiency
            result.SystemEfficiency = CalculateSystemEfficiency(
                result.HydraulicHorsepower, result.TotalHorsepower);

            // Energy consumption
            result.EnergyConsumption = CalculateEnergyConsumption(
                result.MotorHorsepower);

            return result;
        }

        /// <summary>
        /// Calculates pump displacement.
        /// </summary>
        private static decimal CalculatePumpDisplacement(SuckerRodSystemProperties systemProperties)
        {
            // Pump displacement = (π * D² / 4) * Stroke Length * SPM * 1440 minutes/day
            // Convert to bbl/day

            decimal pumpArea = (decimal)Math.PI * systemProperties.PumpDiameter * 
                             systemProperties.PumpDiameter / 4m; // square inches

            decimal strokeLengthFt = systemProperties.StrokeLength / 12m; // feet
            decimal volumePerStroke = pumpArea * strokeLengthFt / 144m; // cubic feet per stroke

            decimal strokesPerDay = systemProperties.StrokesPerMinute * 1440m; // strokes per day
            decimal volumePerDay = volumePerStroke * strokesPerDay; // cubic feet per day

            // Convert to bbl/day (1 bbl = 5.615 ft³)
            decimal pumpDisplacement = volumePerDay / 5.615m; // bbl/day

            return pumpDisplacement;
        }

        /// <summary>
        /// Calculates volumetric efficiency.
        /// </summary>
        private static decimal CalculateVolumetricEfficiency(SuckerRodSystemProperties systemProperties)
        {
            // Volumetric efficiency depends on:
            // - Gas-oil ratio
            // - Pump efficiency
            // - Fluid properties

            decimal baseEfficiency = systemProperties.PumpEfficiency;

            // Gas effect (reduces efficiency)
            decimal gasEffect = 1.0m;
            if (systemProperties.GasOilRatio > 0)
            {
                // Simplified: efficiency decreases with GOR
                decimal gorFactor = systemProperties.GasOilRatio / 1000m; // Normalize
                gasEffect = 1.0m - (gorFactor * 0.1m); // Up to 10% reduction
                gasEffect = Math.Max(0.5m, gasEffect); // Minimum 50%
            }

            // Water cut effect (slight reduction)
            decimal waterCutEffect = 1.0m - (systemProperties.WaterCut * 0.05m); // Up to 5% reduction

            decimal volumetricEfficiency = baseEfficiency * gasEffect * waterCutEffect;

            return Math.Max(0.3m, Math.Min(0.95m, volumetricEfficiency));
        }

        /// <summary>
        /// Calculates polished rod horsepower.
        /// </summary>
        private static decimal CalculatePolishedRodHorsepower(
            SuckerRodSystemProperties systemProperties,
            SuckerRodLoadResult loadResult)
        {
            // PRHP = (Peak Load - Min Load) * Stroke Length * SPM / 33000
            // Simplified: use load range

            decimal strokeLengthFt = systemProperties.StrokeLength / 12m; // feet
            decimal loadRange = loadResult.LoadRange; // pounds

            decimal prhp = loadRange * strokeLengthFt * systemProperties.StrokesPerMinute / 33000m;

            return Math.Max(0m, prhp);
        }

        /// <summary>
        /// Calculates hydraulic horsepower.
        /// </summary>
        private static decimal CalculateHydraulicHorsepower(
            SuckerRodSystemProperties systemProperties,
            decimal productionRate)
        {
            // HHP = (Q * ΔP) / (1714 * efficiency)
            // Where Q = flow rate in GPM, ΔP = pressure differential in psi

            // Convert production rate to GPM
            decimal flowRateGPM = productionRate * 42m / 1440m; // bbl/day to GPM

            // Pressure differential
            decimal pressureDifferential = systemProperties.BottomHolePressure - 
                                         systemProperties.WellheadPressure; // psi

            // Hydraulic horsepower
            decimal hhp = (flowRateGPM * pressureDifferential) / (1714m * systemProperties.PumpEfficiency);

            return Math.Max(0m, hhp);
        }

        /// <summary>
        /// Calculates friction horsepower.
        /// </summary>
        private static decimal CalculateFrictionHorsepower(
            SuckerRodSystemProperties systemProperties,
            SuckerRodLoadResult loadResult)
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
        public static decimal CalculateProductionRate(SuckerRodSystemProperties systemProperties)
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
            SuckerRodSystemProperties systemProperties,
            SuckerRodLoadResult loadResult)
        {
            var flowRatePowerResult = CalculateFlowRateAndPower(systemProperties, loadResult);
            return flowRatePowerResult.MotorHorsepower;
        }
    }
}

