using System.Globalization;
using Beep.OilandGas.LifeCycle.Data.Tables;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Data.Workflow;

/// <summary>
/// One point of an instance's timeline (WFL-03's <c>OgProcessEvent</c>): a step that was reached or completed, or an event the
/// engine recorded.
/// </summary>
/// <param name="ZoneConfirmed">
/// True only when the recorded timestamp carries a confirmed zone (a UTC instant). The owner stores no zone with a history
/// timestamp, so a stored value arrives unspecified and is shown as unconfirmed rather than being assumed.
/// </param>
/// <param name="ActorRecorded">False when the record names nobody, which is stated rather than left blank.</param>
public sealed record OgProcessEvent(
    string Kind,
    string StepKey,
    string Outcome,
    string Actor,
    DateTime? At,
    string? Comment,
    bool ZoneConfirmed,
    bool ActorRecorded)
{
    public string ZoneText => At is null
        ? "no timestamp"
        : ZoneConfirmed
            ? "confirmed"
            : "unconfirmed";

    public string Describe() =>
        (At is { } at ? at.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture) + " (" + ZoneText + ")" : "(no time)") +
        " · " + Kind.ToLowerInvariant() + " · " + StepKey +
        (string.IsNullOrWhiteSpace(Outcome) ? string.Empty : " · " + Outcome) +
        (ActorRecorded ? " · " + Actor : " · no actor recorded") +
        (string.IsNullOrWhiteSpace(Comment) ? string.Empty : " · " + Comment);
}

/// <summary>One instance as the grid lists it (WFL-03's <c>OgProcessInstance</c>).</summary>
public sealed record OgProcessInstanceView(
    string Id,
    string DefinitionId,
    string DefinitionName,
    string SubjectKind,
    string SubjectId,
    string State,
    string? CurrentStepKey,
    string? CurrentStepName,
    string Assignee,
    DateTime? StartedAt,
    DateTime? DueAt,
    decimal? Age,
    string SlaState)
{
    /// <summary>AC-2: a due date exists only when the step records an SLA; otherwise the work is unscheduled, never overdue.</summary>
    public bool IsUnscheduled => string.Equals(SlaState, ProcessDashboardRules.Unscheduled, StringComparison.Ordinal) ||
                                 string.Equals(SlaState, ProcessDashboardRules.NoSla, StringComparison.Ordinal);

    public string DueText => IsUnscheduled
        ? "unscheduled"
        : DueAt?.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture) ?? "unscheduled";

    public string AgeText => Age is { } days
        ? days.ToString("0.0#", CultureInfo.InvariantCulture) + " day(s)"
        : "not started";

    public string SubjectText => string.IsNullOrWhiteSpace(SubjectKind) || string.IsNullOrWhiteSpace(SubjectId)
        ? "no subject recorded"
        : SubjectKind + " " + SubjectId;

    public string Describe() =>
        Id + " · " + DefinitionName +
        (string.IsNullOrWhiteSpace(CurrentStepName) ? string.Empty : " · " + CurrentStepName) +
        " · " + (string.IsNullOrWhiteSpace(State) ? "no state" : State.ToLowerInvariant()) +
        " · " + SubjectText + " · " + AgeText + " · due " + DueText + " · " + SlaState;
}

