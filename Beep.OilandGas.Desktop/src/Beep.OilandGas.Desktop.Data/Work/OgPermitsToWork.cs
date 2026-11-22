using System.Globalization;
using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.PPDM39.Models;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Data.Work;

/// <summary>One contractor in the register with the licences that qualify it (WRK-04).</summary>
public sealed record OgContractor(
    string Id,
    string Name,
    string Kind,
    IReadOnlyList<OgCompetency> Competencies)
{
    public bool IsCurrent => Competencies.Count > 0 && Competencies.All(competency => competency.IsCurrent);

    public string StateText => Competencies.Count == 0
        ? "no competency recorded"
        : Competencies.Count(competency => competency.IsCurrent).ToString(CultureInfo.InvariantCulture) + " of " +
          Competencies.Count.ToString(CultureInfo.InvariantCulture) + " current";

    public string Describe() =>
        Name + " · " + Kind + " · " + StateText +
        (Competencies.Count == 0
            ? string.Empty
            : " · " + string.Join(", ", Competencies.Select(competency => competency.Describe())));
}

/// <summary>One competency record: a licence, its latest status and its expiry (WRK-04, AC-2).</summary>
public sealed record OgCompetency(
    string LicenceId,
    string Type,
    string Status,
    DateTime? Expiry,
    bool IsCurrent,
    string EvidenceRef,
    string Why)
{
    /// <summary>The expiry is named, because "not current" alone does not say when it stopped being current (AC-2).</summary>
    public string Describe() =>
        Type + " " + LicenceId + " · " +
        (IsCurrent ? "current" : "not current: " + (string.IsNullOrWhiteSpace(Why) ? "no reason recorded" : Why)) +
        (Expiry is { } expiry ? " · expires " + expiry.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) : " · no expiry recorded");
}

/// <summary>
/// One readiness item (WRK-04's <c>OgReadinessItem</c>), each with the evidence that satisfies it (AC-5).
/// </summary>
/// <param name="Kind">The owner's own prerequisite kind where it records one, otherwise the item this build derives.</param>
/// <param name="Why">Why it is not satisfied, when it is not - so the checklist reads as reasons, not ticks.</param>
public sealed record OgReadinessItem(
    string Kind,
    bool Required,
    bool Satisfied,
    string? EvidenceRef,
    string Why)
{
    public string Describe() =>
        Kind + " · " + (Satisfied ? "satisfied" : "not satisfied: " + Why) +
        (string.IsNullOrWhiteSpace(EvidenceRef) ? " · no evidence reference" : " · evidence " + EvidenceRef);
}

/// <summary>
/// One execution permit with its window, its requester, its issuer and its readiness (WRK-04's <c>OgWorkPermit</c>).
/// </summary>
public sealed record OgWorkPermit(
    string Id,
    string WorkOrderId,
    string State,
    string Scope,
    string? Hazards,
    string? IsolationRef,
    string? PersonnelRef,
    string? MaterialRef,
    string? Requester,
    string? Issuer,
    DateTime? ValidFromUtc,
    DateTime? ValidToUtc,
    string? Conditions,
    IReadOnlyList<OgReadinessItem> Readiness,
    IReadOnlyList<string> Blocks,
    DateTime AsOf)
{
    public bool IsIssued => string.Equals(State, PermitToWorkRules.Issued, StringComparison.OrdinalIgnoreCase);

    /// <summary>AC-2: a permit outside its window is not usable, whenever it was issued.</summary>
    public bool IsWithinWindow => ValidFromUtc is { } from && ValidToUtc is { } to &&
                                  AsOf >= from && AsOf <= to;

    /// <summary>AC-1: execution may start only when every required item is satisfied and the permit covers this moment.</summary>
    public bool StartAllowed => IsIssued && IsWithinWindow && Blocks.Count == 0 &&
                                Readiness.Where(item => item.Required).All(item => item.Satisfied);

    public string WindowText => ValidFromUtc is { } from && ValidToUtc is { } to
        ? from.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture) + " to " +
          to.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture) + " UTC"
        : "no validity window";

    public string StateText => (string.IsNullOrWhiteSpace(State) ? "no recorded state" : State.ToLowerInvariant()) +
                               (IsIssued && !IsWithinWindow ? " (outside its window)" : string.Empty);

    public string ReadinessText => Readiness.Count == 0
        ? "No readiness item is recorded, so execution cannot be shown to be ready."
        : string.Join(Environment.NewLine, Readiness.Select(item => item.Describe()));

    public string BlocksText => Blocks.Count == 0
        ? "Nothing blocks the start."
        : string.Join(Environment.NewLine, Blocks.Select(block => "✗ " + block));

    public string StartText => StartAllowed
        ? "Execution may start: the permit is issued, inside its window and every required item is satisfied."
        : "Execution may not start: " + (Blocks.Count == 0
            ? (IsWithinWindow ? "the permit is not issued" : "the permit does not cover this moment")
            : string.Join(" ", Blocks));

    public string Describe() =>
        WorkOrderId + " · " + StateText + " · " + WindowText +
        (string.IsNullOrWhiteSpace(Issuer) ? " · no issuer" : " · issued by " + Issuer) +
        (string.IsNullOrWhiteSpace(Requester) ? " · no requester recorded" : " · requested by " + Requester);
}

