using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.DTOs;

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
        Task<List<LeaseDto>> GetLeasesAsync(string? fieldId = null);

        /// <summary>
        /// Gets a lease by ID.
        /// </summary>
        Task<LeaseDto?> GetLeaseAsync(string leaseId);

        /// <summary>
        /// Creates a new lease.
        /// </summary>
        Task<LeaseDto> CreateLeaseAsync(CreateLeaseDto createDto);

        /// <summary>
        /// Updates a lease.
        /// </summary>
        Task<LeaseDto> UpdateLeaseAsync(string leaseId, UpdateLeaseDto updateDto);

        /// <summary>
        /// Renews a lease.
        /// </summary>
        Task<LeaseDto> RenewLeaseAsync(string leaseId, DateTime newExpirationDate);

        /// <summary>
        /// Gets leases expiring within specified days.
        /// </summary>
        Task<List<LeaseDto>> GetExpiringLeasesAsync(int days);

        /// <summary>
        /// Gets land rights for a lease.
        /// </summary>
        Task<List<LandRightDto>> GetLandRightsAsync(string leaseId);

        /// <summary>
        /// Gets mineral rights for a lease.
        /// </summary>
        Task<List<MineralRightDto>> GetMineralRightsAsync(string leaseId);

        /// <summary>
        /// Gets surface agreements for a lease.
        /// </summary>
        Task<List<SurfaceAgreementDto>> GetSurfaceAgreementsAsync(string leaseId);

        /// <summary>
        /// Gets royalties for a lease.
        /// </summary>
        Task<List<RoyaltyDto>> GetRoyaltiesAsync(string leaseId);
    }
}

