using System.Globalization;
using Beep.OilandGas.Models.Data.ProductionOperations;
using Beep.OilandGas.PPDM39.Models;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Data.Production;

/// <summary>One recorded deferment of a well (PRD-03), matching the plan's <c>OgDeferment</c>.</summary>
public sealed record OgDeferment(
    string Id,
    string WellId,
    string? WellName,
    string? FieldId,
    DateTime Start,
    DateTime? End,
    string ReasonCode,
    string? ReasonName,
    decimal? Volume,
    string? Unit,
    string? Comment,
    string? Reporter,
    IReadOnlyList<string> OverlapWith)
{
    /// <summary>AC-5: an ongoing deferment has no end yet, which is a different thing from a closed one.</summary>
    public bool IsOngoing => End is null;

    public bool Overlaps => OverlapWith.Count > 0;

    public decimal ElapsedHours(DateTime asOf) => DefermentRules.ElapsedHours(Start, End, asOf);

    public string DurationText(DateTime asOf) =>
        (IsOngoing ? "ongoing · " : string.Empty) +
        $"{ElapsedHours(asOf).ToString("0.##", CultureInfo.InvariantCulture)} h" +
        (IsOngoing ? " so far" : string.Empty);

    public string VolumeText => OgProductionDay.Format(Volume, Unit);

    public string ReasonText => string.IsNullOrWhiteSpace(ReasonName)
        ? ReasonCode
        : $"{ReasonCode} ({ReasonName!.Trim()})";

    public string StartText => Start.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);

    public string EndText => End?.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture) ?? "ongoing";

    /// <summary>AC-2: the grid's overlap column - the sentence, not a flag the reader has to look up.</summary>
    public string OverlapText => Overlaps
        ? $"overlaps {OverlapWith.Count.ToString(CultureInfo.InvariantCulture)}"
        : "no overlap";

    public string Describe() =>
        $"{(WellName ?? WellId)} · {ReasonText} · {StartText} → {EndText} · {DurationText(DateTime.Today)} · " +
        $"{VolumeText}" +
        (Overlaps ? $" · overlaps {string.Join(", ", OverlapWith)}" : string.Empty) +
        (string.IsNullOrWhiteSpace(Comment) ? string.Empty : $" · {Comment}");
}

/// <summary>One well on the status board (PRD-03).</summary>
public sealed record OgWellCard(
    string WellId,
    string? Name,
    string Status,
    string? StatusType,
    DateTime? StatusSince,
    DateTime? StatusEnd,
    decimal? Rate,
    string? RateUnit,
    string? OpenDefermentId,
    string? OpenReasonCode,
    string? OpenReasonName,
    DateTime? OpenSince)
{
    public bool IsDeferred => OpenDefermentId is not null;

    public string NameText => Name ?? WellId;

    public string StatusText => string.IsNullOrWhiteSpace(Status) ? "no status recorded" : Status!.ToLowerInvariant();

    public string RateText => OgProductionDay.Format(Rate, RateUnit);

    public string SinceText => StatusSince?.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture)
                               ?? "not recorded";

    /// <summary>The reason is the open deferment's coded reason; a well with none says so rather than "unknown".</summary>
    public string ReasonText => IsDeferred
        ? string.IsNullOrWhiteSpace(OpenReasonName)
            ? OpenReasonCode!
            : $"{OpenReasonCode} ({OpenReasonName!.Trim()})"
        : "no open deferment";

    public string DefermentText => IsDeferred
        ? $"deferred since {OpenSince?.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture) ?? "unknown"}"
        : "not deferred";

    public string Describe() =>
        $"{NameText} · {StatusText} · since {SinceText} · rate {RateText} · {DefermentText} · {ReasonText}";
}

/// <summary>One status and the wells that carry it (PRD-03 status board).</summary>
public sealed record OgStatusGroup(
    string Status,
    string? StatusType,
    IReadOnlyList<OgWellCard> Wells)
{
    public int Count => Wells.Count;

    public string StatusText => Status.ToLowerInvariant();

    public string Describe() =>
        $"{StatusText} ({Wells.Count.ToString(CultureInfo.InvariantCulture)})" +
        (string.IsNullOrWhiteSpace(StatusType) ? string.Empty : $" · {StatusType!.Trim()}");
}

