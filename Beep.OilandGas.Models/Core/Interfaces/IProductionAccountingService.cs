using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Orchestrator service for production accounting.
    /// Coordinates allocation, royalty, measurement, and financial operations.
    /// </summary>
    public interface IProductionAccountingService
    {
        Task<bool> ProcessProductionCycleAsync(RUN_TICKET RUN_TICKET, string userId, string connectionName = "PPDM39");
        Task<AccountingStatusData> GetAccountingStatusAsync(string fieldId, DateTime? asOfDate = null, string connectionName = "PPDM39");
        Task<bool> ClosePeriodAsync(string fieldId, DateTime periodEnd, string userId, string connectionName = "PPDM39");
    }

    /// <summary>
    /// Accounting status data for reporting.
    /// </summary>
    
}

