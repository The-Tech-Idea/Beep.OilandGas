using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.Lease;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Service interface for lease acquisition operations.
    /// Reads recorded lease facts and manages their registration and operational status in an explicit FIELD.
    /// </summary>
    public interface ILeaseAcquisitionService
    {
        Task<TheTechIdea.Data.OilGas.LeaseCreationContext> GetCreationContextAsync(string fieldId);
        Task<TheTechIdea.Data.OilGas.LeaseCreationReceipt> ReconcileLeaseCreationAsync(CreateLeaseAcquisition request, string userId);
        /// <summary>
        /// Reads one recorded lease under exact company and FIELD authority; no acquisition recommendation is inferred.
        /// </summary>
        /// <param name="leaseId">Lease identifier</param>
        /// <returns>Lease evaluation result</returns>
        Task<LeaseSummary> EvaluateLeaseAsync(string leaseId, string fieldId);

        /// <summary>
        /// Reads recorded company leases for an exact FIELD assertion, with optional additional supported filters.
        /// </summary>
        /// <param name="filters">Optional filters (field name, value pairs)</param>
        /// <returns>List of leases</returns>
        Task<List<LeaseSummary>> GetAvailableLeasesAsync(Dictionary<string, string>? filters = null);

        /// <summary>
        /// Creates a new lease acquisition record.
        /// </summary>
        /// <param name="leaseRequest">Lease creation request (fee mineral or government)</param>
        /// <param name="userId">User ID for audit</param>
        /// <returns>Created lease identifier</returns>
        Task<string> CreateLeaseAcquisitionAsync(CreateLeaseAcquisition leaseRequest, string userId);

        /// <summary>
        /// Updates lease acquisition status.
        /// </summary>
        /// <param name="leaseId">Lease identifier</param>
        /// <param name="status">New status</param>
        /// <param name="userId">User ID for audit</param>
        /// <returns>Task</returns>
        Task UpdateLeaseStatusAsync(string leaseId, string status, string userId, string fieldId);
    }
}




