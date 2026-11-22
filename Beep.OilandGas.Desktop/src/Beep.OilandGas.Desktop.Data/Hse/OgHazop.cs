using System.Globalization;
using System.Text.Json;
using Beep.OilandGas.Models.Constants;
using Beep.OilandGas.PPDM39.Models;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Data.Hse;

/// <summary>One HAZOP action (HSE-04).</summary>
public sealed record OgHazopAction(
    string StepId,
    string Action,
    string? Owner,
    DateTime? DueDate,
    string State,
    bool Overdue)
{
    public string DueText => DueDate is { } due
        ? due.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) + (Overdue ? " (overdue)" : string.Empty)
        : "no due date";

    public string Describe() =>
        Action + " · owner " + (Owner ?? "(none)") + " · due " + DueText + " · " + State.ToLowerInvariant();
}

/// <summary>One HAZOP deviation (HSE-04), matching the feature's <c>OgHazopDeviation</c>.</summary>
public sealed record OgHazopDeviation(
    int CondSeq,
    string GuideWord,
    string Parameter,
    string Cause,
    string Consequence,
    string Safeguard,
    string Risk,
    string State,
    OgHazopAction? Action)
{
    public string Describe() =>
        CondSeq.ToString(CultureInfo.InvariantCulture) + " · " +
        (string.IsNullOrWhiteSpace(Parameter) ? GuideWord : GuideWord + " " + Parameter) +
        " · " + Consequence + " · " + State.ToLowerInvariant() +
        (string.IsNullOrWhiteSpace(Risk) ? string.Empty : " · risk " + Risk) +
        (Action is null ? " · no action" : " · action " + Action.Describe());
}

/// <summary>One HAZOP node (HSE-04).</summary>
public sealed record OgHazopNode(
    string StepId,
    int Seq,
    string Name,
    string DesignIntent,
    IReadOnlyList<OgHazopDeviation> Deviations)
{
    public string Describe() =>
        Seq.ToString(CultureInfo.InvariantCulture) + " · " + Name +
        (string.IsNullOrWhiteSpace(DesignIntent) ? string.Empty : " · " + DesignIntent) +
        " · " + Deviations.Count.ToString(CultureInfo.InvariantCulture) + " deviation(s)";
}

/// <summary>One HAZOP study (HSE-04), matching the feature's <c>OgHazopStudy</c>.</summary>
public sealed record OgHazopStudy(
    string Id,
    string Name,
    string Scope,
    string? FieldId,
    string? FacilityId,
    string State,
    DateTime? StartDate,
    IReadOnlyList<OgHazopNode> Nodes)
{
    public int NodeCount => Nodes.Count;

    public int DeviationCount => Nodes.Sum(node => node.Deviations.Count);

    public int OpenActions => Nodes
        .SelectMany(node => node.Deviations)
        .Count(deviation => deviation.Action is { } action && !string.Equals(action.State,
            HseReferenceCodes.CorrectiveAction.Completed, StringComparison.OrdinalIgnoreCase));

    public string HeaderText =>
        Name + " · " + (string.IsNullOrWhiteSpace(FieldId) ? "no field" : "field " + FieldId) +
        (string.IsNullOrWhiteSpace(FacilityId) ? string.Empty : " · facility " + FacilityId) +
        " · " + NodeCount.ToString(CultureInfo.InvariantCulture) + " node(s) · " +
        DeviationCount.ToString(CultureInfo.InvariantCulture) + " deviation(s) · " +
        OpenActions.ToString(CultureInfo.InvariantCulture) + " open action(s) · " + State.ToLowerInvariant() +
        (StartDate is { } started ? " · started " + started.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
            : string.Empty);

    public string NodesText => Nodes.Count == 0
        ? "No node is recorded."
        : string.Join(Environment.NewLine, Nodes.Select(node => node.Describe()));

    public string DeviationsText => DeviationCount == 0
        ? "No deviation is recorded."
        : string.Join(Environment.NewLine,
            Nodes.SelectMany(node => node.Deviations.Select(deviation =>
                node.Name + " · " + deviation.Describe())));
}

/// <summary>The HAZOP workspace (HSE-04).</summary>
public sealed record OgHazopWorkspace(
    OgHazopStudy Study,
    OgHazopNode? SelectedNode,
    OgHazopDeviation? SelectedDeviation,
    IReadOnlyList<string> GuideWords,
    IReadOnlyList<string> RiskRatings,
    IReadOnlyList<string> StudyStates,
    IReadOnlyList<string> LegalNextStates,
    DateTime ReadAt)
{
    public string ReadText =>
        "read at " + ReadAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

    public string ReferenceText =>
        "guide words " + string.Join(", ", GuideWords) + " · risk " + string.Join(", ", RiskRatings) +
        " · legal next study states " +
        (LegalNextStates.Count == 0 ? "(none - the study is closed)" : string.Join(", ", LegalNextStates));
}

/// <summary>The HAZOP rules HSE-04 states once.</summary>
public static class HazopRules
{
    public const string CodeNoConnection = "hazop-no-connection";
    public const string CodeReadFailed = "hazop-read-failed";
    public const string CodeWriteFailed = "hazop-write-failed";
    public const string CodeActorRequired = "hazop-actor-required";
    public const string CodeInputRequired = "hazop-input-required";
    public const string CodeInputInvalid = "hazop-input-invalid";
    public const string CodeStudyNotFound = "hazop-study-not-found";
    public const string CodeStudyExists = "hazop-study-exists";
    public const string CodeScopeChanged = "hazop-scope-changed";
    public const string CodeDuplicateNodeKey = "hazop-duplicate-node-key";
    public const string CodeUnknownNode = "hazop-unknown-node";
    public const string CodeUnknownDeviation = "hazop-unknown-deviation";
    public const string CodeActionNeedsNodeAndDeviation = "hazop-action-needs-node-and-deviation";
    public const string CodeTransitionIllegal = "hazop-transition-illegal";
    public const string CodeUnconfirmed = "hazop-unconfirmed";

    /// <summary>
    /// The standard HAZOP guide words. The owner's HAZOP tables carry the guide word in
    /// <c>PROJECT_CONDITION.CONDITION_TYPE</c> but do not name the vocabulary, so it is stated here.
    /// </summary>
    public static IReadOnlyList<string> GuideWords { get; } =
        ["NO", "MORE", "LESS", "AS_WELL_AS", "PART_OF", "REVERSE", "OTHER_THAN"];

    public static IReadOnlyList<string> RiskRatings { get; } = ["HIGH", "MEDIUM", "LOW"];

    /// <summary>The owner's study states, from <see cref="HseReferenceCodes.Hazop"/>.</summary>
    public static IReadOnlyList<string> StudyStates { get; } =
        [HseReferenceCodes.Hazop.Draft, HseReferenceCodes.Hazop.Open, HseReferenceCodes.Hazop.Closed];

