using System.Globalization;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.ProductionAccounting.Constants;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Data.Economics;

/// <summary>One commitment recorded against an AFE revision (ECO-02).</summary>
public sealed record OgAfeCommitment(
    string Id,
    string AfeId,
    string? Reference,
    decimal Amount,
    string? Currency,
    string? Actor,
    DateTime? CommittedAt)
{
    public string Describe() =>
        (Reference ?? "(no reference)") + " · " + MoneyText.Of(Amount) + " " + (Currency ?? string.Empty) +
        " · " + (Actor ?? "(unknown)") + " on " +
        (CommittedAt?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? "an unrecorded date");
}

/// <summary>One revision in an AFE's chain (ECO-02, SG-09-4).</summary>
public sealed record OgAfeRevision(
    string AfeId,
    int RevisionNo,
    string Status,
    decimal Amount,
    decimal Committed,
    bool Effective,
    bool Superseded,
    string? Approver,
    DateTime? ApprovedAt)
{
    public string StatusText =>
        AfeLifecycle.Describe(Status) +
        (Superseded ? " · superseded by a later revision" : string.Empty) +
        (Effective && !Superseded ? " · this is the effective authorisation" : string.Empty);

    public string Describe() =>
        "rev " + RevisionNo.ToString(CultureInfo.InvariantCulture) + " · " + MoneyText.Of(Amount) + " · " +
        StatusText;
}

/// <summary>One authorisation for expenditure (ECO-02), matching the plan's <c>OgAfe</c> plus its chain.</summary>
public sealed record OgAfe(
    string Id,
    string Number,
    string Title,
    string Kind,
    string? EntityId,
    decimal Amount,
    decimal Committed,
    decimal Spent,
    decimal Remaining,
    string Currency,
    string Status,
    string? Author,
    string? Approver,
    DateTime? ApprovedAt,
    DateTime? ClosedAt,
    string? ClosedBy,
    string? CloseNote,
    int RevisionNo,
    string? SupersedesAfeId,
    string? RootAfeId,
    string? RevisionReason,
    IReadOnlyList<OgAfeCommitment> Commitments,
    IReadOnlyList<OgAfeRevision> Revisions,
    string? Description)
{
    public bool IsApproved => AfeLifecycle.IsEffective(Status);

    public bool IsClosed => string.Equals(Status, AfeLifecycle.Closed, StringComparison.OrdinalIgnoreCase);

    /// <summary>SG-09-4: a later revision exists, derived from the chain rather than written onto this row.</summary>
    public bool IsSuperseded => Revisions.Any(revision =>
        string.Equals(revision.Status, "SUPERSEDED", StringComparison.OrdinalIgnoreCase) &&
        string.Equals(revision.AfeId, Id, StringComparison.Ordinal));

    public bool IsEffective => IsApproved && !IsSuperseded;

    /// <summary>AC-4: remaining is the amount less what has been committed, exactly - no rounding, no reserve.</summary>
    public string RemainingText => MoneyText.Of(Remaining) + " " + Currency;

    public string AmountText => MoneyText.Of(Amount) + " " + Currency;

    /// <summary>The variance panel's three numbers, from the same row.</summary>
    public string VarianceText =>
        $"budget {MoneyText.Of(Amount)} · committed {MoneyText.Of(Committed)} · actual {MoneyText.Of(Spent)} · " +
        $"remaining {MoneyText.Of(Remaining)} {Currency}";

    public string StatusText => AfeLifecycle.Describe(Status) +
                                (IsSuperseded ? " (superseded by a later revision)" : string.Empty);

    public string CommitmentsText => Commitments.Count == 0
        ? "no commitment recorded"
        : string.Join(Environment.NewLine, Commitments.Select(commitment => commitment.Describe()));

    public string RevisionsText => Revisions.Count == 0
        ? "no revision"
        : string.Join(Environment.NewLine, Revisions.Select(revision => revision.Describe()));

    public string ApprovalText => IsApproved
        ? $"approved by {Approver ?? "(unknown)"} on " +
          $"{ApprovedAt?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? "an unrecorded date"}"
        : "not approved";

    public string ClosureText => IsClosed
        ? $"closed by {ClosedBy ?? "(unknown)"} on " +
          $"{ClosedAt?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? "an unrecorded date"}" +
          (string.IsNullOrWhiteSpace(CloseNote) ? string.Empty : " · " + CloseNote)
        : "open";

    public string Describe() =>
        $"{Number} rev {RevisionNo.ToString(CultureInfo.InvariantCulture)} · {Title} · {Kind} · {AmountText} · " +
        StatusText;
}

/// <summary>The fields a new AFE is raised from (ECO-02). The number is server-owned and is not an input.</summary>
public sealed record OgAfeEntry(
    string? Title,
    string? Kind,
    string? EntityId,
    decimal? Amount,
    string? Currency,
    string? Description);

