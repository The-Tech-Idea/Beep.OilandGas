using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.ProductionAccounting.Constants;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.ProductionAccounting.Services
{
    public partial class PeriodClosingService
    {
        /// <summary>
        /// A field's production records, as the close checks and closes them. A run ticket names its field
        /// (<c>RUN_TICKET.FIELD_ID</c>); what follows a ticket names the ticket or its allocation, never the field: an
        /// allocation result its ticket (<c>ALLOCATION_REQUEST_ID</c>), a measurement its ticket (<c>RUN_TICKET_ID</c>), a
        /// royalty calculation and a revenue transaction their allocation result (<c>ALLOCATION_RESULT_ID</c>).
        /// </summary>
        /// <remarks>
        /// Their lease columns (<c>RUN_TICKET.LEASE_ID</c>, <c>MEASUREMENT_RECORD.LEASE_ID</c>,
        /// <c>REVENUE_TRANSACTION.PROPERTY_ID</c>, <c>ROYALTY_CALCULATION.PROPERTY_OR_LEASE_ID</c>) hold the lease the ticket
        /// was measured on, which is not the field: a field holds many leases and a lease may cross fields. The close matched
        /// those columns against the field id, so it checked and closed nothing of a field whose leases are not named after
        /// it, and everything of a lease that happened to be. Which field a record belongs to does not depend on whether its
        /// ticket or allocation was archived (as for royalty reporting); the records checked and closed are the active ones.
        /// </remarks>
        private sealed class FieldProduction
        {
            public FieldProduction(IEnumerable<string> ticketIds, IEnumerable<ALLOCATION_RESULT> allocations)
            {
                TicketIds = new HashSet<string>(ticketIds, StringComparer.OrdinalIgnoreCase);
                Allocations = allocations.ToList();
                AllocationIds = new HashSet<string>(
                    Allocations.Select(a => a.ALLOCATION_RESULT_ID).Where(id => !string.IsNullOrWhiteSpace(id)),
                    StringComparer.OrdinalIgnoreCase);
            }

            public HashSet<string> TicketIds { get; }

            /// <summary>The allocation results made from the field's run tickets.</summary>
            public List<ALLOCATION_RESULT> Allocations { get; }

            public HashSet<string> AllocationIds { get; }

            public bool HasTicket(string ticketId) =>
                !string.IsNullOrWhiteSpace(ticketId) && TicketIds.Contains(ticketId);

            public bool HasAllocation(string allocationResultId) =>
                !string.IsNullOrWhiteSpace(allocationResultId) && AllocationIds.Contains(allocationResultId);
        }

        private async Task<FieldProduction> ReadFieldProductionAsync(string fieldId)
        {
            var ticketRepo = await CreateRepositoryAsync<RUN_TICKET>("RUN_TICKET");
            var tickets = await ticketRepo.GetAsync(new List<AppFilter>
            {
                new AppFilter { FieldName = "FIELD_ID", Operator = "=", FilterValue = fieldId }
            });
            var ticketIds = (tickets?.OfType<RUN_TICKET>() ?? Enumerable.Empty<RUN_TICKET>())
                .Select(t => t.RUN_TICKET_ID)
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .ToList();
            if (ticketIds.Count == 0)
                return new FieldProduction(ticketIds, Enumerable.Empty<ALLOCATION_RESULT>());

            var fieldTickets = new HashSet<string>(ticketIds, StringComparer.OrdinalIgnoreCase);
            var allocationRepo = await CreateRepositoryAsync<ALLOCATION_RESULT>("ALLOCATION_RESULT");
            var allocations = await allocationRepo.GetAsync(new List<AppFilter>());
            return new FieldProduction(
                ticketIds,
                (allocations?.OfType<ALLOCATION_RESULT>() ?? Enumerable.Empty<ALLOCATION_RESULT>())
                    .Where(a => !string.IsNullOrWhiteSpace(a.ALLOCATION_REQUEST_ID)
                        && fieldTickets.Contains(a.ALLOCATION_REQUEST_ID)));
        }

        private bool IsActive(string activeIndicator) =>
            string.Equals(activeIndicator, _defaults.GetActiveIndicatorYes(), StringComparison.OrdinalIgnoreCase);

        /// <summary>On or before the period end's day.</summary>
        private static bool OnOrBefore(DateTime? date, DateTime periodEnd) =>
            date.HasValue && date.Value.Date <= periodEnd.Date;

        private static DateTime PeriodStartOf(DateTime periodEnd) => new DateTime(periodEnd.Year, periodEnd.Month, 1);

        /// <summary>The field's active allocation results made on or before the period end.</summary>
        private List<ALLOCATION_RESULT> AllocationsToClose(FieldProduction production, DateTime periodEnd) =>
            production.Allocations
                .Where(a => IsActive(a.ACTIVE_IND) && OnOrBefore(a.ALLOCATION_DATE, periodEnd))
                .ToList();

        /// <summary>The field's active royalty calculations dated from <paramref name="from"/> (when given) to the period end.</summary>
        private async Task<(PPDMGenericRepository Repository, List<ROYALTY_CALCULATION> Royalties)> ReadFieldRoyaltiesAsync(
            FieldProduction production, DateTime? from, DateTime periodEnd)
        {
            var repo = await CreateRepositoryAsync<ROYALTY_CALCULATION>("ROYALTY_CALCULATION");
            if (production.AllocationIds.Count == 0)
                return (repo, new List<ROYALTY_CALCULATION>());

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "CALCULATION_DATE", Operator = "<=", FilterValue = periodEnd.ToString("yyyy-MM-dd") },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = _defaults.GetActiveIndicatorYes() }
            };
            if (from.HasValue)
                filters.Add(new AppFilter { FieldName = "CALCULATION_DATE", Operator = ">=", FilterValue = from.Value.ToString("yyyy-MM-dd") });

            var rows = await repo.GetAsync(filters);
            return (repo, (rows?.OfType<ROYALTY_CALCULATION>() ?? Enumerable.Empty<ROYALTY_CALCULATION>())
                .Where(r => production.HasAllocation(r.ALLOCATION_RESULT_ID))
                .ToList());
        }

        /// <summary>The field's active revenue transactions dated from <paramref name="from"/> (when given) to the period end.</summary>
        private async Task<(PPDMGenericRepository Repository, List<REVENUE_TRANSACTION> Revenue)> ReadFieldRevenueAsync(
            FieldProduction production, DateTime? from, DateTime periodEnd)
        {
            var repo = await CreateRepositoryAsync<REVENUE_TRANSACTION>("REVENUE_TRANSACTION");
            if (production.AllocationIds.Count == 0)
                return (repo, new List<REVENUE_TRANSACTION>());

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "TRANSACTION_DATE", Operator = "<=", FilterValue = periodEnd.ToString("yyyy-MM-dd") },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = _defaults.GetActiveIndicatorYes() }
            };
            if (from.HasValue)
                filters.Add(new AppFilter { FieldName = "TRANSACTION_DATE", Operator = ">=", FilterValue = from.Value.ToString("yyyy-MM-dd") });

            var rows = await repo.GetAsync(filters);
            return (repo, (rows?.OfType<REVENUE_TRANSACTION>() ?? Enumerable.Empty<REVENUE_TRANSACTION>())
                .Where(r => production.HasAllocation(r.ALLOCATION_RESULT_ID))
                .ToList());
        }

        private async Task<List<string>> GetUnreconciledAllocationsAsync(
            string fieldId,
            FieldProduction production,
            DateTime periodEnd)
        {
            var items = new List<string>();
            var detailRepo = await CreateRepositoryAsync<ALLOCATION_DETAIL>("ALLOCATION_DETAIL");

            // Incomplete allocations: those with no details.
            foreach (var alloc in AllocationsToClose(production, periodEnd))
            {
                var details = await detailRepo.GetAsync(new List<AppFilter>
                {
                    new AppFilter { FieldName = "ALLOCATION_RESULT_ID", Operator = "=", FilterValue = alloc.ALLOCATION_RESULT_ID },
                    new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = _defaults.GetActiveIndicatorYes() }
                });
                if (!(details?.OfType<ALLOCATION_DETAIL>().Any() ?? false))
                    items.Add($"Allocation {alloc.ALLOCATION_RESULT_ID} has no details");
            }

            _logger?.LogDebug("Field {FieldId}: {Count} allocations without details", fieldId, items.Count);
            return items;
        }

        private async Task<List<string>> GetUnreconciledRoyaltiesAsync(
            string fieldId,
            FieldProduction production,
            DateTime periodEnd)
        {
            var (_, royalties) = await ReadFieldRoyaltiesAsync(production, from: null, periodEnd);
            var items = royalties
                .Where(royalty =>
                    !string.Equals(royalty.ROYALTY_STATUS, RoyaltyStatus.Accrued, StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(royalty.ROYALTY_STATUS, RoyaltyStatus.Paid, StringComparison.OrdinalIgnoreCase))
                .Select(royalty => $"Royalty {royalty.ROYALTY_CALCULATION_ID}")
                .ToList();

            _logger?.LogDebug("Field {FieldId}: {Count} royalties neither accrued nor paid", fieldId, items.Count);
            return items;
        }

        private async Task<List<string>> GetUnreconciledRevenueAsync(
            string fieldId,
            FieldProduction production,
            DateTime periodEnd)
        {
            var items = new List<string>();
            var (_, transactions) = await ReadFieldRevenueAsync(production, from: null, periodEnd);
            if (transactions.Count == 0)
                return items;

            var allocationRepo = await CreateRepositoryAsync<REVENUE_ALLOCATION>("REVENUE_ALLOCATION");
            var allocationResults = await allocationRepo.GetAsync(new List<AppFilter>
            {
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = _defaults.GetActiveIndicatorYes() }
            });

            var allocatedTransactionIds = new HashSet<string>(
                (allocationResults?.OfType<REVENUE_ALLOCATION>() ?? Enumerable.Empty<REVENUE_ALLOCATION>())
                    .Select(a => a.REVENUE_TRANSACTION_ID)
                    .Where(id => !string.IsNullOrWhiteSpace(id)),
                StringComparer.OrdinalIgnoreCase);

            foreach (var transaction in transactions)
            {
                if (string.IsNullOrWhiteSpace(transaction.REVENUE_TRANSACTION_ID)
                    || !allocatedTransactionIds.Contains(transaction.REVENUE_TRANSACTION_ID))
                    items.Add($"Revenue transaction {transaction.REVENUE_TRANSACTION_ID} missing allocation");
            }

            _logger?.LogDebug("Field {FieldId}: {Count} revenue transactions without allocations", fieldId, items.Count);
            return items;
        }

        private async Task<List<string>> GetReconciliationIssuesAsync(FieldProduction production, DateTime periodEnd)
        {
            var issues = new List<string>();

            var (productionVolume, revenueVolume) = await GetProductionRevenueVolumesAsync(production, periodEnd);
            var volumeVariance = productionVolume - revenueVolume;
            var volumeTolerance = Math.Max(1m, productionVolume * 0.001m);
            if (Math.Abs(volumeVariance) > volumeTolerance)
            {
                issues.Add($"Production vs revenue volume variance: {volumeVariance} (production={productionVolume}, revenue={revenueVolume})");
            }

            var (grossRevenue, totalRoyalty) = await GetRevenueRoyaltyTotalsAsync(production, periodEnd);
            if (totalRoyalty > grossRevenue + 0.01m)
            {
                issues.Add($"Royalty exceeds gross revenue: royalty={totalRoyalty}, gross={grossRevenue}");
            }

            return issues;
        }

        /// <summary>The month's measured volume of the field's run tickets, and the volume its revenue was recorded for.</summary>
        private async Task<(decimal productionVolume, decimal revenueVolume)> GetProductionRevenueVolumesAsync(
            FieldProduction production,
            DateTime periodEnd)
        {
            if (production.TicketIds.Count == 0)
                return (0m, 0m);

            var periodStart = PeriodStartOf(periodEnd);
            var measurementRepo = await CreateRepositoryAsync<MEASUREMENT_RECORD>("MEASUREMENT_RECORD");
            var measurementResults = await measurementRepo.GetAsync(new List<AppFilter>
            {
                new AppFilter { FieldName = "MEASUREMENT_DATETIME", Operator = ">=", FilterValue = periodStart.ToString("yyyy-MM-dd") },
                new AppFilter { FieldName = "MEASUREMENT_DATETIME", Operator = "<=", FilterValue = periodEnd.ToString("yyyy-MM-dd") },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = _defaults.GetActiveIndicatorYes() }
            });
            var productionVolume = (measurementResults?.OfType<MEASUREMENT_RECORD>() ?? Enumerable.Empty<MEASUREMENT_RECORD>())
                .Where(m => production.HasTicket(m.RUN_TICKET_ID))
                .Sum(m => m.NET_VOLUME ?? m.GROSS_VOLUME ?? 0m);

            var (_, revenues) = await ReadFieldRevenueAsync(production, periodStart, periodEnd);
            var revenueVolume = revenues.Sum(r => r.OIL_VOLUME ?? 0m);

            return (productionVolume, revenueVolume);
        }

        /// <summary>The month's gross revenue and royalty of the field's production.</summary>
        private async Task<(decimal grossRevenue, decimal totalRoyalty)> GetRevenueRoyaltyTotalsAsync(
            FieldProduction production,
            DateTime periodEnd)
        {
            var periodStart = PeriodStartOf(periodEnd);
            var (_, revenues) = await ReadFieldRevenueAsync(production, periodStart, periodEnd);
            var (_, royalties) = await ReadFieldRoyaltiesAsync(production, periodStart, periodEnd);
            return (revenues.Sum(r => r.GROSS_REVENUE ?? 0m), royalties.Sum(r => r.ROYALTY_AMOUNT ?? 0m));
        }

        private async Task MarkAllocationsClosed(
            string fieldId,
            FieldProduction production,
            DateTime periodEnd,
            string userId)
        {
            _logger?.LogInformation("Marking allocations closed for field {FieldId} as of {PeriodEnd}", fieldId, periodEnd.ToShortDateString());

            var repo = await CreateRepositoryAsync<ALLOCATION_RESULT>("ALLOCATION_RESULT");
            var allocations = AllocationsToClose(production, periodEnd);

            // Mark each allocation as processed by updating row changed timestamp
            foreach (var alloc in allocations)
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
                allocations.Count, fieldId);
        }

        private async Task MarkRoyaltiesClosed(
            string fieldId,
            FieldProduction production,
            DateTime periodEnd,
            string userId)
        {
            _logger?.LogInformation("Marking royalties closed for field {FieldId} as of {PeriodEnd}", fieldId, periodEnd.ToShortDateString());

            var (repo, royalties) = await ReadFieldRoyaltiesAsync(production, from: null, periodEnd);

            // Update each royalty's status to "ACCRUED" (ready for payment)
            var accrued = 0;
            foreach (var royalty in royalties)
            {
                if (string.Equals(royalty.ROYALTY_STATUS, RoyaltyStatus.Accrued, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(royalty.ROYALTY_STATUS, RoyaltyStatus.Paid, StringComparison.OrdinalIgnoreCase))
                    continue;

                royalty.ROYALTY_STATUS = RoyaltyStatus.Accrued;
                royalty.ROW_CHANGED_BY = userId;
                royalty.ROW_CHANGED_DATE = DateTime.UtcNow;
                await repo.UpdateAsync(royalty, userId);
                accrued++;

                _logger?.LogInformation(
                    "Royalty {RoyaltyId} marked as accrued for field {FieldId}",
                    royalty.ROYALTY_CALCULATION_ID, fieldId);
            }

            _logger?.LogInformation(
                "Successfully marked {Count} royalties as accrued for field {FieldId}",
                accrued, fieldId);
        }

        private async Task MarkRevenueClosed(
            string fieldId,
            FieldProduction production,
            DateTime periodEnd,
            string userId)
        {
            _logger?.LogInformation("Marking revenue closed for field {FieldId} as of {PeriodEnd}", fieldId, periodEnd.ToShortDateString());

            var (repo, revenues) = await ReadFieldRevenueAsync(production, from: null, periodEnd);

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
    }
}
