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
        public static HydraulicJetPumpResult CalculatePerformance(
            HydraulicPumpWellProperties wellProperties,
            HydraulicJetPumpProperties pumpProperties)
        {
            if (wellProperties == null)
                throw new ArgumentNullException(nameof(wellProperties));

            if (pumpProperties == null)
                throw new ArgumentNullException(nameof(pumpProperties));

            var result = new HydraulicJetPumpResult();

            // Calculate area ratios
            decimal nozzleArea = (decimal)Math.PI * pumpProperties.NozzleDiameter * pumpProperties.NozzleDiameter / 4m;
            decimal throatArea = (decimal)Math.PI * pumpProperties.ThroatDiameter * pumpProperties.ThroatDiameter / 4m;
            decimal areaRatio = nozzleArea / throatArea;

            // Calculate fluid properties
            decimal oilDensity = (141.5m / (131.5m + wellProperties.OilGravity)) * 62.4m; // lb/ft³
            decimal waterDensity = 62.4m; // lb/ft³
            decimal liquidDensity = oilDensity * (1.0m - wellProperties.WaterCut) + waterDensity * wellProperties.WaterCut;
            decimal powerFluidDensity = pumpProperties.PowerFluidSpecificGravity * 62.4m;

            // Calculate production rate
            result.ProductionRate = CalculateProductionRate(
                wellProperties, pumpProperties, areaRatio, liquidDensity, powerFluidDensity);

            // Calculate total flow rate
            result.TotalFlowRate = result.ProductionRate + pumpProperties.PowerFluidRate;

            // Calculate production ratio
            if (pumpProperties.PowerFluidRate > 0)
            {
                result.ProductionRatio = result.ProductionRate / pumpProperties.PowerFluidRate;
            }
            else
            {
                result.ProductionRatio = 0m;
            }

            // Calculate pump efficiency
            result.PumpEfficiency = CalculatePumpEfficiency(
                areaRatio, result.ProductionRatio, liquidDensity, powerFluidDensity);

            // Calculate pressures
            result.PumpIntakePressure = CalculatePumpIntakePressure(wellProperties);
            result.PumpDischargePressure = CalculatePumpDischargePressure(
                wellProperties, pumpProperties, result.ProductionRate);

            // Calculate horsepower
            result.PowerFluidHorsepower = CalculatePowerFluidHorsepower(
                pumpProperties, powerFluidDensity);
            result.HydraulicHorsepower = CalculateHydraulicHorsepower(
                result.ProductionRate, result.PumpIntakePressure, result.PumpDischargePressure, liquidDensity);

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
        /// Calculates production rate.
        /// </summary>
        private static decimal CalculateProductionRate(
            HydraulicPumpWellProperties wellProperties,
            HydraulicJetPumpProperties pumpProperties,
            decimal areaRatio,
            decimal liquidDensity,
            decimal powerFluidDensity)
        {
            // Jet pump production rate calculation
            // Based on momentum transfer from power fluid to production fluid

            // Nozzle velocity
            decimal nozzleArea = (decimal)Math.PI * pumpProperties.NozzleDiameter * pumpProperties.NozzleDiameter / 4m;
            decimal powerFluidRateFt3PerSec = pumpProperties.PowerFluidRate * 5.615m / 86400m; // ft³/s
            decimal nozzleVelocity = powerFluidRateFt3PerSec / (nozzleArea / 144m); // ft/s

            // Pressure differential
            decimal pressureDifferential = pumpProperties.PowerFluidPressure - wellProperties.BottomHolePressure;

            // Production rate (simplified)
            decimal productionRate = pumpProperties.PowerFluidRate * areaRatio * 
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
        private static decimal CalculatePumpIntakePressure(HydraulicPumpWellProperties wellProperties)
        {
            // Pump intake pressure is approximately bottom hole pressure
            // Adjusted for depth and fluid column
            return wellProperties.BottomHolePressure;
        }

        /// <summary>
        /// Calculates pump discharge pressure.
        /// </summary>
        private static decimal CalculatePumpDischargePressure(
            HydraulicPumpWellProperties wellProperties,
            HydraulicJetPumpProperties pumpProperties,
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
            HydraulicJetPumpProperties pumpProperties,
            decimal powerFluidDensity)
        {
            // HHP = (Q * P) / (1714 * efficiency)
            decimal flowRateGPM = pumpProperties.PowerFluidRate * 42m / 1440m; // GPM
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
            decimal liquidDensity)
        {
            // HHP = (Q * ΔP) / 1714
            decimal flowRateGPM = productionRate * 42m / 1440m; // GPM
            decimal pressureDifferential = dischargePressure - intakePressure;
            decimal horsepower = (flowRateGPM * pressureDifferential) / 1714m;

            return Math.Max(0m, horsepower);
        }
    }
}

