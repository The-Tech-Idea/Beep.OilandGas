using System.Globalization;
using Beep.OilandGas.Models.Data.DrillingAndConstruction;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Data.Development;

/// <summary>One completion test (DEV-07).</summary>
public sealed record OgCompletionTest(
    string Id,
    string WellId,
    string? ProgramId,
    string TestType,
    decimal? TopDepth,
    decimal? BaseDepth,
    string? DepthUnit,
    string Status,
    string? Result,
    string? ResultUnit,
    string? PerformedBy,
    DateTime? PerformedAt,
    string? EvidenceRef)
{
    public const string Passed = "PASS";
    public const string Failed = "FAIL";

    public static IReadOnlyList<string> Statuses { get; } = [Passed, Failed];

    public bool IsPassed => string.Equals(Status, Passed, StringComparison.OrdinalIgnoreCase);

    public string StatusText => IsPassed ? "pass" : "fail";

    public string DepthText => TopDepth is { } top && BaseDepth is { } bottom
        ? $"{top.ToString("0.##", CultureInfo.InvariantCulture)}–" +
          $"{bottom.ToString("0.##", CultureInfo.InvariantCulture)} " +
          (string.IsNullOrWhiteSpace(DepthUnit) ? "(unit not recorded)" : DepthUnit!.Trim())
        : "(no interval)";

    public string ResultText => Result is { } result
        ? $"{result} {(string.IsNullOrWhiteSpace(ResultUnit) ? "(unit not recorded)" : ResultUnit!.Trim())}".TrimEnd()
        : "(no result recorded)";

    public string Describe() =>
        $"{TestType} · {DepthText} · {StatusText} · {ResultText}" +
        (string.IsNullOrWhiteSpace(EvidenceRef) ? string.Empty : $" · evidence {EvidenceRef}");
}

/// <summary>One evidence item of a completion submission (DEV-07 AC-2).</summary>
public sealed record OgCompletionEvidence(
    string Id,
    string SubmissionId,
    string WellId,
    string? Document,
    string? Reference,
    string? AddedBy,
    DateTime? AddedAt)
{
    public string Describe() =>
        $"{Document ?? "(no document)"}" +
        (string.IsNullOrWhiteSpace(Reference) ? string.Empty : $" · {Reference}") +
        $" · well {WellId}";
}

/// <summary>
/// The production handover record of an accepted completion (DEV-07 AC-4): the well, who accepted it, the conditions
/// that acceptance carried and who it was handed to.
/// </summary>
public sealed record OgCompletionHandover(
    string WellId,
    DateTime? AcceptedAt,
    string? AcceptedBy,
    IReadOnlyList<string> Conditions,
    string? HandoverTo,
    DateTime? HandoverAt)
{
    public string ConditionsText => Conditions.Count == 0
        ? "no conditions"
        : string.Join("; ", Conditions);

    public string Describe() =>
        $"{WellId} accepted by {AcceptedBy ?? "(unknown)"} on " +
        $"{AcceptedAt?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? "unknown"} · " +
        $"handed to {HandoverTo ?? "(nobody)"} on " +
        $"{HandoverAt?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? "unknown"} · {ConditionsText}";
}

/// <summary>The completion submission of one well and its decision (DEV-07).</summary>
public sealed record OgCompletionSubmission(
    string Id,
    string WellId,
    string? ProgramId,
    string? Summary,
    string State,
    string? Author,
    DateTime? SubmittedAt,
    string? Reviewer,
    string? Rationale,
    IReadOnlyList<string> Conditions,
    DateTime? AcceptedAt,
    string? AcceptedBy,
    string? HandoverTo,
    DateTime? HandoverAt)
{
    public const string Draft = "DRAFT";
    public const string Submitted = "SUBMITTED";
    public const string Accepted = "ACCEPTED";
    public const string AcceptedWithConditions = "ACCEPTED_WITH_CONDITIONS";
    public const string Returned = "RETURNED";
    public const string Rejected = "REJECTED";

    public static IReadOnlyList<string> States { get; } =
        [Draft, Submitted, Accepted, AcceptedWithConditions, Returned, Rejected];

    public bool IsAccepted =>
        string.Equals(State, Accepted, StringComparison.OrdinalIgnoreCase)
        || string.Equals(State, AcceptedWithConditions, StringComparison.OrdinalIgnoreCase);

    public bool IsSubmitted => string.Equals(State, Submitted, StringComparison.OrdinalIgnoreCase);

    public bool IsEditable =>
        string.Equals(State, Draft, StringComparison.OrdinalIgnoreCase)
        || string.Equals(State, Returned, StringComparison.OrdinalIgnoreCase);

    public string StateText => State.ToLowerInvariant();

    public OgCompletionHandover? Handover => IsAccepted
        ? new OgCompletionHandover(WellId, AcceptedAt, AcceptedBy, Conditions, HandoverTo, HandoverAt)
        : null;

    public string Describe() =>
        $"well {WellId} · {StateText} · author {Author ?? "(unknown)"}" +
        (Reviewer is null ? string.Empty : $" · reviewer {Reviewer}") +
        (Conditions.Count == 0 ? string.Empty : $" · {Conditions.Count.ToString(CultureInfo.InvariantCulture)} condition(s)");
}

/// <summary>One well that can be completed, taken from the drilling executions of the field.</summary>
public sealed record OgCompletionWell(string WellId, string? ProgramId, string? ExecutionStatus)
{
    public string Label => $"{WellId} · {ExecutionStatus?.ToLowerInvariant() ?? "no status"}";
}

