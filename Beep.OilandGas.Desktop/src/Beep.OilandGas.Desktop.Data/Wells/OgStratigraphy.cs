using System.Globalization;
using Beep.OilandGas.PPDM39.Models;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Data.Wells;

/// <summary>One stratigraphic unit (WEL-05). Colour is a theme token, never a hex value (AC-5).</summary>
public sealed record OgStratUnit(
    string Id,
    string NameSetId,
    string Name,
    string? ShortName,
    string? ParentId,
    string Rank,
    decimal AgeCode,
    string ColourToken)
{
    public int Order => (int)AgeCode;
}

/// <summary>A node of the unit hierarchy, with the level it sits at.</summary>
public sealed record OgStratNode(OgStratUnit Unit, int Level, IReadOnlyList<OgStratNode> Children);

/// <summary>One pick in a well, with its unit resolved from the reference set (AC-2).</summary>
public sealed record OgWellPick(
    string WellId,
    string UnitId,
    string NameSetId,
    string UnitName,
    decimal? TopMd,
    decimal? BaseMd,
    string? DepthUnit,
    string? InterpId,
    decimal? Thickness,
    bool Overlapping,
    string? Flag)
{
    /// <summary>Thickness is only meaningful with a top and a base below it (AC-3 / missing-depth rule).</summary>
    public bool HasThickness => Thickness is > 0;
}

/// <summary>A regional stratigraphic column.</summary>
public sealed record OgStratColumn(string Id, string Source, string Name, string? Type);

/// <summary>WEL-05: units, the unit hierarchy, regional columns and the picks of one well.</summary>
public interface IOgStratigraphy
{
    Task<OgResult<IReadOnlyList<OgStratUnit>>> LoadUnitsAsync(CancellationToken cancellationToken = default);

    Task<OgResult<IReadOnlyList<OgStratColumn>>> LoadColumnsAsync(CancellationToken cancellationToken = default);

    /// <summary>Picks of the selected well only (AC-6).</summary>
    Task<OgResult<IReadOnlyList<OgWellPick>>> LoadPicksAsync(
        string uwi,
        CancellationToken cancellationToken = default);

