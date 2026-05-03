using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.Calculations;
using Beep.OilandGas.Models.Data.NodalAnalysis;
using Beep.OilandGas.Models.Data.ProductionForecasting;

namespace Beep.OilandGas.Client.App.Services.Calculations
{
    internal partial class CalculationsService
    {
        #region Nodal Analysis

        public async Task<NodalAnalysisRunResult> PerformNodalAnalysisAsync(
            string wellUWI,
            NodalAnalysisParameters analysisParameters,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(wellUWI))
                throw new ArgumentException("Well UWI is required", nameof(wellUWI));
            if (analysisParameters == null)
                throw new ArgumentNullException(nameof(analysisParameters));
            if (AccessMode == ServiceAccessMode.Remote)
            {
                var request = new PerformNodalAnalysisRequest
                {
                    WellUWI = wellUWI,
                    AnalysisParameters = analysisParameters
                };
                var result = await PostAsync<PerformNodalAnalysisRequest, NodalAnalysisRunResult>(
                    NodalAnalysisHttpRoutes.Analyze, request, cancellationToken);
                return result ?? throw new InvalidOperationException("Failed to perform nodal analysis");
            }

            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<OptimizationResult> OptimizeSystemAsync(
            string wellUWI,
            OptimizationGoals optimizationGoals,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(wellUWI))
                throw new ArgumentException("Well UWI is required", nameof(wellUWI));
            if (optimizationGoals == null)
                throw new ArgumentNullException(nameof(optimizationGoals));
            if (AccessMode == ServiceAccessMode.Remote)
            {
                var request = new OptimizeSystemRequest
                {
                    WellUWI = wellUWI,
                    OptimizationGoals = optimizationGoals
                };
                var result = await PostAsync<OptimizeSystemRequest, OptimizationResult>(
                    NodalAnalysisHttpRoutes.Optimize, request, cancellationToken);
                return result ?? throw new InvalidOperationException("Failed to optimize nodal system");
            }

            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<bool> SaveNodalAnalysisResultAsync(
            NodalAnalysisRunResult result,
            string? userId = null,
            CancellationToken cancellationToken = default)
        {
            if (result == null)
                throw new ArgumentNullException(nameof(result));
            if (AccessMode == ServiceAccessMode.Remote)
            {
                var queryParams = new Dictionary<string, string>();
                if (!string.IsNullOrEmpty(userId))
                    queryParams["userId"] = userId;
                var endpoint = BuildRequestUriWithParams(NodalAnalysisHttpRoutes.Result, queryParams);
                var response = await PostAsync<NodalAnalysisRunResult, NodalAnalysisSaveApiResponse>(
                    endpoint, result, cancellationToken);
                return response != null;
            }

            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<List<NodalAnalysisRunResult>> GetNodalAnalysisHistoryAsync(
            string wellId,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(wellId))
                throw new ArgumentException("Well UWI is required", nameof(wellId));
            if (AccessMode == ServiceAccessMode.Remote)
            {
                var list = await GetAsync<List<NodalAnalysisRunResult>>(
                    NodalAnalysisHttpRoutes.History(wellId), cancellationToken);
                return list ?? new List<NodalAnalysisRunResult>();
            }

            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<PerformanceMatchingAnalysis> AnalyzePerformanceMatchingAsync(
            PerformNodalAnalysisRequest request,
            CancellationToken cancellationToken = default)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            if (AccessMode == ServiceAccessMode.Remote)
            {
                var result = await PostAsync<PerformNodalAnalysisRequest, PerformanceMatchingAnalysis>(
                    NodalAnalysisHttpRoutes.PerformanceMatching, request, cancellationToken);
                return result ?? throw new InvalidOperationException("Failed to run performance matching");
            }

            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<EconomicSensitivityAnalysisResult> PerformSensitivityAnalysisAsync(
            NodalSensitivityAnalysisRequest request,
            CancellationToken cancellationToken = default)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            if (AccessMode == ServiceAccessMode.Remote)
            {
                var result = await PostAsync<NodalSensitivityAnalysisRequest, EconomicSensitivityAnalysisResult>(
                    NodalAnalysisHttpRoutes.Sensitivity, request, cancellationToken);
                return result ?? throw new InvalidOperationException("Failed to run sensitivity analysis");
            }

            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<ArtificialLiftRecommendation> RecommendArtificialLiftAsync(
            NodalArtificialLiftRequest request,
            CancellationToken cancellationToken = default)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            if (AccessMode == ServiceAccessMode.Remote)
            {
                var result = await PostAsync<NodalArtificialLiftRequest, ArtificialLiftRecommendation>(
                    NodalAnalysisHttpRoutes.ArtificialLift, request, cancellationToken);
                return result ?? throw new InvalidOperationException("Failed to get artificial lift recommendation");
            }

            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<WellDiagnosticsResult> DiagnoseWellPerformanceAsync(
            NodalWellDiagnosticsRequest request,
            CancellationToken cancellationToken = default)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            if (AccessMode == ServiceAccessMode.Remote)
            {
                var result = await PostAsync<NodalWellDiagnosticsRequest, WellDiagnosticsResult>(
                    NodalAnalysisHttpRoutes.Diagnostics, request, cancellationToken);
                return result ?? throw new InvalidOperationException("Failed to diagnose well performance");
            }

            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<PRODUCTION_FORECAST> ForecastNodalProductionAsync(
            NodalProductionForecastRequest request,
            CancellationToken cancellationToken = default)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            if (AccessMode == ServiceAccessMode.Remote)
            {
                var result = await PostAsync<NodalProductionForecastRequest, PRODUCTION_FORECAST>(
                    NodalAnalysisHttpRoutes.ProductionForecast, request, cancellationToken);
                return result ?? throw new InvalidOperationException("Failed to forecast production");
            }

            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<PressureMaintenanceStrategy> AnalyzePressureMaintenanceAsync(
            NodalPressureMaintenanceRequest request,
            CancellationToken cancellationToken = default)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            if (AccessMode == ServiceAccessMode.Remote)
            {
                var result = await PostAsync<NodalPressureMaintenanceRequest, PressureMaintenanceStrategy>(
                    NodalAnalysisHttpRoutes.PressureMaintenance, request, cancellationToken);
                return result ?? throw new InvalidOperationException("Failed to analyze pressure maintenance");
            }

            throw new InvalidOperationException("Local mode not yet implemented");
        }

        #endregion
    }
}