/// <summary>The deferred volume of one reason code, summed within one unit (PRD-03 AC-4).</summary>
public sealed record OgReasonTotal(
    string Code,
    string? Name,
    int Count,
    decimal? Volume,
    string? Unit,
    IReadOnlyList<string> MixedUnits)
{
    public bool HasMixedUnits => MixedUnits.Count > 0;

    public string VolumeText => HasMixedUnits
        ? "mixed units, so no total"
        : OgProductionDay.Format(Volume, Unit);

    public string Text =>
        $"{Code}{(string.IsNullOrWhiteSpace(Name) ? string.Empty : $" ({Name!.Trim()})")}: " +
        $"{Count.ToString(CultureInfo.InvariantCulture)} deferment(s) · {VolumeText}";
}

/// <summary>The status board, the deferment log and their totals (PRD-03).</summary>
public sealed record OgWellStatusBoard(
    string FieldName,
    string? FieldId,
    DateTime AsOf,
    IReadOnlyList<OgStatusGroup> Groups,
    IReadOnlyList<OgWellCard> Wells,
    IReadOnlyList<OgDeferment> Deferments,
    IReadOnlyList<OgReasonTotal> Totals,
    decimal? TotalVolume,
    string? TotalUnit,
    IReadOnlyList<string> Reasons,
    bool ReasonReferenceMissing,
    IReadOnlyList<string> MixedUnits)
{
    public int WellCount => Wells.Count;

    public int DeferredWells => Wells.Count(well => well.IsDeferred);

    public int OngoingCount => Deferments.Count(deferment => deferment.IsOngoing);

    /// <summary>AC-2: overlapping deferments are counted and shown, never merged.</summary>
    public int OverlapCount => Deferments.Count(deferment => deferment.Overlaps);

    public string ReasonsText => ReasonReferenceMissing
        ? "No deferment reason code is recorded, so no deferment can be saved. Add the operator's code book to " +
          "R_DEFERMENT_REASON."
        : string.Join(", ", Reasons);

    public string TotalsText => Totals.Count == 0
        ? "no deferment is recorded for this field"
        : string.Join(" · ", Totals.Select(total => total.Text));

    /// <summary>AC-4: the stated total is the sum of the same rows the list shows.</summary>
    public string TotalText => MixedUnits.Count > 0
        ? "the deferred volume is stated in more than one unit, so no single total is produced"
        : OgProductionDay.Format(TotalVolume, TotalUnit);

    public string Summary =>
        $"{WellCount.ToString(CultureInfo.InvariantCulture)} well(s) · " +
        $"{DeferredWells.ToString(CultureInfo.InvariantCulture)} deferred · " +
        $"{OngoingCount.ToString(CultureInfo.InvariantCulture)} ongoing deferment(s) · " +
        $"{OverlapCount.ToString(CultureInfo.InvariantCulture)} overlapping · deferred volume {TotalText}";

    public string GroupText => Groups.Count == 0
        ? "no well status is recorded for this field"
        : string.Join(" · ", Groups.Select(group => group.Describe()));
}

/// <summary>One deferment, as the page logs it.</summary>
public sealed record OgDefermentValues(
    string? WellId,
    DateTime? Start,
    DateTime? End,
    string? ReasonCode,
    decimal? Volume,
    string? VolumeUnit,
    string? Comment,
    string? Reporter);

/// <summary>The deferment rules, stated once so the page, the service and the tests agree.</summary>
public static class DefermentRules
{
    public const string CodeWellRequired = "deferment-well-required";
    public const string CodeWellUnknown = "deferment-well-unknown";
    public const string CodeStartRequired = "deferment-start-required";
    public const string CodeEndBeforeStart = "deferment-end-before-start";
    public const string CodeReasonRequired = "deferment-reason-required";
    public const string CodeReasonReferenceMissing = "deferment-reason-reference-missing";
    public const string CodeReasonUnknown = "deferment-reason-unknown";
    public const string CodeVolumeInvalid = "deferment-volume-invalid";
    public const string CodeVolumeUnitRequired = "deferment-volume-unit-required";
    public const string CodeReporterRequired = "deferment-reporter-required";
    public const string CodeAlreadyClosed = "deferment-already-closed";
    public const string CodeUnknown = "deferment-unknown";
    public const string CodeActorRequired = "deferment-actor-required";

