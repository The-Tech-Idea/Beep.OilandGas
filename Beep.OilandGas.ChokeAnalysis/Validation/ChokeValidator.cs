using System;

using Beep.OilandGas.ChokeAnalysis.Constants;
using Beep.OilandGas.ChokeAnalysis.Exceptions;
using Beep.OilandGas.Models.ChokeAnalysis;

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
        public static void ValidateChokeProperties(ChokeProperties choke)
        {
            if (choke == null)
                throw new ArgumentNullException(nameof(choke));

            if (choke.ChokeDiameter < ChokeConstants.MinimumChokeDiameter)
                throw new ChokeParameterOutOfRangeException(
                    nameof(choke.ChokeDiameter),
                    $"Choke diameter ({choke.ChokeDiameter} in) is below minimum ({ChokeConstants.MinimumChokeDiameter} in).");

            if (choke.ChokeDiameter > ChokeConstants.MaximumChokeDiameter)
                throw new ChokeParameterOutOfRangeException(
                    nameof(choke.ChokeDiameter),
                    $"Choke diameter ({choke.ChokeDiameter} in) exceeds maximum ({ChokeConstants.MaximumChokeDiameter} in).");

            if (choke.DischargeCoefficient <= 0 || choke.DischargeCoefficient > 1.0m)
                throw new InvalidChokePropertiesException(
                    "Discharge coefficient must be between 0 and 1.");
        }

        /// <summary>
        /// Validates gas choke properties.
        /// </summary>
        public static void ValidateGasChokeProperties(GasChokeProperties gasProperties)
        {
            if (gasProperties == null)
                throw new ArgumentNullException(nameof(gasProperties));

            if (gasProperties.UpstreamPressure <= 0)
                throw new ChokeParameterOutOfRangeException(
                    nameof(gasProperties.UpstreamPressure),
                    "Upstream pressure must be greater than zero.");

            if (gasProperties.DownstreamPressure < 0)
                throw new ChokeParameterOutOfRangeException(
                    nameof(gasProperties.DownstreamPressure),
                    "Downstream pressure cannot be negative.");

            if (gasProperties.DownstreamPressure >= gasProperties.UpstreamPressure)
                throw new InvalidChokePropertiesException(
                    "Downstream pressure must be less than upstream pressure.");

            if (gasProperties.Temperature <= 0)
                throw new ChokeParameterOutOfRangeException(
                    nameof(gasProperties.Temperature),
                    "Temperature must be greater than zero.");

            if (gasProperties.GasSpecificGravity <= 0)
                throw new ChokeParameterOutOfRangeException(
                    nameof(gasProperties.GasSpecificGravity),
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
            ChokeProperties choke,
            GasChokeProperties gasProperties)
        {
            ValidateChokeProperties(choke);
            ValidateGasChokeProperties(gasProperties);
        }
    }
}

