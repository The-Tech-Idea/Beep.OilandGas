using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.DTOs.Accounting;
using Beep.OilandGas.PPDM39.DataManagement.Core.Common;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.DataBase;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.PPDM.Models;

namespace Beep.OilandGas.ProductionAccounting.Accounting
{
    /// <summary>
    /// Service for managing sales accounting operations.
    /// Uses IDataSource directly for database operations.
    /// </summary>
    public class AccountingService : IAccountingService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly ILogger<AccountingService>? _logger;
        private readonly string _connectionName;
        private readonly IJournalEntryService _journalEntryService;

        private const string SALES_TRANSACTION_TABLE = "SALES_TRANSACTION";
        private const string RECEIVABLE_TABLE = "RECEIVABLE";

        public AccountingService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            ILoggerFactory? loggerFactory,
            IJournalEntryService journalEntryService,
            string connectionName = "PPDM39")
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _logger = loggerFactory?.CreateLogger<AccountingService>();
            _journalEntryService = journalEntryService ?? throw new ArgumentNullException(nameof(journalEntryService));
            _connectionName = connectionName ?? "PPDM39";
        }

        /// <summary>
        /// Creates a sales transaction.
        /// </summary>
        public async Task<SALES_TRANSACTION> CreateSalesTransactionAsync(CreateSalesTransactionRequest request, string userId, string? connectionName = null)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            var entity = new SALES_TRANSACTION
            {
                SALES_TRANSACTION_ID = Guid.NewGuid().ToString(),
                RUN_TICKET_ID = request.RunTicketId,
                SALES_AGREEMENT_ID = request.SalesAgreementId,
                CUSTOMER_BA_ID = request.CustomerBaId,
                SALES_DATE = request.SalesDate,
                NET_VOLUME = request.NetVolume,
                PRICE_PER_BARREL = request.PricePerBarrel,
                TOTAL_AMOUNT = request.NetVolume * request.PricePerBarrel,
                TOTAL_COSTS = request.TotalCosts,
                TOTAL_TAXES = request.TotalTaxes,
                NET_REVENUE = (request.NetVolume * request.PricePerBarrel) - (request.TotalCosts ?? 0) - (request.TotalTaxes ?? 0),
                STATUS = request.Status ?? "Pending",
                APPROVAL_STATUS = "Pending",
                ACTIVE_IND = "Y"
            };

            if (entity is IPPDMEntity ppdmEntity)
            {
                _commonColumnHandler.PrepareForInsert(ppdmEntity, userId);
            }

            var result = dataSource.InsertEntity(SALES_TRANSACTION_TABLE, entity);
            if (result != null && result.Errors != null && result.Errors.Count > 0)
            {
                var errorMessage = string.Join("; ", result.Errors.Select(e => e.Message));
                _logger?.LogError("Failed to create sales transaction: {Error}", errorMessage);
                throw new InvalidOperationException($"Failed to save sales transaction: {errorMessage}");
            }

            _logger?.LogDebug("Created sales transaction {TransactionId} for customer {CustomerId}",
                entity.SALES_TRANSACTION_ID, request.CustomerBaId);

            return entity;
        }

        /// <summary>
        /// Gets a sales transaction by ID.
        /// </summary>
        public async Task<SALES_TRANSACTION?> GetSalesTransactionAsync(string transactionId, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(transactionId))
                throw new ArgumentException("Transaction ID is required.", nameof(transactionId));

            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "SALES_TRANSACTION_ID", Operator = "=", FilterValue = transactionId },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            var results = await dataSource.GetEntityAsync(SALES_TRANSACTION_TABLE, filters);
            return results?.Cast<SALES_TRANSACTION>().FirstOrDefault();
        }

        /// <summary>
        /// Gets sales transactions by date range.
        /// </summary>
        public async Task<List<SALES_TRANSACTION>> GetSalesTransactionsByDateRangeAsync(DateTime startDate, DateTime endDate, string? connectionName = null)
        {
            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "SALES_DATE", Operator = ">=", FilterValue = startDate.ToString("yyyy-MM-dd") },
                new AppFilter { FieldName = "SALES_DATE", Operator = "<=", FilterValue = endDate.ToString("yyyy-MM-dd") },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            var results = await dataSource.GetEntityAsync(SALES_TRANSACTION_TABLE, filters);
            if (results == null)
                return new List<SALES_TRANSACTION>();

            return results.Cast<SALES_TRANSACTION>().OrderByDescending(t => t.SALES_DATE).ToList();
        }

        /// <summary>
        /// Gets sales transactions by customer.
        /// </summary>
        public async Task<List<SALES_TRANSACTION>> GetSalesTransactionsByCustomerAsync(string customerBaId, DateTime? startDate, DateTime? endDate, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(customerBaId))
                throw new ArgumentException("Customer BA ID is required.", nameof(customerBaId));

            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "CUSTOMER_BA_ID", Operator = "=", FilterValue = customerBaId },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            if (startDate.HasValue)
            {
                filters.Add(new AppFilter { FieldName = "SALES_DATE", Operator = ">=", FilterValue = startDate.Value.ToString("yyyy-MM-dd") });
            }

            if (endDate.HasValue)
            {
                filters.Add(new AppFilter { FieldName = "SALES_DATE", Operator = "<=", FilterValue = endDate.Value.ToString("yyyy-MM-dd") });
            }

            var results = await dataSource.GetEntityAsync(SALES_TRANSACTION_TABLE, filters);
            if (results == null)
                return new List<SALES_TRANSACTION>();

            return results.Cast<SALES_TRANSACTION>().OrderByDescending(t => t.SALES_DATE).ToList();
        }

        /// <summary>
        /// Creates a receivable.
        /// </summary>
        public async Task<RECEIVABLE> CreateReceivableAsync(CreateReceivableRequest request, string userId, string? connectionName = null)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            var entity = new RECEIVABLE
            {
                RECEIVABLE_ID = Guid.NewGuid().ToString(),
                SALES_TRANSACTION_ID = request.SalesTransactionId,
                CUSTOMER_BA_ID = request.CustomerBaId,
                INVOICE_NUMBER = request.InvoiceNumber,
                INVOICE_DATE = request.InvoiceDate,
                DUE_DATE = request.DueDate,
                ORIGINAL_AMOUNT = request.OriginalAmount,
                AMOUNT_PAID = 0,
                OUTSTANDING_BALANCE = request.OriginalAmount,
                STATUS = "Open",
                ACTIVE_IND = "Y"
            };

            if (entity is IPPDMEntity ppdmEntity)
            {
                _commonColumnHandler.PrepareForInsert(ppdmEntity, userId);
            }

            var result = dataSource.InsertEntity(RECEIVABLE_TABLE, entity);
            if (result != null && result.Errors != null && result.Errors.Count > 0)
            {
                var errorMessage = string.Join("; ", result.Errors.Select(e => e.Message));
                _logger?.LogError("Failed to create receivable: {Error}", errorMessage);
                throw new InvalidOperationException($"Failed to save receivable: {errorMessage}");
            }

            _logger?.LogDebug("Created receivable {ReceivableId} for customer {CustomerId}",
                entity.RECEIVABLE_ID, request.CustomerBaId);

            return entity;
        }

        /// <summary>
        /// Gets receivables by customer.
        /// </summary>
        public async Task<List<RECEIVABLE>> GetReceivablesByCustomerAsync(string customerBaId, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(customerBaId))
                throw new ArgumentException("Customer BA ID is required.", nameof(customerBaId));

            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "CUSTOMER_BA_ID", Operator = "=", FilterValue = customerBaId },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            var results = await dataSource.GetEntityAsync(RECEIVABLE_TABLE, filters);
            if (results == null)
                return new List<RECEIVABLE>();

            return results.Cast<RECEIVABLE>().OrderByDescending(r => r.DUE_DATE).ToList();
        }

        /// <summary>
        /// Gets all receivables.
        /// </summary>
        public async Task<List<RECEIVABLE>> GetAllReceivablesAsync(string? connectionName = null)
        {
            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            var results = await dataSource.GetEntityAsync(RECEIVABLE_TABLE, filters);
            if (results == null)
                return new List<RECEIVABLE>();

            return results.Cast<RECEIVABLE>().OrderByDescending(r => r.DUE_DATE).ToList();
        }

        /// <summary>
        /// Creates a sales journal entry.
        /// </summary>
        public async Task<JOURNAL_ENTRY> CreateSalesJournalEntryAsync(string salesTransactionId, string userId, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(salesTransactionId))
                throw new ArgumentException("Sales transaction ID is required.", nameof(salesTransactionId));

            var transaction = await GetSalesTransactionAsync(salesTransactionId, connectionName);
            if (transaction == null)
                throw new ArgumentException($"Sales transaction not found: {salesTransactionId}", nameof(salesTransactionId));

            // Convert entity to model for journal entry generation
            var salesTransactionModel = ConvertToSalesTransactionModel(transaction);

            // Generate journal entries using static helper
            var journalEntries = SalesJournalEntryGenerator.CreateEntries(salesTransactionModel);


            // Convert to journal entry lines
            var lines = journalEntries.Select(e => new JournalEntryLineData
            {
                GlAccountId = e.AccountCode, // In production, would map account code to GL account ID
                DebitAmount = e.DebitAmount > 0 ? e.DebitAmount : null,
                CreditAmount = e.CreditAmount > 0 ? e.CreditAmount : null,
                Description = e.Description
            }).ToList();

            // Create journal entry request
            var request = new CreateJournalEntryRequest
            {
                EntryNumber = $"SALES-{DateTime.UtcNow:yyyyMMdd}-{salesTransactionId.Substring(0, 8)}",
                EntryDate = transaction.SALES_DATE ?? DateTime.UtcNow,
                EntryType = "Sales",
                Description = $"Sales transaction {salesTransactionId}",
                ReferenceNumber = salesTransactionId,
                SourceModule = "Accounting",
                Lines = lines
            };

            var journalEntry = await _journalEntryService.CreateJournalEntryAsync(request, userId, connectionName);

            _logger?.LogDebug("Created sales journal entry {EntryId} for transaction {TransactionId}",
                journalEntry.JOURNAL_ENTRY_ID, salesTransactionId);

            return journalEntry;
        }

        /// <summary>
        /// Approves a sales transaction.
        /// </summary>
        public async Task<SalesApprovalResult> ApproveSalesTransactionAsync(string transactionId, string approverId, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(transactionId))
                throw new ArgumentException("Transaction ID is required.", nameof(transactionId));
            if (string.IsNullOrEmpty(approverId))
                throw new ArgumentException("Approver ID is required.", nameof(approverId));

            var transaction = await GetSalesTransactionAsync(transactionId, connectionName);
            if (transaction == null)
                throw new ArgumentException($"Sales transaction not found: {transactionId}", nameof(transactionId));

            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            transaction.APPROVAL_STATUS = "Approved";
            transaction.STATUS = "Approved";

            if (transaction is IPPDMEntity ppdmEntity)
            {
                _commonColumnHandler.PrepareForUpdate(ppdmEntity, approverId);
            }

            var result = dataSource.UpdateEntity(SALES_TRANSACTION_TABLE, transaction);
            if (result != null && result.Errors != null && result.Errors.Count > 0)
            {
                var errorMessage = string.Join("; ", result.Errors.Select(e => e.Message));
                _logger?.LogError("Failed to approve sales transaction {TransactionId}: {Error}", transactionId, errorMessage);
                throw new InvalidOperationException($"Failed to approve sales transaction: {errorMessage}");
            }

            _logger?.LogDebug("Approved sales transaction {TransactionId} by {ApproverId}", transactionId, approverId);

            return new SalesApprovalResult
            {
                SalesTransactionId = transactionId,
                IsApproved = true,
                ApproverId = approverId,
                ApprovalDate = DateTime.UtcNow,
                Status = "Approved"
            };
        }

        /// <summary>
        /// Reconciles sales transactions.
        /// </summary>
        public async Task<SalesReconciliationResult> ReconcileSalesAsync(SalesReconciliationRequest request, string userId, string? connectionName = null)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var connName = connectionName ?? _connectionName;
            var salesTransactions = await GetSalesTransactionsByDateRangeAsync(request.StartDate, request.EndDate, connName);

            if (!string.IsNullOrEmpty(request.CustomerBaId))
            {
                salesTransactions = salesTransactions.Where(t => t.CUSTOMER_BA_ID == request.CustomerBaId).ToList();
            }

            var totalSalesVolume = salesTransactions.Sum(t => t.NET_VOLUME ?? 0);
            var totalSalesRevenue = salesTransactions.Sum(t => t.TOTAL_AMOUNT ?? 0);

            // In a full implementation, would query production data for comparison
            var totalProductionVolume = totalSalesVolume; // Placeholder
            var totalProductionRevenue = totalSalesRevenue; // Placeholder

            var issues = new List<SalesReconciliationIssue>();

            var volumeDifference = totalProductionVolume - totalSalesVolume;
            var revenueDifference = totalProductionRevenue - totalSalesRevenue;

            if (Math.Abs(volumeDifference) > 0.01m)
            {
                issues.Add(new SalesReconciliationIssue
                {
                    IssueType = "VolumeMismatch",
                    Description = $"Volume difference: {volumeDifference} barrels",
                    Volume = volumeDifference
                });
            }

            if (Math.Abs(revenueDifference) > 0.01m)
            {
                issues.Add(new SalesReconciliationIssue
                {
                    IssueType = "RevenueMismatch",
                    Description = $"Revenue difference: ${revenueDifference}",
                    Amount = revenueDifference
                });
            }

            _logger?.LogDebug("Reconciled sales transactions from {StartDate} to {EndDate}. Issues: {IssueCount}",
                request.StartDate, request.EndDate, issues.Count);

            return new SalesReconciliationResult
            {
                ReconciliationId = Guid.NewGuid().ToString(),
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                TotalProductionVolume = totalProductionVolume,
                TotalSalesVolume = totalSalesVolume,
                VolumeDifference = volumeDifference,
                TotalProductionRevenue = totalProductionRevenue,
                TotalSalesRevenue = totalSalesRevenue,
                RevenueDifference = revenueDifference,
                Issues = issues,
                IsReconciled = issues.Count == 0
            };
        }

        /// <summary>
        /// Generates a sales statement.
        /// </summary>
        public async Task<SalesStatement> GenerateSalesStatementAsync(string customerBaId, DateTime statementDate, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(customerBaId))
                throw new ArgumentException("Customer BA ID is required.", nameof(customerBaId));

            var connName = connectionName ?? _connectionName;
            var startDate = new DateTime(statementDate.Year, statementDate.Month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            var transactions = await GetSalesTransactionsByCustomerAsync(customerBaId, startDate, endDate, connName);

            // Convert entities to models for statement generation
            var transactionModels = transactions.Select(ConvertToSalesTransactionModel).ToList();

            var statement = new SalesStatement
            {
                StatementId = Guid.NewGuid().ToString(),
                StatementPeriodStart = startDate,
                StatementPeriodEnd = endDate,
                Transactions = transactionModels,
                Summary = new SalesSummary
                {
                    TotalNetVolume = transactionModels.Sum(t => t.NetVolume),
                    AveragePricePerBarrel = transactionModels.Any()
                        ? transactionModels.Average(t => t.PricePerBarrel)
                        : 0,
                    TotalGrossRevenue = transactionModels.Sum(t => t.TotalValue),
                    TotalCosts = transactionModels.Sum(t => t.Costs.TotalCosts),
                    TotalTaxes = transactionModels.Sum(t => t.Taxes.Sum(tax => tax.Amount)),
                    TransactionCount = transactionModels.Count
                }
            };

            _logger?.LogDebug("Generated sales statement for customer {CustomerId} for period {StartDate} to {EndDate}",
                customerBaId, startDate, endDate);

            return statement;
        }

        // Helper methods

        private SalesTransaction ConvertToSalesTransactionModel(SALES_TRANSACTION entity)
        {
            // In a full implementation, would load related data (costs, taxes, etc.)
            return new SalesTransaction
            {
                TransactionId = entity.SALES_TRANSACTION_ID,
                SalesAgreementId = entity.SALES_AGREEMENT_ID,
                RunTicketNumber = entity.RUN_TICKET_ID ?? string.Empty,
                TransactionDate = entity.SALES_DATE ?? DateTime.UtcNow,
                Purchaser = entity.CUSTOMER_BA_ID ?? string.Empty,
                NetVolume = entity.NET_VOLUME ?? 0,
                PricePerBarrel = entity.PRICE_PER_BARREL ?? 0,
                Costs = new ProductionMarketingCosts { NetVolume = entity.NET_VOLUME ?? 0 },
                Taxes = new List<ProductionTax>()
            };
        }
    }
}
