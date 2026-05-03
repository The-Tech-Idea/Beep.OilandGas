using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.Calculations;
using Beep.OilandGas.Models.Data.FlashCalculations;
using Beep.OilandGas.Models.Data.NodalAnalysis;
using Beep.OilandGas.Models.Data.EconomicAnalysis;
using Beep.OilandGas.Models.Data.ProductionForecasting;
namespace Beep.OilandGas.Client.App.Services.Calculations
{
    /// <summary>
    /// Service interface for all Calculation operations
    /// </summary>
    public interface ICalculationsService
    {
        #region Flash Calculation

        Task<FlashResult> PerformIsothermalFlashAsync(FLASH_CONDITIONS request, CancellationToken cancellationToken = default);
        Task<List<FlashResult>> PerformMultiStageFlashAsync(FLASH_CONDITIONS request, CancellationToken cancellationToken = default);
        Task<FLASH_CALCULATION_RESULT> SaveFlashResultAsync(FLASH_CALCULATION_RESULT result, string? userId = null, CancellationToken cancellationToken = default);
        Task<List<FLASH_CALCULATION_RESULT>> GetFlashHistoryAsync(string compositionId, CancellationToken cancellationToken = default);

        #endregion

        #region Nodal Analysis

        /// <summary>Nodal analyze — relative URL <see cref="NodalAnalysisHttpRoutes.Analyze"/> (same contract as API).</summary>
        Task<NodalAnalysisRunResult> PerformNodalAnalysisAsync(string wellUWI, NodalAnalysisParameters analysisParameters, CancellationToken cancellationToken = default);

        /// <summary>Nodal optimize — <see cref="NodalAnalysisHttpRoutes.Optimize"/>.</summary>
        Task<OptimizationResult> OptimizeSystemAsync(string wellUWI, OptimizationGoals optimizationGoals, CancellationToken cancellationToken = default);

        /// <summary>Save nodal result — <see cref="NodalAnalysisHttpRoutes.Result"/>.</summary>
        Task<bool> SaveNodalAnalysisResultAsync(NodalAnalysisRunResult result, string? userId = null, CancellationToken cancellationToken = default);

        /// <summary>Nodal history — GET <see cref="NodalAnalysisHttpRoutes.Prefix"/>/history/{{wellUWI}}.</summary>
        Task<List<NodalAnalysisRunResult>> GetNodalAnalysisHistoryAsync(string wellId, CancellationToken cancellationToken = default);

        /// <summary>Performance matching — <see cref="NodalAnalysisHttpRoutes.PerformanceMatching"/>.</summary>
        Task<PerformanceMatchingAnalysis> AnalyzePerformanceMatchingAsync(PerformNodalAnalysisRequest request, CancellationToken cancellationToken = default);

        /// <summary>Sensitivity — <see cref="NodalAnalysisHttpRoutes.Sensitivity"/>.</summary>
        Task<EconomicSensitivityAnalysisResult> PerformSensitivityAnalysisAsync(NodalSensitivityAnalysisRequest request, CancellationToken cancellationToken = default);

        /// <summary>Artificial lift — <see cref="NodalAnalysisHttpRoutes.ArtificialLift"/>.</summary>
        Task<ArtificialLiftRecommendation> RecommendArtificialLiftAsync(NodalArtificialLiftRequest request, CancellationToken cancellationToken = default);

        /// <summary>Diagnostics — <see cref="NodalAnalysisHttpRoutes.Diagnostics"/>.</summary>
        Task<WellDiagnosticsResult> DiagnoseWellPerformanceAsync(NodalWellDiagnosticsRequest request, CancellationToken cancellationToken = default);

        /// <summary>Nodal decline screening forecast — <see cref="NodalAnalysisHttpRoutes.ProductionForecast"/>.</summary>
        Task<PRODUCTION_FORECAST> ForecastNodalProductionAsync(NodalProductionForecastRequest request, CancellationToken cancellationToken = default);

        /// <summary>Pressure maintenance screening — <see cref="NodalAnalysisHttpRoutes.PressureMaintenance"/>.</summary>
        Task<PressureMaintenanceStrategy> AnalyzePressureMaintenanceAsync(NodalPressureMaintenanceRequest request, CancellationToken cancellationToken = default);

        #endregion

        #region Economic Analysis

        Task<decimal> CalculateNPVAsync(List<CashFlow> request, CancellationToken cancellationToken = default);
        Task<decimal> CalculateIRRAsync(List<CashFlow> request, CancellationToken cancellationToken = default);
        Task<EconomicResult> PerformEconomicAnalysisAsync(List<CashFlow> request, CancellationToken cancellationToken = default);
        Task<List<NPV_PROFILE_POINT>> GenerateNPVProfileAsync(List<ECONOMIC_CASH_FLOW> request, CancellationToken cancellationToken = default);
        Task<ECONOMIC_ANALYSIS_RESULT> SaveEconomicResultAsync(ECONOMIC_ANALYSIS_RESULT result, string? userId = null, CancellationToken cancellationToken = default);
        Task<ECONOMIC_ANALYSIS_RESULT> GetEconomicResultAsync(string resultId, CancellationToken cancellationToken = default);

        #endregion
    }
}
