using Beep.OilandGas.PPDM39.Core;
#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Accounting.Royalty;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.Repositories;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.LifeCycle.Services.Accounting
{
    /// <summary>
    /// LifeCycle accounting service. Delegates sales/AR operations to the PPDM39 data layer
    /// and O&G-specific operations (volumes, costs, royalties) to ProductionAccounting services.
    /// </summary>
    public partial class PPDMAccountingService : IAccountingService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly string _connectionName;
        private readonly Func<Task<string>>? _resolveProductionConnection;
        private readonly ILogger<PPDMAccountingService>? _logger;
        private readonly Beep.OilandGas.Models.Core.Interfaces.IRevenueService? _revenueService;
        private readonly Beep.OilandGas.Models.Core.Interfaces.IRoyaltyService? _royaltyService;
        private readonly Beep.OilandGas.Models.Core.Interfaces.IAllocationService? _allocationService;
        private readonly Microsoft.Extensions.Configuration.IConfiguration? _configuration;
        private readonly Beep.OilandGas.Accounting.Services.CostAllocationService? _costAllocationService;

        public PPDMAccountingService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            string connectionName = "PPDM39",
            ILogger<PPDMAccountingService>? logger = null,
            Beep.OilandGas.Models.Core.Interfaces.IRevenueService? revenueService = null,
            Beep.OilandGas.Models.Core.Interfaces.IRoyaltyService? royaltyService = null,
            Beep.OilandGas.Models.Core.Interfaces.IAllocationService? allocationService = null,
            Microsoft.Extensions.Configuration.IConfiguration? configuration = null,
            Beep.OilandGas.Accounting.Services.CostAllocationService? costAllocationService = null,
            Func<Task<string>>? resolveProductionConnection = null)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _connectionName = connectionName;
            _logger = logger;
            _revenueService = revenueService;
            _royaltyService = royaltyService;
            _allocationService = allocationService;
            _configuration = configuration;
            _costAllocationService = costAllocationService;
            _resolveProductionConnection = resolveProductionConnection;
        }

        public async Task<SalesTransaction> CreateSalesTransactionAsync(CreateSalesTransactionRequest request, string userId, string connectionName = "PPDM39")
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            var connName = connectionName ?? _connectionName;
            var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                typeof(SalesTransaction), connName, "SALES_TRANSACTION");
            var entity = new SalesTransaction
            {
                SALES_TRANSACTION_ID = _defaults.FormatIdForTable("SALES_TRANSACTION", Guid.NewGuid().ToString()),
                RUN_TICKET_ID        = request.RunTicketNumber ?? string.Empty,
                SALES_AGREEMENT_ID   = request.SalesAgreementId ?? string.Empty,
                CUSTOMER_BA_ID       = request.Purchaser ?? string.Empty,
                SALES_DATE           = request.TransactionDate,
                NET_VOLUME           = request.NetVolume,
                PRICE_PER_BARREL     = request.PricePerBarrel,
                TOTAL_AMOUNT         = request.NetVolume * request.PricePerBarrel
            };
            await repo.InsertAsync(entity, userId);
            _logger?.LogInformation("Created sales transaction {Id} for user {UserId}", entity.SALES_TRANSACTION_ID, userId);
            return entity;
        }

        public async Task<SalesTransaction?> GetSalesTransactionAsync(string transactionId, string connectionName = "PPDM39")
        {
            if (string.IsNullOrWhiteSpace(transactionId)) return null;
            var connName = connectionName ?? _connectionName;
            var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                typeof(SalesTransaction), connName, "SALES_TRANSACTION");
            return (await repo.GetByIdAsync(transactionId)) as SalesTransaction;
        }

        public async Task<List<SalesTransaction>> GetSalesTransactionsByDateRangeAsync(DateTime startDate, DateTime endDate, string connectionName = "PPDM39")
        {
            var connName = connectionName ?? _connectionName;
            var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                typeof(SalesTransaction), connName, "SALES_TRANSACTION");
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "SALES_DATE", Operator = ">=", FilterValue = startDate.ToString("yyyy-MM-dd") },
                new AppFilter { FieldName = "SALES_DATE", Operator = "<=", FilterValue = endDate.ToString("yyyy-MM-dd") },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };
            var entities = await repo.GetAsync(filters);
            return entities.OfType<SalesTransaction>().ToList();
        }

        public async Task<List<SalesTransaction>> GetSalesTransactionsByCustomerAsync(string customerBaId, DateTime? startDate = null, DateTime? endDate = null, string connectionName = "PPDM39")
        {
            if (string.IsNullOrWhiteSpace(customerBaId)) return new List<SalesTransaction>();
            var connName = connectionName ?? _connectionName;
            var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                typeof(SalesTransaction), connName, "SALES_TRANSACTION");
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "CUSTOMER_BA_ID", Operator = "=", FilterValue = customerBaId },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };
            if (startDate.HasValue)
                filters.Add(new AppFilter { FieldName = "SALES_DATE", Operator = ">=", FilterValue = startDate.Value.ToString("yyyy-MM-dd") });
            if (endDate.HasValue)
                filters.Add(new AppFilter { FieldName = "SALES_DATE", Operator = "<=", FilterValue = endDate.Value.ToString("yyyy-MM-dd") });
            var entities = await repo.GetAsync(filters);
            return entities.OfType<SalesTransaction>().ToList();
        }

        public async Task<RECEIVABLE> CreateReceivableAsync(CreateReceivableRequest request, string userId, string connectionName = "PPDM39")
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            var connName = connectionName ?? _connectionName;
            var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                typeof(RECEIVABLE), connName, "RECEIVABLE");
            var entity = new RECEIVABLE
            {
                RECEIVABLE_ID   = _defaults.FormatIdForTable("RECEIVABLE", Guid.NewGuid().ToString()),
                TRANSACTION_ID  = request.SalesTransactionId ?? string.Empty,
                CUSTOMER        = request.CustomerBaId ?? string.Empty,
                INVOICE_NUMBER  = request.InvoiceNumber ?? string.Empty,
                INVOICE_DATE    = request.InvoiceDate,
                STATUS          = "OPEN"
            };
            await repo.InsertAsync(entity, userId);
            return entity;
        }

        public async Task<List<RECEIVABLE>> GetReceivablesByCustomerAsync(string customerBaId, string connectionName = "PPDM39")
        {
            if (string.IsNullOrWhiteSpace(customerBaId)) return new List<RECEIVABLE>();
            var connName = connectionName ?? _connectionName;
            var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                typeof(RECEIVABLE), connName, "RECEIVABLE");
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "CUSTOMER", Operator = "=", FilterValue = customerBaId },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };
            var entities = await repo.GetAsync(filters);
            return entities.OfType<RECEIVABLE>().ToList();
        }

        public async Task<List<RECEIVABLE>> GetAllReceivablesAsync(string connectionName = "PPDM39")
        {
            var connName = connectionName ?? _connectionName;
            var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                typeof(RECEIVABLE), connName, "RECEIVABLE");
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };
            var entities = await repo.GetAsync(filters);
            return entities.OfType<RECEIVABLE>().ToList();
        }

        public async Task<JOURNAL_ENTRY> CreateSalesJournalEntryAsync(string salesTransactionId, string userId, string connectionName = "PPDM39")
        {
            if (string.IsNullOrWhiteSpace(salesTransactionId)) throw new ArgumentNullException(nameof(salesTransactionId));
            var connName = connectionName ?? _connectionName;
            var txRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                typeof(SalesTransaction), connName, "SALES_TRANSACTION");
            var tx = (await txRepo.GetByIdAsync(salesTransactionId)) as SalesTransaction;
            if (tx == null)
                throw new KeyNotFoundException($"Sales transaction {salesTransactionId} not found.");

            var jeRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                typeof(JOURNAL_ENTRY), connName, "JOURNAL_ENTRY");
            var entry = new JOURNAL_ENTRY
            {
                JOURNAL_ENTRY_ID = _defaults.FormatIdForTable("JOURNAL_ENTRY", Guid.NewGuid().ToString()),
                ENTRY_NUMBER     = $"JE-{DateTime.UtcNow:yyyyMMddHHmmss}",
                ENTRY_DATE       = DateTime.UtcNow,
                ENTRY_TYPE       = "SALES",
                STATUS           = "POSTED",
                DESCRIPTION      = $"Sales transaction {salesTransactionId}",
                REFERENCE_NUMBER = salesTransactionId,
                SOURCE_MODULE    = "SALES_ACCOUNTING",
                TOTAL_DEBIT      = tx.TOTAL_AMOUNT,
                TOTAL_CREDIT     = tx.TOTAL_AMOUNT
            };
            await jeRepo.InsertAsync(entry, userId);
            return entry;
        }

        public async Task<SalesApprovalResult> ApproveSalesTransactionAsync(string transactionId, string approverId, string connectionName = "PPDM39")
        {
            if (string.IsNullOrWhiteSpace(transactionId)) throw new ArgumentNullException(nameof(transactionId));
            var connName = connectionName ?? _connectionName;
            var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                typeof(SalesTransaction), connName, "SALES_TRANSACTION");
            var tx = (await repo.GetByIdAsync(transactionId)) as SalesTransaction;
            if (tx == null)
                throw new KeyNotFoundException($"Sales transaction {transactionId} not found.");
            await repo.UpdateAsync(tx, approverId);
            return new SalesApprovalResult
            {
                SalesTransactionId = transactionId,
                IsApproved         = true,
                ApproverId         = approverId,
                ApprovalDate       = DateTime.UtcNow
            };
        }

        public async Task<SalesReconciliationResult> ReconcileSalesAsync(SalesReconciliationRequest request, string userId, string connectionName = "PPDM39")
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            var connName = connectionName ?? _connectionName;
            var txs = await GetSalesTransactionsByDateRangeAsync(request.StartDate, request.EndDate, connName);
            decimal totalSales = txs.Sum(t => t.TOTAL_AMOUNT);
            return new SalesReconciliationResult
            {
                ReconciliationId     = Guid.NewGuid().ToString(),
                StartDate            = request.StartDate,
                EndDate              = request.EndDate,
                TotalSalesVolume     = txs.Sum(t => t.NET_VOLUME),
                TotalProductionVolume = totalSales
            };
        }

        public async Task<SalesStatement> GenerateSalesStatementAsync(string customerBaId, DateTime statementDate, string connectionName = "PPDM39")
        {
            if (string.IsNullOrWhiteSpace(customerBaId)) throw new ArgumentNullException(nameof(customerBaId));
            var connName = connectionName ?? _connectionName;
            var txs = await GetSalesTransactionsByCustomerAsync(customerBaId, null, statementDate, connName);
            var statement = new SalesStatement
            {
                StatementId          = Guid.NewGuid().ToString(),
                StatementPeriodStart = txs.Count > 0 ? txs.Min(t => t.SALES_DATE ?? statementDate) : statementDate,
                StatementPeriodEnd   = statementDate,
                Transactions         = txs
            };
            return statement;
        }

        /// <summary>Reconciles measured BBL run tickets against their active allocations, by inclusive calendar dates.</summary>
        public async Task<VolumeReconciliationResult> ReconcileVolumesAsync(string fieldId, DateTime startDate, DateTime endDate, string connectionName = "PPDM39")
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(fieldId);
            if (startDate.Date == DateTime.MinValue.Date || startDate.Date > endDate.Date || endDate.Date == DateTime.MaxValue.Date)
                throw new ArgumentException("A valid inclusive date range is required.");
            if (_allocationService is null)
                throw new InvalidOperationException("The allocation service is not configured.");
            var connName = connectionName ?? _connectionName;
            var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                typeof(RUN_TICKET), connName, "RUN_TICKET");
            var tickets = (await repo.GetAsync(new List<AppFilter>
            {
                new() { FieldName = "FIELD_ID", Operator = "=", FilterValue = fieldId },
                new() { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" },
                new() { FieldName = "TICKET_DATE_TIME", Operator = ">=", FilterValue = startDate.Date.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture) },
                new() { FieldName = "TICKET_DATE_TIME", Operator = "<", FilterValue = endDate.Date.AddDays(1).ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture) }
            })).OfType<RUN_TICKET>().ToList();
            var allocations = new Dictionary<string, List<ALLOCATION_RESULT>>(StringComparer.Ordinal);
            foreach (var id in tickets.Select(ticket => ticket.RUN_TICKET_ID).Where(id => !string.IsNullOrWhiteSpace(id)).Distinct(StringComparer.Ordinal))
                allocations[id] = await _allocationService.GetHistoryAsync(id, connName);
            return RunTicketVolumeReconciler.Reconcile(tickets, allocations);
        }
        public async Task<List<ROYALTY_CALCULATION>> PreviewRoyaltiesAsync(string fieldId, DateTime startDate, DateTime endDate, string userId)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(fieldId);
            ArgumentException.ThrowIfNullOrWhiteSpace(userId);
            if (startDate.Date == DateTime.MinValue.Date || startDate.Date > endDate.Date || endDate.Date == DateTime.MaxValue.Date)
                throw new ArgumentException("A valid inclusive production date range is required.");
            if (_allocationService is null || _royaltyService is null)
                throw new InvalidOperationException("Allocation and royalty services must be configured.");
            var connName = await ResolveProductionConnectionAsync();
            var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                typeof(RUN_TICKET), connName, "RUN_TICKET");
            var tickets = (await repo.GetAsync(new List<AppFilter>
            {
                new() { FieldName = "FIELD_ID", Operator = "=", FilterValue = fieldId },
                new() { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" },
                new() { FieldName = "TICKET_DATE_TIME", Operator = ">=", FilterValue = startDate.Date.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture) },
                new() { FieldName = "TICKET_DATE_TIME", Operator = "<", FilterValue = endDate.Date.AddDays(1).ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture) }
            })).OfType<RUN_TICKET>().ToList();
            if (tickets.Count == 0) throw new InvalidOperationException("No active source tickets exist for the selected field and production dates.");
            var result = new List<ROYALTY_CALCULATION>();
            var seenTickets = new HashSet<string>(StringComparer.Ordinal);
            var seenDetails = new HashSet<string>(StringComparer.Ordinal);
            foreach (var ticket in tickets)
            {
                if (string.IsNullOrWhiteSpace(ticket.RUN_TICKET_ID) || !seenTickets.Add(ticket.RUN_TICKET_ID))
                    throw new InvalidOperationException("Source ticket IDs must be unique and nonempty.");
                var allocations = await _allocationService.GetHistoryAsync(ticket.RUN_TICKET_ID, connName);
                if (allocations.Count != 1 || allocations[0].ALLOCATION_REQUEST_ID != ticket.RUN_TICKET_ID)
                    throw new InvalidOperationException($"Ticket {ticket.RUN_TICKET_ID} requires exactly one active allocation.");
                var allocationId = allocations[0].ALLOCATION_RESULT_ID;
                var details = await _allocationService.GetDetailsAsync(allocationId, connName);
                if (details.Count == 0) throw new InvalidOperationException($"Allocation {allocationId} has no details.");
                foreach (var detail in details)
                {
                    if (string.IsNullOrWhiteSpace(detail.ALLOCATION_DETAIL_ID) || !seenDetails.Add(detail.ALLOCATION_DETAIL_ID) ||
                        detail.ALLOCATION_RESULT_ID != allocationId)
                        throw new InvalidOperationException("Allocation detail identity or ownership is ambiguous.");
                    result.Add(await _royaltyService.PreviewAsync(detail, userId, connName));
                }
            }
            return result;
        }
    }
}