/// <summary>The AFE workspace (ECO-02).</summary>
public sealed record OgAfeWorkspace(
    IReadOnlyList<OgAfe> Afes,
    OgAfe? Selected,
    IReadOnlyList<string> Kinds,
    IReadOnlyList<string> Currencies,
    IReadOnlyList<string> States,
    bool HistoryFailed,
    string? HistoryReason)
{
    public string AfesText => Afes.Count == 0
        ? "No AFE is raised yet."
        : string.Join(Environment.NewLine, Afes.Select(afe => afe.Describe()));

    public string HistoryStateText => HistoryFailed
        ? "the AFEs could not be read; new ones can still be raised"
        : $"{Afes.Count.ToString(CultureInfo.InvariantCulture)} AFE(s)";

    public int OpenCount => Afes.Count(afe => !afe.IsClosed);

    public int AwaitingApproval => Afes.Count(afe =>
        string.Equals(afe.Status, AfeLifecycle.Submitted, StringComparison.OrdinalIgnoreCase));

    public int UnapprovedCommitments => Afes.Count(afe => !afe.IsEffective && afe.Committed > 0m);

    public string SummaryText =>
        $"{Afes.Count.ToString(CultureInfo.InvariantCulture)} AFE(s) · " +
        $"{OpenCount.ToString(CultureInfo.InvariantCulture)} open · " +
        $"{AwaitingApproval.ToString(CultureInfo.InvariantCulture)} awaiting approval · " +
        $"{UnapprovedCommitments.ToString(CultureInfo.InvariantCulture)} with commitments against a revision that is not " +
        "effective";

    /// <summary>The transition list the screen offers, so a state the owner does not name cannot be invented.</summary>
    public string LifecycleText =>
        string.Join(" · ", AfeLifecycle.Transition.Select(transition =>
            $"{AfeLifecycle.Describe(transition.From)} → {AfeLifecycle.Describe(transition.To)} ({transition.Action})")) +
        " · " + AfeLifecycle.StartsRevision + " raises a new revision of an approved AFE instead of moving it";

    public string ReferenceText =>
        $"states {string.Join(", ", States)} · currencies {string.Join(", ", Currencies)} · AFE numbering is server-owned";
}

/// <summary>The AFE rules, stated once so the page, the service and the tests agree.</summary>
public static class AfeRules
{
    public const string CodeNoConnection = "afe-no-connection";
    public const string CodeReadFailed = "afe-read-failed";
    public const string CodeWriteFailed = "afe-write-failed";
    public const string CodeActorRequired = "afe-actor-required";
    public const string CodeInputRequired = "afe-input-required";
    public const string CodeInputInvalid = "afe-input-invalid";
    public const string CodeCurrencyMismatch = "afe-currency-mismatch";
    public const string CodeUnknownAfe = "afe-unknown";
    public const string CodeTransitionRefused = "afe-transition-refused";
    public const string CodeAuthorCannotApprove = "afe-author-cannot-approve";
    public const string CodeClosedImmutable = "afe-closed-immutable";
    public const string CodeNotEffective = "afe-not-effective";
    public const string CodeCommitmentExceedsRemaining = "afe-commitment-exceeds-remaining";
    public const string CodeSupplementNeedsApproved = "afe-supplement-needs-approved";
    public const string CodeNothingPending = "afe-nothing-pending";

    public static IReadOnlyList<string> Kinds { get; } = ["DRILL", "FACILITY", "WORKOVER", "OTHER"];

    public static IReadOnlyList<string> Currencies { get; } = ["USD", "EUR", "GBP", "SAR", "AED"];

    /// <summary>The states, from the owner's lifecycle - not a second list.</summary>
    public static IReadOnlyList<string> States => AfeLifecycle.All;

    public static IReadOnlyList<string> Missing(OgAfeEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);

        var missing = new List<string>();

        if (string.IsNullOrWhiteSpace(entry.Title))
        {
            missing.Add("title");
        }

        if (string.IsNullOrWhiteSpace(entry.Kind))
        {
            missing.Add("kind");
        }

        if (string.IsNullOrWhiteSpace(entry.EntityId))
        {
            missing.Add("well or entity");
        }

        if (entry.Amount is null)
        {
            missing.Add("amount");
        }

        if (string.IsNullOrWhiteSpace(entry.Currency))
        {
            missing.Add("currency");
        }

