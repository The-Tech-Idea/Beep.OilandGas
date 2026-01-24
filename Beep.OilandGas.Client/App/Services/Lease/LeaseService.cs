using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Beep.OilandGas.Client.App.Services.Lease
{
    /// <summary>
    /// Unified service for Lease Acquisition operations
    /// </summary>
    internal class LeaseService : ServiceBase, ILeaseService
    {
        public LeaseService(BeepOilandGasApp app, ILogger<LeaseService>? logger = null)
            : base(app, logger)
        {
        }

        public async Task<object> CreateLeaseAcquisitionAsync(object request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PostAsync<object, object>("/api/lease/acquisition", request, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<object> GetLeaseTermsAsync(string leaseId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(leaseId)) throw new ArgumentException("Lease ID is required", nameof(leaseId));
            if (AccessMode == ServiceAccessMode.Remote)
                return await GetAsync<object>($"/api/lease/{Uri.EscapeDataString(leaseId)}/terms", cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<object> GetRoyaltyObligationsAsync(string leaseId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(leaseId)) throw new ArgumentException("Lease ID is required", nameof(leaseId));
            if (AccessMode == ServiceAccessMode.Remote)
                return await GetAsync<object>($"/api/lease/{Uri.EscapeDataString(leaseId)}/royalty-obligations", cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<object> GetLeaseStatusAsync(string leaseId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(leaseId)) throw new ArgumentException("Lease ID is required", nameof(leaseId));
            if (AccessMode == ServiceAccessMode.Remote)
                return await GetAsync<object>($"/api/lease/{Uri.EscapeDataString(leaseId)}/status", cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<object> UpdateLeaseAsync(string leaseId, object request, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(leaseId)) throw new ArgumentException("Lease ID is required", nameof(leaseId));
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PutAsync<object, object>($"/api/lease/{Uri.EscapeDataString(leaseId)}", request, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<List<object>> GetLeasesByOperatorAsync(string operatorId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(operatorId)) throw new ArgumentException("Operator ID is required", nameof(operatorId));
            if (AccessMode == ServiceAccessMode.Remote)
                return await GetAsync<List<object>>($"/api/lease/operator/{Uri.EscapeDataString(operatorId)}", cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<object> GetLeaseExpirationAsync(string leaseId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(leaseId)) throw new ArgumentException("Lease ID is required", nameof(leaseId));
            if (AccessMode == ServiceAccessMode.Remote)
                return await GetAsync<object>($"/api/lease/{Uri.EscapeDataString(leaseId)}/expiration", cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<object> RenewLeaseAsync(string leaseId, object request, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(leaseId)) throw new ArgumentException("Lease ID is required", nameof(leaseId));
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PostAsync<object, object>($"/api/lease/{Uri.EscapeDataString(leaseId)}/renew", request, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<object> TransferLeaseAsync(string leaseId, object transferRequest, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(leaseId)) throw new ArgumentException("Lease ID is required", nameof(leaseId));
            if (transferRequest == null) throw new ArgumentNullException(nameof(transferRequest));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PostAsync<object, object>($"/api/lease/{Uri.EscapeDataString(leaseId)}/transfer", transferRequest, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<List<object>> GetLeasePaymentHistoryAsync(string leaseId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(leaseId)) throw new ArgumentException("Lease ID is required", nameof(leaseId));
            if (AccessMode == ServiceAccessMode.Remote)
                return await GetAsync<List<object>>($"/api/lease/{Uri.EscapeDataString(leaseId)}/payments", cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<object> CalculateBonusPaymentAsync(object request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PostAsync<object, object>("/api/lease/bonus/calculate", request, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<object> GetLeaseGeometryAsync(string leaseId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(leaseId)) throw new ArgumentException("Lease ID is required", nameof(leaseId));
            if (AccessMode == ServiceAccessMode.Remote)
                return await GetAsync<object>($"/api/lease/{Uri.EscapeDataString(leaseId)}/geometry", cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }
    }
}

