using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.LifeCycle;

namespace Beep.OilandGas.Client.App.Services.LifeCycle
{
    internal partial class LifeCycleService
    {
        #region FacilityManagement

        public async Task<List<FIELD_PHASE>> GetFacilitiesAsync(string fieldId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(fieldId)) throw new ArgumentNullException(nameof(fieldId));
            if (AccessMode == ServiceAccessMode.Remote)
                return await GetAsync<List<FIELD_PHASE>>($"/api/lifecycle/facility/field/{Uri.EscapeDataString(fieldId)}", cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<FIELD_PHASE> CreateFacilityAsync(FIELD_PHASE request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PostAsync<FIELD_PHASE, FIELD_PHASE>("/api/lifecycle/facility/create", request, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<FIELD_PHASE> GetFacilityStatusAsync(string facilityId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(facilityId)) throw new ArgumentNullException(nameof(facilityId));
            if (AccessMode == ServiceAccessMode.Remote)
                return await GetAsync<FIELD_PHASE>($"/api/lifecycle/facility/{Uri.EscapeDataString(facilityId)}/status", cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<FIELD_PHASE> UpdateFacilityAsync(string facilityId, FIELD_PHASE request, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(facilityId)) throw new ArgumentNullException(nameof(facilityId));
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PutAsync<FIELD_PHASE, FIELD_PHASE>($"/api/lifecycle/facility/{Uri.EscapeDataString(facilityId)}", request, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<FIELD_PHASE> GetFacilityCapacityAsync(string facilityId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(facilityId)) throw new ArgumentNullException(nameof(facilityId));
            if (AccessMode == ServiceAccessMode.Remote)
                return await GetAsync<FIELD_PHASE>($"/api/lifecycle/facility/{Uri.EscapeDataString(facilityId)}/capacity", cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        #endregion
    }
}