    /// <summary>The owner's deviation states (OPEN, ACTION_REQUIRED, CLOSED).</summary>
    public static IReadOnlyList<string> DeviationStates { get; } =
    [
        HseReferenceCodes.Hazop.Open,
        HseReferenceCodes.Hazop.ActionRequired,
        HseReferenceCodes.Hazop.Closed,
    ];

    /// <summary>AC-6: the states a study may move to from its current one, and nothing else.</summary>
    public static IReadOnlyList<string> NextStates(string? state) => state?.Trim().ToUpperInvariant() switch
    {
        HseReferenceCodes.Hazop.Draft => [HseReferenceCodes.Hazop.Open],
        HseReferenceCodes.Hazop.Open => [HseReferenceCodes.Hazop.Closed],
        _ => [],
    };

    /// <summary>AC-6 for a deviation: the legal next states of the four-state deviation lifecycle.</summary>
    public static IReadOnlyList<string> NextDeviationStates(string? state) =>
        state?.Trim().ToUpperInvariant() switch
        {
            HseReferenceCodes.Hazop.Open =>
                [HseReferenceCodes.Hazop.ActionRequired, HseReferenceCodes.Hazop.Closed],
            HseReferenceCodes.Hazop.ActionRequired => [HseReferenceCodes.Hazop.Closed],
            _ => [],
        };

    /// <summary>AC-4 and SG-11-5: an action needs its node and its deviation.</summary>
    public static (string Code, string Message)? ValidateAction(
        string? nodeStepId,
        int? condSeq,
        string? action,
        string? owner,
        DateTime? dueDate)
    {
        if (string.IsNullOrWhiteSpace(nodeStepId) || condSeq is null or <= 0)
        {
            return (CodeActionNeedsNodeAndDeviation,
                "A HAZOP action must name the node and the deviation it answers; an action without both cannot be traced " +
                "to the study.");
        }

        if (string.IsNullOrWhiteSpace(action))
        {
            return (CodeInputRequired, "A HAZOP action needs its description.");
        }

        if (string.IsNullOrWhiteSpace(owner))
        {
            return (CodeInputRequired, "A HAZOP action needs the person responsible for it.");
        }

        return dueDate is null
            ? (CodeInputRequired, "A HAZOP action needs its due date.")
            : null;
    }

    /// <summary>Validates a deviation (guide word, parameter, consequence).</summary>
    public static (string Code, string Message)? ValidateDeviation(
        string? guideWord,
        string? parameter,
        string? consequence)
    {
        if (string.IsNullOrWhiteSpace(guideWord) ||
            !GuideWords.Contains(guideWord.Trim().ToUpperInvariant(), StringComparer.Ordinal))
        {
            return (CodeInputInvalid, $"A guide word is {string.Join(", ", GuideWords)}.");
        }

        if (string.IsNullOrWhiteSpace(parameter))
        {
            return (CodeInputRequired, "A deviation needs the parameter it applies to.");
        }

        return string.IsNullOrWhiteSpace(consequence)
            ? (CodeInputRequired, "A deviation needs its consequence.")
            : null;
    }

    /// <summary>AC-6: a transition must be one of the legal next states.</summary>
    public static (string Code, string Message)? CheckTransition(string? current, string? target)
    {
        var legal = NextStates(current);

        return legal.Contains(target?.Trim().ToUpperInvariant() ?? string.Empty, StringComparer.Ordinal)
            ? null
            : (CodeTransitionIllegal,
                "A study in " + (current ?? "(no state)") + " may move to " +
                (legal.Count == 0 ? "nothing - it is closed" : string.Join(", ", legal)) + ", not " +
                (target ?? "(none)") + ".");
    }

    /// <summary>
    /// AC-2: the caller acknowledges the state it reviewed. A different state means the acknowledgement is unconfirmed, and
    /// the command is refused rather than applied to a record the person did not see.
    /// </summary>
    public static (string Code, string Message)? CheckAcknowledged(string? expected, string? actual, string what)
    {
        if (string.IsNullOrWhiteSpace(expected))
        {
            return (CodeUnconfirmed,
                "The " + what + " acknowledgement is missing, so the change is unconfirmed and was not applied. Load the " +
                what + " again and confirm the state you reviewed.");
        }

        return string.Equals(expected.Trim(), actual?.Trim(), StringComparison.OrdinalIgnoreCase)
            ? null
            : (CodeUnconfirmed,
                "The " + what + " acknowledgement does not match: you reviewed " + expected + " but it is now " +
                (actual ?? "(no state)") + ". The change is unconfirmed and was not applied; load it again.");
    }

    /// <summary>The deviation status id the owner uses, so a status row can be found and updated.</summary>
    public static string DeviationStatusId(string stepId, decimal condSeq) =>
        stepId + "-DEV-" + condSeq.ToString("000", CultureInfo.InvariantCulture);

    /// <summary>The study status id the owner uses.</summary>
    public static string StudyStatusId(string studyId) => studyId + "-STUDY";
}

/// <summary>HSE-04: a HAZOP study - nodes, deviations, actions and its legal transitions.</summary>
public interface IOgHazop
{
    /// <summary>Starts a load; a later load invalidates this one, so a late response is discarded (AC-5).</summary>
    long BeginLoad();

    /// <summary>False when a newer load has started (AC-5).</summary>
    bool IsCurrent(long version);

    /// <param name="currentScope">
    /// The session's company and field now; a change during the read clears the study and discards the response (AC-5).
    /// </param>
    Task<OgResult<OgHazopWorkspace>> LoadAsync(
        long version,
        string studyId,
        OgHseScope scope,
        Func<OgHseScope> currentScope,
        CancellationToken cancellationToken = default);

