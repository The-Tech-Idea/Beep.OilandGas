using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.Lease;

namespace Beep.OilandGas.LeaseAcquisition.Services
{
    public partial class LeaseAcquisitionService
    {
        // Explicit implementations of Models.Core.Interfaces.ILeaseAcquisitionService
        Task<LeaseSummary> Beep.OilandGas.Models.Core.Interfaces.ILeaseAcquisitionService.EvaluateLeaseAsync(
            string leaseId)
        {
            return Task.FromResult(new LeaseSummary());
        }

        Task<List<LeaseSummary>> Beep.OilandGas.Models.Core.Interfaces.ILeaseAcquisitionService.GetAvailableLeasesAsync(
            Dictionary<string, string>? filters)
        {
            return Task.FromResult(new List<LeaseSummary>());
        }

        Task<string> Beep.OilandGas.Models.Core.Interfaces.ILeaseAcquisitionService.CreateLeaseAcquisitionAsync(
            CreateLeaseAcquisition leaseRequest, string userId)
        {
            return Task.FromResult(string.Empty);
        }

        Task Beep.OilandGas.Models.Core.Interfaces.ILeaseAcquisitionService.UpdateLeaseStatusAsync(
            string leaseId, string status, string userId)
        {
            return Task.CompletedTask;
        }
    }
}
