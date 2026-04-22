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
                    MapWellTestCalculationRequest(request, "BUILDUP"),
                    cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<WELL_TEST_ANALYSIS_RESULT> AnalyzeDrawdownAsync(WELL_TEST_DATA request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PostAsync<WellTestAnalysisCalculationRequest, WELL_TEST_ANALYSIS_RESULT>(
                    "/api/calculations/well-test",
                    MapWellTestCalculationRequest(request, "DRAWDOWN"),
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

        public async Task<List<WELL_TEST_DATA>> GetWellTestHistoryAsync(string wellId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(wellId)) throw new ArgumentNullException(nameof(wellId));
            if (AccessMode == ServiceAccessMode.Remote)
                return await GetAsync<List<WELL_TEST_DATA>>($"/api/welltest/{wellId}/history", cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        #endregion

        private static WellTestAnalysisCalculationRequest MapWellTestCalculationRequest(WELL_TEST_DATA request, string analysisType)
        {
            return new WellTestAnalysisCalculationRequest
            {
                WellId = request.AREA_ID,
                TestId = request.WELL_TEST_DATA_ID,
                AnalysisType = analysisType,
                AnalysisMethod = "HORNER",
                PressureTimeData = BuildPressureTimeData(request),
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
                InitialReservoirPressure = request.INITIAL_RESERVOIR_PRESSURE
            };
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
            return request.TEST_TYPE?.IndexOf("DRAW", StringComparison.OrdinalIgnoreCase) >= 0
                ? "DRAWDOWN"
                : "BUILDUP";
        }
    }
}
