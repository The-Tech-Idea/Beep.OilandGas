using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.WellTestAnalysis;

namespace Beep.OilandGas.Client.App.Services.Analysis
{
    internal partial class AnalysisService
    {
        #region WellTest

        public async Task<WELL_TEST_ANALYSIS_RESULT> AnalyzeBuildUpAsync(WELL_TEST_DATA request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PostAsync<WellTestAnalysisCalculationRequest, WELL_TEST_ANALYSIS_RESULT>(
                    "/api/calculations/well-test",
                    MapWellTestCalculationRequest(request, WellTestAnalysisWellKnown.AnalysisClassification.BuildUp),
                    cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<WELL_TEST_ANALYSIS_RESULT> AnalyzeDrawdownAsync(WELL_TEST_DATA request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PostAsync<WellTestAnalysisCalculationRequest, WELL_TEST_ANALYSIS_RESULT>(
                    "/api/calculations/well-test",
                    MapWellTestCalculationRequest(request, WellTestAnalysisWellKnown.AnalysisClassification.DrawDown),
                    cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<WELL_TEST_ANALYSIS_RESULT> GetDerivativeAnalysisAsync(WELL_TEST_DATA request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PostAsync<WellTestAnalysisCalculationRequest, WELL_TEST_ANALYSIS_RESULT>(
                    "/api/calculations/well-test",
                    MapWellTestCalculationRequest(request, ResolveLegacyAnalysisType(request)),
                    cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<WELL_TEST_ANALYSIS_RESULT> InterpretWellTestAsync(WELL_TEST_DATA request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PostAsync<WellTestAnalysisCalculationRequest, WELL_TEST_ANALYSIS_RESULT>(
                    "/api/calculations/well-test",
                    MapWellTestCalculationRequest(request, ResolveLegacyAnalysisType(request)),
                    cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<List<WELL_TEST_ANALYSIS_RESULT>> GetWellTestAnalysisHistoryAsync(
            string wellId,
            string? testId = null,
            string? fieldId = null,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(wellId))
                throw new ArgumentException("Well ID is required.", nameof(wellId));
            if (AccessMode == ServiceAccessMode.Remote)
            {
                var query = $"wellId={Uri.EscapeDataString(wellId.Trim())}";
                if (!string.IsNullOrWhiteSpace(testId))
                    query += $"&testId={Uri.EscapeDataString(testId.Trim())}";
                if (!string.IsNullOrWhiteSpace(fieldId))
                    query += $"&fieldId={Uri.EscapeDataString(fieldId.Trim())}";
                return await GetAsync<List<WELL_TEST_ANALYSIS_RESULT>>($"/api/calculations/well-test?{query}", cancellationToken);
            }

            throw new InvalidOperationException("Local mode not yet implemented");
        }

        #endregion

        /// <summary>
        /// Maps in-memory <see cref="WELL_TEST_DATA"/> to the calculations API contract.
        /// <see cref="WellTestAnalysisCalculationRequest.WellId"/> must be the PPDM well UWI — set <see cref="WELL_TEST_DATA.CalculationWellUwi"/> (preferred) or legacy <see cref="WELL_TEST_DATA.AREA_ID"/> only if it truly holds a UWI; do not confuse with PPDM <c>WELL.AREA_ID</c> (references <c>AREA</c>).
        /// For PPDM stored pressure, supply either manual <see cref="BuildPressureTimeData"/> points or <see cref="WELL_TEST_DATA.CalculationPpdmTestNumber"/> (PPDM <c>TEST_NUM</c>); never use <see cref="WELL_TEST_DATA.WELL_TEST_DATA_ID"/> as <c>TestId</c>.
        /// Optional <see cref="WELL_TEST_DATA.CalculationFieldId"/> maps to <see cref="WellTestAnalysisCalculationRequest.FieldId"/> (e.g. <c>WELL.ASSIGNED_FIELD</c> from <c>WellTestAnalysisMapper</c>).
        /// </summary>
        private static WellTestAnalysisCalculationRequest MapWellTestCalculationRequest(WELL_TEST_DATA request, string analysisType)
        {
            var wellId = ResolveCalculationWellId(request);
            if (string.IsNullOrWhiteSpace(wellId))
            {
                throw new ArgumentException(
                    "Well UWI is required. Set WELL_TEST_DATA.CalculationWellUwi (or legacy AREA_ID carrying the UWI) before calling remote well test analysis.",
                    nameof(request));
            }

            var pressureTimeData = BuildPressureTimeData(request);
            if (pressureTimeData is { Count: > 0 and < 3 })
            {
                throw new ArgumentException(
                    "At least three paired Time/Pressure values are required on WELL_TEST_DATA for manual well test analysis.",
                    nameof(request));
            }

            var testId = ResolveCalculationTestId(request, pressureTimeData);
            if (pressureTimeData == null && string.IsNullOrWhiteSpace(testId))
            {
                throw new ArgumentException(
                    "Provide at least three paired Time/Pressure values on WELL_TEST_DATA, or set CalculationPpdmTestNumber (PPDM TEST_NUM) so the API can load WELL_TEST_PRESS_MEAS.",
                    nameof(request));
            }

            return new WellTestAnalysisCalculationRequest
            {
                WellId = wellId,
                TestId = testId,
                AnalysisType = analysisType,
                AnalysisMethod = ResolveAnalysisMethodForLegacyRequest(request, analysisType),
                PressureTimeData = pressureTimeData,
                FlowRate = request.FLOW_RATE,
                WellboreRadius = request.WELLBORE_RADIUS,
                FormationThickness = request.FORMATION_THICKNESS,
                Porosity = request.POROSITY,
                TotalCompressibility = request.TOTAL_COMPRESSIBILITY,
                OilViscosity = request.OIL_VISCOSITY,
                OilFormationVolumeFactor = request.OIL_FORMATION_VOLUME_FACTOR,
                ProductionTime = request.PRODUCTION_TIME,
                IsGasWell = request.IS_GAS_WELL,
                GasSpecificGravity = request.GAS_SPECIFIC_GRAVITY,
                ReservoirTemperature = request.RESERVOIR_TEMPERATURE,
                InitialReservoirPressure = request.INITIAL_RESERVOIR_PRESSURE,
                FieldId = string.IsNullOrWhiteSpace(request.CalculationFieldId) ? null : request.CalculationFieldId.Trim()
            };
        }

        private static string? ResolveCalculationWellId(WELL_TEST_DATA request)
        {
            if (!string.IsNullOrWhiteSpace(request.CalculationWellUwi))
                return request.CalculationWellUwi.Trim();
            // Legacy callers that stuffed UWI into AREA_ID — PPDM WELL.AREA_ID is different (AREA table); prefer CalculationWellUwi.
            if (!string.IsNullOrWhiteSpace(request.AREA_ID))
                return request.AREA_ID.Trim();
            return string.IsNullOrWhiteSpace(request.BUSINESS_ASSOCIATE_ID) ? null : request.BUSINESS_ASSOCIATE_ID.Trim();
        }

        private static string? ResolveCalculationTestId(WELL_TEST_DATA request, List<WellTestDataPoint>? pressureTimeData)
        {
            if (pressureTimeData != null && pressureTimeData.Count > 0)
                return null;
            if (!string.IsNullOrWhiteSpace(request.CalculationPpdmTestNumber))
                return request.CalculationPpdmTestNumber.Trim();
            return null;
        }

        private static string ResolveAnalysisMethodForLegacyRequest(WELL_TEST_DATA request, string analysisType)
        {
            if (request.IS_GAS_WELL is true)
                return WellTestAnalysisWellKnown.AnalysisMethod.Horner;
            if (WellTestAnalysisWellKnown.ClassificationEqualsDrawDown(analysisType))
                return WellTestAnalysisWellKnown.AnalysisMethod.Horner;
            if (!string.IsNullOrWhiteSpace(request.TEST_TYPE)
                && request.TEST_TYPE.IndexOf(WellTestAnalysisWellKnown.TestTypeToken.Mdh, StringComparison.OrdinalIgnoreCase) >= 0)
                return WellTestAnalysisWellKnown.AnalysisMethod.Mdh;
            return WellTestAnalysisWellKnown.AnalysisMethod.Horner;
        }

        private static List<WellTestDataPoint>? BuildPressureTimeData(WELL_TEST_DATA request)
        {
            if (request.Time == null || request.Pressure == null || request.Time.Count == 0 || request.Time.Count != request.Pressure.Count)
            {
                return null;
            }

            var points = new List<WellTestDataPoint>(request.Time.Count);
            for (var index = 0; index < request.Time.Count; index++)
            {
                points.Add(new WellTestDataPoint
                {
                    Time = (decimal)request.Time[index],
                    Pressure = (decimal)request.Pressure[index]
                });
            }

            return points;
        }

        private static string ResolveLegacyAnalysisType(WELL_TEST_DATA request)
        {
            return request.TEST_TYPE is { } tt && tt.IndexOf(WellTestAnalysisWellKnown.TestTypeToken.Draw, StringComparison.OrdinalIgnoreCase) >= 0
                ? WellTestAnalysisWellKnown.AnalysisClassification.DrawDown
                : WellTestAnalysisWellKnown.AnalysisClassification.BuildUp;
        }
    }
}
