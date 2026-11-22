using System.Globalization;
using Beep.OilandGas.PPDM39.Models;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Data.Production;

/// <summary>One calendar day of measured production for the field (PRD-01).</summary>
public sealed record OgProductionDay(
    DateTime Date,
    decimal? Oil,
    decimal? Gas,
    decimal? Water,
    string? OilUnit,
    string? GasUnit,
    string? WaterUnit,
    int WellCount,
    int RowCount)
{
    /// <summary>AC-2: a day with no measured row is "not yet reported", which is not zero.</summary>
    public bool Reported => RowCount > 0;

    public string StateText => Reported ? "reported" : "not yet reported";

    public string OilText => Volume(oil: true);
    public string GasText => Volume(oil: false, gas: true);
    public string WaterText => Volume(oil: false, gas: false);

    private string Volume(bool oil, bool gas = false)
    {
        var value = oil ? Oil : gas ? Gas : Water;
        var unit = oil ? OilUnit : gas ? GasUnit : WaterUnit;

        return value is null && !Reported
            ? "not yet reported"
            : Format(value, unit);
    }

    /// <summary>A volume with its unit, or a statement that it was not recorded (AC-3: never a bare number).</summary>
    public static string Format(decimal? value, string? unit) =>
        value is { } amount
            ? $"{amount.ToString("#,0.###", CultureInfo.InvariantCulture)} " +
              (string.IsNullOrWhiteSpace(unit) ? "(unit not recorded)" : unit!.Trim())
            : "not recorded";

    public string Describe() =>
        $"{Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)} · oil {OilText} · gas {GasText} · " +
        $"water {WaterText} · {WellCount.ToString(CultureInfo.InvariantCulture)} well(s) · {StateText}";
}

/// <summary>One well's measured volume on one day (PRD-01).</summary>
public sealed record OgWellDay(
    string WellId,
    string? Name,
    DateTime Date,
    decimal? Oil,
    decimal? Gas,
    decimal? Water,
    string? OilUnit,
    string? GasUnit,
    string? WaterUnit)
{
    public string OilText => OgProductionDay.Format(Oil, OilUnit);

    public string NameText => Name ?? WellId;
}

/// <summary>One well's change against its own average (PRD-01 "top movers").</summary>
public sealed record OgMover(
    string WellId,
    string? Name,
    decimal Today,
    decimal Average,
    decimal Change,
    string? Unit,
    int DaysOfHistory)
{
    public bool Rose => Change > 0m;

    public string TodayText => OgProductionDay.Format(Today, Unit);

    public string AverageText => OgProductionDay.Format(Average, Unit);

    public string ChangeText =>
        (Rose ? "+" : string.Empty) + OgProductionDay.Format(Change, Unit);

    public string Describe() =>
        $"{(Name ?? WellId)} · today {TodayText} · average {AverageText} · " +
        $"{ChangeText} over {DaysOfHistory.ToString(CultureInfo.InvariantCulture)} day(s)";
}

/// <summary>A well the field expected to report and did not (PRD-01 AC-4).</summary>
public sealed record OgProductionAlert(string WellId, string? Name, string Message)
{
    public string Describe() => $"{(Name ?? WellId)}: {Message}";
}

/// <summary>The rules the operations centre states once, so the page and the tests agree.</summary>
public static class OperationsCentreRules
{
    /// <summary>The history a "top mover" needs before its change is shown (AC-5).</summary>
    public const int RequiredHistoryDays = 7;

    /// <summary>The window the volume strip and the movers read.</summary>
    public const int WindowDays = 30;

    /// <summary>
    /// A volume row is production unless it says it is injection. PPDM writes the production volumes in
    /// OIL_VOLUME/GAS_VOLUME/WATER_VOLUME and names the activity in ACTIVITY_TYPE; an empty value is treated as
    /// production because those columns are the production volumes.
    /// </summary>
    public static bool IsProductionRow(string? activityType) =>
        !string.Equals((activityType ?? string.Empty).Trim(), "INJECTION", StringComparison.OrdinalIgnoreCase);

