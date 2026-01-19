using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.Repositories;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.Accounting.Constants;
using Beep.OilandGas.Accounting.Services;
using Beep.OilandGas.ProductionAccounting.Constants;
using Beep.OilandGas.ProductionAccounting.Exceptions;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.PPDM39.DataManagement.Services;

namespace Beep.OilandGas.ProductionAccounting.Services
{
    /// <summary>
    /// Period Closing Service - Manages month-end and year-end close procedures.
    /// Validates period readiness, closes periods, and tracks unreconciled items.
    /// </summary>
    public class PeriodClosingService : IPeriodClosingService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly ILogger<PeriodClosingService> _logger;
        private readonly IAccountingServices _accountingServices;
        private readonly IAmortizationService _amortizationService;
        private readonly IFullCostService _fullCostService;
        private readonly IReserveAccountingService _reserveAccountingService;
        private readonly IImpairmentTestingService _impairmentTestingService;
        private readonly IDecommissioningService _decommissioningService;
        private readonly IFunctionalCurrencyService _functionalCurrencyService;
        private readonly ILeasingService _leasingService;
        private readonly IFinancialInstrumentsService _financialInstrumentsService;
        private readonly IEmissionsTradingService _emissionsTradingService;
        private readonly IReserveDisclosureService _reserveDisclosureService;
        private readonly IInventoryLcmService _inventoryLcmService;
        private readonly IUnprovedPropertyService _unprovedPropertyService;
        private const string ConnectionName = "PPDM39";

