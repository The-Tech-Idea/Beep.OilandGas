using System.Globalization;
using Beep.OilandGas.Decommissioning.Constants;
using Beep.OilandGas.Models.Data.Decommissioning;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Hse;
using TheTechIdea.OilGas.WellSchematic;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Data.Decommissioning;

/// <summary>
/// One barrier the owner's permanent-abandonment template requires (DEC-02, AC-2).
/// </summary>
/// <param name="Conditional">
/// True when the template adds the element only under a condition it evaluates against a schematic (a host casing, a
/// formation). The desktop holds no schematic document, so a conditional element is required only when one was planned - and
/// the page says so rather than silently dropping it.
/// </param>
public sealed record OgRequiredBarrier(string Envelope, string BarrierType, bool Conditional)
{
    public string Describe() =>
        Envelope + " · " + BarrierType + (Conditional ? " (conditional on the well's schematic)" : " (required)");
}

/// <summary>One recorded barrier of a well's abandonment programme (DEC-02's <c>OgAbandonmentBarrier</c>).</summary>
public sealed record OgAbandonmentBarrier(
    string Id,
    string Envelope,
    string BarrierType,
    string? Description,
    decimal? TopDepth,
    decimal? BaseDepth,
    string DepthUom,
    string? VerificationMethod,
    string? EvidenceRef,
    string? VerifiedBy,
    DateTime? VerifiedDate,
    bool Verified)
{
    /// <summary>AC-1: a barrier counts only when it is marked executed <b>and</b> its evidence reference is recorded.</summary>
    public bool IsVerifiedWithEvidence =>
        Verified && !string.IsNullOrWhiteSpace(EvidenceRef);

    /// <summary>AC-4: a depth is shown with its unit, and never without one.</summary>
    public string DepthText => TopDepth is { } top && BaseDepth is { } bottom
        ? GhgFigure(top) + " - " + GhgFigure(bottom) + " " + (string.IsNullOrWhiteSpace(DepthUom) ? "(no unit)" : DepthUom)
        : "no recorded depth";

    public string Describe() =>
        Envelope + " · " + BarrierType + " · " + DepthText +
        (string.IsNullOrWhiteSpace(Description) ? string.Empty : " · " + Description) +
        (IsVerifiedWithEvidence
            ? " · verified by " + VerifiedBy + " on " +
              (VerifiedDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? "(no date)") +
              " · evidence " + EvidenceRef
            : Verified
                ? " · executed but no evidence is recorded, so it does not count"
                : " · planned, not verified");

    private static string GhgFigure(decimal value) =>
        value.ToString("0.####", CultureInfo.InvariantCulture);
}

/// <summary>One identified leak path (DEC-02, AC-3).</summary>
public sealed record OgLeakPath(
    string Id,
    string Description,
    string Status,
    string? AddressedHow,
    string? AddressedBy,
    DateTime? AddressedDate)
{
    public bool IsOpen => !string.Equals(Status, WellPaRules.Addressed, StringComparison.OrdinalIgnoreCase);

    public string Describe() =>
        Description + " · " + Status.ToLowerInvariant() +
        (IsOpen
            ? string.Empty
            : " · " + AddressedHow + " · " + AddressedBy + " on " +
              (AddressedDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? "(no date)"));
}