/// <summary>
/// One linked acknowledgement that was lost (WRK-04's "lost acknowledgement" state): the permit is retained as the original
/// and no second permit is issued for the same work order.
/// </summary>
public sealed record OgLostAcknowledgement(string PermitId, string WorkOrderId, string Sentence);

/// <summary>The permit-to-work workspace (WRK-04).</summary>
public sealed record OgPermitToWorkWorkspace(
    string? WorkOrderFilter,
    IReadOnlyList<OgContractor> Contractors,
    IReadOnlyList<OgWorkPermit> Permits,
    OgWorkPermit? Selected,
    IReadOnlyList<string> States,
    DateTime AsOf,
    DateTime ReadAt)
{
    public int Ready => Permits.Count(permit => permit.StartAllowed);

    public int Blocked => Permits.Count(permit => !permit.StartAllowed);

    public string CountsText =>
        Contractors.Count.ToString(CultureInfo.InvariantCulture) + " contractor(s) · " +
        Permits.Count.ToString(CultureInfo.InvariantCulture) + " permit(s) · " +
        Ready.ToString(CultureInfo.InvariantCulture) + " ready to start · " +
        Blocked.ToString(CultureInfo.InvariantCulture) + " blocked";

    public string ContractorText => Contractors.Count == 0
        ? "No contractor is recorded with a licence."
        : string.Join(Environment.NewLine, Contractors.Select(contractor => contractor.Describe()));

    public string PermitsText => Permits.Count == 0
        ? "No execution permit is recorded."
        : string.Join(Environment.NewLine, Permits.Select(permit => permit.Describe()));

    public string ReadText =>
        "read at " + ReadAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
}

/// <summary>The permit-to-work rules WRK-04 states once.</summary>
public static class PermitToWorkRules
{
    public const string CodeNoConnection = "permit-to-work-no-connection";
    public const string CodeReadFailed = "permit-to-work-read-failed";
    public const string CodeWriteFailed = "permit-to-work-write-failed";
    public const string CodeActorRequired = "permit-to-work-actor-required";
    public const string CodeWorkOrderRequired = "permit-to-work-work-order-required";
    public const string CodeScopeRequired = "permit-to-work-scope-required";
    public const string CodeWindowInvalid = "permit-to-work-window-invalid";
    public const string CodeUnknownPermit = "permit-to-work-unknown-permit";
    public const string CodeSelfIssue = "permit-to-work-self-issue";
    public const string CodeAlreadyIssued = "permit-to-work-already-issued";
    public const string CodeReadinessIncomplete = "permit-to-work-readiness-incomplete";
    public const string CodeStartBlocked = "permit-to-work-start-blocked";

    public const string Requested = "REQUESTED";
    public const string Issued = "ISSUED";
    public const string StartState = WorkOrderState.InProgress;

    /// <summary>The states that refuse execution, from the same vocabulary WRK-02 uses.</summary>
    public static IReadOnlyList<string> RefusingPermitStates { get; } =
        ["REJECTED", "WITHDRAWN", "CANCELLED", "EXPIRED", "PENDING", "DRAFT", "REVOKED"];

    /// <summary>
    /// The compliance statuses that do not qualify a person or a licence. Named rather than enumerated positively, because the
    /// owner's licence status vocabulary is written by its own licensing workflow.
    /// </summary>
    public static IReadOnlyList<string> RefusingLicenceStates { get; } =
        ["EXPIRED", "SUSPENDED", "REVOKED", "DENIED", "PENDING", "CANCELLED", "WITHDRAWN", "LAPSED"];

    /// <summary>The inspection results that satisfy a prerequisite kind.</summary>
    public static IReadOnlyList<string> SatisfyingResults { get; } = ["PASS", "SATISFIED", "ACCEPTED", "COMPLETE", "COMPLETED"];

    public static IReadOnlyList<string> States { get; } = [Requested, Issued];

