using System.Globalization;
using Beep.OilandGas.LifeCycle.Services.WellLifecycle;
using Beep.OilandGas.PPDM39.Models;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Data.Wells;

/// <summary>One choice in the status picker, from R_WELL_STATUS (AC-2).</summary>
public sealed record OgWellStatusOption(string StatusType, string Status, string Label, bool Active);

/// <summary>One row of the append-only status history.</summary>
public sealed record OgWellStatusHistoryEntry(
    string StatusId,
    string StatusType,
    string Status,
    string Label,
    string? FromCode,
    DateTime? EffectiveDate,
    DateTime? RecordedAt,
    string? Actor,
    string? Reason,
    string? Note,
    bool Active);

/// <summary>The well's current status and when it began.</summary>
public sealed record OgWellStatusCurrent(
    string Status,
    string? StatusType,
    string? Label,
    DateTime? Since,
    string? Actor);

/// <summary>Everything the status page shows for one well.</summary>
public sealed record OgWellStatusView(
    OgWellStatusCurrent? Current,
    IReadOnlyList<OgWellStatusOption> Options,
    IReadOnlyList<OgWellStatusHistoryEntry> History,
    IReadOnlyList<string> LegalNext)
{
    /// <summary>State: the reference table has no active codes.</summary>
    public bool ReferenceDataMissing => Options.Count == 0;

    public string? LegalText => LegalNext.Count == 0
        ? null
        : $"Legal next status(es): {string.Join(", ", LegalNext)}.";
}

/// <summary>
/// WEL-04 AC-1..AC-6: a change appends history (never edits it), codes come from R_WELL_STATUS, an illegal
/// transition is refused with the legal set, a change that is not acknowledged is retained for retry unchanged,
/// the effective date is kept apart from the recorded date, and every change carries actor, from, to and reason.
/// </summary>
public interface IOgWellStatus
{
    Task<OgResult<OgWellStatusView>> LoadAsync(string uwi, CancellationToken cancellationToken = default);

    Task<OgResult<OgWellStatusView>> ChangeAsync(
        OgWellStatusChange change,
        CancellationToken cancellationToken = default);

    /// <summary>Re-sends the change that was not acknowledged, unchanged (AC-4).</summary>
    Task<OgResult<OgWellStatusView>> RetryAsync(CancellationToken cancellationToken = default);

    /// <summary>The change waiting for a retry, or null.</summary>
    OgWellStatusChange? Pending { get; }
}

/// <summary>One status change, as the page submits it.</summary>
public sealed record OgWellStatusChange(
    string WellId,
    string? FromCode,
    string ToCode,
    DateTime? EffectiveDate,
    string? ReasonCode,
    string? Note,
    string Actor,
    DateTimeOffset? RecordedAt = null);

/// <summary>
/// The status over WELL_STATUS (history), R_WELL_STATUS (codes) and WELL (current status), read and written
/// through their entity models and a unit of work.
/// </summary>
/// <remarks>
/// The legal set comes from <see cref="WellLifecycleService.AllowedTransitions"/>, the owning project's state
/// machine, not from a second copy here. A change deactivates the previous rows and inserts a new one; nothing is
/// updated in place, so history cannot be rewritten (AC-1).
/// </remarks>
public sealed class OgWellStatus : IOgWellStatus
{
    public const string CodeNoConnection = "well-status-no-connection";
    public const string CodeReadFailed = "well-status-read-failed";
    public const string CodeWriteFailed = "well-status-write-failed";
    public const string CodeNoReferenceData = "well-status-reference-missing";
    public const string CodeUnknownStatus = "well-status-unknown-code";
    public const string CodeIllegalTransition = "well-status-illegal-transition";
    public const string CodeActorRequired = "well-status-actor-required";
    public const string CodeUnchanged = "well-status-unchanged";
    public const string CodeNothingPending = "well-status-nothing-pending";

    private readonly Func<IDataSource?> _source;
    private readonly IFailureReporter _failures;
    private readonly Action<string, string, string, string>? _audit;

