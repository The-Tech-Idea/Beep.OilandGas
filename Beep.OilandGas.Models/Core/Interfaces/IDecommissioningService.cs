using System;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.Models.Data.Decommissioning;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Service for IAS 37 decommissioning and asset retirement obligations.
    /// </summary>
    public interface IDecommissioningService
    {
        Task<ASSET_RETIREMENT_OBLIGATION> UpdateAroEstimateAsync(
            ASSET_RETIREMENT_OBLIGATION obligation,
            DateTime asOfDate,
            string userId,
            string cn = "PPDM39");

        Task<ASSET_RETIREMENT_OBLIGATION> AccreteAroAsync(
            string aroId,
            DateTime periodEnd,
            string userId,
            string cn = "PPDM39");
    }
}

