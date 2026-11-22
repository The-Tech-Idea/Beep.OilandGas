using System.Globalization;
using Beep.OilandGas.LifeCycle.Data.Tables;
using Beep.OilandGas.Models.Data.ProspectIdentification;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Data.Exploration;

/// <summary>One recorded gate decision on a well program (EXP-05).</summary>
public sealed record OgWellProgramHistoryEntry(
    string Decision,
    string ResultingState,
    string? Rationale,
    IReadOnlyList<string> Conditions,
    string Actor,
    DateTimeOffset? DecidedAt);

/// <summary>
/// A well program and the gate decision on it.
/// </summary>
/// <param name="ProspectRiskComplete">
/// AC-2: true only when the prospect's five risk elements are all recorded, read through the same rule the
/// prospect board and editor use.
/// </param>
/// <param name="Version">The row version the program was read at; a decision is guarded on it (AC-4).</param>
public sealed record OgWellProgram(
    string Id,
    string? ProspectId,
    string? ProspectName,
    string Name,
    string? Objective,
    decimal? TargetDepth,
    string? DepthUnit,
    decimal? EstimatedCost,
    string? Currency,
    string State,
    string? Author,
    DateTimeOffset? SubmittedAt,
    int? AgeDays,
    string? DecidedBy,
    DateTimeOffset? DecidedAt,
    string? Rationale,
    IReadOnlyList<string> Conditions,
    bool ProspectRiskComplete,
    int Version,
    IReadOnlyList<OgWellProgramHistoryEntry> History)
{
    /// <summary>States the gate can be decided from.</summary>
    public const string StatePending = "PENDING";

    public bool IsPending => string.Equals(State, StatePending, StringComparison.OrdinalIgnoreCase);

    /// <summary>AC-5: conditions travel on the approved program as its constraints.</summary>
    public bool HasConditions => Conditions.Count > 0;

    public string StateText => string.IsNullOrWhiteSpace(State) ? "(no state)" : State;

    public string RiskText => ProspectRiskComplete
        ? "prospect risk complete"
        : "prospect risk incomplete - the gate cannot approve it";

    public string CostText => EstimatedCost is { } cost
        ? $"{cost.ToString("#,0.###", CultureInfo.InvariantCulture)} {Currency ?? string.Empty}".TrimEnd()
        : "not recorded";

    public string DepthText => TargetDepth is { } depth
        ? $"{depth.ToString("0.###", CultureInfo.InvariantCulture)} {DepthUnit ?? string.Empty}".TrimEnd()
        : "not recorded";

    public string AgeText => AgeDays is { } days
        ? $"{days.ToString(CultureInfo.InvariantCulture)} d"
        : "-";

    public string Describe() =>
        string.Join(
            " · ",
            new[]
            {
                Name,
                ProspectName ?? ProspectId,
                StateText,
                $"author {Author ?? "(unknown)"}",
                CostText,
            }.Where(part => !string.IsNullOrWhiteSpace(part)));
}

/// <summary>One decision a person makes on a program.</summary>
public sealed record OgWellProgramDecision(
    string Decision,
    string? Rationale,
    IReadOnlyList<string> Conditions,
    string Actor,
    DateTimeOffset? DecidedAt = null)
{
    public const string Approve = "APPROVE";
    public const string ApproveWithConditions = "APPROVE_WITH_CONDITIONS";
    public const string Return = "RETURN";
    public const string Reject = "REJECT";

    public static IReadOnlyList<string> Decisions { get; } = [Approve, ApproveWithConditions, Return, Reject];

    /// <summary>The program state this decision produces.</summary>
    public string ResultingState => Decision.Trim().ToUpperInvariant() switch
    {
        Approve => "APPROVED",
        ApproveWithConditions => "APPROVED_WITH_CONDITIONS",
        Return => "RETURNED",
        Reject => "REJECTED",
        _ => string.Empty,
    };
}

/// <summary>EXP-05: the queue of well programs and the gate decision on each.</summary>
public interface IOgWellProgramApprovals
{
    Task<OgResult<IReadOnlyList<OgWellProgram>>> LoadQueueAsync(
        string? fieldId,
        string fieldName,
        DateTime asOf,
        CancellationToken cancellationToken = default);