/// <summary>The instance and its timeline (WFL-03).</summary>
public sealed record OgProcessInstanceWorkspace(
    string? FieldId,
    IReadOnlyList<OgProcessInstanceView> Instances,
    OgProcessInstanceView? Selected,
    IReadOnlyList<OgProcessEvent> Timeline,
    IReadOnlyList<OgProcessEvent> History,
    string? SubjectWarning,
    IReadOnlyList<string> Blocks,
    DateTime AsOf,
    DateTime ReadAt)
{
    /// <summary>AC-4: the timeline holds every step of the instance and every event recorded against it.</summary>
    public int Events => Timeline.Count;

    public int Unconfirmed => Timeline.Count(entry => entry.At is not null && !entry.ZoneConfirmed);

    public int WithoutActor => Timeline.Count(entry => !entry.ActorRecorded);

    public string CountsText =>
        Instances.Count.ToString(CultureInfo.InvariantCulture) + " instance(s) · " +
        (Selected is null ? "none selected" : "selected " + Selected.Id) + " · " +
        Events.ToString(CultureInfo.InvariantCulture) + " timeline point(s) · " +
        Unconfirmed.ToString(CultureInfo.InvariantCulture) + " unconfirmed timestamp(s) · " +
        WithoutActor.ToString(CultureInfo.InvariantCulture) + " without an actor";

    public string InstancesText => Instances.Count == 0
        ? "No process instance is recorded for this view."
        : string.Join(Environment.NewLine, Instances.Select(instance => instance.Describe()));

    public string TimelineText => Timeline.Count == 0
        ? "No step or event is recorded for this instance."
        : string.Join(Environment.NewLine, Timeline.Select(entry => entry.Describe()));

    public string HistoryText => History.Count == 0
        ? "No history entry is recorded."
        : string.Join(Environment.NewLine, History.Select(entry => entry.Describe()));

    /// <summary>AC-6: the originating record, and what is wrong with the link when it cannot be reached.</summary>
    public string SubjectText => SubjectWarning ?? Selected?.SubjectText ?? "no instance is selected";

    public string BlocksText => Blocks.Count == 0
        ? "Nothing is missing from this instance's record."
        : string.Join(Environment.NewLine, Blocks.Select(block => "✗ " + block));

    public string ReadText =>
        "read at " + ReadAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
}

/// <summary>The instance rules WFL-03 states once.</summary>
public static class ProcessInstanceRules
{
    public const string CodeNoConnection = "instance-no-connection";
    public const string CodeReadFailed = "instance-read-failed";
    public const string CodeIdentityRequired = "instance-identity-required";
    public const string CodeUnknownInstance = "instance-unknown";
    public const string CodeAmbiguousInstance = "instance-ambiguous";
    public const string CodeSubjectAmbiguous = "instance-subject-ambiguous";
    public const string CodeSubjectMissing = "instance-subject-missing";

    /// <summary>
    /// AC-1: an instance is resolved by its exact identity. Two rows with the same identity are an ambiguity the caller must
    /// resolve, and nothing is chosen by recency - the defect the Web had.
    /// </summary>
    public static (OgProcessInstanceView? Instance, string Code, string Message) SelectExact(
        IReadOnlyList<OgProcessInstanceView> candidates,
        string? instanceId)
    {
        ArgumentNullException.ThrowIfNull(candidates);

        if (string.IsNullOrWhiteSpace(instanceId))
        {
            return (null, CodeIdentityRequired,
                "A process instance is opened by its identity; none is named.");
        }

        var matches = candidates
            .Where(candidate => string.Equals(candidate.Id, instanceId.Trim(), StringComparison.OrdinalIgnoreCase))
            .ToList();

        return matches.Count switch
        {
            0 => (null, CodeUnknownInstance, "Process instance '" + instanceId!.Trim() + "' is not recorded."),
            1 => (matches[0], string.Empty, string.Empty),
            _ => (null, CodeAmbiguousInstance,
                matches.Count.ToString(CultureInfo.InvariantCulture) + " instances are recorded as '" + instanceId!.Trim() +
                "', so the identity is ambiguous and none is chosen. The newest is deliberately not preferred."),
        };
    }

    /// <summary>
    /// AC-1 for a subject: several open instances of the same document is an ambiguity, and the answer names them.
    /// </summary>
    public static (OgProcessInstanceView? Instance, string Code, string Message) SelectForSubject(
        IReadOnlyList<OgProcessInstanceView> candidates,
        string? subjectKind,
        string? subjectId)
    {
        ArgumentNullException.ThrowIfNull(candidates);

        if (string.IsNullOrWhiteSpace(subjectKind) || string.IsNullOrWhiteSpace(subjectId))
        {
            return (null, CodeIdentityRequired,
                "An instance is opened by its own identity, or by the exact subject it belongs to; none is named.");
        }

        var matches = candidates
            .Where(candidate => string.Equals(candidate.SubjectKind, subjectKind.Trim(), StringComparison.OrdinalIgnoreCase) &&
                                string.Equals(candidate.SubjectId, subjectId.Trim(), StringComparison.OrdinalIgnoreCase))
            .ToList();

        return matches.Count switch
        {
            0 => (null, CodeSubjectMissing,
                "No process instance is recorded for " + subjectKind.Trim() + " " + subjectId.Trim() + "."),
            1 => (matches[0], string.Empty, string.Empty),
            _ => (null, CodeSubjectAmbiguous,
                subjectKind.Trim() + " " + subjectId.Trim() + " has " +
                matches.Count.ToString(CultureInfo.InvariantCulture) + " process instances (" +
                string.Join(", ", matches.Select(match => match.Id)) +
                "), so the choice is ambiguous and none is chosen."),
        };
    }

