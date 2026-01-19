using System;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.ProductionAccounting;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Handles take-or-pay contractual adjustments for gas/oil sales contracts.
    /// </summary>
    public interface ITakeOrPayService
    {
        Task<REVENUE_TRANSACTION?> ApplyTakeOrPayAsync(
            RUN_TICKET runTicket,
            ALLOCATION_RESULT allocationResult,
            decimal deliveredVolume,
            string userId,
            string cn = "PPDM39");
    }
}
