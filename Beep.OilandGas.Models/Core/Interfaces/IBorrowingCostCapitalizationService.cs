using System;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.ProductionAccounting;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Service for IAS 23 borrowing cost capitalization.
    /// </summary>
    public interface IBorrowingCostCapitalizationService
    {
        Task<ACCOUNTING_COST> CapitalizeBorrowingCostAsync(
            ACCOUNTING_COST cost,
            DateTime periodStart,
            DateTime periodEnd,
            string userId,
            string cn = "PPDM39");
    }
}
