using System;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.ProductionAccounting;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Calculates and applies COPAS overhead on joint interest billing statements.
    /// </summary>
    public interface ICopasOverheadService
    {
        Task<decimal> CalculateOverheadAsync(
            string leaseId,
            decimal baseAmount,
            DateTime asOfDate,
            string cn = "PPDM39");

        Task<JIB_CHARGE?> ApplyOverheadToStatementAsync(
            JOINT_INTEREST_STATEMENT statement,
            decimal baseAmount,
            string userId,
            string cn = "PPDM39");
    }
}
