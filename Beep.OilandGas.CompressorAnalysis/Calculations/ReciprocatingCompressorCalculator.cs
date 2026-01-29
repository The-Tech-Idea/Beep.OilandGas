using System;
using Beep.OilandGas.Models.Data.CompressorAnalysis;
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
        public static COMPRESSOR_POWER_RESULT CalculatePower(
            RECIPROCATING_COMPRESSOR_PROPERTIES compressorProperties,
            bool useSIUnits = false)
        {
            if (compressorProperties == null)
                throw new ArgumentNullException(nameof(compressorProperties));

            if (compressorProperties.OPERATING_CONDITIONS == null)
                throw new ArgumentNullException(nameof(compressorProperties.OPERATING_CONDITIONS));

            var result = new COMPRESSOR_POWER_RESULT();

            var conditions = compressorProperties.OPERATING_CONDITIONS;

            // Calculate compression ratio
            result.COMPRESSION_RATIO = conditions.DISCHARGE_PRESSURE / conditions.SUCTION_PRESSURE;

            // Calculate average pressure and temperature
            decimal averagePressure = (conditions.SUCTION_PRESSURE + conditions.DISCHARGE_PRESSURE) / 2m;
            decimal averageTemperature = (conditions.SUCTION_TEMPERATURE + conditions.DISCHARGE_TEMPERATURE) / 2m;

            // Calculate Z-factor at average conditions
            decimal zFactor = ZFactorCalculator.CalculateBrillBeggs(
                averagePressure, averageTemperature, conditions.GAS_SPECIFIC_GRAVITY);

            // Calculate adiabatic head
            result.ADIABATIC_HEAD = CalculateAdiabaticHead(
                conditions.SUCTION_PRESSURE,
                conditions.DISCHARGE_PRESSURE,
                conditions.SUCTION_TEMPERATURE,
                1.3m, // Typical k for gas
                zFactor,
                conditions.GAS_SPECIFIC_GRAVITY);

            // Calculate theoretical power
            result.THEORETICAL_POWER = CalculateTheoreticalPower(
                compressorProperties,
                conditions,
                result.COMPRESSION_RATIO,
                zFactor);

            // Calculate brake horsepower
            decimal COMPRESSOR_EFFICIENCY = 0.85m; // Typical for reciprocating
            result.BRAKE_HORSEPOWER = result.THEORETICAL_POWER / COMPRESSOR_EFFICIENCY;

            // Calculate motor horsepower
            result.MOTOR_HORSEPOWER = result.BRAKE_HORSEPOWER / conditions.MECHANICAL_EFFICIENCY;

            // Calculate power consumption
            result.POWER_CONSUMPTION_KW = result.MOTOR_HORSEPOWER * 0.746m;

            // Calculate discharge temperature
            result.DISCHARGE_TEMPERATURE = CalculateDischargeTemperature(
                conditions.SUCTION_TEMPERATURE,
                result.COMPRESSION_RATIO,
                1.3m); // Typical k

            // Overall efficiency
            result.OVERALL_EFFICIENCY = COMPRESSOR_EFFICIENCY * conditions.MECHANICAL_EFFICIENCY;

            // Polytropic head (not typically used for reciprocating, but set for consistency)
            result.POLYTROPIC_HEAD = result.ADIABATIC_HEAD;

            return result;
        }

        /// <summary>
        /// Calculates adiabatic head.
        /// </summary>
        private static decimal CalculateAdiabaticHead(
            decimal SUCTION_PRESSURE,
            decimal DISCHARGE_PRESSURE,
            decimal SUCTION_TEMPERATURE,
            decimal SPECIFIC_HEAT_RATIO,
            decimal zFactor,
            decimal GAS_SPECIFIC_GRAVITY)
        {
            decimal k = SPECIFIC_HEAT_RATIO;
            decimal compressionRatio = DISCHARGE_PRESSURE / SUCTION_PRESSURE;
            decimal zAvg = zFactor;
            decimal R = 1545.0m;
            decimal molecularWeight = GAS_SPECIFIC_GRAVITY * 28.9645m;

            decimal head = (zAvg * R * SUCTION_TEMPERATURE / molecularWeight) *
                          (k / (k - 1m)) *
                          ((decimal)Math.Pow((double)compressionRatio, (double)((k - 1m) / k)) - 1m);

            return Math.Max(0m, head);
        }

        /// <summary>
        /// Calculates theoretical power for reciprocating compressor.
        /// </summary>
        private static decimal CalculateTheoreticalPower(
            RECIPROCATING_COMPRESSOR_PROPERTIES compressorProperties,
            COMPRESSOR_OPERATING_CONDITIONS conditions,
            decimal compressionRatio,
            decimal zFactor)
        {
            // Reciprocating compressor power calculation
            // Based on cylinder displacement and compression work

            // Calculate cylinder displacement
            decimal cylinderArea = (decimal)Math.PI * compressorProperties.CYLINDER_DIAMETER *
                                  compressorProperties.CYLINDER_DIAMETER / 4m; // square inches
            decimal strokeLengthFt = compressorProperties.STROKE_LENGTH / 12m; // feet
            decimal displacementPerCylinder = cylinderArea * strokeLengthFt / 144m; // cubic feet

            // Calculate displacement per minute
            decimal displacementPerMinute = displacementPerCylinder *
                                           compressorProperties.ROTATIONAL_SPEED *
                                           compressorProperties.NUMBER_OF_CYLINDERS; // ftÂ³/min

            // Apply volumetric efficiency
            decimal actualDisplacement = displacementPerMinute * compressorProperties.VOLUMETRIC_EFFICIENCY;

            // Calculate compression work
            decimal k = 1.3m; // Typical for gas
            decimal compressionWork = (k / (k - 1m)) *
                                     conditions.SUCTION_PRESSURE * 144m * // Convert to psf
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
            decimal SUCTION_TEMPERATURE,
            decimal compressionRatio,
            decimal SPECIFIC_HEAT_RATIO)
        {
            decimal k = SPECIFIC_HEAT_RATIO;

            decimal dischargeTemperature = SUCTION_TEMPERATURE *
                                         (decimal)Math.Pow((double)compressionRatio, (double)((k - 1m) / k));

            return Math.Max(SUCTION_TEMPERATURE, dischargeTemperature);
        }
    }
}

