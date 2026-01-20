using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.Accounting;

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
        Task<ReconciliationResult> ReconcileAccountBalancesAsync(string periodId, string entityId, string? connectionName = null);

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

    /// <summary>
    /// Result of period closing operation.
    /// </summary>
    public class PeriodClosingResult
    {
        public bool Success { get; set; }
        public string PeriodId { get; set; }
        public string EntityId { get; set; }
        public DateTime CompletedDate { get; set; }
        public List<string> Messages { get; set; } = new();
        public List<string> Errors { get; set; } = new();
    }

    /// <summary>
    /// Validation result for period closing.
    /// </summary>
    public class PeriodClosingValidation
    {
        public bool IsReady { get; set; }
        public List<string> Prerequisites { get; set; } = new();
        public List<string> FailedChecks { get; set; } = new();
        public Dictionary<string, object> CheckDetails { get; set; } = new();
    }

    /// <summary>
    /// Result of journal posting operation.
    /// </summary>
    public class PostingResult
    {
        public int EntriesPosted { get; set; }
        public int EntriesFailed { get; set; }
        public List<string> Errors { get; set; } = new();
    }

    /// <summary>
    /// Result of reconciliation operation.
    /// </summary>
    public class ReconciliationResult
    {
        public bool ReconciliationBalance { get; set; }
        public decimal Variance { get; set; }
        public List<UnreconciledItem> UnreconciledItems { get; set; } = new();
    }

    /// <summary>
    /// Status of a period close.
    /// </summary>
    public class PeriodClosingStatus
    {
        public string PeriodId { get; set; }
        public string EntityId { get; set; }
        public PeriodClosingState State { get; set; }
        public DateTime? OpenedDate { get; set; }
        public DateTime? ClosedDate { get; set; }
        public string ClosedBy { get; set; }
        public bool IsLocked { get; set; }
    }

    /// <summary>
    /// Unreconciled item during reconciliation.
    /// </summary>
    public class UnreconciledItem
    {
        public string AccountId { get; set; }
        public decimal ExpectedBalance { get; set; }
        public decimal ActualBalance { get; set; }
        public decimal Variance { get; set; }
    }

    /// <summary>
    /// State of a period.
    /// </summary>
    public enum PeriodClosingState
    {
        Open,
        InProgress,
        Closed,
        Locked,
        Reopened
    }
}
