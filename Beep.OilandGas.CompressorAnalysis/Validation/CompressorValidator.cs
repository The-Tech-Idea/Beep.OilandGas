using System;
using Beep.OilandGas.CompressorAnalysis.Models;
using Beep.OilandGas.CompressorAnalysis.Constants;
using Beep.OilandGas.CompressorAnalysis.Exceptions;

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
        public static void ValidateOperatingConditions(CompressorOperatingConditions conditions)
        {
            if (conditions == null)
                throw new ArgumentNullException(nameof(conditions));

            if (conditions.SuctionPressure <= 0)
                throw new InvalidOperatingConditionsException("Suction pressure must be greater than zero.");

            if (conditions.DischargePressure <= conditions.SuctionPressure)
                throw new InvalidOperatingConditionsException("Discharge pressure must be greater than suction pressure.");

            if (conditions.SuctionTemperature <= 0)
                throw new InvalidOperatingConditionsException("Suction temperature must be greater than zero.");

            if (conditions.GasFlowRate <= 0)
                throw new InvalidOperatingConditionsException("Gas flow rate must be greater than zero.");

            if (conditions.GasSpecificGravity <= 0)
                throw new InvalidOperatingConditionsException("Gas specific gravity must be greater than zero.");

            if (conditions.CompressorEfficiency <= 0 || conditions.CompressorEfficiency > 1)
                throw new InvalidOperatingConditionsException("Compressor efficiency must be between 0 and 1.");

            if (conditions.MechanicalEfficiency <= 0 || conditions.MechanicalEfficiency > 1)
                throw new InvalidOperatingConditionsException("Mechanical efficiency must be between 0 and 1.");

            // Validate compression ratio
            decimal compressionRatio = conditions.DischargePressure / conditions.SuctionPressure;
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
        public static void ValidateCentrifugalCompressorProperties(CentrifugalCompressorProperties properties)
        {
            if (properties == null)
                throw new ArgumentNullException(nameof(properties));

            ValidateOperatingConditions(properties.OperatingConditions);

            if (properties.PolytropicEfficiency <= 0 || properties.PolytropicEfficiency > 1)
                throw new InvalidCompressorPropertiesException("Polytropic efficiency must be between 0 and 1.");

            if (properties.SpecificHeatRatio <= 1.0m)
                throw new InvalidCompressorPropertiesException("Specific heat ratio must be greater than 1.0.");

            if (properties.NumberOfStages <= 0)
                throw new InvalidCompressorPropertiesException("Number of stages must be greater than zero.");

            if (properties.Speed <= 0)
                throw new InvalidCompressorPropertiesException("Compressor speed must be greater than zero.");
        }

        /// <summary>
        /// Validates reciprocating compressor properties.
        /// </summary>
        public static void ValidateReciprocatingCompressorProperties(ReciprocatingCompressorProperties properties)
        {
            if (properties == null)
                throw new ArgumentNullException(nameof(properties));

            ValidateOperatingConditions(properties.OperatingConditions);

            if (properties.CylinderDiameter <= 0)
                throw new InvalidCompressorPropertiesException("Cylinder diameter must be greater than zero.");

            if (properties.StrokeLength <= 0)
                throw new InvalidCompressorPropertiesException("Stroke length must be greater than zero.");

            if (properties.RotationalSpeed <= 0)
                throw new InvalidCompressorPropertiesException("Rotational speed must be greater than zero.");

            if (properties.NumberOfCylinders <= 0)
                throw new InvalidCompressorPropertiesException("Number of cylinders must be greater than zero.");

            if (properties.VolumetricEfficiency <= 0 || properties.VolumetricEfficiency > 1)
                throw new InvalidCompressorPropertiesException("Volumetric efficiency must be between 0 and 1.");

            if (properties.ClearanceFactor < 0 || properties.ClearanceFactor > 1)
                throw new InvalidCompressorPropertiesException("Clearance factor must be between 0 and 1.");
        }
    }
}