    /// <summary>The hours between the start and the end, or between the start and now while it is ongoing.</summary>
    public static decimal ElapsedHours(DateTime start, DateTime? end, DateTime asOf)
    {
        var finish = end ?? asOf;
        var hours = (decimal)(finish - start).TotalHours;

        return hours < 0m ? 0m : hours;
    }

    /// <summary>
    /// AC-2: two deferments of one well overlap when their intervals intersect; an ongoing deferment is open-ended.
    /// Overlap is flagged, never merged.
    /// </summary>
    public static bool Overlaps(DateTime startA, DateTime? endA, DateTime startB, DateTime? endB)
    {
        var endOfA = endA ?? DateTime.MaxValue;
        var endOfB = endB ?? DateTime.MaxValue;

        return startA <= endOfB && startB <= endOfA;
    }

    /// <summary>AC-1, AC-3 and the state table's "reason code missing is refused".</summary>
    public static (string Code, string Message)? Validate(
        OgDefermentValues values,
        IReadOnlyCollection<string> reasons)
    {
        ArgumentNullException.ThrowIfNull(values);
        ArgumentNullException.ThrowIfNull(reasons);

        if (string.IsNullOrWhiteSpace(values.WellId))
        {
            return (CodeWellRequired, "Choose the well the deferment belongs to.");
        }

        if (values.Start is null)
        {
            return (CodeStartRequired, "A deferment records when the lost production started.");
        }

        if (values.End is { } end && end < values.Start.Value)
        {
            return (CodeEndBeforeStart,
                $"The deferment ends {end:yyyy-MM-dd HH:mm}, before it starts on {values.Start.Value:yyyy-MM-dd HH:mm}.");
        }

        if (string.IsNullOrWhiteSpace(values.ReasonCode))
        {
            return (CodeReasonRequired,
                reasons.Count == 0
                    ? "A deferment needs a reason code, and no code is recorded in the reference set yet."
                    : $"A deferment needs a reason code; use {string.Join(", ", reasons)}.");
        }

        if (reasons.Count == 0)
        {
            return (CodeReasonReferenceMissing,
                "No deferment reason code is recorded in the reference set, so a deferment cannot be saved.");
        }

        if (!reasons.Contains(values.ReasonCode.Trim(), StringComparer.OrdinalIgnoreCase))
        {
            return (CodeReasonUnknown,
                $"'{values.ReasonCode}' is not in the deferment reason set; use {string.Join(", ", reasons)}.");
        }

        if (values.Volume is < 0m)
        {
            return (CodeVolumeInvalid, "A deferred volume cannot be negative.");
        }

        // AC-3: a deferred volume carries its unit.
        if (values.Volume is > 0m && string.IsNullOrWhiteSpace(values.VolumeUnit))
        {
            return (CodeVolumeUnitRequired, "A deferred volume needs the unit it is measured in (for example bbl or MMscf).");
        }

        return string.IsNullOrWhiteSpace(values.Reporter)
            ? (CodeReporterRequired, "A deferment records who reported it.")
            : null;
    }

