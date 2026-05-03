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
using Beep.OilandGas.WellTestAnalysis.Constants;
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
            WELL_TEST_DATA? assembledWellTestData = null;
            string? effectiveAnalysisType = null;
            try
            {
                if (request == null)
                    throw new ArgumentNullException(nameof(request));

                if (string.IsNullOrWhiteSpace(request.WellId))
                    throw new ArgumentException("WellId is required for well test analysis.");

                var explicitAnalysisType = string.IsNullOrWhiteSpace(request.AnalysisType)
                    ? null
                    : request.AnalysisType.Trim();
                if (explicitAnalysisType != null
                    && !string.Equals(explicitAnalysisType, "BUILDUP", StringComparison.OrdinalIgnoreCase)
                    && !string.Equals(explicitAnalysisType, "DRAWDOWN", StringComparison.OrdinalIgnoreCase))
                {
                    throw new ArgumentException(
                        "AnalysisType must be BUILDUP, DRAWDOWN, or omitted so it can be inferred from PPDM WELL_TEST.TEST_TYPE.");
                }

                var usePpdmPressureSeries = request.PressureTimeData == null || request.PressureTimeData.Count == 0;
                if (usePpdmPressureSeries && string.IsNullOrWhiteSpace(request.TestId))
                {
                    throw new ArgumentException(
                        "TestId is required to load WELL_TEST_PRESS_MEAS from PPDM for a specific test. Select a stored well test or provide manual pressure-time data.");
                }

                if (request.PressureTimeData is { Count: > 0 and < 3 })
                {
                    throw new ArgumentException(
                        "At least three pressure-time points are required when PressureTimeData is supplied.");
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
                        WELL_TEST_DATA_ID = Guid.NewGuid().ToString(),
                        // UWI belongs in CalculationWellUwi — AREA_ID is PPDM AREA FK, not the well identifier.
                        CalculationWellUwi = request.WellId!.Trim(),
                        CalculationFieldId = string.IsNullOrWhiteSpace(request.FieldId) ? null : request.FieldId.Trim(),
                        CalculationPpdmTestNumber = string.IsNullOrWhiteSpace(request.TestId) ? null : request.TestId.Trim(),
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
                        GAS_SPECIFIC_GRAVITY = request.IsGasWell is true
                            ? (request.GasSpecificGravity ?? 0.65m)
                            : null,
                        RESERVOIR_TEMPERATURE = request.ReservoirTemperature ?? 150.0m,
                        INITIAL_RESERVOIR_PRESSURE = request.InitialReservoirPressure ?? 0.0m,
                        TEST_TYPE = string.Equals(explicitAnalysisType, "DRAWDOWN", StringComparison.OrdinalIgnoreCase)
                            ? WellTestType.Drawdown.ToString()
                            : WellTestType.BuildUp.ToString()
                    };
                }
                else
                {
                    // Retrieve from PPDM data
                    WELL_TEST_DATA = await GetWellTestDataFromPPDMAsync(request.WellId ?? string.Empty, request.TestId ?? string.Empty);
                    ApplyWellTestAnalysisRequestToPpdmData(WELL_TEST_DATA, request);
                }

                assembledWellTestData = WELL_TEST_DATA;

                effectiveAnalysisType = explicitAnalysisType ?? InferAnalysisTypeFromWellTestData(WELL_TEST_DATA);
                if (string.IsNullOrWhiteSpace(effectiveAnalysisType))
                    effectiveAnalysisType = "BUILDUP";
                if (!string.Equals(effectiveAnalysisType, "BUILDUP", StringComparison.OrdinalIgnoreCase)
                    && !string.Equals(effectiveAnalysisType, "DRAWDOWN", StringComparison.OrdinalIgnoreCase))
                {
                    throw new ArgumentException(
                        "Could not resolve well test analysis type as BUILDUP or DRAWDOWN. Set WellTestAnalysisCalculationRequest.AnalysisType explicitly.");
                }

                SyncWellTestDataTestTypeFromEffectiveAnalysis(WELL_TEST_DATA, effectiveAnalysisType);

                var methodUpper = request.AnalysisMethod?.Trim().ToUpperInvariant();
                if (!string.IsNullOrEmpty(methodUpper)
                    && string.Equals(effectiveAnalysisType, "BUILDUP", StringComparison.OrdinalIgnoreCase)
                    && request.IsGasWell is not true
                    && methodUpper != "HORNER"
                    && methodUpper != "MDH")
                {
                    throw new ArgumentException("AnalysisMethod must be HORNER or MDH for oil build-up.");
                }

                if (string.Equals(effectiveAnalysisType, "DRAWDOWN", StringComparison.OrdinalIgnoreCase)
                    && methodUpper == "MDH")
                {
                    throw new ArgumentException(
                        "MDH (Miller-Dyes-Hutchinson) applies to build-up tests only. Use AnalysisType BUILDUP for MDH, or set AnalysisMethod to HORNER for drawdown.");
                }

                if (string.Equals(WELL_TEST_DATA.TEST_TYPE, WellTestType.BuildUp.ToString(), StringComparison.OrdinalIgnoreCase)
                    && (!WELL_TEST_DATA.PRODUCTION_TIME.HasValue || WELL_TEST_DATA.PRODUCTION_TIME.Value <= 0))
                {
                    throw new ArgumentException(
                        "ProductionTime (hours of flow prior to shut-in) must be positive for build-up Horner/MDH analysis. " +
                        "Set WellTestAnalysisCalculationRequest.ProductionTime or supply PPDM data that includes flowing time.");
                }

                if (WELL_TEST_DATA.FLOW_RATE <= 0)
                {
                    throw new ArgumentException(
                        "Flow rate must be positive. Set FlowRate on the calculation request or ensure WELL_TEST has MAX_OIL_FLOW_RATE or MAX_GAS_FLOW_RATE.");
                }

                if ((double)WELL_TEST_DATA.FLOW_RATE < WellTestConstants.MinFlowRate
                    || (double)WELL_TEST_DATA.FLOW_RATE > WellTestConstants.MaxFlowRate)
                {
                    throw new ArgumentException(
                        $"Flow rate must be between {WellTestConstants.MinFlowRate} and {WellTestConstants.MaxFlowRate} (BPD or Mscf/d per model assumptions).");
                }

                // Step 2: Perform well test analysis
                WELL_TEST_ANALYSIS_RESULT analysisResult;

                if (string.Equals(effectiveAnalysisType, "BUILDUP", StringComparison.OrdinalIgnoreCase))
                {
                    if (WELL_TEST_DATA.IS_GAS_WELL is true)
                    {
                        if (!WELL_TEST_DATA.GAS_SPECIFIC_GRAVITY.HasValue || WELL_TEST_DATA.GAS_SPECIFIC_GRAVITY.Value <= 0)
                        {
                            throw new ArgumentException(
                                "Gas specific gravity is required for gas well build-up (pseudo-pressure m(p)). Set GasSpecificGravity on the request (air = 1.0).");
                        }

                        var gasGrav = (double)WELL_TEST_DATA.GAS_SPECIFIC_GRAVITY.Value;
                        if (gasGrav < 0.05 || gasGrav > 3.0)
                        {
                            throw new ArgumentException(
                                "Gas specific gravity must be between 0.05 and 3.0 (air = 1.0) for pseudo-pressure m(p) build-up.");
                        }

                        if (string.Equals(request.AnalysisMethod?.Trim(), "MDH", StringComparison.OrdinalIgnoreCase))
                        {
                            _logger?.LogInformation(
                                "Gas build-up uses pseudo-pressure Horner; MDH selection ignored for WellId {WellId}, TestId {TestId}.",
                                request.WellId, request.TestId);
                        }

                        // GasWellAnalysis expects reservoir temperature in Rankine; UI / PPDM store °F.
                        var tempRankine = (double)WELL_TEST_DATA.RESERVOIR_TEMPERATURE + WellTestConstants.FahrenheitToRankine;
                        analysisResult = WellTestAnalyzer.AnalyzeGasBuildUp(
                            WELL_TEST_DATA,
                            gasGrav,
                            tempRankine);
                    }
                    else
                    {
                        var analysisMethod = (request.AnalysisMethod ?? "HORNER").Trim().ToUpperInvariant();
                        analysisResult = analysisMethod == "MDH"
                            ? WellTestAnalyzer.AnalyzeBuildUpMDH(WELL_TEST_DATA)
                            : WellTestAnalyzer.AnalyzeBuildUp(WELL_TEST_DATA);
                    }
                }
                else
                {
                    if (WELL_TEST_DATA.IS_GAS_WELL is true)
                    {
                        throw new ArgumentException(
                            "Gas well drawdown is not supported in this release. Use build-up with the gas-well option (m(p) Horner), or turn off Gas well to run the oil drawdown model as an approximation.");
                    }

                    analysisResult = WellTestAnalyzer.AnalyzeDrawdown(WELL_TEST_DATA);
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
                var result = MapWellTestResultToDTO(
                    analysisResult,
                    request,
                    WELL_TEST_DATA,
                    identifiedModel,
                    pressureTimePoints,
                    derivativePoints,
                    effectiveAnalysisType);

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
                    request?.WellId, request?.TestId);

                // Try to store error result (skip when request was never constructed — e.g. null body)
                if (request != null)
                {
                    try
                    {
                        var repository = await GetWellTestResultRepositoryAsync();
                        var analysisTypeForError = effectiveAnalysisType
                            ?? (string.IsNullOrWhiteSpace(request.AnalysisType) ? null : request.AnalysisType.Trim())
                            ?? (assembledWellTestData != null ? InferAnalysisTypeFromWellTestData(assembledWellTestData) : null)
                            ?? "BUILDUP";
                        var errorResult = new WELL_TEST_ANALYSIS_RESULT
                        {
                            CALCULATION_ID = Guid.NewGuid().ToString(),
                            WELL_ID = request.WellId,
                            FIELD_ID = string.IsNullOrWhiteSpace(request.FieldId) ? null : request.FieldId.Trim(),
                            TEST_ID = request.TestId,
                            ANALYSIS_TYPE = NormalizeAnalysisTypeForStorage(analysisTypeForError) ?? string.Empty,
                            ANALYSIS_METHOD = ResolvePersistedWellTestAnalysisMethod(request, assembledWellTestData, analysisTypeForError),
                            CALCULATION_DATE = DateTime.UtcNow,
                            STATUS = "FAILED",
                            ERROR_MESSAGE = ex.Message,
                            USER_ID = request.UserId,
                            IS_GAS_WELL = (assembledWellTestData?.IS_GAS_WELL ?? request.IsGasWell) ?? false
                        };
                        await InsertAnalysisResultAsync(repository, errorResult, request.UserId);
                    }
                    catch (Exception storeEx)
                    {
                        _logger?.LogError(storeEx, "Error storing Well Test Analysis error result");
                    }
                }

                throw;
            }
        }

        #endregion

        #region Well Test Analysis Helper Methods

        /// <summary>
        /// Retrieves well test data from PPDM by assembling WELL_TEST header and WELL_TEST_PRESS_MEAS time-pressure series.
        /// Elapsed times are converted to hours using <see cref="WELL_TEST_PRESS_MEAS.MEASUREMENT_TIME_ELAPSED_OUOM"/> when present,
        /// sorted by that converted time. Pressures are converted to psi using <see cref="WELL_TEST_PRESS_MEAS.MEASUREMENT_PRESSURE_OUOM"/> when recognized.
        /// </summary>
        private async Task<WELL_TEST_DATA> GetWellTestDataFromPPDMAsync(string wellId, string testId)
        {
            var data = new WELL_TEST_DATA
            {
                WELL_TEST_DATA_ID = Guid.NewGuid().ToString(),
                CalculationWellUwi = string.IsNullOrWhiteSpace(wellId) ? null : wellId.Trim(),
                CalculationPpdmTestNumber = string.IsNullOrWhiteSpace(testId) ? null : testId.Trim(),
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

                        var durationHours = ConvertWellTestDurationToHours(testRecord.TEST_DURATION, testRecord.TEST_DURATION_OUOM);
                        if (durationHours.HasValue && durationHours.Value > 0)
                            data.PRODUCTION_TIME = durationHours.Value;
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
                    var measurements = measResults?.OfType<WELL_TEST_PRESS_MEAS>().ToList();

                    if (measurements != null && measurements.Count > 0)
                    {
                        var points = measurements
                            .Select(m =>
                            {
                                var hours = ConvertWellTestDurationToHours(m.MEASUREMENT_TIME_ELAPSED, m.MEASUREMENT_TIME_ELAPSED_OUOM);
                                var tHours = (double)(hours ?? m.MEASUREMENT_TIME_ELAPSED);
                                var psi = ConvertWellTestPressureToPsi(m.MEASUREMENT_PRESSURE, m.MEASUREMENT_PRESSURE_OUOM);
                                return (TimeHours: tHours, PressurePsi: psi, ObsNo: (double)m.MEASUREMENT_OBS_NO);
                            })
                            .OrderBy(p => p.TimeHours)
                            .ThenBy(p => p.ObsNo)
                            .ToList();

                        data.Time = points.Select(p => p.TimeHours).ToList();
                        data.Pressure = points.Select(p => p.PressurePsi).ToList();
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
                var testScope = string.IsNullOrEmpty(testId)
                    ? $"well '{wellId}'"
                    : $"well '{wellId}', test '{testId}'";
                throw new InvalidOperationException(
                    $"No pressure-time measurements found in PPDM for {testScope}. " +
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
            WELL_TEST_DATA dataUsed,
            ReservoirModel? identifiedModel,
            List<PRESSURE_TIME_POINT>? pressureTimePoints,
            List<PRESSURE_TIME_POINT>? derivativePoints,
            string effectiveAnalysisType)
        {
            var resolvedAnalysisMethod = ResolveSuccessWellTestAnalysisMethod(analysisResult, request, dataUsed, effectiveAnalysisType);
            var result = new WELL_TEST_ANALYSIS_RESULT
            {
                CALCULATION_ID = Guid.NewGuid().ToString(),
                WELL_ID = request.WellId,
                FIELD_ID = string.IsNullOrWhiteSpace(request.FieldId) ? null : request.FieldId.Trim(),
                TEST_ID = request.TestId,
                ANALYSIS_TYPE = NormalizeAnalysisTypeForStorage(effectiveAnalysisType) ?? "BUILDUP",
                ANALYSIS_METHOD = resolvedAnalysisMethod,
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
                IS_GAS_WELL = dataUsed.IS_GAS_WELL ?? false,
                GAS_SPECIFIC_GRAVITY = dataUsed.IS_GAS_WELL is true
                    ? (double)(dataUsed.GAS_SPECIFIC_GRAVITY ?? request.GasSpecificGravity ?? 0m)
                    : 0d,
                RESERVOIR_TEMPERATURE = (double)dataUsed.RESERVOIR_TEMPERATURE,
                INITIAL_RESERVOIR_PRESSURE = (double)(dataUsed.INITIAL_RESERVOIR_PRESSURE ?? request.InitialReservoirPressure ?? 0m),
                AdditionalResults = new WellTestAnalysisAdditionalResults()
            };

            result.AdditionalResults.AnalysisMethod = resolvedAnalysisMethod;
            result.AdditionalResults.FlowRate = dataUsed.FLOW_RATE;
            result.AdditionalResults.WellboreRadius = dataUsed.WELLBORE_RADIUS ?? 0.25m;
            result.AdditionalResults.FormationThickness = dataUsed.FORMATION_THICKNESS;

            return result;
        }

        private static List<PRESSURE_TIME_POINT> CreatePressureTimePoints(WELL_TEST_DATA data)
        {
            if (data.Time == null || data.Pressure == null)
                throw new ArgumentException("Time and pressure series are required for derivative and diagnostic plots.");
            if (data.Time.Count != data.Pressure.Count)
                throw new ArgumentException("Time and pressure series must have the same length.");

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

        /// <summary>
        /// Overlays calculation-request inputs onto PPDM-assembled <see cref="WELL_TEST_DATA"/> so UI defaults
        /// (flow rate, porosity, compressibility, etc.) satisfy well-test validation and match the user's selected
        /// analysis type (build-up vs drawdown).
        /// </summary>
        private static void ApplyWellTestAnalysisRequestToPpdmData(WELL_TEST_DATA data, WellTestAnalysisCalculationRequest request)
        {
            // When AnalysisType is omitted, keep WELL_TEST.TEST_TYPE on the assembled row until the service resolves BUILDUP vs DRAWDOWN.
            SetWellTestDataTestTypeFromRequest(data, request);

            if (request.FlowRate.HasValue && request.FlowRate.Value > 0)
                data.FLOW_RATE = request.FlowRate.Value;

            if (request.WellboreRadius.HasValue && request.WellboreRadius.Value > 0)
                data.WELLBORE_RADIUS = request.WellboreRadius;
            else
                data.WELLBORE_RADIUS ??= 0.25m;

            if (request.FormationThickness.HasValue && request.FormationThickness.Value > 0)
                data.FORMATION_THICKNESS = request.FormationThickness.Value;

            data.POROSITY = request.Porosity ?? data.POROSITY ?? 0.2m;
            data.TOTAL_COMPRESSIBILITY = request.TotalCompressibility ?? data.TOTAL_COMPRESSIBILITY ?? 1e-5m;
            data.OIL_VISCOSITY = request.OilViscosity ?? data.OIL_VISCOSITY ?? 1.5m;
            data.OIL_FORMATION_VOLUME_FACTOR = request.OilFormationVolumeFactor ?? data.OIL_FORMATION_VOLUME_FACTOR ?? 1.2m;

            if (request.ProductionTime.HasValue && request.ProductionTime.Value > 0)
                data.PRODUCTION_TIME = request.ProductionTime.Value;

            if (request.IsGasWell.HasValue)
                data.IS_GAS_WELL = request.IsGasWell;

            if (request.IsGasWell is false)
                data.GAS_SPECIFIC_GRAVITY = null;
            else if (request.GasSpecificGravity.HasValue && request.GasSpecificGravity.Value > 0)
                data.GAS_SPECIFIC_GRAVITY = request.GasSpecificGravity;

            if (request.ReservoirTemperature.HasValue && request.ReservoirTemperature.Value > 0)
                data.RESERVOIR_TEMPERATURE = request.ReservoirTemperature.Value;

            if (request.InitialReservoirPressure.HasValue)
                data.INITIAL_RESERVOIR_PRESSURE = request.InitialReservoirPressure;
        }

        /// <summary>
        /// Converts PPDM <c>WELL_TEST.TEST_DURATION</c> to hours for Horner / MDH production time when OUOM is known.
        /// Unknown or empty OUOM is treated as hours (common convention).
        /// Explicit hour tokens (<c>HOUR</c>, <c>HR</c>, <c>HRS</c>, <c>H</c>) are treated as hours without scaling.
        /// <c>WEEK</c>/<c>WK</c> converts to hours; <c>MONTH</c> uses a 30-day month (720 h per month); <c>YEAR</c>/<c>YRS</c> uses 365.25 d;
        /// <c>MILLISEC</c> (substring) converts ms to hours before a generic <c>SEC</c> rule.
        /// </summary>
        private static decimal? ConvertWellTestDurationToHours(decimal duration, string? durationOuom)
        {
            if (duration <= 0)
                return null;

            if (string.IsNullOrWhiteSpace(durationOuom))
                return duration;

            var u = durationOuom.Trim().ToUpperInvariant();
            if (u.Contains("DAY") || u == "D")
                return duration * 24m;
            if (u.Contains("WEEK") || u == "WK")
                return duration * 24m * 7m;
            if (u.Contains("MONTH"))
                return duration * 24m * 30m;
            if (u.Contains("YEAR") || u == "YRS")
                return duration * 24m * 365.25m;
            if (u.Contains("HOUR") || u == "HR" || u == "HRS" || u == "H")
                return duration;
            if (u.Contains("MILLISEC"))
                return duration / 3600000m;
            if (u.Contains("MIN"))
                return duration / 60m;
            if (u.Contains("SEC"))
                return duration / 3600m;
            return duration;
        }

        /// <summary>
        /// Converts a PPDM-reported gauge/bottom-hole pressure to psi for <see cref="WellTestConstants"/> validation and the analyzer.
        /// Unknown or empty OUOM is treated as psi (common convention for legacy rows).
        /// </summary>
        private static double ConvertWellTestPressureToPsi(decimal pressure, string? pressureOuom)
        {
            if (string.IsNullOrWhiteSpace(pressureOuom))
                return (double)pressure;

            var u = pressureOuom.Trim().ToUpperInvariant();
            if (u.Contains("HPA") || u.Contains("HECTOPAS"))
                return (double)(pressure * 0.0145037738m);
            if (u.Contains("KPA"))
                return (double)(pressure * 0.145037738m);
            if (u.Contains("MPA") || u.Contains("MEGAPAS"))
                return (double)(pressure * 145.037738m);
            if (u.Contains("BAR"))
                return (double)(pressure * 14.5037738m);
            if (u.Contains("ATM"))
                return (double)(pressure * 14.6959487755142m);
            if (u.Contains("PAS") && !u.Contains("KPA") && !u.Contains("MPA"))
                return (double)(pressure / 6894.757293168361m);
            if (u.Contains("KGF") && u.Contains("CM"))
                return (double)(pressure * 14.2233433071m);

            return (double)pressure;
        }

        private static void SetWellTestDataTestTypeFromRequest(WELL_TEST_DATA data, WellTestAnalysisCalculationRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.AnalysisType))
                return;
            data.TEST_TYPE = string.Equals(request.AnalysisType, "DRAWDOWN", StringComparison.OrdinalIgnoreCase)
                ? WellTestType.Drawdown.ToString()
                : WellTestType.BuildUp.ToString();
        }

        /// <summary>
        /// Aligns <see cref="WELL_TEST_DATA.TEST_TYPE"/> with the resolved API classification so validators and analyzers agree.
        /// </summary>
        private static void SyncWellTestDataTestTypeFromEffectiveAnalysis(WELL_TEST_DATA data, string effectiveAnalysisType)
        {
            data.TEST_TYPE = string.Equals(effectiveAnalysisType, "DRAWDOWN", StringComparison.OrdinalIgnoreCase)
                ? WellTestType.Drawdown.ToString()
                : WellTestType.BuildUp.ToString();
        }

        /// <summary>
        /// Maps assembled <see cref="WELL_TEST_DATA.TEST_TYPE"/> (from PPDM or manual payload) to <c>BUILDUP</c> or <c>DRAWDOWN</c> for routing.
        /// </summary>
        private static string? InferAnalysisTypeFromWellTestData(WELL_TEST_DATA data)
        {
            if (data?.TEST_TYPE == null)
                return null;
            if (Enum.TryParse<WellTestType>(data.TEST_TYPE, ignoreCase: true, out var testType))
                return testType == WellTestType.Drawdown ? "DRAWDOWN" : "BUILDUP";
            if (data.TEST_TYPE.Contains("DRAW", StringComparison.OrdinalIgnoreCase))
                return "DRAWDOWN";
            return "BUILDUP";
        }

        /// <summary>
        /// Canonical <c>BUILDUP</c> / <c>DRAWDOWN</c> for persistence and API responses; leaves null/whitespace unchanged for error rows when validation never ran.
        /// </summary>
        private static string? NormalizeAnalysisTypeForStorage(string? analysisType)
        {
            if (string.IsNullOrWhiteSpace(analysisType))
                return analysisType;
            if (string.Equals(analysisType, "DRAWDOWN", StringComparison.OrdinalIgnoreCase))
                return "DRAWDOWN";
            if (string.Equals(analysisType, "BUILDUP", StringComparison.OrdinalIgnoreCase))
                return "BUILDUP";
            return analysisType.Trim().ToUpperInvariant();
        }

        /// <summary>
        /// When the analyzer leaves <see cref="WELL_TEST_ANALYSIS_RESULT.ANALYSIS_METHOD"/> empty, gas build-up must not fall back to a client MDH selection (engine is Horner / m(p)).
        /// </summary>
        private static string ResolveSuccessWellTestAnalysisMethod(
            WELL_TEST_ANALYSIS_RESULT analysisResult,
            WellTestAnalysisCalculationRequest request,
            WELL_TEST_DATA dataUsed,
            string effectiveAnalysisType)
        {
            if (!string.IsNullOrWhiteSpace(analysisResult.ANALYSIS_METHOD))
                return analysisResult.ANALYSIS_METHOD.Trim();

            if (dataUsed.IS_GAS_WELL is true
                && string.Equals(effectiveAnalysisType, "BUILDUP", StringComparison.OrdinalIgnoreCase))
                return "HORNER";

            return string.IsNullOrWhiteSpace(request.AnalysisMethod)
                ? "HORNER"
                : request.AnalysisMethod.Trim().ToUpperInvariant();
        }

        /// <summary>
        /// Gas build-up uses m(p) Horner only; persist <c>HORNER</c> on failure rows. Otherwise normalize method text.
        /// </summary>
        private static string ResolvePersistedWellTestAnalysisMethod(
            WellTestAnalysisCalculationRequest request,
            WELL_TEST_DATA? assembledData,
            string? resolvedAnalysisType = null)
        {
            var analysisKind = resolvedAnalysisType ?? request.AnalysisType ?? "BUILDUP";
            var isGasBuildUp = string.Equals(analysisKind, "BUILDUP", StringComparison.OrdinalIgnoreCase)
                && ((assembledData?.IS_GAS_WELL ?? request.IsGasWell) is true);
            if (isGasBuildUp)
                return "HORNER";

            var trimmed = request.AnalysisMethod?.Trim();
            if (string.IsNullOrEmpty(trimmed))
                return "HORNER";
            return trimmed.ToUpperInvariant();
        }

        #endregion
    }
}
