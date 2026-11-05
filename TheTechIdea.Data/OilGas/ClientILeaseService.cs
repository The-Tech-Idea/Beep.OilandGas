using Beep.OilandGas.Models.Data.Lease;
using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.Client.App.Services.Lease;

public interface ILeaseService
{
    Task<LeaseSummary> EvaluateLeaseAsync(string leaseId, string fieldId, CancellationToken cancellationToken = default);
    Task<List<LeaseSummary>> GetAvailableLeasesAsync(Dictionary<string, string>? filters = null, CancellationToken cancellationToken = default);
    Task<LeaseCreationContext> GetCreationContextAsync(string fieldId, CancellationToken cancellationToken = default);
    Task<LeaseCreationReceipt> CreateLeaseAcquisitionAsync(CreateLeaseAcquisition request, CancellationToken cancellationToken = default);
    Task<LeaseCreationReceipt> ReconcileLeaseCreationAsync(CreateLeaseAcquisition request, CancellationToken cancellationToken = default);
    Task UpdateLeaseStatusAsync(string leaseId, string status, string fieldId, CancellationToken cancellationToken = default);
}
