using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.Lease;
using LeaseModel = Beep.OilandGas.Models.Data.Lease.Lease;

namespace Beep.OilandGas.Client.App.Services.Lease
{
    /// <summary>
    /// Service interface for Lease Acquisition operations
    /// </summary>
    public interface ILeaseService
    {
        Task<LeaseModel> CreateLeaseAcquisitionAsync(LeaseModel request, CancellationToken cancellationToken = default);
        Task<LeaseModel> GetLeaseTermsAsync(string leaseId, CancellationToken cancellationToken = default);
        Task<List<Royalty>> GetRoyaltyObligationsAsync(string leaseId, CancellationToken cancellationToken = default);
        Task<string> GetLeaseStatusAsync(string leaseId, CancellationToken cancellationToken = default);
        Task<LeaseModel> UpdateLeaseAsync(string leaseId, LeaseModel request, CancellationToken cancellationToken = default);
        Task<List<LeaseModel>> GetLeasesByOperatorAsync(string operatorId, CancellationToken cancellationToken = default);
        Task<DateTime?> GetLeaseExpirationAsync(string leaseId, CancellationToken cancellationToken = default);
        Task<LeaseModel> RenewLeaseAsync(string leaseId, LeaseModel request, CancellationToken cancellationToken = default);
        Task<LeaseModel> TransferLeaseAsync(string leaseId, TransferLeaseRequest transferRequest, CancellationToken cancellationToken = default);
        Task<List<LeasePayment>> GetLeasePaymentHistoryAsync(string leaseId, CancellationToken cancellationToken = default);
        Task<BonusPaymentResult> CalculateBonusPaymentAsync(BonusPaymentRequest request, CancellationToken cancellationToken = default);
        Task<LeaseGeometry> GetLeaseGeometryAsync(string leaseId, CancellationToken cancellationToken = default);
    }
}

