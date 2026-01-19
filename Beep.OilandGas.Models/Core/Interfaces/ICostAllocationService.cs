using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.ProductionAccounting;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Service for shared cost allocations to cost centers and properties.
    /// </summary>
    public interface ICostAllocationService
    {
        Task<List<COST_ALLOCATION>> AllocateCostAsync(
            ACCOUNTING_COST cost,
            List<COST_ALLOCATION> allocations,
            string userId,
            string cn = "PPDM39");
    }
}