    /// <summary>AC-4: the totals are built from the same rows the list shows, within one unit per reason.</summary>
    public static IReadOnlyList<OgReasonTotal> Totals(IReadOnlyList<OgDeferment> rows)
    {
        ArgumentNullException.ThrowIfNull(rows);

        return rows
            .GroupBy(row => row.ReasonCode, StringComparer.OrdinalIgnoreCase)
            .OrderBy(group => group.Key, StringComparer.OrdinalIgnoreCase)
            .Select(group =>
            {
                var units = group
                    .Where(row => !string.IsNullOrWhiteSpace(row.Unit))
                    .Select(row => row.Unit!.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(unit => unit, StringComparer.OrdinalIgnoreCase)
                    .ToList();

                if (units.Count == 1)
                {
                    return new OgReasonTotal(
                        group.Key,
                        group.Select(row => row.ReasonName).FirstOrDefault(name => !string.IsNullOrWhiteSpace(name)),
                        group.Count(),
                        group.Sum(row => row.Volume ?? 0m),
                        units[0],
                        []);
                }

                // Two units are never added; the rows are counted and the mix is named instead.
                return new OgReasonTotal(
                    group.Key,
                    group.Select(row => row.ReasonName).FirstOrDefault(name => !string.IsNullOrWhiteSpace(name)),
                    group.Count(),
                    null,
                    null,
                    units);
            })
            .ToList();
    }

    /// <summary>The field's deferred volume: a single unit, or null with the mixed units named.</summary>
    public static (decimal? Volume, string? Unit, IReadOnlyList<string> Mixed) TotalVolume(
        IReadOnlyList<OgDeferment> rows)
    {
        var units = rows
            .Where(row => !string.IsNullOrWhiteSpace(row.Unit))
            .Select(row => row.Unit!.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(unit => unit, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return units.Count switch
        {
            0 => (null, null, []),
            1 => (rows.Sum(row => row.Volume ?? 0m), units[0], []),
            _ => (null, null, units),
        };
    }
}

/// <summary>PRD-03: the well status board and the deferment log.</summary>
public interface IOgWellStatusDeferment
{
    Task<OgResult<OgWellStatusBoard>> LoadAsync(
        string? fieldId,
        string fieldName,
        DateTime? asOf = null,
        CancellationToken cancellationToken = default);

    /// <summary>Logs a deferment; a reason code outside the reference set is refused (AC-1).</summary>
    Task<OgResult<OgWellStatusBoard>> LogDefermentAsync(
        OgDefermentValues values,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>Closes an ongoing deferment (AC-5).</summary>
    Task<OgResult<OgWellStatusBoard>> CloseDefermentAsync(
        string defermentId,
        DateTime end,
        string actor,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// The board over the PPDM <c>WELL</c>, <c>WELL_STATUS</c>, <c>PDEN_WELL</c> and <c>PDEN_VOL_SUMMARY</c> rows and the
/// ProductionOperations <c>PRODUCTION_DEFERMENT</c> and <c>R_DEFERMENT_REASON</c> rows, read and written through their
/// entity models and a unit of work.
/// </summary>
/// <remarks>
/// The status itself comes from PPDM's status history (the latest <c>WELL_STATUS</c> row per well); the deferment - its
/// coded reason and its volume - comes from <c>PRODUCTION_DEFERMENT</c>, because the status row carries neither. That is
/// the answer to the feature's open question, recorded in the evidence.
/// </remarks>
public sealed class OgWellStatusDefermentService : IOgWellStatusDeferment
{
    public const string CodeNoConnection = "well-status-no-connection";
    public const string CodeReadFailed = "well-status-read-failed";
    public const string CodeWriteFailed = "well-status-write-failed";

    private readonly Func<IDataSource?> _source;
    private readonly IFailureReporter _failures;

    public OgWellStatusDefermentService(Func<IDataSource?> source, IFailureReporter failures)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _failures = failures ?? throw new ArgumentNullException(nameof(failures));
    }

    public async Task<OgResult<OgWellStatusBoard>> LoadAsync(
        string? fieldId,
        string fieldName,
        DateTime? asOf = null,
        CancellationToken cancellationToken = default)
    {
        var source = _source();

        if (source is null)
        {
            return OgResult<OgWellStatusBoard>.Failed(
                "No connection is open, so the well status board cannot be read.", CodeNoConnection);
        }

        try
        {
            var model = await ReadAsync(source, cancellationToken).ConfigureAwait(false);

            return OgResult<OgWellStatusBoard>.Ok(
                Build(model, fieldId, fieldName, (asOf ?? DateTime.Today).Date));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                "reading the well status board",
                "the status board and the deferment log are not shown",
                FailureSeverity.Error);

            return OgResult<OgWellStatusBoard>.Failed(
                "The well status board could not be read.", reference ?? CodeReadFailed);
        }
    }

    public async Task<OgResult<OgWellStatusBoard>> LogDefermentAsync(
        OgDefermentValues values,
        string actor,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(values);

        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgWellStatusBoard>.Failed(
                "A deferment must record who logged it.", DefermentRules.CodeActorRequired);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgWellStatusBoard>.Failed(
                "No connection is open, so the deferment cannot be saved.", CodeNoConnection);
        }

        try
        {
            var model = await ReadAsync(source, cancellationToken).ConfigureAwait(false);
            var reasons = Reasons(model);

            if (DefermentRules.Validate(values, reasons) is { } refusal)
            {
                return OgResult<OgWellStatusBoard>.Failed(refusal.Message, refusal.Code);
            }

            var well = model.Wells.FirstOrDefault(row =>
                string.Equals(row.UWI, values.WellId!.Trim(), StringComparison.OrdinalIgnoreCase));

            if (well is null)
            {
                return OgResult<OgWellStatusBoard>.Failed(
                    $"Well '{values.WellId}' is not in this database.", DefermentRules.CodeWellUnknown);
            }

            var now = DateTime.UtcNow;

            var unit = OgUow.For<PRODUCTION_DEFERMENT>(source);
            unit.PrimaryKey = "DEFERMENT_ID";

            unit.Add(new PRODUCTION_DEFERMENT
            {
                DEFERMENT_ID = Guid.NewGuid().ToString("N"),
                WELL_ID = well.UWI!,
                FIELD_ID = well.ASSIGNED_FIELD,
                START_AT = values.Start!.Value,
                END_AT = values.End,
                REASON_CODE = values.ReasonCode!.Trim().ToUpperInvariant(),
                DEFERRED_VOLUME = values.Volume,
                VOLUME_OUOM = Blank(values.VolumeUnit)?.Trim(),
                COMMENT = Blank(values.Comment),
                REPORTER = values.Reporter!.Trim(),
                ACTOR = actor.Trim(),
                RECORDED_AT = now,
                ROW_CREATED_BY = actor.Trim(),
                ROW_CREATED_DATE = now,
                ROW_CHANGED_BY = actor.Trim(),
                ROW_CHANGED_DATE = now,
            });

            var committed = await unit.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(committed))
            {
                return OgResult<OgWellStatusBoard>.Failed(
                    OgUow.Message(committed, "The deferment was not saved."), CodeWriteFailed);
            }

            var after = await ReadAsync(source, cancellationToken).ConfigureAwait(false);

            return OgResult<OgWellStatusBoard>.Ok(
                Build(after, well.ASSIGNED_FIELD, string.Empty, DateTime.Today));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                $"logging a deferment on well {values.WellId}",
                "the deferment was not saved",
                FailureSeverity.Error);

            return OgResult<OgWellStatusBoard>.Failed(
                "The deferment could not be saved.", reference ?? CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgWellStatusBoard>> CloseDefermentAsync(
        string defermentId,
        DateTime end,
        string actor,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgWellStatusBoard>.Failed(
                "Closing a deferment must record who closed it.", DefermentRules.CodeActorRequired);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgWellStatusBoard>.Failed(
                "No connection is open, so the deferment cannot be closed.", CodeNoConnection);
        }

        try
        {
            var model = await ReadAsync(source, cancellationToken).ConfigureAwait(false);
            var existing = model.Deferments.FirstOrDefault(row =>
                string.Equals(row.DEFERMENT_ID, defermentId.Trim(), StringComparison.OrdinalIgnoreCase));

            if (existing is null)
            {
                return OgResult<OgWellStatusBoard>.Failed(
                    $"Deferment '{defermentId}' is not in this database.", DefermentRules.CodeUnknown);
            }

            if (existing.END_AT is not null)
            {
                return OgResult<OgWellStatusBoard>.Failed(
                    "This deferment is already closed; a closed deferment is not re-closed.",
                    DefermentRules.CodeAlreadyClosed);
            }

            if (end < existing.START_AT)
            {
                return OgResult<OgWellStatusBoard>.Failed(
                    $"The deferment started {existing.START_AT:yyyy-MM-dd HH:mm}, so it cannot end at {end:yyyy-MM-dd HH:mm}.",
                    DefermentRules.CodeEndBeforeStart);
            }

            var unit = OgUow.For<PRODUCTION_DEFERMENT>(source);
            unit.PrimaryKey = "DEFERMENT_ID";

            var rows = await OgUow.ReadAsync(
                unit,
                [OgUow.Equal("DEFERMENT_ID", existing.DEFERMENT_ID)],
                cancellationToken).ConfigureAwait(false);
            var row = rows.FirstOrDefault();

            if (row is null)
            {
                return OgResult<OgWellStatusBoard>.Failed(
                    $"Deferment '{defermentId}' is not in this database.", DefermentRules.CodeUnknown);
            }

            row.END_AT = end;
            row.ROW_CHANGED_BY = actor.Trim();
            row.ROW_CHANGED_DATE = DateTime.UtcNow;

            var updated = unit.Update(row);

            if (!OgUow.IsOk(updated))
            {
                return OgResult<OgWellStatusBoard>.Failed(
                    OgUow.Message(updated, "The deferment was not closed."), CodeWriteFailed);
            }

            var committed = await unit.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(committed))
            {
                return OgResult<OgWellStatusBoard>.Failed(
                    OgUow.Message(committed, "The deferment was not closed."), CodeWriteFailed);
            }

            var after = await ReadAsync(source, cancellationToken).ConfigureAwait(false);

            return OgResult<OgWellStatusBoard>.Ok(
                Build(after, existing.FIELD_ID, string.Empty, DateTime.Today));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                $"closing deferment {defermentId}",
                "the deferment was not closed",
                FailureSeverity.Error);

            return OgResult<OgWellStatusBoard>.Failed(
                "The deferment could not be closed.", reference ?? CodeWriteFailed);
        }
    }

    // ------------------------------------------------------------------ reads

    private static async Task<StatusModel> ReadAsync(IDataSource source, CancellationToken cancellationToken)
    {
        var wells = await OgUow.ReadAllAsync<WELL>(source).ConfigureAwait(false);
        var statuses = await OgUow.ReadAllAsync<WELL_STATUS>(source).ConfigureAwait(false);
        var pdenWells = await OgUow.ReadAllAsync<PDEN_WELL>(source).ConfigureAwait(false);
        var volumes = await OgUow.ReadAllAsync<PDEN_VOL_SUMMARY>(source).ConfigureAwait(false);
        var deferments = await OgUow.ReadAllAsync<PRODUCTION_DEFERMENT>(source).ConfigureAwait(false);
        var reasons = await OgUow.ReadAllAsync<R_DEFERMENT_REASON>(source).ConfigureAwait(false);

        cancellationToken.ThrowIfCancellationRequested();

        return new StatusModel(wells, statuses, pdenWells, volumes, deferments, reasons);
    }

    private static IReadOnlyList<string> Reasons(StatusModel model) =>
        model.Reasons
            .Where(row => !string.Equals(row.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase))
            .Where(row => !string.IsNullOrWhiteSpace(row.REASON_CODE))
            .Select(row => row.REASON_CODE!.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(code => code, StringComparer.OrdinalIgnoreCase)
            .ToList();

    private static OgWellStatusBoard Build(
        StatusModel model,
        string? fieldId,
        string fieldName,
        DateTime asOf)
    {
        var wells = model.Wells
            .Where(row => !string.IsNullOrWhiteSpace(row.UWI))
            .Where(row => FieldMatches(row.ASSIGNED_FIELD, fieldId, fieldName))
            .GroupBy(row => row.UWI!, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .OrderBy(row => row.WELL_NAME ?? row.UWI, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var wellIds = wells
            .Select(row => row.UWI!)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var reasonNames = model.Reasons
            .Where(row => !string.IsNullOrWhiteSpace(row.REASON_CODE))
            .GroupBy(row => row.REASON_CODE!.Trim(), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                group => group.Key,
                group => Blank(group.First().REASON_NAME),
                StringComparer.OrdinalIgnoreCase);

        var deferments = model.Deferments
            .Where(row => !string.IsNullOrWhiteSpace(row.WELL_ID) && wellIds.Contains(row.WELL_ID!))
            .Select(row => ToDeferment(row, model, reasonNames))
            .Select(deferment => deferment with
            {
                OverlapWith = model.Deferments
                    .Where(other => !string.Equals(other.DEFERMENT_ID, deferment.Id, StringComparison.OrdinalIgnoreCase))
                    .Where(other => string.Equals(other.WELL_ID, deferment.WellId, StringComparison.OrdinalIgnoreCase))
                    .Where(other => DefermentRules.Overlaps(
                        deferment.Start,
                        deferment.End,
                        other.START_AT,
                        other.END_AT))
                    .Select(other => other.DEFERMENT_ID ?? string.Empty)
                    .Where(id => id.Length > 0)
                    .OrderBy(id => id, StringComparer.OrdinalIgnoreCase)
                    .ToList(),
            })
            .OrderByDescending(deferment => deferment.Start)
            .ThenBy(deferment => deferment.WellId, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var latestStatus = model.Statuses
            .Where(row => !string.IsNullOrWhiteSpace(row.UWI))
            .Where(row => !string.Equals(row.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase))
            .GroupBy(row => row.UWI!, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                group => group.Key,
                group => group
                    .OrderByDescending(row => row.STATUS_DATE ?? row.START_TIME ?? row.EFFECTIVE_DATE ?? DateTime.MinValue)
                    .First(),
                StringComparer.OrdinalIgnoreCase);

        var rates = LatestRates(model, wellIds, asOf);

        var cards = wells
            .Select(well =>
            {
                latestStatus.TryGetValue(well.UWI!, out var status);

                var open = deferments.FirstOrDefault(deferment =>
                    deferment.IsOngoing
                    && string.Equals(deferment.WellId, well.UWI, StringComparison.OrdinalIgnoreCase));

                rates.TryGetValue(well.UWI!, out var rate);

                return new OgWellCard(
                    well.UWI!,
                    Blank(well.WELL_NAME),
                    Blank(status?.STATUS) ?? Blank(well.CURRENT_STATUS) ?? string.Empty,
                    Blank(status?.STATUS_TYPE),
                    status?.STATUS_DATE ?? status?.START_TIME,
                    status?.END_TIME,
                    rate.Value,
                    rate.Unit,
                    open?.Id,
                    open?.ReasonCode,
                    open?.ReasonName,
                    open?.Start);
            })
            .ToList();

        var groups = cards
            .GroupBy(card => card.StatusText, StringComparer.OrdinalIgnoreCase)
            .OrderByDescending(group => group.Count())
            .ThenBy(group => group.Key, StringComparer.OrdinalIgnoreCase)
            .Select(group => new OgStatusGroup(
                group.First().Status,
                group.First().StatusType,
                group.OrderBy(card => card.NameText, StringComparer.OrdinalIgnoreCase).ToList()))
            .ToList();

        var (totalVolume, totalUnit, mixed) = DefermentRules.TotalVolume(deferments);

        var fieldOfWell = wells
            .Select(row => Blank(row.ASSIGNED_FIELD))
            .FirstOrDefault(value => value is not null);

        return new OgWellStatusBoard(
            string.IsNullOrWhiteSpace(fieldName) ? "(no field selected)" : fieldName.Trim(),
            Blank(fieldId) ?? fieldOfWell,
            asOf,
            groups,
            cards,
            deferments,
            DefermentRules.Totals(deferments),
            totalVolume,
            totalUnit,
            Reasons(model),
            Reasons(model).Count == 0,
            mixed);
    }

    private static OgDeferment ToDeferment(
        PRODUCTION_DEFERMENT row,
        StatusModel model,
        IReadOnlyDictionary<string, string?> reasonNames)
    {
        var well = model.Wells.FirstOrDefault(candidate =>
            string.Equals(candidate.UWI, row.WELL_ID, StringComparison.OrdinalIgnoreCase));

        var code = Blank(row.REASON_CODE) ?? string.Empty;

        return new OgDeferment(
            row.DEFERMENT_ID ?? string.Empty,
            row.WELL_ID ?? string.Empty,
            Blank(well?.WELL_NAME),
            Blank(row.FIELD_ID),
            row.START_AT,
            row.END_AT,
            code,
            reasonNames.TryGetValue(code, out var name) ? name : null,
            row.DEFERRED_VOLUME,
            Blank(row.VOLUME_OUOM),
            Blank(row.COMMENT),
            Blank(row.REPORTER),
            []);
    }

    /// <summary>Each well's oil on its most recent reported day, with its unit.</summary>
    private static Dictionary<string, (decimal? Value, string? Unit)> LatestRates(
        StatusModel model,
        IReadOnlySet<string> wellIds,
        DateTime asOf)
    {
        var byWell = model.PdenWells
            .Where(row => !string.IsNullOrWhiteSpace(row.PRIMARY_UWI) && wellIds.Contains(row.PRIMARY_UWI!))
            .Where(row => !string.Equals(row.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase))
            .GroupBy(row => row.PRIMARY_UWI!, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                group => group.Key,
                group => group
                    .Select(row => row.PDEN_ID ?? string.Empty)
                    .Where(id => id.Length > 0)
                    .ToHashSet(StringComparer.OrdinalIgnoreCase),
                StringComparer.OrdinalIgnoreCase);

        var result = new Dictionary<string, (decimal? Value, string? Unit)>(StringComparer.OrdinalIgnoreCase);

        foreach (var (well, entities) in byWell)
        {
            var rows = model.Volumes
                .Where(row => !string.IsNullOrWhiteSpace(row.PDEN_ID) && entities.Contains(row.PDEN_ID!))
                .Where(row => !string.Equals(row.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase))
                .Where(row => row.VOLUME_DATE is { } date && date.Date <= asOf)
                .Where(row => OperationsCentreRules.IsProductionRow(row.ACTIVITY_TYPE))
                .ToList();

            if (rows.Count == 0)
            {
                continue;
            }

            var latestDay = rows.Max(row => row.VOLUME_DATE!.Value.Date);

            var dayRows = rows
                .Where(row => row.VOLUME_DATE!.Value.Date == latestDay)
                .GroupBy(row => (row.PDEN_ID!.Trim(), row.VOLUME_DATE!.Value.Date,
                    Activity: (row.ACTIVITY_TYPE ?? string.Empty).Trim().ToUpperInvariant()))
                .Select(group => group.OrderByDescending(row => row.AMENDMENT_SEQ_NO).First())
                .ToList();

            var unit = OperationsCentreRules.DominantUnit(dayRows.Select(row => row.OIL_VOLUME_OUOM));

            var value = unit is null
                ? (decimal?)null
                : dayRows
                    .Where(row => string.Equals((row.OIL_VOLUME_OUOM ?? string.Empty).Trim(), unit,
                        StringComparison.OrdinalIgnoreCase))
                    .Sum(row => row.OIL_VOLUME);

            result[well] = (value, unit);
        }

        return result;
    }

    private static bool FieldMatches(string? value, string? fieldId, string? fieldName) =>
        (string.IsNullOrWhiteSpace(fieldId) && string.IsNullOrWhiteSpace(fieldName))
        || (!string.IsNullOrWhiteSpace(fieldId)
            && string.Equals(value ?? string.Empty, fieldId.Trim(), StringComparison.OrdinalIgnoreCase))
        || (!string.IsNullOrWhiteSpace(fieldName)
            && string.Equals(value ?? string.Empty, fieldName.Trim(), StringComparison.OrdinalIgnoreCase));

    private static string? Blank(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value;

    private sealed record StatusModel(
        IReadOnlyList<WELL> Wells,
        IReadOnlyList<WELL_STATUS> Statuses,
        IReadOnlyList<PDEN_WELL> PdenWells,
        IReadOnlyList<PDEN_VOL_SUMMARY> Volumes,
        IReadOnlyList<PRODUCTION_DEFERMENT> Deferments,
        IReadOnlyList<R_DEFERMENT_REASON> Reasons);
}
