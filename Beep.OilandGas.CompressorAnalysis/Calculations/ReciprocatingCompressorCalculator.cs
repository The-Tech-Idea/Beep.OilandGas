using System;
using Beep.OilandGas.Models.CompressorAnalysis;
using Beep.OilandGas.GasProperties.Calculations;

namespace Beep.OilandGas.CompressorAnalysis.Calculations
{
    /// <summary>
    /// Provides reciprocating compressor power calculations.
    /// </summary>
    public static class ReciprocatingCompressorCalculator
    {
        /// <summary>
        /// Calculates reciprocating compressor power requirements.
        /// </summary>
        /// <param name="compressorProperties">Reciprocating compressor properties.</param>
        /// <param name="useSIUnits">Whether to use SI units (false = US field units).</param>
        /// <returns>Compressor power calculation results.</returns>
        public static CompressorPowerResult CalculatePower(
            ReciprocatingCompressorProperties compressorProperties,
            bool useSIUnits = false)
        {
            if (compressorProperties == null)
                throw new ArgumentNullException(nameof(compressorProperties));

            if (compressorProperties.OperatingConditions == null)
                throw new ArgumentNullException(nameof(compressorProperties.OperatingConditions));

            var result = new CompressorPowerResult();

            var conditions = compressorProperties.OperatingConditions;

            // Calculate compression ratio
            result.CompressionRatio = conditions.DischargePressure / conditions.SuctionPressure;

            // Calculate average pressure and temperature
            decimal averagePressure = (conditions.SuctionPressure + conditions.DischargePressure) / 2m;
            decimal averageTemperature = (conditions.SuctionTemperature + conditions.DischargeTemperature) / 2m;

            // Calculate Z-factor at average conditions
            decimal zFactor = ZFactorCalculator.CalculateBrillBeggs(
                averagePressure, averageTemperature, conditions.GasSpecificGravity);

            // Calculate adiabatic head
            result.AdiabaticHead = CalculateAdiabaticHead(
                conditions.SuctionPressure,
                conditions.DischargePressure,
                conditions.SuctionTemperature,
                1.3m, // Typical k for gas
                zFactor,
                conditions.GasSpecificGravity);

            // Calculate theoretical power
            result.TheoreticalPower = CalculateTheoreticalPower(
                compressorProperties,
                conditions,
                result.CompressionRatio,
                zFactor);

            // Calculate brake horsepower
            decimal compressorEfficiency = 0.85m; // Typical for reciprocating
            result.BrakeHorsepower = result.TheoreticalPower / compressorEfficiency;

            // Calculate motor horsepower
            result.MotorHorsepower = result.BrakeHorsepower / conditions.MechanicalEfficiency;

            // Calculate power consumption
            result.PowerConsumptionKW = result.MotorHorsepower * 0.746m;

            // Calculate discharge temperature
            result.DischargeTemperature = CalculateDischargeTemperature(
                conditions.SuctionTemperature,
                result.CompressionRatio,
                1.3m); // Typical k

            // Overall efficiency
            result.OverallEfficiency = compressorEfficiency * conditions.MechanicalEfficiency;

            // Polytropic head (not typically used for reciprocating, but set for consistency)
            result.PolytropicHead = result.AdiabaticHead;

            return result;
        }

        /// <summary>
        /// Calculates adiabatic head.
        /// </summary>
        private static decimal CalculateAdiabaticHead(
            decimal suctionPressure,
            decimal dischargePressure,
            decimal suctionTemperature,
            decimal specificHeatRatio,
            decimal zFactor,
            decimal gasSpecificGravity)
        {
            decimal k = specificHeatRatio;
            decimal compressionRatio = dischargePressure / suctionPressure;
            decimal zAvg = zFactor;
            decimal R = 1545.0m;
            decimal molecularWeight = gasSpecificGravity * 28.9645m;

            decimal head = (zAvg * R * suctionTemperature / molecularWeight) *
                          (k / (k - 1m)) *
                          ((decimal)Math.Pow((double)compressionRatio, (double)((k - 1m) / k)) - 1m);

            return Math.Max(0m, head);
        }

        /// <summary>
        /// Calculates theoretical power for reciprocating compressor.
        /// </summary>
        private static decimal CalculateTheoreticalPower(
            ReciprocatingCompressorProperties compressorProperties,
            CompressorOperatingConditions conditions,
            decimal compressionRatio,
            decimal zFactor)
        {
            // Reciprocating compressor power calculation
            // Based on cylinder displacement and compression work

            // Calculate cylinder displacement
            decimal cylinderArea = (decimal)Math.PI * compressorProperties.CylinderDiameter *
                                  compressorProperties.CylinderDiameter / 4m; // square inches
            decimal strokeLengthFt = compressorProperties.StrokeLength / 12m; // feet
            decimal displacementPerCylinder = cylinderArea * strokeLengthFt / 144m; // cubic feet

            // Calculate displacement per minute
            decimal displacementPerMinute = displacementPerCylinder *
                                           compressorProperties.RotationalSpeed *
                                           compressorProperties.NumberOfCylinders; // ft³/min

            // Apply volumetric efficiency
            decimal actualDisplacement = displacementPerMinute * compressorProperties.VolumetricEfficiency;

            // Calculate compression work
            decimal k = 1.3m; // Typical for gas
            decimal compressionWork = (k / (k - 1m)) *
                                     conditions.SuctionPressure * 144m * // Convert to psf
                                     actualDisplacement *
                                     ((decimal)Math.Pow((double)compressionRatio, (double)((k - 1m) / k)) - 1m);

            // Convert to horsepower
            decimal theoreticalPower = compressionWork / 33000m; // ft-lbf/min to HP

            return Math.Max(0m, theoreticalPower);
        }

        /// <summary>
        /// Calculates discharge temperature.
        /// </summary>
        private static decimal CalculateDischargeTemperature(
            decimal suctionTemperature,
            decimal compressionRatio,
            decimal specificHeatRatio)
        {
            decimal k = specificHeatRatio;

            decimal dischargeTemperature = suctionTemperature *
                                         (decimal)Math.Pow((double)compressionRatio, (double)((k - 1m) / k));

            return Math.Max(suctionTemperature, dischargeTemperature);
        }
    }
}

