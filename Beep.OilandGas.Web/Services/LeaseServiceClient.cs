using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Models.Data.Lease;

namespace Beep.OilandGas.Web.Services;

// Web presentation adapter; the SDK owns the HTTP contract and response validation.
public sealed class LeaseServiceClient(IBeepOilandGasApp app) : ILeaseServiceClient
{
    public Task<LeaseSummary> EvaluateLeaseAsync(string leaseId, string fieldId, CancellationToken cancellationToken = default) => app.Lease.EvaluateLeaseAsync(leaseId, fieldId, cancellationToken);
    public Task<List<LeaseSummary>> GetAvailableLeasesAsync(Dictionary<string, string>? filters = null, CancellationToken cancellationToken = default) => app.Lease.GetAvailableLeasesAsync(filters, cancellationToken);
    public Task<LeaseCreationContext> GetCreationContextAsync(string fieldId) => app.Lease.GetCreationContextAsync(fieldId);
    public Task<LeaseCreationReceipt> ReconcileLeaseCreationAsync(CreateLeaseAcquisition request) => app.Lease.ReconcileLeaseCreationAsync(request);
    public async Task<string> CreateLeaseAcquisitionAsync(CreateLeaseAcquisition request) => (await app.Lease.CreateLeaseAcquisitionAsync(request)).LeaseId!;
    public Task UpdateLeaseStatusAsync(string leaseId, string status, string fieldId) => app.Lease.UpdateLeaseStatusAsync(leaseId, status, fieldId);
}
