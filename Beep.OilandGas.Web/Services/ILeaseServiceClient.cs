using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.Lease;

namespace Beep.OilandGas.Web.Services
{
    public interface ILeaseServiceClient
    {
        Task<LeaseSummary> EvaluateLeaseAsync(string leaseId);
        Task<List<LeaseSummary>> GetAvailableLeasesAsync(Dictionary<string, string>? filters = null);
        Task<string> CreateLeaseAcquisitionAsync(CreateLeaseAcquisition leaseRequest, string? userId = null);
        Task<bool> UpdateLeaseStatusAsync(string leaseId, string status, string? userId = null);
    }
}