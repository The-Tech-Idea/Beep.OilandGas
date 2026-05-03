using System;
using Beep.OilandGas.HydraulicPumps.Constants;
using Beep.OilandGas.Models.Data.HydraulicPumps;

namespace Beep.OilandGas.HydraulicPumps.Calculations
{
    /// <summary>
    /// Shared screening formulas for jet and piston hydraulic pump paths (fluid density, tubing friction, discharge pressure).
    /// </summary>
    public static class HydraulicPumpSharedCalculations
    {
        /// <summary>
        /// Oil density from API gravity (lb/ft3), using water reference density.
        /// </summary>
        public static decimal OilDensityLbPerFt3(decimal oilGravityApi) =>
            (141.5m / (131.5m + oilGravityApi)) * HydraulicPumpConstants.WaterDensityLbPerFt3;

        /// <summary>
        /// In-situ liquid density from oil API gravity and water cut (fraction 0..1).
        /// </summary>
        public static decimal LiquidDensityLbPerFt3(decimal oilGravityApi, decimal waterCut) =>
            OilDensityLbPerFt3(oilGravityApi) * (1.0m - waterCut) + HydraulicPumpConstants.WaterDensityLbPerFt3 * waterCut;

        /// <summary>
        /// Simplified tubing friction pressure (psia) vs rate for screening correlations.
        /// </summary>
        public static decimal CalculateFrictionPressurePsi(
            HYDRAULIC_PUMP_WELL_PROPERTIES wellProperties,
            decimal flowRateBblDay)
        {
            decimal tubingArea = (decimal)Math.PI * wellProperties.TUBING_DIAMETER * wellProperties.TUBING_DIAMETER / 4m;
            decimal velocity = (flowRateBblDay * HydraulicPumpConstants.BarrelToCubicFeet) /
                               (HydraulicPumpConstants.SecondsPerDay * tubingArea / HydraulicPumpConstants.SquareInchesPerSquareFoot);

            decimal reynoldsNumber = HydraulicPumpConstants.WaterDensityLbPerFt3 * velocity *
                                     (wellProperties.TUBING_DIAMETER / HydraulicPumpConstants.InchesToFeet) / 0.001m;
            if (reynoldsNumber < 1m)
                reynoldsNumber = 1m;

            decimal frictionFactor = reynoldsNumber < 2000m
                ? 64m / reynoldsNumber
                : 0.3164m / (decimal)Math.Pow((double)reynoldsNumber, 0.25);

            decimal frictionPressure = frictionFactor *
                                       (wellProperties.WELL_DEPTH / (wellProperties.TUBING_DIAMETER / HydraulicPumpConstants.InchesToFeet)) *
                                       (velocity * velocity) / (2m * HydraulicPumpConstants.Gravity) *
                                       HydraulicPumpConstants.WaterDensityLbPerFt3 / HydraulicPumpConstants.SquareInchesPerSquareFoot;

            return Math.Max(0m, frictionPressure);
        }

        /// <summary>
        /// Screening discharge pressure: wellhead + friction + simplified liquid hydrostatic column over well depth.
        /// </summary>
        public static decimal CalculateDischargePressurePsi(
            HYDRAULIC_PUMP_WELL_PROPERTIES wellProperties,
            decimal productionRateBblDay)
        {
            decimal wellheadPressure = wellProperties.WELLHEAD_PRESSURE;
            decimal frictionPressure = CalculateFrictionPressurePsi(wellProperties, productionRateBblDay);
            decimal liquidDensity = LiquidDensityLbPerFt3(wellProperties.OIL_GRAVITY, wellProperties.WATER_CUT);
            decimal hydrostaticPressure = liquidDensity * wellProperties.WELL_DEPTH / HydraulicPumpConstants.SquareInchesPerSquareFoot;

            return wellheadPressure + frictionPressure + hydrostaticPressure;
        }
    }
}
