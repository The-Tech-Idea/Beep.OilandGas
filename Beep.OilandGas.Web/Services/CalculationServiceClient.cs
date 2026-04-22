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
using Beep.OilandGas.Models.Data.ProductionForecasting;
using Beep.OilandGas.Models.Data.WellTestAnalysis;
using Microsoft.Extensions.Logging;
using ProductionForecastRequest = Beep.OilandGas.Models.Data.ProductionForecasting.GenerateForecastRequest;

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

        #region DCA Operations

        public async Task<DcaAnalysisResponse> PerformDcaAnalysisAsync(DCARequest request)
        {
            try
            {
                var response = await _apiClient.PostAsync<DCARequest, DcaAnalysisResponse>(
                    "/api/calculations/dca", request);
                return response ?? throw new InvalidOperationException("Failed to perform decline curve analysis");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing DCA analysis for well {WellId}", request.WellId);
                throw;
            }
        }

        public async Task<DCAResult?> GetDcaResultAsync(string calculationId)
        {
            try
            {
                return await _apiClient.GetAsync<DCAResult>(
                    $"/api/calculations/dca/{Uri.EscapeDataString(calculationId)}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting DCA result {CalculationId}", calculationId);
                return null;
            }
        }

        public async Task<List<DCAResult>> GetDcaResultsAsync(string? wellId = null, string? poolId = null, string? fieldId = null)
        {
            try
            {
                var queryParts = new List<string>();

                if (!string.IsNullOrWhiteSpace(wellId))
                {
                    queryParts.Add($"wellId={Uri.EscapeDataString(wellId)}");
                }

                if (!string.IsNullOrWhiteSpace(poolId))
                {
                    queryParts.Add($"poolId={Uri.EscapeDataString(poolId)}");
                }

                if (!string.IsNullOrWhiteSpace(fieldId))
                {
                    queryParts.Add($"fieldId={Uri.EscapeDataString(fieldId)}");
                }

                var endpoint = "/api/calculations/dca";
                if (queryParts.Count > 0)
                {
                    endpoint += $"?{string.Join("&", queryParts)}";
                }

                var result = await _apiClient.GetAsync<List<DCAResult>>(endpoint);
                return result ?? new List<DCAResult>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting DCA results for well {WellId}, pool {PoolId}, field {FieldId}", wellId, poolId, fieldId);
                return new List<DCAResult>();
            }
        }

        #endregion

        #region Well Test Analysis Operations

        public async Task<WELL_TEST_ANALYSIS_RESULT> PerformWellTestAnalysisAsync(WellTestAnalysisCalculationRequest request)
        {
            try
            {
                var result = await _apiClient.PostAsync<WellTestAnalysisCalculationRequest, WELL_TEST_ANALYSIS_RESULT>(
                    "/api/calculations/well-test", request);
                return result ?? throw new InvalidOperationException("Failed to perform well test analysis");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing Well Test Analysis for well {WellId} test {TestId}", request.WellId, request.TestId);
                throw;
            }
        }

        public async Task<WELL_TEST_ANALYSIS_RESULT?> GetWellTestAnalysisResultAsync(string calculationId)
        {
            try
            {
                return await _apiClient.GetAsync<WELL_TEST_ANALYSIS_RESULT>(
                    $"/api/calculations/well-test/{Uri.EscapeDataString(calculationId)}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Well Test Analysis result {CalculationId}", calculationId);
                return null;
            }
        }

        public async Task<List<WELL_TEST_ANALYSIS_RESULT>> GetWellTestAnalysisHistoryAsync(string? wellId = null, string? testId = null, string? fieldId = null)
        {
            try
            {
                var queryParts = new List<string>();

                if (!string.IsNullOrWhiteSpace(wellId))
                {
                    queryParts.Add($"wellId={Uri.EscapeDataString(wellId)}");
                }

                if (!string.IsNullOrWhiteSpace(testId))
                {
                    queryParts.Add($"testId={Uri.EscapeDataString(testId)}");
                }

                if (!string.IsNullOrWhiteSpace(fieldId))
                {
                    queryParts.Add($"fieldId={Uri.EscapeDataString(fieldId)}");
                }

                var endpoint = "/api/calculations/well-test";
                if (queryParts.Count > 0)
                {
                    endpoint += $"?{string.Join("&", queryParts)}";
                }

                var result = await _apiClient.GetAsync<List<WELL_TEST_ANALYSIS_RESULT>>(endpoint);
                return result ?? new List<WELL_TEST_ANALYSIS_RESULT>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Well Test Analysis history for well {WellId} test {TestId}", wellId, testId);
                return new List<WELL_TEST_ANALYSIS_RESULT>();
            }
        }

        #endregion

        #region Choke Analysis Operations

        public async Task<ChokeAnalysisResult> PerformChokeAnalysisAsync(ChokeAnalysisRequest request)
        {
            try
            {
                var result = await _apiClient.PostAsync<ChokeAnalysisRequest, ChokeAnalysisResult>(
                    "/api/calculations/choke", request);
                return result ?? throw new InvalidOperationException("Failed to perform choke analysis");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing Choke Analysis for well {WellId}", request.WellId);
                throw;
            }
        }

        #endregion

        #region Compressor Analysis Operations

        public async Task<CompressorAnalysisResult> PerformCompressorAnalysisAsync(CompressorAnalysisRequest request)
        {
            try
            {
                var result = await _apiClient.PostAsync<CompressorAnalysisRequest, CompressorAnalysisResult>(
                    "/api/calculations/compressor", request);
                return result ?? throw new InvalidOperationException("Failed to perform compressor analysis");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing Compressor Analysis for facility {FacilityId}", request.FacilityId);
                throw;
            }
        }

        #endregion

        #region Pump Performance Operations

        public async Task<ESP_DESIGN_RESULT> PerformPumpPerformanceAnalysisAsync(ESP_DESIGN_PROPERTIES request)
        {
            try
            {
                var result = await _apiClient.PostAsync<ESP_DESIGN_PROPERTIES, ESP_DESIGN_RESULT>(
                    "/api/pump/analyze", request);
                return result ?? throw new InvalidOperationException("Failed to perform pump performance analysis");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing Pump Performance analysis for area {AreaId}", request.AREA_ID);
                throw;
            }
        }

        public async Task<ESP_DESIGN_RESULT> OptimizePumpPerformanceAsync(ESP_DESIGN_PROPERTIES request)
        {
            try
            {
                var result = await _apiClient.PostAsync<ESP_DESIGN_PROPERTIES, ESP_DESIGN_RESULT>(
                    "/api/pump/optimize", request);
                return result ?? throw new InvalidOperationException("Failed to optimize pump performance analysis");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error optimizing Pump Performance analysis for area {AreaId}", request.AREA_ID);
                throw;
            }
        }

        #endregion

        #region Flash Calculations Operations

        public async Task<FlashResult> PerformIsothermalFlashAsync(FLASH_CONDITIONS conditions)
        {
            try
            {
                var result = await _apiClient.PostAsync<FLASH_CONDITIONS, FlashResult>(
                    "/api/flashcalculation/isothermal", conditions);
                return result ?? throw new InvalidOperationException("Failed to perform isothermal flash calculation");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing isothermal flash calculation");
                throw;
            }
        }

        public async Task<List<FlashResult>> PerformMultiStageFlashAsync(MultiStageFlashRequest request)
        {
            try
            {
                var result = await _apiClient.PostAsync<MultiStageFlashRequest, List<FlashResult>>(
                    "/api/flashcalculation/multi-stage", request);
                return result ?? new List<FlashResult>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing multi-stage flash calculation with {Stages} stages", request.Stages);
                throw;
            }
        }

        public async Task<bool> SaveFlashResultAsync(FlashResult result, string? userId = null)
        {
            try
            {
                var endpoint = "/api/flashcalculation/result";
                if (!string.IsNullOrWhiteSpace(userId))
                {
                    endpoint += $"?userId={Uri.EscapeDataString(userId)}";
                }

                return await _apiClient.PostAsync(endpoint, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving flash calculation result");
                return false;
            }
        }

        public async Task<List<FlashResult>> GetFlashHistoryAsync(string? componentId = null)
        {
            try
            {
                var endpoint = "/api/flashcalculation/history";
                if (!string.IsNullOrWhiteSpace(componentId))
                {
                    endpoint += $"?componentId={Uri.EscapeDataString(componentId)}";
                }

                var result = await _apiClient.GetAsync<List<FlashResult>>(endpoint);
                return result ?? new List<FlashResult>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting flash calculation history for component {ComponentId}", componentId);
                return new List<FlashResult>();
            }
        }

        #endregion

        #region Gas Properties Operations

        public async Task<GAS_PROPERTIES> AnalyzeGasPropertiesAsync(GasComposition composition, decimal pressure, decimal temperature, string correlation = "Standing-Katz")
        {
            try
            {
                var zFactor = await _apiClient.PostAsync<CalculateZFactorRequest, decimal>(
                    "/api/gasproperties/calculate-zfactor",
                    new CalculateZFactorRequest
                    {
                        Pressure = pressure,
                        Temperature = temperature,
                        SpecificGravity = composition.SpecificGravity,
                        Correlation = correlation
                    });

                var resolvedZFactor = zFactor;
                var density = await _apiClient.PostAsync<CalculateGasDensityRequest, decimal>(
                    "/api/gasproperties/calculate-density",
                    new CalculateGasDensityRequest
                    {
                        Pressure = pressure,
                        Temperature = temperature,
                        ZFactor = resolvedZFactor,
                        MolecularWeight = composition.MolecularWeight
                    });

                var gasFvf = await _apiClient.PostAsync<CalculateGasFVFRequest, decimal>(
                    "/api/gasproperties/calculate-fvf",
                    new CalculateGasFVFRequest
                    {
                        Pressure = pressure,
                        Temperature = temperature,
                        ZFactor = resolvedZFactor
                    });

                return new GAS_PROPERTIES
                {
                    GAS_COMPOSITION_ID = composition.CompositionId,
                    PRESSURE = pressure,
                    TEMPERATURE = temperature,
                    Z_FACTOR = resolvedZFactor,
                    DENSITY = density,
                    GAS_FVF = gasFvf,
                    SPECIFIC_GRAVITY = composition.SpecificGravity,
                    MOLECULAR_WEIGHT = composition.MolecularWeight
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing gas properties for composition {CompositionId}", composition.CompositionId);
                throw;
            }
        }

        public async Task<CompositionSaveResponse?> SaveGasCompositionAsync(GasComposition composition, string? userId = null)
        {
            try
            {
                var endpoint = "/api/gasproperties/composition";
                if (!string.IsNullOrWhiteSpace(userId))
                {
                    endpoint += $"?userId={Uri.EscapeDataString(userId)}";
                }

                return await _apiClient.PostAsync<GasComposition, CompositionSaveResponse>(endpoint, composition);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving gas composition {CompositionId}", composition.CompositionId);
                throw;
            }
        }

        public async Task<GasComposition?> GetGasCompositionAsync(string compositionId)
        {
            try
            {
                return await _apiClient.GetAsync<GasComposition>(
                    $"/api/gasproperties/composition/{Uri.EscapeDataString(compositionId)}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting gas composition {CompositionId}", compositionId);
                return null;
            }
        }

        #endregion

        #region Oil Properties Operations

        public async Task<OilPropertyResult> CalculateOilPropertiesAsync(CalculateOilPropertiesRequest request)
        {
            try
            {
                var result = await _apiClient.PostAsync<CalculateOilPropertiesRequest, OilPropertyResult>(
                    "/api/oilproperties/calculate-properties", request);
                return result ?? throw new InvalidOperationException("Failed to calculate oil properties");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating oil properties for composition {CompositionId}", request.Composition.CompositionId);
                throw;
            }
        }

        public async Task<CompositionSaveResponse?> SaveOilCompositionAsync(OilComposition composition, string? userId = null)
        {
            try
            {
                var endpoint = "/api/oilproperties/composition";
                if (!string.IsNullOrWhiteSpace(userId))
                {
                    endpoint += $"?userId={Uri.EscapeDataString(userId)}";
                }

                return await _apiClient.PostAsync<OilComposition, CompositionSaveResponse>(endpoint, composition);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving oil composition {CompositionId}", composition.CompositionId);
                throw;
            }
        }

        public async Task<OilComposition?> GetOilCompositionAsync(string compositionId)
        {
            try
            {
                return await _apiClient.GetAsync<OilComposition>(
                    $"/api/oilproperties/composition/{Uri.EscapeDataString(compositionId)}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting oil composition {CompositionId}", compositionId);
                return null;
            }
        }

        public async Task<List<OilPropertyResult>> GetOilPropertyHistoryAsync(string compositionId)
        {
            try
            {
                var result = await _apiClient.GetAsync<List<OilPropertyResult>>(
                    $"/api/oilproperties/composition/{Uri.EscapeDataString(compositionId)}/history");
                return result ?? new List<OilPropertyResult>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting oil property history for composition {CompositionId}", compositionId);
                return new List<OilPropertyResult>();
            }
        }

        public async Task<CalculationSaveResponse?> SaveOilPropertyResultAsync(OilPropertyResult result, string? userId = null)
        {
            try
            {
                var endpoint = "/api/oilproperties/result";
                if (!string.IsNullOrWhiteSpace(userId))
                {
                    endpoint += $"?userId={Uri.EscapeDataString(userId)}";
                }

                return await _apiClient.PostAsync<OilPropertyResult, CalculationSaveResponse>(endpoint, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving oil property result {CalculationId}", result.CalculationId);
                throw;
            }
        }

        #endregion

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
                return result ?? new GAS_LIFT_PERFORMANCE { WELL_UWI = wellUWI };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting gas lift performance for well {WellUWI}", wellUWI);
                return new GAS_LIFT_PERFORMANCE { WELL_UWI = wellUWI };
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
                var request = new ProductionForecastRequest
                {
                    WellUWI = wellUWI,
                    FieldId = fieldId,
                    ForecastMethod = ParseForecastType(forecastMethod),
                    ForecastPeriod = forecastPeriod
                };
                var result = await _apiClient.PostAsync<ProductionForecastRequest, ProductionForecastResult>(
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

        private static ForecastType ParseForecastType(string forecastMethod)
        {
            if (Enum.TryParse<ForecastType>(forecastMethod, ignoreCase: true, out var parsed))
            {
                return parsed;
            }

            return forecastMethod?.Trim().ToUpperInvariant() switch
            {
                "EXPONENTIAL" => ForecastType.Exponential,
                "HYPERBOLIC" => ForecastType.Hyperbolic,
                "HARMONIC" => ForecastType.Harmonic,
                _ => ForecastType.Decline,
            };
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


