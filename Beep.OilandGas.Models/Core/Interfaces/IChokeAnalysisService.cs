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
        Task<ChokeFlowResult> CalculateDownholeChokeFlowAsync(
            ChokeProperties choke,
            GasChokeProperties gasProperties);

        /// <summary>
        /// Calculates gas flow rate through an uphole choke.
        /// </summary>
        /// <param name="choke">Choke properties.</param>
        /// <param name="gasProperties">Gas properties.</param>
        /// <returns>Choke flow result.</returns>
        Task<ChokeFlowResult> CalculateUpholeChokeFlowAsync(
            ChokeProperties choke,
            GasChokeProperties gasProperties);

        /// <summary>
        /// Calculates downstream pressure for a given flow rate and choke configuration.
        /// </summary>
        /// <param name="choke">Choke properties.</param>
        /// <param name="gasProperties">Gas properties.</param>
        /// <param name="flowRate">Target flow rate.</param>
        /// <returns>Calculated downstream pressure.</returns>
        Task<decimal> CalculateDownstreamPressureAsync(
            ChokeProperties choke,
            GasChokeProperties gasProperties,
            decimal flowRate);

        /// <summary>
        /// Calculates required choke size for specified flow conditions.
        /// </summary>
        /// <param name="gasProperties">Gas properties.</param>
        /// <param name="flowRate">Target flow rate.</param>
        /// <returns>Optimal choke diameter.</returns>
        Task<decimal> CalculateRequiredChokeSizeAsync(
            GasChokeProperties gasProperties,
            decimal flowRate);

        /// <summary>
        /// Validates choke configuration parameters.
        /// </summary>
        /// <param name="choke">Choke properties.</param>
        /// <param name="gasProperties">Gas properties.</param>
        /// <returns>Validation result with any errors.</returns>
        Task<ChokeValidationResult> ValidateChokeConfigurationAsync(
            ChokeProperties choke,
            GasChokeProperties gasProperties);

        /// <summary>
        /// Calculates choke performance characteristics across a range of conditions.
        /// </summary>
        /// <param name="choke">Choke properties.</param>
        /// <param name="gasProperties">Base gas properties.</param>
        /// <param name="pressureRange">Downstream pressure range to evaluate.</param>
        /// <param name="numberOfPoints">Number of calculation points.</param>
        /// <returns>Performance curve data.</returns>
        Task<ChokePerformanceCurve[]> CalculatePerformanceCurveAsync(
            ChokeProperties choke,
            GasChokeProperties gasProperties,
            (decimal Min, decimal Max) pressureRange,
            int numberOfPoints = 20);
    }

    /// <summary>
    /// Validation result for choke configuration.
    /// </summary>
    public class ChokeValidationResult
    {
        /// <summary>
        /// Indicates if configuration is valid.
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// List of validation errors.
        /// </summary>
        public string[] Errors { get; set; } = Array.Empty<string>();

        /// <summary>
        /// List of warnings.
        /// </summary>
        public string[] Warnings { get; set; } = Array.Empty<string>();
    }

    /// <summary>
    /// Choke performance curve data point.
    /// </summary>
    public class ChokePerformanceCurve
    {
        /// <summary>
        /// Downstream pressure for this data point.
        /// </summary>
        public decimal DownstreamPressure { get; set; }

        /// <summary>
        /// Calculated flow rate at this condition.
        /// </summary>
        public decimal FlowRate { get; set; }

        /// <summary>
        /// Flow regime at this condition.
        /// </summary>
        public FlowRegime FlowRegime { get; set; }

        /// <summary>
        /// Pressure ratio (P2/P1) for this condition.
        /// </summary>
        public decimal PressureRatio { get; set; }

        /// <summary>
        /// Indicates if flow is critical at this condition.
        /// </summary>
        public bool IsCriticalFlow { get; set; }
    }
}