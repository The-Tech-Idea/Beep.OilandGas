using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.DTOs.Lease;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Service interface for lease acquisition operations.
    /// Provides lease evaluation, acquisition workflow, and due diligence tracking.
    /// </summary>
    public interface ILeaseAcquisitionService
    {
        /// <summary>
        /// Evaluates a lease for acquisition.
        /// </summary>
        /// <param name="leaseId">Lease identifier</param>
        /// <returns>Lease evaluation result</returns>
        Task<LeaseSummary> EvaluateLeaseAsync(string leaseId);

        /// <summary>
        /// Gets all leases available for acquisition.
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
        Task<string> CreateLeaseAcquisitionAsync(CreateLeaseAcquisitionDto leaseRequest, string userId);

        /// <summary>
        /// Updates lease acquisition status.
        /// </summary>
        /// <param name="leaseId">Lease identifier</param>
        /// <param name="status">New status</param>
        /// <param name="userId">User ID for audit</param>
        /// <returns>Task</returns>
        Task UpdateLeaseStatusAsync(string leaseId, string status, string userId);
    }
}

