using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.GasLift;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.EconomicAnalysis;
using Beep.OilandGas.Models.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.Web.Services
{
    /// <summary>
    /// Client service for calculation operations.
    /// </summary>
    public class CalculationServiceClient : ICalculationServiceClient
    {
        private readonly ApiClient _apiClient;
        private readonly ILogger<CalculationServiceClient> _logger;

        public CalculationServiceClient(
            ApiClient apiClient,
            ILogger<CalculationServiceClient> logger)
        {
            _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #region Gas Lift Operations

        public async Task<GAS_LIFT_POTENTIAL_RESULT> AnalyzeGasLiftPotentialAsync(
            GAS_LIFT_WELL_PROPERTIES wellProperties,
            decimal minGasInjectionRate,
            decimal maxGasInjectionRate,
            int numberOfPoints = 50)
        {
            try
            {
                var request = new
                {
                    WellProperties = wellProperties,
                    MinGasInjectionRate = minGasInjectionRate,
                    MaxGasInjectionRate = maxGasInjectionRate,
                    NumberOfPoints = numberOfPoints
                };
                var result = await _apiClient.PostAsync<object, GAS_LIFT_POTENTIAL_RESULT>(
                    "/api/gaslift/analyze-potential", request);
                return result ?? throw new InvalidOperationException("Failed to analyze gas lift potential");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing gas lift potential");
                throw;
            }
        }

        public async Task<GAS_LIFT_VALVE_DESIGN_RESULT> DesignGasLiftValvesAsync(
            GAS_LIFT_WELL_PROPERTIES wellProperties,
            decimal gasInjectionPressure,
            int numberOfValves,
            bool useSIUnits = false)
        {
            try
            {
                var request = new
                {
                    WellProperties = wellProperties,
                    GasInjectionPressure = gasInjectionPressure,
                    NumberOfValves = numberOfValves,
                    UseSIUnits = useSIUnits
                };
                var result = await _apiClient.PostAsync<object, GAS_LIFT_VALVE_DESIGN_RESULT>(
                    "/api/gaslift/design-valves", request);
                return result ?? throw new InvalidOperationException("Failed to design gas lift valves");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error designing gas lift valves");
                throw;
            }
        }

        public async Task<bool> SaveGasLiftDesignAsync(GAS_LIFT_DESIGN design, string? userId = null)
        {
            try
            {
                var endpoint = "/api/gaslift/design";
                if (!string.IsNullOrEmpty(userId))
                {
                    endpoint += $"?userId={Uri.EscapeDataString(userId)}";
                }
                return await _apiClient.PostAsync(endpoint, design);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving gas lift design");
                return false;
            }
        }

        public async Task<GAS_LIFT_PERFORMANCE> GetGasLiftPerformanceAsync(string wellUWI)
        {
            try
            {
                var result = await _apiClient.GetAsync<GAS_LIFT_PERFORMANCE>(
                    $"/api/gaslift/performance/{Uri.EscapeDataString(wellUWI)}");
                return result ?? new GAS_LIFT_PERFORMANCE { WellUWI = wellUWI };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting gas lift performance for well {WellUWI}", wellUWI);
                return new GAS_LIFT_PERFORMANCE { WellUWI = wellUWI };
            }
        }

        #endregion

        #region Nodal Analysis Operations

        public async Task<NodalAnalysisRunResult> PerformNodalAnalysisAsync(string wellUWI, NodalAnalysisParameters analysisParameters)
        {
            try
            {
                var request = new
                {
                    WellUWI = wellUWI,
                    AnalysisParameters = analysisParameters
                };
                var result = await _apiClient.PostAsync<object, NodalAnalysisRunResult>(
                    "/api/nodalanalysis/analyze", request);
                return result ?? throw new InvalidOperationException("Failed to perform nodal analysis");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing nodal analysis for well {WellUWI}", wellUWI);
                throw;
            }
        }

        public async Task<OptimizationResult> OptimizeSystemAsync(string wellUWI, OptimizationGoals optimizationGoals)
        {
            try
            {
                var request = new
                {
                    WellUWI = wellUWI,
                    OptimizationGoals = optimizationGoals
                };
                var result = await _apiClient.PostAsync<object, OptimizationResult>(
                    "/api/nodalanalysis/optimize", request);
                return result ?? throw new InvalidOperationException("Failed to optimize system");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error optimizing system for well {WellUWI}", wellUWI);
                throw;
            }
        }

        public async Task<bool> SaveNodalAnalysisResultAsync(NodalAnalysisRunResult result, string? userId = null)
        {
            try
            {
                var endpoint = "/api/nodalanalysis/result";
                if (!string.IsNullOrEmpty(userId))
                {
                    endpoint += $"?userId={Uri.EscapeDataString(userId)}";
                }
                return await _apiClient.PostAsync(endpoint, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving nodal analysis result");
                return false;
            }
        }

        public async Task<List<NodalAnalysisRunResult>> GetNodalAnalysisHistoryAsync(string wellUWI)
        {
            try
            {
                var result = await _apiClient.GetAsync<List<NodalAnalysisRunResult>>(
                    $"/api/nodalanalysis/history/{Uri.EscapeDataString(wellUWI)}");
                return result ?? new List<NodalAnalysisRunResult>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting nodal analysis history for well {WellUWI}", wellUWI);
                return new List<NodalAnalysisRunResult>();
            }
        }

        #endregion

        #region Production Forecasting Operations

        public async Task<ProductionForecastResult> GenerateForecastAsync(string? wellUWI, string? fieldId, string forecastMethod, int forecastPeriod)
        {
            try
            {
                var request = new
                {
                    WellUWI = wellUWI,
                    FieldId = fieldId,
                    ForecastMethod = forecastMethod,
                    ForecastPeriod = forecastPeriod
                };
                var result = await _apiClient.PostAsync<object, ProductionForecastResult>(
                    "/api/productionforecasting/generate", request);
                return result ?? throw new InvalidOperationException("Failed to generate forecast");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating production forecast");
                throw;
            }
        }

        public async Task<DeclineCurveAnalysis> PerformDeclineCurveAnalysisAsync(string wellUWI, DateTime startDate, DateTime endDate)
        {
            try
            {
                var request = new
                {
                    WellUWI = wellUWI,
                    StartDate = startDate,
                    EndDate = endDate
                };
                var result = await _apiClient.PostAsync<object, DeclineCurveAnalysis>(
                    "/api/productionforecasting/decline-curve", request);
                return result ?? throw new InvalidOperationException("Failed to perform decline curve analysis");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing decline curve analysis for well {WellUWI}", wellUWI);
                throw;
            }
        }

        public async Task<bool> SaveForecastAsync(ProductionForecastResult forecast, string? userId = null)
        {
            try
            {
                var endpoint = "/api/productionforecasting/forecast";
                if (!string.IsNullOrEmpty(userId))
                {
                    endpoint += $"?userId={Uri.EscapeDataString(userId)}";
                }
                return await _apiClient.PostAsync(endpoint, forecast);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving production forecast");
                return false;
            }
        }

        #endregion

        #region Pipeline Analysis Operations

        public async Task<PipelineAnalysisResult> AnalyzePipelineFlowAsync(string pipelineId, decimal flowRate, decimal inletPressure)
        {
            try
            {
                var request = new
                {
                    PipelineId = pipelineId,
                    FlowRate = flowRate,
                    InletPressure = inletPressure
                };
                var result = await _apiClient.PostAsync<object, PipelineAnalysisResult>(
                    "/api/pipelineanalysis/analyze-flow", request);
                return result ?? throw new InvalidOperationException("Failed to analyze pipeline flow");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing pipeline flow for pipeline {PipelineId}", pipelineId);
                throw;
            }
        }

        public async Task<PressureDropResult> CalculatePressureDropAsync(string pipelineId, decimal flowRate)
        {
            try
            {
                var request = new
                {
                    PipelineId = pipelineId,
                    FlowRate = flowRate
                };
                var result = await _apiClient.PostAsync<object, PressureDropResult>(
                    "/api/pipelineanalysis/pressure-drop", request);
                return result ?? throw new InvalidOperationException("Failed to calculate pressure drop");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating pressure drop for pipeline {PipelineId}", pipelineId);
                throw;
            }
        }

        public async Task<bool> SavePipelineAnalysisResultAsync(PipelineAnalysisResult result, string? userId = null)
        {
            try
            {
                var endpoint = "/api/pipelineanalysis/result";
                if (!string.IsNullOrEmpty(userId))
                {
                    endpoint += $"?userId={Uri.EscapeDataString(userId)}";
                }
                return await _apiClient.PostAsync(endpoint, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving pipeline analysis result");
                return false;
            }
        }

        #endregion

        #region Economic Analysis Operations

        public async Task<double> CalculateNPVAsync(CashFlow[] cashFlows, double discountRate)
        {
            try
            {
                var request = new
                {
                    CashFlows = cashFlows,
                    DiscountRate = discountRate
                };
                var result = await _apiClient.PostAsync<object, double>(
                    "/api/economicanalysis/npv", request);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating NPV");
                throw;
            }
        }

        public async Task<double> CalculateIRRAsync(CashFlow[] cashFlows, double initialGuess = 0.1)
        {
            try
            {
                var request = new
                {
                    CashFlows = cashFlows,
                    InitialGuess = initialGuess
                };
                var result = await _apiClient.PostAsync<object, double>(
                    "/api/economicanalysis/irr", request);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating IRR");
                throw;
            }
        }

        public async Task<EconomicResult> AnalyzeEconomicAsync(CashFlow[] cashFlows, double discountRate, double financeRate = 0.1, double reinvestRate = 0.1)
        {
            try
            {
                var request = new
                {
                    CashFlows = cashFlows,
                    DiscountRate = discountRate,
                    FinanceRate = financeRate,
                    ReinvestRate = reinvestRate
                };
                var result = await _apiClient.PostAsync<object, EconomicResult>(
                    "/api/economicanalysis/analyze", request);
                return result ?? throw new InvalidOperationException("Failed to perform economic analysis");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing economic analysis");
                throw;
            }
        }

        public async Task<List<NPV_PROFILE_POINT>> GenerateNPVProfileAsync(CashFlow[] cashFlows, double minRate = 0.0, double maxRate = 1.0, int points = 50)
        {
            try
            {
                var request = new
                {
                    CashFlows = cashFlows,
                    MinRate = minRate,
                    MaxRate = maxRate,
                    Points = points
                };
                var result = await _apiClient.PostAsync<object, List<NPV_PROFILE_POINT>>(
                    "/api/economicanalysis/npv-profile", request);
                return result ?? new List<NPV_PROFILE_POINT>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating NPV profile");
                return new List<NPV_PROFILE_POINT>();
            }
        }

        public async Task<bool> SaveEconomicAnalysisResultAsync(string analysisId, EconomicResult result, string? userId = null)
        {
            try
            {
                var request = new
                {
                    AnalysisId = analysisId,
                    Result = result
                };
                var endpoint = "/api/economicanalysis/result";
                if (!string.IsNullOrEmpty(userId))
                {
                    endpoint += $"?userId={Uri.EscapeDataString(userId)}";
                }
                return await _apiClient.PostAsync(endpoint, request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving economic analysis result");
                return false;
            }
        }

        public async Task<EconomicResult?> GetEconomicAnalysisResultAsync(string analysisId)
        {
            try
            {
                var result = await _apiClient.GetAsync<EconomicResult>(
                    $"/api/economicanalysis/result/{Uri.EscapeDataString(analysisId)}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting economic analysis result {AnalysisId}", analysisId);
                return null;
            }
        }

        #endregion
    }
}

