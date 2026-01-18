using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.ProductionAccounting;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Service for Joint Interest Billing (COPAS standard).
    /// Multi-party cost and revenue sharing.
    /// </summary>
    public interface IJointInterestBillingService
    {
        Task<bool> AllocateToParticipantsAsync(ALLOCATION_RESULT allocation, string userId, string cn = "PPDM39");
        Task<bool> GenerateStatementAsync(string leaseId, DateTime periodEnd, string userId, string cn = "PPDM39");
        Task<bool> ValidateAsync(string leaseId, string cn = "PPDM39");
    }
}
