#nullable enable

using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.DTOs.Accounting;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Service for managing sales accounting operations.
    /// Handles sales transactions, receivables, and accounting workflows.
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
        /// Gets sales transactions within a date range.
        /// </summary>
        Task<List<SALES_TRANSACTION>> GetSalesTransactionsByDateRangeAsync(DateTime startDate, DateTime endDate, string? connectionName = null);

        /// <summary>
        /// Gets sales transactions for a specific customer.
        /// </summary>
        Task<List<SALES_TRANSACTION>> GetSalesTransactionsByCustomerAsync(string customerBaId, DateTime? startDate = null, DateTime? endDate = null, string? connectionName = null);

        /// <summary>
        /// Creates a receivable record.
        /// </summary>
        Task<RECEIVABLE> CreateReceivableAsync(CreateReceivableRequest request, string userId, string? connectionName = null);

        /// <summary>
        /// Gets receivables for a specific customer.
        /// </summary>
        Task<List<RECEIVABLE>> GetReceivablesByCustomerAsync(string customerBaId, string? connectionName = null);

        /// <summary>
        /// Gets all receivables.
        /// </summary>
        Task<List<RECEIVABLE>> GetAllReceivablesAsync(string? connectionName = null);

        /// <summary>
        /// Creates a journal entry for a sales transaction.
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
        /// Generates a sales statement for a customer.
        /// </summary>
        Task<SalesStatement> GenerateSalesStatementAsync(string customerBaId, DateTime statementDate, string? connectionName = null);
    }
}
