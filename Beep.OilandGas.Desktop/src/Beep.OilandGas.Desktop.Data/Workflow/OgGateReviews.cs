using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using Beep.OilandGas.LifeCycle.Data.Tables;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Data.Workflow;

/// <summary>One item of a gate's checklist (WFL-04's <c>OgChecklistItem</c>): a required approval and whether it is recorded.</summary>
public sealed record OgGateChecklistItem(
    string Key,
    bool Required,
    bool Satisfied,
    string? EvidenceRef,
    string Why)
{
    public string Describe() =>
        Key + " · " + (Satisfied ? "satisfied" : "not satisfied: " + Why) +
        (string.IsNullOrWhiteSpace(EvidenceRef) ? " · no evidence reference" : " · evidence " + EvidenceRef);
}

/// <summary>One decision already recorded for the gate (WFL-04).</summary>
public sealed record OgPriorDecision(
    string Kind,
    string Actor,
    string? ActingFor,
    DateTime? At,
    string? Rationale,
    string? Conditions)
{
    public string Describe() =>
        (At is { } at ? at.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture) : "(no time)") +
        " · " + Kind.ToLowerInvariant() +
        (string.IsNullOrWhiteSpace(ActingFor) ? " · " + Actor : " · " + Actor + " acting for " + ActingFor) +
        (string.IsNullOrWhiteSpace(Rationale) ? string.Empty : " · " + Rationale) +
        (string.IsNullOrWhiteSpace(Conditions) ? string.Empty : " · conditions: " + Conditions);
}

/// <summary>One gate awaiting a decision (WFL-04's <c>OgGateReview</c>, in the queue's shape).</summary>
public sealed record OgGateReview(
    string ApprovalId,
    string InstanceId,
    string? StepKey,
    string? StepName,
    string DefinitionName,
    string SubjectKind,
    string SubjectId,
    string Gate,
    string? RequestedBy,
    DateTime? RequestedAt,
    decimal? AgeDays,
    string DueState,
    string? DelegatedFrom,
    string? DelegatedTo,
    string DecisionState,
    IReadOnlyList<OgGateChecklistItem> Checklist,
    IReadOnlyList<OgPriorDecision> PriorDecisions,
    string? AuthorOfSubject,
    IReadOnlyList<string> Blocks)
{
    public bool IsDelegated => !string.IsNullOrWhiteSpace(DelegatedFrom) && !string.IsNullOrWhiteSpace(DelegatedTo);

    public bool IsDecided => !string.Equals(DecisionState, GateRules.Pending, StringComparison.OrdinalIgnoreCase);

    /// <summary>AC-1 and SG-14-3: approve is offered only when nothing blocks it.</summary>
    public bool ApproveAllowed => !IsDecided && Blocks.Count == 0;

    /// <summary>AC-3: both names, so a delegated decision can never read as the delegate's own.</summary>
    public string DelegationText => IsDelegated
        ? "Acting for " + DelegatedFrom + " under a recorded delegation to " + DelegatedTo + "."
        : "Not delegated: this decision is the approver's own.";

    public string SubjectText => string.IsNullOrWhiteSpace(SubjectKind) && string.IsNullOrWhiteSpace(SubjectId)
        ? "no subject recorded"
        : SubjectKind + " " + SubjectId;

    public string ChecklistText => Checklist.Count == 0
        ? "No other approval is required for this gate."
        : string.Join(Environment.NewLine, Checklist.Select(item => item.Describe()));

    public string BlocksText => Blocks.Count == 0
        ? "Nothing blocks an approval."
        : string.Join(Environment.NewLine, Blocks.Select(block => "✗ " + block));

    public string DecisionText => IsDecided
        ? "This gate is " + DecisionState.ToLowerInvariant() + " and is read-only."
        : "This gate is awaiting a decision.";

    public string Describe() =>
        Gate + " · " + SubjectText + " · " +
        (string.IsNullOrWhiteSpace(StepName) ? StepKey ?? "(no step)" : StepName) +
        " · " + (string.IsNullOrWhiteSpace(RequestedBy) ? "no requester" : RequestedBy) +
        (AgeDays is { } age ? " · " + age.ToString("0.0#", CultureInfo.InvariantCulture) + " day(s)" : string.Empty) +
        " · " + DueState + " · " + DecisionState;
}

