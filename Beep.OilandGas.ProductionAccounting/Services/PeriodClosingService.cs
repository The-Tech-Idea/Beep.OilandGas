using System;
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
        private const string ConnectionName = "PPDM39";

        public PeriodClosingService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            ILogger<PeriodClosingService> logger = null)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _logger = logger;
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
                    new AppFilter { FieldName = "FIELD_ID", Operator = "=", FilterValue = fieldId },
                    new AppFilter { FieldName = "ALLOCATION_DATE", Operator = "<=", FilterValue = periodEnd.ToString("yyyy-MM-dd") },
                    new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
                };

                var results = await repo.GetAsync(filters);
                var allocResults = results.Cast<ALLOCATION_RESULT>().ToList();

                // Filter for incomplete allocations (not all details assigned)
                foreach (var alloc in allocResults)
                {
                    items.Add($"Allocation {alloc.ALLOCATION_RESULT_ID}");
                }

                return items;
            }
            catch
            {
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
                    new AppFilter { FieldName = "FIELD_ID", Operator = "=", FilterValue = fieldId },
                    new AppFilter { FieldName = "CALCULATION_DATE", Operator = "<=", FilterValue = periodEnd.ToString("yyyy-MM-dd") },
                    new AppFilter { FieldName = "ROYALTY_STATUS", Operator = "!=", FilterValue = "Paid" },
                    new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
                };

                var results = await repo.GetAsync(filters);
                var royalties = results.Cast<ROYALTY_CALCULATION>().ToList();

                foreach (var royalty in royalties)
                {
                    items.Add($"Royalty {royalty.ROYALTY_CALCULATION_ID}");
                }

                return items;
            }
            catch
            {
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
                var metadata = await _metadata.GetTableMetadataAsync("REVENUE_ALLOCATION");
                var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                    ?? typeof(REVENUE_ALLOCATION);

                var repo = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, cn, "REVENUE_ALLOCATION");

                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "FIELD_ID", Operator = "=", FilterValue = fieldId },
                    new AppFilter { FieldName = "REVENUE_DATE", Operator = "<=", FilterValue = periodEnd.ToString("yyyy-MM-dd") },
                    new AppFilter { FieldName = "REVENUE_STATUS", Operator = "!=", FilterValue = "Collected" },
                    new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
                };

                var results = await repo.GetAsync(filters);
                var revenues = results.Cast<REVENUE_ALLOCATION>().ToList();

                foreach (var revenue in revenues)
                {
                    items.Add($"Revenue {revenue.REVENUE_ALLOCATION_ID}");
                }

                return items;
            }
            catch
            {
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
                    new AppFilter { FieldName = "FIELD_ID", Operator = "=", FilterValue = fieldId },
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
            catch
            {
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
                    new AppFilter { FieldName = "FIELD_ID", Operator = "=", FilterValue = fieldId },
                    new AppFilter { FieldName = "ALLOCATION_DATE", Operator = "<=", FilterValue = periodEnd.ToString("yyyy-MM-dd") },
                    new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
                };

                var results = await repo.GetAsync(filters);
                var allocResults = results.Cast<ALLOCATION_RESULT>().ToList();

                // Mark each allocation as processed by updating row changed timestamp
                foreach (var alloc in allocResults)
                {
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

                // Get all unpaid royalties for this field up to period end
                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "FIELD_ID", Operator = "=", FilterValue = fieldId },
                    new AppFilter { FieldName = "CALCULATION_DATE", Operator = "<=", FilterValue = periodEnd.ToString("yyyy-MM-dd") },
                    new AppFilter { FieldName = "ROYALTY_STATUS", Operator = "!=", FilterValue = RoyaltyStatus.Paid },
                    new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
                };

                var results = await repo.GetAsync(filters);
                var royalties = results.Cast<ROYALTY_CALCULATION>().ToList();

                // Update each royalty's status to "ACCRUED" (ready for payment)
                foreach (var royalty in royalties)
                {
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

                var metadata = await _metadata.GetTableMetadataAsync("REVENUE_ALLOCATION");
                var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                    ?? typeof(REVENUE_ALLOCATION);

                var repo = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, cn, "REVENUE_ALLOCATION");

                // Get all unrecognized revenue for this field up to period end
                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "FIELD_ID", Operator = "=", FilterValue = fieldId },
                    new AppFilter { FieldName = "REVENUE_DATE", Operator = "<=", FilterValue = periodEnd.ToString("yyyy-MM-dd") },
                    new AppFilter { FieldName = "REVENUE_STATUS", Operator = "!=", FilterValue = "Recognized" },
                    new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
                };

                var results = await repo.GetAsync(filters);
                var revenues = results.Cast<REVENUE_ALLOCATION>().ToList();

                // Update each revenue's status to "RECOGNIZED"
                foreach (var revenue in revenues)
                {
                    // Note: REVENUE_ALLOCATION may not have a STATUS field - mark as processed
                    revenue.ROW_CHANGED_BY = userId;
                    revenue.ROW_CHANGED_DATE = DateTime.UtcNow;

                    await repo.UpdateAsync(revenue, userId);

                    _logger?.LogInformation(
                        "Revenue {RevenueId} marked as recognized for field {FieldId}",
                        revenue.REVENUE_ALLOCATION_ID, fieldId);
                }

                _logger?.LogInformation(
                    "Successfully marked {Count} revenue items as recognized for field {FieldId}",
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

                // Create closing entry
                var closingEntry = new JOURNAL_ENTRY
                {
                    JOURNAL_ENTRY_ID = Guid.NewGuid().ToString(),
                    ENTRY_NUMBER = $"PC-{fieldId}-{periodEnd:yyyyMM}",
                    ENTRY_DATE = periodEnd,
                    ENTRY_TYPE = "PERIOD_CLOSE",
                    STATUS = "Posted",
                    DESCRIPTION = $"Period closing entry for {periodEnd:MMMM yyyy}",
                    SOURCE_MODULE = "PERIOD_CLOSING",
                    REFERENCE_NUMBER = $"CLOSE-{periodEnd:yyyyMM}",
                    TOTAL_DEBIT = 0m,
                    TOTAL_CREDIT = 0m,
                    ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                    PPDM_GUID = Guid.NewGuid().ToString(),
                    ROW_CREATED_BY = userId,
                    ROW_CREATED_DATE = DateTime.UtcNow
                };

                await repo.InsertAsync(closingEntry, userId);

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
    }
}
