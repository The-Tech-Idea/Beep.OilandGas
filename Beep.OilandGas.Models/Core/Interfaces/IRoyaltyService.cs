using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.ProductionAccounting;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Service for royalty calculation and management.
    /// Handles mineral, overriding, and net profit interest royalties.
    /// </summary>
    public interface IRoyaltyService
    {
        Task<ROYALTY_CALCULATION> CalculateAsync(ALLOCATION_DETAIL detail, string userId, string cn = "PPDM39");
        Task<ROYALTY_CALCULATION?> GetAsync(string royaltyId, string cn = "PPDM39");
        Task<List<ROYALTY_CALCULATION>> GetByAllocationAsync(string allocationId, string cn = "PPDM39");
        Task<ROYALTY_PAYMENT> RecordPaymentAsync(ROYALTY_CALCULATION royalty, decimal amount, string userId, string cn = "PPDM39");
        Task<bool> ValidateAsync(ROYALTY_CALCULATION royalty, string cn = "PPDM39");
    }
}
