using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.GasLift;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.EconomicAnalysis;
using Beep.OilandGas.Models.Core.Interfaces;

namespace Beep.OilandGas.Web.Services
{
    /// <summary>
    /// Service interface for calculation operations (GasLift, NodalAnalysis, ProductionForecasting, PipelineAnalysis, EconomicAnalysis).
    /// </summary>
    public interface ICalculationServiceClient
    {
        // Gas Lift Operations
        Task<GasLiftPotentialResult> AnalyzeGasLiftPotentialAsync(
            GasLiftWellProperties wellProperties,
            decimal minGasInjectionRate,
            decimal maxGasInjectionRate,
            int numberOfPoints = 50);
        Task<GasLiftValveDesignResult> DesignGasLiftValvesAsync(
            GasLiftWellProperties wellProperties,
            decimal gasInjectionPressure,
            int numberOfValves,
            bool useSIUnits = false);
        Task<bool> SaveGasLiftDesignAsync(GasLiftDesign design, string? userId = null);
        Task<GasLiftPerformance> GetGasLiftPerformanceAsync(string wellUWI);

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
        Task<List<NPVProfilePoint>> GenerateNPVProfileAsync(CashFlow[] cashFlows, double minRate = 0.0, double maxRate = 1.0, int points = 50);
        Task<bool> SaveEconomicAnalysisResultAsync(string analysisId, EconomicResult result, string? userId = null);
        Task<EconomicResult?> GetEconomicAnalysisResultAsync(string analysisId);
    }
}

