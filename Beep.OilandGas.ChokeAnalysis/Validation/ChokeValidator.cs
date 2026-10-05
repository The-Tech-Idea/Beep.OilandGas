using System;

using Beep.OilandGas.ChokeAnalysis.Constants;
using Beep.OilandGas.ChokeAnalysis.Exceptions;
using Beep.OilandGas.Models.Data.ChokeAnalysis;

namespace Beep.OilandGas.ChokeAnalysis.Validation
{
    /// <summary>
    /// Provides validation for choke flow calculations.
    /// </summary>
    /// <remarks>
    /// OILGAS-CATCH-01. Each rule is written once, as a question (<see cref="FindChokeProblem"/>,
    /// <see cref="FindGasChokeProblem"/>); the <c>Validate…</c> methods refuse the problem found. A caller that wants
    /// the answer rather than the refusal — the service's configuration check — asks, instead of catching the refusal.
    /// </remarks>
    public static class ChokeValidator
    {
        /// <summary>
        /// Validates choke properties.
        /// </summary>
        public static void ValidateChokeProperties(CHOKE_PROPERTIES choke) => Refuse(FindChokeProblem(choke));

        /// <summary>
        /// Validates gas choke properties.
        /// </summary>
        public static void ValidateGasChokeProperties(GAS_CHOKE_PROPERTIES gasProperties) => Refuse(FindGasChokeProblem(gasProperties));

        /// <summary>
        /// The first problem with the choke's properties, or null when there is none.
        /// </summary>
        public static ChokeValidationProblem? FindChokeProblem(CHOKE_PROPERTIES choke)
        {
            if (choke == null)
                throw new ArgumentNullException(nameof(choke));

            if (choke.CHOKE_DIAMETER < ChokeConstants.MinimumChokeDiameter)
                return new ChokeValidationProblem(
                    nameof(choke.CHOKE_DIAMETER),
                    $"Choke diameter ({choke.CHOKE_DIAMETER} in) is below minimum ({ChokeConstants.MinimumChokeDiameter} in).");

            if (choke.CHOKE_DIAMETER > ChokeConstants.MaximumChokeDiameter)
                return new ChokeValidationProblem(
                    nameof(choke.CHOKE_DIAMETER),
                    $"Choke diameter ({choke.CHOKE_DIAMETER} in) exceeds maximum ({ChokeConstants.MaximumChokeDiameter} in).");

            if (choke.DISCHARGE_COEFFICIENT <= 0 || choke.DISCHARGE_COEFFICIENT > 1.0m)
                return new ChokeValidationProblem(null,
                    "Discharge coefficient must be between 0 and 1.");

            return null;
        }

        /// <summary>
        /// The first problem with the gas choke properties, or null when there is none.
        /// </summary>
        public static ChokeValidationProblem? FindGasChokeProblem(GAS_CHOKE_PROPERTIES gasProperties)
        {
            if (gasProperties == null)
                throw new ArgumentNullException(nameof(gasProperties));

            if (gasProperties.UPSTREAM_PRESSURE <= 0)
                return new ChokeValidationProblem(
                    nameof(gasProperties.UPSTREAM_PRESSURE),
                    "Upstream pressure must be greater than zero.");

            if (gasProperties.DOWNSTREAM_PRESSURE < 0)
                return new ChokeValidationProblem(
                    nameof(gasProperties.DOWNSTREAM_PRESSURE),
                    "Downstream pressure cannot be negative.");

            if (gasProperties.DOWNSTREAM_PRESSURE >= gasProperties.UPSTREAM_PRESSURE)
                return new ChokeValidationProblem(null,
                    "Downstream pressure must be less than upstream pressure.");

            if (gasProperties.TEMPERATURE <= 0)
                return new ChokeValidationProblem(
                    nameof(gasProperties.TEMPERATURE),
                    "Temperature must be greater than zero.");

            if (gasProperties.GAS_SPECIFIC_GRAVITY <= 0)
                return new ChokeValidationProblem(
                    nameof(gasProperties.GAS_SPECIFIC_GRAVITY),
                    "Gas specific gravity must be greater than zero.");

            return null;
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

        private static void Refuse(ChokeValidationProblem? problem)
        {
            if (problem is null)
                return;

            if (problem.ParameterName is null)
                throw new InvalidChokePropertiesException(problem.Sentence);

            throw new ChokeParameterOutOfRangeException(problem.ParameterName, problem.Sentence);
        }
    }
}