    Task<OgResult<OgWellProgram>> LoadAsync(
        string programId,
        DateTime asOf,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Records the decision. The rules are checked before anything is written: the decision must be known, the
    /// actor must not be the author, the prospect's risk must be complete, the program must still be pending, and a
    /// rationale (and conditions, for an approval with conditions) must be present (AC-1..AC-4).
    /// </summary>
    Task<OgResult<OgWellProgram>> DecideAsync(
        OgWellProgram program,
        OgWellProgramDecision decision,
        CancellationToken cancellationToken = default);

    /// <summary>AC-6: the decision that was not acknowledged, or null.</summary>
    OgWellProgramDecision? Pending { get; }

    /// <summary>AC-6: re-sends the retained decision, unchanged.</summary>
    Task<OgResult<OgWellProgram>> RetryAsync(DateTime asOf, CancellationToken cancellationToken = default);
}

/// <summary>
/// The gate over <c>WELL_PROGRAM</c> and <c>WELL_PROGRAM_DECISION</c>, with the prospect's risk read from
/// <c>PROSPECT_RISK_ASSESSMENT</c>, all through their entity models and a unit of work.
/// </summary>
/// <remarks>
/// The generic approval engine (WFL-04, <c>ApprovalWorkflowEngine</c>) is not wired into this build, so delegation,
/// escalation and quorum are not available; this service records the gate's own decision, its conditions and its
/// history in the owning module's tables. When WFL-04 lands it should become the recorder behind this interface.
/// </remarks>
public sealed class OgWellProgramApprovals : IOgWellProgramApprovals
{
    public const string CodeNoConnection = "well-program-no-connection";
    public const string CodeReadFailed = "well-program-read-failed";
    public const string CodeWriteFailed = "well-program-write-failed";
    public const string CodeUnknown = "well-program-unknown";
    public const string CodeActorRequired = "well-program-actor-required";
    public const string CodeDecisionRequired = "well-program-decision-required";
    public const string CodeRationaleRequired = "well-program-rationale-required";
    public const string CodeConditionsRequired = "well-program-conditions-required";
    public const string CodeConditionsNotAllowed = "well-program-conditions-not-allowed";
    public const string CodeAuthorCannotDecide = "well-program-author-cannot-decide";
    public const string CodeRiskIncomplete = "well-program-risk-incomplete";
    public const string CodeNotPending = "well-program-not-pending";
    public const string CodeConflict = "well-program-conflict";
    public const string CodeNothingPending = "well-program-nothing-pending";

    private readonly Func<IDataSource?> _source;
    private readonly IFailureReporter _failures;

    public OgWellProgramApprovals(Func<IDataSource?> source, IFailureReporter failures)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _failures = failures ?? throw new ArgumentNullException(nameof(failures));
    }

    public OgWellProgramDecision? Pending { get; private set; }

