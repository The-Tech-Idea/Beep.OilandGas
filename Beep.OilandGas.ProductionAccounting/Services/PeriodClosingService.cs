using Beep.OilandGas.PPDM39.Core;
﻿using System;
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
        private readonly JournalEntryService _journalEntries;
        private readonly Func<Task<string>> _resolveConnection;
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
            JournalEntryService journalEntries,
            Func<Task<string>> resolveConnection,
            ILogger<PeriodClosingService> logger = null,
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
            _journalEntries = journalEntries ?? throw new ArgumentNullException(nameof(journalEntries));
            _resolveConnection = resolveConnection ?? throw new ArgumentNullException(nameof(resolveConnection));
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
            string connectionName = "PPDM39")
        {
            if (string.IsNullOrWhiteSpace(fieldId))
                throw new ArgumentNullException(nameof(fieldId));
            if (periodEnd == default)
                throw RefusalException.Invalid("A period end date is required.");

            connectionName = await ResolveConnectionAsync();

            _logger?.LogInformation(
                "Validating period closing readiness for field {FieldId} as of {PeriodEnd}",
                fieldId, periodEnd.ToShortDateString());

            try
            {
                // Check allocations are complete
                var unreconciledAllocations = await GetUnreconciledAllocationsAsync(fieldId, periodEnd, connectionName);
                if (unreconciledAllocations.Count > 0)
                {
                    _logger?.LogWarning(
                        "Period not ready: {Count} unreconciled allocations for field {FieldId}",
                        unreconciledAllocations.Count, fieldId);
                    return false;
                }

                // Check royalties are calculated
                var unreconciledRoyalties = await GetUnreconciledRoyaltiesAsync(fieldId, periodEnd, connectionName);
                if (unreconciledRoyalties.Count > 0)
                {
                    _logger?.LogWarning(
                        "Period not ready: {Count} unreconciled royalties for field {FieldId}",
                        unreconciledRoyalties.Count, fieldId);
                    return false;
                }

                // Check revenue is recognized
                var unreconciledRevenue = await GetUnreconciledRevenueAsync(fieldId, periodEnd, connectionName);
                if (unreconciledRevenue.Count > 0)
                {
                    _logger?.LogWarning(
                        "Period not ready: {Count} unreconciled revenue items for field {FieldId}",
                        unreconciledRevenue.Count, fieldId);
                    return false;
                }

                var reconciliationIssues = await GetReconciliationIssuesAsync(fieldId, periodEnd, connectionName);
                if (reconciliationIssues.Count > 0)
                {
                    _logger?.LogWarning(
                        "Period not ready: {Count} reconciliation issues for field {FieldId}",
                        reconciliationIssues.Count, fieldId);
                    return false;
                }

                // Check GL is balanced
                var glUnbalanced = await GetUnbalancedGLEntriesAsync(fieldId, periodEnd, connectionName);
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
            catch (OperationCanceledException)
            {
                _logger?.LogWarning(
                    "Period close readiness validation cancelled for field {FieldId}",
                    fieldId);
                throw;
            }
            catch (Exception ex) when (ex is not RefusalException)
            {
                _logger?.LogError(
                    ex,
                    "Error validating period closing readiness for field {FieldId}",
                    fieldId);
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
            string connectionName = "PPDM39")
        {
            if (string.IsNullOrWhiteSpace(fieldId))
                throw new ArgumentNullException(nameof(fieldId));
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));
            if (periodEnd == default)
                throw RefusalException.Invalid("A period end date is required.");

            connectionName = await ResolveConnectionAsync();

            _logger?.LogInformation(
                "Closing period for field {FieldId} as of {PeriodEnd} by user {UserId}",
                fieldId, periodEnd.ToShortDateString(), userId);

            try
            {
                // First validate readiness
                var isReady = await ValidateReadinessAsync(fieldId, periodEnd, connectionName);
                if (!isReady)
                {
                    _logger?.LogError(
                        "Cannot close period: Validation failed for field {FieldId}",
                        fieldId);
                    throw RefusalException.Conflict(
                        $"The period is not ready to close: reconcile all items for field {fieldId} first.");
                }

                // Mark all allocation results as closed
                await MarkAllocationsClosed(fieldId, periodEnd, userId, connectionName);

                // Mark all royalty calculations as paid/settled
                await MarkRoyaltiesClosed(fieldId, periodEnd, userId, connectionName);

                // Mark all revenue as collected/billed
                await MarkRevenueClosed(fieldId, periodEnd, userId, connectionName);

                // Apply depletion for the period based on reserves
                await ApplyDepletionAsync(fieldId, periodEnd, userId, connectionName);

                // Run ceiling test at quarter end (full cost)
                if (IsQuarterEnd(periodEnd))
                    await RunCeilingTestAsync(fieldId, userId, connectionName);

                if (IsYearEnd(periodEnd))
                    await RunImpairmentTestingAsync(fieldId, periodEnd, userId, connectionName);

                await ApplyDecommissioningAccretionAsync(fieldId, periodEnd, userId, connectionName);
                await ApplyFunctionalCurrencyTranslationAsync(fieldId, periodEnd, connectionName);
                await UpdateLeaseRemeasurementsAsync(fieldId, periodEnd, userId, connectionName);
                await MeasureFinancialInstrumentsAsync(periodEnd, userId, connectionName);
                await UpdateEmissionsObligationsAsync(fieldId, periodEnd, userId, connectionName);
                await ApplyInventoryLcmAsync(periodEnd, userId, connectionName);

                if (IsYearEnd(periodEnd))
                    await RunUnprovedPropertyImpairmentAsync(periodEnd, userId, connectionName);

                if (IsYearEnd(periodEnd))
                    await BuildReserveDisclosuresAsync(fieldId, periodEnd, connectionName);

                // Post final GL entries (period close entry)
                await PostPeriodCloseEntry(fieldId, periodEnd, userId, connectionName);

                _logger?.LogInformation(
                    "Period closed successfully for field {FieldId} as of {PeriodEnd}",
                    fieldId, periodEnd.ToShortDateString());

                return true;
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (OperationCanceledException)
            {
                _logger?.LogWarning(
                    "Period close cancelled for field {FieldId}",
                    fieldId);
                throw;
            }
            catch (Exception ex) when (ex is not RefusalException)
            {
                _logger?.LogError(
                    ex,
                    "Error closing period for field {FieldId}",
                    fieldId);
                throw new ProductionAccountingException(
                    $"Failed to close period for field {fieldId}", ex);
            }
        }

        private async Task ApplyDepletionAsync(
            string fieldId,
            DateTime periodEnd,
            string userId,
            string connectionName)
        {
            if (_reserveAccountingService != null)
            {
                var reserves = await _reserveAccountingService.GetLatestReservesAsync(fieldId, periodEnd, connectionName);
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

            await _amortizationService.CalculateAsync(fieldId, periodEnd, userId, connectionName);
            await _amortizationService.CalculateFieldwideAsync(fieldId, periodEnd, userId, connectionName);
            await _amortizationService.CalculateSplitAsync(fieldId, periodEnd, userId, connectionName);
        }

        private async Task RunCeilingTestAsync(string fieldId, string userId, string connectionName)
        {
            if (_fullCostService == null)
            {
                _logger?.LogWarning(
                    "Full cost service not configured; ceiling test skipped for field {FieldId}",
                    fieldId);
                return;
            }

            await _fullCostService.PerformCeilingTestAsync(fieldId, userId, connectionName);
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
            string connectionName = "PPDM39")
        {
            if (string.IsNullOrWhiteSpace(fieldId))
                throw new ArgumentNullException(nameof(fieldId));
            if (periodEnd == default)
                throw RefusalException.Invalid("A period end date is required.");

            connectionName = await ResolveConnectionAsync();

            _logger?.LogInformation(
                "Retrieving unreconciled items for field {FieldId} as of {PeriodEnd}",
                fieldId, periodEnd.ToShortDateString());

            try
            {
                var unreconciledItems = new List<string>();

                // Unreconciled allocations
                var unreconciledAllocations = await GetUnreconciledAllocationsAsync(fieldId, periodEnd, connectionName);
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
                var unreconciledRoyalties = await GetUnreconciledRoyaltiesAsync(fieldId, periodEnd, connectionName);
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
                var unreconciledRevenue = await GetUnreconciledRevenueAsync(fieldId, periodEnd, connectionName);
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
                var unbalancedGL = await GetUnbalancedGLEntriesAsync(fieldId, periodEnd, connectionName);
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
                var reconciliationIssues = await GetReconciliationIssuesAsync(fieldId, periodEnd, connectionName);
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
            catch (OperationCanceledException)
            {
                _logger?.LogWarning(
                    "Unreconciled item retrieval cancelled for field {FieldId}",
                    fieldId);
                throw;
            }
            catch (Exception ex) when (ex is not RefusalException)
            {
                _logger?.LogError(
                    ex,
                    "Error retrieving unreconciled items for field {FieldId}",
                    fieldId);
                throw new ProductionAccountingException(
                    $"Failed to retrieve unreconciled items for field {fieldId}", ex);
            }
        }

        // Private helper methods

        private async Task<List<string>> GetUnreconciledAllocationsAsync(
            string fieldId,
            DateTime periodEnd,
            string connectionName)
        {
            var items = new List<string>();

            try
            {
                var repo = await CreateRepositoryAsync<ALLOCATION_RESULT>("ALLOCATION_RESULT");

                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "ALLOCATION_DATE", Operator = "<=", FilterValue = periodEnd.ToString("yyyy-MM-dd") },
                    new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = _defaults.GetActiveIndicatorYes() }
                };

                var results = await repo.GetAsync(filters);
                var allocResults = results.Cast<ALLOCATION_RESULT>().ToList();

                var detailRepo = await CreateRepositoryAsync<ALLOCATION_DETAIL>("ALLOCATION_DETAIL");

                // Filter for incomplete allocations (missing details)
                foreach (var alloc in allocResults)
                {
                    var RUN_TICKET = await GetRunTicketAsync(alloc.ALLOCATION_REQUEST_ID, connectionName);
                    if (RUN_TICKET == null || !string.Equals(RUN_TICKET.LEASE_ID, fieldId, StringComparison.OrdinalIgnoreCase))
                        continue;

                    var detailFilters = new List<AppFilter>
                    {
                        new AppFilter { FieldName = "ALLOCATION_RESULT_ID", Operator = "=", FilterValue = alloc.ALLOCATION_RESULT_ID },
                        new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = _defaults.GetActiveIndicatorYes() }
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
            catch (OperationCanceledException)
            {
                _logger?.LogWarning(
                    "Unreconciled allocation retrieval cancelled for field {FieldId} as of {PeriodEnd}",
                    fieldId,
                    periodEnd);
                throw;
            }
        }

        private async Task<List<string>> GetUnreconciledRoyaltiesAsync(
            string fieldId,
            DateTime periodEnd,
            string connectionName)
        {
            var items = new List<string>();

            try
            {
                var repo = await CreateRepositoryAsync<ROYALTY_CALCULATION>("ROYALTY_CALCULATION");

                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "PROPERTY_OR_LEASE_ID", Operator = "=", FilterValue = fieldId },
                    new AppFilter { FieldName = "CALCULATION_DATE", Operator = "<=", FilterValue = periodEnd.ToString("yyyy-MM-dd") },
                    new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = _defaults.GetActiveIndicatorYes() }
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
            catch (OperationCanceledException)
            {
                _logger?.LogWarning(
                    "Unreconciled royalty retrieval cancelled for field {FieldId} as of {PeriodEnd}",
                    fieldId,
                    periodEnd);
                throw;
            }
        }

        private async Task<List<string>> GetUnreconciledRevenueAsync(
            string fieldId,
            DateTime periodEnd,
            string connectionName)
        {
            var items = new List<string>();

            try
            {
                var transactionRepo = await CreateRepositoryAsync<REVENUE_TRANSACTION>("REVENUE_TRANSACTION");

                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "PROPERTY_ID", Operator = "=", FilterValue = fieldId },
                    new AppFilter { FieldName = "TRANSACTION_DATE", Operator = "<=", FilterValue = periodEnd.ToString("yyyy-MM-dd") },
                    new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = _defaults.GetActiveIndicatorYes() }
                };

                var transactionResults = await transactionRepo.GetAsync(filters);
                var transactions = transactionResults.Cast<REVENUE_TRANSACTION>().ToList();

                var allocationRepo = await CreateRepositoryAsync<REVENUE_ALLOCATION>("REVENUE_ALLOCATION");

                var allocationResults = await allocationRepo.GetAsync(new List<AppFilter>
                {
                    new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = _defaults.GetActiveIndicatorYes() }
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
            catch (OperationCanceledException)
            {
                _logger?.LogWarning(
                    "Unreconciled revenue retrieval cancelled for field {FieldId} as of {PeriodEnd}",
                    fieldId,
                    periodEnd);
                throw;
            }
        }

        private async Task<List<string>> GetUnbalancedGLEntriesAsync(
            string fieldId,
            DateTime periodEnd,
            string connectionName)
        {
            var items = new List<string>();

            try
            {
                var repo = await CreateRepositoryAsync<JOURNAL_ENTRY>("JOURNAL_ENTRY");

                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "ENTRY_DATE", Operator = "<=", FilterValue = periodEnd.ToString("yyyy-MM-dd") },
                    new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = _defaults.GetActiveIndicatorYes() }
                };

                var results = await repo.GetAsync(filters);
                var entries = results.Cast<JOURNAL_ENTRY>().ToList();

                // Filter for unbalanced entries
                foreach (var entry in entries)
                {
                    var debit = entry.TOTAL_DEBIT is decimal d ? d : 0m;
                    var credit = entry.TOTAL_CREDIT is decimal c ? c : 0m;

                    // Allow 0.01 tolerance for floating point
                    if (Math.Abs(debit - credit) > 0.01m)
                    {
                        items.Add($"JE {entry.JOURNAL_ENTRY_ID}: Debit={debit}, Credit={credit}");
                    }
                }

                return items;
            }
            catch (OperationCanceledException)
            {
                _logger?.LogWarning(
                    "Unbalanced GL retrieval cancelled for field {FieldId} as of {PeriodEnd}",
                    fieldId,
                    periodEnd);
                throw;
            }
        }

        private async Task MarkAllocationsClosed(
            string fieldId,
            DateTime periodEnd,
            string userId,
            string connectionName)
        {
            try
            {
                _logger?.LogInformation("Marking allocations closed for field {FieldId} as of {PeriodEnd}", fieldId, periodEnd.ToShortDateString());

                var repo = await CreateRepositoryAsync<ALLOCATION_RESULT>("ALLOCATION_RESULT");

                // Get all unclosed allocations for this field up to period end
                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "ALLOCATION_DATE", Operator = "<=", FilterValue = periodEnd.ToString("yyyy-MM-dd") },
                    new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = _defaults.GetActiveIndicatorYes() }
                };

                var results = await repo.GetAsync(filters);
                var allocResults = results.Cast<ALLOCATION_RESULT>().ToList();

                // Mark each allocation as processed by updating row changed timestamp
                foreach (var alloc in allocResults)
                {
                    var RUN_TICKET = await GetRunTicketAsync(alloc.ALLOCATION_REQUEST_ID, connectionName);
                    if (RUN_TICKET == null || !string.Equals(RUN_TICKET.LEASE_ID, fieldId, StringComparison.OrdinalIgnoreCase))
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
            catch (OperationCanceledException)
            {
                _logger?.LogWarning(
                    "Allocation close marking cancelled for field {FieldId}",
                    fieldId);
                throw;
            }
            catch (Exception ex)
            {
                _logger?.LogError(
                    ex,
                    "Error marking allocations closed for field {FieldId}",
                    fieldId);
                throw;
            }
        }

        private async Task MarkRoyaltiesClosed(
            string fieldId,
            DateTime periodEnd,
            string userId,
            string connectionName)
        {
            try
            {
                _logger?.LogInformation("Marking royalties closed for field {FieldId} as of {PeriodEnd}", fieldId, periodEnd.ToShortDateString());

                var repo = await CreateRepositoryAsync<ROYALTY_CALCULATION>("ROYALTY_CALCULATION");

                // Get all royalties that still need accrual posting
                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "PROPERTY_OR_LEASE_ID", Operator = "=", FilterValue = fieldId },
                    new AppFilter { FieldName = "CALCULATION_DATE", Operator = "<=", FilterValue = periodEnd.ToString("yyyy-MM-dd") },
                    new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = _defaults.GetActiveIndicatorYes() }
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
            catch (OperationCanceledException)
            {
                _logger?.LogWarning(
                    "Royalty close marking cancelled for field {FieldId}",
                    fieldId);
                throw;
            }
            catch (Exception ex)
            {
                _logger?.LogError(
                    ex,
                    "Error marking royalties closed for field {FieldId}",
                    fieldId);
                throw;
            }
        }

        private async Task MarkRevenueClosed(
            string fieldId,
            DateTime periodEnd,
            string userId,
            string connectionName)
        {
            try
            {
                _logger?.LogInformation("Marking revenue closed for field {FieldId} as of {PeriodEnd}", fieldId, periodEnd.ToShortDateString());

                var repo = await CreateRepositoryAsync<REVENUE_TRANSACTION>("REVENUE_TRANSACTION");

                // Get all unrecognized revenue for this field up to period end
                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "PROPERTY_ID", Operator = "=", FilterValue = fieldId },
                    new AppFilter { FieldName = "TRANSACTION_DATE", Operator = "<=", FilterValue = periodEnd.ToString("yyyy-MM-dd") },
                    new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = _defaults.GetActiveIndicatorYes() }
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
            catch (OperationCanceledException)
            {
                _logger?.LogWarning(
                    "Revenue close marking cancelled for field {FieldId}",
                    fieldId);
                throw;
            }
            catch (Exception ex)
            {
                _logger?.LogError(
                    ex,
                    "Error marking revenue closed for field {FieldId}",
                    fieldId);
                throw;
            }
        }

        private async Task PostPeriodCloseEntry(
            string fieldId,
            DateTime periodEnd,
            string userId,
            string connectionName)
        {
            try
            {
                _logger?.LogInformation("Posting period close GL entry for field {FieldId} as of {PeriodEnd}", fieldId, periodEnd.ToShortDateString());

                // Create a period closing journal entry to lock the period
                var repo = await CreateRepositoryAsync<JOURNAL_ENTRY>("JOURNAL_ENTRY");

                var referenceNumber = $"CLOSE-{fieldId}-{periodEnd:yyyyMM}";
                var existing = await repo.GetAsync(new List<AppFilter>
                {
                    new AppFilter { FieldName = "REFERENCE_NUMBER", Operator = "=", FilterValue = referenceNumber },
                    new AppFilter { FieldName = "SOURCE_MODULE", Operator = "=", FilterValue = AccountingSourceModuleCodes.PeriodClosing },
                    new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = _defaults.GetActiveIndicatorYes() }
                });

                if (existing != null && existing.Any())
                {
                    if (existing.Cast<JOURNAL_ENTRY>().Any(entry =>
                        !string.Equals(entry.STATUS, JournalEntryStatusCodes.Posted, StringComparison.OrdinalIgnoreCase)))
                        throw RefusalException.Conflict("An unfinished closing journal requires review before this period can be closed.");

                    _logger?.LogInformation(
                        "Period close entry already exists for field {FieldId} as of {PeriodEnd}",
                        fieldId, periodEnd.ToShortDateString());
                    return;
                }

                var (grossRevenue, totalRoyalty) = await GetRevenueRoyaltyTotalsAsync(fieldId, periodEnd, connectionName);
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
                        GL_ACCOUNT_ID = DefaultGlAccounts.Revenue,
                        LINE_NUMBER = lineNumber++,
                        DEBIT_AMOUNT = grossRevenue,
                        CREDIT_AMOUNT = 0m,
                        DESCRIPTION = PeriodCloseJournalLineDescriptionPhrases.CloseRevenue,
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
                        GL_ACCOUNT_ID = DefaultGlAccounts.RoyaltyExpense,
                        LINE_NUMBER = lineNumber++,
                        DEBIT_AMOUNT = 0m,
                        CREDIT_AMOUNT = totalRoyalty,
                        DESCRIPTION = PeriodCloseJournalLineDescriptionPhrases.CloseRoyaltyExpense,
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
                        GL_ACCOUNT_ID = DefaultGlAccounts.RetainedEarnings,
                        LINE_NUMBER = lineNumber++,
                        DEBIT_AMOUNT = retainedDebit,
                        CREDIT_AMOUNT = retainedCredit,
                        DESCRIPTION = PeriodCloseJournalLineDescriptionPhrases.CloseNetIncomeToRetainedEarnings,
                        ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                        PPDM_GUID = Guid.NewGuid().ToString(),
                        ROW_CREATED_BY = userId,
                        ROW_CREATED_DATE = DateTime.UtcNow
                    });
                }

                var entry = await _journalEntries.CreateEntryAsync(
                    periodEnd, entryDescription, lines, userId,
                    referenceNumber, AccountingSourceModuleCodes.PeriodClosing);
                if (!await _journalEntries.PostEntryAsync(entry.JOURNAL_ENTRY_ID, userId))
                    throw new InvalidOperationException($"Period close journal {entry.JOURNAL_ENTRY_ID} was created but not posted.");

                _logger?.LogInformation(
                    "Period close GL entry {EntryId} posted for field {FieldId} as of {PeriodEnd}",
                    entry.JOURNAL_ENTRY_ID, fieldId, periodEnd.ToShortDateString());
            }
            catch (OperationCanceledException)
            {
                _logger?.LogWarning(
                    "Period close entry posting cancelled for field {FieldId}",
                    fieldId);
                throw;
            }
            catch (Exception ex)
            {
                _logger?.LogError(
                    ex,
                    "Error posting period close entry for field {FieldId}",
                    fieldId);
                throw;
            }
        }

        private async Task<List<string>> GetReconciliationIssuesAsync(
            string fieldId,
            DateTime periodEnd,
            string connectionName)
        {
            var issues = new List<string>();

            var (productionVolume, revenueVolume) = await GetProductionRevenueVolumesAsync(fieldId, periodEnd, connectionName);
            var volumeVariance = productionVolume - revenueVolume;
            var volumeTolerance = Math.Max(1m, productionVolume * 0.001m);
            if (Math.Abs(volumeVariance) > volumeTolerance)
            {
                issues.Add($"Production vs revenue volume variance: {volumeVariance} (production={productionVolume}, revenue={revenueVolume})");
            }

            var (grossRevenue, totalRoyalty) = await GetRevenueRoyaltyTotalsAsync(fieldId, periodEnd, connectionName);
            if (totalRoyalty > grossRevenue + 0.01m)
            {
                issues.Add($"Royalty exceeds gross revenue: royalty={totalRoyalty}, gross={grossRevenue}");
            }

            return issues;
        }

        private async Task<(decimal productionVolume, decimal revenueVolume)> GetProductionRevenueVolumesAsync(
            string fieldId,
            DateTime periodEnd,
            string connectionName)
        {
            var periodStart = new DateTime(periodEnd.Year, periodEnd.Month, 1);

            var measurementRepo = await CreateRepositoryAsync<MEASUREMENT_RECORD>("MEASUREMENT_RECORD");

            var measurementFilters = new List<AppFilter>
            {
                new AppFilter { FieldName = "LEASE_ID", Operator = "=", FilterValue = fieldId },
                new AppFilter { FieldName = "MEASUREMENT_DATETIME", Operator = ">=", FilterValue = periodStart.ToString("yyyy-MM-dd") },
                new AppFilter { FieldName = "MEASUREMENT_DATETIME", Operator = "<=", FilterValue = periodEnd.ToString("yyyy-MM-dd") },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = _defaults.GetActiveIndicatorYes() }
            };

            var measurementResults = await measurementRepo.GetAsync(measurementFilters);
            var measurements = measurementResults?.Cast<MEASUREMENT_RECORD>().ToList() ?? new List<MEASUREMENT_RECORD>();
            var productionVolume = measurements.Sum(m => m.NET_VOLUME ?? m.GROSS_VOLUME ?? 0m);

            var revenueRepo = await CreateRepositoryAsync<REVENUE_TRANSACTION>("REVENUE_TRANSACTION");

            var revenueFilters = new List<AppFilter>
            {
                new AppFilter { FieldName = "PROPERTY_ID", Operator = "=", FilterValue = fieldId },
                new AppFilter { FieldName = "TRANSACTION_DATE", Operator = ">=", FilterValue = periodStart.ToString("yyyy-MM-dd") },
                new AppFilter { FieldName = "TRANSACTION_DATE", Operator = "<=", FilterValue = periodEnd.ToString("yyyy-MM-dd") },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = _defaults.GetActiveIndicatorYes() }
            };

            var revenueResults = await revenueRepo.GetAsync(revenueFilters);
            var revenues = revenueResults?.Cast<REVENUE_TRANSACTION>().ToList() ?? new List<REVENUE_TRANSACTION>();
            var revenueVolume = revenues.Sum(r => r.OIL_VOLUME ?? 0m);

            return (productionVolume, revenueVolume);
        }

        private async Task<(decimal grossRevenue, decimal totalRoyalty)> GetRevenueRoyaltyTotalsAsync(
            string fieldId,
            DateTime periodEnd,
            string connectionName)
        {
            var periodStart = new DateTime(periodEnd.Year, periodEnd.Month, 1);

            var revenueRepo = await CreateRepositoryAsync<REVENUE_TRANSACTION>("REVENUE_TRANSACTION");

            var revenueFilters = new List<AppFilter>
            {
                new AppFilter { FieldName = "PROPERTY_ID", Operator = "=", FilterValue = fieldId },
                new AppFilter { FieldName = "TRANSACTION_DATE", Operator = ">=", FilterValue = periodStart.ToString("yyyy-MM-dd") },
                new AppFilter { FieldName = "TRANSACTION_DATE", Operator = "<=", FilterValue = periodEnd.ToString("yyyy-MM-dd") },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = _defaults.GetActiveIndicatorYes() }
            };

            var revenueResults = await revenueRepo.GetAsync(revenueFilters);
            var revenues = revenueResults?.Cast<REVENUE_TRANSACTION>().ToList() ?? new List<REVENUE_TRANSACTION>();
            var grossRevenue = revenues.Sum(r => r.GROSS_REVENUE ?? 0m);

            var royaltyRepo = await CreateRepositoryAsync<ROYALTY_CALCULATION>("ROYALTY_CALCULATION");

            var royaltyFilters = new List<AppFilter>
            {
                new AppFilter { FieldName = "PROPERTY_OR_LEASE_ID", Operator = "=", FilterValue = fieldId },
                new AppFilter { FieldName = "CALCULATION_DATE", Operator = ">=", FilterValue = periodStart.ToString("yyyy-MM-dd") },
                new AppFilter { FieldName = "CALCULATION_DATE", Operator = "<=", FilterValue = periodEnd.ToString("yyyy-MM-dd") },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = _defaults.GetActiveIndicatorYes() }
            };

            var royaltyResults = await royaltyRepo.GetAsync(royaltyFilters);
            var royalties = royaltyResults?.Cast<ROYALTY_CALCULATION>().ToList() ?? new List<ROYALTY_CALCULATION>();
            var totalRoyalty = royalties.Sum(r => r.ROYALTY_AMOUNT ?? 0m);

            return (grossRevenue, totalRoyalty);
        }

        private async Task<RUN_TICKET?> GetRunTicketAsync(string allocationRequestId, string connectionName)
        {
            if (string.IsNullOrWhiteSpace(allocationRequestId))
                return null;

            var repo = await CreateRepositoryAsync<RUN_TICKET>("RUN_TICKET");

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "RUN_TICKET_ID", Operator = "=", FilterValue = allocationRequestId },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = _defaults.GetActiveIndicatorYes() }
            };

            var results = await repo.GetAsync(filters);
            return results?.Cast<RUN_TICKET>().FirstOrDefault();
        }

        private async Task RunImpairmentTestingAsync(
            string fieldId,
            DateTime periodEnd,
            string userId,
            string connectionName)
        {
            if (_impairmentTestingService == null || _reserveAccountingService == null)
            {
                _logger?.LogDebug("Impairment testing skipped; service not configured.");
                return;
            }
            try
            {
                var carryingAmount = await GetCapitalizedCostsAsync(fieldId, periodEnd, connectionName);
                var pv = await _reserveAccountingService.CalculatePresentValueAsync(fieldId, periodEnd, connectionName);

                await _impairmentTestingService.EvaluateImpairmentAsync(
                    fieldId,
                    carryingAmount,
                    pv,
                    pv * 0.95m,
                    periodEnd,
                    userId,
                    connectionName);
            }
            catch (OperationCanceledException)
            {
                _logger?.LogWarning(
                    "Impairment testing cancelled for field {FieldId}",
                    fieldId);
                throw;
            }
        }

        private async Task ApplyDecommissioningAccretionAsync(
            string fieldId,
            DateTime periodEnd,
            string userId,
            string connectionName)
        {
            if (_decommissioningService == null)
                return;
            try
            {
                var repo = await CreateRepositoryAsync<ASSET_RETIREMENT_OBLIGATION>("ASSET_RETIREMENT_OBLIGATION");

                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "FIELD_ID", Operator = "=", FilterValue = fieldId },
                    new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = _defaults.GetActiveIndicatorYes() }
                };

                var results = await repo.GetAsync(filters);
                var obligations = results?.Cast<ASSET_RETIREMENT_OBLIGATION>().ToList()
                    ?? new List<ASSET_RETIREMENT_OBLIGATION>();

                foreach (var aro in obligations)
                {
                    await _decommissioningService.AccreteAroAsync(aro.ARO_ID, periodEnd, userId, connectionName);
                }
            }
            catch (OperationCanceledException)
            {
                _logger?.LogWarning(
                    "ARO accretion cancelled for field {FieldId}",
                    fieldId);
                throw;
            }
        }

        private async Task ApplyFunctionalCurrencyTranslationAsync(
            string fieldId,
            DateTime periodEnd,
            string connectionName)
        {
            if (_functionalCurrencyService == null)
                return;
            try
            {
                await _functionalCurrencyService.TranslateBalancesAsync(fieldId, periodEnd, AccountingCurrencyCodes.Usd, connectionName);
            }
            catch (OperationCanceledException)
            {
                _logger?.LogWarning(
                    "Functional currency translation cancelled for field {FieldId}",
                    fieldId);
                throw;
            }
        }

        private async Task UpdateLeaseRemeasurementsAsync(
            string fieldId,
            DateTime periodEnd,
            string userId,
            string connectionName)
        {
            if (_leasingService == null)
                return;
            try
            {
                var repo = await CreateRepositoryAsync<LEASE_CONTRACT>("LEASE_CONTRACT");

                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "PROPERTY_ID", Operator = "=", FilterValue = fieldId },
                    new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = _defaults.GetActiveIndicatorYes() }
                };

                var results = await repo.GetAsync(filters);
                var leases = results?.Cast<LEASE_CONTRACT>().ToList()
                    ?? new List<LEASE_CONTRACT>();

                foreach (var lease in leases)
                {
                    await _leasingService.RemeasureLeaseAsync(lease.LEASE_ID, periodEnd, userId, connectionName);
                }
            }
            catch (OperationCanceledException)
            {
                _logger?.LogWarning(
                    "Lease remeasurement cancelled for field {FieldId}",
                    fieldId);
                throw;
            }
        }

        private async Task MeasureFinancialInstrumentsAsync(
            DateTime periodEnd,
            string userId,
            string connectionName)
        {
            if (_financialInstrumentsService == null)
                return;
            try
            {
                var repo = await CreateRepositoryAsync<HEDGE_RELATIONSHIP>("HEDGE_RELATIONSHIP");

                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = _defaults.GetActiveIndicatorYes() }
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
                        connectionName);
                }
            }
            catch (OperationCanceledException)
            {
                _logger?.LogWarning(
                    "Hedge measurement cancelled for period {PeriodEnd}",
                    periodEnd);
                throw;
            }
        }

        private async Task UpdateEmissionsObligationsAsync(
            string fieldId,
            DateTime periodEnd,
            string userId,
            string connectionName)
        {
            if (_emissionsTradingService == null)
                return;
            try
            {
                var repo = await CreateRepositoryAsync<EMISSIONS_OBLIGATION>("EMISSIONS_OBLIGATION");

                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = _defaults.GetActiveIndicatorYes() }
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
                        connectionName);
                }
            }
            catch (OperationCanceledException)
            {
                _logger?.LogWarning(
                    "Emissions obligation update cancelled for period {PeriodEnd}",
                    periodEnd);
                throw;
            }
        }

        private async Task ApplyInventoryLcmAsync(
            DateTime periodEnd,
            string userId,
            string connectionName)
        {
            if (_inventoryLcmService == null)
                return;

            try
            {
                var repo = await CreateRepositoryAsync<INVENTORY_ITEM>("INVENTORY_ITEM");

                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = _defaults.GetActiveIndicatorYes() }
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
                        connectionName);
                }
            }
            catch (OperationCanceledException)
            {
                _logger?.LogWarning(
                    "Inventory LCM adjustment cancelled for period {PeriodEnd}",
                    periodEnd);
                throw;
            }
        }

        private async Task RunUnprovedPropertyImpairmentAsync(
            DateTime periodEnd,
            string userId,
            string connectionName)
        {
            if (_unprovedPropertyService == null)
                return;

            try
            {
                var repo = await CreateRepositoryAsync<ACCOUNTING_COST>("ACCOUNTING_COST");

                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "COST_TYPE", Operator = "=", FilterValue = CostTypes.Acquisition },
                    new AppFilter { FieldName = "IS_CAPITALIZED", Operator = "=", FilterValue = _defaults.GetActiveIndicatorYes() },
                    new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = _defaults.GetActiveIndicatorYes() }
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
                        connectionName);
                }
            }
            catch (OperationCanceledException)
            {
                _logger?.LogWarning(
                    "Unproved property impairment testing cancelled for period {PeriodEnd}",
                    periodEnd);
                throw;
            }
        }

        private async Task BuildReserveDisclosuresAsync(
            string fieldId,
            DateTime periodEnd,
            string connectionName)
        {
            if (_reserveDisclosureService == null)
                return;
            try
            {
                await _reserveDisclosureService.BuildDisclosureAsync(fieldId, periodEnd, connectionName);
            }
            catch (OperationCanceledException)
            {
                _logger?.LogWarning(
                    "Reserve disclosure build cancelled for field {FieldId}",
                    fieldId);
                throw;
            }
        }

        private async Task<decimal> GetCapitalizedCostsAsync(
            string fieldId,
            DateTime periodEnd,
            string connectionName)
        {
            var repo = await CreateRepositoryAsync<ACCOUNTING_COST>("ACCOUNTING_COST");

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "PROPERTY_ID", Operator = "=", FilterValue = fieldId },
                new AppFilter { FieldName = "COST_DATE", Operator = "<=", FilterValue = periodEnd.ToString("yyyy-MM-dd") },
                new AppFilter { FieldName = "IS_CAPITALIZED", Operator = "=", FilterValue = _defaults.GetActiveIndicatorYes() },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = _defaults.GetActiveIndicatorYes() }
            };

            var results = await repo.GetAsync(filters);
            var costs = results?.Cast<ACCOUNTING_COST>().ToList()
                ?? new List<ACCOUNTING_COST>();

            return costs.Sum(c => c.AMOUNT);
        }
        private async Task<string> ResolveConnectionAsync()
        {
            var connection = await _resolveConnection();
            if (string.IsNullOrWhiteSpace(connection))
                throw new InvalidOperationException("A PRODUCTION database binding is required.");
            return connection;
        }

        private async Task<PPDMGenericRepository> CreateRepositoryAsync<T>(string tableName)
        {
            var connection = await ResolveConnectionAsync();
            return new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                typeof(T), connection, tableName);
        }
    }
}