/// <summary>The gate workspace (WFL-04).</summary>
public sealed record OgGateWorkspace(
    string? FieldId,
    IReadOnlyList<OgGateReview> Gates,
    OgGateReview? Selected,
    DateTime AsOf,
    DateTime ReadAt)
{
    public int Awaiting => Gates.Count(gate => !gate.IsDecided);

    public int Ready => Gates.Count(gate => gate.ApproveAllowed);

    public int Blocked => Gates.Count(gate => !gate.IsDecided && gate.Blocks.Count > 0);

    public string CountsText =>
        Gates.Count.ToString(CultureInfo.InvariantCulture) + " gate(s) · " +
        Awaiting.ToString(CultureInfo.InvariantCulture) + " awaiting a decision · " +
        Ready.ToString(CultureInfo.InvariantCulture) + " ready to approve · " +
        Blocked.ToString(CultureInfo.InvariantCulture) + " blocked";

    public string GatesText => Gates.Count == 0
        ? "No gate is awaiting a decision."
        : string.Join(Environment.NewLine, Gates.Select(gate => gate.Describe()));

    public string ReadText =>
        "read at " + ReadAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
}

/// <summary>The gate rules WFL-04 states once.</summary>
public static class GateRules
{
    public const string CodeNoConnection = "gate-no-connection";
    public const string CodeReadFailed = "gate-read-failed";
    public const string CodeWriteFailed = "gate-write-failed";
    public const string CodeActorRequired = "gate-actor-required";
    public const string CodeUnknownGate = "gate-unknown";
    public const string CodeKindInvalid = "gate-kind-invalid";
    public const string CodeRationaleRequired = "gate-rationale-required";
    public const string CodeConditionsRequired = "gate-conditions-required";
    public const string CodeChecklistIncomplete = "gate-checklist-incomplete";
    public const string CodeSodConflict = "gate-sod-conflict";
    public const string CodeAuthorCannotApprove = "gate-author-cannot-approve";
    public const string CodeAlreadyDecided = "gate-already-decided";
    public const string CodeDelegatedToAnother = "gate-delegated-to-another";

    public const string Pending = "PENDING";
    public const string Approve = "APPROVE";
    public const string ApproveWithConditions = "APPROVE_WITH_CONDITIONS";
    public const string Return = "RETURN";
    public const string Reject = "REJECT";

    public static IReadOnlyList<string> Kinds { get; } = [Approve, ApproveWithConditions, Return, Reject];

    /// <summary>The statuses a recorded approval carries once decided.</summary>
    public static IReadOnlyList<string> DecidedStatuses { get; } = ["APPROVED", "RETURNED", "REJECTED", "REJECT"];

    /// <summary>AC-2: reject and return need a rationale; conditions need their text.</summary>
    public static (string Code, string Message)? ValidateDecision(
        string? kind,
        string? rationale,
        string? conditions)
    {
        var normalized = kind?.Trim().ToUpperInvariant() ?? string.Empty;

        if (!Kinds.Contains(normalized, StringComparer.Ordinal))
        {
            return (CodeKindInvalid,
                "'" + (kind ?? "(none)") + "' is not a decision; use " + string.Join(", ", Kinds) + ".");
        }

        if (string.Equals(normalized, ApproveWithConditions, StringComparison.Ordinal) &&
            string.IsNullOrWhiteSpace(conditions))
        {
            return (CodeConditionsRequired,
                "An approval with conditions needs the conditions themselves; say what they are.");
        }

        if ((string.Equals(normalized, Return, StringComparison.Ordinal) ||
             string.Equals(normalized, Reject, StringComparison.Ordinal)) &&
            string.IsNullOrWhiteSpace(rationale))
        {
            return (CodeRationaleRequired,
                "A " + normalized.ToLowerInvariant() + " needs its rationale, so the record shows why the gate was not passed.");
        }

        return null;
    }

