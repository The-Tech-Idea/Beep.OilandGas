using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.ProductionAccounting;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Service for Successful Efforts accounting method.
    /// Capitalizes only successful exploration/development costs.
    /// </summary>
    public interface ISuccessfulEffortsService
    {
        Task<ACCOUNTING_COST> RecordCostAsync(string wellId, decimal cost, string userId, string cn = "PPDM39");
        Task<decimal> CalculateDepletionAsync(string wellId, DateTime startDate, DateTime endDate, string cn = "PPDM39");
        Task<bool> ValidateAsync(ACCOUNTING_COST cost, string cn = "PPDM39");
    }
}
