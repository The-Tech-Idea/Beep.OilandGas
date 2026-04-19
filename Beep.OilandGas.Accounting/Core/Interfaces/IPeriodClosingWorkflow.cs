using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Service interface for period closing workflow orchestration.
    /// Manages the complete GL period closing lifecycle including validation, posting, and reconciliation.
    /// </summary>
    public interface IPeriodClosingWorkflow
    {
        /// <summary>
        /// Executes the complete period closing workflow.
        /// </summary>
        Task<PeriodClosingResult> ExecuteClosingAsync(string periodId, string entityId, string userId, string? connectionName = null);

        /// <summary>
        /// Reverses a completed period close.
        /// </summary>
        Task<PeriodClosingResult> ReverseClosingAsync(string periodId, string entityId, string userId, string? connectionName = null);

        /// <summary>
        /// Validates period closing prerequisites are met.
        /// </summary>
        Task<PeriodClosingValidation> ValidateClosingReadinessAsync(string periodId, string entityId, string? connectionName = null);

        /// <summary>
        /// Posts all pending journal entries for the period.
        /// </summary>
        Task<PostingResult> PostUnpostedEntriesAsync(string periodId, string entityId, string userId, string? connectionName = null);

        /// <summary>
        /// Performs account balance reconciliation for the period.
        /// </summary>
        Task<Beep.OilandGas.Models.Data.Accounting.ReconciliationResult> ReconcileAccountBalancesAsync(string periodId, string entityId, string? connectionName = null);

        /// <summary>
        /// Locks the period from further changes.
        /// </summary>
        Task<bool> LockPeriodAsync(string periodId, string entityId, string userId, string? connectionName = null);

        /// <summary>
        /// Unlocks the period for corrections.
        /// </summary>
        Task<bool> UnlockPeriodAsync(string periodId, string entityId, string userId, string? connectionName = null);

        /// <summary>
        /// Gets the status of a period close.
        /// </summary>
        Task<PeriodClosingStatus> GetStatusAsync(string periodId, string entityId, string? connectionName = null);
    }
}