    /// <summary>
    /// AC-1: the checklist is the other approvals the gate requires, and it is complete only when each of them is recorded as
    /// approved. The decision being taken is not one of them, so a gate cannot block itself.
    /// </summary>
    public static IReadOnlyList<OgGateChecklistItem> ChecklistOf(
        IEnumerable<PROCESS_APPROVAL> approvals,
        string? stepInstanceId,
        string currentApprovalId)
    {
        ArgumentNullException.ThrowIfNull(approvals);

        return approvals
            .Where(row => !string.IsNullOrWhiteSpace(row.PROCESS_STEP_INSTANCE_ID) &&
                          string.Equals(row.PROCESS_STEP_INSTANCE_ID, stepInstanceId, StringComparison.OrdinalIgnoreCase) &&
                          !string.Equals(row.PROCESS_APPROVAL_ID, currentApprovalId, StringComparison.OrdinalIgnoreCase))
            .OrderBy(row => row.APPROVER_SEQUENCE ?? int.MaxValue)
            .Select(row =>
            {
                var status = (row.APPROVAL_STATUS ?? string.Empty).Trim();
                var satisfied = string.Equals(status, "APPROVED", StringComparison.OrdinalIgnoreCase);

                return new OgGateChecklistItem(
                    Key: (string.IsNullOrWhiteSpace(row.REQUIRED_ACTION) ? "APPROVAL" : row.REQUIRED_ACTION!.Trim()) +
                         (row.APPROVER_SEQUENCE is { } sequence
                             ? " #" + sequence.ToString(CultureInfo.InvariantCulture)
                             : string.Empty),
                    Required: true,
                    Satisfied: satisfied,
                    EvidenceRef: string.IsNullOrWhiteSpace(row.APPROVED_BY) ? null : row.APPROVED_BY,
                    Why: satisfied
                        ? string.Empty
                        : string.IsNullOrWhiteSpace(row.APPROVAL_STATUS)
                            ? "no decision is recorded for it yet"
                            : "it is " + status.ToLowerInvariant());
            })
            .ToList();
    }

    /// <summary>
    /// SG-14-3: an unresolved blocking conflict for the actor stops an approval, and the conflict is named. A rule that is not
    /// blocking is shown but does not disable the decision, because the owner records that distinction.
    /// </summary>
    public static IReadOnlyList<string> SodBlocks(
        string? actor,
        IEnumerable<SOD_CONFLICT> conflicts,
        IEnumerable<SOD_RULE> rules)
    {
        ArgumentNullException.ThrowIfNull(conflicts);
        ArgumentNullException.ThrowIfNull(rules);

        if (string.IsNullOrWhiteSpace(actor))
        {
            return [];
        }

        var blocking = rules
            .Where(rule => !string.IsNullOrWhiteSpace(rule.SOD_RULE_ID))
            .Where(rule => string.Equals(rule.IS_BLOCKING?.Trim(), "Y", StringComparison.OrdinalIgnoreCase))
            .Select(rule => rule.SOD_RULE_ID)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return conflicts
            .Where(conflict => string.Equals(conflict.USER_ID?.Trim(), actor.Trim(), StringComparison.OrdinalIgnoreCase))
            .Where(conflict => conflict.RESOLVED_DATE is null &&
                               !string.Equals(conflict.CONFLICT_STATUS?.Trim(), "RESOLVED", StringComparison.OrdinalIgnoreCase))
            .Where(conflict => blocking.Contains(conflict.SOD_RULE_ID ?? string.Empty))
            .Select(conflict =>
                "A segregation-of-duties conflict blocks this approval: " + conflict.RULE_NAME +
                (string.IsNullOrWhiteSpace(conflict.ROLE_A) ? string.Empty : " (" + conflict.ROLE_A) +
                (string.IsNullOrWhiteSpace(conflict.ROLE_B) ? ")" : " and " + conflict.ROLE_B + ")"))
            .ToList();
    }

