using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.Calculations;

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
    

    /// <summary>
    /// DTO for pressure drop result.
    /// </summary>
    
}






