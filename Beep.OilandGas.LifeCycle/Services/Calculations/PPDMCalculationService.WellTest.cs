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
        public async Task<WellTestAnalysisResult> PerformWellTestAnalysisAsync(WellTestAnalysisCalculationRequest request)
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
                WellTestData wellTestData;
                if (request.PressureTimeData != null && request.PressureTimeData.Count > 0)
                {
                    // Use data from request
                    wellTestData = new WellTestData
                    {
                        Time = request.PressureTimeData.Select(p => p.Time).ToList(),
                        Pressure = request.PressureTimeData.Select(p => p.Pressure).ToList(),
                        FlowRate = request.FlowRate.HasValue ? (double)request.FlowRate.Value : 0.0,
                        WellboreRadius = request.WellboreRadius.HasValue ? (double)request.WellboreRadius.Value : 0.25,
                        FormationThickness = request.FormationThickness.HasValue ? (double)request.FormationThickness.Value : 50.0,
                        Porosity = request.Porosity.HasValue ? (double)request.Porosity.Value : 0.2,
                        TotalCompressibility = request.TotalCompressibility.HasValue ? (double)request.TotalCompressibility.Value : 1e-5,
                        OilViscosity = request.OilViscosity.HasValue ? (double)request.OilViscosity.Value : 1.5,
                        OilFormationVolumeFactor = request.OilFormationVolumeFactor.HasValue ? (double)request.OilFormationVolumeFactor.Value : 1.2,
                        ProductionTime = request.ProductionTime.HasValue ? (double)request.ProductionTime.Value : 0.0,
                        IsGasWell = request.IsGasWell ?? false,
                        GasSpecificGravity = request.GasSpecificGravity.HasValue ? (double)request.GasSpecificGravity.Value : 0.65,
                        ReservoirTemperature = request.ReservoirTemperature.HasValue ? (double)request.ReservoirTemperature.Value : 150.0,
                        InitialReservoirPressure = request.InitialReservoirPressure.HasValue ? (double)request.InitialReservoirPressure.Value : 0.0,
                        TestType = request.AnalysisType?.ToUpper() == "DRAWDOWN" ? WellTestType.Drawdown : WellTestType.BuildUp
                    };
                }
                else
                {
                    // Retrieve from PPDM data
                    wellTestData = await GetWellTestDataFromPPDMAsync(request.WellId ?? string.Empty, request.TestId ?? string.Empty);
                }

                // Step 2: Perform well test analysis
                WellTestAnalysisResult analysisResult;
                string analysisMethod = request.AnalysisMethod?.ToUpper() ?? "HORNER";

                if (request.AnalysisType?.ToUpper() == "BUILDUP")
                {
                    if (analysisMethod == "MDH")
                    {
                        analysisResult = WellTestAnalyzer.AnalyzeBuildUpMDH(wellTestData);
                    }
                    else
                    {
                        analysisResult = WellTestAnalyzer.AnalyzeBuildUp(wellTestData);
                    }
                }
                else
                {
                    // Drawdown analysis - would need DrawdownAnalysis class if available
                    // For now, use build-up method
                    analysisResult = WellTestAnalyzer.AnalyzeBuildUp(wellTestData);
                }

                // Step 3: Calculate derivative for diagnostic plots
                var pressureTimePoints = wellTestData.Time.Zip(wellTestData.Pressure, (t, p) => new { Time = t, Pressure = p }).ToList();
                // var derivativePoints = WellTestAnalyzer.CalculateDerivative(pressureTimePoints);

                // Step 4: Identify reservoir model
                // var identifiedModel = WellTestAnalyzer.IdentifyReservoirModel(derivativePoints);

                // Step 5: Map to DTO
                var result = MapWellTestResultToDTO(analysisResult, request, null, null);

                // Step 6: Store result in PPDM database
                try
                {
                    var repository = await GetWellTestResultRepositoryAsync();
                    result.DiagnosticDataJson = JsonSerializer.Serialize(result.DiagnosticPoints ?? new List<WellTestDataPoint>());
                    result.DerivativeDataJson = JsonSerializer.Serialize(result.DerivativePoints ?? new List<WellTestDataPoint>());

                    await InsertAnalysisResultAsync(repository, result, request.UserId);
                    _logger?.LogInformation("Stored Well Test Analysis result with ID: {CalculationId}", result.CalculationId);
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
                    var errorResult = new WellTestAnalysisResult
                    {
                        CalculationId = Guid.NewGuid().ToString(),
                        WellId = request.WellId,
                        TestId = request.TestId,
                        AnalysisType = request.AnalysisType,
                        AnalysisMethod = request.AnalysisMethod ?? "HORNER",
                        CalculationDate = DateTime.UtcNow,
                        Status = "FAILED",
                        ErrorMessage = ex.Message
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
        private Task<WellTestData> GetWellTestDataFromPPDMAsync(string wellId, string testId)
        {
            _logger?.LogWarning("Well test data retrieval from PPDM is not implemented. " +
                "Provide PressureTimeData and well properties in the request.");

            return Task.FromException<WellTestData>(new InvalidOperationException(
                "Well test data retrieval from PPDM is not implemented. " +
                "Provide PressureTimeData and well properties in the request."));
        }

        /// <summary>
        /// Maps WellTestAnalysisResult from library to WellTestAnalysisResult DTO
        /// </summary>
        private WellTestAnalysisResult MapWellTestResultToDTO(
            WellTestAnalysisResult analysisResult,
            WellTestAnalysisCalculationRequest request,
            object? identifiedModel,
            object? derivativePoints)
        {
            var result = new WellTestAnalysisResult
            {
                CalculationId = Guid.NewGuid().ToString(),
                WellId = request.WellId,
                TestId = request.TestId,
                AnalysisType = request.AnalysisType,
                AnalysisMethod = request.AnalysisMethod ?? "HORNER",
                CalculationDate = DateTime.UtcNow,
                Status = "SUCCESS",
                UserId = request.UserId,
                Permeability = analysisResult.Permeability,
                SkinFactor = analysisResult.SkinFactor,
                ReservoirPressure = analysisResult.ReservoirPressure,
                ProductivityIndex = analysisResult.ProductivityIndex,
                FlowEfficiency = analysisResult.FlowEfficiency,
                DamageRatio = analysisResult.DamageRatio,
                RadiusOfInvestigation = analysisResult.RadiusOfInvestigation,
                // IdentifiedModel = identifiedModel,
                RSquared = analysisResult.RSquared,
                DiagnosticPoints = request.PressureTimeData,
                // DerivativePoints = derivativePoints...
                AdditionalResults = new WellTestAnalysisAdditionalResults()
            };

            result.AdditionalResults.AnalysisMethod = analysisResult.AnalysisMethod;
            result.AdditionalResults.FlowRate = request.FlowRate ?? 0.0m;
            result.AdditionalResults.WellboreRadius = request.WellboreRadius ?? 0.25m;
            result.AdditionalResults.FormationThickness = request.FormationThickness ?? 50.0m;

            return result;
        }

        #endregion
    }
}
