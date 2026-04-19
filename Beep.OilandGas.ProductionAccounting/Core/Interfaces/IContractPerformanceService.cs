using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.ProductionAccounting;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Service for ASC 606 contract performance obligations.
    /// </summary>
    public interface IContractPerformanceService
    {
        Task<CONTRACT_PERFORMANCE_OBLIGATION> CreateObligationAsync(CONTRACT_PERFORMANCE_OBLIGATION obligation, string userId, string cn = "PPDM39");
        Task<CONTRACT_PERFORMANCE_OBLIGATION> MarkSatisfiedAsync(string obligationId, DateTime satisfiedDate, string userId, string cn = "PPDM39");
        Task<List<CONTRACT_PERFORMANCE_OBLIGATION>> GetOutstandingAsync(string salesContractId, string cn = "PPDM39");
    }
}
