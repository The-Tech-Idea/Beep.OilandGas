using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.GasLift;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.EconomicAnalysis;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.Calculations;
using Beep.OilandGas.Models.Data.FlashCalculations;
using Beep.OilandGas.Models.Data.GasProperties;
using Beep.OilandGas.Models.Data.PumpPerformance;
using Beep.OilandGas.Models.Data.WellTestAnalysis;

namespace Beep.OilandGas.Web.Services
{
    /// <summary>
    /// Service interface for calculation operations (DCA, GasLift, NodalAnalysis, ProductionForecasting, PipelineAnalysis, EconomicAnalysis).
    /// </summary>
    public interface ICalculationServiceClient
    {
        // DCA Operations
        Task<DcaAnalysisResponse> PerformDcaAnalysisAsync(DCARequest request);
        Task<DCAResult?> GetDcaResultAsync(string calculationId);
        Task<List<DCAResult>> GetDcaResultsAsync(string? wellId = null, string? poolId = null, string? fieldId = null);

        // Well Test Analysis Operations
        Task<WELL_TEST_ANALYSIS_RESULT> PerformWellTestAnalysisAsync(WellTestAnalysisCalculationRequest request);
        Task<WELL_TEST_ANALYSIS_RESULT?> GetWellTestAnalysisResultAsync(string calculationId);
        Task<List<WELL_TEST_ANALYSIS_RESULT>> GetWellTestAnalysisHistoryAsync(string? wellId = null, string? testId = null, string? fieldId = null);

        // Choke Analysis Operations
        Task<ChokeAnalysisResult> PerformChokeAnalysisAsync(ChokeAnalysisRequest request);

        // Compressor Analysis Operations
        Task<CompressorAnalysisResult> PerformCompressorAnalysisAsync(CompressorAnalysisRequest request);

        // Pump Performance Operations
        Task<ESP_DESIGN_RESULT> PerformPumpPerformanceAnalysisAsync(ESP_DESIGN_PROPERTIES request);
        Task<ESP_DESIGN_RESULT> OptimizePumpPerformanceAsync(ESP_DESIGN_PROPERTIES request);

        // Flash Calculations Operations
        Task<FlashResult> PerformIsothermalFlashAsync(FLASH_CONDITIONS conditions);
        Task<List<FlashResult>> PerformMultiStageFlashAsync(MultiStageFlashRequest request);
        Task<bool> SaveFlashResultAsync(FlashResult result, string? userId = null);
        Task<List<FlashResult>> GetFlashHistoryAsync(string? componentId = null);

        // Gas Properties Operations
        Task<GAS_PROPERTIES> AnalyzeGasPropertiesAsync(GasComposition composition, decimal pressure, decimal temperature, string correlation = "Standing-Katz");
        Task<CompositionSaveResponse?> SaveGasCompositionAsync(GasComposition composition, string? userId = null);
        Task<GasComposition?> GetGasCompositionAsync(string compositionId);

        // Oil Properties Operations
        Task<OilPropertyResult> CalculateOilPropertiesAsync(CalculateOilPropertiesRequest request);
        Task<CompositionSaveResponse?> SaveOilCompositionAsync(OilComposition composition, string? userId = null);
        Task<OilComposition?> GetOilCompositionAsync(string compositionId);
        Task<List<OilPropertyResult>> GetOilPropertyHistoryAsync(string compositionId);
        Task<CalculationSaveResponse?> SaveOilPropertyResultAsync(OilPropertyResult result, string? userId = null);

        // Gas Lift Operations
        Task<GAS_LIFT_POTENTIAL_RESULT> AnalyzeGasLiftPotentialAsync(
            GAS_LIFT_WELL_PROPERTIES wellProperties,
            decimal minGasInjectionRate,
            decimal maxGasInjectionRate,
            int numberOfPoints = 50);
        Task<GAS_LIFT_VALVE_DESIGN_RESULT> DesignGasLiftValvesAsync(
            GAS_LIFT_WELL_PROPERTIES wellProperties,
            decimal gasInjectionPressure,
            int numberOfValves,
            bool useSIUnits = false);
        Task<bool> SaveGasLiftDesignAsync(GAS_LIFT_DESIGN design, string? userId = null);
        Task<GAS_LIFT_PERFORMANCE> GetGasLiftPerformanceAsync(string wellUWI);

        // Nodal Analysis Operations
        Task<NodalAnalysisRunResult> PerformNodalAnalysisAsync(string wellUWI, NodalAnalysisParameters analysisParameters);
        Task<OptimizationResult> OptimizeSystemAsync(string wellUWI, OptimizationGoals optimizationGoals);
        Task<bool> SaveNodalAnalysisResultAsync(NodalAnalysisRunResult result, string? userId = null);
        Task<List<NodalAnalysisRunResult>> GetNodalAnalysisHistoryAsync(string wellUWI);

        // Production Forecasting Operations
        Task<ProductionForecastResult> GenerateForecastAsync(string? wellUWI, string? fieldId, string forecastMethod, int forecastPeriod);
        Task<DeclineCurveAnalysis> PerformDeclineCurveAnalysisAsync(string wellUWI, DateTime startDate, DateTime endDate);
        Task<bool> SaveForecastAsync(ProductionForecastResult forecast, string? userId = null);

        // Pipeline Analysis Operations
        Task<PipelineAnalysisResult> AnalyzePipelineFlowAsync(string pipelineId, decimal flowRate, decimal inletPressure);
        Task<PressureDropResult> CalculatePressureDropAsync(string pipelineId, decimal flowRate);
        Task<bool> SavePipelineAnalysisResultAsync(PipelineAnalysisResult result, string? userId = null);

        // Economic Analysis Operations
        Task<double> CalculateNPVAsync(CashFlow[] cashFlows, double discountRate);
        Task<double> CalculateIRRAsync(CashFlow[] cashFlows, double initialGuess = 0.1);
        Task<EconomicResult> AnalyzeEconomicAsync(CashFlow[] cashFlows, double discountRate, double financeRate = 0.1, double reinvestRate = 0.1);
        Task<List<NPV_PROFILE_POINT>> GenerateNPVProfileAsync(CashFlow[] cashFlows, double minRate = 0.0, double maxRate = 1.0, int points = 50);
        Task<bool> SaveEconomicAnalysisResultAsync(string analysisId, EconomicResult result, string? userId = null);
        Task<EconomicResult?> GetEconomicAnalysisResultAsync(string analysisId);
    }
}

