using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using System.Collections.Generic;
using Beep.OilandGas.Models.Data.Lease;
using LeaseModel = Beep.OilandGas.Models.Data.Lease.Lease;

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

        public async Task<LeaseModel> CreateLeaseAcquisitionAsync(LeaseModel request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PostAsync<LeaseModel, LeaseModel>("/api/lease/acquisition", request, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<LeaseModel> GetLeaseTermsAsync(string leaseId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(leaseId)) throw new ArgumentException("Lease ID is required", nameof(leaseId));
            if (AccessMode == ServiceAccessMode.Remote)
                return await GetAsync<LeaseModel>($"/api/lease/{Uri.EscapeDataString(leaseId)}/terms", cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<List<Royalty>> GetRoyaltyObligationsAsync(string leaseId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(leaseId)) throw new ArgumentException("Lease ID is required", nameof(leaseId));
            if (AccessMode == ServiceAccessMode.Remote)
                return await GetAsync<List<Royalty>>($"/api/lease/{Uri.EscapeDataString(leaseId)}/royalty-obligations", cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<string> GetLeaseStatusAsync(string leaseId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(leaseId)) throw new ArgumentException("Lease ID is required", nameof(leaseId));
            if (AccessMode == ServiceAccessMode.Remote)
                return await GetAsync<string>($"/api/lease/{Uri.EscapeDataString(leaseId)}/status", cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<LeaseModel> UpdateLeaseAsync(string leaseId, LeaseModel request, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(leaseId)) throw new ArgumentException("Lease ID is required", nameof(leaseId));
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PutAsync<LeaseModel, LeaseModel>($"/api/lease/{Uri.EscapeDataString(leaseId)}", request, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<List<LeaseModel>> GetLeasesByOperatorAsync(string operatorId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(operatorId)) throw new ArgumentException("Operator ID is required", nameof(operatorId));
            if (AccessMode == ServiceAccessMode.Remote)
                return await GetAsync<List<LeaseModel>>($"/api/lease/operator/{Uri.EscapeDataString(operatorId)}", cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<DateTime?> GetLeaseExpirationAsync(string leaseId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(leaseId)) throw new ArgumentException("Lease ID is required", nameof(leaseId));
            if (AccessMode == ServiceAccessMode.Remote)
                return await GetAsync<DateTime?>($"/api/lease/{Uri.EscapeDataString(leaseId)}/expiration", cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<LeaseModel> RenewLeaseAsync(string leaseId, LeaseModel request, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(leaseId)) throw new ArgumentException("Lease ID is required", nameof(leaseId));
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PostAsync<LeaseModel, LeaseModel>($"/api/lease/{Uri.EscapeDataString(leaseId)}/renew", request, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<LeaseModel> TransferLeaseAsync(string leaseId, TransferLeaseRequest transferRequest, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(leaseId)) throw new ArgumentException("Lease ID is required", nameof(leaseId));
            if (transferRequest == null) throw new ArgumentNullException(nameof(transferRequest));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PostAsync<TransferLeaseRequest, LeaseModel>($"/api/lease/{Uri.EscapeDataString(leaseId)}/transfer", transferRequest, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<List<LeasePayment>> GetLeasePaymentHistoryAsync(string leaseId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(leaseId)) throw new ArgumentException("Lease ID is required", nameof(leaseId));
            if (AccessMode == ServiceAccessMode.Remote)
                return await GetAsync<List<LeasePayment>>($"/api/lease/{Uri.EscapeDataString(leaseId)}/payments", cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<BonusPaymentResult> CalculateBonusPaymentAsync(BonusPaymentRequest request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PostAsync<BonusPaymentRequest, BonusPaymentResult>("/api/lease/bonus/calculate", request, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<LeaseGeometry> GetLeaseGeometryAsync(string leaseId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(leaseId)) throw new ArgumentException("Lease ID is required", nameof(leaseId));
            if (AccessMode == ServiceAccessMode.Remote)
                return await GetAsync<LeaseGeometry>($"/api/lease/{Uri.EscapeDataString(leaseId)}/geometry", cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }
    }
}