    /// <summary>
    /// AC-6: the author of the subject cannot approve it. The instance's creator is the author, and the approval's requester is
    /// refused for the same reason.
    /// </summary>
    public static (string Code, string Message)? CheckAuthor(
        string? author,
        string? requester,
        string? actor)
    {
        if (string.IsNullOrWhiteSpace(actor))
        {
            return (CodeActorRequired, "A decision must record who made it.");
        }

        if (!string.IsNullOrWhiteSpace(author) &&
            string.Equals(author.Trim(), actor.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            return (CodeAuthorCannotApprove,
                author + " authored this subject and therefore cannot approve, return or reject it.");
        }

        return !string.IsNullOrWhiteSpace(requester) &&
               string.Equals(requester.Trim(), actor.Trim(), StringComparison.OrdinalIgnoreCase)
            ? (CodeAuthorCannotApprove, requester + " requested this approval and therefore cannot decide it.")
            : null;
    }

    /// <summary>AC-3: when the approval is delegated, only the delegate may act, and both names are known.</summary>
    public static (string Code, string Message)? CheckDelegation(
        string? delegatedFrom,
        string? delegatedTo,
        string? actor)
    {
        if (string.IsNullOrWhiteSpace(delegatedFrom) && string.IsNullOrWhiteSpace(delegatedTo))
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(delegatedTo))
        {
            return (CodeDelegatedToAnother,
                "This approval records a delegator (" + (delegatedFrom ?? "(none)") +
                ") but no delegate, so nobody can be shown to hold it.");
        }

        return string.Equals(delegatedTo.Trim(), actor?.Trim(), StringComparison.OrdinalIgnoreCase)
            ? null
            : (CodeDelegatedToAnother,
                "This approval is delegated to " + delegatedTo + ", so " + (actor ?? "(nobody)") + " cannot decide it.");
    }

