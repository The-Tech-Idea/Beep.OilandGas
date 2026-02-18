using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Beep.OilandGas.Models.Data.PermitsAndApplications;

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

        public async Task<PermitApplication> CreatePermitApplicationAsync(PermitApplication request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PostAsync<PermitApplication, PermitApplication>("/api/permits/application", request, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<string> GetPermitStatusAsync(string permitId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(permitId)) throw new ArgumentException("Permit ID is required", nameof(permitId));
            if (AccessMode == ServiceAccessMode.Remote)
                return await GetAsync<string>($"/api/permits/{Uri.EscapeDataString(permitId)}/status", cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<List<RequiredDocument>> GetRequiredDocumentsAsync(string permitType, string jurisdiction, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(permitType)) throw new ArgumentException("Permit type is required", nameof(permitType));
            if (string.IsNullOrEmpty(jurisdiction)) throw new ArgumentException("Jurisdiction is required", nameof(jurisdiction));
            if (AccessMode == ServiceAccessMode.Remote)
                return await GetAsync<List<RequiredDocument>>($"/api/permits/documents/{Uri.EscapeDataString(permitType)}/{Uri.EscapeDataString(jurisdiction)}", cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<bool> SubmitPermitAsync(string permitId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(permitId)) throw new ArgumentException("Permit ID is required", nameof(permitId));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PostAsync<object, bool>($"/api/permits/{Uri.EscapeDataString(permitId)}/submit", new { }, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<PermitApplication> UpdatePermitApplicationAsync(string permitId, PermitApplication request, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(permitId)) throw new ArgumentException("Permit ID is required", nameof(permitId));
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PutAsync<PermitApplication, PermitApplication>($"/api/permits/{Uri.EscapeDataString(permitId)}", request, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<List<PermitHistory>> GetPermitHistoryAsync(string assetId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(assetId)) throw new ArgumentException("Asset ID is required", nameof(assetId));
            if (AccessMode == ServiceAccessMode.Remote)
                return await GetAsync<List<PermitHistory>>($"/api/permits/asset/{Uri.EscapeDataString(assetId)}/history", cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<PermitComplianceResult> GetPermitComplianceAsync(string permitId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(permitId)) throw new ArgumentException("Permit ID is required", nameof(permitId));
            if (AccessMode == ServiceAccessMode.Remote)
                return await GetAsync<PermitComplianceResult>($"/api/permits/{Uri.EscapeDataString(permitId)}/compliance", cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<PermitApplication> RenewPermitAsync(string permitId, PermitApplication request, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(permitId)) throw new ArgumentException("Permit ID is required", nameof(permitId));
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PostAsync<PermitApplication, PermitApplication>($"/api/permits/{Uri.EscapeDataString(permitId)}/renew", request, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<List<PermitApplication>> GetPendingPermitsAsync(string operatorId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(operatorId)) throw new ArgumentException("Operator ID is required", nameof(operatorId));
            if (AccessMode == ServiceAccessMode.Remote)
                return await GetAsync<List<PermitApplication>>($"/api/permits/operator/{Uri.EscapeDataString(operatorId)}/pending", cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<PermitRequirements> GetJurisdictionRequirementsAsync(string jurisdiction, string permitType, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(jurisdiction)) throw new ArgumentException("Jurisdiction is required", nameof(jurisdiction));
            if (string.IsNullOrEmpty(permitType)) throw new ArgumentException("Permit type is required", nameof(permitType));
            if (AccessMode == ServiceAccessMode.Remote)
                return await GetAsync<PermitRequirements>($"/api/permits/jurisdiction/{Uri.EscapeDataString(jurisdiction)}/{Uri.EscapeDataString(permitType)}/requirements", cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }
    }
}

