using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.Models.Data.Allocation;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Service for production allocation operations.
    /// Allocates volumes to wells, leases, tracts, and working interests.
    /// </summary>
    public interface IAllocationService
    {
        Task<ALLOCATION_RESULT> AllocateAsync(RUN_TICKET RUN_TICKET, string method, string userId, string cn = "PPDM39");
        Task<ALLOCATION_RESULT?> GetAsync(string allocationId, string cn = "PPDM39");
        Task<List<ALLOCATION_DETAIL>> GetDetailsAsync(string allocationId, string cn = "PPDM39");
        Task<List<ALLOCATION_RESULT>> GetHistoryAsync(string runTicketId, string cn = "PPDM39");
        Task<bool> ValidateAsync(ALLOCATION_RESULT allocation, string cn = "PPDM39");
        Task ReverseAsync(string allocationId, string userId, string cn = "PPDM39");
    }
}

