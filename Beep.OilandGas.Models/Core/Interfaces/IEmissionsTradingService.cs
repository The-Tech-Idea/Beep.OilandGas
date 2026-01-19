using System;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.ProductionAccounting;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Service for emissions trading schemes and carbon accounting.
    /// </summary>
    public interface IEmissionsTradingService
    {
        Task<EMISSIONS_OBLIGATION> UpdateObligationAsync(
            EMISSIONS_OBLIGATION obligation,
            decimal emissionsVolume,
            decimal allowancePrice,
            DateTime periodEnd,
            string userId,
            string cn = "PPDM39");

        Task<EMISSIONS_SETTLEMENT> SettleAsync(
            string obligationId,
            decimal allowancesSurrendered,
            DateTime settlementDate,
            string userId,
            string cn = "PPDM39");
    }
}
