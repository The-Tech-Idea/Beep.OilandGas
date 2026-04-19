using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.ProductionAccounting;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Calculates production taxes (severance, ad valorem) for revenue and royalty transactions.
    /// </summary>
    public interface IProductionTaxService
    {
        Task<TAX_TRANSACTION?> CalculateProductionTaxesAsync(
            REVENUE_TRANSACTION revenueTransaction,
            string userId,
            string cn = "PPDM39");
    }
}
