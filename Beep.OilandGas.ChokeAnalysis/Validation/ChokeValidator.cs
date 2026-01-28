using System;

using Beep.OilandGas.ChokeAnalysis.Constants;
using Beep.OilandGas.ChokeAnalysis.Exceptions;
using Beep.OilandGas.Models.Data.ChokeAnalysis;

namespace Beep.OilandGas.ChokeAnalysis.Validation
{
    /// <summary>
    /// Provides validation for choke flow calculations.
    /// </summary>
    public static class ChokeValidator
    {
        /// <summary>
        /// Validates choke properties.
        /// </summary>
        public static void ValidateChokeProperties(CHOKE_PROPERTIES choke)
        {
            if (choke == null)
                throw new ArgumentNullException(nameof(choke));

            if (choke.CHOKE_DIAMETER < ChokeConstants.MinimumChokeDiameter)
                throw new ChokeParameterOutOfRangeException(
                    nameof(choke.CHOKE_DIAMETER),
                    $"Choke diameter ({choke.CHOKE_DIAMETER} in) is below minimum ({ChokeConstants.MinimumChokeDiameter} in).");

            if (choke.CHOKE_DIAMETER > ChokeConstants.MaximumChokeDiameter)
                throw new ChokeParameterOutOfRangeException(
                    nameof(choke.CHOKE_DIAMETER),
                    $"Choke diameter ({choke.CHOKE_DIAMETER} in) exceeds maximum ({ChokeConstants.MaximumChokeDiameter} in).");

            if (choke.DISCHARGE_COEFFICIENT <= 0 || choke.DISCHARGE_COEFFICIENT > 1.0m)
                throw new InvalidChokePropertiesException(
                    "Discharge coefficient must be between 0 and 1.");
        }

        /// <summary>
        /// Validates gas choke properties.
        /// </summary>
        public static void ValidateGasChokeProperties(GAS_CHOKE_PROPERTIES gasProperties)
        {
            if (gasProperties == null)
                throw new ArgumentNullException(nameof(gasProperties));

            if (gasProperties.UPSTREAM_PRESSURE <= 0)
                throw new ChokeParameterOutOfRangeException(
                    nameof(gasProperties.UPSTREAM_PRESSURE),
                    "Upstream pressure must be greater than zero.");

            if (gasProperties.DOWNSTREAM_PRESSURE < 0)
                throw new ChokeParameterOutOfRangeException(
                    nameof(gasProperties.DOWNSTREAM_PRESSURE),
                    "Downstream pressure cannot be negative.");

            if (gasProperties.DOWNSTREAM_PRESSURE >= gasProperties.UPSTREAM_PRESSURE)
                throw new InvalidChokePropertiesException(
                    "Downstream pressure must be less than upstream pressure.");

            if (gasProperties.TEMPERATURE <= 0)
                throw new ChokeParameterOutOfRangeException(
                    nameof(gasProperties.TEMPERATURE),
                    "Temperature must be greater than zero.");

            if (gasProperties.GAS_SPECIFIC_GRAVITY <= 0)
                throw new ChokeParameterOutOfRangeException(
                    nameof(gasProperties.GAS_SPECIFIC_GRAVITY),
                    "Gas specific gravity must be greater than zero.");
        }

        /// <summary>
        /// Validates flow rate.
        /// </summary>
        public static void ValidateFlowRate(decimal flowRate)
        {
            if (flowRate <= 0)
                throw new ChokeParameterOutOfRangeException(
                    nameof(flowRate),
                    "Flow rate must be greater than zero.");
        }

        /// <summary>
        /// Validates all calculation parameters.
        /// </summary>
        public static void ValidateCalculationParameters(
            CHOKE_PROPERTIES choke,
            GAS_CHOKE_PROPERTIES gasProperties)
        {
            ValidateChokeProperties(choke);
            ValidateGasChokeProperties(gasProperties);
        }
    }
}