    /// <summary>Creates a study with its explicit id, field context and DRAFT state.</summary>
    Task<OgResult<OgHazopWorkspace>> CreateStudyAsync(
        string studyId,
        string studyName,
        string scopeText,
        string fieldId,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>Adds a node; a duplicate node key is refused (AC-3).</summary>
    Task<OgResult<OgHazopWorkspace>> AddNodeAsync(
        string studyId,
        string nodeName,
        string designIntent,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>Adds a deviation to a node.</summary>
    Task<OgResult<OgHazopWorkspace>> AddDeviationAsync(
        string studyId,
        string nodeStepId,
        string guideWord,
        string parameter,
        string cause,
        string consequence,
        string safeguard,
        string risk,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>Raises an action for a node and a deviation (AC-4, SG-11-5).</summary>
    Task<OgResult<OgHazopWorkspace>> AddActionAsync(
        string studyId,
        string nodeStepId,
        int condSeq,
        string action,
        string owner,
        DateTime dueDate,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>Moves the study to a legal next state, if the reviewed state is still current (AC-2, AC-6).</summary>
    Task<OgResult<OgHazopWorkspace>> TransitionStudyAsync(
        string studyId,
        string target,
        string acknowledgedState,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>Moves a deviation to a legal next state (AC-2, AC-6).</summary>
    Task<OgResult<OgHazopWorkspace>> TransitionDeviationAsync(
        string studyId,
        string nodeStepId,
        int condSeq,
        string target,
        string acknowledgedState,
        string actor,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// HSE-04 over the owner's <c>PROJECT</c>, <c>PROJECT_COMPONENT</c>, <c>PROJECT_STEP</c>, <c>PROJECT_STEP_XREF</c>,
/// <c>PROJECT_CONDITION</c>, <c>PROJECT_STATUS</c> and <c>PROJECT_STEP_BA</c>.
/// </summary>
/// <remarks>
/// The owner's shape is kept exactly: a study is a <c>PROJECT</c> of type HAZOP with its field in a
/// <c>PROJECT_COMPONENT</c>; a node is a <c>PROJECT_STEP</c> of type HAZOP_NODE; a deviation is a
/// <c>PROJECT_CONDITION</c> whose guide word is <c>CONDITION_TYPE</c>, whose node is <c>SOURCE</c>, and whose owner payload
/// is the two-field JSON in <c>REMARK</c>. The desktop's extra fields (parameter, cause, safeguard, risk) go in the
/// condition's otherwise-unused <c>DESCRIPTION</c> as <c>key=value</c> lines, so the owner's own reader still parses the
/// payload unchanged. An action is a <c>PROJECT_STEP</c> of type HAZOP_ACTION with a <c>PROJECT_STEP_XREF</c> to its node,
/// its responsible person in <c>PROJECT_STEP_BA</c>, and the deviation number in its own <c>REMARK</c>.
/// </remarks>
public sealed class OgHazop : IOgHazop
{
    private const string HazopType = HseReferenceCodes.Hazop.ProjectType;
    private const string NodeType = HseReferenceCodes.Hazop.NodeType;
    private const string StudyStatusType = HseReferenceCodes.Hazop.StudyStatusType;
    private const string DeviationStatusType = HseReferenceCodes.Hazop.DeviationStatusType;
    private const string ActionType = "HAZOP_ACTION";
    private const string NodeXrefType = "HAZOP_ACTION_NODE";
    private const string FieldSource = HseReferenceCodes.Hazop.FieldContextSource;

    private readonly Func<IDataSource?> _source;
    private readonly IFailureReporter _failures;
    private long _version;

    public OgHazop(Func<IDataSource?> source, IFailureReporter failures)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _failures = failures ?? throw new ArgumentNullException(nameof(failures));
    }

    public long BeginLoad() => Interlocked.Increment(ref _version);

    public bool IsCurrent(long version) => version == Volatile.Read(ref _version);

    public async Task<OgResult<OgHazopWorkspace>> LoadAsync(
        long version,
        string studyId,
        OgHseScope scope,
        Func<OgHseScope> currentScope,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(scope);
        ArgumentNullException.ThrowIfNull(currentScope);

        if (string.IsNullOrWhiteSpace(studyId))
        {
            return OgResult<OgHazopWorkspace>.Failed(
                "A HAZOP study is opened by its recorded id; no study is assumed.", HazopRules.CodeInputRequired);
        }

        var start = currentScope();

        if (start != scope || !IsCurrent(version))
        {
            return ScopeChanged(studyId, scope, start);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgHazopWorkspace>.Failed(
                "No connection is open, so the study cannot be read.", HazopRules.CodeNoConnection);
        }

        try
        {
            var read = await ReadAsync(source, studyId.Trim(), cancellationToken).ConfigureAwait(false);

            if (read.DuplicateNodeKey is { } duplicateNodes)
            {
                return Failure(duplicateNodes, HazopRules.CodeDuplicateNodeKey);
            }

            if (read.Study is null)
            {
                return OgResult<OgHazopWorkspace>.Failed(
                    $"HAZOP study '{studyId}' is not recorded. No other study is opened in its place.",
                    HazopRules.CodeStudyNotFound);
            }

            var end = currentScope();

            if (end != start || !IsCurrent(version))
            {
                return ScopeChanged(studyId, scope, end);
            }

            return OgResult<OgHazopWorkspace>.Ok(new OgHazopWorkspace(
                read.Study,
                read.Study.Nodes.FirstOrDefault(),
                read.Study.Nodes.FirstOrDefault()?.Deviations.FirstOrDefault(),
                HazopRules.GuideWords,
                HazopRules.RiskRatings,
                HazopRules.StudyStates,
                HazopRules.NextStates(read.Study.State),
                DateTime.UtcNow));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, $"reading HAZOP study {studyId}", "the study is not shown", FailureSeverity.Error);

            return OgResult<OgHazopWorkspace>.Failed(
                "The HAZOP study could not be read" +
                (string.IsNullOrWhiteSpace(reference) ? "." : " (reference " + reference + ")."),
                reference ?? HazopRules.CodeReadFailed);
        }
    }

    public async Task<OgResult<OgHazopWorkspace>> CreateStudyAsync(
        string studyId,
        string studyName,
        string scopeText,
        string fieldId,
        string actor,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(actor))
        {
            return Failure("A study must record who created it.", HazopRules.CodeActorRequired);
        }

        if (string.IsNullOrWhiteSpace(studyId) || string.IsNullOrWhiteSpace(studyName))
        {
            return Failure("A study needs its id and its name.", HazopRules.CodeInputRequired);
        }

        var source = _source();

        if (source is null)
        {
            return Failure("No connection is open, so the study cannot be created.", HazopRules.CodeNoConnection);
        }

        try
        {
            var projects = await OgUow.ReadAllAsync<PROJECT>(source).ConfigureAwait(false);

            if (projects.Any(row => string.Equals(row.PROJECT_ID, studyId.Trim(), StringComparison.OrdinalIgnoreCase)))
            {
                return Failure($"A study with id '{studyId}' already exists.", HazopRules.CodeStudyExists);
            }

            var now = DateTime.UtcNow;

            var study = OgUow.For<PROJECT>(source);
            study.PrimaryKey = "PROJECT_ID";

            study.Add(new PROJECT
            {
                PROJECT_ID = studyId.Trim(),
                PROJECT_TYPE = HazopType,
                PROJECT_NAME = studyName.Trim(),
                DESCRIPTION = scopeText?.Trim(),
                REMARK = scopeText?.Trim(),
                START_DATE = now.Date,
                EFFECTIVE_DATE = now,
                ACTIVE_IND = "Y",
                SOURCE = FieldSource,
                PPDM_GUID = Guid.NewGuid().ToString("N"),
                ROW_CREATED_BY = actor.Trim(),
                ROW_CREATED_DATE = now,
                ROW_CHANGED_BY = actor.Trim(),
                ROW_CHANGED_DATE = now,
            });

            var committed = await study.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(committed))
            {
                return Failure(OgUow.Message(committed, "The study was not saved."), HazopRules.CodeWriteFailed);
            }

            if (!string.IsNullOrWhiteSpace(fieldId))
            {
                var context = OgUow.For<PROJECT_COMPONENT>(source);
                context.PrimaryKey = "PROJECT_ID";

                context.Add(new PROJECT_COMPONENT
                {
                    PROJECT_ID = studyId.Trim(),
                    COMPONENT_OBS_NO = 1m,
                    FIELD_ID = fieldId.Trim(),
                    FIELD_SOURCE = FieldSource,
                    REMARK = scopeText?.Trim(),
                    SOURCE = FieldSource,
                    EFFECTIVE_DATE = now,
                    ACTIVE_IND = "Y",
                    PPDM_GUID = Guid.NewGuid().ToString("N"),
                    ROW_CREATED_BY = actor.Trim(),
                    ROW_CREATED_DATE = now,
                    ROW_CHANGED_BY = actor.Trim(),
                    ROW_CHANGED_DATE = now,
                });

                var contextCommitted = await context.Commit().ConfigureAwait(false);

                if (!OgUow.IsOk(contextCommitted))
                {
                    return Failure(
                        OgUow.Message(contextCommitted, "The study's field context was not saved."),
                        HazopRules.CodeWriteFailed);
                }
            }

            if (await InsertStatusAsync(source, studyId.Trim(), HazopRules.StudyStatusId(studyId.Trim()),
                    StudyStatusType, HseReferenceCodes.Hazop.Draft, null, actor).ConfigureAwait(false) is { } statusFailure)
            {
                return Failure(statusFailure, HazopRules.CodeWriteFailed);
            }

            return await AfterAsync(source, studyId.Trim(), cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, $"creating HAZOP study {studyId}", "the study was not created", FailureSeverity.Error);

            return Failure(
                "The study could not be created" +
                (string.IsNullOrWhiteSpace(reference) ? "." : " (reference " + reference + ")."),
                reference ?? HazopRules.CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgHazopWorkspace>> AddNodeAsync(
        string studyId,
        string nodeName,
        string designIntent,
        string actor,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(actor))
        {
            return Failure("A node must record who added it.", HazopRules.CodeActorRequired);
        }

        if (string.IsNullOrWhiteSpace(nodeName))
        {
            return Failure("A node needs its name.", HazopRules.CodeInputRequired);
        }

        var source = _source();

        if (source is null)
        {
            return Failure("No connection is open, so the node cannot be added.", HazopRules.CodeNoConnection);
        }

        try
        {
            var read = await ReadAsync(source, studyId.Trim(), cancellationToken).ConfigureAwait(false);

            if (read.DuplicateNodeKey is { } duplicateNodes)
            {
                return Failure(duplicateNodes, HazopRules.CodeDuplicateNodeKey);
            }

            if (read.Study is null)
            {
                return Failure($"HAZOP study '{studyId}' is not recorded.", HazopRules.CodeStudyNotFound);
            }

            if (read.DuplicateNodeKey is { } duplicate)
            {
                return Failure(duplicate, HazopRules.CodeDuplicateNodeKey);
            }

            // AC-3: a node's key is its step id, which is derived from the study and the next sequence; the read above has
            // already refused an existing duplicate, so the generated key cannot collide with a recorded one.
            var sequence = read.Study.Nodes.Count == 0 ? 1 : read.Study.Nodes.Max(node => node.Seq) + 1;
            var stepId = studyId.Trim() + "-NODE-" + sequence.ToString("000", CultureInfo.InvariantCulture);
            var now = DateTime.UtcNow;

            var unit = OgUow.For<PROJECT_STEP>(source);
            unit.PrimaryKey = "PROJECT_ID";

            unit.Add(new PROJECT_STEP
            {
                PROJECT_ID = studyId.Trim(),
                STEP_ID = stepId,
                STEP_TYPE = NodeType,
                STEP_NAME = nodeName.Trim(),
                STEP_SEQ_NO = sequence,
                DESCRIPTION = designIntent?.Trim(),
                REMARK = designIntent?.Trim(),
                EFFECTIVE_DATE = now,
                ACTIVE_IND = "Y",
                SOURCE = FieldSource,
                PPDM_GUID = Guid.NewGuid().ToString("N"),
                ROW_CREATED_BY = actor.Trim(),
                ROW_CREATED_DATE = now,
                ROW_CHANGED_BY = actor.Trim(),
                ROW_CHANGED_DATE = now,
            });

            var committed = await unit.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(committed))
            {
                return Failure(OgUow.Message(committed, "The node was not saved."), HazopRules.CodeWriteFailed);
            }

            return await AfterAsync(source, studyId.Trim(), cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, $"adding a node to HAZOP study {studyId}", "the node was not added", FailureSeverity.Error);

            return Failure(
                "The node could not be added" +
                (string.IsNullOrWhiteSpace(reference) ? "." : " (reference " + reference + ")."),
                reference ?? HazopRules.CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgHazopWorkspace>> AddDeviationAsync(
        string studyId,
        string nodeStepId,
        string guideWord,
        string parameter,
        string cause,
        string consequence,
        string safeguard,
        string risk,
        string actor,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(actor))
        {
            return Failure("A deviation must record who added it.", HazopRules.CodeActorRequired);
        }

        if (HazopRules.ValidateDeviation(guideWord, parameter, consequence) is { } refusal)
        {
            return Failure(refusal.Message, refusal.Code);
        }

        if (!string.IsNullOrWhiteSpace(risk) &&
            !HazopRules.RiskRatings.Contains(risk.Trim().ToUpperInvariant(), StringComparer.Ordinal))
        {
            return Failure($"A risk rating is {string.Join(", ", HazopRules.RiskRatings)}.", HazopRules.CodeInputInvalid);
        }

        var source = _source();

        if (source is null)
        {
            return Failure("No connection is open, so the deviation cannot be added.", HazopRules.CodeNoConnection);
        }

        try
        {
            var read = await ReadAsync(source, studyId.Trim(), cancellationToken).ConfigureAwait(false);

            if (read.DuplicateNodeKey is { } duplicateNodes)
            {
                return Failure(duplicateNodes, HazopRules.CodeDuplicateNodeKey);
            }

            if (read.Study is null)
            {
                return Failure($"HAZOP study '{studyId}' is not recorded.", HazopRules.CodeStudyNotFound);
            }

            if (read.Study.Nodes.All(node =>
                    !string.Equals(node.StepId, nodeStepId?.Trim(), StringComparison.OrdinalIgnoreCase)))
            {
                return Failure($"'{nodeStepId}' is not a node of study {studyId}.", HazopRules.CodeUnknownNode);
            }

            var conditions = await OgUow.ReadAllAsync<PROJECT_CONDITION>(source).ConfigureAwait(false);
            var next = conditions
                .Where(row => string.Equals(row.PROJECT_ID, studyId.Trim(), StringComparison.OrdinalIgnoreCase))
                .Select(row => row.CONDITION_OBS_NO)
                .DefaultIfEmpty(0m)
                .Max() + 1m;

            var now = DateTime.UtcNow;

            var unit = OgUow.For<PROJECT_CONDITION>(source);
            unit.PrimaryKey = "PROJECT_ID";

            unit.Add(new PROJECT_CONDITION
            {
                PROJECT_ID = studyId.Trim(),
                CONDITION_OBS_NO = next,
                CONDITION_TYPE = guideWord.Trim().ToUpperInvariant(),
                PROJECT_TYPE = HazopType,
                SOURCE = nodeStepId!.Trim(),
                // The owner's payload stays exactly its two fields so its own reader is unaffected.
                REMARK = JsonSerializer.Serialize(new { Deviation = parameter.Trim(), Consequence = consequence.Trim() }),
                START_DATE = now,
                EFFECTIVE_DATE = now,
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
                return Failure(OgUow.Message(committed, "The deviation was not saved."), HazopRules.CodeWriteFailed);
            }

            // The owner's payload must stay exactly its two fields (its reader requires two properties), so the desktop's
            // extra fields live in a PROJECT_COMPONENT row linked to the condition by CONDITION_ID - the owner's own link.
            var components = await OgUow.ReadAllAsync<PROJECT_COMPONENT>(source).ConfigureAwait(false);
            var nextComponent = components
                .Where(row => string.Equals(row.PROJECT_ID, studyId.Trim(), StringComparison.OrdinalIgnoreCase))
                .Select(row => row.COMPONENT_OBS_NO)
                .DefaultIfEmpty(0m)
                .Max() + 1m;

            var detail = OgUow.For<PROJECT_COMPONENT>(source);
            detail.PrimaryKey = "PROJECT_ID";

            detail.Add(new PROJECT_COMPONENT
            {
                PROJECT_ID = studyId.Trim(),
                COMPONENT_OBS_NO = nextComponent,
                CONDITION_ID = HazopRules.DeviationStatusId(nodeStepId!.Trim(), next),
                COMPONENT_TYPE = "HAZOP_DEVIATION_DETAIL",
                DESCRIPTION = ExtrasText(parameter, cause, safeguard, risk),
                EFFECTIVE_DATE = now,
                ACTIVE_IND = "Y",
                SOURCE = FieldSource,
                PPDM_GUID = Guid.NewGuid().ToString("N"),
                ROW_CREATED_BY = actor.Trim(),
                ROW_CREATED_DATE = now,
                ROW_CHANGED_BY = actor.Trim(),
                ROW_CHANGED_DATE = now,
            });

            var detailCommitted = await detail.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(detailCommitted))
            {
                return Failure(OgUow.Message(detailCommitted, "The deviation's detail was not saved."),
                    HazopRules.CodeWriteFailed);
            }

            if (await InsertStatusAsync(source, studyId.Trim(),
                    HazopRules.DeviationStatusId(nodeStepId!.Trim(), next), DeviationStatusType,
                    HseReferenceCodes.Hazop.Open, nodeStepId!.Trim(), actor).ConfigureAwait(false) is { } initialFailure)
            {
                return Failure(initialFailure, HazopRules.CodeWriteFailed);
            }

            return await AfterAsync(source, studyId.Trim(), cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, $"adding a deviation to HAZOP study {studyId}", "the deviation was not added",
                FailureSeverity.Error);

            return Failure(
                "The deviation could not be added" +
                (string.IsNullOrWhiteSpace(reference) ? "." : " (reference " + reference + ")."),
                reference ?? HazopRules.CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgHazopWorkspace>> AddActionAsync(
        string studyId,
        string nodeStepId,
        int condSeq,
        string action,
        string owner,
        DateTime dueDate,
        string actor,
        CancellationToken cancellationToken = default)
    {
        // AC-4 and SG-11-5: no action without its node and its deviation.
        if (HazopRules.ValidateAction(nodeStepId, condSeq, action, owner, dueDate) is { } refusal)
        {
            return Failure(refusal.Message, refusal.Code);
        }

        if (string.IsNullOrWhiteSpace(actor))
        {
            return Failure("An action must record who raised it.", HazopRules.CodeActorRequired);
        }

        var source = _source();

        if (source is null)
        {
            return Failure("No connection is open, so the action cannot be raised.", HazopRules.CodeNoConnection);
        }

        try
        {
            var read = await ReadAsync(source, studyId.Trim(), cancellationToken).ConfigureAwait(false);

            if (read.DuplicateNodeKey is { } duplicateNodes)
            {
                return Failure(duplicateNodes, HazopRules.CodeDuplicateNodeKey);
            }

            if (read.Study is null)
            {
                return Failure($"HAZOP study '{studyId}' is not recorded.", HazopRules.CodeStudyNotFound);
            }

            var node = read.Study.Nodes.FirstOrDefault(candidate =>
                string.Equals(candidate.StepId, nodeStepId.Trim(), StringComparison.OrdinalIgnoreCase));

            if (node is null)
            {
                return Failure($"'{nodeStepId}' is not a node of study {studyId}.", HazopRules.CodeUnknownNode);
            }

            if (node.Deviations.All(deviation => deviation.CondSeq != condSeq))
            {
                return Failure(
                    $"Deviation {condSeq} is not recorded on node {node.Name}; an action must answer a recorded deviation.",
                    HazopRules.CodeUnknownDeviation);
            }

            var now = DateTime.UtcNow;
            var stepId = studyId.Trim() + "-ACTION-" + Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();

            var unit = OgUow.For<PROJECT_STEP>(source);
            unit.PrimaryKey = "PROJECT_ID";

            unit.Add(new PROJECT_STEP
            {
                PROJECT_ID = studyId.Trim(),
                STEP_ID = stepId,
                STEP_TYPE = ActionType,
                STEP_NAME = action.Trim(),
                STEP_SEQ_NO = node.Seq,
                DUE_DATE = dueDate.Date,
                // The node is linked by the xref row below; the deviation number travels on the step so the pair is
                // complete without a second table.
                REMARK = "node=" + node.StepId + ";dev=" + condSeq.ToString(CultureInfo.InvariantCulture),
                EFFECTIVE_DATE = now,
                ACTIVE_IND = "Y",
                SOURCE = FieldSource,
                PPDM_GUID = Guid.NewGuid().ToString("N"),
                ROW_CREATED_BY = actor.Trim(),
                ROW_CREATED_DATE = now,
                ROW_CHANGED_BY = actor.Trim(),
                ROW_CHANGED_DATE = now,
            });

            var committed = await unit.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(committed))
            {
                return Failure(OgUow.Message(committed, "The action was not saved."), HazopRules.CodeWriteFailed);
            }

            var xref = OgUow.For<PROJECT_STEP_XREF>(source);
            xref.PrimaryKey = "PROJECT_ID";

            xref.Add(new PROJECT_STEP_XREF
            {
                PROJECT_ID = studyId.Trim(),
                STEP_ID1 = node.StepId,
                STEP_ID2 = stepId,
                XREF_OBS_NO = 1m,
                STEP_XREF_TYPE = NodeXrefType,
                EFFECTIVE_DATE = now,
                ACTIVE_IND = "Y",
                SOURCE = FieldSource,
                PPDM_GUID = Guid.NewGuid().ToString("N"),
                ROW_CREATED_BY = actor.Trim(),
                ROW_CREATED_DATE = now,
                ROW_CHANGED_BY = actor.Trim(),
                ROW_CHANGED_DATE = now,
            });

            var xrefCommitted = await xref.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(xrefCommitted))
            {
                return Failure(OgUow.Message(xrefCommitted, "The action was not linked to its node."),
                    HazopRules.CodeWriteFailed);
            }

            var responsible = OgUow.For<PROJECT_STEP_BA>(source);
            responsible.PrimaryKey = "PROJECT_ID";

            responsible.Add(new PROJECT_STEP_BA
            {
                PROJECT_ID = studyId.Trim(),
                BUSINESS_ASSOCIATE_ID = owner.Trim(),
                ROLE = HseReferenceCodes.CorrectiveAction.ResponsibleRole,
                ROLE_SEQ_NO = 1m,
                STEP_ID = stepId,
                STEP_BA_OBS_NO = 1m,
                EFFECTIVE_DATE = now,
                ACTIVE_IND = "Y",
                SOURCE = FieldSource,
                PPDM_GUID = Guid.NewGuid().ToString("N"),
                ROW_CREATED_BY = actor.Trim(),
                ROW_CREATED_DATE = now,
                ROW_CHANGED_BY = actor.Trim(),
                ROW_CHANGED_DATE = now,
            });

            var ownerCommitted = await responsible.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(ownerCommitted))
            {
                return Failure(OgUow.Message(ownerCommitted, "The action's owner was not saved."),
                    HazopRules.CodeWriteFailed);
            }

            return await AfterAsync(source, studyId.Trim(), cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, $"raising a HAZOP action on {studyId}", "the action was not raised", FailureSeverity.Error);

            return Failure(
                "The action could not be raised" +
                (string.IsNullOrWhiteSpace(reference) ? "." : " (reference " + reference + ")."),
                reference ?? HazopRules.CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgHazopWorkspace>> TransitionStudyAsync(
        string studyId,
        string target,
        string acknowledgedState,
        string actor,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(actor))
        {
            return Failure("A transition must record who made it.", HazopRules.CodeActorRequired);
        }

        var source = _source();

        if (source is null)
        {
            return Failure("No connection is open, so the study cannot change state.", HazopRules.CodeNoConnection);
        }

        try
        {
            var read = await ReadAsync(source, studyId.Trim(), cancellationToken).ConfigureAwait(false);

            if (read.DuplicateNodeKey is { } duplicateNodes)
            {
                return Failure(duplicateNodes, HazopRules.CodeDuplicateNodeKey);
            }

            if (read.Study is null)
            {
                return Failure($"HAZOP study '{studyId}' is not recorded.", HazopRules.CodeStudyNotFound);
            }

            // AC-2 before AC-6: an unconfirmed acknowledgement is reported as such, not as an illegal transition.
            if (HazopRules.CheckAcknowledged(acknowledgedState, read.Study.State, "study") is { } unconfirmed)
            {
                return Failure(unconfirmed.Message, unconfirmed.Code);
            }

            if (HazopRules.CheckTransition(read.Study.State, target) is { } illegal)
            {
                return Failure(illegal.Message, illegal.Code);
            }

            if (await UpsertStatusAsync(source, studyId.Trim(), HazopRules.StudyStatusId(studyId.Trim()),
                    StudyStatusType, target.Trim().ToUpperInvariant(), null, actor, cancellationToken)
                .ConfigureAwait(false) is { } studyFailure)
            {
                return Failure(studyFailure, HazopRules.CodeWriteFailed);
            }

            return await AfterAsync(source, studyId.Trim(), cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, $"changing HAZOP study {studyId} to {target}", "the study did not change state",
                FailureSeverity.Error);

            return Failure(
                "The study could not change state" +
                (string.IsNullOrWhiteSpace(reference) ? "." : " (reference " + reference + ")."),
                reference ?? HazopRules.CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgHazopWorkspace>> TransitionDeviationAsync(
        string studyId,
        string nodeStepId,
        int condSeq,
        string target,
        string acknowledgedState,
        string actor,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(actor))
        {
            return Failure("A transition must record who made it.", HazopRules.CodeActorRequired);
        }

        var source = _source();

        if (source is null)
        {
            return Failure("No connection is open, so the deviation cannot change state.",
                HazopRules.CodeNoConnection);
        }

        try
        {
            var read = await ReadAsync(source, studyId.Trim(), cancellationToken).ConfigureAwait(false);

            if (read.DuplicateNodeKey is { } duplicateNodes)
            {
                return Failure(duplicateNodes, HazopRules.CodeDuplicateNodeKey);
            }

            if (read.Study is null)
            {
                return Failure($"HAZOP study '{studyId}' is not recorded.", HazopRules.CodeStudyNotFound);
            }

            var deviation = read.Study.Nodes
                .FirstOrDefault(node => string.Equals(node.StepId, nodeStepId?.Trim(), StringComparison.OrdinalIgnoreCase))
                ?.Deviations.FirstOrDefault(candidate => candidate.CondSeq == condSeq);

            if (deviation is null)
            {
                return Failure(
                    $"Deviation {condSeq} is not recorded on node '{nodeStepId}'.",
                    HazopRules.CodeUnknownDeviation);
            }

            if (HazopRules.CheckAcknowledged(acknowledgedState, deviation.State, "deviation") is { } unconfirmed)
            {
                return Failure(unconfirmed.Message, unconfirmed.Code);
            }

            var legal = HazopRules.NextDeviationStates(deviation.State);

            if (!legal.Contains(target?.Trim().ToUpperInvariant() ?? string.Empty, StringComparer.Ordinal))
            {
                return Failure(
                    "A deviation in " + deviation.State + " may move to " +
                    (legal.Count == 0 ? "nothing - it is closed" : string.Join(", ", legal)) + ", not " +
                    (target ?? "(none)") + ".",
                    HazopRules.CodeTransitionIllegal);
            }

            if (await UpsertStatusAsync(source, studyId.Trim(),
                    HazopRules.DeviationStatusId(nodeStepId!.Trim(), condSeq), DeviationStatusType,
                    target.Trim().ToUpperInvariant(), nodeStepId.Trim(), actor, cancellationToken)
                .ConfigureAwait(false) is { } deviationFailure)
            {
                return Failure(deviationFailure, HazopRules.CodeWriteFailed);
            }

            return await AfterAsync(source, studyId.Trim(), cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, $"changing deviation {condSeq} on {studyId} to {target}", "the deviation did not change state",
                FailureSeverity.Error);

            return Failure(
                "The deviation could not change state" +
                (string.IsNullOrWhiteSpace(reference) ? "." : " (reference " + reference + ")."),
                reference ?? HazopRules.CodeWriteFailed);
        }
    }

    // ------------------------------------------------------------------ the read model

    private sealed record ReadResult(
        OgHazopStudy? Study,
        string? DuplicateNodeKey);

    private async Task<OgResult<OgHazopWorkspace>> AfterAsync(
        IDataSource source,
        string studyId,
        CancellationToken cancellationToken)
    {
        var read = await ReadAsync(source, studyId, cancellationToken).ConfigureAwait(false);

        return read.Study is null
            ? Failure($"HAZOP study '{studyId}' is not recorded.", HazopRules.CodeStudyNotFound)
            : OgResult<OgHazopWorkspace>.Ok(new OgHazopWorkspace(
                read.Study,
                read.Study.Nodes.FirstOrDefault(),
                read.Study.Nodes.FirstOrDefault()?.Deviations.FirstOrDefault(),
                HazopRules.GuideWords,
                HazopRules.RiskRatings,
                HazopRules.StudyStates,
                HazopRules.NextStates(read.Study.State),
                DateTime.UtcNow));
    }

    private async Task<ReadResult> ReadAsync(
        IDataSource source,
        string studyId,
        CancellationToken cancellationToken)
    {
        var projects = await OgUow.ReadAllAsync<PROJECT>(source).ConfigureAwait(false);
        var components = await OgUow.ReadAllAsync<PROJECT_COMPONENT>(source).ConfigureAwait(false);
        var steps = await OgUow.ReadAllAsync<PROJECT_STEP>(source).ConfigureAwait(false);
        var conditions = await OgUow.ReadAllAsync<PROJECT_CONDITION>(source).ConfigureAwait(false);
        var statuses = await OgUow.ReadAllAsync<PROJECT_STATUS>(source).ConfigureAwait(false);
        var parties = await OgUow.ReadAllAsync<PROJECT_STEP_BA>(source).ConfigureAwait(false);

        cancellationToken.ThrowIfCancellationRequested();

        // AC-1: the study is opened by its explicit recorded id, never as "the latest".
        var project = projects.FirstOrDefault(row =>
            string.Equals(row.PROJECT_ID, studyId, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(row.PROJECT_TYPE?.Trim(), HazopType, StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(row.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase));

        if (project is null)
        {
            return new ReadResult(null, null);
        }

        var context = components
            .Where(row => string.Equals(row.PROJECT_ID, studyId, StringComparison.OrdinalIgnoreCase))
            .Where(row => !string.Equals(row.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase))
            .FirstOrDefault();

        var studyStatus = statuses
            .Where(row => string.Equals(row.PROJECT_ID, studyId, StringComparison.OrdinalIgnoreCase))
            .Where(row => string.Equals(row.STATUS_TYPE?.Trim(), StudyStatusType, StringComparison.OrdinalIgnoreCase))
            .Where(row => !string.Equals(row.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(row => row.EFFECTIVE_DATE)
            .FirstOrDefault()?.STATUS ?? string.Empty;

        var nodes = steps
            .Where(row => string.Equals(row.PROJECT_ID, studyId, StringComparison.OrdinalIgnoreCase))
            .Where(row => string.Equals(row.STEP_TYPE?.Trim(), NodeType, StringComparison.OrdinalIgnoreCase))
            .Where(row => !string.Equals(row.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase))
            .OrderBy(row => row.STEP_SEQ_NO)
            .ToList();

        // AC-3: a duplicate node key is refused rather than shown as two nodes.
        var duplicateId = nodes
            .GroupBy(row => row.STEP_ID, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault(group => group.Count() > 1)?.Key;

        var duplicateSeq = nodes
            .GroupBy(row => row.STEP_SEQ_NO)
            .FirstOrDefault(group => group.Count() > 1);

        if (duplicateId is not null || duplicateSeq is not null)
        {
            return new ReadResult(null,
                "The study has a duplicate node key (" +
                (duplicateId ?? "sequence " + duplicateSeq!.Key.ToString(CultureInfo.InvariantCulture)) +
                "), so it cannot be shown. Correct the node identities before continuing.");
        }

        var deviationDetails = components
            .Where(row => string.Equals(row.PROJECT_ID, studyId, StringComparison.OrdinalIgnoreCase))
            .Where(row => !string.IsNullOrWhiteSpace(row.CONDITION_ID))
            .Where(row => !string.Equals(row.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase))
            .GroupBy(row => row.CONDITION_ID!, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.First().DESCRIPTION, StringComparer.OrdinalIgnoreCase);

        var deviationStatuses = statuses
            .Where(row => string.Equals(row.PROJECT_ID, studyId, StringComparison.OrdinalIgnoreCase))
            .Where(row => string.Equals(row.STATUS_TYPE?.Trim(), DeviationStatusType,
                StringComparison.OrdinalIgnoreCase))
            .Where(row => !string.Equals(row.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase))
            .ToDictionary(
                row => row.STATUS_ID ?? string.Empty,
                row => row.STATUS ?? string.Empty,
                StringComparer.OrdinalIgnoreCase);

        var actions = steps
            .Where(row => string.Equals(row.PROJECT_ID, studyId, StringComparison.OrdinalIgnoreCase))
            .Where(row => string.Equals(row.STEP_TYPE?.Trim(), ActionType, StringComparison.OrdinalIgnoreCase))
            .Where(row => !string.Equals(row.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase))
            .ToList();

        var mappedNodes = new List<OgHazopNode>();

        foreach (var node in nodes)
        {
            var nodeConditions = conditions
                .Where(row => string.Equals(row.PROJECT_ID, studyId, StringComparison.OrdinalIgnoreCase))
                .Where(row => string.Equals(row.PROJECT_TYPE?.Trim(), HazopType, StringComparison.OrdinalIgnoreCase))
                .Where(row => string.Equals(row.SOURCE?.Trim(), node.STEP_ID, StringComparison.OrdinalIgnoreCase))
                .Where(row => !string.Equals(row.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase))
                .OrderBy(row => row.CONDITION_OBS_NO)
                .ToList();

            var deviations = nodeConditions
                .Select(condition =>
                {
                    var (deviation, consequence) = ReadPayload(condition, studyId);
                    var extras = ReadExtras(deviationDetails.TryGetValue(
                        HazopRules.DeviationStatusId(node.STEP_ID, condition.CONDITION_OBS_NO), out var detailText)
                        ? detailText
                        : null);
                    var condSeq = (int)condition.CONDITION_OBS_NO;

                    var action = actions
                        .Where(row => string.Equals(row.REMARK?.Trim(),
                            "node=" + node.STEP_ID + ";dev=" + condSeq.ToString(CultureInfo.InvariantCulture),
                            StringComparison.OrdinalIgnoreCase))
                        .Select(row => new OgHazopAction(
                            row.STEP_ID,
                            row.STEP_NAME ?? string.Empty,
                            parties
                                .Where(party => string.Equals(party.PROJECT_ID, studyId, StringComparison.OrdinalIgnoreCase))
                                .Where(party => string.Equals(party.STEP_ID, row.STEP_ID,
                                    StringComparison.OrdinalIgnoreCase))
                                .Where(party => string.Equals(party.ROLE?.Trim(),
                                    HseReferenceCodes.CorrectiveAction.ResponsibleRole,
                                    StringComparison.OrdinalIgnoreCase))
                                .Where(party => !string.Equals(party.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase))
                                .Select(party => party.BUSINESS_ASSOCIATE_ID)
                                .FirstOrDefault(),
                            row.DUE_DATE,
                            row.ACTUAL_END_DATE is null
                                ? HseReferenceCodes.CorrectiveAction.Open
                                : HseReferenceCodes.CorrectiveAction.Completed,
                            row.ACTUAL_END_DATE is null && row.DUE_DATE is { } due &&
                            due.Date < DateTime.UtcNow.Date))
                        .FirstOrDefault();

                    return new OgHazopDeviation(
                        condSeq,
                        condition.CONDITION_TYPE ?? string.Empty,
                        extras.GetValueOrDefault("parameter") ?? deviation,
                        extras.GetValueOrDefault("cause") ?? string.Empty,
                        consequence,
                        extras.GetValueOrDefault("safeguard") ?? string.Empty,
                        extras.GetValueOrDefault("risk") ?? string.Empty,
                        deviationStatuses.TryGetValue(HazopRules.DeviationStatusId(node.STEP_ID,
                            condition.CONDITION_OBS_NO), out var state)
                            ? state
                            : string.Empty,
                        action);
                })
                .ToList();

            mappedNodes.Add(new OgHazopNode(
                node.STEP_ID,
                (int)node.STEP_SEQ_NO,
                node.STEP_NAME ?? string.Empty,
                node.DESCRIPTION ?? string.Empty,
                deviations));
        }

        return new ReadResult(new OgHazopStudy(
            project.PROJECT_ID,
            project.PROJECT_NAME ?? string.Empty,
            project.DESCRIPTION ?? project.REMARK ?? string.Empty,
            context?.FIELD_ID,
            context?.FACILITY_ID,
            studyStatus,
            project.START_DATE,
            mappedNodes), null);
    }

    private (string Deviation, string Consequence) ReadPayload(PROJECT_CONDITION condition, string studyId)
    {
        if (string.IsNullOrWhiteSpace(condition.REMARK))
        {
            return (string.Empty, string.Empty);
        }

        try
        {
            using var document = JsonDocument.Parse(condition.REMARK);

            var deviation = document.RootElement.TryGetProperty("Deviation", out var value) ? value.GetString() : null;
            var consequence = document.RootElement.TryGetProperty("Consequence", out var second)
                ? second.GetString()
                : null;

            return (deviation ?? string.Empty, consequence ?? string.Empty);
        }
        catch (JsonException ex)
        {
            // A malformed payload is data this screen cannot read; it is reported with a reference and the deviation is
            // shown with what the columns around it say, rather than failing the whole study.
            _failures.ReportHandled(
                ex, $"reading a HAZOP deviation payload on {studyId}", "the deviation's payload is not shown",
                FailureSeverity.Degraded);

            return (string.Empty, string.Empty);
        }
    }

    private static string ExtrasText(string parameter, string cause, string safeguard, string risk) =>
        "parameter=" + parameter.Trim() + "\n" +
        "cause=" + (cause ?? string.Empty).Trim() + "\n" +
        "safeguard=" + (safeguard ?? string.Empty).Trim() + "\n" +
        "risk=" + (risk ?? string.Empty).Trim();

    private static Dictionary<string, string> ReadExtras(string? description)
    {
        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        if (string.IsNullOrWhiteSpace(description))
        {
            return values;
        }

        foreach (var line in description.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var separator = line.IndexOf('=');

            if (separator <= 0)
            {
                continue;
            }

            values[line[..separator].Trim()] = line[(separator + 1)..].Trim();
        }

        return values;
    }

    private static async Task<string?> InsertStatusAsync(
        IDataSource source,
        string studyId,
        string statusId,
        string statusType,
        string status,
        string? stepId,
        string actor)
    {
        var now = DateTime.UtcNow;
        var unit = OgUow.For<PROJECT_STATUS>(source);
        unit.PrimaryKey = "PROJECT_ID";

        unit.Add(new PROJECT_STATUS
        {
            PROJECT_ID = studyId,
            STATUS_ID = statusId,
            STEP_ID = stepId,
            STATUS_TYPE = statusType,
            STATUS = status,
            EFFECTIVE_DATE = now,
            ACTIVE_IND = "Y",
            SOURCE = FieldSource,
            PPDM_GUID = Guid.NewGuid().ToString("N"),
            ROW_CREATED_BY = actor.Trim(),
            ROW_CREATED_DATE = now,
            ROW_CHANGED_BY = actor.Trim(),
            ROW_CHANGED_DATE = now,
        });

        var committed = await unit.Commit().ConfigureAwait(false);

        return OgUow.IsOk(committed) ? null : OgUow.Message(committed, "The status was not saved.");
    }

    /// <summary>
    /// The owner keeps one status row per study/deviation and updates it, so a transition updates the row through the unit
    /// that read it rather than appending a second one.
    /// </summary>
    private static async Task<string?> UpsertStatusAsync(
        IDataSource source,
        string studyId,
        string statusId,
        string statusType,
        string status,
        string? stepId,
        string actor,
        CancellationToken cancellationToken)
    {
        var unit = OgUow.For<PROJECT_STATUS>(source);
        unit.PrimaryKey = "PROJECT_ID";

        var rows = await OgUow.ReadAsync(
            unit,
            [OgUow.Equal("PROJECT_ID", studyId), OgUow.Equal("STATUS_ID", statusId)],
            cancellationToken).ConfigureAwait(false);

        var row = rows.FirstOrDefault();

        if (row is null)
        {
            return await InsertStatusAsync(source, studyId, statusId, statusType, status, stepId, actor)
                .ConfigureAwait(false);
        }

        row.STATUS = status;
        row.STEP_ID = stepId;
        row.EFFECTIVE_DATE = DateTime.UtcNow;
        row.ROW_CHANGED_BY = actor.Trim();
        row.ROW_CHANGED_DATE = DateTime.UtcNow;

        var updated = unit.Update(row);

        if (!OgUow.IsOk(updated))
        {
            return OgUow.Message(updated, "The status was not updated.");
        }

        var committed = await unit.Commit().ConfigureAwait(false);

        return OgUow.IsOk(committed) ? null : OgUow.Message(committed, "The status was not updated.");
    }

    private static OgResult<OgHazopWorkspace> ScopeChanged(string studyId, OgHseScope scope, OgHseScope current) =>
        OgResult<OgHazopWorkspace>.Failed(
            "The company or field changed while the study was loading, so study " + studyId + " was cleared and its rows " +
            $"were discarded. The load began for company {scope.CompanyId}, field {scope.FieldId ?? "(none)"} and the " +
            $"session is now company {current.CompanyId}, field {current.FieldId ?? "(none)"}. Load again.",
            HazopRules.CodeScopeChanged);

    private static OgResult<OgHazopWorkspace> Failure(string message, string code) =>
        OgResult<OgHazopWorkspace>.Failed(message, code);

}