    /// <summary>AC-3: a water cut is computed only when oil and water are stated in the same unit.</summary>
    public static decimal? WaterCut(decimal? oil, decimal? water, string? oilUnit, string? waterUnit)
    {
        if (oil is not { } oilAmount || water is not { } waterAmount)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(oilUnit)
            || string.IsNullOrWhiteSpace(waterUnit)
            || !string.Equals(oilUnit.Trim(), waterUnit.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var total = oilAmount + waterAmount;

        return total <= 0m ? null : waterAmount / total;
    }

    public static string WaterCutText(decimal? cut, string? oilUnit, string? waterUnit) =>
        cut is { } value
            ? (value * 100m).ToString("0.##", CultureInfo.InvariantCulture) + "%"
            : string.IsNullOrWhiteSpace(oilUnit) || string.IsNullOrWhiteSpace(waterUnit)
                ? "not computable: a volume has no unit"
                : $"not computable: oil is in {oilUnit.Trim()} and water is in {waterUnit.Trim()}";

    /// <summary>The dominant unit of a set of rows: the unit most rows are stated in.</summary>
    public static string? DominantUnit(IEnumerable<string?> units) =>
        units
            .Where(unit => !string.IsNullOrWhiteSpace(unit))
            .Select(unit => unit!.Trim())
            .GroupBy(unit => unit, StringComparer.OrdinalIgnoreCase)
            .OrderByDescending(group => group.Count())
            .ThenBy(group => group.Key, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .FirstOrDefault();
}

/// <summary>The operations centre for one field and day (PRD-01).</summary>
public sealed record OgOperationsCentre(
    string FieldName,
    string? FieldId,
    DateTime Day,
    OgProductionDay Today,
    IReadOnlyList<OgProductionDay> Days,
    IReadOnlyList<OgWellDay> Wells,
    IReadOnlyList<OgMover> Gainers,
    IReadOnlyList<OgMover> Decliners,
    bool TopMoversAvailable,
    string TopMoversReason,
    IReadOnlyList<OgProductionAlert> Alerts,
    int ActiveWells,
    int TotalWells,
    IReadOnlyList<string> NotComparable,
    int InjectionRowsExcluded,
    int UndatedRowsExcluded,
    int SupersededRowsExcluded)
{
    public bool IsReported => Today.Reported;

    public int MissingWellCount => Alerts.Count;

    public bool HasData => Days.Any(day => day.Reported);

    public string OilText => Today.OilText;
    public string GasText => Today.GasText;
    public string WaterText => Today.WaterText;

    public decimal? WaterCut => OperationsCentreRules.WaterCut(
        Today.Oil, Today.Water, Today.OilUnit, Today.WaterUnit);

    public string WaterCutText => OperationsCentreRules.WaterCutText(
        WaterCut, Today.OilUnit, Today.WaterUnit);

    public string WellsText =>
        $"{ActiveWells.ToString(CultureInfo.InvariantCulture)} active of " +
        $"{TotalWells.ToString(CultureInfo.InvariantCulture)} well(s)";

    /// <summary>AC-2: the header says which of "reported", "not yet reported" or "no data at all" applies.</summary>
    public string DayStateText => Today.Reported
        ? $"reported for {Day.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}"
        : HasData
            ? $"not yet reported for {Day.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}"
            : "no production is recorded for this field in the last " +
              $"{OperationsCentreRules.WindowDays.ToString(CultureInfo.InvariantCulture)} day(s)";

    /// <summary>AC-4: how complete the day is, counted from the field's own wells.</summary>
    public string CompletenessText => Today.Reported
        ? MissingWellCount == 0
            ? "complete: every active well reported"
            : $"{MissingWellCount.ToString(CultureInfo.InvariantCulture)} of " +
              $"{ActiveWells.ToString(CultureInfo.InvariantCulture)} active well(s) have no measurement for the day"
        : "no data for the day, so completeness is not known";

    /// <summary>PRD-03 owns the deferment log; until it exists the card is labelled, never zero.</summary>
    public bool DeferredAvailable => false;

    public string DeferredText => "not recorded: the deferment log is PRD-03";

    /// <summary>PRD-04 owns allocation; until it exists the day is measured only, labelled.</summary>
    public bool AllocationAvailable => false;

    public string AllocationText => "measured only: allocation is PRD-04";

    public string DowntimeText => "no downtime rows: the deferment log is PRD-03";

    public string NotComparableText => NotComparable.Count == 0
        ? "every volume is stated in one unit per fluid"
        : "excluded from the totals because the unit is not the fluid's recorded unit: " +
          string.Join("; ", NotComparable);

    /// <summary>The 30-day strip, oldest first; an unreported day is named, not drawn as zero.</summary>
    public string StripText => Days.Count == 0
        ? "no day in the window"
        : string.Join(
            " · ",
            Days.Select(day =>
                $"{day.Date.ToString("MM-dd", CultureInfo.InvariantCulture)} " +
                (day.Reported
                    ? OgProductionDay.Format(day.Oil, day.OilUnit)
                    : "not reported")));

    public string ExcludedText =>
        $"excluded rows: {InjectionRowsExcluded.ToString(CultureInfo.InvariantCulture)} injection · " +
        $"{UndatedRowsExcluded.ToString(CultureInfo.InvariantCulture)} without a volume date · " +
        $"{SupersededRowsExcluded.ToString(CultureInfo.InvariantCulture)} superseded amendment";
}

/// <summary>DEV/PRD: the daily operations centre (PRD-01).</summary>
public interface IOgOperationsCentre
{
    /// <summary>
    /// Reads one day and the window behind it. A failed read is a failure, never a page of zero cards (AC-1).
    /// </summary>
    Task<OgResult<OgOperationsCentre>> LoadAsync(
        string? fieldId,
        string fieldName,
        DateTime? day = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// The operations centre over the PPDM <c>WELL</c>, <c>PDEN_WELL</c>, <c>PDEN_VOL_SUMMARY</c> and <c>FIELD</c> rows,
/// read through their entity models and a unit of work. It writes nothing: the day is measurement, not a document.
/// </summary>
/// <remarks>
/// The grain is <c>PDEN_VOL_SUMMARY.VOLUME_DATE</c> for the production entity of each well of the field, taking the
/// highest <c>AMENDMENT_SEQ_NO</c> per entity/day/activity so an amended row supersedes the original. Injection rows and
/// rows without a volume date are excluded and counted. Totals are summed only within one unit per fluid; a row in
/// another unit is named as not comparable and is never converted (AC-3).
/// </remarks>
public sealed class OgOperationsCentreService : IOgOperationsCentre
{
    public const string CodeNoConnection = "operations-no-connection";
    public const string CodeReadFailed = "operations-read-failed";

    private readonly Func<IDataSource?> _source;
    private readonly IFailureReporter _failures;

    public OgOperationsCentreService(Func<IDataSource?> source, IFailureReporter failures)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _failures = failures ?? throw new ArgumentNullException(nameof(failures));
    }

    public async Task<OgResult<OgOperationsCentre>> LoadAsync(
        string? fieldId,
        string fieldName,
        DateTime? day = null,
        CancellationToken cancellationToken = default)
    {
        var source = _source();

        if (source is null)
        {
            return OgResult<OgOperationsCentre>.Failed(
                "No connection is open, so the operations centre cannot be read.", CodeNoConnection);
        }

        try
        {
            var model = await ReadAsync(source, cancellationToken).ConfigureAwait(false);
            var selected = (day ?? DateTime.Today).Date;
            var windowStart = selected.AddDays(-(OperationsCentreRules.WindowDays - 1));

            var field = model.Fields.FirstOrDefault(row =>
                (!string.IsNullOrWhiteSpace(fieldId)
                 && string.Equals(row.FIELD_ID, fieldId.Trim(), StringComparison.OrdinalIgnoreCase))
                || (!string.IsNullOrWhiteSpace(fieldName)
                    && string.Equals(row.FIELD_NAME, fieldName.Trim(), StringComparison.OrdinalIgnoreCase)));

            var wells = model.Wells
                .Where(well => !string.IsNullOrWhiteSpace(well.UWI))
                .Where(well => FieldMatches(well.ASSIGNED_FIELD, fieldId, fieldName, field))
                .GroupBy(well => well.UWI!, StringComparer.OrdinalIgnoreCase)
                .Select(group => group.First())
                .ToList();

            var uwiById = wells.ToDictionary(well => well.UWI!, well => well, StringComparer.OrdinalIgnoreCase);

            var pdenToWell = model.PdenWells
                .Where(row => !string.IsNullOrWhiteSpace(row.PDEN_ID) && !string.IsNullOrWhiteSpace(row.PRIMARY_UWI))
                .Where(row => !string.Equals(row.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase))
                .Where(row => uwiById.ContainsKey(row.PRIMARY_UWI!))
                .GroupBy(row => row.PDEN_ID!, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);

            var rows = model.Volumes
                .Where(row => !string.IsNullOrWhiteSpace(row.PDEN_ID))
                .Where(row => pdenToWell.ContainsKey(row.PDEN_ID!))
                .Where(row => !string.Equals(row.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase))
                .ToList();

            var undated = rows.Count(row => row.VOLUME_DATE is null);
            var dated = rows.Where(row => row.VOLUME_DATE is not null).ToList();

            // The highest amendment of a row supersedes the original.
            var latest = dated
                .GroupBy(row => (
                    row.PDEN_ID!.Trim(),
                    Day: row.VOLUME_DATE!.Value.Date,
                    Activity: (row.ACTIVITY_TYPE ?? string.Empty).Trim().ToUpperInvariant()))
                .Select(group => group.OrderByDescending(row => row.AMENDMENT_SEQ_NO).First())
                .ToList();

            var superseded = dated.Count - latest.Count;
            var injection = latest.Count(row => !OperationsCentreRules.IsProductionRow(row.ACTIVITY_TYPE));
            var production = latest.Where(row => OperationsCentreRules.IsProductionRow(row.ACTIVITY_TYPE)).ToList();

            var outOfWindow = production
                .Where(row => row.VOLUME_DATE!.Value.Date < windowStart || row.VOLUME_DATE!.Value.Date > selected)
                .Count();

            var inWindow = production
                .Where(row => row.VOLUME_DATE!.Value.Date >= windowStart && row.VOLUME_DATE!.Value.Date <= selected)
                .ToList();

            var notComparable = new List<string>();

            var days = Enumerable.Range(0, OperationsCentreRules.WindowDays)
                .Select(offset => windowStart.AddDays(offset))
                .Select(date => BuildDay(date, inWindow, notComparable))
                .ToList();

            var today = days.Single(day => day.Date == selected);

            var wellDays = inWindow
                .Where(row => row.VOLUME_DATE!.Value.Date == selected)
                .Select(row => BuildWellDay(row, uwiById, pdenToWell))
                .Where(row => row is not null)
                .Select(row => row!)
                .OrderBy(row => row.WellId, StringComparer.OrdinalIgnoreCase)
                .ToList();

            var reportedWells = wellDays
                .Select(row => row.WellId)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var alerts = wells
                .Where(well => !reportedWells.Contains(well.UWI!))
                .Select(well => new OgProductionAlert(
                    well.UWI!,
                    Blank(well.WELL_NAME),
                    today.Reported
                        ? "no production measurement for the day"
                        : "not yet reported for the day"))
                .OrderBy(alert => alert.WellId, StringComparer.OrdinalIgnoreCase)
                .ToList();

            var (gainers, decliners, moversAvailable, moversReason) =
                Movers(inWindow, selected, pdenToWell, uwiById);

            return OgResult<OgOperationsCentre>.Ok(new OgOperationsCentre(
                Blank(field?.FIELD_NAME) ?? (string.IsNullOrWhiteSpace(fieldName) ? "(no field selected)" : fieldName.Trim()),
                Blank(field?.FIELD_ID) ?? Blank(fieldId),
                selected,
                today,
                days,
                wellDays,
                gainers,
                decliners,
                moversAvailable,
                moversReason,
                alerts,
                wells.Count(well => !string.Equals(well.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase)),
                wells.Count,
                notComparable.Distinct(StringComparer.OrdinalIgnoreCase).ToList(),
                injection,
                undated + outOfWindow,
                superseded));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                "reading the operations centre",
                "the day's production is not shown and no card is rendered",
                FailureSeverity.Error);

            return OgResult<OgOperationsCentre>.Failed(
                "The operations centre could not be read.", reference ?? CodeReadFailed);
        }
    }

    private static OgProductionDay BuildDay(
        DateTime date,
        IReadOnlyList<PDEN_VOL_SUMMARY> rows,
        List<string> notComparable)
    {
        var mine = rows.Where(row => row.VOLUME_DATE!.Value.Date == date).ToList();

        var oilUnit = OperationsCentreRules.DominantUnit(mine.Select(row => row.OIL_VOLUME_OUOM));
        var gasUnit = OperationsCentreRules.DominantUnit(mine.Select(row => row.GAS_VOLUME_OUOM));
        var waterUnit = OperationsCentreRules.DominantUnit(mine.Select(row => row.WATER_VOLUME_OUOM));

        var (oil, oilExcluded) = Sum(mine, row => row.OIL_VOLUME, row => row.OIL_VOLUME_OUOM, oilUnit);
        var (gas, gasExcluded) = Sum(mine, row => row.GAS_VOLUME, row => row.GAS_VOLUME_OUOM, gasUnit);
        var (water, waterExcluded) = Sum(mine, row => row.WATER_VOLUME, row => row.WATER_VOLUME_OUOM, waterUnit);

        if (oilExcluded > 0 && oilUnit is not null)
        {
            notComparable.Add($"{oilExcluded.ToString(CultureInfo.InvariantCulture)} oil row(s) not in {oilUnit}");
        }

        if (gasExcluded > 0 && gasUnit is not null)
        {
            notComparable.Add($"{gasExcluded.ToString(CultureInfo.InvariantCulture)} gas row(s) not in {gasUnit}");
        }

        if (waterExcluded > 0 && waterUnit is not null)
        {
            notComparable.Add($"{waterExcluded.ToString(CultureInfo.InvariantCulture)} water row(s) not in {waterUnit}");
        }

        var wellCount = mine
            .Select(row => row.PDEN_ID!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Count();

        return new OgProductionDay(
            date,
            mine.Count == 0 ? null : oil,
            mine.Count == 0 ? null : gas,
            mine.Count == 0 ? null : water,
            oilUnit,
            gasUnit,
            waterUnit,
            wellCount,
            mine.Count);
    }

    private static (decimal? Total, int Excluded) Sum(
        IReadOnlyList<PDEN_VOL_SUMMARY> rows,
        Func<PDEN_VOL_SUMMARY, decimal> value,
        Func<PDEN_VOL_SUMMARY, string?> unit,
        string? dominant)
    {
        if (rows.Count == 0)
        {
            return (null, 0);
        }

        if (dominant is null)
        {
            return (null, 0);
        }

        var matching = rows
            .Where(row => string.Equals((unit(row) ?? string.Empty).Trim(), dominant, StringComparison.OrdinalIgnoreCase))
            .ToList();

        var total = matching.Count == 0 ? 0m : matching.Sum(value);

        return (total, rows.Count - matching.Count);
    }

    private static OgWellDay? BuildWellDay(
        PDEN_VOL_SUMMARY row,
        IReadOnlyDictionary<string, WELL> wells,
        IReadOnlyDictionary<string, PDEN_WELL> pdenToWell)
    {
        if (!pdenToWell.TryGetValue(row.PDEN_ID!, out var link) || link.PRIMARY_UWI is null)
        {
            return null;
        }

        wells.TryGetValue(link.PRIMARY_UWI, out var well);

        return new OgWellDay(
            link.PRIMARY_UWI,
            Blank(well?.WELL_NAME),
            row.VOLUME_DATE!.Value.Date,
            row.OIL_VOLUME,
            row.GAS_VOLUME,
            row.WATER_VOLUME,
            Blank(row.OIL_VOLUME_OUOM),
            Blank(row.GAS_VOLUME_OUOM),
            Blank(row.WATER_VOLUME_OUOM));
    }

    /// <summary>
    /// AC-5: a well's change needs at least seven days of its own history. Wells with less are left out and counted,
    /// and when no well qualifies the panel is hidden with the reason.
    /// </summary>
    private static (IReadOnlyList<OgMover> Gainers, IReadOnlyList<OgMover> Decliners, bool Available, string Reason)
        Movers(
            IReadOnlyList<PDEN_VOL_SUMMARY> rows,
            DateTime day,
            IReadOnlyDictionary<string, PDEN_WELL> pdenToWell,
            IReadOnlyDictionary<string, WELL> wells)
    {
        var byWell = rows
            .Where(row => row.PDEN_ID is not null)
            .GroupBy(row => row.PDEN_ID!, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var movers = new List<OgMover>();

        foreach (var entity in byWell)
        {
            var unit = OperationsCentreRules.DominantUnit(entity.Select(row => row.OIL_VOLUME_OUOM));

            if (unit is null)
            {
                continue;
            }

            var daily = entity
                .Where(row => string.Equals(
                    (row.OIL_VOLUME_OUOM ?? string.Empty).Trim(), unit, StringComparison.OrdinalIgnoreCase))
                .GroupBy(row => row.VOLUME_DATE!.Value.Date)
                .ToDictionary(group => group.Key, group => group.Sum(row => row.OIL_VOLUME));

            if (!daily.TryGetValue(day, out var today))
            {
                continue;
            }

            var history = daily
                .Where(pair => pair.Key < day)
                .OrderByDescending(pair => pair.Key)
                .Take(OperationsCentreRules.RequiredHistoryDays)
                .ToList();

            if (history.Count < OperationsCentreRules.RequiredHistoryDays)
            {
                continue;
            }

            var average = history.Average(pair => pair.Value);

            // The well is the PDEN link's UWI; a row whose entity has no well is not a well mover.
            if (!pdenToWell.TryGetValue(entity.Key, out var link) || link.PRIMARY_UWI is null)
            {
                continue;
            }

            wells.TryGetValue(link.PRIMARY_UWI, out var well);

            movers.Add(new OgMover(
                link.PRIMARY_UWI,
                Blank(well?.WELL_NAME),
                today,
                average,
                today - average,
                unit,
                history.Count));
        }

        var qualified = movers
            .OrderByDescending(mover => mover.Change)
            .ToList();

        var gainers = qualified.Where(mover => mover.Change > 0m).Take(5).ToList();
        var decliners = qualified.Where(mover => mover.Change < 0m).Reverse().Take(5).ToList();

        return qualified.Count > 0
            ? (gainers, decliners, true, $"compared with each well's previous " +
                $"{OperationsCentreRules.RequiredHistoryDays.ToString(CultureInfo.InvariantCulture)} day(s); " +
                $"{qualified.Count.ToString(CultureInfo.InvariantCulture)} well(s) have enough history")
            : ([], [], false,
                $"hidden: no well has {OperationsCentreRules.RequiredHistoryDays.ToString(CultureInfo.InvariantCulture)} " +
                "day(s) of history before this day");
    }

    private static async Task<CentreModel> ReadAsync(IDataSource source, CancellationToken cancellationToken)
    {
        var fields = await OgUow.ReadAllAsync<FIELD>(source).ConfigureAwait(false);
        var wells = await OgUow.ReadAllAsync<WELL>(source).ConfigureAwait(false);
        var pdenWells = await OgUow.ReadAllAsync<PDEN_WELL>(source).ConfigureAwait(false);
        var volumes = await OgUow.ReadAllAsync<PDEN_VOL_SUMMARY>(source).ConfigureAwait(false);

        cancellationToken.ThrowIfCancellationRequested();

        return new CentreModel(fields, wells, pdenWells, volumes);
    }

    private static bool FieldMatches(string? value, string? fieldId, string? fieldName, FIELD? field)
    {
        if (!string.IsNullOrWhiteSpace(fieldId)
            && string.Equals(value ?? string.Empty, fieldId.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (!string.IsNullOrWhiteSpace(fieldName)
            && string.Equals(value ?? string.Empty, fieldName.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        // The session may name the field by its id while the wells carry the field id; when neither matches directly,
        // the field row's own id is the last honest comparison.
        return field?.FIELD_ID is { } resolved
               && string.Equals(value ?? string.Empty, resolved.Trim(), StringComparison.OrdinalIgnoreCase);
    }

    private static string? Blank(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value;

    private sealed record CentreModel(
        IReadOnlyList<FIELD> Fields,
        IReadOnlyList<WELL> Wells,
        IReadOnlyList<PDEN_WELL> PdenWells,
        IReadOnlyList<PDEN_VOL_SUMMARY> Volumes);
}