    /// <summary>AC-4: a decided gate is read-only; the original decision is retained and no second one is written.</summary>
    public static (string Code, string Message)? CheckUndecided(string? status)
    {
        var normalized = (status ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(normalized) ||
            string.Equals(normalized, Pending, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return (CodeAlreadyDecided,
            "This gate already records the decision '" + normalized.ToLowerInvariant() +
            "', so it is read-only and the original decision is retained.");
    }

    /// <summary>The approval status a decision records.</summary>
    public static string StatusFor(string kind) => kind.Trim().ToUpperInvariant() switch
    {
        Approve => "APPROVED",
        ApproveWithConditions => "APPROVED",
        Return => "RETURNED",
        _ => "REJECTED",
    };
}

/// <summary>WFL-04: gate reviews and approvals.</summary>
public interface IOgGateReviews
{
    /// <param name="approvalId">The gate to open; the first awaiting one when omitted.</param>
    /// <param name="actor">The signed-in person, so the segregation checks are theirs.</param>
    Task<OgResult<OgGateWorkspace>> LoadAsync(
        string? fieldId,
        string? approvalId,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Records a decision: evidence, decision, rationale and conditions (AC-1, AC-2, AC-5), with SoD, authorship, delegation
    /// and single-decision rules enforced (SG-14-3, AC-3, AC-4, AC-6).
    /// </summary>
    Task<OgResult<OgGateWorkspace>> DecideAsync(
        string approvalId,
        string kind,
        string rationale,
        string conditions,
        string actor,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// WFL-04 over the owner's <c>PROCESS_APPROVAL</c>, <c>SOD_RULE</c>, <c>SOD_CONFLICT</c>, <c>PROCESS_STEP_INSTANCE</c> and
/// <c>PROCESS_INSTANCE</c>.
/// </summary>
/// <remarks>
/// <para>
/// The gate is an approval row, so a decision is a write to the row the queue already reads - not to a second table. Its
/// checklist is the other approvals the same step requires, which is why a gate cannot block itself, and its segregation check
/// reads the owner's own <c>SOD_CONFLICT</c>/<c>SOD_RULE</c> pair, honouring the <c>IS_BLOCKING</c> distinction the owner records.
/// </para>
/// <para>
/// A decision appends a <c>PROCESS_HISTORY</c> event and merges the conditions into the instance's
/// <c>INSTANCE_DATA_JSON</c>, so the conditions reach the subject rather than living only on the approval.
/// </para>
/// </remarks>
public sealed class OgGateReviewsService : IOgGateReviews
{
    private readonly Func<IDataSource?> _source;
    private readonly IFailureReporter _failures;

    public OgGateReviewsService(Func<IDataSource?> source, IFailureReporter failures)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _failures = failures ?? throw new ArgumentNullException(nameof(failures));
    }

    public async Task<OgResult<OgGateWorkspace>> LoadAsync(
        string? fieldId,
        string? approvalId,
        string actor,
        CancellationToken cancellationToken = default)
    {
        var source = _source();

        if (source is null)
        {
            return OgResult<OgGateWorkspace>.Failed(
                "No connection is open, so the gate queue cannot be read.", GateRules.CodeNoConnection);
        }

        try
        {
            return await ReadAsync(source, fieldId, approvalId, actor ?? string.Empty, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, "reading the gate queue", "the gates are not shown", FailureSeverity.Error);

            return OgResult<OgGateWorkspace>.Failed(
                "The gates could not be read" +
                (string.IsNullOrWhiteSpace(reference) ? "." : " (reference " + reference + ")."),
                reference ?? GateRules.CodeReadFailed);
        }
    }

    public async Task<OgResult<OgGateWorkspace>> DecideAsync(
        string approvalId,
        string kind,
        string rationale,
        string conditions,
        string actor,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgGateWorkspace>.Failed(
                "A decision must record who made it.", GateRules.CodeActorRequired);
        }

        if (GateRules.ValidateDecision(kind, rationale, conditions) is { } invalid)
        {
            return OgResult<OgGateWorkspace>.Failed(invalid.Message, invalid.Code);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgGateWorkspace>.Failed(
                "No connection is open, so the decision cannot be recorded.", GateRules.CodeNoConnection);
        }

        try
        {
            var read = await ReadAsync(source, null, approvalId, actor, cancellationToken).ConfigureAwait(false);

            if (!read.Success || read.Value is null)
            {
                return read;
            }

            var gate = read.Value.Selected;

            if (gate is null || !string.Equals(gate.ApprovalId, approvalId.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                return OgResult<OgGateWorkspace>.Failed(
                    "Gate '" + approvalId + "' is not recorded.", GateRules.CodeUnknownGate);
            }

            // AC-4: a decided gate is read-only.
            var undecided = GateRules.CheckUndecided(gate.DecisionState);

            if (undecided is { } decided)
            {
                return OgResult<OgGateWorkspace>.Failed(decided.Message, decided.Code);
            }

            // AC-6: the author cannot approve their own subject, and neither can the requester.
            if (GateRules.CheckAuthor(gate.AuthorOfSubject, gate.RequestedBy, actor) is { } authorRefusal)
            {
                return OgResult<OgGateWorkspace>.Failed(authorRefusal.Message, authorRefusal.Code);
            }

            // AC-3: when it is delegated, only the delegate acts.
            if (GateRules.CheckDelegation(gate.DelegatedFrom, gate.DelegatedTo, actor) is { } delegation)
            {
                return OgResult<OgGateWorkspace>.Failed(delegation.Message, delegation.Code);
            }

            // AC-1 / SG-14-3: the blocks the page already shows are the blocks the command gives.
            if (gate.Blocks.Count > 0)
            {
                var code = gate.Blocks.Any(block => block.Contains("segregation-of-duties", StringComparison.Ordinal))
                    ? GateRules.CodeSodConflict
                    : GateRules.CodeChecklistIncomplete;

                return OgResult<OgGateWorkspace>.Failed(string.Join(" ", gate.Blocks), code);
            }

            var unit = OgUow.For<PROCESS_APPROVAL>(source);
            unit.PrimaryKey = nameof(PROCESS_APPROVAL.PROCESS_APPROVAL_ID);

            var rows = await OgUow.ReadAsync(
                unit,
                [OgUow.Equal(nameof(PROCESS_APPROVAL.PROCESS_APPROVAL_ID), gate.ApprovalId)],
                cancellationToken).ConfigureAwait(false);

            var row = rows.FirstOrDefault();

            if (row is null)
            {
                return OgResult<OgGateWorkspace>.Failed(
                    "Gate '" + approvalId + "' is not recorded.", GateRules.CodeUnknownGate);
            }

            var now = DateTime.UtcNow;
            var normalized = kind.Trim().ToUpperInvariant();

            row.APPROVAL_STATUS = GateRules.StatusFor(normalized);
            row.APPROVAL_DATE = now;
            row.APPROVED_BY = actor.Trim();
            row.APPROVAL_NOTES = BuildNotes(normalized, rationale, conditions);

            if (gate.IsDelegated)
            {
                // AC-3: the delegator is recorded on the decision, so the record shows both names for good.
                row.DELEGATED_BY = gate.DelegatedFrom;
                row.DELEGATION_DATE ??= now;
            }

            row.ROW_CHANGED_BY = actor.Trim();
            row.ROW_CHANGED_DATE = now;

            var updated = unit.Update(row);

            if (!OgUow.IsOk(updated))
            {
                return OgResult<OgGateWorkspace>.Failed(
                    OgUow.Message(updated, "The decision was not saved."), GateRules.CodeWriteFailed);
            }

            var committed = await unit.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(committed))
            {
                return OgResult<OgGateWorkspace>.Failed(
                    OgUow.Message(committed, "The decision was not saved."), GateRules.CodeWriteFailed);
            }

            await RecordOnSubjectAsync(source, gate, normalized, rationale, conditions, actor, cancellationToken)
                .ConfigureAwait(false);

            return await ReadAsync(source, read.Value.FieldId, gate.ApprovalId, actor, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, "deciding gate " + approvalId, "the decision was not saved", FailureSeverity.Error);

            return OgResult<OgGateWorkspace>.Failed(
                "The decision could not be saved" +
                (string.IsNullOrWhiteSpace(reference) ? "." : " (reference " + reference + ")."),
                reference ?? GateRules.CodeWriteFailed);
        }
    }

    /// <summary>AC-5: the conditions reach the subject, and the decision is on its history.</summary>
    private async Task RecordOnSubjectAsync(
        IDataSource source,
        OgGateReview gate,
        string kind,
        string rationale,
        string conditions,
        string actor,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        var history = OgUow.For<PROCESS_HISTORY>(source);
        history.PrimaryKey = nameof(PROCESS_HISTORY.PROCESS_HISTORY_ID);

        history.Add(new PROCESS_HISTORY
        {
            PROCESS_HISTORY_ID = Guid.NewGuid().ToString("N"),
            PROCESS_INSTANCE_ID = gate.InstanceId,
            PROCESS_STEP_INSTANCE_ID = null,
            EVENT_TYPE = "GATE_" + kind,
            EVENT_DATE = now,
            USER_ID = actor.Trim(),
            DETAILS = string.IsNullOrWhiteSpace(rationale)
                ? GateRules.StatusFor(kind)
                : rationale.Trim(),
        });

        var committed = await history.Commit().ConfigureAwait(false);

        if (!OgUow.IsOk(committed))
        {
            _failures.ReportHandled(
                new InvalidOperationException(OgUow.Message(committed, "The history entry was not written.")),
                "recording the gate decision on " + gate.InstanceId,
                "the approval records the decision; the instance history does not",
                FailureSeverity.Degraded);
        }

        if (string.IsNullOrWhiteSpace(conditions))
        {
            return;
        }

        // The conditions are merged into the instance's data so the subject carries them, and the original payload is kept.
        var instances = await OgUow.ReadAllAsync<PROCESS_INSTANCE>(source).ConfigureAwait(false);

        var instance = instances.FirstOrDefault(row =>
            string.Equals(row.PROCESS_INSTANCE_ID, gate.InstanceId, StringComparison.OrdinalIgnoreCase));

        if (instance is null)
        {
            return;
        }

        var root = string.IsNullOrWhiteSpace(instance.INSTANCE_DATA_JSON)
            ? new JsonObject()
            : JsonNode.Parse(instance.INSTANCE_DATA_JSON) as JsonObject ?? new JsonObject();

        var gates = root["gateConditions"] as JsonArray ?? [];

        gates.Add(new JsonObject
        {
            ["approvalId"] = gate.ApprovalId,
            ["decision"] = kind,
            ["conditions"] = conditions.Trim(),
            ["decidedBy"] = actor.Trim(),
            ["actingFor"] = gate.DelegatedFrom,
            ["decidedAtUtc"] = now.ToString("O", CultureInfo.InvariantCulture),
        });

        root["gateConditions"] = gates;
        instance.INSTANCE_DATA_JSON = root.ToJsonString();

        var unit = OgUow.For<PROCESS_INSTANCE>(source);
        unit.PrimaryKey = nameof(PROCESS_INSTANCE.PROCESS_INSTANCE_ID);

        var updated = unit.Update(instance);

        if (!OgUow.IsOk(updated))
        {
            _failures.ReportHandled(
                new InvalidOperationException(OgUow.Message(updated, "The instance data was not updated.")),
                "propagating the conditions to " + gate.InstanceId,
                "the approval records the conditions; the subject does not", FailureSeverity.Degraded);

            return;
        }

        var saved = await unit.Commit().ConfigureAwait(false);

        if (!OgUow.IsOk(saved))
        {
            _failures.ReportHandled(
                new InvalidOperationException(OgUow.Message(saved, "The instance data was not committed.")),
                "saving the conditions on " + gate.InstanceId,
                "the approval records the conditions; the subject does not", FailureSeverity.Degraded);
        }

        cancellationToken.ThrowIfCancellationRequested();
    }

    private static string BuildNotes(string kind, string rationale, string conditions)
    {
        var text = kind == GateRules.Approve && string.IsNullOrWhiteSpace(rationale)
            ? "Approved."
            : (string.IsNullOrWhiteSpace(rationale) ? kind : rationale.Trim());

        return string.IsNullOrWhiteSpace(conditions) ? text : text + " · conditions: " + conditions.Trim();
    }

    // ------------------------------------------------------------------ the read model

    private async Task<OgResult<OgGateWorkspace>> ReadAsync(
        IDataSource source,
        string? fieldId,
        string? approvalId,
        string actor,
        CancellationToken cancellationToken)
    {
        var approvals = await OgUow.ReadAllAsync<PROCESS_APPROVAL>(source).ConfigureAwait(false);
        var steps = await OgUow.ReadAllAsync<PROCESS_STEP_INSTANCE>(source).ConfigureAwait(false);
        var instances = await OgUow.ReadAllAsync<PROCESS_INSTANCE>(source).ConfigureAwait(false);
        var definitions = await OgUow.ReadAllAsync<PROCESS_DEFINITION>(source).ConfigureAwait(false);
        var rules = await OgUow.ReadAllAsync<SOD_RULE>(source).ConfigureAwait(false);
        var conflicts = await OgUow.ReadAllAsync<SOD_CONFLICT>(source).ConfigureAwait(false);

        cancellationToken.ThrowIfCancellationRequested();

        var definitionNames = definitions
            .GroupBy(row => row.PROCESS_DEFINITION_ID, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key,
                group => string.IsNullOrWhiteSpace(group.First().PROCESS_NAME) ? group.Key : group.First().PROCESS_NAME!,
                StringComparer.OrdinalIgnoreCase);

        var instanceById = instances
            .GroupBy(row => row.PROCESS_INSTANCE_ID, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);

        var stepById = steps
            .GroupBy(row => row.PROCESS_STEP_INSTANCE_ID, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);

        var scoped = approvals
            .Where(row => !string.IsNullOrWhiteSpace(row.PROCESS_APPROVAL_ID))
            .Where(row =>
            {
                var instance = row.PROCESS_INSTANCE_ID is { } id && instanceById.TryGetValue(id, out var found)
                    ? found
                    : null;

                return string.IsNullOrWhiteSpace(fieldId) ||
                       (instance is not null &&
                        string.Equals(instance.FIELD_ID?.Trim(), fieldId.Trim(), StringComparison.OrdinalIgnoreCase));
            })
            .ToList();

        var gates = scoped
            .Select(row => Build(row, instanceById, stepById, definitionNames, approvals, rules, conflicts))
            .OrderBy(gate => gate.IsDecided)
            .ThenByDescending(gate => gate.RequestedAt ?? DateTime.MinValue)
            .ThenBy(gate => gate.ApprovalId, StringComparer.Ordinal)
            .ToList();

        OgGateReview? selected = null;

        if (!string.IsNullOrWhiteSpace(approvalId))
        {
            selected = gates.FirstOrDefault(gate =>
                string.Equals(gate.ApprovalId, approvalId.Trim(), StringComparison.OrdinalIgnoreCase));

            if (selected is null)
            {
                return OgResult<OgGateWorkspace>.Failed(
                    "Gate '" + approvalId.Trim() + "' is not recorded.", GateRules.CodeUnknownGate);
            }
        }
        else
        {
            selected = gates.FirstOrDefault(gate => !gate.IsDecided);
        }

        _ = actor;

        return OgResult<OgGateWorkspace>.Ok(new OgGateWorkspace(
            string.IsNullOrWhiteSpace(fieldId) ? null : fieldId.Trim(),
            gates,
            selected,
            DateTime.UtcNow,
            DateTime.UtcNow));
    }

    private OgGateReview Build(
        PROCESS_APPROVAL row,
        IReadOnlyDictionary<string, PROCESS_INSTANCE> instances,
        IReadOnlyDictionary<string, PROCESS_STEP_INSTANCE> steps,
        IReadOnlyDictionary<string, string> definitionNames,
        IReadOnlyList<PROCESS_APPROVAL> approvals,
        IReadOnlyList<SOD_RULE> rules,
        IReadOnlyList<SOD_CONFLICT> conflicts)
    {
        var instance = row.PROCESS_INSTANCE_ID is { } instanceId && instances.TryGetValue(instanceId, out var found)
            ? found
            : null;

        var step = row.PROCESS_STEP_INSTANCE_ID is { } stepId && steps.TryGetValue(stepId, out var stepFound)
            ? stepFound
            : null;

        var status = (row.APPROVAL_STATUS ?? string.Empty).Trim();
        var decided = GateRules.DecidedStatuses.Contains(status, StringComparer.OrdinalIgnoreCase);

        var checklist = GateRules.ChecklistOf(approvals, row.PROCESS_STEP_INSTANCE_ID, row.PROCESS_APPROVAL_ID);

        var actor = row.APPROVER_USER_ID ?? row.DELEGATED_TO ?? string.Empty;

        var blocks = new List<string>();

        foreach (var item in checklist.Where(item => item.Required && !item.Satisfied))
        {
            blocks.Add(item.Key + " is not satisfied: " + item.Why + ".");
        }

        blocks.AddRange(GateRules.SodBlocks(actor, conflicts, rules));

        if (!string.IsNullOrWhiteSpace(instance?.ROW_CREATED_BY) &&
            string.Equals(instance!.ROW_CREATED_BY!.Trim(), actor, StringComparison.OrdinalIgnoreCase))
        {
            blocks.Add(instance.ROW_CREATED_BY + " authored this subject and therefore cannot approve it.");
        }

        var prior = approvals
            .Where(other => string.Equals(other.PROCESS_STEP_INSTANCE_ID, row.PROCESS_STEP_INSTANCE_ID,
                StringComparison.OrdinalIgnoreCase))
            .Where(other => !string.IsNullOrWhiteSpace(other.APPROVAL_STATUS))
            .Where(other => GateRules.DecidedStatuses.Contains(other.APPROVAL_STATUS!.Trim(), StringComparer.OrdinalIgnoreCase))
            .OrderBy(other => other.APPROVAL_DATE ?? DateTime.MinValue)
            .Select(other => new OgPriorDecision(
                (other.APPROVAL_STATUS ?? string.Empty).Trim(),
                other.APPROVED_BY ?? other.APPROVER_USER_ID ?? string.Empty,
                other.DELEGATED_BY,
                other.APPROVAL_DATE,
                other.APPROVAL_NOTES,
                null))
            .ToList();

        var due = ProcessInstanceRules.DueOf(step?.STARTED_DATE, step?.SLA_HOURS);

        return new OgGateReview(
            row.PROCESS_APPROVAL_ID,
            row.PROCESS_INSTANCE_ID ?? string.Empty,
            step?.STEP_ID,
            step?.STEP_NAME,
            instance is not null && definitionNames.TryGetValue(instance.PROCESS_DEFINITION_ID ?? string.Empty, out var name)
                ? name
                : instance?.PROCESS_DEFINITION_ID ?? string.Empty,
            instance?.ENTITY_TYPE ?? string.Empty,
            instance?.ENTITY_ID ?? string.Empty,
            string.IsNullOrWhiteSpace(row.REQUIRED_ACTION) ? "GATE" : row.REQUIRED_ACTION!.Trim(),
            row.REQUESTED_BY,
            row.REQUESTED_DATE,
            ProcessDashboardRules.DaysBetween(row.REQUESTED_DATE, DateTime.UtcNow),
            due is null ? "unscheduled" : due.Value.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture),
            string.IsNullOrWhiteSpace(row.DELEGATED_FROM) ? null : row.DELEGATED_FROM,
            string.IsNullOrWhiteSpace(row.DELEGATED_TO) ? null : row.DELEGATED_TO,
            string.IsNullOrWhiteSpace(status) ? GateRules.Pending : status,
            checklist,
            prior,
            instance?.ROW_CREATED_BY,
            decided ? [] : blocks);
    }
}
