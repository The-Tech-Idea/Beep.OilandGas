using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.ProspectIdentification;

namespace Beep.OilandGas.Client.App.Services.LifeCycle
{
    internal partial class LifeCycleService
    {
        #region Exploration

        public async Task<EXPLORATION_PROGRAM> CreateExplorationProjectAsync(EXPLORATION_PROGRAM request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PostAsync<EXPLORATION_PROGRAM, EXPLORATION_PROGRAM>("/api/lifecycle/exploration/create", request, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<List<PROSPECT>> GetProspectsAsync(string areaId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(areaId)) throw new ArgumentNullException(nameof(areaId));
            if (AccessMode == ServiceAccessMode.Remote)
                return await GetAsync<List<PROSPECT>>($"/api/lifecycle/exploration/prospects/{Uri.EscapeDataString(areaId)}", cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<PROSPECT_SEIS_SURVEY> AnalyzeSeismicAsync(PROSPECT_SEIS_SURVEY request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PostAsync<PROSPECT_SEIS_SURVEY, PROSPECT_SEIS_SURVEY>("/api/lifecycle/exploration/seismic", request, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<EXPLORATION_PROGRAM> GetExplorationStatusAsync(string projectId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(projectId)) throw new ArgumentNullException(nameof(projectId));
            if (AccessMode == ServiceAccessMode.Remote)
                return await GetAsync<EXPLORATION_PROGRAM>($"/api/lifecycle/exploration/{Uri.EscapeDataString(projectId)}/status", cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<EXPLORATION_PROGRAM> SaveExplorationResultAsync(EXPLORATION_PROGRAM result, string? userId = null, CancellationToken cancellationToken = default)
        {
            if (result == null) throw new ArgumentNullException(nameof(result));
            if (AccessMode == ServiceAccessMode.Remote)
            {
                var queryParams = new Dictionary<string, string>();
                if (!string.IsNullOrEmpty(userId)) queryParams["userId"] = userId;
                var endpoint = BuildRequestUriWithParams("/api/lifecycle/exploration/result", queryParams);
                return await PostAsync<EXPLORATION_PROGRAM, EXPLORATION_PROGRAM>(endpoint, result, cancellationToken);
            }
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        #endregion
    }
}
