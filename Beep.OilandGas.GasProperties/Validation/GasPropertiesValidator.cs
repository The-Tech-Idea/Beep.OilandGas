using System;
using Beep.OilandGas.Models.GasProperties;
using Beep.OilandGas.GasProperties.Constants;
using Beep.OilandGas.GasProperties.Exceptions;

namespace Beep.OilandGas.GasProperties.Validation
{
    /// <summary>
    /// Provides validation for gas property calculations.
    /// </summary>
    public static class GasPropertiesValidator
    {
        /// <summary>
        /// Validates pressure value.
        /// </summary>
        public static void ValidatePressure(decimal pressure)
        {
            if (pressure < GasPropertiesConstants.MinimumPressure)
                throw new ParameterOutOfRangeException(
                    nameof(pressure),
                    $"Pressure ({pressure} psia) is below minimum ({GasPropertiesConstants.MinimumPressure} psia).");

            if (pressure > GasPropertiesConstants.MaximumPressure)
                throw new ParameterOutOfRangeException(
                    nameof(pressure),
                    $"Pressure ({pressure} psia) exceeds maximum ({GasPropertiesConstants.MaximumPressure} psia).");
        }

        /// <summary>
        /// Validates temperature value.
        /// </summary>
        public static void ValidateTemperature(decimal temperature)
        {
            if (temperature < GasPropertiesConstants.MinimumTemperature)
                throw new ParameterOutOfRangeException(
                    nameof(temperature),
                    $"Temperature ({temperature} °R) is below minimum ({GasPropertiesConstants.MinimumTemperature} °R).");

            if (temperature > GasPropertiesConstants.MaximumTemperature)
                throw new ParameterOutOfRangeException(
                    nameof(temperature),
                    $"Temperature ({temperature} °R) exceeds maximum ({GasPropertiesConstants.MaximumTemperature} °R).");
        }

        /// <summary>
        /// Validates specific gravity value.
        /// </summary>
        public static void ValidateSpecificGravity(decimal specificGravity)
        {
            if (specificGravity < GasPropertiesConstants.MinimumSpecificGravity)
                throw new ParameterOutOfRangeException(
                    nameof(specificGravity),
                    $"Specific gravity ({specificGravity}) is below minimum ({GasPropertiesConstants.MinimumSpecificGravity}).");

            if (specificGravity > GasPropertiesConstants.MaximumSpecificGravity)
                throw new ParameterOutOfRangeException(
                    nameof(specificGravity),
                    $"Specific gravity ({specificGravity}) exceeds maximum ({GasPropertiesConstants.MaximumSpecificGravity}).");
        }

        /// <summary>
        /// Validates Z-factor value.
        /// </summary>
        public static void ValidateZFactor(decimal zFactor)
        {
            if (zFactor < GasPropertiesConstants.MinimumZFactor)
                throw new ParameterOutOfRangeException(
                    nameof(zFactor),
                    $"Z-factor ({zFactor}) is below minimum ({GasPropertiesConstants.MinimumZFactor}).");

            if (zFactor > GasPropertiesConstants.MaximumZFactor)
                throw new ParameterOutOfRangeException(
                    nameof(zFactor),
                    $"Z-factor ({zFactor}) exceeds maximum ({GasPropertiesConstants.MaximumZFactor}).");
        }

        /// <summary>
        /// Validates gas composition.
        /// </summary>
        public static void ValidateGasComposition(GasComposition composition)
        {
            if (composition == null)
                throw new ArgumentNullException(nameof(composition));

            if (!composition.IsValid())
                throw new InvalidGasCompositionException(
                    "Gas composition fractions must sum to 1.0 (within tolerance).");
        }

        /// <summary>
        /// Validates all calculation parameters.
        /// </summary>
        public static void ValidateCalculationParameters(
            decimal pressure,
            decimal temperature,
            decimal specificGravity)
        {
            ValidatePressure(pressure);
            ValidateTemperature(temperature);
            ValidateSpecificGravity(specificGravity);
        }
    }
}

