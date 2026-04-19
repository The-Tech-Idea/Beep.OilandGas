using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.Models.Data.Inventory;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Service for inventory management (tanks, pipelines, storage).
    /// </summary>
    public interface IInventoryService
    {
        Task<TANK_INVENTORY> UpdateInventoryAsync(string tankId, decimal volume, string userId, string cn = "PPDM39");
        Task<TANK_INVENTORY?> GetInventoryAsync(string tankId, string cn = "PPDM39");
        Task<bool> ValidateAsync(TANK_INVENTORY inventory, string cn = "PPDM39");
        Task<INVENTORY_VALUATION> CalculateValuationAsync(string inventoryItemId, DateTime valuationDate, string method, string userId, string cn = "PPDM39");
        Task<INVENTORY_REPORT_SUMMARY> GenerateReconciliationReportAsync(string inventoryItemId, DateTime periodStart, DateTime periodEnd, string userId, string cn = "PPDM39");
    }
}

