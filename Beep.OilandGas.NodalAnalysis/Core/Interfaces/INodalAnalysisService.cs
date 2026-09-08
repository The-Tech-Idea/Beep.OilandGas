using Beep.OilandGas.Models.Data.NodalAnalysis;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.ProductionForecasting;

namespace Beep.OilandGas.NodalAnalysis.Core.Interfaces;

public interface INodalAnalysisService
{
    Task<NodalAnalysisRunResult> PerformNodalAnalysisAsync(string wellUWI, NodalAnalysisParameters analysisParameters, CancellationToken cancellationToken = default);
    Task<OptimizationResult> OptimizeSystemAsync(string wellUWI, OptimizationGoals optimizationGoals, CancellationToken cancellationToken = default);
    Task SaveAnalysisResultAsync(NodalAnalysisRunResult result, string userId, CancellationToken cancellationToken = default);
    Task<List<NodalAnalysisRunResult>> GetAnalysisHistoryAsync(string wellUWI, CancellationToken cancellationToken = default);
    Task<PerformanceMatchingAnalysis> AnalyzePerformanceMatchingAsync(string wellUWI, NodalAnalysisParameters analysisParameters, CancellationToken cancellationToken = default);
    Task<EconomicSensitivityAnalysisResult> PerformSensitivityAnalysisAsync(string wellUWI, NodalAnalysisParameters baselineParameters, List<string> parametersToVary, CancellationToken cancellationToken = default);
    Task<ArtificialLiftRecommendation> RecommendArtificialLiftAsync(string wellUWI, decimal currentProduction, decimal targetProduction, decimal wellDepth, decimal waterCut, CancellationToken cancellationToken = default);
    Task<WellDiagnosticsResult> DiagnoseWellPerformanceAsync(string wellUWI, decimal expectedProduction, decimal actualProduction, decimal wellheadPressure, decimal bottomholePressure, CancellationToken cancellationToken = default);
    Task<PRODUCTION_FORECAST> ForecastProductionAsync(string wellUWI, decimal currentProduction, decimal declineRate, int forecastMonths, CancellationToken cancellationToken = default);
    Task<PressureMaintenanceStrategy> AnalyzePressureMaintenanceAsync(string wellUWI, decimal currentReservoirPressure, decimal bubblePointPressure, decimal productivityIndex, CancellationToken cancellationToken = default);
}
