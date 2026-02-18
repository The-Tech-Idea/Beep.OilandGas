using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.NodalAnalysis;

namespace Beep.OilandGas.Client.App.Services.Calculations
{
    internal partial class CalculationsService
    {
        #region Nodal Analysis

        public async Task<OperatingPoint> PerformNodalAnalysisAsync(ReservoirProperties request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PostAsync<ReservoirProperties, OperatingPoint>("/api/nodalanalysis/analyze", request, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<NODAL_ANALYSIS_RESULT> OptimizeNodalAnalysisAsync(NODAL_RESERVOIR_PROPERTIES request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PostAsync<NODAL_RESERVOIR_PROPERTIES, NODAL_ANALYSIS_RESULT>("/api/nodalanalysis/optimize", request, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<NODAL_ANALYSIS_RESULT> SaveNodalAnalysisResultAsync(NODAL_ANALYSIS_RESULT result, string? userId = null, CancellationToken cancellationToken = default)
        {
            if (result == null) throw new ArgumentNullException(nameof(result));
            if (AccessMode == ServiceAccessMode.Remote)
            {
                var queryParams = new Dictionary<string, string>();
                if (!string.IsNullOrEmpty(userId)) queryParams["userId"] = userId;
                var endpoint = BuildRequestUriWithParams("/api/nodalanalysis/result", queryParams);
                return await PostAsync<NODAL_ANALYSIS_RESULT, NODAL_ANALYSIS_RESULT>(endpoint, result, cancellationToken);
            }
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<List<NODAL_ANALYSIS_RESULT>> GetNodalAnalysisHistoryAsync(string wellId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(wellId)) throw new ArgumentException("Well ID is required", nameof(wellId));
            if (AccessMode == ServiceAccessMode.Remote)
                return await GetAsync<List<NODAL_ANALYSIS_RESULT>>($"/api/nodalanalysis/history/{Uri.EscapeDataString(wellId)}", cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        #endregion
    }
}