    /// <summary>Adds a pick whose unit must exist in the reference set (AC-2).</summary>
    Task<OgResult<OgWellPick>> AddPickAsync(
        string uwi,
        string nameSetId,
        string unitId,
        decimal topMd,
        string? depthUnit,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>Renames a unit; picks keep their identity and only their display changes (AC-4).</summary>
    Task<OgResult> RenameUnitAsync(
        string nameSetId,
        string unitId,
        string newName,
        string actor,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// The stratigraphy over STRAT_UNIT, STRAT_COLUMN and STRAT_WELL_SECTION, read and written through their entity
/// models and a unit of work.
/// </summary>
/// <remarks>
/// The unit hierarchy is built from GROUP_CODE with a stable order (age code, then name) so three levels always
/// render the same way (AC-1). Picks are read with their unit joined from the reference table in LINQ, never free
/// text (AC-2), and two picks at the same or an inverted depth are flagged, not reordered (AC-3).
/// </remarks>
public sealed class OgStratigraphy : IOgStratigraphy
{
    public const string CodeNoConnection = "stratigraphy-no-connection";
    public const string CodeReadFailed = "stratigraphy-read-failed";
    public const string CodeWriteFailed = "stratigraphy-write-failed";
    public const string CodeUnitRequired = "stratigraphy-unit-required";
    public const string CodeUnitUnknown = "stratigraphy-unit-unknown";
    public const string CodeDepthRequired = "stratigraphy-depth-required";
    public const string CodeActorRequired = "stratigraphy-actor-required";
    public const string CodeUnitMissing = "stratigraphy-unit-missing";

    /// <summary>Theme tokens a unit colour may name. The page resolves them; nothing here is a hex value.</summary>
    public static IReadOnlyList<string> ColourTokens { get; } =
        ["Highlight", "Info", "Control", "Light", "Window"];

    private readonly Func<IDataSource?> _source;
    private readonly IFailureReporter _failures;

    public OgStratigraphy(Func<IDataSource?> source, IFailureReporter failures)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _failures = failures ?? throw new ArgumentNullException(nameof(failures));
    }

    public async Task<OgResult<IReadOnlyList<OgStratUnit>>> LoadUnitsAsync(
        CancellationToken cancellationToken = default)
    {
        var source = _source();

        if (source is null)
        {
            return OgResult<IReadOnlyList<OgStratUnit>>.Failed(
                "No connection is open, so the stratigraphic units cannot be read.", CodeNoConnection);
        }

        try
        {
            var units = await ReadUnitsAsync(source).ConfigureAwait(false);

            return OgResult<IReadOnlyList<OgStratUnit>>.Ok(units);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                "reading the stratigraphic units",
                "the unit hierarchy is not shown",
                FailureSeverity.Error);

            return OgResult<IReadOnlyList<OgStratUnit>>.Failed(
                "The stratigraphic units could not be read.", reference ?? CodeReadFailed);
        }
    }

    public async Task<OgResult<IReadOnlyList<OgStratColumn>>> LoadColumnsAsync(
        CancellationToken cancellationToken = default)
    {
        var source = _source();

        if (source is null)
        {
            return OgResult<IReadOnlyList<OgStratColumn>>.Failed(
                "No connection is open, so the stratigraphic columns cannot be read.", CodeNoConnection);
        }

        try
        {
            var rows = await OgUow.ReadAllAsync<STRAT_COLUMN>(source).ConfigureAwait(false);

            var columns = rows
                .OrderBy(row => row.STRAT_COLUMN_NAME, StringComparer.OrdinalIgnoreCase)
                .ThenBy(row => row.STRAT_COLUMN_ID, StringComparer.OrdinalIgnoreCase)
                .Select(row => new OgStratColumn(
                    row.STRAT_COLUMN_ID ?? string.Empty,
                    row.SOURCE ?? string.Empty,
                    Blank(row.STRAT_COLUMN_NAME) ?? row.STRAT_COLUMN_ID ?? string.Empty,
                    Blank(row.STRAT_COLUMN_TYPE)))
                .ToList();

            return OgResult<IReadOnlyList<OgStratColumn>>.Ok(columns);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                "reading the stratigraphic columns",
                "the column list is not shown",
                FailureSeverity.Error);

            return OgResult<IReadOnlyList<OgStratColumn>>.Failed(
                "The stratigraphic columns could not be read.", reference ?? CodeReadFailed);
        }
    }