    public async Task<OgResult<IReadOnlyList<OgWellProgram>>> LoadQueueAsync(
        string? fieldId,
        string fieldName,
        DateTime asOf,
        CancellationToken cancellationToken = default)
    {
        var source = _source();

        if (source is null)
        {
            return OgResult<IReadOnlyList<OgWellProgram>>.Failed(
                "No connection is open, so the well programs cannot be read.", CodeNoConnection);
        }

        try
        {
            var context = await ReadContextAsync(source, cancellationToken).ConfigureAwait(false);

            // Pending first, then the decisions already made, so the queue is what a person opens.
            var programs = context.Programs
                .Where(program => FieldMatches(program.FIELD_ID, fieldId, fieldName))
                .OrderBy(program => IsPending(program) ? 0 : 1)
                .ThenByDescending(program => program.SUBMITTED_AT)
                .ThenBy(program => program.PROGRAM_ID, StringComparer.Ordinal)
                .Select(program => ToProgram(program, asOf.Date, context))
                .ToList();

            return OgResult<IReadOnlyList<OgWellProgram>>.Ok(programs);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                "reading the well program queue",
                "the queue is not shown",
                FailureSeverity.Error);

            return OgResult<IReadOnlyList<OgWellProgram>>.Failed(
                "The well program queue could not be read.", reference ?? CodeReadFailed);
        }
    }

    public async Task<OgResult<OgWellProgram>> LoadAsync(
        string programId,
        DateTime asOf,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(programId);

        var source = _source();

        if (source is null)
        {
            return OgResult<OgWellProgram>.Failed(
                "No connection is open, so the well program cannot be read.", CodeNoConnection);
        }

        try
        {
            var context = await ReadContextAsync(source, cancellationToken).ConfigureAwait(false);

            var program = context.Programs.FirstOrDefault(row =>
                string.Equals(row.PROGRAM_ID, programId.Trim(), StringComparison.OrdinalIgnoreCase));

            if (program is null)
            {
                return OgResult<OgWellProgram>.Failed(
                    $"Well program '{programId}' is not in this database.", CodeUnknown);
            }

            return OgResult<OgWellProgram>.Ok(ToProgram(program, asOf.Date, context));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                $"reading well program {programId}",
                "the program is not shown",
                FailureSeverity.Error);

            return OgResult<OgWellProgram>.Failed(
                "The well program could not be read.", reference ?? CodeReadFailed);
        }
    }

    public async Task<OgResult<OgWellProgram>> DecideAsync(
        OgWellProgram program,
        OgWellProgramDecision decision,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(program);
        ArgumentNullException.ThrowIfNull(decision);

        var refusal = Validate(program, decision);

        if (refusal is { } reason)
        {
            return OgResult<OgWellProgram>.Failed(reason.Message, reason.Code);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgWellProgram>.Failed(
                "No connection is open, so the decision cannot be recorded.", CodeNoConnection);
        }

        try
        {
            var unit = OgUow.For<WELL_PROGRAM>(source);
            var rows = await OgUow.ReadAsync(unit, [OgUow.Equal("PROGRAM_ID", program.Id)], cancellationToken)
                .ConfigureAwait(false);
            var current = rows.FirstOrDefault();

            if (current is null)
            {
                return OgResult<OgWellProgram>.Failed(
                    $"Well program '{program.Id}' is no longer in this database.", CodeUnknown);
            }

            // AC-4: a decision somebody else recorded, or a resubmission, is a conflict rather than an overwrite.
            if (!string.Equals(current.STATE ?? string.Empty, OgWellProgram.StatePending, StringComparison.OrdinalIgnoreCase))
            {
                return OgResult<OgWellProgram>.Failed(
                    $"{program.Name} is {current.STATE}; somebody has already decided it. Reload the queue.",
                    CodeConflict);
            }

            var version = current.ROW_VERSION ?? 0;

            if (version != program.Version)
            {
                return OgResult<OgWellProgram>.Failed(
                    $"{program.Name} changed since it was opened (version {program.Version} read, " +
                    $"{version} now), so the decision was not recorded. Reload the program.",
                    CodeConflict);
            }

            var decidedAt = decision.DecidedAt ?? DateTimeOffset.UtcNow;
            var conditions = Clean(decision.Conditions);
            var state = decision.ResultingState;

            current.STATE = state;
            current.DECIDED_BY = decision.Actor.Trim();
            current.DECIDED_AT = decidedAt.UtcDateTime;
            current.RATIONALE = Trimmed(decision.Rationale);
            current.CONDITIONS_TEXT = Join(conditions);
            current.ROW_VERSION = version + 1;
            current.ROW_CHANGED_BY = decision.Actor.Trim();
            current.ROW_CHANGED_DATE = decidedAt.UtcDateTime;

            var updated = unit.Update(current);

            if (!OgUow.IsOk(updated))
            {
                // AC-6: the decision is kept so a retry sends the same rationale, conditions and actor.
                Pending = decision;
                RetainedProgramId = program.Id;
                return OgResult<OgWellProgram>.Failed(
                    $"The decision was not acknowledged: {OgUow.Message(updated, "the row was not updated")}. " +
                    "Retry sends the same decision.",
                    CodeWriteFailed);
            }

            var committed = await unit.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(committed))
            {
                Pending = decision;
                RetainedProgramId = program.Id;
                return OgResult<OgWellProgram>.Failed(
                    $"The decision was not acknowledged: {OgUow.Message(committed, "the row was not updated")}. " +
                    "Retry sends the same decision.",
                    CodeWriteFailed);
            }

            await RecordDecisionAsync(source, program.Id, decision, state, conditions, decidedAt, cancellationToken)
                .ConfigureAwait(false);

            Pending = null;
            RetainedProgramId = string.Empty;

            return await LoadAsync(program.Id, decidedAt.UtcDateTime, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            Pending = decision;
            RetainedProgramId = program.Id;
            var reference = _failures.ReportHandled(
                ex,
                $"recording the decision on well program {program.Id}",
                "the decision was not recorded; the original is retained for retry",
                FailureSeverity.Error);

            return OgResult<OgWellProgram>.Failed(
                "The decision could not be recorded; retry sends the same decision.",
                reference ?? CodeWriteFailed);
        }
    }

    public Task<OgResult<OgWellProgram>> RetryAsync(
        DateTime asOf,
        CancellationToken cancellationToken = default)
    {
        if (Pending is not { } pending)
        {
            return Task.FromResult(OgResult<OgWellProgram>.Failed(
                "There is no decision waiting to be retried.", CodeNothingPending));
        }

        // The program is re-read so the retry is judged against its current state, and the retained decision is
        // sent unchanged.
        return RetryCoreAsync(RetainedProgramId, pending, asOf, cancellationToken);
    }

    private async Task<OgResult<OgWellProgram>> RetryCoreAsync(
        string programId,
        OgWellProgramDecision pending,
        DateTime asOf,
        CancellationToken cancellationToken)
    {
        var current = await LoadAsync(programId, asOf, cancellationToken).ConfigureAwait(false);

        return current.Success && current.Value is not null
            ? await DecideAsync(current.Value, pending, cancellationToken).ConfigureAwait(false)
            : current;
    }

    /// <summary>The program the retained decision belongs to.</summary>
    public string RetainedProgramId { get; private set; } = string.Empty;

    /// <summary>
    /// The decision rules, stated once so the page, the service and the tests share them (AC-1..AC-4).
    /// </summary>
    public static (string Code, string Message)? Validate(OgWellProgram program, OgWellProgramDecision decision)
    {
        ArgumentNullException.ThrowIfNull(program);
        ArgumentNullException.ThrowIfNull(decision);

        if (string.IsNullOrWhiteSpace(decision.Actor))
        {
            return (CodeActorRequired, "Every decision records who made it, so an actor is required.");
        }

        var normalised = decision.Decision?.Trim().ToUpperInvariant() ?? string.Empty;

        if (!OgWellProgramDecision.Decisions.Contains(normalised, StringComparer.Ordinal))
        {
            return (CodeDecisionRequired,
                $"'{decision.Decision}' is not a gate decision; use {string.Join(", ", OgWellProgramDecision.Decisions)}.");
        }

        // AC-3
        if (!string.IsNullOrWhiteSpace(program.Author)
            && string.Equals(program.Author.Trim(), decision.Actor.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            return (CodeAuthorCannotDecide,
                $"{decision.Actor.Trim()} submitted this program, so they cannot decide it. Another approver must.");
        }

        // AC-2
        if (!program.ProspectRiskComplete)
        {
            return (CodeRiskIncomplete,
                "The prospect's risk assessment is incomplete, so the gate cannot approve or reject the program. " +
                "Record the missing risk elements first.");
        }

        // AC-4
        if (!program.IsPending)
        {
            return (CodeNotPending,
                $"{program.Name} is {program.StateText}, so it is not awaiting a decision.");
        }

        var conditions = Clean(decision.Conditions);
        var rationale = Trimmed(decision.Rationale);

        if (normalised == OgWellProgramDecision.Approve)
        {
            if (conditions.Count > 0)
            {
                return (CodeConditionsNotAllowed,
                    "Conditions belong to an approval with conditions; change the decision, or record them as " +
                    "constraints of the approval.");
            }

            return null;
        }

        if (rationale is null)
        {
            return (CodeRationaleRequired,
                $"A {normalised.Replace('_', ' ').ToLowerInvariant()} decision needs a rationale.");
        }

        if (normalised == OgWellProgramDecision.ApproveWithConditions && conditions.Count == 0)
        {
            return (CodeConditionsRequired,
                "An approval with conditions needs at least one condition.");
        }

        if (normalised != OgWellProgramDecision.ApproveWithConditions && conditions.Count > 0)
        {
            return (CodeConditionsNotAllowed,
                "Only an approval with conditions carries conditions.");
        }

        return null;
    }

    // ------------------------------------------------------------------ reads

    private static async Task<ProgramContext> ReadContextAsync(
        IDataSource source,
        CancellationToken cancellationToken)
    {
        var programs = await OgUow.ReadAllAsync<WELL_PROGRAM>(source).ConfigureAwait(false);
        var prospects = await OgUow.ReadAllAsync<PROSPECT>(source).ConfigureAwait(false);
        var assessments = await OgUow.ReadAllAsync<PROSPECT_RISK_ASSESSMENT>(source).ConfigureAwait(false);
        var decisions = await OgUow.ReadAllAsync<WELL_PROGRAM_DECISION>(source).ConfigureAwait(false);

        cancellationToken.ThrowIfCancellationRequested();

        var names = prospects
            .Where(prospect => !string.IsNullOrWhiteSpace(prospect.PROSPECT_ID))
            .GroupBy(prospect => prospect.PROSPECT_ID!, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                group => group.Key,
                group => Blank(group.First().PROSPECT_NAME),
                StringComparer.OrdinalIgnoreCase);

        var risks = assessments
            .Where(row => !string.IsNullOrWhiteSpace(row.PROSPECT_ID))
            .GroupBy(row => row.PROSPECT_ID!, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                group => group.Key,
                group => group.Any(IsComplete),
                StringComparer.OrdinalIgnoreCase);

        var history = decisions
            .Where(row => !string.IsNullOrWhiteSpace(row.PROGRAM_ID))
            .GroupBy(row => row.PROGRAM_ID!, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                group => group.Key,
                group => group
                    .OrderByDescending(row => row.DECISION_SEQ_NO)
                    .Select(row => new OgWellProgramHistoryEntry(
                        Blank(row.DECISION) ?? string.Empty,
                        Blank(row.RESULTING_STATE) ?? string.Empty,
                        Blank(row.RATIONALE),
                        Split(row.CONDITIONS_TEXT),
                        Blank(row.DECIDED_BY) ?? string.Empty,
                        AsOffset(row.DECIDED_AT)))
                    .ToList(),
                StringComparer.OrdinalIgnoreCase);

        return new ProgramContext(names, risks, history, programs);
    }

    private static OgWellProgram ToProgram(WELL_PROGRAM row, DateTime asOf, ProgramContext context)
    {
        var id = row.PROGRAM_ID ?? string.Empty;
        var prospectId = Blank(row.PROSPECT_ID);

        context.Names.TryGetValue(prospectId ?? string.Empty, out var prospectName);

        var submitted = row.SUBMITTED_AT;

        return new OgWellProgram(
            id,
            prospectId,
            prospectName,
            Blank(row.PROGRAM_NAME) ?? id,
            Blank(row.OBJECTIVE),
            row.TARGET_DEPTH,
            Blank(row.DEPTH_OUOM),
            row.ESTIMATED_COST,
            Blank(row.CURRENCY),
            Blank(row.STATE) ?? string.Empty,
            Blank(row.AUTHOR),
            AsOffset(submitted),
            submitted is null ? null : Math.Max(0, (asOf.Date - submitted.Value.Date).Days),
            Blank(row.DECIDED_BY),
            AsOffset(row.DECIDED_AT),
            Blank(row.RATIONALE),
            Split(row.CONDITIONS_TEXT),
            prospectId is not null
            && context.Risks.TryGetValue(prospectId, out var complete)
            && complete,
            row.ROW_VERSION ?? 0,
            context.History.TryGetValue(id, out var entries) ? entries : []);
    }

    private static bool IsComplete(PROSPECT_RISK_ASSESSMENT row) =>
        OgProspectChance.IsComplete(new Dictionary<string, decimal?>(StringComparer.OrdinalIgnoreCase)
        {
            [OgProspectChance.RequiredElements[0]] = row.TRAP_RISK,
            [OgProspectChance.RequiredElements[1]] = row.RESERVOIR_RISK,
            [OgProspectChance.RequiredElements[2]] = row.SEAL_RISK,
            [OgProspectChance.RequiredElements[3]] = row.SOURCE_RISK,
            [OgProspectChance.RequiredElements[4]] = row.TIMING_RISK,
        });

    private static bool IsPending(WELL_PROGRAM row) =>
        string.Equals(row.STATE ?? string.Empty, OgWellProgram.StatePending, StringComparison.OrdinalIgnoreCase);

    private static bool FieldMatches(string? value, string? fieldId, string? fieldName) =>
        (string.IsNullOrWhiteSpace(fieldId) && string.IsNullOrWhiteSpace(fieldName))
        || (!string.IsNullOrWhiteSpace(fieldId)
         && string.Equals(value ?? string.Empty, fieldId.Trim(), StringComparison.OrdinalIgnoreCase))
        || (!string.IsNullOrWhiteSpace(fieldName)
            && string.Equals(value ?? string.Empty, fieldName.Trim(), StringComparison.OrdinalIgnoreCase));

    // ----------------------------------------------------------------- writes

    private async Task RecordDecisionAsync(
        IDataSource source,
        string programId,
        OgWellProgramDecision decision,
        string state,
        IReadOnlyList<string> conditions,
        DateTimeOffset decidedAt,
        CancellationToken cancellationToken)
    {
        try
        {
            var unit = OgUow.For<WELL_PROGRAM_DECISION>(source);
            var existing = await OgUow.ReadAsync(unit, [OgUow.Equal("PROGRAM_ID", programId)], cancellationToken)
                .ConfigureAwait(false);

            var sequence = existing.Count == 0 ? 1 : existing.Max(row => row.DECISION_SEQ_NO) + 1;

            unit.Add(new WELL_PROGRAM_DECISION
            {
                PROGRAM_ID = programId,
                DECISION_SEQ_NO = sequence,
                DECISION = decision.Decision.Trim().ToUpperInvariant(),
                RESULTING_STATE = state,
                RATIONALE = Trimmed(decision.Rationale),
                CONDITIONS_TEXT = Join(conditions),
                DECIDED_BY = decision.Actor.Trim(),
                DECIDED_AT = decidedAt.UtcDateTime,
                ROW_CREATED_BY = decision.Actor.Trim(),
                ROW_CREATED_DATE = decidedAt.UtcDateTime,
                ROW_CHANGED_BY = decision.Actor.Trim(),
                ROW_CHANGED_DATE = decidedAt.UtcDateTime,
            });

            var committed = await unit.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(committed))
            {
                _failures.ReportHandled(
                    new InvalidOperationException(
                        $"The decision history row for {programId} was not written: " +
                        OgUow.Message(committed, "the row was not inserted")),
                    $"recording the decision history of {programId}",
                    "the decision stands but its history row is missing",
                    FailureSeverity.Degraded);
            }
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            _failures.ReportHandled(
                ex,
                $"recording the decision history of {programId}",
                "the decision stands but its history row is missing",
                FailureSeverity.Degraded);
        }
    }

    // ---------------------------------------------------------------- helpers

    private static IReadOnlyList<string> Clean(IReadOnlyList<string>? conditions) =>
        (conditions ?? [])
            .Where(condition => !string.IsNullOrWhiteSpace(condition))
            .Select(condition => condition.Trim())
            .ToList();

    private static IReadOnlyList<string> Split(string? text) =>
        string.IsNullOrWhiteSpace(text)
            ? []
            : text.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    private static string? Join(IReadOnlyList<string> conditions) =>
        conditions.Count == 0 ? null : string.Join("\n", conditions);

    private static string? Trimmed(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string? Blank(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value;

    private static DateTimeOffset? AsOffset(DateTime? moment) =>
        moment is { } value ? new DateTimeOffset(value, TimeSpan.Zero) : null;

    private sealed record ProgramContext(
        IReadOnlyDictionary<string, string?> Names,
        IReadOnlyDictionary<string, bool> Risks,
        IReadOnlyDictionary<string, List<OgWellProgramHistoryEntry>> History,
        IReadOnlyList<WELL_PROGRAM> Programs);
}
