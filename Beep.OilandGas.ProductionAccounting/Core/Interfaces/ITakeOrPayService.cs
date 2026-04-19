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
            RUN_TICKET RUN_TICKET,
            ALLOCATION_RESULT ALLOCATION_RESULT,
            decimal deliveredVolume,
            string userId,
            string cn = "PPDM39");
    }
}
