using System;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.ProductionAccounting;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Applies lower-of-cost-or-market (LCM) adjustments to inventory.
    /// </summary>
    public interface IInventoryLcmService
    {
        Task<INVENTORY_ADJUSTMENT?> ApplyLowerOfCostOrMarketAsync(
            string inventoryItemId,
            DateTime valuationDate,
            string userId,
            string cn = "PPDM39");

        Task<decimal> GetMarketValueAsync(
            string inventoryItemId,
            DateTime valuationDate,
            string cn = "PPDM39");
    }
}