        return missing;
    }

    public static (string Code, string Message)? Validate(OgAfeEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);

        var missing = Missing(entry);

        if (missing.Count > 0)
        {
            return (CodeInputRequired, $"The AFE needs {string.Join(", ", missing)}.");
        }

        if (!Kinds.Contains(entry.Kind!.Trim().ToUpperInvariant(), StringComparer.Ordinal))
        {
            return (CodeInputInvalid,
                $"'{entry.Kind}' is not an AFE kind; use {string.Join(", ", Kinds)}.");
        }

        if (!Currencies.Contains(entry.Currency!.Trim().ToUpperInvariant(), StringComparer.Ordinal))
        {
            return (CodeCurrencyMismatch,
                $"'{entry.Currency}' is not a currency this app raises AFEs in; use {string.Join(", ", Currencies)}.");
        }

        return entry.Amount <= 0m
            ? (CodeInputInvalid, "The AFE amount must be above zero.")
            : null;
    }

    /// <summary>
    /// The next AFE number. The stage requires numbering to be server-owned, so the screen never accepts one: the number is
    /// the year plus one more than the highest sequence already used in that year.
    /// </summary>
    public static string NextNumber(IReadOnlyList<OgAfe> existing, int year)
    {
        ArgumentNullException.ThrowIfNull(existing);

        var prefix = "AFE-" + year.ToString(CultureInfo.InvariantCulture) + "-";

        var highest = existing
            .Select(afe => afe.Number)
            .Where(number => number.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            .Select(number => int.TryParse(number[prefix.Length..], NumberStyles.Integer,
                CultureInfo.InvariantCulture, out var sequence)
                ? sequence
                : 0)
            .DefaultIfEmpty(0)
            .Max();

        return prefix + (highest + 1).ToString("000", CultureInfo.InvariantCulture);
    }
}