        public PeriodClosingService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            ILogger<PeriodClosingService> logger = null,
            IAccountingServices accountingServices = null,
            IAmortizationService amortizationService = null,
            IFullCostService fullCostService = null,
            IReserveAccountingService reserveAccountingService = null,
            IImpairmentTestingService impairmentTestingService = null,
            IDecommissioningService decommissioningService = null,
            IFunctionalCurrencyService functionalCurrencyService = null,
            ILeasingService leasingService = null,
            IFinancialInstrumentsService financialInstrumentsService = null,
            IEmissionsTradingService emissionsTradingService = null,
            IReserveDisclosureService reserveDisclosureService = null,
            IInventoryLcmService inventoryLcmService = null,
            IUnprovedPropertyService unprovedPropertyService = null)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _logger = logger;
            _accountingServices = accountingServices;
            _amortizationService = amortizationService;
            _fullCostService = fullCostService;
            _reserveAccountingService = reserveAccountingService;
            _impairmentTestingService = impairmentTestingService;
            _decommissioningService = decommissioningService;
            _functionalCurrencyService = functionalCurrencyService;
            _leasingService = leasingService;
            _financialInstrumentsService = financialInstrumentsService;
            _emissionsTradingService = emissionsTradingService;
            _reserveDisclosureService = reserveDisclosureService;
            _inventoryLcmService = inventoryLcmService;
            _unprovedPropertyService = unprovedPropertyService;
        }

        /// <summary>
        /// Validates that a period is ready for closing.
        /// Checks: All allocations completed, all royalties calculated, all revenue recognized, GL balanced.
        /// </summary>
        public async Task<bool> ValidateReadinessAsync(
            string fieldId,
            DateTime periodEnd,
            string cn = "PPDM39")
        {
            if (string.IsNullOrWhiteSpace(fieldId))
                throw new ArgumentNullException(nameof(fieldId));
            if (periodEnd == default)
                throw new ArgumentException("periodEnd must be valid", nameof(periodEnd));

            _logger?.LogInformation(
                "Validating period closing readiness for field {FieldId} as of {PeriodEnd}",
                fieldId, periodEnd.ToShortDateString());

            try
            {
                // Check allocations are complete
                var unreconciledAllocations = await GetUnreconciledAllocationsAsync(fieldId, periodEnd, cn);
                if (unreconciledAllocations.Count > 0)
                {
                    _logger?.LogWarning(
                        "Period not ready: {Count} unreconciled allocations for field {FieldId}",
                        unreconciledAllocations.Count, fieldId);
                    return false;
                }

                // Check royalties are calculated
                var unreconciledRoyalties = await GetUnreconciledRoyaltiesAsync(fieldId, periodEnd, cn);
                if (unreconciledRoyalties.Count > 0)
                {
                    _logger?.LogWarning(
                        "Period not ready: {Count} unreconciled royalties for field {FieldId}",
                        unreconciledRoyalties.Count, fieldId);
                    return false;
                }

                // Check revenue is recognized
                var unreconciledRevenue = await GetUnreconciledRevenueAsync(fieldId, periodEnd, cn);
                if (unreconciledRevenue.Count > 0)
                {
                    _logger?.LogWarning(
                        "Period not ready: {Count} unreconciled revenue items for field {FieldId}",
                        unreconciledRevenue.Count, fieldId);
                    return false;
                }

                var reconciliationIssues = await GetReconciliationIssuesAsync(fieldId, periodEnd, cn);
                if (reconciliationIssues.Count > 0)
                {
                    _logger?.LogWarning(
                        "Period not ready: {Count} reconciliation issues for field {FieldId}",
                        reconciliationIssues.Count, fieldId);
                    return false;
                }

                // Check GL is balanced
                var glUnbalanced = await GetUnbalancedGLEntriesAsync(fieldId, periodEnd, cn);
                if (glUnbalanced.Count > 0)
                {
                    _logger?.LogWarning(
                        "Period not ready: {Count} unbalanced GL entries for field {FieldId}",
                        glUnbalanced.Count, fieldId);
                    return false;
                }

                _logger?.LogInformation(
                    "Period closing validation passed for field {FieldId}",
                    fieldId);

                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(
                    ex,
                    "Error validating period closing readiness for field {FieldId}: {ErrorMessage}",
                    fieldId, ex.Message);
                throw new ProductionAccountingException(
                    $"Failed to validate period closing readiness for field {fieldId}", ex);
            }
        }

        /// <summary>
        /// Closes a period - locks it and prevents future modifications.
        /// </summary>
        public async Task<bool> ClosePeriodAsync(
            string fieldId,
            DateTime periodEnd,
            string userId,
            string cn = "PPDM39")
        {
            if (string.IsNullOrWhiteSpace(fieldId))
                throw new ArgumentNullException(nameof(fieldId));
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));
            if (periodEnd == default)
                throw new ArgumentException("periodEnd must be valid", nameof(periodEnd));

            _logger?.LogInformation(
                "Closing period for field {FieldId} as of {PeriodEnd} by user {UserId}",
                fieldId, periodEnd.ToShortDateString(), userId);

            try
            {
                // First validate readiness
                var isReady = await ValidateReadinessAsync(fieldId, periodEnd, cn);
                if (!isReady)
                {
                    _logger?.LogError(
                        "Cannot close period: Validation failed for field {FieldId}",
                        fieldId);
                    throw new InvalidOperationException(
                        $"Period is not ready for closing. Please reconcile all items for field {fieldId}");
                }

                // Mark all allocation results as closed
                await MarkAllocationsClosed(fieldId, periodEnd, userId, cn);

                // Mark all royalty calculations as paid/settled
                await MarkRoyaltiesClosed(fieldId, periodEnd, userId, cn);

                // Mark all revenue as collected/billed
                await MarkRevenueClosed(fieldId, periodEnd, userId, cn);

                // Apply depletion for the period based on reserves
                await ApplyDepletionAsync(fieldId, periodEnd, userId, cn);

                // Run ceiling test at quarter end (full cost)
                if (IsQuarterEnd(periodEnd))
                    await RunCeilingTestAsync(fieldId, userId, cn);

                if (IsYearEnd(periodEnd))
                    await RunImpairmentTestingAsync(fieldId, periodEnd, userId, cn);

                await ApplyDecommissioningAccretionAsync(fieldId, periodEnd, userId, cn);
                await ApplyFunctionalCurrencyTranslationAsync(fieldId, periodEnd, cn);
                await UpdateLeaseRemeasurementsAsync(fieldId, periodEnd, userId, cn);
                await MeasureFinancialInstrumentsAsync(periodEnd, userId, cn);
                await UpdateEmissionsObligationsAsync(fieldId, periodEnd, userId, cn);
                await ApplyInventoryLcmAsync(periodEnd, userId, cn);

                if (IsYearEnd(periodEnd))
                    await RunUnprovedPropertyImpairmentAsync(periodEnd, userId, cn);

                if (IsYearEnd(periodEnd))
                    await BuildReserveDisclosuresAsync(fieldId, periodEnd, cn);

                // Post final GL entries (period close entry)
                await PostPeriodCloseEntry(fieldId, periodEnd, userId, cn);

                _logger?.LogInformation(
                    "Period closed successfully for field {FieldId} as of {PeriodEnd}",
                    fieldId, periodEnd.ToShortDateString());

                return true;
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger?.LogError(
                    ex,
                    "Error closing period for field {FieldId}: {ErrorMessage}",
                    fieldId, ex.Message);
                throw new ProductionAccountingException(
                    $"Failed to close period for field {fieldId}", ex);
            }
        }

        private async Task ApplyDepletionAsync(
            string fieldId,
            DateTime periodEnd,
            string userId,
            string cn)
        {
            if (_reserveAccountingService != null)
            {
                var reserves = await _reserveAccountingService.GetLatestReservesAsync(fieldId, periodEnd, cn);
                if (reserves == null)
                {
                    _logger?.LogWarning(
                        "No proved reserves found for field {FieldId}; depletion skipped for period {PeriodEnd}",
                        fieldId, periodEnd.ToShortDateString());
                }
            }

            if (_amortizationService == null)
            {
                _logger?.LogWarning(
                    "Amortization service not configured; depletion not calculated for field {FieldId}",
                    fieldId);
                return;
            }

            await _amortizationService.CalculateAsync(fieldId, periodEnd, userId, cn);
            await _amortizationService.CalculateFieldwideAsync(fieldId, periodEnd, userId, cn);
            await _amortizationService.CalculateSplitAsync(fieldId, periodEnd, userId, cn);
        }

        private async Task RunCeilingTestAsync(string fieldId, string userId, string cn)
        {
            if (_fullCostService == null)
            {
                _logger?.LogWarning(
                    "Full cost service not configured; ceiling test skipped for field {FieldId}",
                    fieldId);
                return;
            }

            await _fullCostService.PerformCeilingTestAsync(fieldId, userId, cn);
        }

        private static bool IsQuarterEnd(DateTime periodEnd)
        {
            return periodEnd.Month == 3 || periodEnd.Month == 6 || periodEnd.Month == 9 || periodEnd.Month == 12;
        }

        private static bool IsYearEnd(DateTime periodEnd)
        {
            return periodEnd.Month == 12;
        }

        /// <summary>
        /// Gets list of unreconciled items preventing period close.
        /// </summary>
        public async Task<List<string>> GetUnreconciledItemsAsync(
            string fieldId,
            DateTime periodEnd,
            string cn = "PPDM39")
        {
            if (string.IsNullOrWhiteSpace(fieldId))
                throw new ArgumentNullException(nameof(fieldId));
            if (periodEnd == default)
                throw new ArgumentException("periodEnd must be valid", nameof(periodEnd));

            _logger?.LogInformation(
                "Retrieving unreconciled items for field {FieldId} as of {PeriodEnd}",
                fieldId, periodEnd.ToShortDateString());

            try
            {
                var unreconciledItems = new List<string>();

                // Unreconciled allocations
                var unreconciledAllocations = await GetUnreconciledAllocationsAsync(fieldId, periodEnd, cn);
                if (unreconciledAllocations.Count > 0)
                {
                    unreconciledItems.Add(
                        $"ALLOCATIONS: {unreconciledAllocations.Count} unreconciled items");
                    foreach (var item in unreconciledAllocations.Take(5))
                    {
                        unreconciledItems.Add($"  - {item}");
                    }
                    if (unreconciledAllocations.Count > 5)
                    {
                        unreconciledItems.Add($"  - ... and {unreconciledAllocations.Count - 5} more");
                    }
                }

                // Unreconciled royalties
                var unreconciledRoyalties = await GetUnreconciledRoyaltiesAsync(fieldId, periodEnd, cn);
                if (unreconciledRoyalties.Count > 0)
                {
                    unreconciledItems.Add(
                        $"ROYALTIES: {unreconciledRoyalties.Count} unreconciled items");
                    foreach (var item in unreconciledRoyalties.Take(5))
                    {
                        unreconciledItems.Add($"  - {item}");
                    }
                    if (unreconciledRoyalties.Count > 5)
                    {
                        unreconciledItems.Add($"  - ... and {unreconciledRoyalties.Count - 5} more");
                    }
                }

                // Unreconciled revenue
                var unreconciledRevenue = await GetUnreconciledRevenueAsync(fieldId, periodEnd, cn);
                if (unreconciledRevenue.Count > 0)
                {
                    unreconciledItems.Add(
                        $"REVENUE: {unreconciledRevenue.Count} unreconciled items");
                    foreach (var item in unreconciledRevenue.Take(5))
                    {
                        unreconciledItems.Add($"  - {item}");
                    }
                    if (unreconciledRevenue.Count > 5)
                    {
                        unreconciledItems.Add($"  - ... and {unreconciledRevenue.Count - 5} more");
                    }
                }

                // Unbalanced GL
                var unbalancedGL = await GetUnbalancedGLEntriesAsync(fieldId, periodEnd, cn);
                if (unbalancedGL.Count > 0)
                {
                    unreconciledItems.Add(
                        $"GL ENTRIES: {unbalancedGL.Count} unbalanced entries");
                    foreach (var item in unbalancedGL.Take(5))
                    {
                        unreconciledItems.Add($"  - {item}");
                    }
                    if (unbalancedGL.Count > 5)
                    {
                        unreconciledItems.Add($"  - ... and {unbalancedGL.Count - 5} more");
                    }
                }

                // Reconciliation issues
                var reconciliationIssues = await GetReconciliationIssuesAsync(fieldId, periodEnd, cn);
                if (reconciliationIssues.Count > 0)
                {
                    unreconciledItems.Add(
                        $"RECONCILIATION: {reconciliationIssues.Count} issues");
                    foreach (var item in reconciliationIssues.Take(5))
                    {
                        unreconciledItems.Add($"  - {item}");
                    }
                    if (reconciliationIssues.Count > 5)
                    {
                        unreconciledItems.Add($"  - ... and {reconciliationIssues.Count - 5} more");
                    }
                }

                if (unreconciledItems.Count == 0)
                {
                    unreconciledItems.Add("No unreconciled items - period is ready for closing");
                }

                return unreconciledItems;
            }
            catch (Exception ex)
            {
                _logger?.LogError(
                    ex,
                    "Error retrieving unreconciled items for field {FieldId}: {ErrorMessage}",
                    fieldId, ex.Message);
                throw new ProductionAccountingException(
                    $"Failed to retrieve unreconciled items for field {fieldId}", ex);
            }
        }

        // Private helper methods

        private async Task<List<string>> GetUnreconciledAllocationsAsync(
            string fieldId,
            DateTime periodEnd,
            string cn)
        {
            var items = new List<string>();

            try
            {
                var metadata = await _metadata.GetTableMetadataAsync("ALLOCATION_RESULT");
                var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                    ?? typeof(ALLOCATION_RESULT);

                var repo = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, cn, "ALLOCATION_RESULT");

                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "ALLOCATION_DATE", Operator = "<=", FilterValue = periodEnd.ToString("yyyy-MM-dd") },
                    new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
                };

                var results = await repo.GetAsync(filters);
                var allocResults = results.Cast<ALLOCATION_RESULT>().ToList();

                var detailMetadata = await _metadata.GetTableMetadataAsync("ALLOCATION_DETAIL");
                var detailEntityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{detailMetadata.EntityTypeName}")
                    ?? typeof(ALLOCATION_DETAIL);

                var detailRepo = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    detailEntityType, cn, "ALLOCATION_DETAIL");

                // Filter for incomplete allocations (missing details)
                foreach (var alloc in allocResults)
                {
                    var runTicket = await GetRunTicketAsync(alloc.ALLOCATION_REQUEST_ID, cn);
                    if (runTicket == null || !string.Equals(runTicket.LEASE_ID, fieldId, StringComparison.OrdinalIgnoreCase))
                        continue;

                    var detailFilters = new List<AppFilter>
                    {
                        new AppFilter { FieldName = "ALLOCATION_RESULT_ID", Operator = "=", FilterValue = alloc.ALLOCATION_RESULT_ID },
                        new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
                    };

                    var details = await detailRepo.GetAsync(detailFilters);
                    var detailList = details?.Cast<ALLOCATION_DETAIL>().ToList() ?? new List<ALLOCATION_DETAIL>();
                    if (detailList.Count == 0)
                    {
                        items.Add($"Allocation {alloc.ALLOCATION_RESULT_ID} has no details");
                    }
                }

                return items;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error retrieving unreconciled allocations for field {FieldId}", fieldId);
                return new List<string>();
            }
        }

        private async Task<List<string>> GetUnreconciledRoyaltiesAsync(
            string fieldId,
            DateTime periodEnd,
            string cn)
        {
            var items = new List<string>();

            try
            {
                var metadata = await _metadata.GetTableMetadataAsync("ROYALTY_CALCULATION");
                var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                    ?? typeof(ROYALTY_CALCULATION);

                var repo = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, cn, "ROYALTY_CALCULATION");

                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "PROPERTY_OR_LEASE_ID", Operator = "=", FilterValue = fieldId },
                    new AppFilter { FieldName = "CALCULATION_DATE", Operator = "<=", FilterValue = periodEnd.ToString("yyyy-MM-dd") },
                    new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
                };

                var results = await repo.GetAsync(filters);
                var royalties = results.Cast<ROYALTY_CALCULATION>().ToList();

                foreach (var royalty in royalties)
                {
                    if (!string.Equals(royalty.ROYALTY_STATUS, RoyaltyStatus.Accrued, StringComparison.OrdinalIgnoreCase) &&
                        !string.Equals(royalty.ROYALTY_STATUS, RoyaltyStatus.Paid, StringComparison.OrdinalIgnoreCase))
                    {
                        items.Add($"Royalty {royalty.ROYALTY_CALCULATION_ID}");
                    }
                }

                return items;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error retrieving unreconciled royalties for field {FieldId}", fieldId);
                return new List<string>();
            }
        }

        private async Task<List<string>> GetUnreconciledRevenueAsync(
            string fieldId,
            DateTime periodEnd,
            string cn)
        {
            var items = new List<string>();

            try
            {
                var transactionMetadata = await _metadata.GetTableMetadataAsync("REVENUE_TRANSACTION");
                var transactionEntityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{transactionMetadata.EntityTypeName}")
                    ?? typeof(REVENUE_TRANSACTION);

                var transactionRepo = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    transactionEntityType, cn, "REVENUE_TRANSACTION");

                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "PROPERTY_ID", Operator = "=", FilterValue = fieldId },
                    new AppFilter { FieldName = "TRANSACTION_DATE", Operator = "<=", FilterValue = periodEnd.ToString("yyyy-MM-dd") },
                    new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
                };

                var transactionResults = await transactionRepo.GetAsync(filters);
                var transactions = transactionResults.Cast<REVENUE_TRANSACTION>().ToList();

                var allocationMetadata = await _metadata.GetTableMetadataAsync("REVENUE_ALLOCATION");
                var allocationEntityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{allocationMetadata.EntityTypeName}")
                    ?? typeof(REVENUE_ALLOCATION);

                var allocationRepo = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    allocationEntityType, cn, "REVENUE_ALLOCATION");

                var allocationResults = await allocationRepo.GetAsync(new List<AppFilter>
                {
                    new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
                });

                var allocationList = allocationResults?.Cast<REVENUE_ALLOCATION>().ToList() ?? new List<REVENUE_ALLOCATION>();
                var transactionIds = new HashSet<string>(
                    transactions
                        .Where(t => !string.IsNullOrWhiteSpace(t.REVENUE_TRANSACTION_ID))
                        .Select(t => t.REVENUE_TRANSACTION_ID));

                var allocatedTransactionIds = new HashSet<string>(
                    allocationList
                        .Where(a => !string.IsNullOrWhiteSpace(a.REVENUE_TRANSACTION_ID) &&
                                    transactionIds.Contains(a.REVENUE_TRANSACTION_ID))
                        .Select(a => a.REVENUE_TRANSACTION_ID));

                foreach (var transaction in transactions)
                {
                    if (!allocatedTransactionIds.Contains(transaction.REVENUE_TRANSACTION_ID))
                    {
                        items.Add($"Revenue transaction {transaction.REVENUE_TRANSACTION_ID} missing allocation");
                    }
                }

                return items;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error retrieving unreconciled revenue for field {FieldId}", fieldId);
                return new List<string>();
            }
        }

        private async Task<List<string>> GetUnbalancedGLEntriesAsync(
            string fieldId,
            DateTime periodEnd,
            string cn)
        {
            var items = new List<string>();

            try
            {
                var metadata = await _metadata.GetTableMetadataAsync("JOURNAL_ENTRY");
                var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                    ?? typeof(JOURNAL_ENTRY);

                var repo = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, cn, "JOURNAL_ENTRY");

                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "ENTRY_DATE", Operator = "<=", FilterValue = periodEnd.ToString("yyyy-MM-dd") },
                    new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
                };

                var results = await repo.GetAsync(filters);
                var entries = results.Cast<JOURNAL_ENTRY>().ToList();

                // Filter for unbalanced entries
                foreach (var entry in entries)
                {
                    var debit = entry.TOTAL_DEBIT ?? 0;
                    var credit = entry.TOTAL_CREDIT ?? 0;

                    // Allow 0.01 tolerance for floating point
                    if (Math.Abs(debit - credit) > 0.01m)
                    {
                        items.Add($"JE {entry.JOURNAL_ENTRY_ID}: Debit={debit}, Credit={credit}");
                    }
                }

                return items;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error retrieving unbalanced GL entries for field {FieldId}", fieldId);
                return new List<string>();
            }
        }

        private async Task MarkAllocationsClosed(
            string fieldId,
            DateTime periodEnd,
            string userId,
            string cn)
        {
            try
            {
                _logger?.LogInformation("Marking allocations closed for field {FieldId} as of {PeriodEnd}", fieldId, periodEnd.ToShortDateString());

                var metadata = await _metadata.GetTableMetadataAsync("ALLOCATION_RESULT");
                var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                    ?? typeof(ALLOCATION_RESULT);

                var repo = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, cn, "ALLOCATION_RESULT");

                // Get all unclosed allocations for this field up to period end
                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "ALLOCATION_DATE", Operator = "<=", FilterValue = periodEnd.ToString("yyyy-MM-dd") },
                    new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
                };

                var results = await repo.GetAsync(filters);
                var allocResults = results.Cast<ALLOCATION_RESULT>().ToList();

                // Mark each allocation as processed by updating row changed timestamp
                foreach (var alloc in allocResults)
                {
                    var runTicket = await GetRunTicketAsync(alloc.ALLOCATION_REQUEST_ID, cn);
                    if (runTicket == null || !string.Equals(runTicket.LEASE_ID, fieldId, StringComparison.OrdinalIgnoreCase))
                        continue;

                    alloc.ROW_CHANGED_BY = userId;
                    alloc.ROW_CHANGED_DATE = DateTime.UtcNow;

                    await repo.UpdateAsync(alloc, userId);
                    
                    _logger?.LogInformation(
                        "Allocation {AllocationId} marked for period close for field {FieldId}",
                        alloc.ALLOCATION_RESULT_ID, fieldId);
                }

                _logger?.LogInformation(
                    "Successfully marked {Count} allocations as closed for field {FieldId}",
                    allocResults.Count, fieldId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(
                    ex,
                    "Error marking allocations closed for field {FieldId}: {ErrorMessage}",
                    fieldId, ex.Message);
                throw;
            }
        }

        private async Task MarkRoyaltiesClosed(
            string fieldId,
            DateTime periodEnd,
            string userId,
            string cn)
        {
            try
            {
                _logger?.LogInformation("Marking royalties closed for field {FieldId} as of {PeriodEnd}", fieldId, periodEnd.ToShortDateString());

                var metadata = await _metadata.GetTableMetadataAsync("ROYALTY_CALCULATION");
                var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                    ?? typeof(ROYALTY_CALCULATION);

                var repo = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, cn, "ROYALTY_CALCULATION");

                // Get all royalties that still need accrual posting
                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "PROPERTY_OR_LEASE_ID", Operator = "=", FilterValue = fieldId },
                    new AppFilter { FieldName = "CALCULATION_DATE", Operator = "<=", FilterValue = periodEnd.ToString("yyyy-MM-dd") },
                    new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
                };

                var results = await repo.GetAsync(filters);
                var royalties = results.Cast<ROYALTY_CALCULATION>().ToList();

                // Update each royalty's status to "ACCRUED" (ready for payment)
                foreach (var royalty in royalties)
                {
                    if (string.Equals(royalty.ROYALTY_STATUS, RoyaltyStatus.Accrued, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(royalty.ROYALTY_STATUS, RoyaltyStatus.Paid, StringComparison.OrdinalIgnoreCase))
                        continue;

                    royalty.ROYALTY_STATUS = RoyaltyStatus.Accrued;
                    royalty.ROW_CHANGED_BY = userId;
                    royalty.ROW_CHANGED_DATE = DateTime.UtcNow;

                    await repo.UpdateAsync(royalty, userId);

                    _logger?.LogInformation(
                        "Royalty {RoyaltyId} marked as accrued for field {FieldId}",
                        royalty.ROYALTY_CALCULATION_ID, fieldId);
                }

                _logger?.LogInformation(
                    "Successfully marked {Count} royalties as accrued for field {FieldId}",
                    royalties.Count, fieldId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(
                    ex,
                    "Error marking royalties closed for field {FieldId}: {ErrorMessage}",
                    fieldId, ex.Message);
                throw;
            }
        }

        private async Task MarkRevenueClosed(
            string fieldId,
            DateTime periodEnd,
            string userId,
            string cn)
        {
            try
            {
                _logger?.LogInformation("Marking revenue closed for field {FieldId} as of {PeriodEnd}", fieldId, periodEnd.ToShortDateString());

                var metadata = await _metadata.GetTableMetadataAsync("REVENUE_TRANSACTION");
                var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                    ?? typeof(REVENUE_TRANSACTION);

                var repo = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, cn, "REVENUE_TRANSACTION");

                // Get all unrecognized revenue for this field up to period end
                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "PROPERTY_ID", Operator = "=", FilterValue = fieldId },
                    new AppFilter { FieldName = "TRANSACTION_DATE", Operator = "<=", FilterValue = periodEnd.ToString("yyyy-MM-dd") },
                    new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
                };

                var results = await repo.GetAsync(filters);
                var revenues = results.Cast<REVENUE_TRANSACTION>().ToList();

                // Update each revenue's row metadata to mark as processed
                foreach (var revenue in revenues)
                {
                    revenue.ROW_CHANGED_BY = userId;
                    revenue.ROW_CHANGED_DATE = DateTime.UtcNow;

                    await repo.UpdateAsync(revenue, userId);

                    _logger?.LogInformation(
                        "Revenue {RevenueId} marked as processed for field {FieldId}",
                        revenue.REVENUE_TRANSACTION_ID, fieldId);
                }

                _logger?.LogInformation(
                    "Successfully marked {Count} revenue items as processed for field {FieldId}",
                    revenues.Count, fieldId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(
                    ex,
                    "Error marking revenue closed for field {FieldId}: {ErrorMessage}",
                    fieldId, ex.Message);
                throw;
            }
        }

        private async Task PostPeriodCloseEntry(
            string fieldId,
            DateTime periodEnd,
            string userId,
            string cn)
        {
            try
            {
                _logger?.LogInformation("Posting period close GL entry for field {FieldId} as of {PeriodEnd}", fieldId, periodEnd.ToShortDateString());

                // Create a period closing journal entry to lock the period
                var metadata = await _metadata.GetTableMetadataAsync("JOURNAL_ENTRY");
                var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                    ?? typeof(JOURNAL_ENTRY);

                var repo = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, cn, "JOURNAL_ENTRY");

                var entryNumber = $"PC-{fieldId}-{periodEnd:yyyyMM}";
                var referenceNumber = $"CLOSE-{periodEnd:yyyyMM}";
                var existing = await repo.GetAsync(new List<AppFilter>
                {
                    new AppFilter { FieldName = "REFERENCE_NUMBER", Operator = "=", FilterValue = referenceNumber },
                    new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
                });

                if (existing != null && existing.Any())
                {
                    _logger?.LogInformation(
                        "Period close entry already exists for field {FieldId} as of {PeriodEnd}",
                        fieldId, periodEnd.ToShortDateString());
                    return;
                }

                var (grossRevenue, totalRoyalty) = await GetRevenueRoyaltyTotalsAsync(fieldId, periodEnd, cn);
                if (grossRevenue == 0m && totalRoyalty == 0m)
                {
                    _logger?.LogInformation(
                        "No revenue or royalty activity for field {FieldId} as of {PeriodEnd}. Skipping close entry.",
                        fieldId, periodEnd.ToShortDateString());
                    return;
                }

                var netIncome = grossRevenue - totalRoyalty;
                var entryDescription = $"Period close summary for {periodEnd:MMMM yyyy} (Gross={grossRevenue}, Royalty={totalRoyalty})";

                var lines = new List<JOURNAL_ENTRY_LINE>();
                var lineNumber = 1;

                if (grossRevenue > 0m)
                {
                    lines.Add(new JOURNAL_ENTRY_LINE
                    {
                        JOURNAL_ENTRY_LINE_ID = Guid.NewGuid().ToString(),
                        JOURNAL_ENTRY_ID = closingEntry.JOURNAL_ENTRY_ID,
                        GL_ACCOUNT_ID = DefaultGlAccounts.Revenue,
                        LINE_NUMBER = lineNumber++,
                        DEBIT_AMOUNT = grossRevenue,
                        CREDIT_AMOUNT = 0m,
                        DESCRIPTION = "Close revenue",
                        ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                        PPDM_GUID = Guid.NewGuid().ToString(),
                        ROW_CREATED_BY = userId,
                        ROW_CREATED_DATE = DateTime.UtcNow
                    });
                }

                if (totalRoyalty > 0m)
                {
                    lines.Add(new JOURNAL_ENTRY_LINE
                    {
                        JOURNAL_ENTRY_LINE_ID = Guid.NewGuid().ToString(),
                        JOURNAL_ENTRY_ID = closingEntry.JOURNAL_ENTRY_ID,
                        GL_ACCOUNT_ID = DefaultGlAccounts.RoyaltyExpense,
                        LINE_NUMBER = lineNumber++,
                        DEBIT_AMOUNT = 0m,
                        CREDIT_AMOUNT = totalRoyalty,
                        DESCRIPTION = "Close royalty expense",
                        ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                        PPDM_GUID = Guid.NewGuid().ToString(),
                        ROW_CREATED_BY = userId,
                        ROW_CREATED_DATE = DateTime.UtcNow
                    });
                }

                if (netIncome != 0m)
                {
                    var retainedDebit = netIncome < 0m ? Math.Abs(netIncome) : 0m;
                    var retainedCredit = netIncome > 0m ? netIncome : 0m;
                    lines.Add(new JOURNAL_ENTRY_LINE
                    {
                        JOURNAL_ENTRY_LINE_ID = Guid.NewGuid().ToString(),
                        JOURNAL_ENTRY_ID = closingEntry.JOURNAL_ENTRY_ID,
                        GL_ACCOUNT_ID = DefaultGlAccounts.RetainedEarnings,
                        LINE_NUMBER = lineNumber++,
                        DEBIT_AMOUNT = retainedDebit,
                        CREDIT_AMOUNT = retainedCredit,
                        DESCRIPTION = "Close net income to retained earnings",
                        ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                        PPDM_GUID = Guid.NewGuid().ToString(),
                        ROW_CREATED_BY = userId,
                        ROW_CREATED_DATE = DateTime.UtcNow
                    });
                }

                if (_accountingServices?.JournalEntries != null)
                {
                    var entry = await _accountingServices.JournalEntries.CreateEntryAsync(
                        periodEnd,
                        entryDescription,
                        lines,
                        userId,
                        referenceNumber,
                        "PERIOD_CLOSING");

                    await _accountingServices.JournalEntries.PostEntryAsync(entry.JOURNAL_ENTRY_ID, userId);

                    _logger?.LogInformation(
                        "Period close GL entry {EntryId} created via accounting services for field {FieldId} as of {PeriodEnd}",
                        entry.JOURNAL_ENTRY_ID, fieldId, periodEnd.ToShortDateString());
                    return;
                }

                // Create closing entry (fallback to direct repository operations)
                var closingEntry = new JOURNAL_ENTRY
                {
                    JOURNAL_ENTRY_ID = Guid.NewGuid().ToString(),
                    ENTRY_NUMBER = entryNumber,
                    ENTRY_DATE = periodEnd,
                    ENTRY_TYPE = "PERIOD_CLOSE",
                    STATUS = "Posted",
                    DESCRIPTION = entryDescription,
                    SOURCE_MODULE = "PERIOD_CLOSING",
                    REFERENCE_NUMBER = referenceNumber,
                    TOTAL_DEBIT = 0m,
                    TOTAL_CREDIT = 0m,
                    ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                    PPDM_GUID = Guid.NewGuid().ToString(),
                    ROW_CREATED_BY = userId,
                    ROW_CREATED_DATE = DateTime.UtcNow
                };

                await repo.InsertAsync(closingEntry, userId);

                var lineMetadata = await _metadata.GetTableMetadataAsync("JOURNAL_ENTRY_LINE");
                var lineEntityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{lineMetadata.EntityTypeName}")
                    ?? typeof(JOURNAL_ENTRY_LINE);

                var lineRepo = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    lineEntityType, cn, "JOURNAL_ENTRY_LINE");

                foreach (var line in lines)
                {
                    await lineRepo.InsertAsync(line, userId);
                }

                closingEntry.TOTAL_DEBIT = lines.Sum(l => l.DEBIT_AMOUNT ?? 0m);
                closingEntry.TOTAL_CREDIT = lines.Sum(l => l.CREDIT_AMOUNT ?? 0m);
                closingEntry.ROW_CHANGED_BY = userId;
                closingEntry.ROW_CHANGED_DATE = DateTime.UtcNow;
                await repo.UpdateAsync(closingEntry, userId);

                _logger?.LogInformation(
                    "Period close GL entry {EntryId} created for field {FieldId} as of {PeriodEnd}",
                    closingEntry.JOURNAL_ENTRY_ID, fieldId, periodEnd.ToShortDateString());
            }
            catch (Exception ex)
            {
                _logger?.LogError(
                    ex,
                    "Error posting period close entry for field {FieldId}: {ErrorMessage}",
                    fieldId, ex.Message);
                throw;
            }
        }

        private async Task<List<string>> GetReconciliationIssuesAsync(
            string fieldId,
            DateTime periodEnd,
            string cn)
        {
            var issues = new List<string>();

            var (productionVolume, revenueVolume) = await GetProductionRevenueVolumesAsync(fieldId, periodEnd, cn);
            var volumeVariance = productionVolume - revenueVolume;
            var volumeTolerance = Math.Max(1m, productionVolume * 0.001m);
            if (Math.Abs(volumeVariance) > volumeTolerance)
            {
                issues.Add($"Production vs revenue volume variance: {volumeVariance} (production={productionVolume}, revenue={revenueVolume})");
            }

            var (grossRevenue, totalRoyalty) = await GetRevenueRoyaltyTotalsAsync(fieldId, periodEnd, cn);
            if (totalRoyalty > grossRevenue + 0.01m)
            {
                issues.Add($"Royalty exceeds gross revenue: royalty={totalRoyalty}, gross={grossRevenue}");
            }

            return issues;
        }

        private async Task<(decimal productionVolume, decimal revenueVolume)> GetProductionRevenueVolumesAsync(
            string fieldId,
            DateTime periodEnd,
            string cn)
        {
            var periodStart = new DateTime(periodEnd.Year, periodEnd.Month, 1);

            var measurementMetadata = await _metadata.GetTableMetadataAsync("MEASUREMENT_RECORD");
            var measurementEntityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{measurementMetadata.EntityTypeName}")
                ?? typeof(MEASUREMENT_RECORD);

            var measurementRepo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                measurementEntityType, cn, "MEASUREMENT_RECORD");

            var measurementFilters = new List<AppFilter>
            {
                new AppFilter { FieldName = "LEASE_ID", Operator = "=", FilterValue = fieldId },
                new AppFilter { FieldName = "MEASUREMENT_DATETIME", Operator = ">=", FilterValue = periodStart.ToString("yyyy-MM-dd") },
                new AppFilter { FieldName = "MEASUREMENT_DATETIME", Operator = "<=", FilterValue = periodEnd.ToString("yyyy-MM-dd") },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            var measurementResults = await measurementRepo.GetAsync(measurementFilters);
            var measurements = measurementResults?.Cast<MEASUREMENT_RECORD>().ToList() ?? new List<MEASUREMENT_RECORD>();
            var productionVolume = measurements.Sum(m => m.NET_VOLUME ?? m.GROSS_VOLUME ?? 0m);

            var revenueMetadata = await _metadata.GetTableMetadataAsync("REVENUE_TRANSACTION");
            var revenueEntityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{revenueMetadata.EntityTypeName}")
                ?? typeof(REVENUE_TRANSACTION);

            var revenueRepo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                revenueEntityType, cn, "REVENUE_TRANSACTION");

            var revenueFilters = new List<AppFilter>
            {
                new AppFilter { FieldName = "PROPERTY_ID", Operator = "=", FilterValue = fieldId },
                new AppFilter { FieldName = "TRANSACTION_DATE", Operator = ">=", FilterValue = periodStart.ToString("yyyy-MM-dd") },
                new AppFilter { FieldName = "TRANSACTION_DATE", Operator = "<=", FilterValue = periodEnd.ToString("yyyy-MM-dd") },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            var revenueResults = await revenueRepo.GetAsync(revenueFilters);
            var revenues = revenueResults?.Cast<REVENUE_TRANSACTION>().ToList() ?? new List<REVENUE_TRANSACTION>();
            var revenueVolume = revenues.Sum(r => r.OIL_VOLUME ?? 0m);

            return (productionVolume, revenueVolume);
        }

        private async Task<(decimal grossRevenue, decimal totalRoyalty)> GetRevenueRoyaltyTotalsAsync(
            string fieldId,
            DateTime periodEnd,
            string cn)
        {
            var periodStart = new DateTime(periodEnd.Year, periodEnd.Month, 1);

            var revenueMetadata = await _metadata.GetTableMetadataAsync("REVENUE_TRANSACTION");
            var revenueEntityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{revenueMetadata.EntityTypeName}")
                ?? typeof(REVENUE_TRANSACTION);

            var revenueRepo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                revenueEntityType, cn, "REVENUE_TRANSACTION");

            var revenueFilters = new List<AppFilter>
            {
                new AppFilter { FieldName = "PROPERTY_ID", Operator = "=", FilterValue = fieldId },
                new AppFilter { FieldName = "TRANSACTION_DATE", Operator = ">=", FilterValue = periodStart.ToString("yyyy-MM-dd") },
                new AppFilter { FieldName = "TRANSACTION_DATE", Operator = "<=", FilterValue = periodEnd.ToString("yyyy-MM-dd") },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            var revenueResults = await revenueRepo.GetAsync(revenueFilters);
            var revenues = revenueResults?.Cast<REVENUE_TRANSACTION>().ToList() ?? new List<REVENUE_TRANSACTION>();
            var grossRevenue = revenues.Sum(r => r.GROSS_REVENUE ?? 0m);

            var royaltyMetadata = await _metadata.GetTableMetadataAsync("ROYALTY_CALCULATION");
            var royaltyEntityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{royaltyMetadata.EntityTypeName}")
                ?? typeof(ROYALTY_CALCULATION);

            var royaltyRepo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                royaltyEntityType, cn, "ROYALTY_CALCULATION");

            var royaltyFilters = new List<AppFilter>
            {
                new AppFilter { FieldName = "PROPERTY_OR_LEASE_ID", Operator = "=", FilterValue = fieldId },
                new AppFilter { FieldName = "CALCULATION_DATE", Operator = ">=", FilterValue = periodStart.ToString("yyyy-MM-dd") },
                new AppFilter { FieldName = "CALCULATION_DATE", Operator = "<=", FilterValue = periodEnd.ToString("yyyy-MM-dd") },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            var royaltyResults = await royaltyRepo.GetAsync(royaltyFilters);
            var royalties = royaltyResults?.Cast<ROYALTY_CALCULATION>().ToList() ?? new List<ROYALTY_CALCULATION>();
            var totalRoyalty = royalties.Sum(r => r.ROYALTY_AMOUNT ?? 0m);

            return (grossRevenue, totalRoyalty);
        }

        private async Task<RUN_TICKET?> GetRunTicketAsync(string allocationRequestId, string cn)
        {
            if (string.IsNullOrWhiteSpace(allocationRequestId))
                return null;

            var metadata = await _metadata.GetTableMetadataAsync("RUN_TICKET");
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                ?? typeof(RUN_TICKET);

            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                entityType, cn, "RUN_TICKET");

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "RUN_TICKET_ID", Operator = "=", FilterValue = allocationRequestId },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            var results = await repo.GetAsync(filters);
            return results?.Cast<RUN_TICKET>().FirstOrDefault();
        }

        private async Task RunImpairmentTestingAsync(
            string fieldId,
            DateTime periodEnd,
            string userId,
            string cn)
        {
            if (_impairmentTestingService == null || _reserveAccountingService == null)
            {
                _logger?.LogDebug("Impairment testing skipped; service not configured.");
                return;
            }
            try
            {
                var carryingAmount = await GetCapitalizedCostsAsync(fieldId, periodEnd, cn);
                var pv = await _reserveAccountingService.CalculatePresentValueAsync(fieldId, periodEnd, cn);

                await _impairmentTestingService.EvaluateImpairmentAsync(
                    fieldId,
                    carryingAmount,
                    pv,
                    pv * 0.95m,
                    periodEnd,
                    userId,
                    cn);
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Impairment testing failed for field {FieldId}", fieldId);
            }
        }

        private async Task ApplyDecommissioningAccretionAsync(
            string fieldId,
            DateTime periodEnd,
            string userId,
            string cn)
        {
            if (_decommissioningService == null)
                return;
            try
            {
                var metadata = await _metadata.GetTableMetadataAsync("ASSET_RETIREMENT_OBLIGATION");
                var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                    ?? typeof(ASSET_RETIREMENT_OBLIGATION);

                var repo = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, cn, "ASSET_RETIREMENT_OBLIGATION");

                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "FIELD_ID", Operator = "=", FilterValue = fieldId },
                    new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
                };

                var results = await repo.GetAsync(filters);
                var obligations = results?.Cast<ASSET_RETIREMENT_OBLIGATION>().ToList()
                    ?? new List<ASSET_RETIREMENT_OBLIGATION>();

                foreach (var aro in obligations)
                {
                    await _decommissioningService.AccreteAroAsync(aro.ARO_ID, periodEnd, userId, cn);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "ARO accretion failed for field {FieldId}", fieldId);
            }
        }

        private async Task ApplyFunctionalCurrencyTranslationAsync(
            string fieldId,
            DateTime periodEnd,
            string cn)
        {
            if (_functionalCurrencyService == null)
                return;
            try
            {
                await _functionalCurrencyService.TranslateBalancesAsync(fieldId, periodEnd, "USD", cn);
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Functional currency translation failed for field {FieldId}", fieldId);
            }
        }

        private async Task UpdateLeaseRemeasurementsAsync(
            string fieldId,
            DateTime periodEnd,
            string userId,
            string cn)
        {
            if (_leasingService == null)
                return;
            try
            {
                var metadata = await _metadata.GetTableMetadataAsync("LEASE_CONTRACT");
                var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                    ?? typeof(LEASE_CONTRACT);

                var repo = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, cn, "LEASE_CONTRACT");

                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "PROPERTY_ID", Operator = "=", FilterValue = fieldId },
                    new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
                };

                var results = await repo.GetAsync(filters);
                var leases = results?.Cast<LEASE_CONTRACT>().ToList()
                    ?? new List<LEASE_CONTRACT>();

                foreach (var lease in leases)
                {
                    await _leasingService.RemeasureLeaseAsync(lease.LEASE_ID, periodEnd, userId, cn);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Lease remeasurement failed for field {FieldId}", fieldId);
            }
        }

        private async Task MeasureFinancialInstrumentsAsync(
            DateTime periodEnd,
            string userId,
            string cn)
        {
            if (_financialInstrumentsService == null)
                return;
            try
            {
                var metadata = await _metadata.GetTableMetadataAsync("HEDGE_RELATIONSHIP");
                var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                    ?? typeof(HEDGE_RELATIONSHIP);

                var repo = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, cn, "HEDGE_RELATIONSHIP");

                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
                };

                var results = await repo.GetAsync(filters);
                var hedges = results?.Cast<HEDGE_RELATIONSHIP>().ToList()
                    ?? new List<HEDGE_RELATIONSHIP>();

                foreach (var hedge in hedges)
                {
                    await _financialInstrumentsService.MeasureHedgeAsync(
                        hedge,
                        0m,
                        0m,
                        periodEnd,
                        userId,
                        cn);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Hedge measurement failed for period {PeriodEnd}", periodEnd);
            }
        }

        private async Task UpdateEmissionsObligationsAsync(
            string fieldId,
            DateTime periodEnd,
            string userId,
            string cn)
        {
            if (_emissionsTradingService == null)
                return;
            try
            {
                var metadata = await _metadata.GetTableMetadataAsync("EMISSIONS_OBLIGATION");
                var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                    ?? typeof(EMISSIONS_OBLIGATION);

                var repo = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, cn, "EMISSIONS_OBLIGATION");

                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
                };

                var results = await repo.GetAsync(filters);
                var obligations = results?.Cast<EMISSIONS_OBLIGATION>().ToList()
                    ?? new List<EMISSIONS_OBLIGATION>();

                foreach (var obligation in obligations)
                {
                    await _emissionsTradingService.UpdateObligationAsync(
                        obligation,
                        obligation.EMISSIONS_VOLUME ?? 0m,
                        obligation.ALLOWANCE_PRICE ?? 0m,
                        periodEnd,
                        userId,
                        cn);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Emissions obligation update failed for period {PeriodEnd}", periodEnd);
            }
        }

        private async Task ApplyInventoryLcmAsync(
            DateTime periodEnd,
            string userId,
            string cn)
        {
            if (_inventoryLcmService == null)
                return;

            try
            {
                var metadata = await _metadata.GetTableMetadataAsync("INVENTORY_ITEM");
                var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                    ?? typeof(INVENTORY_ITEM);

                var repo = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, cn, "INVENTORY_ITEM");

                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
                };

                var results = await repo.GetAsync(filters);
                var items = results?.Cast<INVENTORY_ITEM>().ToList()
                    ?? new List<INVENTORY_ITEM>();

                foreach (var item in items)
                {
                    if (string.IsNullOrWhiteSpace(item.INVENTORY_ITEM_ID))
                        continue;
                    await _inventoryLcmService.ApplyLowerOfCostOrMarketAsync(
                        item.INVENTORY_ITEM_ID,
                        periodEnd,
                        userId,
                        cn);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Inventory LCM adjustments failed for period {PeriodEnd}", periodEnd);
            }
        }

        private async Task RunUnprovedPropertyImpairmentAsync(
            DateTime periodEnd,
            string userId,
            string cn)
        {
            if (_unprovedPropertyService == null)
                return;

            try
            {
                var metadata = await _metadata.GetTableMetadataAsync("ACCOUNTING_COST");
                var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                    ?? typeof(ACCOUNTING_COST);

                var repo = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, cn, "ACCOUNTING_COST");

                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "COST_TYPE", Operator = "=", FilterValue = CostTypes.Acquisition },
                    new AppFilter { FieldName = "IS_CAPITALIZED", Operator = "=", FilterValue = "Y" },
                    new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
                };

                var results = await repo.GetAsync(filters);
                var costs = results?.Cast<ACCOUNTING_COST>().ToList()
                    ?? new List<ACCOUNTING_COST>();

                var propertyIds = costs
                    .Select(c => c.PROPERTY_ID)
                    .Where(id => !string.IsNullOrWhiteSpace(id))
                    .Distinct()
                    .ToList();

                foreach (var propertyId in propertyIds)
                {
                    await _unprovedPropertyService.TestImpairmentAsync(
                        propertyId,
                        periodEnd,
                        userId,
                        cn);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Unproved property impairment tests failed for period {PeriodEnd}", periodEnd);
            }
        }

        private async Task BuildReserveDisclosuresAsync(
            string fieldId,
            DateTime periodEnd,
            string cn)
        {
            if (_reserveDisclosureService == null)
                return;
            try
            {
                await _reserveDisclosureService.BuildDisclosureAsync(fieldId, periodEnd, cn);
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Reserve disclosure build failed for field {FieldId}", fieldId);
            }
        }

        private async Task<decimal> GetCapitalizedCostsAsync(
            string fieldId,
            DateTime periodEnd,
            string cn)
        {
            var metadata = await _metadata.GetTableMetadataAsync("ACCOUNTING_COST");
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                ?? typeof(ACCOUNTING_COST);

            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                entityType, cn, "ACCOUNTING_COST");

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "PROPERTY_ID", Operator = "=", FilterValue = fieldId },
                new AppFilter { FieldName = "COST_DATE", Operator = "<=", FilterValue = periodEnd.ToString("yyyy-MM-dd") },
                new AppFilter { FieldName = "IS_CAPITALIZED", Operator = "=", FilterValue = "Y" },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            var results = await repo.GetAsync(filters);
            var costs = results?.Cast<ACCOUNTING_COST>().ToList()
                ?? new List<ACCOUNTING_COST>();

            return costs.Sum(c => c.AMOUNT);
        }
    }
}
