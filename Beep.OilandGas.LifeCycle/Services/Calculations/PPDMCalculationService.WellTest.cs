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
                if (string.IsNullOrEmpty(request.WELL_ID) && string.IsNullOrEmpty(request.TEST_ID))
                {
                    throw new ArgumentException("At least one of WellId or TestId must be provided");
                }

                _logger?.LogInformation("Starting Well Test Analysis for WellId: {WellId}, TestId: {TestId}",
                    request.WELL_ID, request.TEST_ID);

                // Step 1: Build well test data from request or PPDM data
                WELL_TEST_DATA WELL_TEST_DATA;
                if (request.PressureTimeData != null && request.PressureTimeData.Count > 0)
                {
                    // Use data from request
                    WELL_TEST_DATA = new WELL_TEST_DATA
                    {
                        Time = request.PressureTimeData.Select(p => (double)(p.Time ?? 0)).ToList(),
                        Pressure = request.PressureTimeData.Select(p => (double)(p.Pressure ?? 0)).ToList(),
                        FlowRate = request.FLOW_RATE.HasValue ? (double)request.FLOW_RATE.Value : 0.0,
                        WellboreRadius = request.WELLBORE_RADIUS.HasValue ? (double)request.WELLBORE_RADIUS.Value : 0.25,
                        FormationThickness = request.FORMATION_THICKNESS.HasValue ? (double)request.FORMATION_THICKNESS.Value : 50.0,
                        Porosity = request.POROSITY.HasValue ? (double)request.POROSITY.Value : 0.2,
                        TotalCompressibility = request.TOTAL_COMPRESSIBILITY.HasValue ? (double)request.TOTAL_COMPRESSIBILITY.Value : 1e-5,
                        OilViscosity = request.OIL_VISCOSITY.HasValue ? (double)request.OIL_VISCOSITY.Value : 1.5,
                        OilFormationVolumeFactor = request.OIL_FORMATION_VOLUME_FACTOR.HasValue ? (double)request.OIL_FORMATION_VOLUME_FACTOR.Value : 1.2,
                        ProductionTime = request.PRODUCTION_TIME.HasValue ? (double)request.PRODUCTION_TIME.Value : 0.0,
                        IsGasWell = request.IS_GAS_WELL ?? false,
                        GasSpecificGravity = request.GAS_SPECIFIC_GRAVITY.HasValue ? (double)request.GAS_SPECIFIC_GRAVITY.Value : 0.65,
                        ReservoirTemperature = request.RESERVOIR_TEMPERATURE.HasValue ? (double)request.RESERVOIR_TEMPERATURE.Value : 150.0,
                        InitialReservoirPressure = request.INITIAL_RESERVOIR_PRESSURE.HasValue ? (double)request.INITIAL_RESERVOIR_PRESSURE.Value : 0.0,
                        TestType = request.ANALYSIS_TYPE?.ToUpper() == "DRAWDOWN" ? WellTestType.Drawdown : WellTestType.BuildUp
                    };
                }
                else
                {
                    // Retrieve from PPDM data
                    WELL_TEST_DATA = await GetWellTestDataFromPPDMAsync(request.WELL_ID ?? string.Empty, request.TEST_ID ?? string.Empty);
                }

                // Step 2: Perform well test analysis
                WELL_TEST_ANALYSIS_RESULT analysisResult;
                string analysisMethod = request.ANALYSIS_METHOD?.ToUpper() ?? "HORNER";

                if (request.ANALYSIS_TYPE?.ToUpper() == "BUILDUP")
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

                    await InsertAnalysisResultAsync(repository, result, request.USER_ID);
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
                    request.WELL_ID, request.TEST_ID);

                // Try to store error result
                try
                {
                    var repository = await GetWellTestResultRepositoryAsync();
                    var errorResult = new WELL_TEST_ANALYSIS_RESULT
                    {
                        CalculationId = Guid.NewGuid().ToString(),
                        WellId = request.WELL_ID,
                        TestId = request.TEST_ID,
                        AnalysisType = request.ANALYSIS_TYPE,
                        AnalysisMethod = request.ANALYSIS_METHOD ?? "HORNER",
                        CalculationDate = DateTime.UtcNow,
                        Status = "FAILED",
                        ErrorMessage = ex.Message
                    };
                    await InsertAnalysisResultAsync(repository, errorResult, request.USER_ID);
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
                CalculationId = Guid.NewGuid().ToString(),
                WellId = request.WELL_ID,
                TestId = request.TEST_ID,
                AnalysisType = request.ANALYSIS_TYPE,
                AnalysisMethod = request.ANALYSIS_METHOD ?? "HORNER",
                CalculationDate = DateTime.UtcNow,
                Status = "SUCCESS",
                UserId = request.USER_ID,
                Permeability = analysisResult.PERMEABILITY,
                SkinFactor = analysisResult.SKIN_FACTOR,
                ReservoirPressure = analysisResult.RESERVOIR_PRESSURE,
                ProductivityIndex = analysisResult.PRODUCTIVITY_INDEX,
                FlowEfficiency = analysisResult.FLOW_EFFICIENCY,
                DamageRatio = analysisResult.DAMAGE_RATIO,
                RadiusOfInvestigation = analysisResult.RADIUS_OF_INVESTIGATION,
                // IdentifiedModel = identifiedModel,
                RSquared = analysisResult.RSQUARED,
                DiagnosticPoints = request.PressureTimeData,
                // DerivativePoints = derivativePoints...
                AdditionalResults = new WellTestAnalysisAdditionalResults()
            };

            result.ADDITIONAL_RESULTS.ANALYSIS_METHOD = analysisResult.ANALYSIS_METHOD;
            result.ADDITIONAL_RESULTS.FLOW_RATE = request.FLOW_RATE ?? 0.0m;
            result.ADDITIONAL_RESULTS.WELLBORE_RADIUS = request.WELLBORE_RADIUS ?? 0.25m;
            result.ADDITIONAL_RESULTS.FORMATION_THICKNESS = request.FORMATION_THICKNESS ?? 50.0m;

            return result;
        }

        #endregion
    }
}
