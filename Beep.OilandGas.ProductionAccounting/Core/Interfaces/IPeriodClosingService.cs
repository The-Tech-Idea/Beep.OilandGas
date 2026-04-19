using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.ProductionAccounting;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Service for accounting period closing.
    /// </summary>
    public interface IPeriodClosingService
    {
        Task<bool> ValidateReadinessAsync(string fieldId, DateTime periodEnd, string cn = "PPDM39");
        Task<bool> ClosePeriodAsync(string fieldId, DateTime periodEnd, string userId, string cn = "PPDM39");
        Task<List<string>> GetUnreconciledItemsAsync(string fieldId, DateTime periodEnd, string cn = "PPDM39");
    }
}
