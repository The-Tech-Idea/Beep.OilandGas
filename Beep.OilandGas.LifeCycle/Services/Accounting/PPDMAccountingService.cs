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
    /// Stub implementation of IAccountingService for the LifeCycle layer.
    /// Delegates to the PPDM39 data layer for persistence.
    /// </summary>
    public class PPDMAccountingService : IAccountingService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly string _connectionName;
        private readonly ILogger<PPDMAccountingService>? _logger;

        public PPDMAccountingService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            string connectionName = "PPDM39",
            ILogger<PPDMAccountingService>? logger = null)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _connectionName = connectionName;
            _logger = logger;
        }

        public async Task<SalesTransaction> CreateSalesTransactionAsync(CreateSalesTransactionRequest request, string userId, string? connectionName = null)
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

        public async Task<SalesTransaction?> GetSalesTransactionAsync(string transactionId, string? connectionName = null)
        {
            if (string.IsNullOrWhiteSpace(transactionId)) return null;
            var connName = connectionName ?? _connectionName;
            var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                typeof(SalesTransaction), connName, "SALES_TRANSACTION");
            return (await repo.GetByIdAsync(transactionId)) as SalesTransaction;
        }

        public async Task<List<SalesTransaction>> GetSalesTransactionsByDateRangeAsync(DateTime startDate, DateTime endDate, string? connectionName = null)
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

        public async Task<List<SalesTransaction>> GetSalesTransactionsByCustomerAsync(string customerBaId, DateTime? startDate = null, DateTime? endDate = null, string? connectionName = null)
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

        public async Task<RECEIVABLE> CreateReceivableAsync(CreateReceivableRequest request, string userId, string? connectionName = null)
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

        public async Task<List<RECEIVABLE>> GetReceivablesByCustomerAsync(string customerBaId, string? connectionName = null)
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

        public async Task<List<RECEIVABLE>> GetAllReceivablesAsync(string? connectionName = null)
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

        public async Task<JOURNAL_ENTRY> CreateSalesJournalEntryAsync(string salesTransactionId, string userId, string? connectionName = null)
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

        public async Task<SalesApprovalResult> ApproveSalesTransactionAsync(string transactionId, string approverId, string? connectionName = null)
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

        public async Task<SalesReconciliationResult> ReconcileSalesAsync(SalesReconciliationRequest request, string userId, string? connectionName = null)
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

        public async Task<SalesStatement> GenerateSalesStatementAsync(string customerBaId, DateTime statementDate, string? connectionName = null)
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

        public Task<VolumeReconciliationResult> ReconcileVolumesAsync(string fieldId, DateTime startDate, DateTime endDate, string? connectionName = null)
            => Task.FromResult(new VolumeReconciliationResult
            {
                Status = ReconciliationStatus.Matched,
                FieldProductionVolume = 0m,
                ALLOCATED_VOLUME = 0m,
                Discrepancy = 0m,
                DiscrepancyPercentage = 0m,
                OilVolume = new VolumeBreakdownResult(),
                GasVolume = new VolumeBreakdownResult(),
                Issues = new List<VolumeReconciliationIssue>()
            });

        public Task<CostAllocationComputationResult> AllocateCostsAsync(string fieldId, DateTime startDate, DateTime endDate, CostAllocationMethod allocationMethod, string? connectionName = null)
            => Task.FromResult(new CostAllocationComputationResult
            {
                TotalOperatingCosts = 0m,
                TotalCapitalCosts = 0m,
                AllocationDetails = new List<CostAllocationBreakdown>()
            });

        public Task<ProductionRoyaltyCalculationResult> CalculateRoyaltiesAsync(string fieldId, DateTime startDate, DateTime endDate, string? poolId = null, string? connectionName = null)
            => Task.FromResult(new ProductionRoyaltyCalculationResult
            {
                GrossOilVolume = 0m,
                GrossGasVolume = 0m,
                RoyaltyOilVolume = 0m,
                RoyaltyGasVolume = 0m,
                OilRoyaltyRate = 0m,
                GasRoyaltyRate = 0m
            });

        public async Task SaveRoyaltyCalculationAsync(ROYALTY_CALCULATION calculation, string userId, string? connectionName = null)
        {
            if (calculation == null) throw new ArgumentNullException(nameof(calculation));
            var connName = connectionName ?? _connectionName;
            var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                typeof(ROYALTY_CALCULATION), connName, "ROYALTY_CALCULATION");
            if (string.IsNullOrWhiteSpace(calculation.ROYALTY_CALCULATION_ID))
                calculation.ROYALTY_CALCULATION_ID = _defaults.FormatIdForTable("ROYALTY_CALCULATION", Guid.NewGuid().ToString());
            await repo.InsertAsync(calculation, userId);
        }

        public async Task<List<ROYALTY_CALCULATION>> GetRoyaltyCalculationsAsync(string? fieldId = null, string? poolId = null, DateTime? startDate = null, DateTime? endDate = null, string? connectionName = null)
        {
            var connName = connectionName ?? _connectionName;
            var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                typeof(ROYALTY_CALCULATION), connName, "ROYALTY_CALCULATION");
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };
            if (!string.IsNullOrWhiteSpace(fieldId))
                filters.Add(new AppFilter { FieldName = "PROPERTY_OR_LEASE_ID", Operator = "=", FilterValue = fieldId });
            if (!string.IsNullOrWhiteSpace(poolId))
                filters.Add(new AppFilter { FieldName = "ROYALTY_INTEREST_ID", Operator = "=", FilterValue = poolId });
            if (startDate.HasValue)
                filters.Add(new AppFilter { FieldName = "CALCULATION_DATE", Operator = ">=", FilterValue = startDate.Value.ToString("yyyy-MM-dd") });
            if (endDate.HasValue)
                filters.Add(new AppFilter { FieldName = "CALCULATION_DATE", Operator = "<=", FilterValue = endDate.Value.ToString("yyyy-MM-dd") });
            var entities = await repo.GetAsync(filters);
            return entities.OfType<ROYALTY_CALCULATION>().ToList();
        }
    }
}
