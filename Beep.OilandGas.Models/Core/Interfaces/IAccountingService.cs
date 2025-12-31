using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.DTOs.Accounting;
using AccountingReceivable = Beep.OilandGas.Models.Data.Accounting.RECEIVABLE;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Service interface for sales accounting operations.
    /// </summary>
    public interface IAccountingService
    {
        /// <summary>
        /// Creates a sales transaction.
        /// </summary>
        Task<SALES_TRANSACTION> CreateSalesTransactionAsync(CreateSalesTransactionRequest request, string userId, string? connectionName = null);
        
        /// <summary>
        /// Gets a sales transaction by ID.
        /// </summary>
        Task<SALES_TRANSACTION?> GetSalesTransactionAsync(string transactionId, string? connectionName = null);
        
        /// <summary>
        /// Gets sales transactions by date range.
        /// </summary>
        Task<List<SALES_TRANSACTION>> GetSalesTransactionsByDateRangeAsync(DateTime startDate, DateTime endDate, string? connectionName = null);
        
        /// <summary>
        /// Gets sales transactions by customer.
        /// </summary>
        Task<List<SALES_TRANSACTION>> GetSalesTransactionsByCustomerAsync(string customerBaId, DateTime? startDate, DateTime? endDate, string? connectionName = null);
        
        /// <summary>
        /// Creates a receivable.
        /// </summary>
        Task<AccountingReceivable> CreateReceivableAsync(CreateReceivableRequest request, string userId, string? connectionName = null);
        
        /// <summary>
        /// Gets receivables by customer.
        /// </summary>
        Task<List<AccountingReceivable>> GetReceivablesByCustomerAsync(string customerBaId, string? connectionName = null);
        
        /// <summary>
        /// Gets all receivables.
        /// </summary>
        Task<List<AccountingReceivable>> GetAllReceivablesAsync(string? connectionName = null);
        
        /// <summary>
        /// Creates a sales journal entry.
        /// </summary>
        Task<JOURNAL_ENTRY> CreateSalesJournalEntryAsync(string salesTransactionId, string userId, string? connectionName = null);
        
        /// <summary>
        /// Approves a sales transaction.
        /// </summary>
        Task<SalesApprovalResult> ApproveSalesTransactionAsync(string transactionId, string approverId, string? connectionName = null);
        
        /// <summary>
        /// Reconciles sales transactions.
        /// </summary>
        Task<SalesReconciliationResult> ReconcileSalesAsync(SalesReconciliationRequest request, string userId, string? connectionName = null);
        
        /// <summary>
        /// Generates a sales statement.
        /// </summary>
        Task<SalesStatement> GenerateSalesStatementAsync(string customerBaId, DateTime statementDate, string? connectionName = null);
    }
}