    /// <param name="audit">
    /// Optional audit sink (verb, subjectId, outcome, detail). The page passes the application's audit log; a test
    /// passes a recorder, which is how AC-6's actor/from/to/reason is checked without a UI.
    /// </param>
    public OgWellStatus(
        Func<IDataSource?> source,
        IFailureReporter failures,
        Action<string, string, string, string>? audit = null)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _failures = failures ?? throw new ArgumentNullException(nameof(failures));
        _audit = audit;
    }

    public OgWellStatusChange? Pending { get; private set; }

    public async Task<OgResult<OgWellStatusView>> LoadAsync(
        string uwi,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(uwi);

        var source = _source();

        if (source is null)
        {
            return OgResult<OgWellStatusView>.Failed(
                "No connection is open, so the status cannot be read.", CodeNoConnection);
        }

        try
        {
            return OgResult<OgWellStatusView>.Ok(await ReadAsync(source, uwi, cancellationToken).ConfigureAwait(false));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                $"reading the status of {uwi}",
                "the status is not shown",
                FailureSeverity.Error);

            return OgResult<OgWellStatusView>.Failed(
                "The well status could not be read.", reference ?? CodeReadFailed);
        }
    }

    public async Task<OgResult<OgWellStatusView>> ChangeAsync(
        OgWellStatusChange change,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(change);

        if (string.IsNullOrWhiteSpace(change.Actor))
        {
            return OgResult<OgWellStatusView>.Failed(
                "A status change must record who made it.", CodeActorRequired);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgWellStatusView>.Failed(
                "No connection is open, so the status cannot be changed.", CodeNoConnection);
        }

        try
        {
            var view = await ReadAsync(source, change.WellId, cancellationToken).ConfigureAwait(false);
            var options = view.Options;

            if (options.Count == 0)
            {
                return OgResult<OgWellStatusView>.Failed(
                    "Status reference data is not seeded, so a status cannot be chosen.", CodeNoReferenceData);
            }

            // AC-2: only a code the reference table offers.
            var option = options.FirstOrDefault(candidate =>
                string.Equals(candidate.Status, change.ToCode?.Trim(), StringComparison.OrdinalIgnoreCase));

            if (option is null)
            {
                return OgResult<OgWellStatusView>.Failed(
                    $"'{change.ToCode}' is not a status in the reference table. Choose one of: " +
                    $"{string.Join(", ", options.Select(candidate => candidate.Status))}.",
                    CodeUnknownStatus);
            }

            var current = view.Current?.Status ?? "PLANNED";
            var legal = LegalNext(view);

            if (string.Equals(option.Status, current, StringComparison.OrdinalIgnoreCase))
            {
                return OgResult<OgWellStatusView>.Failed(
                    $"The well is already {option.Status}.", CodeUnchanged);
            }

            // AC-3: the owning project's state machine decides, and the refusal lists what it allows.
            if (!legal.Contains(option.Status, StringComparer.OrdinalIgnoreCase))
            {
                return OgResult<OgWellStatusView>.Failed(
                    $"{current} cannot become {option.Status}. " +
                    (legal.Count == 0
                        ? $"{current} has no legal next status."
                        : $"Legal next status(es): {string.Join(", ", legal)}."),
                    CodeIllegalTransition);
            }

            var written = await WriteAsync(source, change, option, current, cancellationToken).ConfigureAwait(false);

            if (!written.Success)
            {
                // AC-4: the original is retained so a retry sends the same effective date, note, reason and actor.
                Pending = change;
                return OgResult<OgWellStatusView>.Failed(
                    written.Message ?? "The status change was not acknowledged; retry sends the same change.",
                    written.Reference ?? CodeWriteFailed);
            }

            Pending = null;
            _audit?.Invoke(
                "well.status.change",
                change.WellId,
                "ok",
                $"from={current} to={option.Status} reason={change.ReasonCode ?? "(none)"} actor={change.Actor} " +
                $"effective={change.EffectiveDate:yyyy-MM-dd}");

            return OgResult<OgWellStatusView>.Ok(
                await ReadAsync(source, change.WellId, cancellationToken).ConfigureAwait(false));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            Pending = change;
            var reference = _failures.ReportHandled(
                ex,
                $"changing the status of {change.WellId}",
                "the status change was not applied; the original is retained for retry",
                FailureSeverity.Error);

            return OgResult<OgWellStatusView>.Failed(
                "The status change could not be applied; retry sends the same change.", reference ?? CodeWriteFailed);
        }
    }

    public Task<OgResult<OgWellStatusView>> RetryAsync(CancellationToken cancellationToken = default)
    {
        return Pending is { } pending
            ? ChangeAsync(pending, cancellationToken)
            : Task.FromResult(OgResult<OgWellStatusView>.Failed(
                "There is no status change waiting to be retried.", CodeNothingPending));
    }

    /// <summary>The legal next codes for the current status, from the owning state machine.</summary>
    public static IReadOnlyList<string> LegalNext(OgWellStatusView view)
    {
        ArgumentNullException.ThrowIfNull(view);

        var current = view.Current?.Status ?? "PLANNED";

        if (!WellLifecycleService.AllowedTransitions.TryGetValue(current, out var next))
        {
            return [];
        }

        // Offer only the legal states the reference table actually provides; the raw set is the fallback when the
        // reference data has none of them (the state is then refused by the code check with the list).
        var offered = view.Options
            .Where(option => next.Contains(option.Status, StringComparer.OrdinalIgnoreCase))
            .Select(option => option.Status)
            .ToList();

        return offered.Count > 0 ? offered : next.ToList();
    }

    private static async Task<OgWellStatusView> ReadAsync(
        IDataSource source,
        string uwi,
        CancellationToken cancellationToken)
    {
        var options = await ReadOptionsAsync(source, cancellationToken).ConfigureAwait(false);
        var history = await ReadHistoryAsync(source, uwi, options, cancellationToken).ConfigureAwait(false);

        var current = history.FirstOrDefault(entry => entry.Active) ?? history.FirstOrDefault();

        var currentStatus = current is null
            ? await ReadWellStatusAsync(source, uwi, cancellationToken).ConfigureAwait(false)
            : new OgWellStatusCurrent(
                current.Status,
                current.StatusType,
                current.Label,
                current.EffectiveDate,
                current.Actor);

        var view = new OgWellStatusView(currentStatus, options, history, []);

        return view with { LegalNext = LegalNext(view) };
    }

    private static async Task<List<OgWellStatusOption>> ReadOptionsAsync(
        IDataSource source,
        CancellationToken cancellationToken)
    {
        var rows = await OgUow.ReadAllAsync<R_WELL_STATUS>(source).ConfigureAwait(false);

        return rows
            .Where(row => !string.IsNullOrWhiteSpace(row.STATUS))
            .OrderBy(row => row.STATUS_TYPE, StringComparer.OrdinalIgnoreCase)
            .ThenBy(row => row.STATUS, StringComparer.OrdinalIgnoreCase)
            .Select(row => new OgWellStatusOption(
                row.STATUS_TYPE ?? string.Empty,
                row.STATUS!,
                Blank(row.LONG_NAME) ?? Blank(row.SHORT_NAME) ?? row.STATUS!,
                !string.Equals(row.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase)))
            .Where(option => option.Active)
            .ToList();
    }

    private static async Task<List<OgWellStatusHistoryEntry>> ReadHistoryAsync(
        IDataSource source,
        string uwi,
        IReadOnlyList<OgWellStatusOption> options,
        CancellationToken cancellationToken)
    {
        var labels = options.ToDictionary(
            option => option.Status,
            option => option.Label,
            StringComparer.OrdinalIgnoreCase);

        var unit = OgUow.For<WELL_STATUS>(source);
        var rows = await OgUow.ReadAsync(unit, [OgUow.Equal("UWI", uwi)], cancellationToken).ConfigureAwait(false);

        var ordered = rows
            .OrderBy(row => row.EFFECTIVE_DATE)
            .ThenBy(row => row.STATUS_DATE)
            .Select(row =>
            {
                var status = row.STATUS ?? string.Empty;

                return new OgWellStatusHistoryEntry(
                    row.STATUS_ID ?? string.Empty,
                    row.STATUS_TYPE ?? string.Empty,
                    status,
                    labels.TryGetValue(status, out var label) ? label : status,
                    FromCode: null,
                    EffectiveDate: row.EFFECTIVE_DATE,
                    RecordedAt: row.STATUS_DATE,
                    Actor: Blank(row.ROW_CREATED_BY),
                    Reason: Blank(row.STATUS_QUALIFIER),
                    Note: Blank(row.REMARK),
                    Active: string.Equals(row.ACTIVE_IND, "Y", StringComparison.OrdinalIgnoreCase));
            })
            .ToList();

        // The "from" of each row is the status before it; the history is then shown newest first.
        for (var index = 0; index < ordered.Count; index++)
        {
            ordered[index] = ordered[index] with { FromCode = index == 0 ? null : ordered[index - 1].Status };
        }

        ordered.Reverse();
        return ordered;
    }

    private static async Task<OgWellStatusCurrent?> ReadWellStatusAsync(
        IDataSource source,
        string uwi,
        CancellationToken cancellationToken)
    {
        var unit = OgUow.For<WELL>(source);
        var rows = await OgUow.ReadAsync(unit, [OgUow.Equal("UWI", uwi)], cancellationToken).ConfigureAwait(false);
        var row = rows.FirstOrDefault();

        return string.IsNullOrWhiteSpace(row?.CURRENT_STATUS)
            ? null
            : new OgWellStatusCurrent(row!.CURRENT_STATUS!, null, null, row.CURRENT_STATUS_DATE, null);
    }

    private async Task<OgResult> WriteAsync(
        IDataSource source,
        OgWellStatusChange change,
        OgWellStatusOption option,
        string fromCode,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var recordedAt = change.RecordedAt ?? DateTimeOffset.UtcNow;
        var effective = change.EffectiveDate ?? now;
        var statusId = Guid.NewGuid().ToString("N");

        var history = OgUow.For<WELL_STATUS>(source);
        var existing = await OgUow.ReadAsync(history, [OgUow.Equal("UWI", change.WellId)], cancellationToken)
            .ConfigureAwait(false);

        var active = existing
            .Where(row => string.Equals(row.ACTIVE_IND, "Y", StringComparison.OrdinalIgnoreCase))
            .ToList();

        var sourceName = Blank(active.FirstOrDefault()?.SOURCE) ?? "DESKTOP";

        // Deactivate first, then insert: a failure leaves no active row rather than two, and the retry re-runs the
        // same change. Nothing updates an existing row's status - history is appended, never rewritten (AC-1).
        foreach (var row in active)
        {
            row.ACTIVE_IND = "N";
            row.END_TIME = recordedAt.UtcDateTime;
            row.ROW_CHANGED_BY = change.Actor;
            row.ROW_CHANGED_DATE = recordedAt.UtcDateTime;

            var updated = history.Update(row);

            if (!OgUow.IsOk(updated))
            {
                return OgResult.Failed(
                    OgUow.Message(updated, "The status change could not be applied."), CodeWriteFailed);
            }
        }

        history.Add(new WELL_STATUS
        {
            UWI = change.WellId,
            SOURCE = sourceName,
            STATUS_ID = statusId,
            STATUS_TYPE = option.StatusType,
            STATUS = option.Status,
            STATUS_DATE = recordedAt.UtcDateTime,
            EFFECTIVE_DATE = effective,
            START_TIME = effective,
            ACTIVE_IND = "Y",
            STATUS_QUALIFIER = Blank(change.ReasonCode),
            REMARK = Blank(change.Note),
            ROW_CREATED_BY = change.Actor,
            ROW_CREATED_DATE = recordedAt.UtcDateTime,
            ROW_CHANGED_BY = change.Actor,
            ROW_CHANGED_DATE = recordedAt.UtcDateTime,
        });

        var committed = await history.Commit().ConfigureAwait(false);

        if (!OgUow.IsOk(committed))
        {
            return OgResult.Failed(
                OgUow.Message(committed, "The status change could not be applied."), CodeWriteFailed);
        }

        // The well row carries the current status for every other screen (the header shows it read-only).
        var wells = OgUow.For<WELL>(source);
        var well = (await OgUow.ReadAsync(wells, [OgUow.Equal("UWI", change.WellId)], cancellationToken)
            .ConfigureAwait(false)).FirstOrDefault();

        if (well is null)
        {
            return OgResult.Failed(
                $"Well '{change.WellId}' is not in this database.", "well-status-unknown-well");
        }

        well.CURRENT_STATUS = option.Status;
        well.CURRENT_STATUS_DATE = effective;
        well.ROW_CHANGED_BY = change.Actor;
        well.ROW_CHANGED_DATE = recordedAt.UtcDateTime;

        var updatedWell = wells.Update(well);

        if (!OgUow.IsOk(updatedWell))
        {
            return OgResult.Failed(
                OgUow.Message(updatedWell, "The status change could not be applied."), CodeWriteFailed);
        }

        var wellCommitted = await wells.Commit().ConfigureAwait(false);

        return OgUow.IsOk(wellCommitted)
            ? OgResult.Ok($"Status changed from {fromCode} to {option.Status}.")
            : OgResult.Failed(
                OgUow.Message(wellCommitted, "The status change could not be applied."), CodeWriteFailed);
    }

    private static string? Blank(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value;
}
