using System.Globalization;
using Beep.OilandGas.Models.Constants;
using Beep.OilandGas.Models.Data.HSE;
using Beep.OilandGas.PPDM39.Models;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Data.Hse;

/// <summary>One recorded cause (HSE-03).</summary>
/// <param name="Type">IMMEDIATE, CONTRIBUTING or ROOT - the owner's <see cref="Models.Data.HSE.CauseType"/> values.</param>
/// <param name="Category">The owner's cause code, e.g. HUMAN or EQUIPMENT.</param>
public sealed record OgCause(int Seq, string Type, string Category, string Description, DateTime? RecordedAt)
{
    public bool IsRoot => string.Equals(Type, CauseType.Root, StringComparison.OrdinalIgnoreCase);

    public string Describe() =>
        Seq.ToString(CultureInfo.InvariantCulture) + " · " + Type.ToLowerInvariant() +
        (string.IsNullOrWhiteSpace(Category) ? string.Empty : " · " + Category) + " · " + Description;
}

/// <summary>One corrective action (HSE-03), matching the feature's <c>OgCorrectiveAction</c>.</summary>
public sealed record OgCorrectiveAction(
    string PlanId,
    string StepId,
    string Action,
    string? Owner,
    DateTime? DueDate,
    string State,
    string? VerifiedBy,
    DateTime? VerifiedAt,
    bool Overdue)
{
    public bool IsCompleted => string.Equals(State, HseReferenceCodes.CorrectiveAction.Completed,
        StringComparison.OrdinalIgnoreCase);

    public bool IsVerified => !string.IsNullOrWhiteSpace(VerifiedBy);

    /// <summary>AC-4: a step counts only when it is both completed and verified by someone.</summary>
    public bool IsClosedOut => IsCompleted && IsVerified;

    public string DueText => DueDate is { } due
        ? due.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) + (Overdue ? " (overdue)" : string.Empty)
        : "no due date";

    public string Describe() =>
        Action + " · owner " + (Owner ?? "(none)") + " · due " + DueText + " · " + State.ToLowerInvariant() +
        (IsVerified ? " · verified by " + VerifiedBy : " · not verified");
}

/// <summary>The investigation for one incident (HSE-03).</summary>
public sealed record OgInvestigationWorkspace(
    string IncidentId,
    string IncidentState,
    string? Investigator,
    string? InvestigatorRole,
    IReadOnlyList<OgCause> Causes,
    IReadOnlyList<OgCorrectiveAction> Actions,
    string? PlanId,
    string? PlanName,
    string? PlanNumber,
    DateTime ReadAt)
{
    /// <summary>The owner's own RCA rule: a root cause whose description is at least 20 characters.</summary>
    public bool RootCauseComplete => InvestigationRules.HasRootCause(Causes);

    /// <summary>AC-4: every action must be completed and verified before closure.</summary>
    public IReadOnlyList<OgCorrectiveAction> Unverified =>
        Actions.Where(action => !action.IsClosedOut).ToList();

    public bool AllActionsVerified => Unverified.Count == 0;

    public string CausesText => Causes.Count == 0
        ? "No cause is recorded."
        : string.Join(Environment.NewLine, Causes.Select(cause => cause.Describe()));

    public string ActionsText => Actions.Count == 0
        ? "No corrective action is recorded."
        : string.Join(Environment.NewLine, Actions.Select(action => action.Describe()));

    public string InvestigatorText => string.IsNullOrWhiteSpace(Investigator)
        ? "no investigator is assigned"
        : Investigator + (string.IsNullOrWhiteSpace(InvestigatorRole) ? string.Empty : " (" + InvestigatorRole + ")");

    /// <summary>The closure checklist, so the page shows the same reasons the command will give (AC-4, AC-5).</summary>
    public string ChecklistText
    {
        get
        {
            var lines = new List<string>
            {
                (RootCauseComplete ? "✓" : "✗") + " a root cause of at least 20 characters is recorded",
                (AllActionsVerified
                    ? "✓ every action is completed and verified"
                    : "✗ " + Unverified.Count.ToString(CultureInfo.InvariantCulture) +
                      " action(s) are not completed and verified: " +
                      string.Join("; ", Unverified.Select(action => action.Action))),
            };

            return string.Join(Environment.NewLine, lines);
        }
    }

    public string ReadText =>
        "read at " + ReadAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
}

/// <summary>The investigation rules HSE-03 states once.</summary>
public static class InvestigationRules
{
    public const string CodeNoConnection = "investigation-no-connection";
    public const string CodeReadFailed = "investigation-read-failed";
    public const string CodeWriteFailed = "investigation-write-failed";
    public const string CodeActorRequired = "investigation-actor-required";
    public const string CodeInputRequired = "investigation-input-required";
    public const string CodeInputInvalid = "investigation-input-invalid";
    public const string CodeUnknownIncident = "investigation-unknown-incident";
    public const string CodeUnknownAction = "investigation-unknown-action";
    public const string CodeAssignmentRecorded = "investigation-assignment-recorded";
    public const string CodeFieldChanged = "investigation-field-changed";
    public const string CodeActionNotCompleted = "investigation-action-not-completed";
    public const string CodeAlreadyVerified = "investigation-already-completed";
    public const string CodeClosureBlocked = "investigation-closure-blocked";
    public const string CodeCloserIsInvestigator = "investigation-closer-is-investigator";
    public const string CodeAlreadyClosed = "investigation-already-closed";
    public const string CodeReasonRequired = "investigation-reason-required";

    /// <summary>
    /// The role recorded on the <c>PROJECT_STEP_BA</c> row that verifies an action's effectiveness. The owner has an
    /// OPEN/COMPLETED status but no verification role, so the role is stated here rather than left to the screen.
    /// </summary>
    public const string VerifierRole = "VERIFIER";

