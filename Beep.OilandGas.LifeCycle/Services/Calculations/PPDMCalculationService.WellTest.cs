using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.PPDM39.Repositories;
using Beep.OilandGas.WellTestAnalysis;
using Beep.OilandGas.WellTestAnalysis.Calculations;
using Beep.OilandGas.Models.Data.WellTestAnalysis;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.LifeCycle.Services.Calculations
{
    public partial class PPDMCalculationService
    {
        #region Well Test Analysis

        /// <summary>
        /// Performs well test analysis (pressure transient analysis) for a well.
        /// Supports build-up and drawdown analysis using Horner or MDH methods.
        /// </summary>
        /// <param name="request">Well test analysis request containing well ID, test ID, pressure-time data, and analysis parameters</param>
        /// <returns>Well test analysis result with permeability, skin factor, reservoir pressure, productivity index, and diagnostic data</returns>
        /// <exception cref="ArgumentException">Thrown when request validation fails</exception>
        /// <exception cref="InvalidOperationException">Thrown when well test data is unavailable or calculation fails</exception>
        public async Task<WELL_TEST_ANALYSIS_RESULT> PerformWellTestAnalysisAsync(WellTestAnalysisCalculationRequest request)
        {
            try
            {
                // Validate request
                if (string.IsNullOrEmpty(request.WellId) && string.IsNullOrEmpty(request.TestId))
                {
                    throw new ArgumentException("At least one of WellId or TestId must be provided");
                }

                _logger?.LogInformation("Starting Well Test Analysis for WellId: {WellId}, TestId: {TestId}",
                    request.WellId, request.TestId);

                // Step 1: Build well test data from request or PPDM data
                WELL_TEST_DATA WELL_TEST_DATA;
                if (request.PressureTimeData != null && request.PressureTimeData.Count > 0)
                {
                    // Use data from request
                    WELL_TEST_DATA = new WELL_TEST_DATA
                    {
                        Time = request.PressureTimeData.Select(p => (double)(p.Time ?? 0)).ToList(),
                        Pressure = request.PressureTimeData.Select(p => (double)(p.Pressure ?? 0)).ToList(),
                        FLOW_RATE = request.FlowRate ?? 0.0m,
                        WELLBORE_RADIUS = request.WellboreRadius ?? 0.25m,
                        FORMATION_THICKNESS = request.FormationThickness ?? 50.0m,
                        POROSITY = request.Porosity ?? 0.2m,
                        TOTAL_COMPRESSIBILITY = request.TotalCompressibility ?? 1e-5m,
                        OIL_VISCOSITY = request.OilViscosity ?? 1.5m,
                        OIL_FORMATION_VOLUME_FACTOR = request.OilFormationVolumeFactor ?? 1.2m,
                        PRODUCTION_TIME = request.ProductionTime ?? 0.0m,
                        IS_GAS_WELL = request.IsGasWell ?? false,
                        GAS_SPECIFIC_GRAVITY = request.GasSpecificGravity ?? 0.65m,
                        RESERVOIR_TEMPERATURE = request.ReservoirTemperature ?? 150.0m,
                        INITIAL_RESERVOIR_PRESSURE = request.InitialReservoirPressure ?? 0.0m,
                        TEST_TYPE = request.AnalysisType?.ToUpper() == "DRAWDOWN"
                            ? WellTestType.Drawdown.ToString()
                            : WellTestType.BuildUp.ToString()
                    };
                }
                else
                {
                    // Retrieve from PPDM data
                    WELL_TEST_DATA = await GetWellTestDataFromPPDMAsync(request.WellId ?? string.Empty, request.TestId ?? string.Empty);
                }

                // Step 2: Perform well test analysis
                WELL_TEST_ANALYSIS_RESULT analysisResult;
                string analysisMethod = request.AnalysisMethod?.ToUpper() ?? "HORNER";

                if (request.AnalysisType?.ToUpper() == "BUILDUP")
                {
                    if (analysisMethod == "MDH")
                    {
                        analysisResult = WellTestAnalyzer.AnalyzeBuildUpMDH(WELL_TEST_DATA);
                    }
                    else
                    {
                        analysisResult = WellTestAnalyzer.AnalyzeBuildUp(WELL_TEST_DATA);
                    }
                }
                else
                {
                    // Drawdown analysis using MDH-based drawdown method
                    if (analysisMethod == "MDH")
                    {
                        analysisResult = WellTestAnalyzer.AnalyzeDrawdown(WELL_TEST_DATA);
                    }
                    else
                    {
                        analysisResult = WellTestAnalyzer.AnalyzeDrawdown(WELL_TEST_DATA);
                    }
                }

                // Step 3: Calculate derivative for diagnostic plots
                var pressureTimePoints = CreatePressureTimePoints(WELL_TEST_DATA);
                List<PRESSURE_TIME_POINT>? derivativePoints = null;
                ReservoirModel? identifiedModel = null;

                try
                {
                    derivativePoints = WellTestAnalyzer.CalculateDerivative(pressureTimePoints);
                    if (derivativePoints.Count > 0)
                    {
                        identifiedModel = WellTestAnalyzer.IdentifyReservoirModel(derivativePoints);
                    }
                }
                catch (Exception diagnosticEx)
                {
                    _logger?.LogWarning(diagnosticEx, "Well Test diagnostics could not be derived for WellId: {WellId}, TestId: {TestId}",
                        request.WellId, request.TestId);
                }

                // Step 5: Map to DTO
                var result = MapWellTestResultToDTO(analysisResult, request, identifiedModel, pressureTimePoints, derivativePoints);

                // Step 6: Store result in PPDM database
                try
                {
                    var repository = await GetWellTestResultRepositoryAsync();
                    result.DIAGNOSTIC_DATA_JSON = JsonSerializer.Serialize(result.DiagnosticPoints ?? new List<WellTestDataPoint>());
                    result.DERIVATIVE_DATA_JSON = JsonSerializer.Serialize(result.DerivativePoints ?? new List<WellTestDataPoint>());

                    await InsertAnalysisResultAsync(repository, result, request.UserId);
                    _logger?.LogInformation("Stored Well Test Analysis result with ID: {CalculationId}", result.CALCULATION_ID);
                }
                catch (Exception storeEx)
                {
                    _logger?.LogError(storeEx, "Error storing Well Test Analysis result");
                    // Continue - don't fail the operation if storage fails
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error performing Well Test Analysis for WellId: {WellId}, TestId: {TestId}",
                    request.WellId, request.TestId);

                // Try to store error result
                try
                {
                    var repository = await GetWellTestResultRepositoryAsync();
                    var errorResult = new WELL_TEST_ANALYSIS_RESULT
                    {
                        CALCULATION_ID = Guid.NewGuid().ToString(),
                        WELL_ID = request.WellId,
                        TEST_ID = request.TestId,
                        ANALYSIS_TYPE = request.AnalysisType,
                        ANALYSIS_METHOD = request.AnalysisMethod ?? "HORNER",
                        CALCULATION_DATE = DateTime.UtcNow,
                        STATUS = "FAILED",
                        ERROR_MESSAGE = ex.Message
                    };
                    await InsertAnalysisResultAsync(repository, errorResult, request.UserId);
                }
                catch (Exception storeEx)
                {
                    _logger?.LogError(storeEx, "Error storing Well Test Analysis error result");
                }

                throw;
            }
        }

        #endregion

        #region Well Test Analysis Helper Methods

        /// <summary>
        /// Retrieves well test data from PPDM by assembling WELL_TEST header and WELL_TEST_PRESS_MEAS time-pressure series.
        /// </summary>
        private async Task<WELL_TEST_DATA> GetWellTestDataFromPPDMAsync(string wellId, string testId)
        {
            var data = new WELL_TEST_DATA
            {
                WELL_TEST_DATA_ID = Guid.NewGuid().ToString(),
                FLOW_RATE = 0m,
                FORMATION_THICKNESS = 50m,
                WELLBORE_RADIUS = 0.25m,
                RESERVOIR_TEMPERATURE = 150m,
                TEST_TYPE = "BUILDUP"
            };

            try
            {
                // Query WELL_TEST header for flow rate and test metadata
                var testMetadata = await _metadata.GetTableMetadataAsync("WELL_TEST");
                if (testMetadata != null)
                {
                    var testEntityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{testMetadata.EntityTypeName}") ?? typeof(WELL_TEST);
                    var testRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata, testEntityType, _connectionName, "WELL_TEST");

                    var testFilters = new List<AppFilter>
                    {
                        new AppFilter { FieldName = "UWI", Operator = "=", FilterValue = wellId },
                        new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
                    };
                    if (!string.IsNullOrEmpty(testId))
                        testFilters.Add(new AppFilter { FieldName = "TEST_NUM", Operator = "=", FilterValue = testId });

                    var testResults = await testRepo.GetAsync(testFilters);
                    var testRecord = testResults?.OfType<WELL_TEST>().OrderByDescending(t => t.EFFECTIVE_DATE).FirstOrDefault();
                    if (testRecord != null)
                    {
                        data.FLOW_RATE = testRecord.MAX_OIL_FLOW_RATE > 0 ? testRecord.MAX_OIL_FLOW_RATE
                            : testRecord.MAX_GAS_FLOW_RATE > 0 ? testRecord.MAX_GAS_FLOW_RATE : 0m;
                        data.RESERVOIR_TEMPERATURE = testRecord.FLOW_TEMPERATURE > 0 ? testRecord.FLOW_TEMPERATURE : 150m;
                        data.TEST_TYPE = testRecord.TEST_TYPE ?? "BUILDUP";
                    }
                }

                // Query WELL_TEST_PRESS_MEAS for time-pressure series
                var measMetadata = await _metadata.GetTableMetadataAsync("WELL_TEST_PRESS_MEAS");
                if (measMetadata != null)
                {
                    var measEntityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{measMetadata.EntityTypeName}") ?? typeof(WELL_TEST_PRESS_MEAS);
                    var measRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata, measEntityType, _connectionName, "WELL_TEST_PRESS_MEAS");

                    var measFilters = new List<AppFilter>
                    {
                        new AppFilter { FieldName = "UWI", Operator = "=", FilterValue = wellId },
                        new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
                    };
                    if (!string.IsNullOrEmpty(testId))
                        measFilters.Add(new AppFilter { FieldName = "TEST_NUM", Operator = "=", FilterValue = testId });

                    var measResults = await measRepo.GetAsync(measFilters);
                    var measurements = measResults?.OfType<WELL_TEST_PRESS_MEAS>()
                        .OrderBy(m => m.MEASUREMENT_TIME_ELAPSED)
                        .ToList();

                    if (measurements != null && measurements.Count > 0)
                    {
                        data.Time = measurements.Select(m => (double)m.MEASUREMENT_TIME_ELAPSED).ToList();
                        data.Pressure = measurements.Select(m => (double)m.MEASUREMENT_PRESSURE).ToList();
                        return data;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Error retrieving well test data from PPDM for well {WellId}. Provide PressureTimeData in the request.", wellId);
            }

            if (data.Time.Count == 0)
            {
                throw new InvalidOperationException(
                    $"No pressure-time measurements found in PPDM for well '{wellId}'. " +
                    "Provide PressureTimeData in the request.");
            }

            return data;
        }

        /// <summary>
        /// Maps WELL_TEST_ANALYSIS_RESULT from library to WELL_TEST_ANALYSIS_RESULT DTO
        /// </summary>
        private WELL_TEST_ANALYSIS_RESULT MapWellTestResultToDTO(
            WELL_TEST_ANALYSIS_RESULT analysisResult,
            WellTestAnalysisCalculationRequest request,
            ReservoirModel? identifiedModel,
            List<PRESSURE_TIME_POINT>? pressureTimePoints,
            List<PRESSURE_TIME_POINT>? derivativePoints)
        {
            var result = new WELL_TEST_ANALYSIS_RESULT
            {
                CALCULATION_ID = Guid.NewGuid().ToString(),
                WELL_ID = request.WellId,
                TEST_ID = request.TestId,
                ANALYSIS_TYPE = request.AnalysisType,
                ANALYSIS_METHOD = request.AnalysisMethod ?? "HORNER",
                CALCULATION_DATE = DateTime.UtcNow,
                STATUS = "SUCCESS",
                USER_ID = request.UserId,
                PERMEABILITY = analysisResult.PERMEABILITY,
                SKIN_FACTOR = analysisResult.SKIN_FACTOR,
                RESERVOIR_PRESSURE = analysisResult.RESERVOIR_PRESSURE,
                PRODUCTIVITY_INDEX = analysisResult.PRODUCTIVITY_INDEX,
                FLOW_EFFICIENCY = analysisResult.FLOW_EFFICIENCY,
                DAMAGE_RATIO = analysisResult.DAMAGE_RATIO,
                RADIUS_OF_INVESTIGATION = analysisResult.RADIUS_OF_INVESTIGATION,
                IDENTIFIED_MODEL = identifiedModel?.ToString() ?? analysisResult.IDENTIFIED_MODEL,
                IDENTIFIED_MODEL_STRING = identifiedModel?.ToString() ?? analysisResult.IDENTIFIED_MODEL_STRING,
                R_SQUARED = analysisResult.R_SQUARED,
                DiagnosticPoints = ConvertPressureTimePointsToDataPoints(pressureTimePoints, useDerivative: false),
                DerivativePoints = ConvertPressureTimePointsToDataPoints(derivativePoints, useDerivative: true),
                IS_SUCCESSFUL = true,
                AdditionalResults = new WellTestAnalysisAdditionalResults()
            };

            result.AdditionalResults.AnalysisMethod = analysisResult.ANALYSIS_METHOD;
            result.AdditionalResults.FlowRate = request.FlowRate ?? 0.0m;
            result.AdditionalResults.WellboreRadius = request.WellboreRadius ?? 0.25m;
            result.AdditionalResults.FormationThickness = request.FormationThickness ?? 50.0m;

            return result;
        }

        private static List<PRESSURE_TIME_POINT> CreatePressureTimePoints(WELL_TEST_DATA data)
        {
            return data.Time
                .Zip(data.Pressure, (time, pressure) => new { time, pressure })
                .Select((point, index) => new PRESSURE_TIME_POINT
                {
                    PRESSURE_TIME_POINT_ID = Guid.NewGuid().ToString(),
                    WELL_TEST_DATA_ID = data.WELL_TEST_DATA_ID,
                    TIME = point.time,
                    PRESSURE = point.pressure,
                    POINT_ORDER = index + 1
                })
                .ToList();
        }

        private static List<WellTestDataPoint> ConvertPressureTimePointsToDataPoints(List<PRESSURE_TIME_POINT>? points, bool useDerivative)
        {
            if (points == null || points.Count == 0)
                return new List<WellTestDataPoint>();

            return points
                .Select(point => new WellTestDataPoint
                {
                    Time = point.TIME.HasValue ? (decimal?)point.TIME.Value : null,
                    Pressure = useDerivative
                        ? (point.PRESSURE_DERIVATIVE.HasValue ? (decimal?)point.PRESSURE_DERIVATIVE.Value : null)
                        : (point.PRESSURE.HasValue ? (decimal?)point.PRESSURE.Value : null)
                })
                .ToList();
        }

        #endregion
    }
}