/// <summary>A well's plug-and-abandonment programme (DEC-02).</summary>
public sealed record OgWellPa(
    OgHseScope Scope,
    string WellId,
    string Status,
    string StatusLabel,
    DateTime? PlannedDate,
    string? PlannedBy,
    IReadOnlyList<OgAbandonmentBarrier> Barriers,
    IReadOnlyList<OgRequiredBarrier> Required,
    IReadOnlyList<OgLeakPath> LeakPaths,
    string? Verification,
    IReadOnlyList<string> Blocks,
    DateTime ReadAt)
{
    public bool IsComplete => string.Equals(Status, WellPaRules.Completed, StringComparison.OrdinalIgnoreCase);

    public int VerifiedBarriers => Barriers.Count(barrier => barrier.IsVerifiedWithEvidence);

    /// <summary>AC-2: the required envelopes, as the owner's template names them.</summary>
    public IReadOnlyList<string> RequiredEnvelopes =>
        Required.Select(barrier => barrier.Envelope).Distinct(StringComparer.OrdinalIgnoreCase).ToList();

    public IReadOnlyList<OgAbandonmentBarrier> Unverified =>
        Barriers.Where(barrier => !barrier.IsVerifiedWithEvidence).ToList();

    public IReadOnlyList<OgLeakPath> OpenLeakPaths => LeakPaths.Where(path => path.IsOpen).ToList();

    public string PlannedText => PlannedDate is { } planned
        ? "planned " + planned.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) +
          (string.IsNullOrWhiteSpace(PlannedBy) ? string.Empty : " by " + PlannedBy)
        : "no planned date";

    public string StatusText => StatusLabel + (string.IsNullOrWhiteSpace(Status)
        ? string.Empty
        : " (" + Status.ToLowerInvariant() + ")") + " · " + PlannedText;

    public string BarriersText => Barriers.Count == 0
        ? "No barrier is recorded; a programme has not been planned."
        : string.Join(Environment.NewLine, Barriers.Select(barrier => barrier.Describe()));

    public string RequiredText => Required.Count == 0
        ? "The owner's template names no required barrier, so nothing can be required of this well."
        : string.Join(Environment.NewLine, Required.Select(required => required.Describe()));

    public string LeakPathsText => LeakPaths.Count == 0
        ? "No leak path is recorded."
        : string.Join(Environment.NewLine, LeakPaths.Select(path => path.Describe()));

    /// <summary>
    /// The completion checklist, so the page shows the same reasons the command will give (AC-1, AC-3). A programme that is
    /// already closed says so as its outcome, not as a defect: the 'already completed' block exists to refuse a second
    /// closure, not to make a finished abandonment look unfinished.
    /// </summary>
    public string ChecklistText => IsComplete
        ? "The abandonment is complete: every required barrier is verified with its own evidence and no leak path is open."
        : Blocks.Count == 0
            ? "Every required barrier is verified with its evidence, no leak path is open, and the programme is not yet closed."
            : string.Join(Environment.NewLine, Blocks.Select(block => "✗ " + block));

    public string VerificationText => string.IsNullOrWhiteSpace(Verification)
        ? "no closure verification is recorded"
        : Verification;

    /// <summary>AC-5: the schematic is a visual aid, and the page says once what it is not.</summary>
    public const string SchematicNotice =
        "The abandonment schematic is a visual aid. It is not the barrier evidence: a barrier counts only when its own " +
        "evidence reference is recorded against it.";

    public string ReadText =>
        "read at " + ReadAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
}

/// <summary>The well plug-and-abandonment rules DEC-02 states once.</summary>
public static class WellPaRules
{
    public const string CodeNoConnection = "wellpa-no-connection";
    public const string CodeReadFailed = "wellpa-read-failed";
    public const string CodeWriteFailed = "wellpa-write-failed";
    public const string CodeActorRequired = "wellpa-actor-required";
    public const string CodeNoWell = "wellpa-no-well";
    public const string CodeUnknownBarrier = "wellpa-unknown-barrier";
    public const string CodePlannedDateRequired = "wellpa-planned-date-required";
    public const string CodeNoBarriers = "wellpa-no-barriers";
    public const string CodeBarrierInvalid = "wellpa-barrier-invalid";
    public const string CodeDepthUnitRequired = "wellpa-depth-unit-required";
    public const string CodeEvidenceRequired = "wellpa-evidence-required";
    public const string CodeRequiredBarriersMissing = "wellpa-required-barriers-missing";
    public const string CodeLeakPathOpen = "wellpa-leak-path-open";
    public const string CodeAlreadyComplete = "wellpa-already-complete";
    public const string CodeSelfClosure = "wellpa-self-closure";
    public const string CodeReasonRequired = "wellpa-reason-required";
    public const string CodeLeakPathNotFound = "wellpa-leak-path-not-found";

    /// <summary>The owner's abandonment states, from its own reference defaults.</summary>
    public const string Planned = DecommissioningReferenceDefaults.Planned;
    public const string InProgress = DecommissioningReferenceDefaults.InProgress;
    public const string Completed = DecommissioningReferenceDefaults.Completed;
    public const string Cancelled = DecommissioningReferenceDefaults.Cancelled;

    public const string Addressed = "ADDRESSED";
    public const string Open = "OPEN";

    /// <summary>
    /// AC-2: the required barrier set, read from the owner's permanent-abandonment template — never from the view.
    /// </summary>
    /// <remarks>
    /// The owner's <c>BarrierTemplates.PermanentAbandonment</c> derives a barrier set from a schematic document, which this
    /// build does not hold; its <b>template</b> is the same definition and is what can be read without one. Its deep envelopes
    /// and its surface envelope are the required envelopes, and the per-plug element types are what each one must contain. A
    /// rule the template guards with a <c>when</c> condition depends on the schematic (a host casing, a formation), so it is
    /// marked conditional rather than required outright.
    /// </remarks>
    public static IReadOnlyList<OgRequiredBarrier> RequiredBarriers()
    {
        if (!JsonBarrierTemplates.BuiltIn.Templates.TryGetValue("permanentAbandonment", out var template) ||
            template?.Plugs is null)
        {
            return [];
        }

        var envelopes = new List<string>();

        envelopes.AddRange(template.Plugs.DeepEnvelopes ?? []);

        if (!string.IsNullOrWhiteSpace(template.Plugs.SurfaceEnvelope))
        {
            envelopes.Add(template.Plugs.SurfaceEnvelope);
        }

        var required = new List<OgRequiredBarrier>();

        foreach (var envelope in envelopes.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            foreach (var rule in template.Plugs.PerPlug ?? [])
            {
                if (string.IsNullOrWhiteSpace(rule?.Type))
                {
                    continue;
                }

                required.Add(new OgRequiredBarrier(
                    envelope,
                    rule.Type.Trim(),
                    !string.IsNullOrWhiteSpace(rule.When)));
            }
        }

        return required;
    }