    /// <summary>The plan's action status vocabulary, from the owner's corrective-action codes.</summary>
    public static IReadOnlyList<string> ActionStates { get; } =
        [HseReferenceCodes.CorrectiveAction.Open, HseReferenceCodes.CorrectiveAction.Completed];

    /// <summary>The owner's three cause types.</summary>
    public static IReadOnlyList<string> CauseTypes { get; } =
        [CauseType.Immediate, CauseType.Contributing, CauseType.Root];

    /// <summary>The owner's cause categories.</summary>
    public static IReadOnlyList<string> CauseCategories { get; } =
        [CauseCategory.Human, CauseCategory.Equipment, CauseCategory.Process, CauseCategory.Environment];

    /// <summary>The owner's `IsRCACompleteAsync` rule: a ROOT cause with a description of at least 20 characters.</summary>
    public const int RootCauseMinimumLength = 20;

    public static bool HasRootCause(IEnumerable<OgCause> causes)
    {
        ArgumentNullException.ThrowIfNull(causes);

        return causes.Any(cause => cause.IsRoot && cause.Description.Trim().Length >= RootCauseMinimumLength);
    }

    /// <summary>AC-6: an action needs its owner and its due date.</summary>
    public static (string Code, string Message)? ValidateAction(string? action, string? owner, DateTime? dueDate)
    {
        if (string.IsNullOrWhiteSpace(action))
        {
            return (CodeInputRequired, "A corrective action needs its description.");
        }

        if (string.IsNullOrWhiteSpace(owner))
        {
            return (CodeInputRequired, "A corrective action needs the person responsible for it.");
        }

        return dueDate is null
            ? (CodeInputRequired, "A corrective action needs its due date.")
            : null;
    }

    /// <summary>Validates an assignment request (AC-1, AC-2).</summary>
    public static (string Code, string Message)? ValidateAssignment(
        string? participantId,
        string? role,
        string? rationale)
    {
        if (string.IsNullOrWhiteSpace(participantId))
        {
            return (CodeInputRequired, "An assignment needs the participant to assign.");
        }

        if (string.IsNullOrWhiteSpace(role))
        {
            return (CodeInputRequired, "An assignment needs the participant's role.");
        }

        return string.IsNullOrWhiteSpace(rationale)
            ? (CodeInputRequired, "An assignment needs its rationale, so the choice can be reviewed later.")
            : null;
    }

    /// <summary>Validates a cause (HSE-03's RCA panel).</summary>
    public static (string Code, string Message)? ValidateCause(string? type, string? description)
    {
        if (string.IsNullOrWhiteSpace(type) ||
            !CauseTypes.Contains(type.Trim().ToUpperInvariant(), StringComparer.Ordinal))
        {
            return (CodeInputInvalid, $"A cause type is {string.Join(", ", CauseTypes)}.");
        }

        return string.IsNullOrWhiteSpace(description)
            ? (CodeInputRequired, "A cause needs its description.")
            : null;
    }

    /// <summary>
    /// AC-1 and AC-2: whether a new assignment may replace the recorded one. An unacknowledged original is retained, so a
    /// second, different assignment is refused rather than overwriting it.
    /// </summary>
    public static (string Code, string Message)? CheckAssignment(
        string? recordedInvestigator,
        string? requested,
        string incidentId)
    {
        if (string.IsNullOrWhiteSpace(requested))
        {
            return (CodeInputRequired, "An assignment needs the participant to assign.");
        }

        if (string.IsNullOrWhiteSpace(recordedInvestigator))
        {
            return null;
        }

        return string.Equals(recordedInvestigator.Trim(), requested.Trim(), StringComparison.OrdinalIgnoreCase)
            ? null
            : (CodeAssignmentRecorded,
                $"{incidentId} already records {recordedInvestigator} as its investigator. The original is retained, so a " +
                "second assignment is blocked; a reassignment needs the workflow's review step.");
    }

