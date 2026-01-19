using System;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.ProductionAccounting;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Handles drilling-specific scenarios (dry hole, sidetrack, plug-back) per successful efforts rules.
    /// </summary>
    public interface IDrillingScenarioAccountingService
    {
        Task<ACCOUNTING_COST> RecordDrillingCostAsync(
            string wellId,
            decimal cost,
            string scenario,
            DateTime costDate,
            string userId,
            string cn = "PPDM39");

        Task<ACCOUNTING_COST> RecordSalvageRecoveryAsync(
            string wellId,
            decimal salvageAmount,
            DateTime salvageDate,
            string userId,
            string cn = "PPDM39");

        Task<ACCOUNTING_COST> RecordTestWellContributionAsync(
            string wellId,
            decimal cost,
            DateTime costDate,
            string userId,
            string cn = "PPDM39");

        Task<bool> ValidateScenarioAsync(string scenario);
    }
}
