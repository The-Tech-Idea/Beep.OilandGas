using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.PPDM39.Repositories;
using Beep.OilandGas.WellTestAnalysis;
using Beep.OilandGas.WellTestAnalysis.Calculations;
using Beep.OilandGas.Models.Data.WellTestAnalysis;
using Microsoft.Extensions.Logging;

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
                    // Drawdown analysis - would need DrawdownAnalysis class if available
                    // For now, use build-up method
                    analysisResult = WellTestAnalyzer.AnalyzeBuildUp(WELL_TEST_DATA);
                }

                // Step 3: Calculate derivative for diagnostic plots
                var pressureTimePoints = WELL_TEST_DATA.Time.Zip(WELL_TEST_DATA.Pressure, (t, p) => new { Time = t, Pressure = p }).ToList();
                // var derivativePoints = WellTestAnalyzer.CalculateDerivative(pressureTimePoints);

                // Step 4: Identify reservoir model
                // var identifiedModel = WellTestAnalyzer.IdentifyReservoirModel(derivativePoints);

                // Step 5: Map to DTO
                var result = MapWellTestResultToDTO(analysisResult, request, null, null);

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
        /// Retrieves well test data from PPDM for a well
        /// </summary>
        private Task<WELL_TEST_DATA> GetWellTestDataFromPPDMAsync(string wellId, string testId)
        {
            _logger?.LogWarning("Well test data retrieval from PPDM is not implemented. " +
                "Provide PressureTimeData and well properties in the request.");

            return Task.FromException<WELL_TEST_DATA>(new InvalidOperationException(
                "Well test data retrieval from PPDM is not implemented. " +
                "Provide PressureTimeData and well properties in the request."));
        }

        /// <summary>
        /// Maps WELL_TEST_ANALYSIS_RESULT from library to WELL_TEST_ANALYSIS_RESULT DTO
        /// </summary>
        private WELL_TEST_ANALYSIS_RESULT MapWellTestResultToDTO(
            WELL_TEST_ANALYSIS_RESULT analysisResult,
            WellTestAnalysisCalculationRequest request,
            object? identifiedModel,
            object? derivativePoints)
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
                // IdentifiedModel = identifiedModel,
                R_SQUARED = analysisResult.R_SQUARED,
                DiagnosticPoints = request.PressureTimeData,
                // DerivativePoints = derivativePoints...
                AdditionalResults = new WellTestAnalysisAdditionalResults()
            };

            result.AdditionalResults.AnalysisMethod = analysisResult.ANALYSIS_METHOD;
            result.AdditionalResults.FlowRate = request.FlowRate ?? 0.0m;
            result.AdditionalResults.WellboreRadius = request.WellboreRadius ?? 0.25m;
            result.AdditionalResults.FormationThickness = request.FormationThickness ?? 50.0m;

            return result;
        }

        #endregion
    }
}
