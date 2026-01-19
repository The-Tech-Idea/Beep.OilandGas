using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.ProductionAccounting;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Service for Full Cost accounting method.
    /// Capitalizes all exploration/development costs in cost centers.
    /// </summary>
    public interface IFullCostService
    {
        Task<ACCOUNTING_COST> RecordCostAsync(string costCenterId, decimal cost, string userId, string cn = "PPDM39");
        Task<bool> PerformCeilingTestAsync(string costCenterId, string userId, string cn = "PPDM39");
        Task<IMPAIRMENT_RECORD> RecordImpairmentAsync(string costCenterId, decimal impairmentAmount, string reason, string userId, string cn = "PPDM39");
        Task<decimal> CalculateDepletionAsync(string costCenterId, DateTime startDate, DateTime endDate, string cn = "PPDM39");
        Task<bool> ValidateAsync(ACCOUNTING_COST cost, string cn = "PPDM39");
    }
}