    /// <summary>
    /// AC-4: a validity window needs both ends and an end after its start. Nothing is defaulted - a permit with no window
    /// cannot be shown to cover any moment.
    /// </summary>
    public static (string Code, string Message)? ValidateWindow(DateTime? from, DateTime? to)
    {
        if (from is null || to is null)
        {
            return (CodeWindowInvalid,
                "A permit's validity is a window: both its start and its end must be recorded.");
        }

        return to.Value <= from.Value
            ? (CodeWindowInvalid,
                "A permit's end (" + to.Value.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture) +
                " UTC) must be after its start (" + from.Value.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture) +
                " UTC).")
            : null;
    }

    /// <summary>AC-4: the window a requested permit is asked for, measured against now.</summary>
    public static bool Covers(DateTime? from, DateTime? to, DateTime asOf) =>
        from is { } start && to is { } end && asOf >= start && asOf <= end;

    /// <summary>AC-3: a licence is current when it is active, its latest status does not refuse it and its expiry has not passed.</summary>
    public static (bool IsCurrent, string Why) LicenceState(
        string? activeInd,
        string? status,
        DateTime? expiry,
        DateTime asOf)
    {
        if (string.Equals(activeInd?.Trim(), "N", StringComparison.OrdinalIgnoreCase))
        {
            return (false, "the licence is not active");
        }

        if (string.IsNullOrWhiteSpace(status))
        {
            return (false, "no licence status is recorded");
        }

        if (RefusingLicenceStates.Contains(status.Trim(), StringComparer.OrdinalIgnoreCase))
        {
            return (false, "the licence is " + status.Trim().ToLowerInvariant());
        }

        if (expiry is null)
        {
            return (false, "no expiry is recorded, so it cannot be shown current");
        }

        return expiry.Value.Date < asOf.Date
            ? (false, "expired on " + expiry.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture))
            : (true, string.Empty);
    }

    /// <summary>AC-3: the issuer cannot be the requester.</summary>
    public static (string Code, string Message)? CheckIssuer(string? requester, string? actor)
    {
        if (string.IsNullOrWhiteSpace(requester) || string.IsNullOrWhiteSpace(actor))
        {
            return null;
        }

        return string.Equals(requester.Trim(), actor.Trim(), StringComparison.OrdinalIgnoreCase)
            ? (CodeSelfIssue, requester + " requested this permit and therefore cannot issue it.")
            : null;
    }

    /// <summary>The placeholder a required reference column records when the request carries none.</summary>
    public const string NotRecorded = "NOT-RECORDED";

    /// <summary>A required reference column's value: the recorded reference, or an explicit placeholder.</summary>
    public static string OrPlaceholder(string? value) =>
        string.IsNullOrWhiteSpace(value) ? NotRecorded : value.Trim();

    /// <summary>The permit id for a request intent, so a retry lands on the permit it already wrote (AC-6).</summary>
    public static string IdFor(string intent) => "PTW-" + intent.Trim();

    /// <summary>
    /// AC-1: execution start is refused unless every required readiness item is satisfied and the permit covers this moment;
    /// the reasons are collected so the page and the command cannot disagree.
    /// </summary>
    public static IReadOnlyList<string> StartBlocks(OgWorkPermit permit)
    {
        ArgumentNullException.ThrowIfNull(permit);

        var blocks = new List<string>();

        if (!permit.IsIssued)
        {
            blocks.Add("The permit is " + (string.IsNullOrWhiteSpace(permit.State)
                ? "not recorded in a state that authorises execution"
                : permit.State.ToLowerInvariant()) + ".");
        }

        if (!permit.IsWithinWindow)
        {
            blocks.Add("The permit's window is " + permit.WindowText + ", which does not cover this moment.");
        }

        foreach (var item in permit.Readiness.Where(item => item.Required && !item.Satisfied))
        {
            blocks.Add(item.Kind + " is not satisfied: " + item.Why);
        }

        return blocks;
    }
}

/// <summary>WRK-04: the contractor register, the permit request, its issue and the execution-start gate.</summary>
public interface IOgPermitsToWork
{
    Task<OgResult<OgPermitToWorkWorkspace>> LoadAsync(
        string? workOrderId,
        DateTime asOf,
        CancellationToken cancellationToken = default);

