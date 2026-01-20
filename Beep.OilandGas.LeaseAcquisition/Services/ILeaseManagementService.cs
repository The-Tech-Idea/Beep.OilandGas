using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.LeaseAcquisition.Services
{
    /// <summary>
    /// Service for managing lease agreements.
    /// </summary>
    public interface ILeaseManagementService
    {
        /// <summary>
        /// Gets all leases.
        /// </summary>
        Task<List<Lease>> GetLeasesAsync(string? fieldId = null);

        /// <summary>
        /// Gets a lease by ID.
        /// </summary>
        Task<Lease?> GetLeaseAsync(string leaseId);

        /// <summary>
        /// Creates a new lease.
        /// </summary>
        Task<Lease> CreateLeaseAsync(CreateLease createDto);

        /// <summary>
        /// Updates a lease.
        /// </summary>
        Task<Lease> UpdateLeaseAsync(string leaseId, UpdateLease updateDto);

        /// <summary>
        /// Renews a lease.
        /// </summary>
        Task<Lease> RenewLeaseAsync(string leaseId, DateTime newExpirationDate);

        /// <summary>
        /// Gets leases expiring within specified days.
        /// </summary>
        Task<List<Lease>> GetExpiringLeasesAsync(int days);

        /// <summary>
        /// Gets land rights for a lease.
        /// </summary>
        Task<List<LandRight>> GetLandRightsAsync(string leaseId);

        /// <summary>
        /// Gets mineral rights for a lease.
        /// </summary>
        Task<List<MineralRight>> GetMineralRightsAsync(string leaseId);

        /// <summary>
        /// Gets surface agreements for a lease.
        /// </summary>
        Task<List<SurfaceAgreement>> GetSurfaceAgreementsAsync(string leaseId);

        /// <summary>
        /// Gets royalties for a lease.
        /// </summary>
        Task<List<Royalty>> GetRoyaltiesAsync(string leaseId);
    }
}

