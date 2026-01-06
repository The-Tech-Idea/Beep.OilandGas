using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.DTOs;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Service interface for nodal analysis operations.
    /// Provides system analysis, optimization, and performance evaluation.
    /// </summary>
    public interface INodalAnalysisService
    {
        /// <summary>
        /// Performs nodal analysis for a well system.
        /// </summary>
        /// <param name="wellUWI">Well UWI</param>
        /// <param name="analysisParameters">Analysis parameters</param>
        /// <returns>Nodal analysis result</returns>
        Task<NodalAnalysisResultDto> PerformNodalAnalysisAsync(string wellUWI, NodalAnalysisParametersDto analysisParameters);

        /// <summary>
        /// Optimizes well system based on nodal analysis.
        /// </summary>
        /// <param name="wellUWI">Well UWI</param>
        /// <param name="optimizationGoals">Optimization goals</param>
        /// <returns>Optimization result</returns>
        Task<OptimizationResultDto> OptimizeSystemAsync(string wellUWI, OptimizationGoalsDto optimizationGoals);

        /// <summary>
        /// Saves nodal analysis result to database.
        /// </summary>
        /// <param name="result">Analysis result</param>
        /// <param name="userId">User ID for audit</param>
        /// <returns>Task</returns>
        Task SaveAnalysisResultAsync(NodalAnalysisResultDto result, string userId);

        /// <summary>
        /// Gets nodal analysis history for a well.
        /// </summary>
        /// <param name="wellUWI">Well UWI</param>
        /// <returns>List of analysis results</returns>
        Task<List<NodalAnalysisResultDto>> GetAnalysisHistoryAsync(string wellUWI);
    }
}




