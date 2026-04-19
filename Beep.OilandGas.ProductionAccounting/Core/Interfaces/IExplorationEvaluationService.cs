using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.ProductionAccounting;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Service for IFRS 6 exploration and evaluation accounting.
    /// </summary>
    public interface IExplorationEvaluationService
    {
        Task<ACCOUNTING_COST> RecordEvaluationCostAsync(
            ACCOUNTING_COST cost,
            bool capitalize,
            string userId,
            string cn = "PPDM39");
    }
}
