using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data;

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
    

    /// <summary>
    /// Variance identified during reconciliation.
    /// </summary>
    

    /// <summary>
    /// Intercompany reconciliation result.
    /// </summary>
    

    /// <summary>
    /// Bank reconciliation result.
    /// </summary>
    

    /// <summary>
    /// Aging analysis bucket.
    /// </summary>
    

    /// <summary>
    /// Aging bucket detail.
    /// </summary>
    

    /// <summary>
    /// History entry for reconciliation.
    /// </summary>
    

    /// <summary>
    /// Unmatched transaction during reconciliation.
    /// </summary>
    

    /// <summary>
    /// Outstanding item (check, deposit) in bank reconciliation.
    /// </summary>
    

    /// <summary>
    /// Type of variance.
    /// </summary>

}

