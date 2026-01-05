using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Beep.OilandGas.Client.App.Services.Permits
{
    /// <summary>
    /// Unified service for Permits and Applications operations
    /// </summary>
    internal class PermitsService : ServiceBase, IPermitsService
    {
        public PermitsService(BeepOilandGasApp app, ILogger<PermitsService>? logger = null)
            : base(app, logger)
        {
        }

        public async Task<object> CreatePermitApplicationAsync(object request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PostAsync<object, object>("/api/permits/application", request, null, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<object> GetPermitStatusAsync(string permitId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(permitId)) throw new ArgumentException("Permit ID is required", nameof(permitId));
            if (AccessMode == ServiceAccessMode.Remote)
                return await GetAsync<object>($"/api/permits/{Uri.EscapeDataString(permitId)}/status", null, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<List<object>> GetRequiredDocumentsAsync(string permitType, string jurisdiction, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(permitType)) throw new ArgumentException("Permit type is required", nameof(permitType));
            if (string.IsNullOrEmpty(jurisdiction)) throw new ArgumentException("Jurisdiction is required", nameof(jurisdiction));
            if (AccessMode == ServiceAccessMode.Remote)
                return await GetAsync<List<object>>($"/api/permits/documents/{Uri.EscapeDataString(permitType)}/{Uri.EscapeDataString(jurisdiction)}", null, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<object> SubmitPermitAsync(string permitId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(permitId)) throw new ArgumentException("Permit ID is required", nameof(permitId));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PostAsync<object, object>($"/api/permits/{Uri.EscapeDataString(permitId)}/submit", new { }, null, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<object> UpdatePermitApplicationAsync(string permitId, object request, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(permitId)) throw new ArgumentException("Permit ID is required", nameof(permitId));
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PutAsync<object, object>($"/api/permits/{Uri.EscapeDataString(permitId)}", request, null, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<List<object>> GetPermitHistoryAsync(string assetId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(assetId)) throw new ArgumentException("Asset ID is required", nameof(assetId));
            if (AccessMode == ServiceAccessMode.Remote)
                return await GetAsync<List<object>>($"/api/permits/asset/{Uri.EscapeDataString(assetId)}/history", null, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<object> GetPermitComplianceAsync(string permitId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(permitId)) throw new ArgumentException("Permit ID is required", nameof(permitId));
            if (AccessMode == ServiceAccessMode.Remote)
                return await GetAsync<object>($"/api/permits/{Uri.EscapeDataString(permitId)}/compliance", null, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<object> RenewPermitAsync(string permitId, object request, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(permitId)) throw new ArgumentException("Permit ID is required", nameof(permitId));
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PostAsync<object, object>($"/api/permits/{Uri.EscapeDataString(permitId)}/renew", request, null, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<List<object>> GetPendingPermitsAsync(string operatorId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(operatorId)) throw new ArgumentException("Operator ID is required", nameof(operatorId));
            if (AccessMode == ServiceAccessMode.Remote)
                return await GetAsync<List<object>>($"/api/permits/operator/{Uri.EscapeDataString(operatorId)}/pending", null, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<object> GetJurisdictionRequirementsAsync(string jurisdiction, string permitType, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(jurisdiction)) throw new ArgumentException("Jurisdiction is required", nameof(jurisdiction));
            if (string.IsNullOrEmpty(permitType)) throw new ArgumentException("Permit type is required", nameof(permitType));
            if (AccessMode == ServiceAccessMode.Remote)
                return await GetAsync<object>($"/api/permits/jurisdiction/{Uri.EscapeDataString(jurisdiction)}/{Uri.EscapeDataString(permitType)}/requirements", null, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }
    }
}