/// <summary>The completion workspace for one well.</summary>
public sealed record OgCompletionWorkspace(
    string FieldName,
    IReadOnlyList<OgCompletionWell> Wells,
    string? SelectedWellId,
    IReadOnlyList<OgCompletionTest> Tests,
    IReadOnlyList<OgCompletionSubmission> Submissions,
    OgCompletionSubmission? Selected,
    IReadOnlyList<OgCompletionEvidence> Evidence,
    OgCompletionHandover? Handover)
{
    public int PassedTests => Tests.Count(test => test.IsPassed);

    public int FailedTests => Tests.Count(test => !test.IsPassed);

    public bool HasPassingTest => PassedTests > 0;

    public bool IsEmpty => Wells.Count == 0;

    /// <summary>What a submission still needs; an empty list means it can be submitted.</summary>
    public IReadOnlyList<string> SubmissionGaps => CompletionRules.SubmissionGaps(Tests, Evidence);

    /// <summary>AC-1: acceptance needs at least one passed test.</summary>
    public bool CanAccept => HasPassingTest;

    public string TestsText => Tests.Count == 0
        ? "no completion test is recorded"
        : $"{PassedTests.ToString(CultureInfo.InvariantCulture)} passed · " +
          $"{FailedTests.ToString(CultureInfo.InvariantCulture)} failed";
}

/// <summary>One completion test, as the page records it.</summary>
public sealed record OgCompletionTestValues(
    string TestType,
    decimal? TopDepth,
    decimal? BaseDepth,
    string? DepthUnit,
    string? Status,
    string? Result,
    string? ResultUnit,
    string? PerformedBy,
    DateTime? PerformedAt,
    string? EvidenceRef);

/// <summary>The decision retained for a retry: the submission, the decision and who made it.</summary>
public sealed record OgCompletionPendingDecision(
    string SubmissionId,
    OgCompletionDecision Decision,
    string Reviewer);

/// <summary>One decision, as the reviewer submits it.</summary>
public sealed record OgCompletionDecision(
    string Decision,
    string? Rationale,
    IReadOnlyList<string> Conditions,
    string? HandoverTo,
    DateTime? AcceptedAt = null)
{
    public const string Accept = "ACCEPT";
    public const string AcceptWithConditions = "ACCEPT_WITH_CONDITIONS";
    public const string Return = "RETURN";
    public const string Reject = "REJECT";

    public static IReadOnlyList<string> Decisions { get; } = [Accept, AcceptWithConditions, Return, Reject];

    public string ResultingState => Decision.Trim().ToUpperInvariant() switch
    {
        Accept => OgCompletionSubmission.Accepted,
        AcceptWithConditions => OgCompletionSubmission.AcceptedWithConditions,
        Return => OgCompletionSubmission.Returned,
        Reject => OgCompletionSubmission.Rejected,
        _ => string.Empty,
    };

    public bool Accepts =>
        string.Equals(ResultingState, OgCompletionSubmission.Accepted, StringComparison.OrdinalIgnoreCase)
        || string.Equals(ResultingState, OgCompletionSubmission.AcceptedWithConditions, StringComparison.OrdinalIgnoreCase);
}

/// <summary>The shared completion rules, stated once so the page, the service and the tests agree.</summary>
public static class CompletionRules
{
    public const string CodeTestTypeRequired = "completion-test-type-required";
    public const string CodeStatusInvalid = "completion-test-status-invalid";
    public const string CodeUnitRequired = "completion-test-unit-required";
    public const string CodeIntervalInvalid = "completion-test-interval-invalid";
    public const string CodeNoPassingTest = "completion-no-passing-test";
    public const string CodeAuthorCannotAccept = "completion-author-cannot-accept";
    public const string CodeConditionsRequired = "completion-conditions-required";
    public const string CodeHandoverRequired = "completion-handover-required";
    public const string CodeRationaleRequired = "completion-rationale-required";
    public const string CodeDecisionInvalid = "completion-decision-invalid";

    /// <summary>What a submission still needs: at least one test, and at least one evidence item (DEV-07 states).</summary>
    public static IReadOnlyList<string> SubmissionGaps(
        IReadOnlyList<OgCompletionTest> tests,
        IReadOnlyList<OgCompletionEvidence> evidence) =>
        SubmissionGaps(tests.Count, evidence.Count);

    /// <summary>The same rule over counts, for a caller that holds the entity rows.</summary>
    public static IReadOnlyList<string> SubmissionGaps(int testCount, int evidenceCount)
    {
        var gaps = new List<string>();

        if (testCount == 0)
        {
            gaps.Add("no completion test is recorded");
        }

        if (evidenceCount == 0)
        {
            gaps.Add("no evidence item is attached");
        }

        return gaps;
    }

