using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.Models.DTOs.ProductionAccounting;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Service interface for allocation operations.
    /// </summary>
    public interface IAllocationService
    {
        /// <summary>
        /// Allocates production to wells.
        /// </summary>
        Task<AllocationRequest> AllocateProductionAsync(
            string runTicketId,
            AllocationMethod method,
            List<WellAllocationDataDto> wells,
            string userId,
            string? connectionName = null);
        
        /// <summary>
        /// Allocates production to leases.
        /// </summary>
        Task<AllocationRequest> AllocateToLeasesAsync(
            string runTicketId,
            AllocationMethod method,
            List<LeaseAllocationDataDto> leases,
            string userId,
            string? connectionName = null);
        
        /// <summary>
        /// Allocates production to tracts.
        /// </summary>
        Task<AllocationRequest> AllocateToTractsAsync(
            string runTicketId,
            AllocationMethod method,
            List<TractAllocationDataDto> tracts,
            string userId,
            string? connectionName = null);
        
        /// <summary>
        /// Gets an allocation result by ID.
        /// </summary>
        Task<AllocationRequest?> GetAllocationResultAsync(string allocationId, string? connectionName = null);
        
        /// <summary>
        /// Gets allocation history for a run ticket.
        /// </summary>
        Task<List<AllocationRequest>> GetAllocationHistoryAsync(string runTicketId, string? connectionName = null);
        
        /// <summary>
        /// Gets allocation details for an allocation result.
        /// </summary>
        Task<List<ALLOCATION_DETAIL>> GetAllocationDetailsAsync(string allocationId, string? connectionName = null);
    }
}




