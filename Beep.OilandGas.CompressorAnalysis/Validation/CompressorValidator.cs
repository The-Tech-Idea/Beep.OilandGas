using System;

using Beep.OilandGas.CompressorAnalysis.Constants;
using Beep.OilandGas.CompressorAnalysis.Exceptions;
using Beep.OilandGas.Models.Data.CompressorAnalysis;

namespace Beep.OilandGas.CompressorAnalysis.Validation
{
    /// <summary>
    /// Provides validation for compressor calculations.
    /// </summary>
    public static class CompressorValidator
    {
        /// <summary>
        /// Validates operating conditions.
        /// </summary>
        public static void ValidateOperatingConditions(COMPRESSOR_OPERATING_CONDITIONS conditions)
        {
            if (conditions == null)
                throw new ArgumentNullException(nameof(conditions));

            if (conditions.SUCTION_PRESSURE <= 0)
                throw new InvalidOperatingConditionsException("Suction pressure must be greater than zero.");

            if (conditions.DISCHARGE_PRESSURE <= conditions.SUCTION_PRESSURE)
                throw new InvalidOperatingConditionsException("Discharge pressure must be greater than suction pressure.");

            if (conditions.SUCTION_TEMPERATURE <= 0)
                throw new InvalidOperatingConditionsException("Suction temperature must be greater than zero.");

            if (conditions.GAS_FLOW_RATE <= 0)
                throw new InvalidOperatingConditionsException("Gas flow rate must be greater than zero.");

            if (conditions.GAS_SPECIFIC_GRAVITY <= 0)
                throw new InvalidOperatingConditionsException("Gas specific gravity must be greater than zero.");

            if (conditions.COMPRESSOR_EFFICIENCY <= 0 || conditions.COMPRESSOR_EFFICIENCY > 1)
                throw new InvalidOperatingConditionsException("Compressor efficiency must be between 0 and 1.");

            if (conditions.MECHANICAL_EFFICIENCY <= 0 || conditions.MECHANICAL_EFFICIENCY > 1)
                throw new InvalidOperatingConditionsException("Mechanical efficiency must be between 0 and 1.");

            // Validate compression ratio
            decimal compressionRatio = conditions.DISCHARGE_PRESSURE / conditions.SUCTION_PRESSURE;
            if (compressionRatio > CompressorConstants.MaximumCompressionRatio)
            {
                throw new CompressorParameterOutOfRangeException(
                    nameof(compressionRatio),
                    $"Compression ratio ({compressionRatio:F2}) exceeds maximum ({CompressorConstants.MaximumCompressionRatio}).");
            }
        }

        /// <summary>
        /// Validates centrifugal compressor properties.
        /// </summary>
        public static void ValidateCentrifugalCompressorProperties(CENTRIFUGAL_COMPRESSOR_PROPERTIES properties)
        {
            if (properties == null)
                throw new ArgumentNullException(nameof(properties));

            ValidateOperatingConditions(properties.OPERATING_CONDITIONS);

            if (properties.POLYTROPIC_EFFICIENCY <= 0 || properties.POLYTROPIC_EFFICIENCY > 1)
                throw new InvalidCompressorPropertiesException("Polytropic efficiency must be between 0 and 1.");

            if (properties.SPECIFIC_HEAT_RATIO <= 1.0m)
                throw new InvalidCompressorPropertiesException("Specific heat ratio must be greater than 1.0.");

            if (properties.NUMBER_OF_STAGES <= 0)
                throw new InvalidCompressorPropertiesException("Number of stages must be greater than zero.");

            if (properties.SPEED <= 0)
                throw new InvalidCompressorPropertiesException("Compressor speed must be greater than zero.");
        }

        /// <summary>
        /// Validates reciprocating compressor properties.
        /// </summary>
        public static void ValidateReciprocatingCompressorProperties(RECIPROCATING_COMPRESSOR_PROPERTIES properties)
        {
            if (properties == null)
                throw new ArgumentNullException(nameof(properties));

            ValidateOperatingConditions(properties.OPERATING_CONDITIONS);

            if (properties.CYLINDER_DIAMETER <= 0)
                throw new InvalidCompressorPropertiesException("Cylinder diameter must be greater than zero.");

            if (properties.STROKE_LENGTH <= 0)
                throw new InvalidCompressorPropertiesException("Stroke length must be greater than zero.");

            if (properties.ROTATIONAL_SPEED <= 0)
                throw new InvalidCompressorPropertiesException("Rotational speed must be greater than zero.");

            if (properties.NUMBER_OF_CYLINDERS <= 0)
                throw new InvalidCompressorPropertiesException("Number of cylinders must be greater than zero.");

            if (properties.VOLUMETRIC_EFFICIENCY <= 0 || properties.VOLUMETRIC_EFFICIENCY > 1)
                throw new InvalidCompressorPropertiesException("Volumetric efficiency must be between 0 and 1.");

            if (properties.CLEARANCE_FACTOR < 0 || properties.CLEARANCE_FACTOR > 1)
                throw new InvalidCompressorPropertiesException("Clearance factor must be between 0 and 1.");
        }
    }
}