    /// <summary>
    /// AC-1 and AC-4: a barrier needs its type, and a recorded interval needs both depths and its unit with the base below
    /// the top.
    /// </summary>
    public static (string Code, string Message)? ValidateBarrier(
        string? barrierType,
        decimal? topDepth,
        decimal? baseDepth,
        string? depthUom)
    {
        if (string.IsNullOrWhiteSpace(barrierType))
        {
            return (CodeBarrierInvalid, "A barrier needs its type, so it can be matched to the template's requirement.");
        }

        if (topDepth is null && baseDepth is null)
        {
            return null;
        }

        if (topDepth is null || baseDepth is null)
        {
            return (CodeBarrierInvalid,
                "A barrier interval needs both its top and its base depth, or neither.");
        }

        if (string.IsNullOrWhiteSpace(depthUom))
        {
            return (CodeDepthUnitRequired,
                "A depth needs its unit: " + topDepth + " means nothing without it.");
        }

        return baseDepth <= topDepth
            ? (CodeBarrierInvalid,
                "A barrier's base depth (" + baseDepth + ") must be below its top (" + topDepth + ").")
            : null;
    }

    /// <summary>AC-6's segregation of duties: the person who planned or executed the programme cannot close it.</summary>
    public static (string Code, string Message)? CheckSeparateDuties(
        string? plannedBy,
        IEnumerable<string> barrierExecutors,
        string actor)
    {
        ArgumentNullException.ThrowIfNull(barrierExecutors);

        if (string.Equals(plannedBy?.Trim(), actor.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            return (CodeSelfClosure, plannedBy + " planned this abandonment and therefore cannot close it.");
        }

        var executor = barrierExecutors.FirstOrDefault(name =>
            string.Equals(name?.Trim(), actor.Trim(), StringComparison.OrdinalIgnoreCase));

        return executor is null
            ? null
            : (CodeSelfClosure, executor + " recorded this abandonment's barrier evidence and therefore cannot close it.");
    }

    /// <summary>
    /// AC-1 and AC-3: every reason completion is refused, all at once. A required envelope without a verified, evidenced
    /// barrier blocks; so does an open leak path, and so does a programme that is already closed.
    /// </summary>
    public static IReadOnlyList<string> CompletionBlocks(OgWellPa programme)
    {
        ArgumentNullException.ThrowIfNull(programme);

        var blocks = new List<string>();

        if (programme.IsComplete)
        {
            blocks.Add(programme.WellId + " is already completed.");
        }

        if (programme.Barriers.Count == 0)
        {
            blocks.Add("No barrier is recorded, so there is nothing to verify.");
        }

        // A barrier that is executed without evidence does not count (AC-1), and is named as such.
        foreach (var barrier in programme.Barriers.Where(barrier => barrier.Verified && !barrier.IsVerifiedWithEvidence))
        {
            blocks.Add(barrier.Envelope + " · " + barrier.BarrierType +
                       " is marked executed but records no evidence reference, so it does not verify the barrier.");
        }

        foreach (var envelope in programme.RequiredEnvelopes)
        {
            var inEnvelope = programme.Barriers
                .Where(barrier => string.Equals(barrier.Envelope, envelope, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (inEnvelope.Any(barrier => barrier.IsVerifiedWithEvidence))
            {
                continue;
            }

            // Name what the template requires in that envelope, so the refusal says what is missing rather than only that
            // something is.
            var required = programme.Required
                .Where(rule => string.Equals(rule.Envelope, envelope, StringComparison.OrdinalIgnoreCase) &&
                               !rule.Conditional)
                .Select(rule => rule.BarrierType)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            blocks.Add("The " + envelope + " envelope has no verified barrier with evidence" +
                       (required.Count == 0
                           ? "."
                           : ", and the owner's template requires " + string.Join(", ", required) + " in it.") +
                       (inEnvelope.Count == 0
                           ? " Nothing is planned in it."
                           : " Planned: " + string.Join(", ", inEnvelope.Select(barrier => barrier.BarrierType)) + "."));
        }

        foreach (var path in programme.OpenLeakPaths)
        {
            blocks.Add("Leak path '" + path.Description + "' is still open, so the abandonment is not complete.");
        }

        return blocks;
    }

    /// <summary>The owner's reference label for a state, when it names one.</summary>
    public static string Label(string? state) => state?.Trim().ToUpperInvariant() switch
    {
        Planned => "planned",
        InProgress => "in progress",
        Completed => "completed",
        Cancelled => "cancelled",
        null or "" => "no state recorded",
        _ => state!.Trim().ToLowerInvariant(),
    };
}

/// <summary>The inputs of a barrier to plan (DEC-02).</summary>
public sealed record OgBarrierInput(
    string Envelope,
    string BarrierType,
    string? Description,
    decimal? TopDepth,
    decimal? BaseDepth,
    string? DepthUom,
    string? VerificationMethod);

/// <summary>DEC-02: the well plug-and-abandonment programme, its barriers, its leak paths and its closure.</summary>
public interface IOgWellPa
{
    Task<OgResult<OgWellPa>> LoadAsync(
        OgHseScope scope,
        string wellId,
        CancellationToken cancellationToken = default);

    /// <summary>Plans the programme: the planned date (which DEC-01 needs) and the barriers with their depths and units.</summary>
    Task<OgResult<OgWellPa>> PlanAsync(
        OgHseScope scope,
        string wellId,
        DateTime plannedDate,
        IReadOnlyList<OgBarrierInput> barriers,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>Records a barrier's execution and its evidence; refused when the evidence reference is missing (AC-1).</summary>
    Task<OgResult<OgWellPa>> VerifyBarrierAsync(
        OgHseScope scope,
        string wellId,
        string barrierId,
        string verificationMethod,
        string evidenceRef,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>Records an identified leak path (AC-3).</summary>
    Task<OgResult<OgWellPa>> AddLeakPathAsync(
        OgHseScope scope,
        string wellId,
        string description,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>Addresses a leak path, which is what allows a completion that was blocked by it.</summary>
    Task<OgResult<OgWellPa>> AddressLeakPathAsync(
        OgHseScope scope,
        string wellId,
        string leakPathId,
        string how,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>Closes the programme when every required barrier is verified with evidence and no leak path is open.</summary>
    Task<OgResult<OgWellPa>> CloseAsync(
        OgHseScope scope,
        string wellId,
        string rationale,
        string actor,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// DEC-02 over the owner's <c>ABANDONMENT_STATUS</c>, and the <c>ABANDONMENT_BARRIER</c> and <c>ABANDONMENT_LEAK_PATH</c>
/// records this feature added to the decommissioning module, with the required barrier set read from the owner's
/// permanent-abandonment template.
/// </summary>
/// <remarks>
/// The programme is one abandonment record per well (<c>WA-&lt;well&gt;</c>), and its state is a series of status rows, of
/// which the latest is current - the shape DEC-01 already reads. Planning writes the first row with the planned date, each
/// barrier verification records its own evidence, and closure appends the completed row with who verified it and when, so
/// DEC-01 sees the completed state without a second source.
/// </remarks>
public sealed class OgWellPaService : IOgWellPa
{
    private readonly Func<IDataSource?> _source;
    private readonly IFailureReporter _failures;

    public OgWellPaService(Func<IDataSource?> source, IFailureReporter failures)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _failures = failures ?? throw new ArgumentNullException(nameof(failures));
    }

    public async Task<OgResult<OgWellPa>> LoadAsync(
        OgHseScope scope,
        string wellId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(scope);

        if (string.IsNullOrWhiteSpace(wellId))
        {
            return OgResult<OgWellPa>.Failed(
                "A plug-and-abandonment programme is opened for one well; none is named.", WellPaRules.CodeNoWell);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgWellPa>.Failed(
                "No connection is open, so the abandonment programme cannot be read.", WellPaRules.CodeNoConnection);
        }

        try
        {
            return await ReadAsync(source, scope, wellId.Trim(), cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, "reading the abandonment programme for " + wellId,
                "the abandonment programme is not shown", FailureSeverity.Error);

            return OgResult<OgWellPa>.Failed(
                "The abandonment programme could not be read" +
                (string.IsNullOrWhiteSpace(reference) ? "." : " (reference " + reference + ")."),
                reference ?? WellPaRules.CodeReadFailed);
        }
    }

    public async Task<OgResult<OgWellPa>> PlanAsync(
        OgHseScope scope,
        string wellId,
        DateTime plannedDate,
        IReadOnlyList<OgBarrierInput> barriers,
        string actor,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(scope);
        ArgumentNullException.ThrowIfNull(barriers);

        if (string.IsNullOrWhiteSpace(actor))
        {
            return Failure("Planning a programme must record who planned it.", WellPaRules.CodeActorRequired);
        }

        if (string.IsNullOrWhiteSpace(wellId))
        {
            return Failure("A programme is planned for one well; none is named.", WellPaRules.CodeNoWell);
        }

        // The planned date is what DEC-01 needs to compute a planned year and an overdue, so it is required, not optional.
        if (plannedDate == default)
        {
            return Failure(
                "A programme needs its planned date: without one nothing can say when the work is due, and no overdue can " +
                "be computed.", WellPaRules.CodePlannedDateRequired);
        }

        if (barriers.Count == 0)
        {
            return Failure(
                "A programme needs at least one barrier; a plan without barriers cannot be verified.",
                WellPaRules.CodeNoBarriers);
        }

        foreach (var barrier in barriers)
        {
            if (WellPaRules.ValidateBarrier(barrier.BarrierType, barrier.TopDepth, barrier.BaseDepth, barrier.DepthUom)
                is { } refusal)
            {
                return Failure(refusal.Message, refusal.Code);
            }
        }

        var source = _source();

        if (source is null)
        {
            return Failure("No connection is open, so the programme cannot be planned.", WellPaRules.CodeNoConnection);
        }

        try
        {
            var now = DateTime.UtcNow;
            var programmeId = ProgrammeId(wellId);

            // The status row is the programme's record: appending PLANNED with its date is what makes DEC-01 see the
            // timetable, and appending (rather than editing) keeps every earlier decision.
            var status = OgUow.For<ABANDONMENT_STATUS>(source);
            status.PrimaryKey = nameof(ABANDONMENT_STATUS.ABANDONMENT_STATUS_ID);

            status.Add(new ABANDONMENT_STATUS
            {
                ABANDONMENT_STATUS_ID = Guid.NewGuid().ToString("N"),
                WELL_ABANDONMENT_ID = programmeId,
                UWI = wellId.Trim(),
                STATUS = WellPaRules.Planned,
                STATUS_DATE = now,
                STATUS_CHANGED_BY = actor.Trim(),
                PLANNED_DATE = plannedDate.Date,
                PLANNED_BY = actor.Trim(),
                NOTES = "Programme planned with " + barriers.Count.ToString(CultureInfo.InvariantCulture) + " barrier(s).",
                ACTIVE_IND = "Y",
                ROW_CREATED_BY = actor.Trim(),
                ROW_CREATED_DATE = now,
                ROW_CHANGED_BY = actor.Trim(),
                ROW_CHANGED_DATE = now,
            });

            var committed = await status.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(committed))
            {
                return Failure(OgUow.Message(committed, "The programme was not saved."), WellPaRules.CodeWriteFailed);
            }

            var existing = await OgUow.ReadAllAsync<ABANDONMENT_BARRIER>(source).ConfigureAwait(false);

            var unit = OgUow.For<ABANDONMENT_BARRIER>(source);
            unit.PrimaryKey = nameof(ABANDONMENT_BARRIER.ABANDONMENT_BARRIER_ID);

            foreach (var barrier in barriers)
            {
                var id = BarrierId(programmeId, barrier.Envelope, barrier.BarrierType);

                if (existing.Any(row => string.Equals(row.ABANDONMENT_BARRIER_ID, id, StringComparison.Ordinal)))
                {
                    continue;
                }

                unit.Add(new ABANDONMENT_BARRIER
                {
                    ABANDONMENT_BARRIER_ID = id,
                    WELL_ABANDONMENT_ID = programmeId,
                    UWI = wellId.Trim(),
                    ENVELOPE = barrier.Envelope.Trim().ToUpperInvariant(),
                    BARRIER_TYPE = barrier.BarrierType.Trim().ToUpperInvariant(),
                    DESCRIPTION = string.IsNullOrWhiteSpace(barrier.Description) ? null : barrier.Description.Trim(),
                    TOP_DEPTH = barrier.TopDepth,
                    BASE_DEPTH = barrier.BaseDepth,
                    DEPTH_UOM = string.IsNullOrWhiteSpace(barrier.DepthUom) ? null : barrier.DepthUom.Trim(),
                    VERIFICATION_METHOD = string.IsNullOrWhiteSpace(barrier.VerificationMethod)
                        ? null
                        : barrier.VerificationMethod.Trim(),
                    VERIFIED_IND = "N",
                    ACTIVE_IND = "Y",
                    ROW_CREATED_BY = actor.Trim(),
                    ROW_CREATED_DATE = now,
                    ROW_CHANGED_BY = actor.Trim(),
                    ROW_CHANGED_DATE = now,
                });
            }

            var barriersCommitted = await unit.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(barriersCommitted))
            {
                return Failure(OgUow.Message(barriersCommitted, "The barriers were not saved."),
                    WellPaRules.CodeWriteFailed);
            }

            return await AfterAsync(source, scope, wellId, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, "planning the abandonment programme for " + wellId,
                "the programme was not saved", FailureSeverity.Error);

            return Failure(
                "The programme could not be planned" +
                (string.IsNullOrWhiteSpace(reference) ? "." : " (reference " + reference + ")."),
                reference ?? WellPaRules.CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgWellPa>> VerifyBarrierAsync(
        OgHseScope scope,
        string wellId,
        string barrierId,
        string verificationMethod,
        string evidenceRef,
        string actor,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(scope);

        if (string.IsNullOrWhiteSpace(actor))
        {
            return Failure("A verification must record who made it.", WellPaRules.CodeActorRequired);
        }

        // AC-1: evidence is what verifies a barrier; without it the barrier stays unverified rather than being asserted.
        if (string.IsNullOrWhiteSpace(evidenceRef))
        {
            return Failure(
                "A barrier is verified by its evidence, and no evidence reference was given. Record the report, log or " +
                "ticket that verifies it.", WellPaRules.CodeEvidenceRequired);
        }

        var source = _source();

        if (source is null)
        {
            return Failure("No connection is open, so the barrier cannot be verified.", WellPaRules.CodeNoConnection);
        }

        try
        {
            var unit = OgUow.For<ABANDONMENT_BARRIER>(source);
            unit.PrimaryKey = nameof(ABANDONMENT_BARRIER.ABANDONMENT_BARRIER_ID);

            var rows = await OgUow.ReadAsync(
                unit,
                [OgUow.Equal(nameof(ABANDONMENT_BARRIER.ABANDONMENT_BARRIER_ID), barrierId)],
                cancellationToken).ConfigureAwait(false);

            var barrier = rows.FirstOrDefault();

            if (barrier is null)
            {
                return Failure("Barrier '" + barrierId + "' is not recorded.", WellPaRules.CodeUnknownBarrier);
            }

            barrier.VERIFIED_IND = "Y";
            barrier.VERIFIED_BY = actor.Trim();
            barrier.VERIFIED_DATE = DateTime.UtcNow;
            barrier.EVIDENCE_REF = evidenceRef.Trim();
            barrier.VERIFICATION_METHOD = string.IsNullOrWhiteSpace(verificationMethod)
                ? barrier.VERIFICATION_METHOD
                : verificationMethod.Trim();
            barrier.ROW_CHANGED_BY = actor.Trim();
            barrier.ROW_CHANGED_DATE = DateTime.UtcNow;

            var updated = unit.Update(barrier);

            if (!OgUow.IsOk(updated))
            {
                return Failure(OgUow.Message(updated, "The barrier verification was not saved."),
                    WellPaRules.CodeWriteFailed);
            }

            var committed = await unit.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(committed))
            {
                return Failure(OgUow.Message(committed, "The barrier verification was not saved."),
                    WellPaRules.CodeWriteFailed);
            }

            return await AfterAsync(source, scope, wellId, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, "verifying barrier " + barrierId + " on " + wellId,
                "the barrier verification was not saved", FailureSeverity.Error);

            return Failure(
                "The barrier could not be verified" +
                (string.IsNullOrWhiteSpace(reference) ? "." : " (reference " + reference + ")."),
                reference ?? WellPaRules.CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgWellPa>> AddLeakPathAsync(
        OgHseScope scope,
        string wellId,
        string description,
        string actor,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(scope);

        if (string.IsNullOrWhiteSpace(actor))
        {
            return Failure("A leak path must record who identified it.", WellPaRules.CodeActorRequired);
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            return Failure("A leak path needs its description.", WellPaRules.CodeBarrierInvalid);
        }

        var source = _source();

        if (source is null)
        {
            return Failure("No connection is open, so the leak path cannot be recorded.", WellPaRules.CodeNoConnection);
        }

        try
        {
            var now = DateTime.UtcNow;

            var unit = OgUow.For<ABANDONMENT_LEAK_PATH>(source);
            unit.PrimaryKey = nameof(ABANDONMENT_LEAK_PATH.ABANDONMENT_LEAK_PATH_ID);

            unit.Add(new ABANDONMENT_LEAK_PATH
            {
                ABANDONMENT_LEAK_PATH_ID = Guid.NewGuid().ToString("N"),
                WELL_ABANDONMENT_ID = ProgrammeId(wellId),
                UWI = wellId.Trim(),
                DESCRIPTION = description.Trim(),
                STATUS = WellPaRules.Open,
                ACTIVE_IND = "Y",
                ROW_CREATED_BY = actor.Trim(),
                ROW_CREATED_DATE = now,
                ROW_CHANGED_BY = actor.Trim(),
                ROW_CHANGED_DATE = now,
            });

            var committed = await unit.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(committed))
            {
                return Failure(OgUow.Message(committed, "The leak path was not saved."), WellPaRules.CodeWriteFailed);
            }

            return await AfterAsync(source, scope, wellId, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, "recording a leak path on " + wellId, "the leak path was not saved", FailureSeverity.Error);

            return Failure(
                "The leak path could not be recorded" +
                (string.IsNullOrWhiteSpace(reference) ? "." : " (reference " + reference + ")."),
                reference ?? WellPaRules.CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgWellPa>> AddressLeakPathAsync(
        OgHseScope scope,
        string wellId,
        string leakPathId,
        string how,
        string actor,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(scope);

        if (string.IsNullOrWhiteSpace(actor) || string.IsNullOrWhiteSpace(how))
        {
            return Failure(
                "Addressing a leak path records who did it and how.", WellPaRules.CodeActorRequired);
        }

        var source = _source();

        if (source is null)
        {
            return Failure("No connection is open, so the leak path cannot be addressed.", WellPaRules.CodeNoConnection);
        }

        try
        {
            var unit = OgUow.For<ABANDONMENT_LEAK_PATH>(source);
            unit.PrimaryKey = nameof(ABANDONMENT_LEAK_PATH.ABANDONMENT_LEAK_PATH_ID);

            var rows = await OgUow.ReadAsync(
                unit,
                [OgUow.Equal(nameof(ABANDONMENT_LEAK_PATH.ABANDONMENT_LEAK_PATH_ID), leakPathId)],
                cancellationToken).ConfigureAwait(false);

            var path = rows.FirstOrDefault();

            if (path is null)
            {
                return Failure("Leak path '" + leakPathId + "' is not recorded.", WellPaRules.CodeLeakPathNotFound);
            }

            path.STATUS = WellPaRules.Addressed;
            path.ADDRESSED_HOW = how.Trim();
            path.ADDRESSED_BY = actor.Trim();
            path.ADDRESSED_DATE = DateTime.UtcNow;
            path.ROW_CHANGED_BY = actor.Trim();
            path.ROW_CHANGED_DATE = DateTime.UtcNow;

            var updated = unit.Update(path);

            if (!OgUow.IsOk(updated))
            {
                return Failure(OgUow.Message(updated, "The leak path was not updated."), WellPaRules.CodeWriteFailed);
            }

            var committed = await unit.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(committed))
            {
                return Failure(OgUow.Message(committed, "The leak path was not updated."), WellPaRules.CodeWriteFailed);
            }

            return await AfterAsync(source, scope, wellId, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, "addressing leak path " + leakPathId + " on " + wellId,
                "the leak path was not updated", FailureSeverity.Error);

            return Failure(
                "The leak path could not be addressed" +
                (string.IsNullOrWhiteSpace(reference) ? "." : " (reference " + reference + ")."),
                reference ?? WellPaRules.CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgWellPa>> CloseAsync(
        OgHseScope scope,
        string wellId,
        string rationale,
        string actor,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(scope);

        if (string.IsNullOrWhiteSpace(actor))
        {
            return Failure("A closure must record who made it.", WellPaRules.CodeActorRequired);
        }

        if (string.IsNullOrWhiteSpace(rationale))
        {
            return Failure(
                "A closure needs its rationale, so the record shows why the abandonment is complete.",
                WellPaRules.CodeReasonRequired);
        }

        var source = _source();

        if (source is null)
        {
            return Failure("No connection is open, so the programme cannot be closed.", WellPaRules.CodeNoConnection);
        }

        try
        {
            var read = await ReadAsync(source, scope, wellId.Trim(), cancellationToken).ConfigureAwait(false);

            if (!read.Success || read.Value is null)
            {
                return read;
            }

            var programme = read.Value;

            var blocks = WellPaRules.CompletionBlocks(programme);

            if (blocks.Count > 0)
            {
                return Failure(string.Join(" ", blocks), WellPaRules.CodeRequiredBarriersMissing);
            }

            if (WellPaRules.CheckSeparateDuties(
                    programme.PlannedBy,
                    programme.Barriers.Select(barrier => barrier.VerifiedBy ?? string.Empty),
                    actor) is { } self)
            {
                return Failure(self.Message, self.Code);
            }

            var now = DateTime.UtcNow;

            var unit = OgUow.For<ABANDONMENT_STATUS>(source);
            unit.PrimaryKey = nameof(ABANDONMENT_STATUS.ABANDONMENT_STATUS_ID);

            unit.Add(new ABANDONMENT_STATUS
            {
                ABANDONMENT_STATUS_ID = Guid.NewGuid().ToString("N"),
                WELL_ABANDONMENT_ID = ProgrammeId(wellId),
                UWI = wellId.Trim(),
                STATUS = WellPaRules.Completed,
                STATUS_DATE = now,
                STATUS_CHANGED_BY = actor.Trim(),
                PLANNED_DATE = programme.PlannedDate,
                PLANNED_BY = programme.PlannedBy,
                // AC-6: closure records the actor, the date and the verification, not just the state.
                VERIFIED_BY = actor.Trim(),
                VERIFICATION_DATE = now,
                NOTES = rationale.Trim() + " · " + programme.VerifiedBarriers.ToString(CultureInfo.InvariantCulture) +
                        " barrier(s) verified with evidence.",
                ACTIVE_IND = "Y",
                ROW_CREATED_BY = actor.Trim(),
                ROW_CREATED_DATE = now,
                ROW_CHANGED_BY = actor.Trim(),
                ROW_CHANGED_DATE = now,
            });

            var committed = await unit.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(committed))
            {
                return Failure(OgUow.Message(committed, "The closure was not saved."), WellPaRules.CodeWriteFailed);
            }

            return await AfterAsync(source, scope, wellId, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, "closing the abandonment programme for " + wellId,
                "the closure was not saved", FailureSeverity.Error);

            return Failure(
                "The programme could not be closed" +
                (string.IsNullOrWhiteSpace(reference) ? "." : " (reference " + reference + ")."),
                reference ?? WellPaRules.CodeWriteFailed);
        }
    }

    // ------------------------------------------------------------------ the read model

    private async Task<OgResult<OgWellPa>> AfterAsync(
        IDataSource source,
        OgHseScope scope,
        string wellId,
        CancellationToken cancellationToken) =>
        await ReadAsync(source, scope, wellId.Trim(), cancellationToken).ConfigureAwait(false);

    private async Task<OgResult<OgWellPa>> ReadAsync(
        IDataSource source,
        OgHseScope scope,
        string wellId,
        CancellationToken cancellationToken)
    {
        var statuses = await OgUow.ReadAllAsync<ABANDONMENT_STATUS>(source).ConfigureAwait(false);
        var barriers = await OgUow.ReadAllAsync<ABANDONMENT_BARRIER>(source).ConfigureAwait(false);
        var leakPaths = await OgUow.ReadAllAsync<ABANDONMENT_LEAK_PATH>(source).ConfigureAwait(false);

        cancellationToken.ThrowIfCancellationRequested();

        var programmeId = ProgrammeId(wellId);

        var mine = statuses
            .Where(row => !string.Equals(row.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase))
            .Where(row => string.Equals(row.WELL_ABANDONMENT_ID, programmeId, StringComparison.OrdinalIgnoreCase) ||
                          string.Equals(row.UWI?.Trim(), wellId, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(row => row.STATUS_DATE)
            .ThenByDescending(row => row.ROW_CREATED_DATE)
            .ToList();

        var latest = mine.FirstOrDefault();
        var planned = mine.FirstOrDefault(row => row.PLANNED_DATE is not null);

        var recorded = barriers
            .Where(row => !string.Equals(row.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase))
            .Where(row => string.Equals(row.WELL_ABANDONMENT_ID, programmeId, StringComparison.OrdinalIgnoreCase))
            .OrderBy(row => row.ENVELOPE, StringComparer.Ordinal)
            .ThenBy(row => row.BARRIER_TYPE, StringComparer.Ordinal)
            .Select(row => new OgAbandonmentBarrier(
                row.ABANDONMENT_BARRIER_ID,
                row.ENVELOPE,
                row.BARRIER_TYPE,
                row.DESCRIPTION,
                row.TOP_DEPTH,
                row.BASE_DEPTH,
                row.DEPTH_UOM ?? string.Empty,
                row.VERIFICATION_METHOD,
                row.EVIDENCE_REF,
                row.VERIFIED_BY,
                row.VERIFIED_DATE,
                string.Equals(row.VERIFIED_IND?.Trim(), "Y", StringComparison.OrdinalIgnoreCase)))
            .ToList();

        var paths = leakPaths
            .Where(row => !string.Equals(row.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase))
            .Where(row => string.Equals(row.WELL_ABANDONMENT_ID, programmeId, StringComparison.OrdinalIgnoreCase))
            .OrderBy(row => row.DESCRIPTION, StringComparer.Ordinal)
            .Select(row => new OgLeakPath(
                row.ABANDONMENT_LEAK_PATH_ID,
                row.DESCRIPTION,
                row.STATUS,
                row.ADDRESSED_HOW,
                row.ADDRESSED_BY,
                row.ADDRESSED_DATE))
            .ToList();

        var required = WellPaRules.RequiredBarriers();

        var workspace = new OgWellPa(
            scope,
            wellId,
            latest?.STATUS ?? string.Empty,
            WellPaRules.Label(latest?.STATUS),
            planned?.PLANNED_DATE,
            planned?.PLANNED_BY,
            recorded,
            required,
            paths,
            latest is not null && string.Equals(latest.STATUS, WellPaRules.Completed, StringComparison.OrdinalIgnoreCase)
                ? (latest.NOTES ?? string.Empty) +
                  (latest.VERIFICATION_DATE is { } verified
                      ? " · verified by " + latest.VERIFIED_BY + " on " +
                        verified.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
                      : string.Empty)
                : null,
            [],
            DateTime.UtcNow);

        // The checklist is computed from the state just read, so the page and the command cannot disagree.
        return OgResult<OgWellPa>.Ok(workspace with { Blocks = WellPaRules.CompletionBlocks(workspace) });
    }

    private static string ProgrammeId(string wellId) => "WA-" + wellId.Trim();

    private static string BarrierId(string programmeId, string envelope, string barrierType) =>
        "AB-" + programmeId + "-" + envelope.Trim().ToUpperInvariant() + "-" + barrierType.Trim().ToUpperInvariant();

    private static OgResult<OgWellPa> Failure(string message, string code) =>
        OgResult<OgWellPa>.Failed(message, code);
}
