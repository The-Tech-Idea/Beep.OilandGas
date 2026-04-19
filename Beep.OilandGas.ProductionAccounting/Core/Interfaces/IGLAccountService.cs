using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.Models.Data.Accounting;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Service interface for GL account operations.
    /// </summary>
    public interface IGLAccountService
    {
        /// <summary>
        /// Creates a new GL account.
        /// </summary>
        Task<GL_ACCOUNT> CreateAccountAsync(CreateGLAccountRequest request, string userId, string? connectionName = null);
        
        /// <summary>
        /// Gets a GL account by ID.
        /// </summary>
        Task<GL_ACCOUNT?> GetAccountAsync(string accountId, string? connectionName = null);
        
        /// <summary>
        /// Gets a GL account by account number.
        /// </summary>
        Task<GL_ACCOUNT?> GetAccountByNumberAsync(string accountNumber, string? connectionName = null);
        
        /// <summary>
        /// Gets accounts by type.
        /// </summary>
        Task<List<GL_ACCOUNT>> GetAccountsByTypeAsync(string accountType, string? connectionName = null);
        
        /// <summary>
        /// Gets account hierarchy (parent-child relationships).
        /// </summary>
        Task<List<GL_ACCOUNT>> GetAccountHierarchyAsync(string? connectionName = null);
        
        /// <summary>
        /// Updates a GL account.
        /// </summary>
        Task<GL_ACCOUNT> UpdateAccountAsync(UpdateGLAccountRequest request, string userId, string? connectionName = null);
        
        /// <summary>
        /// Deletes a GL account (soft delete by setting ACTIVE_IND = 'N').
        /// </summary>
        Task<bool> DeleteAccountAsync(string accountId, string userId, string? connectionName = null);
        
        /// <summary>
        /// Gets account balance summary.
        /// </summary>
        Task<AccountBalanceSummary> GetAccountBalanceAsync(string accountId, DateTime? asOfDate, string? connectionName = null);
        
        /// <summary>
        /// Reconciles an account.
        /// </summary>
        Task<AccountReconciliationResult> ReconcileAccountAsync(string accountId, DateTime reconciliationDate, string userId, string? connectionName = null);
        
        /// <summary>
        /// Gets accounts requiring reconciliation.
        /// </summary>
        Task<List<GL_ACCOUNT>> GetAccountsRequiringReconciliationAsync(string? connectionName = null);
    }
}