    /// <summary>AC-5 and the interval rule: a test carries a result unit and a real interval.</summary>
    public static (string Code, string Message)? ValidateTest(OgCompletionTestValues values)
    {
        ArgumentNullException.ThrowIfNull(values);

        if (string.IsNullOrWhiteSpace(values.TestType))
        {
            return (CodeTestTypeRequired, "A completion test needs its test type.");
        }

        var status = (values.Status ?? string.Empty).Trim().ToUpperInvariant();

        if (!OgCompletionTest.Statuses.Contains(status, StringComparer.Ordinal))
        {
            return (CodeStatusInvalid,
                $"'{values.Status}' is not a test status; use {string.Join(" or ", OgCompletionTest.Statuses)}.");
        }

        if (values.TopDepth is < 0m || values.BaseDepth is < 0m)
        {
            return (CodeIntervalInvalid, "A test depth cannot be negative.");
        }

        if (values.TopDepth is { } top && values.BaseDepth is { } bottom && bottom <= top)
        {
            return (CodeIntervalInvalid,
                $"The interval ends at {bottom.ToString("0.##", CultureInfo.InvariantCulture)}, which is not below " +
                $"its top at {top.ToString("0.##", CultureInfo.InvariantCulture)}.");
        }

        if ((values.TopDepth is not null || values.BaseDepth is not null) && string.IsNullOrWhiteSpace(values.DepthUnit))
        {
            return (CodeUnitRequired, "A test interval needs the unit it is measured in (m or ft).");
        }

        // AC-5: every test states the unit of its result.
        if (string.IsNullOrWhiteSpace(values.ResultUnit))
        {
            return (CodeUnitRequired, "Every test needs the unit of its result (for example psi, bbl/d or %).");
        }

        return null;
    }

    /// <summary>The decision rules: AC-1, AC-3, AC-4 and the rationale/conditions requirements.</summary>
    public static (string Code, string Message)? ValidateDecision(
        OgCompletionSubmission submission,
        OgCompletionDecision decision,
        string reviewer,
        bool hasPassingTest)
    {
        ArgumentNullException.ThrowIfNull(submission);
        ArgumentNullException.ThrowIfNull(decision);

        if (string.IsNullOrWhiteSpace(reviewer))
        {
            return ("completion-reviewer-required", "A decision must record who made it.");
        }

        var normalised = (decision.Decision ?? string.Empty).Trim().ToUpperInvariant();

        if (!OgCompletionDecision.Decisions.Contains(normalised, StringComparer.Ordinal))
        {
            return (CodeDecisionInvalid,
                $"'{decision.Decision}' is not a completion decision; use {string.Join(", ", OgCompletionDecision.Decisions)}.");
        }

        if (!submission.IsSubmitted)
        {
            return ("completion-not-submitted",
                $"This submission is {submission.StateText}; only a submitted completion can be decided.");
        }

        // AC-3: the author cannot accept (or decide) their own submission.
        if (!string.IsNullOrWhiteSpace(submission.Author)
            && string.Equals(submission.Author.Trim(), reviewer.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            return (CodeAuthorCannotAccept,
                $"{reviewer.Trim()} submitted this completion, so they cannot decide it. Another reviewer must.");
        }

        // AC-1: acceptance needs at least one passed test record.
        if (decision.Accepts && !hasPassingTest)
        {
            return (CodeNoPassingTest,
                "The completion cannot be accepted: no test has a PASS result. Record a passing test first.");
        }

        if (decision.Accepts && string.IsNullOrWhiteSpace(decision.HandoverTo))
        {
            return (CodeHandoverRequired,
                "An acceptance hands the well to production, so it needs the recipient.");
        }

        if (normalised == OgCompletionDecision.AcceptWithConditions
            && (decision.Conditions ?? []).Count(condition => !string.IsNullOrWhiteSpace(condition)) == 0)
        {
            return (CodeConditionsRequired, "An acceptance with conditions needs at least one condition.");
        }

        if (normalised is OgCompletionDecision.Return or OgCompletionDecision.Reject
            && string.IsNullOrWhiteSpace(decision.Rationale))
        {
            return (CodeRationaleRequired,
                $"A {normalised.ToLowerInvariant()} decision needs a rationale.");
        }

        return null;
    }
}

/// <summary>DEV-07: completion tests, the submission and the independent acceptance.</summary>
public interface IOgWellCompletion
{
    Task<OgResult<OgCompletionWorkspace>> LoadAsync(
        string? fieldId,
        string fieldName,
        string? wellId = null,
        CancellationToken cancellationToken = default);

    Task<OgResult<OgCompletionWorkspace>> RecordTestAsync(
        string wellId,
        OgCompletionTestValues values,
        string actor,
        CancellationToken cancellationToken = default);

    Task<OgResult<OgCompletionWorkspace>> CreateSubmissionAsync(
        string wellId,
        string? summary,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>Attaches evidence; an item naming another well is refused (AC-2).</summary>
    Task<OgResult<OgCompletionWorkspace>> AddEvidenceAsync(
        string submissionId,
        string evidenceWellId,
        string? document,
        string? reference,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>Submits a completion that has at least one test and one evidence item.</summary>
    Task<OgResult<OgCompletionWorkspace>> SubmitAsync(
        string submissionId,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>The independent decision; an acceptance carries its conditions and the handover recipient (AC-1..AC-4).</summary>
    Task<OgResult<OgCompletionWorkspace>> DecideAsync(
        string submissionId,
        OgCompletionDecision decision,
        string reviewer,
        CancellationToken cancellationToken = default);

    /// <summary>The decision that was not acknowledged, or null (AC-6).</summary>
    OgCompletionPendingDecision? Pending { get; }

    /// <summary>Re-sends the retained decision; an acceptance already recorded is returned, not written twice (AC-6).</summary>
    Task<OgResult<OgCompletionWorkspace>> RetryDecisionAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// The completion over the DrillingAndConstruction-owned <c>DEVELOPMENT_COMPLETION_TEST</c>,
/// <c>DEVELOPMENT_COMPLETION_SUBMISSION</c> and <c>DEVELOPMENT_COMPLETION_EVIDENCE</c>, with the wells taken from the
/// drilling executions of the field, all through a unit of work.
/// </summary>
public sealed class OgWellCompletionService : IOgWellCompletion
{
    public const string CodeNoConnection = "completion-no-connection";
    public const string CodeReadFailed = "completion-read-failed";
    public const string CodeWriteFailed = "completion-write-failed";
    public const string CodeActorRequired = "completion-actor-required";
    public const string CodeUnknown = "completion-unknown";
    public const string CodeWellRequired = "completion-well-required";
    public const string CodeWellUnknown = "completion-well-unknown";
    public const string CodeEvidenceForeignWell = "completion-evidence-foreign-well";
    public const string CodeEvidenceRequired = "completion-evidence-required";
    public const string CodeLocked = "completion-locked";
    public const string CodeAlreadySubmitted = "completion-already-submitted";
    public const string CodeNothingPending = "completion-nothing-pending";

    private readonly Func<IDataSource?> _source;
    private readonly IFailureReporter _failures;

    public OgWellCompletionService(Func<IDataSource?> source, IFailureReporter failures)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _failures = failures ?? throw new ArgumentNullException(nameof(failures));
    }

    public OgCompletionPendingDecision? Pending { get; private set; }

    public async Task<OgResult<OgCompletionWorkspace>> LoadAsync(
        string? fieldId,
        string fieldName,
        string? wellId = null,
        CancellationToken cancellationToken = default)
    {
        var source = _source();

        if (source is null)
        {
            return OgResult<OgCompletionWorkspace>.Failed(
                "No connection is open, so completion cannot be read.", CodeNoConnection);
        }

        try
        {
            var model = await ReadAsync(source, cancellationToken).ConfigureAwait(false);
            return OgResult<OgCompletionWorkspace>.Ok(Build(model, fieldId, fieldName, wellId));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                "reading the well completion",
                "completion is not shown",
                FailureSeverity.Error);

            return OgResult<OgCompletionWorkspace>.Failed(
                "The completion could not be read.", reference ?? CodeReadFailed);
        }
    }

    public async Task<OgResult<OgCompletionWorkspace>> RecordTestAsync(
        string wellId,
        OgCompletionTestValues values,
        string actor,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(values);

        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgCompletionWorkspace>.Failed(
                "A completion test must record who took it.", CodeActorRequired);
        }

        if (string.IsNullOrWhiteSpace(wellId))
        {
            return OgResult<OgCompletionWorkspace>.Failed("Choose the well before recording a test.", CodeWellRequired);
        }

        if (CompletionRules.ValidateTest(values) is { } refusal)
        {
            return OgResult<OgCompletionWorkspace>.Failed(refusal.Message, refusal.Code);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgCompletionWorkspace>.Failed(
                "No connection is open, so the test cannot be recorded.", CodeNoConnection);
        }

        try
        {
            var model = await ReadAsync(source, cancellationToken).ConfigureAwait(false);
            var well = ResolveWell(model, wellId);

            if (well is null)
            {
                return OgResult<OgCompletionWorkspace>.Failed(
                    $"Well '{wellId}' has no drilling execution in this database.", CodeWellUnknown);
            }

            var now = DateTime.UtcNow;
            var unit = OgUow.For<DEVELOPMENT_COMPLETION_TEST>(source);
            unit.PrimaryKey = "TEST_ID";

            unit.Add(new DEVELOPMENT_COMPLETION_TEST
            {
                TEST_ID = Guid.NewGuid().ToString("N"),
                WELL_ID = well.WellId,
                PROGRAM_ID = well.ProgramId,
                TEST_TYPE = values.TestType.Trim().ToUpperInvariant(),
                TOP_DEPTH = values.TopDepth ?? 0m,
                BASE_DEPTH = values.BaseDepth ?? 0m,
                DEPTH_OUOM = Blank(values.DepthUnit)?.Trim(),
                STATUS = values.Status!.Trim().ToUpperInvariant(),
                RESULT = Blank(values.Result),
                RESULT_OUOM = values.ResultUnit!.Trim(),
                PERFORMED_BY = Blank(values.PerformedBy) ?? actor.Trim(),
                PERFORMED_AT = values.PerformedAt ?? now,
                EVIDENCE_REF = Blank(values.EvidenceRef),
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
                return OgResult<OgCompletionWorkspace>.Failed(
                    OgUow.Message(committed, "The test was not recorded."), CodeWriteFailed);
            }

            var after = await ReadAsync(source, cancellationToken).ConfigureAwait(false);
            return OgResult<OgCompletionWorkspace>.Ok(Build(after, null, string.Empty, well.WellId));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                $"recording a completion test on well {wellId}",
                "the test was not recorded",
                FailureSeverity.Error);

            return OgResult<OgCompletionWorkspace>.Failed(
                "The test could not be recorded.", reference ?? CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgCompletionWorkspace>> CreateSubmissionAsync(
        string wellId,
        string? summary,
        string actor,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgCompletionWorkspace>.Failed(
                "A submission must record who prepared it.", CodeActorRequired);
        }

        if (string.IsNullOrWhiteSpace(wellId))
        {
            return OgResult<OgCompletionWorkspace>.Failed("Choose the well before submitting it.", CodeWellRequired);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgCompletionWorkspace>.Failed(
                "No connection is open, so the submission cannot be created.", CodeNoConnection);
        }

        try
        {
            var model = await ReadAsync(source, cancellationToken).ConfigureAwait(false);
            var well = ResolveWell(model, wellId);

            if (well is null)
            {
                return OgResult<OgCompletionWorkspace>.Failed(
                    $"Well '{wellId}' has no drilling execution in this database.", CodeWellUnknown);
            }

            var open = model.Submissions.FirstOrDefault(row =>
                string.Equals(row.WELL_ID, well.WellId, StringComparison.OrdinalIgnoreCase)
                && (string.Equals(row.STATE, OgCompletionSubmission.Draft, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(row.STATE, OgCompletionSubmission.Submitted, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(row.STATE, OgCompletionSubmission.Returned, StringComparison.OrdinalIgnoreCase)));

            if (open is not null)
            {
                return OgResult<OgCompletionWorkspace>.Failed(
                    $"Well '{well.WellId}' already has a completion submission in {open.STATE.ToLowerInvariant()}.",
                    CodeAlreadySubmitted);
            }

            var now = DateTime.UtcNow;
            var submissionId = Guid.NewGuid().ToString("N");

            var unit = OgUow.For<DEVELOPMENT_COMPLETION_SUBMISSION>(source);
            unit.PrimaryKey = "SUBMISSION_ID";

            unit.Add(new DEVELOPMENT_COMPLETION_SUBMISSION
            {
                SUBMISSION_ID = submissionId,
                WELL_ID = well.WellId,
                PROGRAM_ID = well.ProgramId,
                SUMMARY = Blank(summary),
                STATE = OgCompletionSubmission.Draft,
                AUTHOR = actor.Trim(),
                ROW_CREATED_BY = actor.Trim(),
                ROW_CREATED_DATE = now,
                ROW_CHANGED_BY = actor.Trim(),
                ROW_CHANGED_DATE = now,
            });

            var committed = await unit.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(committed))
            {
                return OgResult<OgCompletionWorkspace>.Failed(
                    OgUow.Message(committed, "The submission was not created."), CodeWriteFailed);
            }

            var after = await ReadAsync(source, cancellationToken).ConfigureAwait(false);
            return OgResult<OgCompletionWorkspace>.Ok(Build(after, null, string.Empty, well.WellId, submissionId));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                $"creating a completion submission for well {wellId}",
                "the submission was not created",
                FailureSeverity.Error);

            return OgResult<OgCompletionWorkspace>.Failed(
                "The submission could not be created.", reference ?? CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgCompletionWorkspace>> AddEvidenceAsync(
        string submissionId,
        string evidenceWellId,
        string? document,
        string? reference,
        string actor,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgCompletionWorkspace>.Failed(
                "Evidence must record who attached it.", CodeActorRequired);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgCompletionWorkspace>.Failed(
                "No connection is open, so the evidence cannot be attached.", CodeNoConnection);
        }

        try
        {
            var model = await ReadAsync(source, cancellationToken).ConfigureAwait(false);
            var submission = model.Submissions.FirstOrDefault(row =>
                string.Equals(row.SUBMISSION_ID, submissionId.Trim(), StringComparison.OrdinalIgnoreCase));

            if (submission is null)
            {
                return OgResult<OgCompletionWorkspace>.Failed(
                    $"Submission '{submissionId}' is not in this database.", CodeUnknown);
            }

            if (!string.Equals(submission.STATE, OgCompletionSubmission.Draft, StringComparison.OrdinalIgnoreCase)
                && !string.Equals(submission.STATE, OgCompletionSubmission.Returned, StringComparison.OrdinalIgnoreCase))
            {
                return OgResult<OgCompletionWorkspace>.Failed(
                    $"This submission is {submission.STATE!.ToLowerInvariant()}; evidence is added to a draft.",
                    CodeLocked);
            }

            // AC-2: evidence that names another well is refused, not filed against this one.
            if (!string.Equals(
                    (evidenceWellId ?? string.Empty).Trim(),
                    submission.WELL_ID.Trim(),
                    StringComparison.OrdinalIgnoreCase))
            {
                return OgResult<OgCompletionWorkspace>.Failed(
                    $"The evidence belongs to well '{evidenceWellId}', but this submission is for " +
                    $"'{submission.WELL_ID}'. Attach evidence for the same well.",
                    CodeEvidenceForeignWell);
            }

            if (string.IsNullOrWhiteSpace(document) && string.IsNullOrWhiteSpace(reference))
            {
                return OgResult<OgCompletionWorkspace>.Failed(
                    "An evidence item needs a document or a reference.", CodeEvidenceRequired);
            }

            var now = DateTime.UtcNow;
            var unit = OgUow.For<DEVELOPMENT_COMPLETION_EVIDENCE>(source);
            unit.PrimaryKey = "EVIDENCE_ID";

            unit.Add(new DEVELOPMENT_COMPLETION_EVIDENCE
            {
                EVIDENCE_ID = Guid.NewGuid().ToString("N"),
                SUBMISSION_ID = submission.SUBMISSION_ID!,
                WELL_ID = submission.WELL_ID,
                DOCUMENT = Blank(document),
                REFERENCE = Blank(reference),
                ADDED_BY = actor.Trim(),
                ADDED_AT = now,
                ROW_CREATED_BY = actor.Trim(),
                ROW_CREATED_DATE = now,
                ROW_CHANGED_BY = actor.Trim(),
                ROW_CHANGED_DATE = now,
            });

            var committed = await unit.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(committed))
            {
                return OgResult<OgCompletionWorkspace>.Failed(
                    OgUow.Message(committed, "The evidence was not attached."), CodeWriteFailed);
            }

            var after = await ReadAsync(source, cancellationToken).ConfigureAwait(false);
            return OgResult<OgCompletionWorkspace>.Ok(
                Build(after, null, string.Empty, submission.WELL_ID, submission.SUBMISSION_ID));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var failure = _failures.ReportHandled(
                ex,
                $"attaching evidence to completion {submissionId}",
                "the evidence was not attached",
                FailureSeverity.Error);

            return OgResult<OgCompletionWorkspace>.Failed(
                "The evidence could not be attached.", failure ?? CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgCompletionWorkspace>> SubmitAsync(
        string submissionId,
        string actor,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgCompletionWorkspace>.Failed("A submit must record who sent it.", CodeActorRequired);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgCompletionWorkspace>.Failed(
                "No connection is open, so the completion cannot be submitted.", CodeNoConnection);
        }

        try
        {
            var model = await ReadAsync(source, cancellationToken).ConfigureAwait(false);
            var submission = model.Submissions.FirstOrDefault(row =>
                string.Equals(row.SUBMISSION_ID, submissionId.Trim(), StringComparison.OrdinalIgnoreCase));

            if (submission is null)
            {
                return OgResult<OgCompletionWorkspace>.Failed(
                    $"Submission '{submissionId}' is not in this database.", CodeUnknown);
            }

            if (string.Equals(submission.STATE, OgCompletionSubmission.Submitted, StringComparison.OrdinalIgnoreCase))
            {
                return OgResult<OgCompletionWorkspace>.Failed(
                    "This completion is already submitted.", CodeAlreadySubmitted);
            }

            if (!string.Equals(submission.STATE, OgCompletionSubmission.Draft, StringComparison.OrdinalIgnoreCase)
                && !string.Equals(submission.STATE, OgCompletionSubmission.Returned, StringComparison.OrdinalIgnoreCase))
            {
                return OgResult<OgCompletionWorkspace>.Failed(
                    $"This completion is {submission.STATE!.ToLowerInvariant()} and cannot be submitted again.",
                    CodeLocked);
            }

            var tests = model.Tests
                .Where(row => string.Equals(row.WELL_ID, submission.WELL_ID, StringComparison.OrdinalIgnoreCase))
                .ToList();

            var evidence = model.Evidence
                .Where(row => string.Equals(row.SUBMISSION_ID, submission.SUBMISSION_ID, StringComparison.OrdinalIgnoreCase))
                .ToList();

            var gaps = CompletionRules.SubmissionGaps(tests.Count, evidence.Count);

            if (gaps.Count > 0)
            {
                return OgResult<OgCompletionWorkspace>.Failed(
                    "The completion cannot be submitted: " + string.Join("; ", gaps) + ".",
                    evidence.Count == 0 ? CodeEvidenceRequired : "completion-tests-required");
            }

            var now = DateTime.UtcNow;
            var unit = OgUow.For<DEVELOPMENT_COMPLETION_SUBMISSION>(source);
            unit.PrimaryKey = "SUBMISSION_ID";

            var rows = await OgUow.ReadAsync(unit, [OgUow.Equal("SUBMISSION_ID", submission.SUBMISSION_ID)],
                cancellationToken).ConfigureAwait(false);
            var row = rows.FirstOrDefault();

            if (row is null)
            {
                return OgResult<OgCompletionWorkspace>.Failed(
                    $"Submission '{submissionId}' is not in this database.", CodeUnknown);
            }

            row.STATE = OgCompletionSubmission.Submitted;
            row.SUBMITTED_AT = now;
            row.ROW_CHANGED_BY = actor.Trim();
            row.ROW_CHANGED_DATE = now;

            var updated = unit.Update(row);

            if (!OgUow.IsOk(updated))
            {
                return OgResult<OgCompletionWorkspace>.Failed(
                    OgUow.Message(updated, "The completion was not submitted."), CodeWriteFailed);
            }

            var committed = await unit.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(committed))
            {
                return OgResult<OgCompletionWorkspace>.Failed(
                    OgUow.Message(committed, "The completion was not submitted."), CodeWriteFailed);
            }

            var after = await ReadAsync(source, cancellationToken).ConfigureAwait(false);
            return OgResult<OgCompletionWorkspace>.Ok(
                Build(after, null, string.Empty, row.WELL_ID, row.SUBMISSION_ID));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                $"submitting completion {submissionId}",
                "the completion was not submitted",
                FailureSeverity.Error);

            return OgResult<OgCompletionWorkspace>.Failed(
                "The completion could not be submitted.", reference ?? CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgCompletionWorkspace>> DecideAsync(
        string submissionId,
        OgCompletionDecision decision,
        string reviewer,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(decision);

        var source = _source();

        if (source is null)
        {
            return OgResult<OgCompletionWorkspace>.Failed(
                "No connection is open, so the decision cannot be recorded.", CodeNoConnection);
        }

        try
        {
            var model = await ReadAsync(source, cancellationToken).ConfigureAwait(false);
            var submission = model.Submissions.FirstOrDefault(row =>
                string.Equals(row.SUBMISSION_ID, submissionId.Trim(), StringComparison.OrdinalIgnoreCase));

            if (submission is null)
            {
                return OgResult<OgCompletionWorkspace>.Failed(
                    $"Submission '{submissionId}' is not in this database.", CodeUnknown);
            }

            var hasPassingTest = model.Tests
                .Where(row => string.Equals(row.WELL_ID, submission.WELL_ID, StringComparison.OrdinalIgnoreCase))
                .Any(row => string.Equals(row.STATUS, OgCompletionTest.Passed, StringComparison.OrdinalIgnoreCase));

            var state = Blank(submission.STATE) ?? OgCompletionSubmission.Draft;
            var alreadyAccepted =
                string.Equals(state, OgCompletionSubmission.Accepted, StringComparison.OrdinalIgnoreCase)
                || string.Equals(state, OgCompletionSubmission.AcceptedWithConditions, StringComparison.OrdinalIgnoreCase);

            // AC-6: an acceptance that was already recorded by this reviewer is returned, never written a second time.
            if (alreadyAccepted
                && decision.Accepts
                && string.Equals(Blank(submission.REVIEWER), reviewer.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                return OgResult<OgCompletionWorkspace>.Ok(
                    Build(model, null, string.Empty, submission.WELL_ID, submission.SUBMISSION_ID));
            }

            var refusal = CompletionRules.ValidateDecision(
                new OgCompletionSubmission(
                    submission.SUBMISSION_ID ?? string.Empty,
                    submission.WELL_ID,
                    Blank(submission.PROGRAM_ID),
                    Blank(submission.SUMMARY),
                    state,
                    Blank(submission.AUTHOR),
                    submission.SUBMITTED_AT,
                    Blank(submission.REVIEWER),
                    Blank(submission.RATIONALE),
                    Split(submission.CONDITIONS_TEXT),
                    submission.ACCEPTED_AT,
                    Blank(submission.ACCEPTED_BY),
                    Blank(submission.HANDOVER_TO),
                    submission.HANDOVER_AT),
                decision,
                reviewer,
                hasPassingTest);

            if (refusal is { } reason)
            {
                return OgResult<OgCompletionWorkspace>.Failed(reason.Message, reason.Code);
            }

            var now = DateTime.UtcNow;
            var decidedState = decision.ResultingState;
            var conditions = (decision.Conditions ?? [])
                .Where(condition => !string.IsNullOrWhiteSpace(condition))
                .Select(condition => condition.Trim())
                .ToList();

            var unit = OgUow.For<DEVELOPMENT_COMPLETION_SUBMISSION>(source);
            unit.PrimaryKey = "SUBMISSION_ID";

            var rows = await OgUow.ReadAsync(unit, [OgUow.Equal("SUBMISSION_ID", submission.SUBMISSION_ID)],
                cancellationToken).ConfigureAwait(false);
            var row = rows.FirstOrDefault();

            if (row is null)
            {
                return OgResult<OgCompletionWorkspace>.Failed(
                    $"Submission '{submissionId}' is not in this database.", CodeUnknown);
            }

            row.STATE = decidedState;
            row.REVIEWER = reviewer.Trim();
            row.RATIONALE = Blank(decision.Rationale);
            row.CONDITIONS_TEXT = conditions.Count == 0 ? null : string.Join("\n", conditions);
            row.ROW_CHANGED_BY = reviewer.Trim();
            row.ROW_CHANGED_DATE = now;

            if (decision.Accepts)
            {
                // AC-4: the acceptance record carries its conditions and the handover recipient on this one row.
                row.ACCEPTED_AT = decision.AcceptedAt?.Date ?? now;
                row.ACCEPTED_BY = reviewer.Trim();
                row.HANDOVER_TO = Blank(decision.HandoverTo)?.Trim();
                row.HANDOVER_AT = decision.AcceptedAt?.Date ?? now;
            }

            var updated = unit.Update(row);

            if (!OgUow.IsOk(updated))
            {
                Pending = new OgCompletionPendingDecision(submission.SUBMISSION_ID!, decision, reviewer.Trim());
                return OgResult<OgCompletionWorkspace>.Failed(
                    OgUow.Message(updated, "The decision was not acknowledged; retry sends the same decision."),
                    CodeWriteFailed);
            }

            var committed = await unit.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(committed))
            {
                Pending = new OgCompletionPendingDecision(submission.SUBMISSION_ID!, decision, reviewer.Trim());
                return OgResult<OgCompletionWorkspace>.Failed(
                    OgUow.Message(committed, "The decision was not acknowledged; retry sends the same decision."),
                    CodeWriteFailed);
            }

            Pending = null;

            var after = await ReadAsync(source, cancellationToken).ConfigureAwait(false);
            return OgResult<OgCompletionWorkspace>.Ok(
                Build(after, null, string.Empty, row.WELL_ID, row.SUBMISSION_ID));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            Pending = new OgCompletionPendingDecision(submissionId.Trim(), decision, reviewer.Trim());
            var reference = _failures.ReportHandled(
                ex,
                $"recording the completion decision on {submissionId}",
                "the decision was not recorded; the original is retained for retry",
                FailureSeverity.Error);

            return OgResult<OgCompletionWorkspace>.Failed(
                "The decision could not be recorded; retry sends the same decision.", reference ?? CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgCompletionWorkspace>> RetryDecisionAsync(
        CancellationToken cancellationToken = default)
    {
        if (Pending is not { } pending)
        {
            return OgResult<OgCompletionWorkspace>.Failed(
                "There is no decision waiting to be retried.", CodeNothingPending);
        }

        return await DecideAsync(pending.SubmissionId, pending.Decision, pending.Reviewer, cancellationToken)
            .ConfigureAwait(false);
    }

    // ------------------------------------------------------------------ reads

    private static async Task<CompletionModel> ReadAsync(IDataSource source, CancellationToken cancellationToken)
    {
        var executions = await OgUow.ReadAllAsync<DEVELOPMENT_DRILLING_EXECUTION>(source).ConfigureAwait(false);
        var tests = await OgUow.ReadAllAsync<DEVELOPMENT_COMPLETION_TEST>(source).ConfigureAwait(false);
        var submissions = await OgUow.ReadAllAsync<DEVELOPMENT_COMPLETION_SUBMISSION>(source).ConfigureAwait(false);
        var evidence = await OgUow.ReadAllAsync<DEVELOPMENT_COMPLETION_EVIDENCE>(source).ConfigureAwait(false);

        cancellationToken.ThrowIfCancellationRequested();

        return new CompletionModel(executions, tests, submissions, evidence);
    }

    private static CompletionWell? ResolveWell(CompletionModel model, string wellId) =>
        model.Executions
            .Where(row => !string.IsNullOrWhiteSpace(row.WELL_ID))
            .GroupBy(row => row.WELL_ID!, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.OrderByDescending(row => row.SPUD_DATE).First())
            .Where(row => string.Equals(row.WELL_ID, wellId.Trim(), StringComparison.OrdinalIgnoreCase))
            .Select(row => new CompletionWell(row.WELL_ID!, Blank(row.PROGRAM_ID), Blank(row.STATUS)))
            .FirstOrDefault();

    private static OgCompletionWorkspace Build(
        CompletionModel model,
        string? fieldId,
        string fieldName,
        string? wellId,
        string? submissionId = null)
    {
        var wells = model.Executions
            .Where(row => !string.IsNullOrWhiteSpace(row.WELL_ID))
            .Where(row => FieldMatches(row.FIELD_ID, fieldId, fieldName))
            .GroupBy(row => row.WELL_ID!, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.OrderByDescending(row => row.SPUD_DATE).First())
            .OrderBy(row => row.WELL_ID, StringComparer.OrdinalIgnoreCase)
            .Select(row => new OgCompletionWell(row.WELL_ID!, Blank(row.PROGRAM_ID), Blank(row.STATUS)))
            .ToList();

        var selectedWell = wells.FirstOrDefault(well =>
                                !string.IsNullOrWhiteSpace(wellId)
                                && string.Equals(well.WellId, wellId.Trim(), StringComparison.OrdinalIgnoreCase))
                            ?? wells.FirstOrDefault();

        if (selectedWell is null)
        {
            return new OgCompletionWorkspace(
                string.IsNullOrWhiteSpace(fieldName) ? "(no field selected)" : fieldName.Trim(),
                wells, null, [], [], null, [], null);
        }

        var tests = model.Tests
            .Where(row => string.Equals(row.WELL_ID, selectedWell.WellId, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(row => row.PERFORMED_AT)
            .Select(row => new OgCompletionTest(
                row.TEST_ID ?? string.Empty,
                row.WELL_ID ?? string.Empty,
                Blank(row.PROGRAM_ID),
                Blank(row.TEST_TYPE) ?? "(no type)",
                row.TOP_DEPTH > 0m || row.BASE_DEPTH > 0m ? row.TOP_DEPTH : null,
                row.BASE_DEPTH > 0m ? row.BASE_DEPTH : null,
                Blank(row.DEPTH_OUOM),
                Blank(row.STATUS) ?? string.Empty,
                Blank(row.RESULT),
                Blank(row.RESULT_OUOM),
                Blank(row.PERFORMED_BY),
                row.PERFORMED_AT,
                Blank(row.EVIDENCE_REF)))
            .ToList();

        var submissions = model.Submissions
            .Where(row => string.Equals(row.WELL_ID, selectedWell.WellId, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(row => row.ROW_CREATED_DATE)
            .Select(row => new OgCompletionSubmission(
                row.SUBMISSION_ID ?? string.Empty,
                row.WELL_ID ?? string.Empty,
                Blank(row.PROGRAM_ID),
                Blank(row.SUMMARY),
                Blank(row.STATE) ?? OgCompletionSubmission.Draft,
                Blank(row.AUTHOR),
                row.SUBMITTED_AT,
                Blank(row.REVIEWER),
                Blank(row.RATIONALE),
                Split(row.CONDITIONS_TEXT),
                row.ACCEPTED_AT,
                Blank(row.ACCEPTED_BY),
                Blank(row.HANDOVER_TO),
                row.HANDOVER_AT))
            .ToList();

        var selected = submissions.FirstOrDefault(submission =>
                           !string.IsNullOrWhiteSpace(submissionId)
                           && string.Equals(submission.Id, submissionId!.Trim(), StringComparison.OrdinalIgnoreCase))
                       ?? submissions.FirstOrDefault()
                       ?? null;

        var evidence = selected is null
            ? []
            : model.Evidence
                .Where(row => string.Equals(row.SUBMISSION_ID, selected.Id, StringComparison.OrdinalIgnoreCase))
                .OrderBy(row => row.ADDED_AT)
                .Select(row => new OgCompletionEvidence(
                    row.EVIDENCE_ID ?? string.Empty,
                    row.SUBMISSION_ID ?? string.Empty,
                    row.WELL_ID ?? string.Empty,
                    Blank(row.DOCUMENT),
                    Blank(row.REFERENCE),
                    Blank(row.ADDED_BY),
                    row.ADDED_AT))
                .ToList();

        return new OgCompletionWorkspace(
            string.IsNullOrWhiteSpace(fieldName) ? "(no field selected)" : fieldName.Trim(),
            wells,
            selectedWell.WellId,
            tests,
            submissions,
            selected,
            evidence,
            selected?.Handover);
    }

    private static IReadOnlyList<string> Split(string? text) =>
        string.IsNullOrWhiteSpace(text)
            ? []
            : text.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    private static bool FieldMatches(string? value, string? fieldId, string? fieldName) =>
        (string.IsNullOrWhiteSpace(fieldId) && string.IsNullOrWhiteSpace(fieldName))
        || (!string.IsNullOrWhiteSpace(fieldId)
            && string.Equals(value ?? string.Empty, fieldId.Trim(), StringComparison.OrdinalIgnoreCase))
        || (!string.IsNullOrWhiteSpace(fieldName)
            && string.Equals(value ?? string.Empty, fieldName.Trim(), StringComparison.OrdinalIgnoreCase));

    private static string? Blank(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value;

    private sealed record CompletionWell(string WellId, string? ProgramId, string? Status);

    private sealed record CompletionModel(
        IReadOnlyList<DEVELOPMENT_DRILLING_EXECUTION> Executions,
        IReadOnlyList<DEVELOPMENT_COMPLETION_TEST> Tests,
        IReadOnlyList<DEVELOPMENT_COMPLETION_SUBMISSION> Submissions,
        IReadOnlyList<DEVELOPMENT_COMPLETION_EVIDENCE> Evidence);
}
