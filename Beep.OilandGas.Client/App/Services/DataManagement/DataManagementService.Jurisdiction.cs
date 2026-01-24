using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Beep.OilandGas.Client.App.Services.DataManagement
{
    internal partial class DataManagementService
    {
        #region Jurisdiction

        public async Task<List<object>> GetJurisdictionsAsync(CancellationToken cancellationToken = default)
        {
            if (AccessMode == ServiceAccessMode.Remote)
                return await GetAsync<List<object>>("/api/ppdm39jurisdiction", cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<object> GetJurisdictionAsync(string jurisdictionId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(jurisdictionId)) throw new ArgumentException("Jurisdiction ID is required", nameof(jurisdictionId));
            if (AccessMode == ServiceAccessMode.Remote)
                return await GetAsync<object>($"/api/ppdm39jurisdiction/{Uri.EscapeDataString(jurisdictionId)}", cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<object> GetJurisdictionRequirementsAsync(string jurisdictionId, string entityType, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(jurisdictionId)) throw new ArgumentException("Jurisdiction ID is required", nameof(jurisdictionId));
            if (string.IsNullOrEmpty(entityType)) throw new ArgumentException("Entity type is required", nameof(entityType));
            if (AccessMode == ServiceAccessMode.Remote)
                return await GetAsync<object>($"/api/ppdm39jurisdiction/{Uri.EscapeDataString(jurisdictionId)}/requirements/{Uri.EscapeDataString(entityType)}", cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        #endregion
    }
}

