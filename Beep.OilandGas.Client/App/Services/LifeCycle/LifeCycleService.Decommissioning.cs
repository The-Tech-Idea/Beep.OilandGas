using System;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.Decommissioning;

namespace Beep.OilandGas.Client.App.Services.LifeCycle
{
    internal partial class LifeCycleService
    {
        #region Decommissioning

        public async Task<DECOMMISSIONING_STATUS> CreateDecommissioningPlanAsync(DECOMMISSIONING_STATUS request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PostAsync<DECOMMISSIONING_STATUS, DECOMMISSIONING_STATUS>("/api/lifecycle/decommissioning/create", request, null, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<ABANDONMENT_STATUS> GetPluggingStatusAsync(string wellId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(wellId)) throw new ArgumentNullException(nameof(wellId));
            if (AccessMode == ServiceAccessMode.Remote)
                return await GetAsync<ABANDONMENT_STATUS>($"/api/lifecycle/decommissioning/plugging/{Uri.EscapeDataString(wellId)}", null, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<DECOMMISSIONING_STATUS> GetEnvironmentalAssessmentAsync(string assetId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(assetId)) throw new ArgumentNullException(nameof(assetId));
            if (AccessMode == ServiceAccessMode.Remote)
                return await GetAsync<DECOMMISSIONING_STATUS>($"/api/lifecycle/decommissioning/environmental/{Uri.EscapeDataString(assetId)}", null, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<DECOMMISSIONING_STATUS> UpdateDecommissioningStatusAsync(string planId, DECOMMISSIONING_STATUS status, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(planId)) throw new ArgumentNullException(nameof(planId));
            if (status == null) throw new ArgumentNullException(nameof(status));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PutAsync<DECOMMISSIONING_STATUS, DECOMMISSIONING_STATUS>($"/api/lifecycle/decommissioning/{Uri.EscapeDataString(planId)}/status", status, null, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<DECOMMISSIONING_STATUS> GetDecommissioningCostEstimateAsync(string assetId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(assetId)) throw new ArgumentNullException(nameof(assetId));
            if (AccessMode == ServiceAccessMode.Remote)
                return await GetAsync<DECOMMISSIONING_STATUS>($"/api/lifecycle/decommissioning/cost/{Uri.EscapeDataString(assetId)}", null, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        #endregion
    }
}
