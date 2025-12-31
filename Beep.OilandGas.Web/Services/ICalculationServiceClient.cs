using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.GasLift;
using Beep.OilandGas.Models.DTOs;
using Beep.OilandGas.Models.EconomicAnalysis;
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
        Task<bool> SaveGasLiftDesignAsync(GasLiftDesignDto design, string? userId = null);
        Task<GasLiftPerformanceDto> GetGasLiftPerformanceAsync(string wellUWI);

        // Nodal Analysis Operations
        Task<NodalAnalysisResultDto> PerformNodalAnalysisAsync(string wellUWI, NodalAnalysisParametersDto analysisParameters);
        Task<OptimizationResultDto> OptimizeSystemAsync(string wellUWI, OptimizationGoalsDto optimizationGoals);
        Task<bool> SaveNodalAnalysisResultAsync(NodalAnalysisResultDto result, string? userId = null);
        Task<List<NodalAnalysisResultDto>> GetNodalAnalysisHistoryAsync(string wellUWI);

        // Production Forecasting Operations
        Task<ProductionForecastResultDto> GenerateForecastAsync(string? wellUWI, string? fieldId, string forecastMethod, int forecastPeriod);
        Task<DeclineCurveAnalysisDto> PerformDeclineCurveAnalysisAsync(string wellUWI, DateTime startDate, DateTime endDate);
        Task<bool> SaveForecastAsync(ProductionForecastResultDto forecast, string? userId = null);

        // Pipeline Analysis Operations
        Task<PipelineAnalysisResultDto> AnalyzePipelineFlowAsync(string pipelineId, decimal flowRate, decimal inletPressure);
        Task<PressureDropResultDto> CalculatePressureDropAsync(string pipelineId, decimal flowRate);
        Task<bool> SavePipelineAnalysisResultAsync(PipelineAnalysisResultDto result, string? userId = null);

        // Economic Analysis Operations
        Task<double> CalculateNPVAsync(CashFlow[] cashFlows, double discountRate);
        Task<double> CalculateIRRAsync(CashFlow[] cashFlows, double initialGuess = 0.1);
        Task<EconomicResult> AnalyzeEconomicAsync(CashFlow[] cashFlows, double discountRate, double financeRate = 0.1, double reinvestRate = 0.1);
        Task<List<NPVProfilePoint>> GenerateNPVProfileAsync(CashFlow[] cashFlows, double minRate = 0.0, double maxRate = 1.0, int points = 50);
        Task<bool> SaveEconomicAnalysisResultAsync(string analysisId, EconomicResult result, string? userId = null);
        Task<EconomicResult?> GetEconomicAnalysisResultAsync(string analysisId);
    }
}

