using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.ProductionAccounting;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Orchestrator service for production accounting.
    /// Coordinates allocation, royalty, measurement, and financial operations.
    /// </summary>
    public interface IProductionAccountingService
    {
        Task<bool> ProcessProductionCycleAsync(RUN_TICKET runTicket, string userId, string connectionName = "PPDM39");
        Task<AccountingStatusData> GetAccountingStatusAsync(string fieldId, DateTime? asOfDate = null, string connectionName = "PPDM39");
        Task<bool> ClosePeriodAsync(string fieldId, DateTime periodEnd, string userId, string connectionName = "PPDM39");
    }

    /// <summary>
    /// Accounting status data for reporting.
    /// </summary>
    public class AccountingStatusData
    {
        public string FieldId { get; set; }
        public DateTime? AsOfDate { get; set; }
        public decimal TotalProduction { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalRoyalty { get; set; }
        public decimal TotalCosts { get; set; }
        public decimal NetIncome { get; set; }
        public string AccountingMethod { get; set; }
        public string PeriodStatus { get; set; }
    }
}
