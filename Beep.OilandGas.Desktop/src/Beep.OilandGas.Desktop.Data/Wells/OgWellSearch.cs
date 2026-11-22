using System.Globalization;
using Beep.OilandGas.PPDM39.Models;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Core.Wells;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Data.Wells;

/// <summary>WEL-01: the well index's data side.</summary>
public interface IOgWellSearch
{
    Task<OgResult<OgPage<OgWellSummary>>> SearchAsync(
        OgWellQuery query,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// The well index over the PPDM <c>WELL</c> and <c>WELL_XREF</c> tables, read through their entity models.
/// </summary>
/// <remarks>
/// The rows come from a <see cref="UnitofWork{T}"/> and the filtering, ordering and paging are LINQ over those
/// rows, so the assembly contains no statement text. A missing table or a provider error is a failed result
/// carrying a reference, never an empty page (AC-3).
/// </remarks>
public sealed class OgWellSearch : IOgWellSearch
{
    public const int DefaultPageSize = 100;
    public const int MaximumPageSize = 500;

    /// <summary>Code returned when the provider cannot be read.</summary>
    public const string CodeReadFailed = "well-search-failed";

    /// <summary>Code returned when no connection is open.</summary>
    public const string CodeNoConnection = "well-search-no-connection";

    private readonly Func<IDataSource?> _source;
    private readonly IFailureReporter _failures;

    public OgWellSearch(Func<IDataSource?> source, IFailureReporter failures)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _failures = failures ?? throw new ArgumentNullException(nameof(failures));
    }

    public async Task<OgResult<OgPage<OgWellSummary>>> SearchAsync(
        OgWellQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var source = _source();

        if (source is null)
        {
            return OgResult<OgPage<OgWellSummary>>.Failed(
                "No connection is open, so wells cannot be searched.", CodeNoConnection);
        }

        var page = Math.Max(0, query.Page);
        var size = Math.Clamp(query.PageSize <= 0 ? DefaultPageSize : query.PageSize, 1, MaximumPageSize);

        try
        {
            var wells = await OgUow.ReadAllAsync<WELL>(source).ConfigureAwait(false);
            var xrefs = await OgUow.ReadAllAsync<WELL_XREF>(source).ConfigureAwait(false);

            cancellationToken.ThrowIfCancellationRequested();

            var matching = Filter(wells, xrefs, query)
                .OrderBy(well => well.WELL_NAME, StringComparer.OrdinalIgnoreCase)
                .ThenBy(well => well.UWI, StringComparer.OrdinalIgnoreCase)
                .ToList();

            var apiNumbers = ApiNumbers(xrefs);

            var items = matching
                .Skip(page * size)
                .Take(size)
                .Select(well => ToSummary(well, apiNumbers))
                .ToList();

            return OgResult<OgPage<OgWellSummary>>.Ok(
                new OgPage<OgWellSummary>(items, page, size, matching.Count, matching.Count));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                "searching wells",
                "the well index is not shown",
                FailureSeverity.Error);

            return OgResult<OgPage<OgWellSummary>>.Failed(
                "The well index could not be read; the grid shows a failed state rather than no wells.",
                reference ?? CodeReadFailed);
        }
    }

    /// <summary>
    /// The rows a query selects. Free text finds the name, the UWI or any recorded alternate identity; every
    /// other filter that is set must also match (AC-1).
    /// </summary>
    public static IEnumerable<WELL> Filter(
        IEnumerable<WELL> wells,
        IEnumerable<WELL_XREF> xrefs,
        OgWellQuery query)
    {
        ArgumentNullException.ThrowIfNull(wells);
        ArgumentNullException.ThrowIfNull(xrefs);
        ArgumentNullException.ThrowIfNull(query);

        var identities = xrefs
            .Where(xref => !string.IsNullOrWhiteSpace(xref.UWI))
            .GroupBy(xref => xref.UWI, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.ToList(), StringComparer.OrdinalIgnoreCase);

        var text = string.IsNullOrWhiteSpace(query.Text) ? null : query.Text.Trim();
        var field = string.IsNullOrWhiteSpace(query.Field) ? null : query.Field.Trim();
        var status = string.IsNullOrWhiteSpace(query.Status) ? null : query.Status.Trim();
        var type = string.IsNullOrWhiteSpace(query.WellType) ? null : query.WellType.Trim();

        return wells.Where(well =>
            (text is null || Matches(well, identities, text))
            && (field is null || string.Equals(well.ASSIGNED_FIELD, field, StringComparison.OrdinalIgnoreCase))
            && (status is null || string.Equals(well.CURRENT_STATUS, status, StringComparison.OrdinalIgnoreCase))
            && (type is null || string.Equals(well.WELL_LEVEL_TYPE, type, StringComparison.OrdinalIgnoreCase))
            && (query.SpudYearFrom is not { } from || (well.SPUD_DATE?.Year ?? int.MinValue) >= from)
            && (query.SpudYearTo is not { } to || (well.SPUD_DATE?.Year ?? int.MaxValue) <= to));
    }

    private static bool Matches(
        WELL well,
        IReadOnlyDictionary<string, List<WELL_XREF>> identities,
        string text) =>
        Contains(well.WELL_NAME, text)
        || Contains(well.UWI, text)
        || (well.UWI is not null
            && identities.TryGetValue(well.UWI, out var rows)
            && rows.Any(row => Contains(row.XREF_ID, text)));

    private static bool Contains(string? value, string text) =>
        value?.Contains(text, StringComparison.OrdinalIgnoreCase) == true;

    private static Dictionary<string, string> ApiNumbers(IEnumerable<WELL_XREF> xrefs) =>
        xrefs
            .Where(xref => !string.IsNullOrWhiteSpace(xref.UWI))
            .Where(xref => xref.XREF_TYPE?.Contains("API", StringComparison.OrdinalIgnoreCase) == true)
            .GroupBy(xref => xref.UWI, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                group => group.Key,
                group => group.Select(xref => xref.XREF_ID).FirstOrDefault(id => !string.IsNullOrWhiteSpace(id)) ?? string.Empty,
                StringComparer.OrdinalIgnoreCase);

    private static OgWellSummary ToSummary(WELL well, IReadOnlyDictionary<string, string> apiNumbers)
    {
        var api = well.UWI is not null && apiNumbers.TryGetValue(well.UWI, out var number)
            ? number
            : null;

        return new OgWellSummary(
            Id: well.UWI ?? string.Empty,
            Name: well.WELL_NAME ?? string.Empty,
            ApiNumber: string.IsNullOrWhiteSpace(api) ? null : api,
            Field: well.ASSIGNED_FIELD,
            Status: well.CURRENT_STATUS,
            WellType: well.WELL_LEVEL_TYPE,
            SpudDate: well.SPUD_DATE,
            LastActivity: well.CURRENT_STATUS_DATE);
    }
}
