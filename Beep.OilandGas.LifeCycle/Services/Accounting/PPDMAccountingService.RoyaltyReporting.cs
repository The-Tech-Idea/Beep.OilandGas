using System.Globalization;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.PPDM39.Core;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.LifeCycle.Services.Accounting;

public partial class PPDMAccountingService
{
    private async Task<string> ResolveProductionConnectionAsync()
    {
        var connection = _resolveProductionConnection is null ? null : await _resolveProductionConnection();
        if (string.IsNullOrWhiteSpace(connection))
            throw new InvalidOperationException("Royalty operations require a configured PRODUCTION module database.");
        return connection;
    }

    /// <summary>Reads recorded royalties by source-ticket field and inclusive calculation dates.</summary>
    public async Task<List<ROYALTY_CALCULATION>> GetRoyaltyCalculationsAsync(
        string fieldId, DateTime? startDate = null, DateTime? endDate = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fieldId);
        if (startDate?.Date == DateTime.MinValue.Date || endDate?.Date == DateTime.MaxValue.Date ||
            (startDate.HasValue && endDate.HasValue && startDate.Value.Date > endDate.Value.Date))
            throw new ArgumentException("A valid inclusive calculation date range is required.");

        var connection = await ResolveProductionConnectionAsync();
        async Task<List<T>> ReadAsync<T>(string table, List<AppFilter> filters) =>
            (await new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                typeof(T), connection, table).GetAsync(filters)).OfType<T>().ToList();
        static AppFilter Equal(string field, string value) => new() { FieldName = field, Operator = "=", FilterValue = value };
        static string RequireUnique(string? id, HashSet<string> seen)
        {
            if (string.IsNullOrWhiteSpace(id) || !seen.Add(id))
                throw new InvalidOperationException("Royalty source relationships contain missing or duplicate identities.");
            return id;
        }

        var tickets = await ReadAsync<RUN_TICKET>("RUN_TICKET", new() { Equal("FIELD_ID", fieldId) });
        var ticketIds = new HashSet<string>(StringComparer.Ordinal);
        var allocationIds = new HashSet<string>(StringComparer.Ordinal);
        var detailIds = new HashSet<string>(StringComparer.Ordinal);
        var calculationIds = new HashSet<string>(StringComparer.Ordinal);
        var result = new List<ROYALTY_CALCULATION>();
        // Archived source records remain valid provenance for a recorded financial obligation.
        // Only calculation records are filtered by ACTIVE_IND; parent records are not hidden.
        foreach (var ticket in tickets)
        {
            var ticketId = RequireUnique(ticket.RUN_TICKET_ID, ticketIds);
            var allocations = await ReadAsync<ALLOCATION_RESULT>("ALLOCATION_RESULT", new() { Equal("ALLOCATION_REQUEST_ID", ticketId) });
            foreach (var allocation in allocations)
            {
                var allocationId = RequireUnique(allocation.ALLOCATION_RESULT_ID, allocationIds);
                var details = await ReadAsync<ALLOCATION_DETAIL>("ALLOCATION_DETAIL", new() { Equal("ALLOCATION_RESULT_ID", allocationId) });
                foreach (var detail in details)
                {
                    var detailId = RequireUnique(detail.ALLOCATION_DETAIL_ID, detailIds);
                    var filters = new List<AppFilter> { Equal("ALLOCATION_DETAIL_ID", detailId), Equal("ACTIVE_IND", "Y") };
                    if (startDate.HasValue) filters.Add(new() { FieldName = "CALCULATION_DATE", Operator = ">=",
                        FilterValue = startDate.Value.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) });
                    if (endDate.HasValue) filters.Add(new() { FieldName = "CALCULATION_DATE", Operator = "<",
                        FilterValue = endDate.Value.Date.AddDays(1).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) });
                    foreach (var calculation in await ReadAsync<ROYALTY_CALCULATION>("ROYALTY_CALCULATION", filters))
                    {
                        RequireUnique(calculation.ROYALTY_CALCULATION_ID, calculationIds);
                        result.Add(calculation);
                    }
                }
            }
        }
        return result.OrderByDescending(r => r.CALCULATION_DATE)
            .ThenBy(r => r.ROYALTY_CALCULATION_ID, StringComparer.Ordinal).ToList();
    }
}