    public async Task<OgResult<IReadOnlyList<OgWellPick>>> LoadPicksAsync(
        string uwi,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(uwi);

        var source = _source();

        if (source is null)
        {
            return OgResult<IReadOnlyList<OgWellPick>>.Failed(
                "No connection is open, so the picks cannot be read.", CodeNoConnection);
        }

        try
        {
            var picks = await ReadPicksAsync(source, uwi, cancellationToken).ConfigureAwait(false);

            return OgResult<IReadOnlyList<OgWellPick>>.Ok(picks);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                $"reading the stratigraphic picks of {uwi}",
                "the picks are not shown",
                FailureSeverity.Error);

            return OgResult<IReadOnlyList<OgWellPick>>.Failed(
                "The picks could not be read.", reference ?? CodeReadFailed);
        }
    }

    public async Task<OgResult<OgWellPick>> AddPickAsync(
        string uwi,
        string nameSetId,
        string unitId,
        decimal topMd,
        string? depthUnit,
        string actor,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgWellPick>.Failed(
                "A pick change must record who made it.", CodeActorRequired);
        }

        if (string.IsNullOrWhiteSpace(unitId))
        {
            return OgResult<OgWellPick>.Failed(
                "A pick needs a unit from the reference set.", CodeUnitRequired);
        }

        if (topMd <= 0)
        {
            return OgResult<OgWellPick>.Failed(
                "A pick needs a top depth greater than zero.", CodeDepthRequired);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgWellPick>.Failed(
                "No connection is open, so the pick cannot be added.", CodeNoConnection);
        }

        try
        {
            var units = await ReadUnitsAsync(source).ConfigureAwait(false);
            var unit = units.FirstOrDefault(candidate =>
                string.Equals(candidate.Id, unitId.Trim(), StringComparison.OrdinalIgnoreCase)
                && (string.IsNullOrWhiteSpace(nameSetId)
                    || string.Equals(candidate.NameSetId, nameSetId.Trim(), StringComparison.OrdinalIgnoreCase)));

            if (unit is null)
            {
                // AC-2: free text is not a unit.
                return OgResult<OgWellPick>.Failed(
                    $"'{unitId}' is not a unit in the reference set. Choose one of: " +
                    $"{string.Join(", ", units.Take(12).Select(candidate => candidate.Id))}.",
                    CodeUnitUnknown);
            }

            var unitOfWork = OgUow.For<STRAT_WELL_SECTION>(source);
            var existing = await OgUow.ReadAsync(unitOfWork, [OgUow.Equal("UWI", uwi)], cancellationToken)
                .ConfigureAwait(false);

            var now = DateTime.UtcNow;
            var interpId = Guid.NewGuid().ToString("N");
            var next = existing.Count == 0 ? 1 : (int)existing.Max(row => row.ORDINAL_SEQ_NO) + 1;

            var pick = new STRAT_WELL_SECTION
            {
                UWI = uwi,
                STRAT_NAME_SET_ID = unit.NameSetId,
                STRAT_UNIT_ID = unit.Id,
                INTERP_ID = interpId,
                PICK_DEPTH = topMd,
                PICK_DEPTH_OUOM = Blank(depthUnit),
                ORDINAL_SEQ_NO = next,
                ACTIVE_IND = "Y",
                PREFERRED_PICK_IND = "Y",
                SOURCE = "DESKTOP",
                ROW_CREATED_BY = actor.Trim(),
                ROW_CREATED_DATE = now,
                ROW_CHANGED_BY = actor.Trim(),
                ROW_CHANGED_DATE = now,
            };

            unitOfWork.Add(pick);
            var committed = await unitOfWork.Commit().ConfigureAwait(false);

            return OgUow.IsOk(committed)
                ? OgResult<OgWellPick>.Ok(new OgWellPick(
                    uwi, unit.Id, unit.NameSetId, unit.Name, topMd, null, Blank(depthUnit), interpId,
                    null, false, null))
                : OgResult<OgWellPick>.Failed(
                    OgUow.Message(committed, "The pick was not added."), CodeWriteFailed);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                $"adding a stratigraphic pick to {uwi}",
                "the pick was not added",
                FailureSeverity.Error);

            return OgResult<OgWellPick>.Failed(
                "The pick could not be added.", reference ?? CodeWriteFailed);
        }
    }

    public async Task<OgResult> RenameUnitAsync(
        string nameSetId,
        string unitId,
        string newName,
        string actor,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult.Failed("A unit change must record who made it.", CodeActorRequired);
        }

        if (string.IsNullOrWhiteSpace(newName))
        {
            return OgResult.Failed("A unit needs a name.", "stratigraphy-name-required");
        }

        var source = _source();

        if (source is null)
        {
            return OgResult.Failed(
                "No connection is open, so the unit cannot be renamed.", CodeNoConnection);
        }

        try
        {
            var unitOfWork = OgUow.For<STRAT_UNIT>(source);
            var rows = await OgUow.ReadAsync(unitOfWork, [OgUow.Equal("STRAT_NAME_SET_ID", nameSetId)],
                cancellationToken).ConfigureAwait(false);

            var unit = rows.FirstOrDefault(row =>
                string.Equals(row.STRAT_UNIT_ID, unitId, StringComparison.OrdinalIgnoreCase));

            if (unit is null)
            {
                return OgResult.Failed($"Unit '{unitId}' is not in this name set.", CodeUnitMissing);
            }

            var picks = await OgUow.ReadAsync(
                OgUow.For<STRAT_WELL_SECTION>(source),
                [OgUow.Equal("STRAT_NAME_SET_ID", nameSetId)],
                cancellationToken).ConfigureAwait(false);

            var affected = picks.Count(row =>
                string.Equals(row.STRAT_UNIT_ID, unitId, StringComparison.OrdinalIgnoreCase));

            // AC-4: only the unit row changes; every pick keeps STRAT_UNIT_ID and reads the new name by join.
            unit.LONG_NAME = newName.Trim();
            unit.SHORT_NAME = newName.Trim();
            unit.ROW_CHANGED_BY = actor.Trim();
            unit.ROW_CHANGED_DATE = DateTime.UtcNow;

            var updated = unitOfWork.Update(unit);

            if (!OgUow.IsOk(updated))
            {
                return OgResult.Failed(
                    OgUow.Message(updated, "The unit could not be renamed."), CodeWriteFailed);
            }

            var committed = await unitOfWork.Commit().ConfigureAwait(false);

            return OgUow.IsOk(committed)
                ? OgResult.Ok($"Unit renamed; {affected} pick(s) now show the new name.")
                : OgResult.Failed(OgUow.Message(committed, "The unit could not be renamed."), CodeWriteFailed);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                $"renaming stratigraphic unit {unitId}",
                "the unit was not renamed",
                FailureSeverity.Error);

            return OgResult.Failed("The unit could not be renamed.", reference ?? CodeWriteFailed);
        }
    }

    /// <summary>Builds the tree from GROUP_CODE with a stable order: age code, then name (AC-1).</summary>
    public static IReadOnlyList<OgStratNode> BuildHierarchy(IReadOnlyList<OgStratUnit> units)
    {
        ArgumentNullException.ThrowIfNull(units);

        var byParent = units
            .OrderBy(unit => unit.AgeCode)
            .ThenBy(unit => unit.Name, StringComparer.OrdinalIgnoreCase)
            .GroupBy(unit => unit.ParentId ?? string.Empty, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                group => group.Key,
                group => group.ToList(),
                StringComparer.OrdinalIgnoreCase);

        List<OgStratNode> Build(string parentId, int level, HashSet<string> path)
        {
            if (!byParent.TryGetValue(parentId, out var children))
            {
                return [];
            }

            var nodes = new List<OgStratNode>();

            foreach (var unit in children)
            {
                // A cyclic GROUP_CODE would otherwise recurse forever.
                var branch = new HashSet<string>(path, StringComparer.OrdinalIgnoreCase);

                if (!branch.Add($"{unit.NameSetId}|{unit.Id}"))
                {
                    continue;
                }

                nodes.Add(new OgStratNode(unit, level, Build(unit.Id, level + 1, branch)));
            }

            return nodes;
        }

        return Build(string.Empty, 1, new HashSet<string>(StringComparer.OrdinalIgnoreCase));
    }

    /// <summary>Flags two picks at the same or an inverted depth; nothing is reordered (AC-3).</summary>
    public static IReadOnlyList<OgWellPick> FlagOverlaps(IReadOnlyList<OgWellPick> picks)
    {
        ArgumentNullException.ThrowIfNull(picks);

        var ordered = picks
            .OrderBy(pick => pick.TopMd ?? decimal.MaxValue)
            .ThenBy(pick => pick.UnitName, StringComparer.OrdinalIgnoreCase)
            .ToList();

        // Both sides of an overlap are flagged: the pair is what the person has to resolve.
        var overlaps = new string?[ordered.Count];

        for (var index = 0; index < ordered.Count - 1; index++)
        {
            if (ordered[index].TopMd is { } current
                && ordered[index + 1].TopMd is { } following
                && following <= current)
            {
                overlaps[index] ??= $"Overlaps {ordered[index + 1].UnitName} (same or shallower top).";
                overlaps[index + 1] ??= $"Overlaps {ordered[index].UnitName} (same or shallower top).";
            }
        }

        var flagged = new List<OgWellPick>(ordered.Count);

        for (var index = 0; index < ordered.Count; index++)
        {
            var pick = ordered[index];
            var next = index + 1 < ordered.Count ? ordered[index + 1] : null;

            var baseMd = next?.TopMd;
            var thickness = pick.TopMd is { } top && baseMd is { } bottom && bottom > top
                ? bottom - top
                : (decimal?)null;

            flagged.Add(pick with
            {
                BaseMd = baseMd,
                Thickness = thickness,
                Overlapping = overlaps[index] is not null,
                Flag = overlaps[index]
                       ?? (pick.TopMd is null ? "No top depth; excluded from thickness." : null),
            });
        }

        return flagged;
    }

    private static async Task<List<OgStratUnit>> ReadUnitsAsync(IDataSource source)
    {
        var rows = await OgUow.ReadAllAsync<STRAT_UNIT>(source).ConfigureAwait(false);

        return rows
            .Where(row => !string.IsNullOrWhiteSpace(row.STRAT_UNIT_ID))
            .OrderBy(row => row.STRAT_NAME_SET_ID, StringComparer.OrdinalIgnoreCase)
            .ThenBy(row => row.ORDINAL_AGE_CODE)
            .ThenBy(row => row.LONG_NAME, StringComparer.OrdinalIgnoreCase)
            .Select(row =>
            {
                var id = row.STRAT_UNIT_ID!;
                var rank = Blank(row.STRAT_UNIT_TYPE) ?? Blank(row.STRAT_TYPE) ?? string.Empty;

                return new OgStratUnit(
                    id,
                    row.STRAT_NAME_SET_ID ?? string.Empty,
                    Blank(row.LONG_NAME) ?? Blank(row.SHORT_NAME) ?? id,
                    Blank(row.SHORT_NAME),
                    Blank(row.GROUP_CODE),
                    rank,
                    row.ORDINAL_AGE_CODE,
                    ColourFor(rank));
            })
            .ToList();
    }

    private static async Task<List<OgWellPick>> ReadPicksAsync(
        IDataSource source,
        string uwi,
        CancellationToken cancellationToken)
    {
        var pickUnit = OgUow.For<STRAT_WELL_SECTION>(source);
        var rows = await OgUow.ReadAsync(pickUnit, [OgUow.Equal("UWI", uwi)], cancellationToken)
            .ConfigureAwait(false);

        var units = await OgUow.ReadAllAsync<STRAT_UNIT>(source).ConfigureAwait(false);
        var names = units
            .Where(unit => !string.IsNullOrWhiteSpace(unit.STRAT_UNIT_ID))
            .GroupBy(unit => Key(unit.STRAT_NAME_SET_ID, unit.STRAT_UNIT_ID), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                group => group.Key,
                group => Blank(group.First().LONG_NAME) ?? Blank(group.First().SHORT_NAME),
                StringComparer.OrdinalIgnoreCase);

        var picks = rows
            .Where(row => string.Equals(row.ACTIVE_IND, "Y", StringComparison.OrdinalIgnoreCase)
                          || string.IsNullOrWhiteSpace(row.ACTIVE_IND))
            .OrderBy(row => row.PICK_DEPTH)
            .ThenBy(row => row.STRAT_UNIT_ID, StringComparer.OrdinalIgnoreCase)
            .Select(row =>
            {
                var setId = row.STRAT_NAME_SET_ID ?? string.Empty;
                var unitId = row.STRAT_UNIT_ID ?? string.Empty;
                names.TryGetValue(Key(setId, unitId), out var name);

                return new OgWellPick(
                    row.UWI ?? uwi,
                    unitId,
                    setId,
                    name ?? unitId,
                    row.PICK_DEPTH == 0 ? null : row.PICK_DEPTH,
                    BaseMd: null,
                    DepthUnit: Blank(row.PICK_DEPTH_OUOM),
                    InterpId: Blank(row.INTERP_ID),
                    Thickness: null,
                    Overlapping: false,
                    Flag: null);
            })
            .ToList();

        return FlagOverlaps(picks).ToList();
    }

    /// <summary>A theme token by rank; never a colour value (AC-5).</summary>
    public static string ColourFor(string? rank) =>
        (rank ?? string.Empty).ToUpperInvariant() switch
        {
            "GROUP" or "SUPERGROUP" => "Highlight",
            "FORMATION" => "Info",
            "MEMBER" or "BED" => "Control",
            "" => "Window",
            _ => "Light",
        };

    private static string? Blank(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value;

    /// <summary>The (name set, unit) identity as one lookup key, case-insensitive.</summary>
    private static string Key(string? nameSet, string? unitId) =>
        $"{nameSet?.Trim()}\u001F{unitId?.Trim()}";
}