    /// <summary>
    /// The closure checklist: the root cause, every action verified, the closer is not the investigator, and the incident
    /// is still open (AC-4, AC-5).
    /// </summary>
    public static IReadOnlyList<(string Code, string Message)> ClosureBlocks(
        OgInvestigationWorkspace workspace,
        string actor)
    {
        ArgumentNullException.ThrowIfNull(workspace);

        var blocks = new List<(string Code, string Message)>();

        if (string.Equals(workspace.IncidentState, IncidentState.Closed, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(workspace.IncidentState, IncidentState.Cancelled, StringComparison.OrdinalIgnoreCase))
        {
            blocks.Add((CodeAlreadyClosed,
                workspace.IncidentId + " is already " + workspace.IncidentState.ToLowerInvariant() + "."));
        }

        if (!workspace.AllActionsVerified)
        {
            blocks.Add((CodeClosureBlocked,
                workspace.Unverified.Count.ToString(CultureInfo.InvariantCulture) +
                " corrective action(s) are not completed and verified: " +
                string.Join("; ", workspace.Unverified.Select(action => action.Action)) + "."));
        }

        if (!workspace.RootCauseComplete)
        {
            blocks.Add((CodeClosureBlocked,
                "No root cause of at least " + RootCauseMinimumLength.ToString(CultureInfo.InvariantCulture) +
                " characters is recorded, so the investigation is not complete."));
        }

        if (!string.IsNullOrWhiteSpace(workspace.Investigator) &&
            string.Equals(workspace.Investigator.Trim(), actor.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            blocks.Add((CodeCloserIsInvestigator,
                workspace.Investigator + " investigated " + workspace.IncidentId + " and therefore cannot close it."));
        }

        return blocks;
    }
}

/// <summary>HSE-03: investigator assignment, RCA, corrective actions and closure.</summary>
public interface IOgInvestigation
{
    /// <summary>The investigation for one incident, with its checklist and actions.</summary>
    Task<OgResult<OgInvestigationWorkspace>> LoadAsync(
        string incidentId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Assigns the investigator. An existing assignment is retained: the same participant is adopted (a retry), a different
    /// one is refused (AC-1), and a different field context is refused (AC-2).
    /// </summary>
    Task<OgResult<OgInvestigationWorkspace>> AssignAsync(
        string incidentId,
        string participantId,
        string role,
        string rationale,
        OgHseScope scope,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>Records a cause on the chain.</summary>
    Task<OgResult<OgInvestigationWorkspace>> AddCauseAsync(
        string incidentId,
        string causeType,
        string? category,
        string description,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>Adds an action to the incident's plan, creating the plan on the first action (AC-6).</summary>
    Task<OgResult<OgInvestigationWorkspace>> AddActionAsync(
        string incidentId,
        string planName,
        string action,
        string owner,
        DateTime dueDate,
        OgHseScope scope,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>Records an action's completion (the owner's COMPLETED status).</summary>
    Task<OgResult<OgInvestigationWorkspace>> CompleteActionAsync(
        string incidentId,
        string stepId,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>Verifies an action's effectiveness, by someone other than its owner.</summary>
    Task<OgResult<OgInvestigationWorkspace>> VerifyActionAsync(
        string incidentId,
        string stepId,
        string verifier,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>Closes the incident when the checklist passes (AC-4, AC-5).</summary>
    Task<OgResult<OgInvestigationWorkspace>> CloseAsync(
        string incidentId,
        string decision,
        string reason,
        string actor,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// HSE-03 over the owner's <c>HSE_INCIDENT_CAUSE</c>, <c>PROJECT</c>/<c>PROJECT_STEP</c>/<c>PROJECT_STEP_BA</c>/
/// <c>PROJECT_STATUS</c> and <c>HSE_INCIDENT_RESPONSE</c>.
/// </summary>
/// <remarks>
/// The plan is linked to the incident through the component's <c>PROJECT_ID</c> - the link HSE-02 already reads - so an
/// action's progress appears on the incident without a second link. Actions are steps of that plan; the responsible person
/// is a <c>PROJECT_STEP_BA</c> row with the owner's RESPONSIBLE role, and verification of effectiveness is a second
/// <c>PROJECT_STEP_BA</c> row with the VERIFIER role, because the owner's status vocabulary has no verified state.
/// </remarks>
public sealed class OgInvestigation : IOgInvestigation
{
    private readonly Func<IDataSource?> _source;
    private readonly IFailureReporter _failures;

    public OgInvestigation(Func<IDataSource?> source, IFailureReporter failures)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _failures = failures ?? throw new ArgumentNullException(nameof(failures));
    }

    public async Task<OgResult<OgInvestigationWorkspace>> LoadAsync(
        string incidentId,
        CancellationToken cancellationToken = default)
    {
        var source = _source();

        if (source is null)
        {
            return OgResult<OgInvestigationWorkspace>.Failed(
                "No connection is open, so the investigation cannot be read.", InvestigationRules.CodeNoConnection);
        }

        try
        {
            var workspace = await ReadAsync(source, incidentId, cancellationToken).ConfigureAwait(false);

            return workspace is null
                ? OgResult<OgInvestigationWorkspace>.Failed(
                    $"Incident '{incidentId}' is not in this database.", InvestigationRules.CodeUnknownIncident)
                : OgResult<OgInvestigationWorkspace>.Ok(workspace);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, $"reading the investigation for {incidentId}", "the investigation is not shown",
                FailureSeverity.Error);

            return OgResult<OgInvestigationWorkspace>.Failed(
                "The investigation could not be read" +
                (string.IsNullOrWhiteSpace(reference) ? "." : " (reference " + reference + ")."),
                reference ?? InvestigationRules.CodeReadFailed);
        }
    }

    public async Task<OgResult<OgInvestigationWorkspace>> AssignAsync(
        string incidentId,
        string participantId,
        string role,
        string rationale,
        OgHseScope scope,
        string actor,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(scope);

        if (string.IsNullOrWhiteSpace(actor))
        {
            return Failure("An assignment must record who made it.", InvestigationRules.CodeActorRequired);
        }

        if (InvestigationRules.ValidateAssignment(participantId, role, rationale) is { } refusal)
        {
            return Failure(refusal.Message, refusal.Code);
        }

        var source = _source();

        if (source is null)
        {
            return Failure("No connection is open, so the investigator cannot be assigned.",
                InvestigationRules.CodeNoConnection);
        }

        try
        {
            var workspace = await ReadAsync(source, incidentId, cancellationToken).ConfigureAwait(false);

            if (workspace is null)
            {
                return Failure($"Incident '{incidentId}' is not in this database.",
                    InvestigationRules.CodeUnknownIncident);
            }

            // AC-2: the assignment is reviewed against a field; if the incident's recorded field is another one, the
            // original context no longer matches and nothing is written.
            var components = await OgUow.ReadAllAsync<HSE_INCIDENT_COMPONENT>(source).ConfigureAwait(false);
            var mine = components
                .Where(row => string.Equals(row.INCIDENT_ID, incidentId, StringComparison.OrdinalIgnoreCase))
                .Where(row => !string.Equals(row.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase))
                .ToList();

            var primary = mine.FirstOrDefault(row => string.Equals(row.COMPONENT_ROLE?.Trim(),
                HseReferenceCodes.FieldContext.PrimaryRole, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(scope.FieldId) &&
                !string.Equals(primary?.FIELD_ID?.Trim(), scope.FieldId.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                return Failure(
                    $"{incidentId} is recorded in field '{primary?.FIELD_ID ?? "(none)"}', not '{scope.FieldId}'. A " +
                    "pending assignment is not cleared or moved by a different field context.",
                    InvestigationRules.CodeFieldChanged);
            }

            // AC-1 and AC-3: an assignment already recorded is retained. The roster is not consulted, so a roster change
            // cannot clear or replace it.
            if (InvestigationRules.CheckAssignment(workspace.Investigator, participantId, incidentId) is { } blocked)
            {
                return Failure(blocked.Message, blocked.Code);
            }

            if (!string.IsNullOrWhiteSpace(workspace.Investigator))
            {
                // The same participant: a retry, and the original is adopted unchanged.
                return OgResult<OgInvestigationWorkspace>.Ok(workspace);
            }

            var now = DateTime.UtcNow;
            var unit = OgUow.For<HSE_INCIDENT_COMPONENT>(source);
            unit.PrimaryKey = "INCIDENT_ID";

            unit.Add(new HSE_INCIDENT_COMPONENT
            {
                INCIDENT_ID = incidentId,
                COMPONENT_OBS_NO = mine.Count + 1m,
                COMPONENT_ROLE = HseReferenceCodes.Investigator.Role,
                COMPONENT_TYPE = role.Trim().ToUpperInvariant(),
                BUSINESS_ASSOCIATE_ID = participantId.Trim(),
                REMARK = rationale.Trim(),
                ACTIVE_IND = "Y",
                SOURCE = "BeepOilGasDesktop",
                PPDM_GUID = Guid.NewGuid().ToString("N"),
                ROW_CREATED_BY = actor.Trim(),
                ROW_CREATED_DATE = now,
                ROW_CHANGED_BY = actor.Trim(),
                ROW_CHANGED_DATE = now,
            });

            var committed = await unit.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(committed))
            {
                return Failure(OgUow.Message(committed, "The assignment was not saved."),
                    InvestigationRules.CodeWriteFailed);
            }

            return await AfterAsync(source, incidentId, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, $"assigning an investigator to {incidentId}", "the assignment was not saved",
                FailureSeverity.Error);

            return Failure(
                "The assignment could not be saved" +
                (string.IsNullOrWhiteSpace(reference) ? "." : " (reference " + reference + ")."),
                reference ?? InvestigationRules.CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgInvestigationWorkspace>> AddCauseAsync(
        string incidentId,
        string causeType,
        string? category,
        string description,
        string actor,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(actor))
        {
            return Failure("A cause must record who recorded it.", InvestigationRules.CodeActorRequired);
        }

        if (InvestigationRules.ValidateCause(causeType, description) is { } refusal)
        {
            return Failure(refusal.Message, refusal.Code);
        }

        var source = _source();

        if (source is null)
        {
            return Failure("No connection is open, so the cause cannot be recorded.",
                InvestigationRules.CodeNoConnection);
        }

        try
        {
            if (await ReadAsync(source, incidentId, cancellationToken).ConfigureAwait(false) is null)
            {
                return Failure($"Incident '{incidentId}' is not in this database.",
                    InvestigationRules.CodeUnknownIncident);
            }

            var causes = await OgUow.ReadAllAsync<HSE_INCIDENT_CAUSE>(source).ConfigureAwait(false);
            var next = causes
                .Where(row => string.Equals(row.INCIDENT_ID, incidentId, StringComparison.OrdinalIgnoreCase))
                .Select(row => row.CAUSE_OBS_NO)
                .DefaultIfEmpty(0m)
                .Max() + 1m;

            var now = DateTime.UtcNow;
            var unit = OgUow.For<HSE_INCIDENT_CAUSE>(source);
            unit.PrimaryKey = "INCIDENT_ID";

            unit.Add(new HSE_INCIDENT_CAUSE
            {
                INCIDENT_ID = incidentId,
                CAUSE_OBS_NO = next,
                CAUSE_TYPE = causeType.Trim().ToUpperInvariant(),
                CAUSE_CODE = string.IsNullOrWhiteSpace(category) ? null : category.Trim().ToUpperInvariant(),
                REMARK = description.Trim(),
                EFFECTIVE_DATE = now,
                ACTIVE_IND = "Y",
                SOURCE = "BeepOilGasDesktop",
                PPDM_GUID = Guid.NewGuid().ToString("N"),
                ROW_CREATED_BY = actor.Trim(),
                ROW_CREATED_DATE = now,
                ROW_CHANGED_BY = actor.Trim(),
                ROW_CHANGED_DATE = now,
            });

            var committed = await unit.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(committed))
            {
                return Failure(OgUow.Message(committed, "The cause was not saved."),
                    InvestigationRules.CodeWriteFailed);
            }

            return await AfterAsync(source, incidentId, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, $"recording a cause for {incidentId}", "the cause was not saved", FailureSeverity.Error);

            return Failure(
                "The cause could not be saved" +
                (string.IsNullOrWhiteSpace(reference) ? "." : " (reference " + reference + ")."),
                reference ?? InvestigationRules.CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgInvestigationWorkspace>> AddActionAsync(
        string incidentId,
        string planName,
        string action,
        string owner,
        DateTime dueDate,
        OgHseScope scope,
        string actor,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(scope);

        if (string.IsNullOrWhiteSpace(actor))
        {
            return Failure("An action must record who raised it.", InvestigationRules.CodeActorRequired);
        }

        // AC-6: every action carries its owner and its due date.
        if (InvestigationRules.ValidateAction(action, owner, dueDate) is { } refusal)
        {
            return Failure(refusal.Message, refusal.Code);
        }

        var source = _source();

        if (source is null)
        {
            return Failure("No connection is open, so the action cannot be added.",
                InvestigationRules.CodeNoConnection);
        }

        try
        {
            var workspace = await ReadAsync(source, incidentId, cancellationToken).ConfigureAwait(false);

            if (workspace is null)
            {
                return Failure($"Incident '{incidentId}' is not in this database.",
                    InvestigationRules.CodeUnknownIncident);
            }

            var now = DateTime.UtcNow;
            var planId = workspace.PlanId;

            if (string.IsNullOrWhiteSpace(planId))
            {
                // The first action creates the plan and links it to the incident through the component's PROJECT_ID, the
                // link HSE-02 already reads so the incident's open count follows.
                planId = Guid.NewGuid().ToString("N");
                var number = "CAP-" + now.Year.ToString(CultureInfo.InvariantCulture) + "-" +
                             planId[..6].ToUpperInvariant();

                var plan = OgUow.For<PROJECT>(source);
                plan.PrimaryKey = "PROJECT_ID";

                plan.Add(new PROJECT
                {
                    PROJECT_ID = planId,
                    PROJECT_TYPE = HseReferenceCodes.CorrectiveAction.ProjectType,
                    PROJECT_NAME = planName.Trim(),
                    PROJECT_NUM = number,
                    START_DATE = now.Date,
                    ACTIVE_IND = "Y",
                    SOURCE = "BeepOilGasDesktop",
                    PPDM_GUID = Guid.NewGuid().ToString("N"),
                    ROW_CREATED_BY = actor.Trim(),
                    ROW_CREATED_DATE = now,
                    ROW_CHANGED_BY = actor.Trim(),
                    ROW_CHANGED_DATE = now,
                });

                var planCommitted = await plan.Commit().ConfigureAwait(false);

                if (!OgUow.IsOk(planCommitted))
                {
                    return Failure(OgUow.Message(planCommitted, "The corrective action plan was not saved."),
                        InvestigationRules.CodeWriteFailed);
                }

                var components = await OgUow.ReadAllAsync<HSE_INCIDENT_COMPONENT>(source).ConfigureAwait(false);

                var component = OgUow.For<HSE_INCIDENT_COMPONENT>(source);
                component.PrimaryKey = "INCIDENT_ID";

                component.Add(new HSE_INCIDENT_COMPONENT
                {
                    INCIDENT_ID = incidentId,
                    COMPONENT_OBS_NO = components.Count(row =>
                        string.Equals(row.INCIDENT_ID, incidentId, StringComparison.OrdinalIgnoreCase)) + 1m,
                    PROJECT_ID = planId,
                    COMPONENT_ROLE = HseReferenceCodes.CorrectiveAction.ProjectType,
                    COMPONENT_TYPE = "CA_PLAN",
                    FIELD_ID = string.IsNullOrWhiteSpace(scope.FieldId) ? null : scope.FieldId.Trim(),
                    ACTIVE_IND = "Y",
                    SOURCE = "BeepOilGasDesktop",
                    PPDM_GUID = Guid.NewGuid().ToString("N"),
                    ROW_CREATED_BY = actor.Trim(),
                    ROW_CREATED_DATE = now,
                    ROW_CHANGED_BY = actor.Trim(),
                    ROW_CHANGED_DATE = now,
                });

                var componentCommitted = await component.Commit().ConfigureAwait(false);

                if (!OgUow.IsOk(componentCommitted))
                {
                    return Failure(
                        OgUow.Message(componentCommitted, "The plan was not linked to the incident."),
                        InvestigationRules.CodeWriteFailed);
                }
            }

            var steps = await OgUow.ReadAllAsync<PROJECT_STEP>(source).ConfigureAwait(false);
            var sequence = steps.Count(row => string.Equals(row.PROJECT_ID, planId, StringComparison.Ordinal)) + 1m;
            var stepId = Guid.NewGuid().ToString("N");

            var step = OgUow.For<PROJECT_STEP>(source);
            step.PrimaryKey = "PROJECT_ID";

            step.Add(new PROJECT_STEP
            {
                PROJECT_ID = planId,
                STEP_ID = stepId,
                STEP_NAME = action.Trim(),
                STEP_SEQ_NO = sequence,
                STEP_TYPE = "ACTION",
                DUE_DATE = dueDate.Date,
                ACTIVE_IND = "Y",
                SOURCE = "BeepOilGasDesktop",
                PPDM_GUID = Guid.NewGuid().ToString("N"),
                ROW_CREATED_BY = actor.Trim(),
                ROW_CREATED_DATE = now,
                ROW_CHANGED_BY = actor.Trim(),
                ROW_CHANGED_DATE = now,
            });

            var stepCommitted = await step.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(stepCommitted))
            {
                return Failure(OgUow.Message(stepCommitted, "The action was not saved."),
                    InvestigationRules.CodeWriteFailed);
            }

            var ownerRow = OgUow.For<PROJECT_STEP_BA>(source);
            ownerRow.PrimaryKey = "PROJECT_ID";

            ownerRow.Add(new PROJECT_STEP_BA
            {
                PROJECT_ID = planId,
                BUSINESS_ASSOCIATE_ID = owner.Trim(),
                ROLE = HseReferenceCodes.CorrectiveAction.ResponsibleRole,
                ROLE_SEQ_NO = 1m,
                STEP_ID = stepId,
                STEP_BA_OBS_NO = 1m,
                EFFECTIVE_DATE = now,
                ACTIVE_IND = "Y",
                SOURCE = "BeepOilGasDesktop",
                PPDM_GUID = Guid.NewGuid().ToString("N"),
                ROW_CREATED_BY = actor.Trim(),
                ROW_CREATED_DATE = now,
                ROW_CHANGED_BY = actor.Trim(),
                ROW_CHANGED_DATE = now,
            });

            var ownerCommitted = await ownerRow.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(ownerCommitted))
            {
                return Failure(OgUow.Message(ownerCommitted, "The action's owner was not saved."),
                    InvestigationRules.CodeWriteFailed);
            }

            var status = OgUow.For<PROJECT_STATUS>(source);
            status.PrimaryKey = "PROJECT_ID";

            status.Add(new PROJECT_STATUS
            {
                PROJECT_ID = planId,
                STATUS_ID = stepId,
                STEP_ID = stepId,
                STATUS_TYPE = HseReferenceCodes.CorrectiveAction.StatusType,
                STATUS = HseReferenceCodes.CorrectiveAction.Open,
                EFFECTIVE_DATE = now,
                ACTIVE_IND = "Y",
                SOURCE = "BeepOilGasDesktop",
                PPDM_GUID = Guid.NewGuid().ToString("N"),
                ROW_CREATED_BY = actor.Trim(),
                ROW_CREATED_DATE = now,
                ROW_CHANGED_BY = actor.Trim(),
                ROW_CHANGED_DATE = now,
            });

            var statusCommitted = await status.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(statusCommitted))
            {
                return Failure(OgUow.Message(statusCommitted, "The action's first status was not saved."),
                    InvestigationRules.CodeWriteFailed);
            }

            return await AfterAsync(source, incidentId, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, $"adding a corrective action to {incidentId}", "the action was not saved",
                FailureSeverity.Error);

            return Failure(
                "The action could not be saved" +
                (string.IsNullOrWhiteSpace(reference) ? "." : " (reference " + reference + ")."),
                reference ?? InvestigationRules.CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgInvestigationWorkspace>> CompleteActionAsync(
        string incidentId,
        string stepId,
        string actor,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(actor))
        {
            return Failure("A completion must record who made it.", InvestigationRules.CodeActorRequired);
        }

        var source = _source();

        if (source is null)
        {
            return Failure("No connection is open, so the action cannot be completed.",
                InvestigationRules.CodeNoConnection);
        }

        try
        {
            var workspace = await ReadAsync(source, incidentId, cancellationToken).ConfigureAwait(false);

            if (workspace is null)
            {
                return Failure($"Incident '{incidentId}' is not in this database.",
                    InvestigationRules.CodeUnknownIncident);
            }

            var action = workspace.Actions.FirstOrDefault(row =>
                string.Equals(row.StepId, stepId, StringComparison.OrdinalIgnoreCase));

            if (action is null)
            {
                return Failure($"Action '{stepId}' is not on {incidentId}'s plan.",
                    InvestigationRules.CodeUnknownAction);
            }

            if (action.IsCompleted)
            {
                return OgResult<OgInvestigationWorkspace>.Ok(workspace);
            }

            var now = DateTime.UtcNow;

            var status = OgUow.For<PROJECT_STATUS>(source);
            status.PrimaryKey = "PROJECT_ID";

            status.Add(new PROJECT_STATUS
            {
                PROJECT_ID = action.PlanId,
                STATUS_ID = Guid.NewGuid().ToString("N"),
                STEP_ID = stepId,
                STATUS_TYPE = HseReferenceCodes.CorrectiveAction.StatusType,
                STATUS = HseReferenceCodes.CorrectiveAction.Completed,
                EFFECTIVE_DATE = now,
                ACTIVE_IND = "Y",
                SOURCE = "BeepOilGasDesktop",
                PPDM_GUID = Guid.NewGuid().ToString("N"),
                ROW_CREATED_BY = actor.Trim(),
                ROW_CREATED_DATE = now,
                ROW_CHANGED_BY = actor.Trim(),
                ROW_CHANGED_DATE = now,
            });

            var committed = await status.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(committed))
            {
                return Failure(OgUow.Message(committed, "The completion was not saved."),
                    InvestigationRules.CodeWriteFailed);
            }

            // The completion is the PROJECT_STATUS row above - that is what the register and the closure checklist read.
            // The step's ACTUAL_END_DATE is not written here: PROJECT_STEP carries no declared key in the owner metadata,
            // so an update through the unit of work cannot build its WHERE clause and would silently change nothing.
            return await AfterAsync(source, incidentId, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, $"completing action {stepId} on {incidentId}", "the completion was not saved",
                FailureSeverity.Error);

            return Failure(
                "The completion could not be saved" +
                (string.IsNullOrWhiteSpace(reference) ? "." : " (reference " + reference + ")."),
                reference ?? InvestigationRules.CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgInvestigationWorkspace>> VerifyActionAsync(
        string incidentId,
        string stepId,
        string verifier,
        string actor,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(actor))
        {
            return Failure("A verification must record who made it.", InvestigationRules.CodeActorRequired);
        }

        if (string.IsNullOrWhiteSpace(verifier))
        {
            return Failure("A verification must record who verified the action.",
                InvestigationRules.CodeInputRequired);
        }

        var source = _source();

        if (source is null)
        {
            return Failure("No connection is open, so the action cannot be verified.",
                InvestigationRules.CodeNoConnection);
        }

        try
        {
            var workspace = await ReadAsync(source, incidentId, cancellationToken).ConfigureAwait(false);

            if (workspace is null)
            {
                return Failure($"Incident '{incidentId}' is not in this database.",
                    InvestigationRules.CodeUnknownIncident);
            }

            var action = workspace.Actions.FirstOrDefault(row =>
                string.Equals(row.StepId, stepId, StringComparison.OrdinalIgnoreCase));

            if (action is null)
            {
                return Failure($"Action '{stepId}' is not on {incidentId}'s plan.",
                    InvestigationRules.CodeUnknownAction);
            }

            if (!action.IsCompleted)
            {
                return Failure(
                    $"'{action.Action}' has not been completed, so its effectiveness cannot yet be verified.",
                    InvestigationRules.CodeActionNotCompleted);
            }

            if (action.IsVerified)
            {
                return OgResult<OgInvestigationWorkspace>.Ok(workspace);
            }

            // Effectiveness is judged by someone other than the owner, which is the point of a verification.
            if (string.Equals(action.Owner?.Trim(), verifier.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                return Failure(
                    $"{verifier} owns '{action.Action}' and therefore cannot verify its effectiveness.",
                    InvestigationRules.CodeInputInvalid);
            }

            var now = DateTime.UtcNow;
            var unit = OgUow.For<PROJECT_STEP_BA>(source);
            unit.PrimaryKey = "PROJECT_ID";

            unit.Add(new PROJECT_STEP_BA
            {
                PROJECT_ID = action.PlanId,
                BUSINESS_ASSOCIATE_ID = verifier.Trim(),
                ROLE = InvestigationRules.VerifierRole,
                ROLE_SEQ_NO = 1m,
                STEP_ID = stepId,
                STEP_BA_OBS_NO = 1m,
                EFFECTIVE_DATE = now,
                ACTIVE_IND = "Y",
                SOURCE = "BeepOilGasDesktop",
                PPDM_GUID = Guid.NewGuid().ToString("N"),
                ROW_CREATED_BY = actor.Trim(),
                ROW_CREATED_DATE = now,
                ROW_CHANGED_BY = actor.Trim(),
                ROW_CHANGED_DATE = now,
            });

            var committed = await unit.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(committed))
            {
                return Failure(OgUow.Message(committed, "The verification was not saved."),
                    InvestigationRules.CodeWriteFailed);
            }

            return await AfterAsync(source, incidentId, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, $"verifying action {stepId} on {incidentId}", "the verification was not saved",
                FailureSeverity.Error);

            return Failure(
                "The verification could not be saved" +
                (string.IsNullOrWhiteSpace(reference) ? "." : " (reference " + reference + ")."),
                reference ?? InvestigationRules.CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgInvestigationWorkspace>> CloseAsync(
        string incidentId,
        string decision,
        string reason,
        string actor,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(actor))
        {
            return Failure("A closure must record who made it.", InvestigationRules.CodeActorRequired);
        }

        if (string.IsNullOrWhiteSpace(decision))
        {
            return Failure("A closure needs its decision.", InvestigationRules.CodeInputRequired);
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            return Failure("A closure needs its reason.", InvestigationRules.CodeReasonRequired);
        }

        var source = _source();

        if (source is null)
        {
            return Failure("No connection is open, so the incident cannot be closed.",
                InvestigationRules.CodeNoConnection);
        }

        try
        {
            var workspace = await ReadAsync(source, incidentId, cancellationToken).ConfigureAwait(false);

            if (workspace is null)
            {
                return Failure($"Incident '{incidentId}' is not in this database.",
                    InvestigationRules.CodeUnknownIncident);
            }

            // AC-4 and AC-5: the checklist decides, and every reason is returned rather than the first one.
            var blocks = InvestigationRules.ClosureBlocks(workspace, actor);

            if (blocks.Count > 0)
            {
                return Failure(
                    string.Join(" ", blocks.Select(block => block.Message)),
                    blocks[0].Code);
            }

            var responses = await OgUow.ReadAllAsync<HSE_INCIDENT_RESPONSE>(source).ConfigureAwait(false);
            var next = responses
                .Where(row => string.Equals(row.INCIDENT_ID, incidentId, StringComparison.OrdinalIgnoreCase))
                .Select(row => row.RESPONSE_OBS_NO)
                .DefaultIfEmpty(0m)
                .Max() + 1m;

            var now = DateTime.UtcNow;
            var unit = OgUow.For<HSE_INCIDENT_RESPONSE>(source);
            unit.PrimaryKey = "INCIDENT_ID";

            unit.Add(new HSE_INCIDENT_RESPONSE
            {
                INCIDENT_ID = incidentId,
                RESPONSE_OBS_NO = next,
                RESPONSE_TYPE = "STATUS",
                RESPONSE_RESULT = IncidentState.Closed,
                EFFECTIVE_DATE = now,
                REMARK = decision.Trim() + " · " + reason.Trim(),
                ACTIVE_IND = "Y",
                SOURCE = "BeepOilGasDesktop",
                PPDM_GUID = Guid.NewGuid().ToString("N"),
                ROW_CREATED_BY = actor.Trim(),
                ROW_CREATED_DATE = now,
                ROW_CHANGED_BY = actor.Trim(),
                ROW_CHANGED_DATE = now,
            });

            var committed = await unit.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(committed))
            {
                return Failure(OgUow.Message(committed, "The closure was not saved."),
                    InvestigationRules.CodeWriteFailed);
            }

            return await AfterAsync(source, incidentId, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, $"closing {incidentId}", "the closure was not saved", FailureSeverity.Error);

            return Failure(
                "The closure could not be saved" +
                (string.IsNullOrWhiteSpace(reference) ? "." : " (reference " + reference + ")."),
                reference ?? InvestigationRules.CodeWriteFailed);
        }
    }

    // ------------------------------------------------------------------ the read model

    private async Task<OgResult<OgInvestigationWorkspace>> AfterAsync(
        IDataSource source,
        string incidentId,
        CancellationToken cancellationToken)
    {
        var workspace = await ReadAsync(source, incidentId, cancellationToken).ConfigureAwait(false);

        return workspace is null
            ? Failure($"Incident '{incidentId}' is not in this database.", InvestigationRules.CodeUnknownIncident)
            : OgResult<OgInvestigationWorkspace>.Ok(workspace);
    }

    private static async Task<OgInvestigationWorkspace?> ReadAsync(
        IDataSource source,
        string incidentId,
        CancellationToken cancellationToken)
    {
        var incidents = await OgUow.ReadAllAsync<HSE_INCIDENT>(source).ConfigureAwait(false);
        var responses = await OgUow.ReadAllAsync<HSE_INCIDENT_RESPONSE>(source).ConfigureAwait(false);
        var components = await OgUow.ReadAllAsync<HSE_INCIDENT_COMPONENT>(source).ConfigureAwait(false);
        var causes = await OgUow.ReadAllAsync<HSE_INCIDENT_CAUSE>(source).ConfigureAwait(false);
        var projects = await OgUow.ReadAllAsync<PROJECT>(source).ConfigureAwait(false);
        var steps = await OgUow.ReadAllAsync<PROJECT_STEP>(source).ConfigureAwait(false);
        var parties = await OgUow.ReadAllAsync<PROJECT_STEP_BA>(source).ConfigureAwait(false);
        var statuses = await OgUow.ReadAllAsync<PROJECT_STATUS>(source).ConfigureAwait(false);

        cancellationToken.ThrowIfCancellationRequested();

        var incident = incidents.FirstOrDefault(row =>
            string.Equals(row.INCIDENT_ID, incidentId, StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(row.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase));

        if (incident is null)
        {
            return null;
        }

        var state = responses
            .Where(row => string.Equals(row.INCIDENT_ID, incidentId, StringComparison.OrdinalIgnoreCase))
            .Where(row => !string.Equals(row.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase))
            .Where(row => string.Equals(row.RESPONSE_TYPE?.Trim(), "STATUS", StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(row => row.RESPONSE_OBS_NO)
            .ThenByDescending(row => row.EFFECTIVE_DATE)
            .FirstOrDefault()?.RESPONSE_RESULT ?? string.Empty;

        var mine = components
            .Where(row => string.Equals(row.INCIDENT_ID, incidentId, StringComparison.OrdinalIgnoreCase))
            .Where(row => !string.Equals(row.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase))
            .ToList();

        var investigatorRow = mine.FirstOrDefault(row => string.Equals(row.COMPONENT_ROLE?.Trim(),
            HseReferenceCodes.Investigator.Role, StringComparison.OrdinalIgnoreCase));

        var planId = mine
            .Select(row => row.PROJECT_ID)
            .FirstOrDefault(id => !string.IsNullOrWhiteSpace(id));

        var plan = projects.FirstOrDefault(row =>
            string.Equals(row.PROJECT_ID, planId, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(row.PROJECT_TYPE?.Trim(), HseReferenceCodes.CorrectiveAction.ProjectType,
                StringComparison.OrdinalIgnoreCase));

        var recordedCauses = causes
            .Where(row => string.Equals(row.INCIDENT_ID, incidentId, StringComparison.OrdinalIgnoreCase))
            .Where(row => !string.Equals(row.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase))
            .OrderBy(row => row.CAUSE_OBS_NO)
            .Select(row => new OgCause(
                (int)row.CAUSE_OBS_NO,
                row.CAUSE_TYPE ?? string.Empty,
                row.CAUSE_CODE ?? string.Empty,
                row.REMARK ?? string.Empty,
                row.EFFECTIVE_DATE))
            .ToList();

        var actions = new List<OgCorrectiveAction>();

        if (plan is not null)
        {
            var planSteps = steps
                .Where(row => string.Equals(row.PROJECT_ID, plan.PROJECT_ID, StringComparison.Ordinal))
                .Where(row => !string.Equals(row.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase))
                .OrderBy(row => row.STEP_SEQ_NO)
                .ToList();

            foreach (var step in planSteps)
            {
                var latest = statuses
                    .Where(row => string.Equals(row.PROJECT_ID, plan.PROJECT_ID, StringComparison.Ordinal))
                    .Where(row => string.Equals(row.STEP_ID, step.STEP_ID, StringComparison.OrdinalIgnoreCase))
                    .Where(row => !string.Equals(row.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase))
                    .OrderByDescending(row => row.EFFECTIVE_DATE)
                    .ThenByDescending(row => row.ROW_CREATED_DATE)
                    .FirstOrDefault();

                var owner = parties
                    .Where(row => string.Equals(row.PROJECT_ID, plan.PROJECT_ID, StringComparison.Ordinal))
                    .Where(row => string.Equals(row.STEP_ID, step.STEP_ID, StringComparison.OrdinalIgnoreCase))
                    .Where(row => string.Equals(row.ROLE?.Trim(),
                        HseReferenceCodes.CorrectiveAction.ResponsibleRole, StringComparison.OrdinalIgnoreCase))
                    .Where(row => !string.Equals(row.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase))
                    .Select(row => row.BUSINESS_ASSOCIATE_ID)
                    .FirstOrDefault();

                var verifier = parties
                    .Where(row => string.Equals(row.PROJECT_ID, plan.PROJECT_ID, StringComparison.Ordinal))
                    .Where(row => string.Equals(row.STEP_ID, step.STEP_ID, StringComparison.OrdinalIgnoreCase))
                    .Where(row => string.Equals(row.ROLE?.Trim(), InvestigationRules.VerifierRole,
                        StringComparison.OrdinalIgnoreCase))
                    .Where(row => !string.Equals(row.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase))
                    .OrderByDescending(row => row.EFFECTIVE_DATE)
                    .FirstOrDefault();

                var completed = string.Equals(latest?.STATUS?.Trim(),
                    HseReferenceCodes.CorrectiveAction.Completed, StringComparison.OrdinalIgnoreCase);

                actions.Add(new OgCorrectiveAction(
                    plan.PROJECT_ID,
                    step.STEP_ID,
                    step.STEP_NAME ?? string.Empty,
                    owner,
                    step.DUE_DATE,
                    latest?.STATUS ?? HseReferenceCodes.CorrectiveAction.Open,
                    verifier?.BUSINESS_ASSOCIATE_ID,
                    verifier?.EFFECTIVE_DATE,
                    !completed && step.DUE_DATE is { } due && due.Date < DateTime.UtcNow.Date));
            }
        }

        return new OgInvestigationWorkspace(
            incidentId,
            state,
            investigatorRow?.BUSINESS_ASSOCIATE_ID ?? investigatorRow?.REMARK,
            investigatorRow?.COMPONENT_TYPE,
            recordedCauses,
            actions,
            plan?.PROJECT_ID,
            plan?.PROJECT_NAME,
            plan?.PROJECT_NUM,
            DateTime.UtcNow);
    }

    private static OgResult<OgInvestigationWorkspace> Failure(string message, string code) =>
        OgResult<OgInvestigationWorkspace>.Failed(message, code);
}
