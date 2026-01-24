using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.Drilling;

namespace Beep.OilandGas.Client.App.Services.Drilling
{
    internal partial class DrillingService
    {
        #region Operations

        public async Task<DRILLING_OPERATION> CreateDrillingProgramAsync(DRILLING_OPERATION request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PostAsync<DRILLING_OPERATION, DRILLING_OPERATION>("/api/drilling/program/create", request, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<DRILLING_OPERATION> GetDrillingStatusAsync(string wellId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(wellId)) throw new ArgumentNullException(nameof(wellId));
            if (AccessMode == ServiceAccessMode.Remote)
                return await GetAsync<DRILLING_OPERATION>($"/api/drilling/{Uri.EscapeDataString(wellId)}/status", cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<WELL_CONSTRUCTION> GetBHADesignAsync(string wellId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(wellId)) throw new ArgumentNullException(nameof(wellId));
            if (AccessMode == ServiceAccessMode.Remote)
                return await GetAsync<WELL_CONSTRUCTION>($"/api/drilling/{Uri.EscapeDataString(wellId)}/bha", cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<DRILLING_REPORT> GetMudProgramAsync(string wellId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(wellId)) throw new ArgumentNullException(nameof(wellId));
            if (AccessMode == ServiceAccessMode.Remote)
                return await GetAsync<DRILLING_REPORT>($"/api/drilling/{Uri.EscapeDataString(wellId)}/mud", cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<DRILLING_OPERATION> UpdateDrillingProgressAsync(string wellId, DRILLING_REPORT progress, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(wellId)) throw new ArgumentNullException(nameof(wellId));
            if (progress == null) throw new ArgumentNullException(nameof(progress));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PutAsync<DRILLING_REPORT, DRILLING_OPERATION>($"/api/drilling/{Uri.EscapeDataString(wellId)}/progress", progress, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<List<DRILLING_REPORT>> GetDrillingHistoryAsync(string wellId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(wellId)) throw new ArgumentNullException(nameof(wellId));
            if (AccessMode == ServiceAccessMode.Remote)
                return await GetAsync<List<DRILLING_REPORT>>($"/api/drilling/{Uri.EscapeDataString(wellId)}/history", cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<List<CASING_STRING>> GetCasingDesignAsync(string wellId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(wellId)) throw new ArgumentNullException(nameof(wellId));
            if (AccessMode == ServiceAccessMode.Remote)
                return await GetAsync<List<CASING_STRING>>($"/api/drilling/{Uri.EscapeDataString(wellId)}/casing", cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<List<COMPLETION_STRING>> GetCementingProgramAsync(string wellId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(wellId)) throw new ArgumentNullException(nameof(wellId));
            if (AccessMode == ServiceAccessMode.Remote)
                return await GetAsync<List<COMPLETION_STRING>>($"/api/drilling/{Uri.EscapeDataString(wellId)}/cementing", cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        #endregion
    }
}
