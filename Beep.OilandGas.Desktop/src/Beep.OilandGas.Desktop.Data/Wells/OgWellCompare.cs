using System.Globalization;
using System.Text;
using Beep.OilandGas.PPDM39.Models;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Data.Wells;

/// <summary>One well in the comparison.</summary>
public sealed record OgCompareWell(string Uwi, string? Name);

/// <summary>One cell: what is shown, the value behind it, and why it is not comparable.</summary>
public sealed record OgCompareCell(string Text, decimal? Value, bool Missing, bool UnitMismatch);

/// <summary>One row of the matrix: a property across the selected wells, plus the deltas against the first well.</summary>
public sealed record OgComparisonRow(
    string PropertyKey,
    string Label,
    string Category,
    string Unit,
    IReadOnlyList<OgCompareCell> Values,
    IReadOnlyList<string> Deltas,
    bool UnitMismatch)
{
    /// <summary>One sentence for the difference column (empty when nothing can be compared).</summary>
    public string DifferenceText => Deltas.Count == 0 ? string.Empty : string.Join("; ", Deltas);
}

/// <summary>The comparison matrix, and whether the schematic overlay can be offered.</summary>
public sealed record OgComparisonMatrix(
    IReadOnlyList<OgCompareWell> Wells,
    IReadOnlyList<OgComparisonRow> Rows,
    bool CanOverlay,
    string? OverlayReason)
{
    /// <summary>AC-3: the export uses this matrix's own rows and columns.</summary>
    public string ToCsv()
    {
        var builder = new StringBuilder();
        builder.Append("Property,Category,Unit");
        builder.Append(',').AppendJoin(',', Wells.Select(well => Csv(well.Uwi)));
        builder.Append(",Difference\n");

        foreach (var row in Rows)
        {
            builder.Append(Csv(row.Label)).Append(',')
                .Append(Csv(row.Category)).Append(',')
                .Append(Csv(row.Unit));

            foreach (var cell in row.Values)
            {
                builder.Append(',').Append(Csv(cell.Text));
            }

            builder.Append(',').Append(Csv(row.DifferenceText)).Append('\n');
        }

        return builder.ToString();
    }

    private static string Csv(string? value) =>
        value is not null && (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
            ? '"' + value.Replace("\"", "\"\"") + '"'
            : value ?? string.Empty;
}

/// <summary>WEL-06: compare two to six wells on named properties, with units explicit.</summary>
public interface IOgWellComparison
{
    /// <summary>The wells the picker offers.</summary>
    Task<OgResult<IReadOnlyList<OgCompareWell>>> ListWellsAsync(
        int take = 250,
        CancellationToken cancellationToken = default);

    /// <summary>Compares the selected wells; fewer than two is refused.</summary>
    Task<OgResult<OgComparisonMatrix>> CompareAsync(
        IReadOnlyList<string> uwis,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// The comparison over <c>WELL</c> and the well's stratigraphic picks, read through their entity models.
/// </summary>
/// <remarks>
/// A numeric delta is only produced when every well has a value and they share a unit (AC-1). A missing value is
/// shown as "-" and never as zero (AC-2). The schematic overlay is offered only when every selected well has the
/// geometry it needs, and the refusal names the well and the missing part (AC-4).
/// </remarks>
public sealed class OgWellComparison : IOgWellComparison
{
    public const string CodeNoConnection = "well-compare-no-connection";
    public const string CodeReadFailed = "well-compare-read-failed";
    public const string CodeNeedsTwo = "well-compare-needs-two";
    public const string CodeTooMany = "well-compare-too-many";
    public const string CodeUnknownWell = "well-compare-unknown-well";

    public const int MaximumWells = 6;

    private readonly Func<IDataSource?> _source;
    private readonly IFailureReporter _failures;

    public OgWellComparison(Func<IDataSource?> source, IFailureReporter failures)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _failures = failures ?? throw new ArgumentNullException(nameof(failures));
    }

    public async Task<OgResult<IReadOnlyList<OgCompareWell>>> ListWellsAsync(
        int take = 250,
        CancellationToken cancellationToken = default)
    {
        var source = _source();

        if (source is null)
        {
            return OgResult<IReadOnlyList<OgCompareWell>>.Failed(
                "No connection is open, so wells cannot be listed.", CodeNoConnection);
        }

        try
        {
            var wells = await OgUow.ReadAllAsync<WELL>(source).ConfigureAwait(false);

            var listed = wells
                .Where(well => !string.IsNullOrWhiteSpace(well.UWI))
                .OrderBy(well => well.WELL_NAME, StringComparer.OrdinalIgnoreCase)
                .ThenBy(well => well.UWI, StringComparer.OrdinalIgnoreCase)
                .Take(Math.Clamp(take, 1, 1000))
                .Select(well => new OgCompareWell(well.UWI!, Blank(well.WELL_NAME)))
                .ToList();

            return OgResult<IReadOnlyList<OgCompareWell>>.Ok(listed);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                "listing wells for comparison",
                "no wells are offered",
                FailureSeverity.Error);

            return OgResult<IReadOnlyList<OgCompareWell>>.Failed(
                "The wells could not be listed.", reference ?? CodeReadFailed);
        }
    }

    public async Task<OgResult<OgComparisonMatrix>> CompareAsync(
        IReadOnlyList<string> uwis,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(uwis);

        var selected = uwis
            .Where(uwi => !string.IsNullOrWhiteSpace(uwi))
            .Select(uwi => uwi.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (selected.Count < 2)
        {
            return OgResult<OgComparisonMatrix>.Failed(
                "Select at least two wells to compare.", CodeNeedsTwo);
        }

        if (selected.Count > MaximumWells)
        {
            return OgResult<OgComparisonMatrix>.Failed(
                $"Compare at most {MaximumWells} wells at once; remove {selected.Count - MaximumWells}.",
                CodeTooMany);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgComparisonMatrix>.Failed(
                "No connection is open, so the wells cannot be compared.", CodeNoConnection);
        }

        try
        {
            var wells = await OgUow.ReadAllAsync<WELL>(source).ConfigureAwait(false);
            var picks = await OgUow.ReadAllAsync<STRAT_WELL_SECTION>(source).ConfigureAwait(false);

            var byUwi = wells
                .Where(well => !string.IsNullOrWhiteSpace(well.UWI))
                .GroupBy(well => well.UWI!, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);

            var pickCounts = picks
                .Where(pick => string.Equals(pick.ACTIVE_IND, "Y", StringComparison.OrdinalIgnoreCase)
                               || string.IsNullOrWhiteSpace(pick.ACTIVE_IND))
                .Where(pick => !string.IsNullOrWhiteSpace(pick.UWI))
                .GroupBy(pick => pick.UWI!, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(group => group.Key, group => group.Count(), StringComparer.OrdinalIgnoreCase);

            var facts = new List<OgWellFacts>(selected.Count);

            foreach (var uwi in selected)
            {
                if (!byUwi.TryGetValue(uwi, out var well))
                {
                    return OgResult<OgComparisonMatrix>.Failed(
                        $"Well '{uwi}' is not in this database.", CodeUnknownWell);
                }

                facts.Add(ReadFacts(
                    well,
                    pickCounts.TryGetValue(uwi, out var count) ? count : 0));
            }

            var cells = facts
                .Select(fact => new OgCompareWell(fact.Uwi, fact.Name))
                .ToList();

            var rows = BuildRows(facts);
            var overlay = OverlayDecision(facts);

            return OgResult<OgComparisonMatrix>.Ok(
                new OgComparisonMatrix(cells, rows, overlay.CanOverlay, overlay.Reason));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                "comparing wells",
                "the comparison is not shown",
                FailureSeverity.Error);

            return OgResult<OgComparisonMatrix>.Failed(
                "The wells could not be compared.", reference ?? CodeReadFailed);
        }
    }

    /// <summary>The matrix rows for the facts, in a stable category/property order.</summary>
    public static IReadOnlyList<OgComparisonRow> BuildRows(IReadOnlyList<OgWellFacts> facts)
    {
        ArgumentNullException.ThrowIfNull(facts);

        return
        [
            Text(facts, "Identity", "name", "Name", fact => fact.Name),
            Text(facts, "Identity", "field", "Field", fact => fact.Field),
            Text(facts, "Identity", "operator", "Operator", fact => fact.Operator),
            Text(facts, "Identity", "government-id", "Government id", fact => fact.GovernmentId),
            Text(facts, "Status", "status", "Status", fact => fact.Status),
            Text(facts, "Status", "class", "Class", fact => fact.WellClass),
            Number(facts, "Geometry", "final-td", "Total depth", fact => fact.TotalDepth, fact => fact.TotalDepthUnit),
            Number(facts, "Geometry", "kb", "Kelly bushing", fact => fact.KellyBushing, fact => fact.ElevationUnit),
            Number(facts, "Geometry", "ground", "Ground elevation", fact => fact.GroundElevation, fact => fact.ElevationUnit),
            Number(facts, "Geometry", "latitude", "Surface latitude", fact => fact.Latitude, _ => "deg"),
            Number(facts, "Geometry", "longitude", "Surface longitude", fact => fact.Longitude, _ => "deg"),
            Date(facts, "Completion", "spud", "Spud date", fact => fact.SpudDate),
            Date(facts, "Completion", "completed", "Completion date", fact => fact.CompletionDate),
            Date(facts, "Completion", "abandoned", "Abandonment date", fact => fact.AbandonmentDate),
            Integer(facts, "Activity", "picks", "Stratigraphic picks", fact => fact.PickCount, "count"),
        ];
    }

    /// <summary>AC-4: the overlay needs a depth with its unit and a surface coordinate on every well.</summary>
    public static (bool CanOverlay, string? Reason) OverlayDecision(IReadOnlyList<OgWellFacts> facts)
    {
        ArgumentNullException.ThrowIfNull(facts);

        var missing = new List<string>();

        foreach (var fact in facts)
        {
            var parts = new List<string>();

            if (fact.TotalDepth is null || string.IsNullOrWhiteSpace(fact.TotalDepthUnit))
            {
                parts.Add("total depth with its unit");
            }

            if (fact.Latitude is null || fact.Longitude is null)
            {
                parts.Add("surface coordinates");
            }

            if (parts.Count > 0)
            {
                missing.Add($"{fact.Uwi} ({string.Join(" and ", parts)})");
            }
        }

        return missing.Count == 0
            ? (true, null)
            : (false, $"Overlay unavailable: {string.Join("; ", missing)}.");
    }

    private static OgComparisonRow Text(
        IReadOnlyList<OgWellFacts> facts,
        string category,
        string key,
        string label,
        Func<OgWellFacts, string?> read)
    {
        var cells = facts
            .Select(fact =>
            {
                var text = read(fact);

                return string.IsNullOrWhiteSpace(text)
                    ? new OgCompareCell("-", null, true, false)
                    : new OgCompareCell(text, null, false, false);
            })
            .ToList();

        return new OgComparisonRow(key, label, category, string.Empty, cells, [], false);
    }

    private static OgComparisonRow Number(
        IReadOnlyList<OgWellFacts> facts,
        string category,
        string key,
        string label,
        Func<OgWellFacts, decimal?> read,
        Func<OgWellFacts, string?> unit)
    {
        var units = facts.Select(unit).Select(value => value?.Trim() ?? string.Empty).ToList();
        var distinctUnits = units
            .Where(value => value.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        // AC-1: a unit that differs across the wells (or is missing on one and present on another) stops the delta.
        var unitMismatch = distinctUnits.Count > 1
                           || units.Any(value => value.Length == 0) && units.Any(value => value.Length > 0);

        var rowUnit = distinctUnits.Count == 1 ? distinctUnits[0] : string.Empty;

        var values = facts
            .Select(fact => read(fact))
            .ToList();

        var cells = values
            .Select((value, index) =>
            {
                if (value is null)
                {
                    return new OgCompareCell("-", null, true, unitMismatch);
                }

                // When the units differ, each cell shows its own unit, so the mismatch is visible rather than hidden
                // behind a shared unitless number.
                var cellUnit = unitMismatch ? units[index] : rowUnit;

                return new OgCompareCell(
                    $"{value.Value.ToString("0.###", CultureInfo.InvariantCulture)}" +
                    (cellUnit.Length > 0 ? $" {cellUnit}" : string.Empty),
                    value,
                    false,
                    unitMismatch);
            })
            .ToList();

        var deltas = new List<string>();

        if (!unitMismatch && values.Count > 0 && values[0] is { } baseline && values.All(value => value is not null))
        {
            for (var index = 1; index < values.Count; index++)
            {
                var delta = values[index]!.Value - baseline;
                deltas.Add(
                    $"{facts[index].Uwi}: {(delta > 0 ? "+" : string.Empty)}" +
                    $"{delta.ToString("0.###", CultureInfo.InvariantCulture)} {rowUnit}".TrimEnd());
            }
        }

        return new OgComparisonRow(key, label, category, rowUnit, cells, deltas, unitMismatch);
    }

    private static OgComparisonRow Date(
        IReadOnlyList<OgWellFacts> facts,
        string category,
        string key,
        string label,
        Func<OgWellFacts, DateTime?> read)
    {
        var cells = facts
            .Select(fact => read(fact) is { } date
                ? new OgCompareCell(date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), null, false, false)
                : new OgCompareCell("-", null, true, false))
            .ToList();

        return new OgComparisonRow(key, label, category, string.Empty, cells, [], false);
    }

    private static OgComparisonRow Integer(
        IReadOnlyList<OgWellFacts> facts,
        string category,
        string key,
        string label,
        Func<OgWellFacts, int> read,
        string unit)
    {
        var values = facts.Select(read).ToList();
        var baseline = values[0];

        var cells = values
            .Select(value => new OgCompareCell(
                value.ToString(CultureInfo.InvariantCulture), value, false, false))
            .ToList();

        var deltas = values
            .Skip(1)
            .Select((value, index) =>
                $"{facts[index + 1].Uwi}: {(value - baseline > 0 ? "+" : string.Empty)}{value - baseline} {unit}")
            .ToList();

        return new OgComparisonRow(key, label, category, unit, cells, deltas, false);
    }

    private static OgWellFacts ReadFacts(WELL well, int pickCount) =>
        new(
            Uwi: well.UWI ?? string.Empty,
            Name: Blank(well.WELL_NAME),
            Field: Blank(well.ASSIGNED_FIELD),
            Operator: Blank(well.OPERATOR),
            GovernmentId: Blank(well.WELL_GOVERNMENT_ID),
            Status: Blank(well.CURRENT_STATUS),
            WellClass: Blank(well.CURRENT_CLASS),
            TotalDepth: well.FINAL_TD == 0 ? null : well.FINAL_TD,
            TotalDepthUnit: Blank(well.FINAL_TD_OUOM),
            KellyBushing: well.KB_ELEV,
            GroundElevation: well.GROUND_ELEV,
            ElevationUnit: Blank(well.GROUND_ELEV_OUOM),
            Latitude: well.SURFACE_LATITUDE == 0 ? null : well.SURFACE_LATITUDE,
            Longitude: well.SURFACE_LONGITUDE == 0 ? null : well.SURFACE_LONGITUDE,
            SpudDate: well.SPUD_DATE,
            CompletionDate: well.COMPLETION_DATE,
            AbandonmentDate: well.ABANDONMENT_DATE)
        {
            PickCount = pickCount,
        };

    private static string? Blank(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value;
}

/// <summary>The values a comparison reads from one well.</summary>
public sealed record OgWellFacts(
    string Uwi,
    string? Name,
    string? Field,
    string? Operator,
    string? GovernmentId,
    string? Status,
    string? WellClass,
    decimal? TotalDepth,
    string? TotalDepthUnit,
    decimal? KellyBushing,
    decimal? GroundElevation,
    string? ElevationUnit,
    decimal? Latitude,
    decimal? Longitude,
    DateTime? SpudDate,
    DateTime? CompletionDate,
    DateTime? AbandonmentDate)
{
    public int PickCount { get; init; }
}
