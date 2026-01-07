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
        public static HydraulicPistonPumpResult CalculatePerformance(
            HydraulicPumpWellProperties wellProperties,
            HydraulicPistonPumpProperties pumpProperties)
        {
            if (wellProperties == null)
                throw new ArgumentNullException(nameof(wellProperties));

            if (pumpProperties == null)
                throw new ArgumentNullException(nameof(pumpProperties));

            var result = new HydraulicPistonPumpResult();

            // Calculate pump displacement
            result.PumpDisplacement = CalculatePumpDisplacement(pumpProperties);

            // Calculate volumetric efficiency
            result.VolumetricEfficiency = CalculateVolumetricEfficiency(
                wellProperties, pumpProperties);

            // Calculate production rate
            result.ProductionRate = result.PumpDisplacement * result.VolumetricEfficiency;

            // Calculate power fluid consumption
            result.PowerFluidConsumption = CalculatePowerFluidConsumption(
                pumpProperties, result.ProductionRate);

            // Calculate pressures
            result.PumpIntakePressure = CalculatePumpIntakePressure(wellProperties);
            result.PumpDischargePressure = CalculatePumpDischargePressure(
                wellProperties, pumpProperties, result.ProductionRate);

            // Calculate horsepower
            result.PowerFluidHorsepower = CalculatePowerFluidHorsepower(
                pumpProperties, result.PowerFluidConsumption);
            result.HydraulicHorsepower = CalculateHydraulicHorsepower(
                result.ProductionRate, result.PumpIntakePressure, result.PumpDischargePressure, wellProperties);

            // System efficiency
            if (result.PowerFluidHorsepower > 0)
            {
                result.SystemEfficiency = result.HydraulicHorsepower / result.PowerFluidHorsepower;
            }
            else
            {
                result.SystemEfficiency = 0m;
            }

            return result;
        }

        /// <summary>
        /// Calculates pump displacement.
        /// </summary>
        private static decimal CalculatePumpDisplacement(HydraulicPistonPumpProperties pumpProperties)
        {
            // Pump displacement = (π * D² / 4) * Stroke Length * SPM * 1440 minutes/day
            // Convert to bbl/day

            decimal pistonArea = (decimal)Math.PI * pumpProperties.PistonDiameter * pumpProperties.PistonDiameter / 4m; // square inches
            decimal strokeLengthFt = pumpProperties.StrokeLength / 12m; // feet
            decimal volumePerStroke = pistonArea * strokeLengthFt / 144m; // cubic feet per stroke

            decimal strokesPerDay = pumpProperties.StrokesPerMinute * 1440m; // strokes per day
            decimal volumePerDay = volumePerStroke * strokesPerDay; // cubic feet per day

            // Convert to bbl/day (1 bbl = 5.615 ft³)
            decimal pumpDisplacement = volumePerDay / 5.615m; // bbl/day

            return pumpDisplacement;
        }

        /// <summary>
        /// Calculates volumetric efficiency.
        /// </summary>
        private static decimal CalculateVolumetricEfficiency(
            HydraulicPumpWellProperties wellProperties,
            HydraulicPistonPumpProperties pumpProperties)
        {
            // Volumetric efficiency depends on:
            // - Gas-oil ratio
            // - Pump design
            // - Operating conditions

            decimal baseEfficiency = 0.85m; // Base efficiency

            // Gas effect (reduces efficiency)
            decimal gasEffect = 1.0m;
            if (wellProperties.GasOilRatio > 0)
            {
                decimal gorFactor = wellProperties.GasOilRatio / 1000m; // Normalize
                gasEffect = 1.0m - (gorFactor * 0.15m); // Up to 15% reduction
                gasEffect = Math.Max(0.5m, gasEffect); // Minimum 50%
            }

            // Water cut effect (slight reduction)
            decimal waterCutEffect = 1.0m - (wellProperties.WaterCut * 0.05m); // Up to 5% reduction

            // Pressure effect
            decimal pressureRatio = wellProperties.BottomHolePressure / pumpProperties.PowerFluidPressure;
            decimal pressureEffect = 1.0m - (decimal)Math.Abs((double)(pressureRatio - 0.5m)) * 0.2m; // Optimal at 0.5
            pressureEffect = Math.Max(0.6m, Math.Min(1.0m, pressureEffect));

            decimal volumetricEfficiency = baseEfficiency * gasEffect * waterCutEffect * pressureEffect;

            return Math.Max(0.3m, Math.Min(0.95m, volumetricEfficiency));
        }

        /// <summary>
        /// Calculates power fluid consumption.
        /// </summary>
        private static decimal CalculatePowerFluidConsumption(
            HydraulicPistonPumpProperties pumpProperties,
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
        private static decimal CalculatePumpIntakePressure(HydraulicPumpWellProperties wellProperties)
        {
            // Pump intake pressure is approximately bottom hole pressure
            return wellProperties.BottomHolePressure;
        }

        /// <summary>
        /// Calculates pump discharge pressure.
        /// </summary>
        private static decimal CalculatePumpDischargePressure(
            HydraulicPumpWellProperties wellProperties,
            HydraulicPistonPumpProperties pumpProperties,
            decimal productionRate)
        {
            // Discharge pressure = wellhead pressure + friction + hydrostatic
            decimal wellheadPressure = wellProperties.WellheadPressure;

            // Friction pressure (simplified)
            decimal frictionPressure = CalculateFrictionPressure(wellProperties, productionRate);

            // Hydrostatic head
            decimal oilDensity = (141.5m / (131.5m + wellProperties.OilGravity)) * 62.4m;
            decimal waterDensity = 62.4m;
            decimal liquidDensity = oilDensity * (1.0m - wellProperties.WaterCut) + waterDensity * wellProperties.WaterCut;
            decimal hydrostaticPressure = liquidDensity * wellProperties.WellDepth / 144m;

            return wellheadPressure + frictionPressure + hydrostaticPressure;
        }

        /// <summary>
        /// Calculates friction pressure.
        /// </summary>
        private static decimal CalculateFrictionPressure(
            HydraulicPumpWellProperties wellProperties,
            decimal flowRate)
        {
            // Simplified friction calculation
            decimal tubingArea = (decimal)Math.PI * wellProperties.TubingDiameter * wellProperties.TubingDiameter / 4m;
            decimal velocity = (flowRate * 5.615m) / (86400m * tubingArea / 144m); // ft/s

            decimal reynoldsNumber = 62.4m * velocity * (wellProperties.TubingDiameter / 12m) / 0.001m;
            decimal frictionFactor = reynoldsNumber < 2000m
                ? 64m / reynoldsNumber
                : 0.3164m / (decimal)Math.Pow((double)reynoldsNumber, 0.25);

            decimal frictionPressure = frictionFactor * (wellProperties.WellDepth / (wellProperties.TubingDiameter / 12m)) *
                                      (velocity * velocity) / (2m * 32.174m) * 62.4m / 144m;

            return Math.Max(0m, frictionPressure);
        }

        /// <summary>
        /// Calculates power fluid horsepower.
        /// </summary>
        private static decimal CalculatePowerFluidHorsepower(
            HydraulicPistonPumpProperties pumpProperties,
            decimal powerFluidConsumption)
        {
            // HHP = (Q * P) / 1714
            decimal flowRateGPM = powerFluidConsumption * 42m / 1440m; // GPM
            decimal horsepower = (flowRateGPM * pumpProperties.PowerFluidPressure) / 1714m;

            return Math.Max(0m, horsepower);
        }

        /// <summary>
        /// Calculates hydraulic horsepower.
        /// </summary>
        private static decimal CalculateHydraulicHorsepower(
            decimal productionRate,
            decimal intakePressure,
            decimal dischargePressure,
            HydraulicPumpWellProperties wellProperties)
        {
            // HHP = (Q * ΔP) / 1714
            decimal flowRateGPM = productionRate * 42m / 1440m; // GPM
            decimal pressureDifferential = dischargePressure - intakePressure;
            decimal horsepower = (flowRateGPM * pressureDifferential) / 1714m;

            return Math.Max(0m, horsepower);
        }
    }
}

