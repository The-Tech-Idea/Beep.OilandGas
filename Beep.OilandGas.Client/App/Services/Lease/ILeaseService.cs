using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Beep.OilandGas.Client.App.Services.Lease
{
    /// <summary>
    /// Service interface for Lease Acquisition operations
    /// </summary>
    public interface ILeaseService
    {
        Task<object> CreateLeaseAcquisitionAsync(object request, CancellationToken cancellationToken = default);
        Task<object> GetLeaseTermsAsync(string leaseId, CancellationToken cancellationToken = default);
        Task<object> GetRoyaltyObligationsAsync(string leaseId, CancellationToken cancellationToken = default);
        Task<object> GetLeaseStatusAsync(string leaseId, CancellationToken cancellationToken = default);
        Task<object> UpdateLeaseAsync(string leaseId, object request, CancellationToken cancellationToken = default);
        Task<List<object>> GetLeasesByOperatorAsync(string operatorId, CancellationToken cancellationToken = default);
        Task<object> GetLeaseExpirationAsync(string leaseId, CancellationToken cancellationToken = default);
        Task<object> RenewLeaseAsync(string leaseId, object request, CancellationToken cancellationToken = default);
        Task<object> TransferLeaseAsync(string leaseId, object transferRequest, CancellationToken cancellationToken = default);
        Task<List<object>> GetLeasePaymentHistoryAsync(string leaseId, CancellationToken cancellationToken = default);
        Task<object> CalculateBonusPaymentAsync(object request, CancellationToken cancellationToken = default);
        Task<object> GetLeaseGeometryAsync(string leaseId, CancellationToken cancellationToken = default);
    }
}

