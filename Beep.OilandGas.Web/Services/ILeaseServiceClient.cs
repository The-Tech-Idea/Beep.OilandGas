using Beep.OilandGas.Models.Data.Company;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.Lease;

namespace Beep.OilandGas.Web.Services
{
    public interface ILeaseServiceClient
    {
        Task<Beep.OilandGas.Models.Data.Lease.LeaseCreationContext> GetCreationContextAsync(string fieldId);
        Task<Beep.OilandGas.Models.Data.Lease.LeaseCreationReceipt> ReconcileLeaseCreationAsync(CreateLeaseAcquisition request);
        Task<LeaseSummary> EvaluateLeaseAsync(string leaseId, string fieldId, System.Threading.CancellationToken cancellationToken = default);
        Task<List<LeaseSummary>> GetAvailableLeasesAsync(Dictionary<string, string>? filters = null, System.Threading.CancellationToken cancellationToken = default);
        Task<string> CreateLeaseAcquisitionAsync(CreateLeaseAcquisition leaseRequest);
        Task UpdateLeaseStatusAsync(string leaseId, string status, string fieldId);
    }
}
