using System.Collections.Generic;
using System.Threading.Tasks;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Service interface for pipeline analysis operations.
    /// Provides flow analysis, pressure drop calculations, and pipeline optimization.
    /// </summary>
    public interface IPipelineAnalysisService
    {
        /// <summary>
        /// Analyzes pipeline flow.
        /// </summary>
        /// <param name="pipelineId">Pipeline identifier</param>
        /// <param name="flowRate">Flow rate</param>
        /// <param name="inletPressure">Inlet pressure</param>
        /// <returns>Pipeline analysis result</returns>
        Task<PipelineAnalysisResult> AnalyzePipelineFlowAsync(string pipelineId, decimal flowRate, decimal inletPressure);

        /// <summary>
        /// Calculates pressure drop in pipeline.
        /// </summary>
        /// <param name="pipelineId">Pipeline identifier</param>
        /// <param name="flowRate">Flow rate</param>
        /// <returns>Pressure drop result</returns>
        Task<PressureDropResult> CalculatePressureDropAsync(string pipelineId, decimal flowRate);

        /// <summary>
        /// Saves pipeline analysis result to database.
        /// </summary>
        /// <param name="result">Analysis result</param>
        /// <param name="userId">User ID for audit</param>
        /// <returns>Task</returns>
        Task SaveAnalysisResultAsync(PipelineAnalysisResult result, string userId);
    }

    /// <summary>
    /// DTO for pipeline analysis result.
    /// </summary>
    public class PipelineAnalysisResult
    {
        public string AnalysisId { get; set; } = string.Empty;
        public string PipelineId { get; set; } = string.Empty;
        public System.DateTime AnalysisDate { get; set; }
        public decimal FlowRate { get; set; }
        public decimal InletPressure { get; set; }
        public decimal OutletPressure { get; set; }
        public decimal PressureDrop { get; set; }
        public decimal Velocity { get; set; }
        public string? Status { get; set; }
    }

    /// <summary>
    /// DTO for pressure drop result.
    /// </summary>
    public class PressureDropResult
    {
        public decimal PressureDrop { get; set; }
        public decimal FrictionFactor { get; set; }
        public decimal ReynoldsNumber { get; set; }
        public string FlowRegime { get; set; } = string.Empty;
    }
}




