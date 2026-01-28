using System;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.ChokeAnalysis;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Interface for choke analysis services providing industry-standard flow calculations.
    /// Implements petroleum engineering best practices for choke performance evaluation.
    /// </summary>
    public interface IChokeAnalysisService
    {
        /// <summary>
        /// Calculates gas flow rate through a downhole choke with enhanced accuracy.
        /// </summary>
        /// <param name="choke">Choke properties.</param>
        /// <param name="gasProperties">Gas properties.</param>
        /// <returns>Choke flow result with enhanced calculations.</returns>
        Task<CHOKE_FLOW_RESULT> CalculateDownholeChokeFlowAsync(
            CHOKE_PROPERTIES choke,
            GAS_CHOKE_PROPERTIES gasProperties);

        /// <summary>
        /// Calculates gas flow rate through an uphole choke.
        /// </summary>
        /// <param name="choke">Choke properties.</param>
        /// <param name="gasProperties">Gas properties.</param>
        /// <returns>Choke flow result.</returns>
        Task<CHOKE_FLOW_RESULT> CalculateUpholeChokeFlowAsync(
            CHOKE_PROPERTIES choke,
            GAS_CHOKE_PROPERTIES gasProperties);

        /// <summary>
        /// Calculates downstream pressure for a given flow rate and choke configuration.
        /// </summary>
        /// <param name="choke">Choke properties.</param>
        /// <param name="gasProperties">Gas properties.</param>
        /// <param name="flowRate">Target flow rate.</param>
        /// <returns>Calculated downstream pressure.</returns>
        Task<decimal> CalculateDownstreamPressureAsync(
            CHOKE_PROPERTIES choke,
            GAS_CHOKE_PROPERTIES gasProperties,
            decimal flowRate);

        /// <summary>
        /// Calculates required choke size for specified flow conditions.
        /// </summary>
        /// <param name="gasProperties">Gas properties.</param>
        /// <param name="flowRate">Target flow rate.</param>
        /// <returns>Optimal choke diameter.</returns>
        Task<decimal> CalculateRequiredChokeSizeAsync(
            GAS_CHOKE_PROPERTIES gasProperties,
            decimal flowRate);

        /// <summary>
        /// Validates choke configuration parameters.
        /// </summary>
        /// <param name="choke">Choke properties.</param>
        /// <param name="gasProperties">Gas properties.</param>
        /// <returns>Validation result with any errors.</returns>
        Task<ChokeValidationResult> ValidateChokeConfigurationAsync(
            CHOKE_PROPERTIES choke,
            GAS_CHOKE_PROPERTIES gasProperties);

        /// <summary>
        /// Calculates choke performance characteristics across a range of conditions.
        /// </summary>
        /// <param name="choke">Choke properties.</param>
        /// <param name="gasProperties">Base gas properties.</param>
        /// <param name="pressureRange">Downstream pressure range to evaluate.</param>
        /// <param name="numberOfPoints">Number of calculation points.</param>
        /// <returns>Performance curve data.</returns>
        Task<ChokePerformanceCurve[]> CalculatePerformanceCurveAsync(
            CHOKE_PROPERTIES choke,
            GAS_CHOKE_PROPERTIES gasProperties,
            (decimal Min, decimal Max) pressureRange,
            int numberOfPoints = 20);
    }
}

