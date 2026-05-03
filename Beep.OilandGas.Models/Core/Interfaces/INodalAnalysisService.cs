using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.NodalAnalysis;
using Beep.OilandGas.Models.Data.ProductionForecasting;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Canonical API-facing contract for nodal analysis operations.
    /// </summary>
    /// <remarks>
    /// Registered in API DI as the primary nodal-analysis boundary. Core CRUD-style operations (analyze,
    /// optimize, save, history) are stable; additional screening/diagnostic methods are part of the same
    /// contract for API and integration consumers.
    /// </remarks>
    public interface INodalAnalysisService
    {
        /// <summary>
        /// Performs nodal analysis for a well system.
        /// </summary>
        /// <param name="wellUWI">Well UWI</param>
        /// <param name="analysisParameters">Analysis parameters</param>
        /// <returns>Nodal analysis result</returns>
        Task<NodalAnalysisRunResult> PerformNodalAnalysisAsync(string wellUWI, NodalAnalysisParameters analysisParameters);

        /// <summary>
        /// Optimizes well system based on nodal analysis.
        /// </summary>
        /// <param name="wellUWI">Well UWI</param>
        /// <param name="optimizationGoals">Optimization goals</param>
        /// <returns>Optimization result</returns>
        Task<OptimizationResult> OptimizeSystemAsync(string wellUWI, OptimizationGoals optimizationGoals);

        /// <summary>
        /// Saves nodal analysis result to database.
        /// </summary>
        /// <param name="result">Analysis result</param>
        /// <param name="userId">User ID for audit</param>
        /// <returns>Task</returns>
        Task SaveAnalysisResultAsync(NodalAnalysisRunResult result, string userId);

        /// <summary>
        /// Gets nodal analysis history for a well.
        /// </summary>
        /// <param name="wellUWI">Well UWI</param>
        /// <returns>List of analysis results</returns>
        Task<List<NodalAnalysisRunResult>> GetAnalysisHistoryAsync(string wellUWI);

        /// <summary>IPR/VLP performance matching and bottleneck screening.</summary>
        Task<PerformanceMatchingAnalysis> AnalyzePerformanceMatchingAsync(string wellUWI, NodalAnalysisParameters analysisParameters);

        /// <summary>Economic sensitivity scenarios for selected parameters.</summary>
        Task<EconomicSensitivityAnalysisResult> PerformSensitivityAnalysisAsync(
            string wellUWI,
            NodalAnalysisParameters baselineParameters,
            List<string> parametersToVary);

        /// <summary>Ranked artificial lift recommendations with scoring metadata.</summary>
        Task<ArtificialLiftRecommendation> RecommendArtificialLiftAsync(
            string wellUWI,
            decimal currentProduction,
            decimal targetProduction,
            decimal wellDepth,
            decimal waterCut);

        /// <summary>Diagnostics for expected vs actual performance.</summary>
        Task<WellDiagnosticsResult> DiagnoseWellPerformanceAsync(
            string wellUWI,
            decimal expectedProduction,
            decimal actualProduction,
            decimal wellheadPressure,
            decimal bottomholePressure);

        /// <summary>Simple decline forecast for screening (not full reservoir simulation).</summary>
        Task<PRODUCTION_FORECAST> ForecastProductionAsync(
            string wellUWI,
            decimal currentProduction,
            decimal declineRate,
            int forecastMonths);

        /// <summary>Pressure maintenance strategy screening vs bubble point.</summary>
        Task<PressureMaintenanceStrategy> AnalyzePressureMaintenanceAsync(
            string wellUWI,
            decimal currentReservoirPressure,
            decimal bubblePointPressure,
            decimal productivityIndex);
    }
}