/// <summary>ECO-02: AFE management.</summary>
public interface IOgAfe
{
    /// <summary>Raises a new AFE in draft. The number is server-owned.</summary>
    Task<OgResult<OgAfeWorkspace>> RaiseAsync(
        OgAfeEntry entry,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>Moves a draft to submitted.</summary>
    Task<OgResult<OgAfeWorkspace>> SubmitAsync(
        string afeId,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>Approves a submitted AFE; the author may not approve (AC-3).</summary>
    Task<OgResult<OgAfeWorkspace>> ApproveAsync(
        string afeId,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>Raises a supplement as a new revision; the approved version is not written (AC-1, SG-09-4).</summary>
    Task<OgResult<OgAfeWorkspace>> SupplementAsync(
        string afeId,
        decimal amount,
        string reason,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>Records a commitment against an effective revision, refusing one above the remaining amount (AC-2).</summary>
    Task<OgResult<OgAfeWorkspace>> CommitAsync(
        string afeId,
        string reference,
        decimal amount,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>Closes an approved AFE; a closed AFE is read-only (AC-5).</summary>
    Task<OgResult<OgAfeWorkspace>> CloseAsync(
        string afeId,
        string note,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>The AFEs with their chains; an unavailable register is a state.</summary>
    Task<OgResult<OgAfeWorkspace>> LoadAsync(
        string? afeId = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// ECO-02 over the ProductionAccounting module's AFE and AFE_COMMITMENT rows, its lifecycle and its status codes, through
/// units of work.
/// </summary>
/// <remarks>
/// The lifecycle is the owner's <see cref="AfeLifecycle"/> - extracted from the two transitions <c>AfeService</c> actually
/// implements and extended with the ones the feature needs, in one place. A supplement creates a <b>new</b> row whose
/// <c>SUPERSEDES_AFE_ID</c> points at the approved one; the approved row is never written, so its amount, approval and
/// dates stay exactly as they were (AC-1, SG-09-4). Supersession is derived by asking whether any row points at it.
/// </remarks>
public sealed class OgAfeService : IOgAfe
{
    private readonly Func<IDataSource?> _source;
    private readonly IFailureReporter _failures;

    public OgAfeService(Func<IDataSource?> source, IFailureReporter failures)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _failures = failures ?? throw new ArgumentNullException(nameof(failures));
    }

    public async Task<OgResult<OgAfeWorkspace>> RaiseAsync(
        OgAfeEntry entry,
        string actor,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entry);

        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgAfeWorkspace>.Failed(
                "An AFE must record who raised it.", AfeRules.CodeActorRequired);
        }

        if (AfeRules.Validate(entry) is { } refusal)
        {
            return OgResult<OgAfeWorkspace>.Failed(refusal.Message, refusal.Code);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgAfeWorkspace>.Failed(
                "No connection is open, so the AFE cannot be raised.", AfeRules.CodeNoConnection);
        }

        try
        {
            var existing = await ReadAfesAsync(source, cancellationToken).ConfigureAwait(false);
            var now = DateTime.UtcNow;
            var id = Guid.NewGuid().ToString("N");
            var number = AfeRules.NextNumber(existing, now.Year);

            var unit = OgUow.For<AFE>(source);
            unit.PrimaryKey = "AFE_ID";

            unit.Add(new AFE
            {
                AFE_ID = id,
                AFE_NUMBER = number,
                AFE_NAME = entry.Title!.Trim(),
                AFE_KIND = entry.Kind!.Trim().ToUpperInvariant(),
                ENTITY_ID = entry.EntityId!.Trim(),
                PROPERTY_ID = entry.EntityId.Trim(),
                FIELD_ID = entry.EntityId.Trim(),
                ESTIMATED_COST = entry.Amount!.Value,
                ACTUAL_COST = 0m,
                CURRENCY_CODE = entry.Currency!.Trim().ToUpperInvariant(),
                STATUS = AfeLifecycle.Draft,
                DESCRIPTION = entry.Description,
                AUTHOR = actor.Trim(),
                REVISION_NO = 1,
                ROOT_AFE_ID = id,
                ACTIVE_IND = "Y",
                PPDM_GUID = Guid.NewGuid().ToString("N"),
                ROW_CREATED_BY = actor.Trim(),
                ROW_CREATED_DATE = now,
                ROW_CHANGED_BY = actor.Trim(),
                ROW_CHANGED_DATE = now,
            });

            var committed = await unit.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(committed))
            {
                return OgResult<OgAfeWorkspace>.Failed(
                    OgUow.Message(committed, "The AFE was not raised."), AfeRules.CodeWriteFailed);
            }

            var history = await ReadAfesAsync(source, cancellationToken).ConfigureAwait(false);

            return OgResult<OgAfeWorkspace>.Ok(Workspace(
                history,
                history.FirstOrDefault(candidate => candidate.Id == id)));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                "raising an AFE",
                "the AFE was not raised",
                FailureSeverity.Error);

            return OgResult<OgAfeWorkspace>.Failed(
                "The AFE could not be raised.", reference ?? AfeRules.CodeWriteFailed);
        }
    }

    public Task<OgResult<OgAfeWorkspace>> SubmitAsync(
        string afeId,
        string actor,
        CancellationToken cancellationToken = default) =>
        TransitionAsync(afeId, AfeLifecycle.Submitted, "submit", actor, cancellationToken);

    public Task<OgResult<OgAfeWorkspace>> ApproveAsync(
        string afeId,
        string actor,
        CancellationToken cancellationToken = default) =>
        TransitionAsync(afeId, AfeLifecycle.Approved, "approve", actor, cancellationToken);

    public async Task<OgResult<OgAfeWorkspace>> SupplementAsync(
        string afeId,
        decimal amount,
        string reason,
        string actor,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgAfeWorkspace>.Failed(
                "A supplement must record who raised it.", AfeRules.CodeActorRequired);
        }

        if (amount <= 0m)
        {
            return OgResult<OgAfeWorkspace>.Failed(
                "A supplement needs an amount above zero.", AfeRules.CodeInputInvalid);
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            return OgResult<OgAfeWorkspace>.Failed(
                "A supplement needs its reason recorded, or nobody can review what it changes.",
                AfeRules.CodeInputRequired);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgAfeWorkspace>.Failed(
                "No connection is open, so the supplement cannot be raised.", AfeRules.CodeNoConnection);
        }

        try
        {
            var existing = await ReadAfesAsync(source, cancellationToken).ConfigureAwait(false);
            var prior = existing.FirstOrDefault(afe => string.Equals(afe.Id, afeId?.Trim(), StringComparison.OrdinalIgnoreCase));

            if (prior is null)
            {
                return OgResult<OgAfeWorkspace>.Failed(
                    $"AFE '{afeId}' is not in this database.", AfeRules.CodeUnknownAfe);
            }

            // AC-5: a closed AFE is read-only, so it cannot be supplemented either.
            if (prior.IsClosed)
            {
                return OgResult<OgAfeWorkspace>.Failed(
                    $"{prior.Number} rev {prior.RevisionNo} is closed, and a closed AFE is immutable.",
                    AfeRules.CodeClosedImmutable);
            }

            // AC-6: only an approved revision is worth supplementing, and the supplement itself starts unapproved.
            if (!prior.IsApproved)
            {
                return OgResult<OgAfeWorkspace>.Failed(
                    $"{prior.Number} rev {prior.RevisionNo} is {AfeLifecycle.Describe(prior.Status)}; a supplement is " +
                    "raised against an approved revision so there is something to supersede.",
                    AfeRules.CodeSupplementNeedsApproved);
            }

            var now = DateTime.UtcNow;
            var id = Guid.NewGuid().ToString("N");

            var unit = OgUow.For<AFE>(source);
            unit.PrimaryKey = "AFE_ID";

            // AC-1 and SG-09-4: the new revision points at the approved one; the approved row is not touched at all, so
            // its amount, approver and dates stay exactly as they were.
            unit.Add(new AFE
            {
                AFE_ID = id,
                AFE_NUMBER = prior.Number,
                AFE_NAME = prior.Title,
                AFE_KIND = prior.Kind,
                ENTITY_ID = prior.EntityId,
                PROPERTY_ID = prior.EntityId,
                FIELD_ID = prior.EntityId,
                ESTIMATED_COST = amount,
                ACTUAL_COST = 0m,
                CURRENCY_CODE = prior.Currency,
                STATUS = AfeLifecycle.Draft,
                DESCRIPTION = prior.Description,
                AUTHOR = actor.Trim(),
                REVISION_NO = prior.RevisionNo + 1,
                SUPERSEDES_AFE_ID = prior.Id,
                ROOT_AFE_ID = prior.RootAfeId ?? prior.Id,
                REVISION_REASON = reason.Trim(),
                ACTIVE_IND = "Y",
                PPDM_GUID = Guid.NewGuid().ToString("N"),
                ROW_CREATED_BY = actor.Trim(),
                ROW_CREATED_DATE = now,
                ROW_CHANGED_BY = actor.Trim(),
                ROW_CHANGED_DATE = now,
            });

            var committed = await unit.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(committed))
            {
                return OgResult<OgAfeWorkspace>.Failed(
                    OgUow.Message(committed, "The supplement was not raised."), AfeRules.CodeWriteFailed);
            }

            var history = await ReadAfesAsync(source, cancellationToken).ConfigureAwait(false);

            return OgResult<OgAfeWorkspace>.Ok(Workspace(
                history,
                history.FirstOrDefault(candidate => candidate.Id == id)));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                $"raising a supplement on AFE {afeId}",
                "the supplement was not raised",
                FailureSeverity.Error);

            return OgResult<OgAfeWorkspace>.Failed(
                "The supplement could not be raised.", reference ?? AfeRules.CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgAfeWorkspace>> CommitAsync(
        string afeId,
        string reference,
        decimal amount,
        string actor,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgAfeWorkspace>.Failed(
                "A commitment must record who made it.", AfeRules.CodeActorRequired);
        }

        if (amount <= 0m)
        {
            return OgResult<OgAfeWorkspace>.Failed(
                "A commitment needs an amount above zero.", AfeRules.CodeInputInvalid);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgAfeWorkspace>.Failed(
                "No connection is open, so the commitment cannot be recorded.", AfeRules.CodeNoConnection);
        }

        try
        {
            var existing = await ReadAfesAsync(source, cancellationToken).ConfigureAwait(false);
            var afe = existing.FirstOrDefault(candidate =>
                string.Equals(candidate.Id, afeId?.Trim(), StringComparison.OrdinalIgnoreCase));

            if (afe is null)
            {
                return OgResult<OgAfeWorkspace>.Failed(
                    $"AFE '{afeId}' is not in this database.", AfeRules.CodeUnknownAfe);
            }

            // AC-5: a closed AFE takes no new money.
            if (afe.IsClosed)
            {
                return OgResult<OgAfeWorkspace>.Failed(
                    $"{afe.Number} is closed, so it accepts no further commitment.",
                    AfeRules.CodeClosedImmutable);
            }

            // AC-6: a supplement is inert until it is itself approved, so nothing may be committed against it.
            if (!afe.IsApproved)
            {
                return OgResult<OgAfeWorkspace>.Failed(
                    $"{afe.Number} rev {afe.RevisionNo} is {AfeLifecycle.Describe(afe.Status)}; commitments are " +
                    "recorded against an approved revision, so a supplement has to be approved first.",
                    AfeRules.CodeNotEffective);
            }

            // AC-2: the commitment may not take the total past what was approved.
            if (amount > afe.Remaining)
            {
                return OgResult<OgAfeWorkspace>.Failed(
                    $"The commitment of {MoneyText.Of(amount)} {afe.Currency} is more than the " +
                    $"{MoneyText.Of(afe.Remaining)} {afe.Currency} remaining on {afe.Number}: " +
                    $"{MoneyText.Of(afe.Amount)} approved less {MoneyText.Of(afe.Committed)} committed. Raise a " +
                    "supplement and approve it before committing more.",
                    AfeRules.CodeCommitmentExceedsRemaining);
            }

            var now = DateTime.UtcNow;
            var unit = OgUow.For<AFE_COMMITMENT>(source);
            unit.PrimaryKey = "AFE_COMMITMENT_ID";

            unit.Add(new AFE_COMMITMENT
            {
                AFE_COMMITMENT_ID = Guid.NewGuid().ToString("N"),
                AFE_ID = afe.Id,
                REFERENCE = string.IsNullOrWhiteSpace(reference) ? null : reference.Trim(),
                AMOUNT = amount,
                CURRENCY_CODE = afe.Currency,
                COMMITTED_AT = now,
                ACTOR = actor.Trim(),
                ROW_CREATED_BY = actor.Trim(),
                ROW_CREATED_DATE = now,
                ROW_CHANGED_BY = actor.Trim(),
                ROW_CHANGED_DATE = now,
            });

            var committed = await unit.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(committed))
            {
                return OgResult<OgAfeWorkspace>.Failed(
                    OgUow.Message(committed, "The commitment was not recorded."), AfeRules.CodeWriteFailed);
            }

            var history = await ReadAfesAsync(source, cancellationToken).ConfigureAwait(false);

            return OgResult<OgAfeWorkspace>.Ok(Workspace(
                history,
                history.FirstOrDefault(candidate => candidate.Id == afe.Id)));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference_ = _failures.ReportHandled(
                ex,
                $"committing against AFE {afeId}",
                "the commitment was not recorded",
                FailureSeverity.Error);

            return OgResult<OgAfeWorkspace>.Failed(
                "The commitment could not be recorded.", reference_ ?? AfeRules.CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgAfeWorkspace>> CloseAsync(
        string afeId,
        string note,
        string actor,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgAfeWorkspace>.Failed(
                "A closure must record who made it.", AfeRules.CodeActorRequired);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgAfeWorkspace>.Failed(
                "No connection is open, so the AFE cannot be closed.", AfeRules.CodeNoConnection);
        }

        try
        {
            var existing = await ReadAfesAsync(source, cancellationToken).ConfigureAwait(false);
            var afe = existing.FirstOrDefault(candidate =>
                string.Equals(candidate.Id, afeId?.Trim(), StringComparison.OrdinalIgnoreCase));

            if (afe is null)
            {
                return OgResult<OgAfeWorkspace>.Failed(
                    $"AFE '{afeId}' is not in this database.", AfeRules.CodeUnknownAfe);
            }

            // AC-5: the closure is the last write; a closed AFE is refused before anything else happens.
            if (afe.IsClosed)
            {
                return OgResult<OgAfeWorkspace>.Failed(
                    $"{afe.Number} is already closed, and a closed AFE is immutable.",
                    AfeRules.CodeClosedImmutable);
            }

            if (!AfeLifecycle.CanTransition(afe.Status, AfeLifecycle.Closed))
            {
                return OgResult<OgAfeWorkspace>.Failed(
                    $"{afe.Number} is {AfeLifecycle.Describe(afe.Status)}; only an approved AFE can be closed. From " +
                    $"here: {(AfeLifecycle.AvailableFrom(afe.Status).Count == 0 ? "no move is available" : string.Join(", ", AfeLifecycle.AvailableFrom(afe.Status)))}.",
                    AfeRules.CodeTransitionRefused);
            }

            var row = await ReadRowAsync(source, afe.Id, cancellationToken).ConfigureAwait(false);

            if (row is null)
            {
                return OgResult<OgAfeWorkspace>.Failed(
                    $"AFE '{afeId}' is not in this database.", AfeRules.CodeUnknownAfe);
            }

            row.STATUS = AfeLifecycle.Closed;
            row.CLOSED_AT = DateTime.UtcNow;
            row.CLOSED_BY = actor.Trim();
            row.CLOSE_NOTE = string.IsNullOrWhiteSpace(note) ? null : note.Trim();
            row.ROW_CHANGED_BY = actor.Trim();
            row.ROW_CHANGED_DATE = DateTime.UtcNow;

            var closed = await UpdateAsync(source, row).ConfigureAwait(false);

            if (closed is not null)
            {
                return OgResult<OgAfeWorkspace>.Failed(closed, AfeRules.CodeWriteFailed);
            }

            var history = await ReadAfesAsync(source, cancellationToken).ConfigureAwait(false);

            return OgResult<OgAfeWorkspace>.Ok(Workspace(
                history,
                history.FirstOrDefault(candidate => candidate.Id == afe.Id)));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                $"closing AFE {afeId}",
                "the AFE was not closed",
                FailureSeverity.Error);

            return OgResult<OgAfeWorkspace>.Failed(
                "The AFE could not be closed.", reference ?? AfeRules.CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgAfeWorkspace>> LoadAsync(
        string? afeId = null,
        CancellationToken cancellationToken = default)
    {
        var source = _source();

        if (source is null)
        {
            return OgResult<OgAfeWorkspace>.Failed(
                "No connection is open, so the AFEs cannot be read.", AfeRules.CodeNoConnection);
        }

        try
        {
            var history = await ReadAfesAsync(source, cancellationToken).ConfigureAwait(false);

            var selected = string.IsNullOrWhiteSpace(afeId)
                ? history.FirstOrDefault()
                : history.FirstOrDefault(afe =>
                      string.Equals(afe.Id, afeId.Trim(), StringComparison.OrdinalIgnoreCase))
                  ?? history.FirstOrDefault();

            return OgResult<OgAfeWorkspace>.Ok(Workspace(history, selected));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                "reading the AFEs",
                "the AFEs are unavailable; new ones can still be raised",
                FailureSeverity.Degraded);

            return OgResult<OgAfeWorkspace>.Ok(Workspace(
                [],
                null,
                true,
                $"the AFEs could not be read (reference {reference})"));
        }
    }

    // ------------------------------------------------------------------ the transitions

    /// <summary>
    /// One move of a single row: submit, approve. The guard order is the same for both - closed first (AC-5), then the
    /// lifecycle (the owner's transition list), then approval's extra rules (AC-3).
    /// </summary>
    private async Task<OgResult<OgAfeWorkspace>> TransitionAsync(
        string afeId,
        string target,
        string action,
        string actor,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgAfeWorkspace>.Failed(
                $"An {action} must record who did it.", AfeRules.CodeActorRequired);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgAfeWorkspace>.Failed(
                "No connection is open, so the AFE cannot be moved.", AfeRules.CodeNoConnection);
        }

        try
        {
            var existing = await ReadAfesAsync(source, cancellationToken).ConfigureAwait(false);
            var afe = existing.FirstOrDefault(candidate =>
                string.Equals(candidate.Id, afeId?.Trim(), StringComparison.OrdinalIgnoreCase));

            if (afe is null)
            {
                return OgResult<OgAfeWorkspace>.Failed(
                    $"AFE '{afeId}' is not in this database.", AfeRules.CodeUnknownAfe);
            }

            if (afe.IsClosed)
            {
                return OgResult<OgAfeWorkspace>.Failed(
                    $"{afe.Number} is closed, and a closed AFE is immutable.",
                    AfeRules.CodeClosedImmutable);
            }

            // AC-3: the person who raised the AFE cannot approve it.
            if (string.Equals(target, AfeLifecycle.Approved, StringComparison.OrdinalIgnoreCase)
                && !string.IsNullOrWhiteSpace(afe.Author)
                && string.Equals(afe.Author.Trim(), actor.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                return OgResult<OgAfeWorkspace>.Failed(
                    $"{afe.Number} was raised by {afe.Author}, who therefore cannot approve it. A different approver " +
                    "must sign it.",
                    AfeRules.CodeAuthorCannotApprove);
            }

            if (!AfeLifecycle.CanTransition(afe.Status, target))
            {
                var available = AfeLifecycle.AvailableFrom(afe.Status);

                return OgResult<OgAfeWorkspace>.Failed(
                    $"{afe.Number} is {AfeLifecycle.Describe(afe.Status)} and cannot be {action}d. From here: " +
                    (available.Count == 0 ? "no move is available" : string.Join(", ", available)) + ".",
                    AfeRules.CodeTransitionRefused);
            }

            var row = await ReadRowAsync(source, afe.Id, cancellationToken).ConfigureAwait(false);

            if (row is null)
            {
                return OgResult<OgAfeWorkspace>.Failed(
                    $"AFE '{afeId}' is not in this database.", AfeRules.CodeUnknownAfe);
            }

            row.STATUS = target;
            row.ROW_CHANGED_BY = actor.Trim();
            row.ROW_CHANGED_DATE = DateTime.UtcNow;

            if (string.Equals(target, AfeLifecycle.Approved, StringComparison.OrdinalIgnoreCase))
            {
                row.APPROVER = actor.Trim();
                row.APPROVAL_DATE = DateTime.UtcNow;
                row.APPROVED_AMOUNT = row.ESTIMATED_COST ?? 0m;
            }

            var written = await UpdateAsync(source, row).ConfigureAwait(false);

            if (written is not null)
            {
                return OgResult<OgAfeWorkspace>.Failed(written, AfeRules.CodeWriteFailed);
            }

            var history = await ReadAfesAsync(source, cancellationToken).ConfigureAwait(false);

            return OgResult<OgAfeWorkspace>.Ok(Workspace(
                history,
                history.FirstOrDefault(candidate => candidate.Id == afe.Id)));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                $"{action}ing AFE {afeId}",
                "the AFE was not moved",
                FailureSeverity.Error);

            return OgResult<OgAfeWorkspace>.Failed(
                "The AFE could not be moved.", reference ?? AfeRules.CodeWriteFailed);
        }
    }

    // ------------------------------------------------------------------ the reads and writes

    private static async Task<string?> UpdateAsync(IDataSource source, AFE row)
    {
        var unit = OgUow.For<AFE>(source);
        unit.PrimaryKey = "AFE_ID";

        var updated = unit.Update(row);

        if (!OgUow.IsOk(updated))
        {
            return OgUow.Message(updated, "The AFE was not saved.");
        }

        var committed = await unit.Commit().ConfigureAwait(false);

        return OgUow.IsOk(committed) ? null : OgUow.Message(committed, "The AFE was not saved.");
    }

    private static async Task<AFE?> ReadRowAsync(
        IDataSource source,
        string afeId,
        CancellationToken cancellationToken)
    {
        var unit = OgUow.For<AFE>(source);
        unit.PrimaryKey = "AFE_ID";

        var rows = await OgUow.ReadAsync(unit, [OgUow.Equal("AFE_ID", afeId)], cancellationToken)
            .ConfigureAwait(false);

        return rows.FirstOrDefault();
    }

    private static async Task<IReadOnlyList<OgAfe>> ReadAfesAsync(
        IDataSource source,
        CancellationToken cancellationToken)
    {
        var rows = await OgUow.ReadAllAsync<AFE>(source).ConfigureAwait(false);
        var commitments = await OgUow.ReadAllAsync<AFE_COMMITMENT>(source).ConfigureAwait(false);

        cancellationToken.ThrowIfCancellationRequested();

        var byAfe = commitments
            .Where(commitment => !string.IsNullOrWhiteSpace(commitment.AFE_ID))
            .GroupBy(commitment => commitment.AFE_ID!, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.ToList(), StringComparer.OrdinalIgnoreCase);

        var live = rows
            .Where(row => !string.IsNullOrWhiteSpace(row.AFE_ID))
            .Where(row => !string.Equals(row.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase))
            .ToList();

        // The chain: which rows have been superseded is derived from the later rows' own SUPERSEDES_AFE_ID.
        var supersededBy = live
            .Where(row => !string.IsNullOrWhiteSpace(row.SUPERSEDES_AFE_ID))
            .GroupBy(row => row.SUPERSEDES_AFE_ID!, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.First().AFE_ID, StringComparer.OrdinalIgnoreCase);

        var chainOf = live.ToDictionary(
            row => row.AFE_ID,
            row =>
            {
                var root = string.IsNullOrWhiteSpace(row.ROOT_AFE_ID) ? row.AFE_ID : row.ROOT_AFE_ID!;

                return live
                    .Where(candidate =>
                        string.Equals(string.IsNullOrWhiteSpace(candidate.ROOT_AFE_ID) ? candidate.AFE_ID : candidate.ROOT_AFE_ID,
                            root, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(candidate => candidate.REVISION_NO)
                    .ToList();
            },
            StringComparer.OrdinalIgnoreCase);

        return live
            .OrderBy(row => row.AFE_NUMBER, StringComparer.OrdinalIgnoreCase)
            .ThenBy(row => row.REVISION_NO)
            .Select(row => ToAfe(
                row,
                byAfe.TryGetValue(row.AFE_ID, out var mine) ? mine : [],
                supersededBy.ContainsKey(row.AFE_ID),
                chainOf.TryGetValue(row.AFE_ID, out var chain) ? chain : [row],
                byAfe,
                supersededBy))
            .ToList();
    }

    private static OgAfe ToAfe(
        AFE row,
        IReadOnlyList<AFE_COMMITMENT> commitments,
        bool superseded,
        IReadOnlyList<AFE> chain,
        IReadOnlyDictionary<string, List<AFE_COMMITMENT>> byAfe,
        IReadOnlyDictionary<string, string> supersededBy)
    {
        var status = row.STATUS ?? AfeLifecycle.Draft;
        var amount = string.Equals(status, AfeLifecycle.Approved, StringComparison.OrdinalIgnoreCase)
                     || string.Equals(status, AfeLifecycle.Closed, StringComparison.OrdinalIgnoreCase)
            ? row.APPROVED_AMOUNT ?? row.ESTIMATED_COST ?? 0m
            : row.ESTIMATED_COST ?? 0m;

        var committed = commitments.Sum(commitment => commitment.AMOUNT);

        var revisions = chain
            .Select(candidate =>
            {
                var candidateStatus = candidate.STATUS ?? AfeLifecycle.Draft;
                var candidateCommitted = byAfe.TryGetValue(candidate.AFE_ID, out var rows)
                    ? rows.Sum(commitment => commitment.AMOUNT)
                    : 0m;

                return new OgAfeRevision(
                    candidate.AFE_ID,
                    candidate.REVISION_NO,
                    supersededBy.ContainsKey(candidate.AFE_ID) ? "SUPERSEDED" : candidateStatus,
                    candidate.APPROVED_AMOUNT ?? candidate.ESTIMATED_COST ?? 0m,
                    candidateCommitted,
                    AfeLifecycle.IsEffective(candidateStatus) && !supersededBy.ContainsKey(candidate.AFE_ID),
                    supersededBy.ContainsKey(candidate.AFE_ID),
                    candidate.APPROVER,
                    candidate.APPROVAL_DATE);
            })
            .ToList();

        return new OgAfe(
            row.AFE_ID,
            row.AFE_NUMBER ?? string.Empty,
            row.AFE_NAME ?? string.Empty,
            row.AFE_KIND ?? string.Empty,
            row.ENTITY_ID ?? row.PROPERTY_ID,
            amount,
            committed,
            row.ACTUAL_COST ?? 0m,
            amount - committed,
            row.CURRENCY_CODE ?? string.Empty,
            status,
            row.AUTHOR ?? row.ROW_CREATED_BY,
            row.APPROVER,
            row.APPROVAL_DATE,
            row.CLOSED_AT,
            row.CLOSED_BY,
            row.CLOSE_NOTE,
            row.REVISION_NO,
            row.SUPERSEDES_AFE_ID,
            row.ROOT_AFE_ID ?? row.AFE_ID,
            row.REVISION_REASON,
            commitments
                .OrderBy(commitment => commitment.COMMITTED_AT)
                .Select(commitment => new OgAfeCommitment(
                    commitment.AFE_COMMITMENT_ID,
                    commitment.AFE_ID,
                    commitment.REFERENCE,
                    commitment.AMOUNT,
                    commitment.CURRENCY_CODE,
                    commitment.ACTOR,
                    commitment.COMMITTED_AT))
                .ToList(),
            revisions,
            row.DESCRIPTION);
    }

    private static OgAfeWorkspace Workspace(
        IReadOnlyList<OgAfe> afes,
        OgAfe? selected,
        bool historyFailed = false,
        string? historyReason = null) =>
        new(
            afes,
            selected,
            AfeRules.Kinds,
            AfeRules.Currencies,
            AfeRules.States,
            historyFailed,
            historyReason);
}
