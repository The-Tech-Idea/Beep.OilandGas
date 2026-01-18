using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.ProductionAccounting;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Service interface for account reconciliation operations.
    /// Manages account-to-subledger and intercompany reconciliation.
    /// </summary>
    public interface IAccountReconciliationService
    {
        /// <summary>
        /// Reconciles an account against its subledger.
        /// </summary>
        Task<ReconciliationSummary> ReconcileAccountAsync(string accountId, DateTime periodEnd, string? connectionName = null);

        /// <summary>
        /// Identifies variances between GL account and subledger.
        /// </summary>
        Task<List<ReconciliationVariance>> IdentifyVariancesAsync(
            string accountId, 
            DateTime periodStart, 
            DateTime periodEnd, 
            string? connectionName = null);

        /// <summary>
        /// Performs intercompany reconciliation.
        /// </summary>
        Task<IntercompanyReconciliation> ReconcileIntercompanyAsync(
            string company1Id, 
            string company2Id, 
            DateTime periodEnd, 
            string? connectionName = null);

        /// <summary>
        /// Performs bank reconciliation for cash accounts.
        /// </summary>
        Task<BankReconciliation> ReconciledBankAsync(
            string cashAccountId, 
            DateTime periodEnd, 
            decimal bankBalance, 
            string? connectionName = null);

        /// <summary>
        /// Resolves a reconciliation variance.
        /// </summary>
        Task<bool> ResolveVarianceAsync(
            string varianceId, 
            string resolutionType, 
            string notes, 
            string userId, 
            string? connectionName = null);

        /// <summary>
        /// Gets aging analysis for accounts receivable/payable.
        /// </summary>
        Task<AgingAnalysis> GetAgingAnalysisAsync(
            string accountId, 
            DateTime asOfDate, 
            int[] agingBuckets, 
            string? connectionName = null);

        /// <summary>
        /// Gets reconciliation history for an account.
        /// </summary>
        Task<List<ReconciliationHistory>> GetReconciliationHistoryAsync(
            string accountId, 
            DateTime? startDate, 
            DateTime? endDate, 
            string? connectionName = null);
    }

    /// <summary>
    /// Summary of reconciliation for an account.
    /// </summary>
    public class ReconciliationSummary
    {
        public string AccountId { get; set; }
        public DateTime PeriodEnd { get; set; }
        public decimal GLBalance { get; set; }
        public decimal SubledgerBalance { get; set; }
        public decimal Variance { get; set; }
        public bool IsReconciled { get; set; }
        public List<ReconciliationVariance> Variances { get; set; } = new();
    }

    /// <summary>
    /// Variance identified during reconciliation.
    /// </summary>
    public class ReconciliationVariance
    {
        public string VarianceId { get; set; }
        public string AccountId { get; set; }
        public DateTime TransactionDate { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public string GLReference { get; set; }
        public string SubledgerReference { get; set; }
        public ReconciliationVarianceType Type { get; set; }
        public string Status { get; set; } // Open, Resolved, Investigated
    }

    /// <summary>
    /// Intercompany reconciliation result.
    /// </summary>
    public class IntercompanyReconciliation
    {
        public string Company1Id { get; set; }
        public string Company2Id { get; set; }
        public DateTime PeriodEnd { get; set; }
        public decimal Company1Amount { get; set; }
        public decimal Company2Amount { get; set; }
        public decimal Variance { get; set; }
        public bool IsReconciled { get; set; }
        public List<UnmatchedTransaction> UnmatchedTransactions { get; set; } = new();
    }

    /// <summary>
    /// Bank reconciliation result.
    /// </summary>
    public class BankReconciliation
    {
        public string CashAccountId { get; set; }
        public DateTime PeriodEnd { get; set; }
        public decimal GLBalance { get; set; }
        public decimal BankBalance { get; set; }
        public decimal OutstandingDeposits { get; set; }
        public decimal OutstandingChecks { get; set; }
        public decimal ReconciledBalance { get; set; }
        public bool IsReconciled { get; set; }
        public List<OutstandingItem> OutstandingItems { get; set; } = new();
    }

    /// <summary>
    /// Aging analysis bucket.
    /// </summary>
    public class AgingAnalysis
    {
        public string AccountId { get; set; }
        public DateTime AsOfDate { get; set; }
        public List<AgingBucket> Buckets { get; set; } = new();
        public decimal TotalAmount { get; set; }
    }

    /// <summary>
    /// Aging bucket detail.
    /// </summary>
    public class AgingBucket
    {
        public string BucketName { get; set; }
        public int DaysMin { get; set; }
        public int DaysMax { get; set; }
        public decimal Amount { get; set; }
        public decimal PercentageOfTotal { get; set; }
        public List<string> Items { get; set; } = new();
    }

    /// <summary>
    /// History entry for reconciliation.
    /// </summary>
    public class ReconciliationHistory
    {
        public string ReconciliationId { get; set; }
        public DateTime ReconciliationDate { get; set; }
        public DateTime PeriodEnd { get; set; }
        public string PerformedBy { get; set; }
        public decimal VarianceAmount { get; set; }
        public bool WasReconciled { get; set; }
    }

    /// <summary>
    /// Unmatched transaction during reconciliation.
    /// </summary>
    public class UnmatchedTransaction
    {
        public string TransactionId { get; set; }
        public DateTime TransactionDate { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
    }

    /// <summary>
    /// Outstanding item (check, deposit) in bank reconciliation.
    /// </summary>
    public class OutstandingItem
    {
        public string ItemId { get; set; }
        public DateTime ItemDate { get; set; }
        public decimal Amount { get; set; }
        public string Type { get; set; } // Check, Deposit
        public int DaysOutstanding { get; set; }
    }

    /// <summary>
    /// Type of variance.
    /// </summary>
    public enum ReconciliationVarianceType
    {
        Timing,
        Amount,
        Missing,
        Extra,
        Description,
        Other
    }
}
