using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.Models.Data.Imbalance;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Service for imbalance management (inventory/financial imbalances).
    /// </summary>
    public interface IImbalanceService
    {
        Task<IMBALANCE_ADJUSTMENT> RecordImbalanceAsync(string leaseId, decimal volume, string userId, string cn = "PPDM39");
        Task<bool> ReconcileAsync(string leaseId, DateTime startDate, DateTime endDate, string userId, string cn = "PPDM39");
        Task<decimal> GetOutstandingImbalanceAsync(string leaseId, string cn = "PPDM39");
        Task<bool> ValidateAsync(IMBALANCE_ADJUSTMENT imbalance, string cn = "PPDM39");
    }
}

