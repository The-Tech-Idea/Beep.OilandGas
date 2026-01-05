using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.LifeCycle;

namespace Beep.OilandGas.Client.App.Services.LifeCycle
{
    internal partial class LifeCycleService
    {
        #region WellManagement

        public async Task<FIELD_PHASE> GetWellStatusAsync(string wellId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(wellId)) throw new ArgumentNullException(nameof(wellId));
            if (AccessMode == ServiceAccessMode.Remote)
                return await GetAsync<FIELD_PHASE>($"/api/lifecycle/well/{Uri.EscapeDataString(wellId)}/status", null, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<FIELD_PHASE> UpdateWellStatusAsync(string wellId, FIELD_PHASE status, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(wellId)) throw new ArgumentNullException(nameof(wellId));
            if (status == null) throw new ArgumentNullException(nameof(status));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PutAsync<FIELD_PHASE, FIELD_PHASE>($"/api/lifecycle/well/{Uri.EscapeDataString(wellId)}/status", status, null, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<List<FIELD_PHASE>> GetWellHistoryAsync(string wellId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(wellId)) throw new ArgumentNullException(nameof(wellId));
            if (AccessMode == ServiceAccessMode.Remote)
                return await GetAsync<List<FIELD_PHASE>>($"/api/lifecycle/well/{Uri.EscapeDataString(wellId)}/history", null, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<FIELD_PHASE> GetWellIntegrityAsync(string wellId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(wellId)) throw new ArgumentNullException(nameof(wellId));
            if (AccessMode == ServiceAccessMode.Remote)
                return await GetAsync<FIELD_PHASE>($"/api/lifecycle/well/{Uri.EscapeDataString(wellId)}/integrity", null, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<FIELD_PHASE> ScheduleWellInterventionAsync(FIELD_PHASE request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PostAsync<FIELD_PHASE, FIELD_PHASE>("/api/lifecycle/well/intervention", request, null, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        #endregion
    }
}
