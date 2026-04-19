using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.ProductionAccounting;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Service for asset swaps and farm-in/farm-out transactions.
    /// </summary>
    public interface IAssetSwapService
    {
        Task<ASSET_SWAP_TRANSACTION> RecordSwapAsync(
            ASSET_SWAP_TRANSACTION swap,
            string userId,
            string cn = "PPDM39");
    }
}
