using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.Models.Data.Royalty;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Service for royalty calculation and management.
    /// Handles mineral, overriding, and net profit interest royalties.
    /// </summary>
    public interface IRoyaltyService
    {
        Task<ROYALTY_CALCULATION> CalculateAsync(string allocationDetailId, string userId);
        Task<string> GetAllocationFieldAsync(string allocationDetailId);
        Task<ROYALTY_CALCULATION> PreviewAsync(ALLOCATION_DETAIL detail, string userId, string connectionName = "PPDM39");
        Task<ROYALTY_CALCULATION?> GetAsync(string royaltyId, string connectionName = "PPDM39");
        Task<List<ROYALTY_CALCULATION>> GetByAllocationAsync(string allocationId, string connectionName = "PPDM39");
        Task<ROYALTY_PAYMENT> RecordPaymentAsync(string royaltyId, Guid requestId, decimal amount, string userId);
        Task<List<ROYALTY_PAYMENT>> GetPaymentsAsync(string royaltyId);
        Task<List<RoyaltyPostingReview>> ReviewPostingsAsync(string royaltyId);
        Task<bool> ValidateAsync(ROYALTY_CALCULATION royalty, string connectionName = "PPDM39");
    }
}
