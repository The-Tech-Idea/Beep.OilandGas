using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.ProductionAccounting;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Service for accounting period closing, and the one owner of how a closed period is recognized: by the closing
    /// journal the close posts. Every reader of a period's close status asks this service.
    /// </summary>
    public interface IPeriodClosingService
    {
        Task<bool> ValidateReadinessAsync(string fieldId, DateTime periodEnd, string connectionName = "PPDM39");
        Task<bool> ClosePeriodAsync(string fieldId, DateTime periodEnd, string userId, string connectionName = "PPDM39");
        Task<List<string>> GetUnreconciledItemsAsync(string fieldId, DateTime periodEnd, string connectionName = "PPDM39");

        /// <summary>
        /// The close status of the field's period containing <paramref name="periodEnd"/>, as a <c>PERIOD_CLOSE_STATUS</c>
        /// code: <c>CLOSED</c> while its closing journal stands, <c>CLOSING</c> while one was created and never posted,
        /// <c>REOPENED</c> once it was reversed, <c>OPEN</c> when the period was never closed.
        /// </summary>
        Task<string> GetPeriodStatusAsync(string fieldId, DateTime periodEnd);

        /// <summary>
        /// Every close of the field's periods, newest period first, from the closing journals; every field's when
        /// <paramref name="fieldId"/> is null or empty.
        /// </summary>
        Task<List<PeriodCloseSummary>> GetPeriodClosesAsync(string fieldId);
    }
}