    /// <summary>AC-2: the due date is the current step's start plus its SLA, and nothing else.</summary>
    public static DateTime? DueOf(DateTime? stepStartedAt, double? slaHours) =>
        stepStartedAt is { } start && slaHours is > 0
            ? start.AddHours(slaHours.Value)
            : null;

    /// <summary>
    /// AC-3: a timestamp is confirmed only when it carries a zone. The owner records no zone on a step or history timestamp, so
    /// a stored value is shown as unconfirmed rather than being read as UTC.
    /// </summary>
    public static bool ZoneConfirmed(DateTime? at) => at?.Kind == DateTimeKind.Utc;

    /// <summary>
    /// AC-4: the timeline merges the instance's steps and its recorded events into one ordered list. Ordering is by the
    /// timestamp, then by the step sequence, then by the text, so a tie is stable rather than arbitrary.
    /// </summary>
    public static IReadOnlyList<OgProcessEvent> Timeline(
        IEnumerable<(int Sequence, OgProcessEvent Event)> steps,
        IEnumerable<OgProcessEvent> history)
    {
        ArgumentNullException.ThrowIfNull(steps);
        ArgumentNullException.ThrowIfNull(history);

        return steps
            .OrderBy(pair => pair.Event.At ?? DateTime.MinValue)
            .ThenBy(pair => pair.Sequence)
            .Select(pair => pair.Event)
            .Concat(history.OrderBy(entry => entry.At ?? DateTime.MinValue))
            .OrderBy(entry => entry.At ?? DateTime.MinValue)
            .ThenBy(entry => entry.Kind, StringComparer.Ordinal)
            .ThenBy(entry => entry.StepKey, StringComparer.Ordinal)
            .ToList();
    }
}