    /// <summary>Requests a permit for a work order, recording the requester, the window and the scope (AC-4).</summary>
    Task<OgResult<OgPermitToWorkWorkspace>> RequestAsync(
        string workOrderId,
        string scope,
        string? hazards,
        string? isolationRef,
        string? personnelRef,
        string? materialRef,
        DateTime fromUtc,
        DateTime toUtc,
        string intentId,
        string actor,
        DateTime asOf,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Issues a requested permit. Refused for the requester (AC-3), refused when the readiness is incomplete (AC-1) and refused
    /// when an issued permit for the same work order already covers this moment (AC-6).
    /// </summary>
    Task<OgResult<OgPermitToWorkWorkspace>> IssueAsync(
        string permitId,
        string conditions,
        string actor,
        DateTime asOf,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// WRK-04 over the owner's <c>WORK_ORDER_EXECUTION_PERMIT</c> (with the request columns this feature added),
/// <c>WORK_ORDER</c>, <c>WORK_ORDER_COMPONENT</c>, <c>WORK_ORDER_INSPECTION</c>, <c>BUSINESS_ASSOCIATE</c>,
/// <c>BA_LICENSE</c> and <c>BA_LICENSE_STATUS</c>.
/// </summary>
/// <remarks>
/// <para>
/// The feature's open question - *the readiness item list is not enumerated in the repository* - is answered by the owner's own
/// columns: a work order's <c>WORK_ORDER_INSPECTION</c> rows <b>are</b> its execution prerequisites, each carrying
/// <c>PERMIT_PREREQUISITE_KIND</c>, a result and the acceptance revisions that evidence it. Readiness is therefore that list plus
/// the item the permit itself supplies - a valid window - and the personnel check derived from the assigned people's licences.
/// </para>
/// <para>
/// Nothing is invented: a condition with no result is not satisfied, a licence with no expiry is not current, and a permit with
/// no window covers nothing.
/// </para>
/// </remarks>
public sealed class OgPermitsToWorkService : IOgPermitsToWork
{
    private readonly Func<IDataSource?> _source;
    private readonly IFailureReporter _failures;

    public OgPermitsToWorkService(Func<IDataSource?> source, IFailureReporter failures)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _failures = failures ?? throw new ArgumentNullException(nameof(failures));
    }

    public async Task<OgResult<OgPermitToWorkWorkspace>> LoadAsync(
        string? workOrderId,
        DateTime asOf,
        CancellationToken cancellationToken = default)
    {
        var source = _source();

        if (source is null)
        {
            return OgResult<OgPermitToWorkWorkspace>.Failed(
                "No connection is open, so the permit-to-work register cannot be read.",
                PermitToWorkRules.CodeNoConnection);
        }

        try
        {
            return await ReadAsync(source, workOrderId, asOf, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, "reading the permit-to-work register", "the register is not shown", FailureSeverity.Error);

            return OgResult<OgPermitToWorkWorkspace>.Failed(
                "The permit-to-work register could not be read" +
                (string.IsNullOrWhiteSpace(reference) ? "." : " (reference " + reference + ")."),
                reference ?? PermitToWorkRules.CodeReadFailed);
        }
    }

    public async Task<OgResult<OgPermitToWorkWorkspace>> RequestAsync(
        string workOrderId,
        string scope,
        string? hazards,
        string? isolationRef,
        string? personnelRef,
        string? materialRef,
        DateTime fromUtc,
        DateTime toUtc,
        string intentId,
        string actor,
        DateTime asOf,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgPermitToWorkWorkspace>.Failed(
                "A permit request must record who made it.", PermitToWorkRules.CodeActorRequired);
        }

        if (string.IsNullOrWhiteSpace(workOrderId))
        {
            return OgResult<OgPermitToWorkWorkspace>.Failed(
                "A permit is requested for one work order; none is named.",
                PermitToWorkRules.CodeWorkOrderRequired);
        }

        if (string.IsNullOrWhiteSpace(scope))
        {
            return OgResult<OgPermitToWorkWorkspace>.Failed(
                "A permit request records the scope it authorises.", PermitToWorkRules.CodeScopeRequired);
        }

        if (PermitToWorkRules.ValidateWindow(fromUtc, toUtc) is { } refusal)
        {
            return OgResult<OgPermitToWorkWorkspace>.Failed(refusal.Message, refusal.Code);
        }

        if (string.IsNullOrWhiteSpace(intentId))
        {
            return OgResult<OgPermitToWorkWorkspace>.Failed(
                "A permit request needs an intent key, so a retry finds the permit it already wrote.",
                PermitToWorkRules.CodeWindowInvalid);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgPermitToWorkWorkspace>.Failed(
                "No connection is open, so the permit cannot be requested.",
                PermitToWorkRules.CodeNoConnection);
        }

        try
        {
            var permits = await OgUow.ReadAllAsync<WORK_ORDER_EXECUTION_PERMIT>(source).ConfigureAwait(false);

            var id = PermitToWorkRules.IdFor(intentId);

            // AC-6: the intent key is what a retry lands on - the original permit is retained, never duplicated.
            var existing = permits.FirstOrDefault(row =>
                string.Equals(row.PERMIT_ID, id, StringComparison.OrdinalIgnoreCase));

            if (existing is not null)
            {
                if (!string.Equals(existing.WORK_ORDER_ID?.Trim(), workOrderId.Trim(), StringComparison.OrdinalIgnoreCase) ||
                    !string.Equals((existing.SCOPE_DESCRIPTION ?? string.Empty).Trim(), scope.Trim(), StringComparison.Ordinal))
                {
                    return OgResult<OgPermitToWorkWorkspace>.Failed(
                        "Intent '" + intentId + "' was already used to request " + id + " for a different order or scope. " +
                        "Use a new intent key for a different permit.",
                        PermitToWorkRules.CodeAlreadyIssued);
                }
            }
            else
            {
                var now = DateTime.UtcNow;

                var unit = OgUow.For<WORK_ORDER_EXECUTION_PERMIT>(source);
                unit.PrimaryKey = nameof(WORK_ORDER_EXECUTION_PERMIT.PERMIT_ID);

                unit.Add(new WORK_ORDER_EXECUTION_PERMIT
                {
                    PERMIT_ID = id,
                    TENANT_ID = "APP",
                    BUSINESS_ASSOCIATE_ID = "APP",
                    FIELD_ID = await FieldOfAsync(source, workOrderId.Trim(), cancellationToken).ConfigureAwait(false)
                               ?? "UNSPECIFIED",
                    WORK_ORDER_ID = workOrderId.Trim(),
                    PROCESS_INSTANCE_ID = "NO-PROCESS",
                    ISSUE_INTENT_ID = intentId.Trim(),
                    // The owner marks these required and a requested permit has neither yet: the placeholder says so
                    // rather than leaving the column blank (recorded as a limit).
                    ISSUED_BY = "NOT-ISSUED",
                    INSPECTION_REVISION = "NO-INSPECTION",
                    STATUS = PermitToWorkRules.Requested,
                    PERMIT_TYPE = "EXECUTION",
                    JURISDICTION = "UNSPECIFIED",
                    SCOPE_DESCRIPTION = scope.Trim(),
                    // The owner marks these reference columns required, and a request may not have one yet: the
                    // placeholder records that explicitly instead of leaving a blank that would fail the commit.
                    ISOLATION_REFERENCE = PermitToWorkRules.OrPlaceholder(isolationRef),
                    RISK_REFERENCE = PermitToWorkRules.OrPlaceholder(hazards),
                    PERSONNEL_REFERENCE = PermitToWorkRules.OrPlaceholder(personnelRef),
                    MATERIAL_REFERENCE = PermitToWorkRules.OrPlaceholder(materialRef),
                    VALID_FROM_UTC = fromUtc,
                    VALID_TO_UTC = toUtc,
                    ISSUE_DATA_JSON = "{}",
                    REQUESTED_BY = actor.Trim(),
                    REQUESTED_AT_UTC = now,
                    REQUEST_INTENT_ID = intentId.Trim(),
                    ACTIVE_IND = "Y",
                    ROW_CREATED_BY = actor.Trim(),
                    ROW_CREATED_DATE = now,
                    ROW_CHANGED_BY = actor.Trim(),
                    ROW_CHANGED_DATE = now,
                });

                var committed = await unit.Commit().ConfigureAwait(false);

                if (!OgUow.IsOk(committed))
                {
                    return OgResult<OgPermitToWorkWorkspace>.Failed(
                        OgUow.Message(committed, "The permit request was not saved."),
                        PermitToWorkRules.CodeWriteFailed);
                }
            }

            return await ReadAsync(source, workOrderId, asOf, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, "requesting a permit for " + workOrderId, "the permit request was not saved",
                FailureSeverity.Error);

            return OgResult<OgPermitToWorkWorkspace>.Failed(
                "The permit could not be requested" +
                (string.IsNullOrWhiteSpace(reference) ? "." : " (reference " + reference + ")."),
                reference ?? PermitToWorkRules.CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgPermitToWorkWorkspace>> IssueAsync(
        string permitId,
        string conditions,
        string actor,
        DateTime asOf,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgPermitToWorkWorkspace>.Failed(
                "An issue must record who made it.", PermitToWorkRules.CodeActorRequired);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgPermitToWorkWorkspace>.Failed(
                "No connection is open, so the permit cannot be issued.",
                PermitToWorkRules.CodeNoConnection);
        }

        try
        {
            var read = await ReadAsync(source, null, asOf, cancellationToken).ConfigureAwait(false);

            if (!read.Success || read.Value is null)
            {
                return read;
            }

            var permit = read.Value.Permits.FirstOrDefault(candidate =>
                string.Equals(candidate.Id, permitId, StringComparison.OrdinalIgnoreCase));

            if (permit is null)
            {
                return OgResult<OgPermitToWorkWorkspace>.Failed(
                    "Permit '" + permitId + "' is not recorded.", PermitToWorkRules.CodeUnknownPermit);
            }

            // AC-3: the requester cannot issue the permit they asked for.
            if (PermitToWorkRules.CheckIssuer(permit.Requester, actor) is { } self)
            {
                return OgResult<OgPermitToWorkWorkspace>.Failed(self.Message, self.Code);
            }

            // AC-6: a second issued permit for the same order would authorise the same work twice.
            var other = read.Value.Permits.FirstOrDefault(candidate =>
                !string.Equals(candidate.Id, permit.Id, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(candidate.WorkOrderId, permit.WorkOrderId, StringComparison.OrdinalIgnoreCase) &&
                candidate.IsIssued && candidate.IsWithinWindow);

            if (other is not null)
            {
                return OgResult<OgPermitToWorkWorkspace>.Failed(
                    permit.WorkOrderId + " already has an issued permit (" + other.Id + ") that covers this moment, so no " +
                    "second permit is issued for it. The original is retained.",
                    PermitToWorkRules.CodeAlreadyIssued);
            }

            // AC-4: the window it was requested with must still be a window, and it must not have closed.
            if (PermitToWorkRules.ValidateWindow(permit.ValidFromUtc, permit.ValidToUtc) is { } window)
            {
                return OgResult<OgPermitToWorkWorkspace>.Failed(window.Message, window.Code);
            }

            if (!permit.IsWithinWindow)
            {
                return OgResult<OgPermitToWorkWorkspace>.Failed(
                    "The requested window (" + permit.WindowText + ") has closed, so the permit cannot be issued for it. " +
                    "Request a new window.",
                    PermitToWorkRules.CodeWindowInvalid);
            }

            // AC-1: every required readiness item must be satisfied before a permit is issued.
            var incomplete = permit.Readiness
                .Where(item => item.Required && !item.Satisfied &&
                               !string.Equals(item.Kind, PermitToWorkRules.Issued, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (incomplete.Count > 0)
            {
                return OgResult<OgPermitToWorkWorkspace>.Failed(
                    "The permit cannot be issued while readiness is incomplete: " +
                    string.Join(" ", incomplete.Select(item => item.Kind + " is not satisfied: " + item.Why)),
                    PermitToWorkRules.CodeReadinessIncomplete);
            }

            var unit = OgUow.For<WORK_ORDER_EXECUTION_PERMIT>(source);
            unit.PrimaryKey = nameof(WORK_ORDER_EXECUTION_PERMIT.PERMIT_ID);

            var rows = await OgUow.ReadAsync(
                unit,
                [OgUow.Equal(nameof(WORK_ORDER_EXECUTION_PERMIT.PERMIT_ID), permit.Id)],
                cancellationToken).ConfigureAwait(false);

            var row = rows.FirstOrDefault();

            if (row is null)
            {
                return OgResult<OgPermitToWorkWorkspace>.Failed(
                    "Permit '" + permitId + "' is not recorded.", PermitToWorkRules.CodeUnknownPermit);
            }

            var now = DateTime.UtcNow;

            row.STATUS = PermitToWorkRules.Issued;
            row.ISSUED_BY = actor.Trim();
            row.DECIDED_BY = actor.Trim();
            row.DECIDED_AT_UTC = now;
            row.CONDITIONS = string.IsNullOrWhiteSpace(conditions) ? "no conditions" : conditions.Trim();
            row.DECISION_DATA_JSON = "{}";
            row.ROW_CHANGED_BY = actor.Trim();
            row.ROW_CHANGED_DATE = now;

            var updated = unit.Update(row);

            if (!OgUow.IsOk(updated))
            {
                return OgResult<OgPermitToWorkWorkspace>.Failed(
                    OgUow.Message(updated, "The permit was not issued."), PermitToWorkRules.CodeWriteFailed);
            }

            var committed = await unit.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(committed))
            {
                return OgResult<OgPermitToWorkWorkspace>.Failed(
                    OgUow.Message(committed, "The permit was not issued."), PermitToWorkRules.CodeWriteFailed);
            }

            return await ReadAsync(source, permit.WorkOrderId, asOf, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, "issuing permit " + permitId, "the permit was not issued", FailureSeverity.Error);

            return OgResult<OgPermitToWorkWorkspace>.Failed(
                "The permit could not be issued" +
                (string.IsNullOrWhiteSpace(reference) ? "." : " (reference " + reference + ")."),
                reference ?? PermitToWorkRules.CodeWriteFailed);
        }
    }

    // ------------------------------------------------------------------ the read model

    private async Task<OgResult<OgPermitToWorkWorkspace>> ReadAsync(
        IDataSource source,
        string? workOrderId,
        DateTime asOf,
        CancellationToken cancellationToken)
    {
        var permits = await OgUow.ReadAllAsync<WORK_ORDER_EXECUTION_PERMIT>(source).ConfigureAwait(false);
        var orders = await OgUow.ReadAllAsync<WORK_ORDER>(source).ConfigureAwait(false);
        var components = await OgUow.ReadAllAsync<WORK_ORDER_COMPONENT>(source).ConfigureAwait(false);
        var inspections = await OgUow.ReadAllAsync<WORK_ORDER_INSPECTION>(source).ConfigureAwait(false);
        var associates = await OgUow.ReadAllAsync<BUSINESS_ASSOCIATE>(source).ConfigureAwait(false);
        var licences = await OgUow.ReadAllAsync<BA_LICENSE>(source).ConfigureAwait(false);
        var licenceStatuses = await OgUow.ReadAllAsync<BA_LICENSE_STATUS>(source).ConfigureAwait(false);

        cancellationToken.ThrowIfCancellationRequested();

        var contractors = BuildContractors(associates, licences, licenceStatuses, asOf);

        var mine = permits
            .Where(row => !string.Equals(row.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase))
            .Where(row => string.IsNullOrWhiteSpace(workOrderId) ||
                          string.Equals(row.WORK_ORDER_ID?.Trim(), workOrderId.Trim(), StringComparison.OrdinalIgnoreCase))
            .ToList();

        var rows = new List<OgWorkPermit>();

        foreach (var row in mine)
        {
            var orderId = row.WORK_ORDER_ID ?? string.Empty;

            var people = components
                .Where(component => !string.Equals(component.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase))
                .Where(component => string.Equals(component.WORK_ORDER_ID, orderId, StringComparison.OrdinalIgnoreCase))
                .Select(component => component.EMPLOYEE_BA_ID ?? component.BUSINESS_ASSOCIATE_ID)
                .Where(person => !string.IsNullOrWhiteSpace(person))
                .Select(person => person!.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var readiness = new List<OgReadinessItem>();

            // The owner's own prerequisite rows: each inspection condition is a readiness item.
            foreach (var condition in inspections.Where(condition =>
                         string.Equals(condition.WORK_ORDER_ID, orderId, StringComparison.OrdinalIgnoreCase)))
            {
                var kind = string.IsNullOrWhiteSpace(condition.PERMIT_PREREQUISITE_KIND)
                    ? condition.COND_TYPE
                    : condition.PERMIT_PREREQUISITE_KIND;

                var result = (condition.RESULT_STATUS ?? string.Empty).Trim().ToUpperInvariant();

                var evidence = FirstEvidence(condition);

                var satisfied = PermitToWorkRules.SatisfyingResults.Contains(result, StringComparer.OrdinalIgnoreCase) &&
                                !string.IsNullOrWhiteSpace(evidence);

                readiness.Add(new OgReadinessItem(
                    kind,
                    Required: true,
                    satisfied,
                    evidence,
                    satisfied
                        ? string.Empty
                        : string.IsNullOrWhiteSpace(result) || result == "PENDING"
                            ? "no result is recorded yet"
                            : string.Equals(result, "FAIL", StringComparison.OrdinalIgnoreCase)
                                ? "it failed"
                                : string.IsNullOrWhiteSpace(evidence)
                                    ? "it recorded no evidence reference"
                                    : "the result '" + result.ToLowerInvariant() + "' does not satisfy it"));
            }

            // The personnel check the owner records on its licences, for the people assigned to the order.
            if (people.Count > 0)
            {
                var problems = new List<string>();
                var evidence = new List<string>();

                foreach (var person in people)
                {
                    var competency = contractors
                        .SelectMany(contractor => contractor.Competencies.Select(row => (contractor.Id, row)))
                        .Where(pair => string.Equals(pair.Id, person, StringComparison.OrdinalIgnoreCase))
                        .Select(pair => pair.row)
                        .ToList();

                    if (competency.Count == 0)
                    {
                        problems.Add(person + " holds no licence");
                        continue;
                    }

                    var current = competency.Where(row => row.IsCurrent).ToList();

                    if (current.Count == 0)
                    {
                        problems.Add(person + " (" + competency[0].Describe() + ")");
                        continue;
                    }

                    evidence.AddRange(current.Select(row => row.EvidenceRef));
                }

                readiness.Add(new OgReadinessItem(
                    "PERSONNEL_COMPETENCY",
                    Required: true,
                    problems.Count == 0,
                    evidence.Count == 0 ? null : string.Join(", ", evidence.Distinct(StringComparer.Ordinal)),
                    problems.Count == 0 ? string.Empty : string.Join("; ", problems)));
            }

            // The item the permit itself supplies: a window that can cover a moment (AC-4).
            var window = PermitToWorkRules.ValidateWindow(row.VALID_FROM_UTC, row.VALID_TO_UTC);

            readiness.Add(new OgReadinessItem(
                "PERMIT_WINDOW",
                Required: true,
                window is null,
                window is null
                    ? row.PERMIT_ID + " · " + row.VALID_FROM_UTC.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture) +
                      " to " + row.VALID_TO_UTC.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture)
                    : null,
                window?.Message ?? string.Empty));

            var permit = new OgWorkPermit(
                row.PERMIT_ID,
                orderId,
                (row.STATUS ?? string.Empty).Trim(),
                row.SCOPE_DESCRIPTION ?? string.Empty,
                Recorded(row.RISK_REFERENCE),
                Recorded(row.ISOLATION_REFERENCE),
                Recorded(row.PERSONNEL_REFERENCE),
                Recorded(row.MATERIAL_REFERENCE),
                row.REQUESTED_BY,
                row.ISSUED_BY,
                row.VALID_FROM_UTC,
                row.VALID_TO_UTC,
                row.CONDITIONS,
                readiness,
                [],
                asOf);

            rows.Add(permit with { Blocks = PermitToWorkRules.StartBlocks(permit) });
        }

        var ordered = rows
            .OrderBy(permit => permit.IsIssued && permit.IsWithinWindow ? 0 : 1)
            .ThenBy(permit => permit.WorkOrderId, StringComparer.Ordinal)
            .ToList();

        _ = orders;

        return OgResult<OgPermitToWorkWorkspace>.Ok(new OgPermitToWorkWorkspace(
            string.IsNullOrWhiteSpace(workOrderId) ? null : workOrderId.Trim(),
            contractors,
            ordered,
            ordered.FirstOrDefault(),
            PermitToWorkRules.States,
            asOf,
            DateTime.UtcNow));
    }

    /// <summary>The contractors of the register: business associates that hold at least one licence.</summary>
    private static IReadOnlyList<OgContractor> BuildContractors(
        IReadOnlyList<BUSINESS_ASSOCIATE> associates,
        IReadOnlyList<BA_LICENSE> licences,
        IReadOnlyList<BA_LICENSE_STATUS> statuses,
        DateTime asOf)
    {
        var latest = statuses
            .Where(row => !string.Equals(row.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase))
            .Where(row => !string.IsNullOrWhiteSpace(row.LICENSEE_BA_ID) && !string.IsNullOrWhiteSpace(row.LICENSE_ID))
            .GroupBy(row => row.LICENSEE_BA_ID + "\u001f" + row.LICENSE_ID, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                group => group.Key,
                group => group
                    .OrderByDescending(row => row.EFFECTIVE_DATE)
                    .ThenByDescending(row => row.ROW_CREATED_DATE)
                    .First(),
                StringComparer.OrdinalIgnoreCase);

        return licences
            .Where(row => !string.Equals(row.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase))
            .Where(row => !string.IsNullOrWhiteSpace(row.LICENSEE_BA_ID))
            .GroupBy(row => row.LICENSEE_BA_ID!.Trim(), StringComparer.OrdinalIgnoreCase)
            .Select(group =>
            {
                var associate = associates.FirstOrDefault(candidate =>
                    string.Equals(candidate.BUSINESS_ASSOCIATE_ID?.Trim(), group.Key, StringComparison.OrdinalIgnoreCase));

                var competencies = group
                    .Select(licence =>
                    {
                        var status = latest.TryGetValue(group.Key + "\u001f" + licence.LICENSE_ID, out var found)
                            ? found
                            : null;

                        var (current, why) = PermitToWorkRules.LicenceState(
                            licence.ACTIVE_IND, status?.LICENSE_STATUS,
                            licence.EXPIRY_DATE ?? status?.EXPIRY_DATE, asOf);

                        return new OgCompetency(
                            licence.LICENSE_ID ?? string.Empty,
                            licence.LICENSE_TYPE ?? "LICENSE",
                            status?.LICENSE_STATUS ?? string.Empty,
                            licence.EXPIRY_DATE ?? status?.EXPIRY_DATE,
                            current,
                            licence.LICENSE_NUM ?? licence.LICENSE_ID ?? string.Empty,
                            why);
                    })
                    .OrderBy(competency => competency.Type, StringComparer.Ordinal)
                    .ToList();

                return new OgContractor(
                    group.Key,
                    associate?.BA_LONG_NAME ?? associate?.BA_SHORT_NAME ?? group.Key,
                    associate?.BA_TYPE ?? associate?.BA_CATEGORY ?? "not classified",
                    competencies);
            })
            .OrderBy(contractor => contractor.Name, StringComparer.Ordinal)
            .ToList();
    }

    /// <summary>A reference as recorded, with the placeholder read back as "no reference recorded".</summary>
    private static string? Recorded(string? value) =>
        string.IsNullOrWhiteSpace(value) ||
        string.Equals(value.Trim(), PermitToWorkRules.NotRecorded, StringComparison.OrdinalIgnoreCase)
            ? null
            : value.Trim();

    private static string? FirstEvidence(WORK_ORDER_INSPECTION condition)
    {
        string?[] candidates =
        [
            condition.PERSONNEL_ACCEPTANCE_REVISION,
            condition.MATERIAL_ACCEPTANCE_REVISION,
            condition.DOCUMENT_ACCEPTANCE_REVISION,
            condition.REGULATORY_REF,
        ];

        return candidates.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value))?.Trim();
    }

    private static async Task<string?> FieldOfAsync(
        IDataSource source,
        string workOrderId,
        CancellationToken cancellationToken)
    {
        var components = await OgUow.ReadAllAsync<WORK_ORDER_COMPONENT>(source).ConfigureAwait(false);

        cancellationToken.ThrowIfCancellationRequested();

        return components
            .Where(row => !string.Equals(row.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase))
            .Where(row => string.Equals(row.WORK_ORDER_ID, workOrderId, StringComparison.OrdinalIgnoreCase))
            .Select(row => row.FIELD_ID)
            .FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));
    }
}