/// <summary>WFL-03: the instance list, one instance's timeline and its history.</summary>
public interface IOgProcessInstances
{
    /// <param name="instanceId">The instance to open by its exact identity.</param>
    /// <param name="subjectKind">The instance to open by its subject, when no identity is given.</param>
    Task<OgResult<OgProcessInstanceWorkspace>> LoadAsync(
        string? fieldId,
        string? instanceId,
        string? subjectKind,
        string? subjectId,
        DateTime asOf,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// WFL-03 over the owner's <c>PROCESS_INSTANCE</c>, <c>PROCESS_STEP_INSTANCE</c>, <c>PROCESS_HISTORY</c> and
/// <c>PROCESS_DEFINITION</c>.
/// </summary>
/// <remarks>
/// <para>
/// Nothing is chosen by recency: an identity that matches more than one row is refused with the ambiguity named, and so is a
/// subject that owns more than one instance (AC-1). The due date is the current step's start plus its recorded SLA, so a step with
/// no SLA is <b>unscheduled</b> rather than overdue (AC-2), and a timestamp is only <b>confirmed</b> when it carries a zone (AC-3).
/// </para>
/// <para>
/// The timeline merges the instance's steps and its history into one ordered list, so it cannot be missing either half (AC-4),
/// every entry states its actor or says none is recorded (AC-5), and the originating record is looked up in the data source's own
/// entity list, so a broken link is shown rather than the subject being hidden (AC-6).
/// </para>
/// </remarks>
public sealed class OgProcessInstancesService : IOgProcessInstances
{
    private readonly Func<IDataSource?> _source;
    private readonly IFailureReporter _failures;

    public OgProcessInstancesService(Func<IDataSource?> source, IFailureReporter failures)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _failures = failures ?? throw new ArgumentNullException(nameof(failures));
    }

    public async Task<OgResult<OgProcessInstanceWorkspace>> LoadAsync(
        string? fieldId,
        string? instanceId,
        string? subjectKind,
        string? subjectId,
        DateTime asOf,
        CancellationToken cancellationToken = default)
    {
        var source = _source();

        if (source is null)
        {
            return OgResult<OgProcessInstanceWorkspace>.Failed(
                "No connection is open, so the process instances cannot be read.",
                ProcessInstanceRules.CodeNoConnection);
        }

        try
        {
            var instances = await OgUow.ReadAllAsync<PROCESS_INSTANCE>(source).ConfigureAwait(false);
            var steps = await OgUow.ReadAllAsync<PROCESS_STEP_INSTANCE>(source).ConfigureAwait(false);
            var history = await OgUow.ReadAllAsync<PROCESS_HISTORY>(source).ConfigureAwait(false);
            var definitions = await OgUow.ReadAllAsync<PROCESS_DEFINITION>(source).ConfigureAwait(false);

            cancellationToken.ThrowIfCancellationRequested();

            var definitionNames = definitions
                .GroupBy(row => row.PROCESS_DEFINITION_ID, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(group => group.Key,
                    group => string.IsNullOrWhiteSpace(group.First().PROCESS_NAME)
                        ? group.Key
                        : group.First().PROCESS_NAME!,
                    StringComparer.OrdinalIgnoreCase);

            var scoped = instances
                .Where(row => string.IsNullOrWhiteSpace(fieldId) ||
                              string.Equals(row.FIELD_ID?.Trim(), fieldId.Trim(), StringComparison.OrdinalIgnoreCase))
                .ToList();

            var views = scoped
                .Select(row =>
                {
                    var mine = steps
                        .Where(step => string.Equals(step.PROCESS_INSTANCE_ID, row.PROCESS_INSTANCE_ID,
                            StringComparison.OrdinalIgnoreCase))
                        .ToList();

                    var current = mine
                        .Where(step => step.COMPLETION_DATE is null)
                        .OrderByDescending(step => step.STEP_SEQUENCE)
                        .FirstOrDefault();

                    var due = ProcessInstanceRules.DueOf(current?.STARTED_DATE, current?.SLA_HOURS);

                    return new OgProcessInstanceView(
                        row.PROCESS_INSTANCE_ID,
                        row.PROCESS_DEFINITION_ID ?? string.Empty,
                        definitionNames.TryGetValue(row.PROCESS_DEFINITION_ID ?? string.Empty, out var name)
                            ? name
                            : row.PROCESS_DEFINITION_ID ?? string.Empty,
                        row.ENTITY_TYPE ?? string.Empty,
                        row.ENTITY_ID ?? string.Empty,
                        row.STATUS ?? string.Empty,
                        current?.STEP_ID ?? row.CURRENT_STEP_ID,
                        current?.STEP_NAME,
                        current?.ASSIGNED_TO ?? string.Empty,
                        row.STARTED_DATE,
                        due,
                        ProcessDashboardRules.DaysBetween(row.STARTED_DATE, asOf),
                        ProcessDashboardRules.SlaState(current?.SLA_HOURS, current?.STARTED_DATE, asOf));
                })
                .OrderBy(view => view.DefinitionName, StringComparer.Ordinal)
                .ThenBy(view => view.Id, StringComparer.Ordinal)
                .ToList();

            OgProcessInstanceView? selected = null;

            if (!string.IsNullOrWhiteSpace(instanceId))
            {
                // AC-1: the identity decides; an ambiguity is refused rather than resolved by recency.
                var (found, code, message) = ProcessInstanceRules.SelectExact(views, instanceId);

                if (found is null)
                {
                    return OgResult<OgProcessInstanceWorkspace>.Failed(message, code);
                }

                selected = found;
            }
            else if (!string.IsNullOrWhiteSpace(subjectKind) || !string.IsNullOrWhiteSpace(subjectId))
            {
                var (found, code, message) = ProcessInstanceRules.SelectForSubject(views, subjectKind, subjectId);

                if (found is null)
                {
                    return OgResult<OgProcessInstanceWorkspace>.Failed(message, code);
                }

                selected = found;
            }

            var stepPairs = new List<(int Sequence, OgProcessEvent Event)>();
            var recorded = new List<OgProcessEvent>();
            string? subjectWarning = null;
            var blocks = new List<string>();

            if (selected is not null)
            {
                var mineSteps = steps
                    .Where(step => string.Equals(step.PROCESS_INSTANCE_ID, selected.Id, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(step => step.STEP_SEQUENCE)
                    .ToList();

                stepPairs = mineSteps
                    .Select(step => (Sequence: step.STEP_SEQUENCE, Event: new OgProcessEvent(
                        "STEP",
                        string.IsNullOrWhiteSpace(step.STEP_NAME) ? step.STEP_ID : step.STEP_NAME,
                        step.COMPLETION_DATE is null
                            ? (string.IsNullOrWhiteSpace(step.STATUS) ? "in progress" : step.STATUS!)
                            : (string.IsNullOrWhiteSpace(step.OUTCOME) ? "completed" : step.OUTCOME!),
                        First(step.COMPLETED_BY, step.ASSIGNED_TO) ?? string.Empty,
                        step.COMPLETION_DATE ?? step.STARTED_DATE,
                        string.IsNullOrWhiteSpace(step.NOTES) ? null : step.NOTES,
                        ProcessInstanceRules.ZoneConfirmed(step.COMPLETION_DATE ?? step.STARTED_DATE),
                        !string.IsNullOrWhiteSpace(First(step.COMPLETED_BY, step.ASSIGNED_TO)))))
                    .ToList();

                recorded = history
                    .Where(entry => string.Equals(entry.PROCESS_INSTANCE_ID, selected.Id, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(entry => entry.EVENT_DATE)
                    .Select(entry => new OgProcessEvent(
                        "EVENT",
                        string.IsNullOrWhiteSpace(entry.EVENT_TYPE) ? "(instance)" : entry.EVENT_TYPE!,
                        string.IsNullOrWhiteSpace(entry.TO_STATUS) ? string.Empty : entry.TO_STATUS!,
                        entry.USER_ID ?? string.Empty,
                        entry.EVENT_DATE,
                        string.IsNullOrWhiteSpace(entry.DETAILS) ? null : entry.DETAILS,
                        ProcessInstanceRules.ZoneConfirmed(entry.EVENT_DATE),
                        !string.IsNullOrWhiteSpace(entry.USER_ID)))
                    .ToList();

                // AC-6: the originating record is reachable only when the data source knows its entity type.
                var entities = source.GetEntitesList()?
                    .Where(entity => !string.IsNullOrWhiteSpace(entity))
                    .ToHashSet(StringComparer.OrdinalIgnoreCase) ?? [];

                if (string.IsNullOrWhiteSpace(selected.SubjectKind) || string.IsNullOrWhiteSpace(selected.SubjectId))
                {
                    subjectWarning = "The instance records no originating subject, so there is no record to open.";
                }
                else if (!entities.Contains(selected.SubjectKind))
                {
                    subjectWarning =
                        "The originating record cannot be reached: no entity type named '" + selected.SubjectKind +
                        "' is available in this data source, so " + selected.SubjectId + " cannot be opened.";
                }

                if (string.IsNullOrWhiteSpace(selected.State))
                {
                    blocks.Add("The instance records no status.");
                }

                if (string.IsNullOrWhiteSpace(selected.CurrentStepKey))
                {
                    blocks.Add("The instance has no open step, so no due date or assignee can be read.");
                }

                foreach (var entry in stepPairs.Select(pair => pair.Event).Concat(recorded)
                             .Where(entry => entry.At is not null && !entry.ZoneConfirmed))
                {
                    blocks.Add("The timestamp of '" + entry.StepKey +
                               "' carries no confirmed zone, so it is shown as unconfirmed.");
                }

                foreach (var entry in stepPairs.Select(pair => pair.Event).Concat(recorded)
                             .Where(entry => !entry.ActorRecorded))
                {
                    blocks.Add("'" + entry.StepKey + "' records no actor.");
                }
            }

            var ordered = ProcessInstanceRules.Timeline(stepPairs, recorded);

            return OgResult<OgProcessInstanceWorkspace>.Ok(new OgProcessInstanceWorkspace(
                string.IsNullOrWhiteSpace(fieldId) ? null : fieldId.Trim(),
                views,
                selected,
                ordered,
                recorded,
                subjectWarning,
                blocks.Distinct(StringComparer.Ordinal).ToList(),
                asOf,
                DateTime.UtcNow));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, "reading the process instances", "the instances are not shown", FailureSeverity.Error);

            return OgResult<OgProcessInstanceWorkspace>.Failed(
                "The process instances could not be read" +
                (string.IsNullOrWhiteSpace(reference) ? "." : " (reference " + reference + ")."),
                reference ?? ProcessInstanceRules.CodeReadFailed);
        }
    }

    private static string? First(params string?[] values) =>
        values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value))?.Trim();
}
