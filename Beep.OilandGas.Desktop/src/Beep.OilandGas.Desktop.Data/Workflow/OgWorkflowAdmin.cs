using System.Globalization;
using System.Text.Json;
using Beep.OilandGas.LifeCycle.Data.Tables;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Data.Workflow;

/// <summary>One recorded configuration change (WFL-06, AC-2).</summary>
public sealed record OgConfigChange(
    string Area,
    string TargetId,
    string? TargetDetail,
    string Actor,
    string Reason,
    string? Previous,
    string? New,
    DateTime At)
{
    public string Describe() =>
        At.ToLocalTime().ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture) + " · " + Area.ToLowerInvariant() +
        " · " + TargetId +
        (string.IsNullOrWhiteSpace(TargetDetail) ? string.Empty : " / " + TargetDetail) +
        " · " + Actor + " · " + Reason +
        (string.IsNullOrWhiteSpace(Previous) ? string.Empty : " · was " + Previous) +
        (string.IsNullOrWhiteSpace(New) ? string.Empty : " · now " + New);
}

/// <summary>One recorded version of a definition (WFL-06, the Versions tab).</summary>
public sealed record OgVersionRow(
    string VersionId,
    string DefinitionId,
    string DefinitionName,
    string Number,
    string Change,
    bool HasSnapshot,
    bool IsLive)
{
    public string StateText => IsLive
        ? "live"
        : HasSnapshot ? "available" : "no snapshot - cannot be activated";

    public string Describe() =>
        DefinitionName + " · version " + Number + " · " + StateText +
        (string.IsNullOrWhiteSpace(Change) ? string.Empty : " · " + Change);
}

/// <summary>One dependency between definitions (WFL-06, the Dependencies tab).</summary>
public sealed record OgDependencyRow(
    string Id,
    string From,
    string To,
    string Type,
    string? Condition)
{
    public string Describe() =>
        From + " " + Type.ToLowerInvariant() + " " + To +
        (string.IsNullOrWhiteSpace(Condition) ? string.Empty : " when " + Condition);
}

/// <summary>One business-event trigger (WFL-06, the Triggers tab).</summary>
public sealed record OgTriggerRow(
    string Id,
    string Name,
    string EventType,
    string EntityType,
    string TargetDefinitionId,
    bool Active,
    int Priority,
    string? Description)
{
    public string StateText => Active ? "enabled" : "disabled";

    public string Describe() =>
        Name + " · " + EventType + " on " + EntityType + " → " + TargetDefinitionId + " · " + StateText +
        " · priority " + Priority.ToString(CultureInfo.InvariantCulture);
}

/// <summary>One segregation-of-duties rule (WFL-06, the SoD rules tab).</summary>
public sealed record OgSodRuleRow(
    string Id,
    string Name,
    string PermissionA,
    string PermissionB,
    string Severity,
    bool Blocking,
    bool Active,
    string Scope,
    string? Description,
    string? Mitigation)
{
    public string StateText => Active
        ? Blocking ? "active and blocking" : "active, advisory"
        : "inactive";

    public string Describe() =>
        Name + " · " + PermissionA + " + " + PermissionB + " · " + Severity.ToLowerInvariant() + " · " + StateText +
        (string.IsNullOrWhiteSpace(Mitigation) ? string.Empty : " · mitigation: " + Mitigation);
}

/// <summary>The outcome of testing a combination (WFL-06, AC-4).</summary>
public sealed record OgSodTest(
    string PermissionA,
    string PermissionB,
    bool Failed,
    string Sentence,
    IReadOnlyList<string> Rules)
{
    public string StateText => Failed ? "fail" : "pass";

    public string Describe() => StateText + ": " + Sentence;
}

/// <summary>One delegation, person to person, with its window (WFL-06, AC-3).</summary>
public sealed record OgDelegationRow(
    string Id,
    string Name,
    string? Delegator,
    string? Delegate,
    DateTime? From,
    DateTime? To,
    decimal Limit,
    string? Currency,
    bool Active,
    string StateText)
{
    public string Describe() =>
        Name + " · " + (Delegator ?? "(no delegator)") + " → " + (Delegate ?? "(no delegate)") +
        " · " + (From?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? "open") + " to " +
        (To?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? "open") +
        " · limit " + Limit.ToString("0.##", CultureInfo.InvariantCulture) + " " + (Currency ?? string.Empty) +
        " · " + StateText;
}

/// <summary>One compensating control (WFL-06, the controls tab).</summary>
public sealed record OgCompensatingControlRow(
    string Id,
    string ConflictId,
    string UserId,
    string Type,
    string Description,
    string Status,
    DateTime EffectiveFrom,
    DateTime EffectiveTo)
{
    public string Describe() =>
        UserId + " · " + Type + " · " + Description + " · " + Status.ToLowerInvariant() + " · " +
        EffectiveFrom.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) + " to " +
        EffectiveTo.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
}

/// <summary>One finding in the Diagnostics tab (WFL-06, AC-5).</summary>
public sealed record OgAdminDiagnostic(string Kind, string TargetId, string Cause)
{
    public string Describe() => Kind + " · " + TargetId + " · " + Cause;
}

/// <summary>The workflow administration workspace (WFL-06).</summary>
public sealed record OgAdminWorkspace(
    IReadOnlyList<OgVersionRow> Versions,
    IReadOnlyList<OgDependencyRow> Dependencies,
    IReadOnlyList<OgTriggerRow> Triggers,
    IReadOnlyList<OgSodRuleRow> SodRules,
    IReadOnlyList<OgDelegationRow> Delegations,
    IReadOnlyList<OgCompensatingControlRow> Controls,
    IReadOnlyList<OgAdminDiagnostic> Diagnostics,
    IReadOnlyList<OgConfigChange> Changes,
    DateTime AsOf,
    DateTime ReadAt)
{
    public int InactiveDelegations => Delegations.Count(delegation => !delegation.Active);

    public string CountsText =>
        Versions.Count.ToString(CultureInfo.InvariantCulture) + " version(s) · " +
        Dependencies.Count.ToString(CultureInfo.InvariantCulture) + " dependency(ies) · " +
        Triggers.Count.ToString(CultureInfo.InvariantCulture) + " trigger(s) · " +
        SodRules.Count.ToString(CultureInfo.InvariantCulture) + " SoD rule(s) · " +
        Delegations.Count.ToString(CultureInfo.InvariantCulture) + " delegation(s) · " +
        Diagnostics.Count.ToString(CultureInfo.InvariantCulture) + " diagnostic(s)";

    public string VersionsText => Versions.Count == 0
        ? "No version is recorded."
        : string.Join(Environment.NewLine, Versions.Select(version => version.Describe()));

    public string DependenciesText => Dependencies.Count == 0
        ? "No dependency is recorded."
        : string.Join(Environment.NewLine, Dependencies.Select(dependency => dependency.Describe()));

    public string TriggersText => Triggers.Count == 0
        ? "No trigger is recorded."
        : string.Join(Environment.NewLine, Triggers.Select(trigger => trigger.Describe()));

    public string SodText => SodRules.Count == 0
        ? "No segregation-of-duties rule is recorded."
        : string.Join(Environment.NewLine, SodRules.Select(rule => rule.Describe()));

    public string DelegationsText => Delegations.Count == 0
        ? "No delegation is recorded."
        : string.Join(Environment.NewLine, Delegations.Select(delegation => delegation.Describe()));

    public string ControlsText => Controls.Count == 0
        ? "No compensating control is recorded."
        : string.Join(Environment.NewLine, Controls.Select(control => control.Describe()));

    public string DiagnosticsText => Diagnostics.Count == 0
        ? "No orphan, missing definition or broken handoff is recorded."
        : string.Join(Environment.NewLine, Diagnostics.Select(finding => "✗ " + finding.Describe()));

    public string ChangesText => Changes.Count == 0
        ? "No configuration change is recorded."
        : string.Join(Environment.NewLine, Changes.Select(change => change.Describe()));

    public string ReadText =>
        "read at " + ReadAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
}

/// <summary>The workflow administration rules WFL-06 states once.</summary>
public static class WorkflowAdminRules
{
    public const string CodeNoConnection = "workflow-admin-no-connection";
    public const string CodeReadFailed = "workflow-admin-read-failed";
    public const string CodeWriteFailed = "workflow-admin-write-failed";
    public const string CodeReasonRequired = "workflow-admin-reason-required";
    public const string CodeActorRequired = "workflow-admin-actor-required";
    public const string CodeDependencyCycle = "workflow-admin-dependency-cycle";
    public const string CodeUnknownTarget = "workflow-admin-unknown-target";
    public const string CodeNoSnapshot = "workflow-admin-no-snapshot";
    public const string CodeWindowInvalid = "workflow-admin-window-invalid";
    public const string CodeDelegationInvalid = "workflow-admin-delegation-invalid";
    public const string CodeAlreadyInThatState = "workflow-admin-already-in-that-state";
    public const string CodeSelfDependency = "workflow-admin-self-dependency";

    /// <summary>AC-2 and the state table: every change names who made it and why.</summary>
    public static (string Code, string Message)? ValidateChange(string? actor, string? reason, string? confirmation)
    {
        if (string.IsNullOrWhiteSpace(actor))
        {
            return (CodeActorRequired, "A configuration change must record who made it.");
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            return (CodeReasonRequired,
                "A configuration change needs its reason: it affects every approval that uses this configuration.");
        }

        return string.IsNullOrWhiteSpace(confirmation)
            ? (CodeReasonRequired,
                "A configuration change needs the confirmation that names the definitions it affects.")
            : null;
    }

    /// <summary>
    /// AC-1: the dependency cannot be added if it closes a cycle on itself. The refusal names the path.
    /// </summary>
    public static (string Code, string Message)? ValidateDependency(
        IReadOnlyList<OgDependencyRow> dependencies,
        string? dependent,
        string? prerequisite)
    {
        ArgumentNullException.ThrowIfNull(dependencies);

        if (string.IsNullOrWhiteSpace(dependent) || string.IsNullOrWhiteSpace(prerequisite))
        {
            return (CodeUnknownTarget, "A dependency names both the definition that depends and the one it requires.");
        }

        if (string.Equals(dependent.Trim(), prerequisite.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            return (CodeSelfDependency,
                dependent.Trim() + " cannot depend on itself: that is a cycle of one definition.");
        }

        var candidate = dependencies
            .Select(row => new OgWorkflowDependency(row.From, null, row.To, null, row.Type, row.Condition))
            .Append(new OgWorkflowDependency(dependent.Trim(), null, prerequisite.Trim(), null, "REQUIRES", null))
            .ToList();

        var cycles = DefinitionCatalogueRules.DependencyCycles(candidate);

        return cycles.Count == 0
            ? null
            : (CodeDependencyCycle,
                "Adding " + dependent.Trim() + " → " + prerequisite.Trim() + " would close a cycle: " +
                string.Join(" → ", cycles[0].Path) + ". The dependency is refused.");
    }

    /// <summary>
    /// AC-3: a delegation is active only while it is in force and inside its window; outside it, it is inactive and marked.
    /// </summary>
    public static (bool Active, string StateText) DelegationState(
        string? activeInd,
        DateTime? from,
        DateTime? to,
        DateTime asOf)
    {
        if (string.Equals(activeInd?.Trim(), "N", StringComparison.OrdinalIgnoreCase))
        {
            return (false, "inactive (withdrawn)");
        }

        if (from is null && to is null)
        {
            return (true, "active (no window recorded)");
        }

        if (from is { } start && asOf < start)
        {
            return (false, "inactive (outside its window: starts " +
                           start.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) + ")");
        }

        if (to is { } end && asOf > end)
        {
            return (false, "inactive (outside its window: ended " +
                           end.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) + ")");
        }

        return (true, "active (inside its window)");
    }

    /// <summary>
    /// AC-4: the tester answers pass or fail and names the rule that decided it. A blocking match is a failure; an advisory
    /// match is reported as one, so the administrator knows the difference.
    /// </summary>
    public static OgSodTest Test(IReadOnlyList<OgSodRuleRow> rules, string? permissionA, string? permissionB)
    {
        ArgumentNullException.ThrowIfNull(rules);

        var first = permissionA?.Trim() ?? string.Empty;
        var second = permissionB?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(first) || string.IsNullOrWhiteSpace(second))
        {
            return new OgSodTest(first, second, false,
                "Both permissions are needed to test a combination.", []);
        }

        var matches = rules
            .Where(rule => rule.Active)
            .Where(rule =>
                (string.Equals(rule.PermissionA, first, StringComparison.OrdinalIgnoreCase) &&
                 string.Equals(rule.PermissionB, second, StringComparison.OrdinalIgnoreCase)) ||
                (string.Equals(rule.PermissionA, second, StringComparison.OrdinalIgnoreCase) &&
                 string.Equals(rule.PermissionB, first, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        if (matches.Count == 0)
        {
            return new OgSodTest(first, second, false,
                "No recorded rule forbids combining " + first + " with " + second + ".", []);
        }

        var blocking = matches.Where(match => match.Blocking).ToList();

        return blocking.Count > 0
            ? new OgSodTest(first, second, true,
                "Combining " + first + " with " + second + " breaks " +
                string.Join(", ", blocking.Select(match => "'" + match.Name + "'")) + ".",
                blocking.Select(match => match.Id).ToList())
            : new OgSodTest(first, second, false,
                "Combining " + first + " with " + second + " matches " +
                string.Join(", ", matches.Select(match => "'" + match.Name + "'")) +
                ", which is advisory rather than blocking.",
                matches.Select(match => match.Id).ToList());
    }

    /// <summary>AC-3's write rule: a delegation needs both names and a window that ends after it begins.</summary>
    public static (string Code, string Message)? ValidateDelegation(
        string? delegator,
        string? delegateUser,
        DateTime? from,
        DateTime? to,
        decimal limit)
    {
        if (string.IsNullOrWhiteSpace(delegator) || string.IsNullOrWhiteSpace(delegateUser))
        {
            return (CodeDelegationInvalid, "A delegation names the person acting and the person acted for.");
        }

        if (string.Equals(delegator.Trim(), delegateUser.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            return (CodeDelegationInvalid, delegator.Trim() + " cannot delegate to themselves.");
        }

        if (from is null || to is null)
        {
            return (CodeWindowInvalid, "A delegation records the window it may be used in: both its start and its end.");
        }

        if (to.Value <= from.Value)
        {
            return (CodeWindowInvalid,
                "A delegation's end (" + to.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) +
                ") must be after its start (" + from.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) + ").");
        }

        return limit < 0m
            ? (CodeDelegationInvalid, "A delegation's authority limit cannot be negative.")
            : null;
    }
}

/// <summary>WFL-06: workflow administration.</summary>
public interface IOgWorkflowAdmin
{
    Task<OgResult<OgAdminWorkspace>> LoadAsync(
        DateTime asOf,
        CancellationToken cancellationToken = default);

    /// <summary>AC-4: tests a combination without writing anything.</summary>
    OgSodTest TestSod(OgAdminWorkspace workspace, string permissionA, string permissionB);

    /// <summary>AC-1 and AC-2: adds a dependency, refusing a cycle and recording actor, reason and previous value.</summary>
    Task<OgResult<OgAdminWorkspace>> AddDependencyAsync(
        string dependent,
        string prerequisite,
        string type,
        string? condition,
        string reason,
        string confirmation,
        string actor,
        CancellationToken cancellationToken = default);

    Task<OgResult<OgAdminWorkspace>> SetTriggerActiveAsync(
        string triggerId,
        bool active,
        string reason,
        string confirmation,
        string actor,
        CancellationToken cancellationToken = default);

    Task<OgResult<OgAdminWorkspace>> SetSodRuleBlockingAsync(
        string ruleId,
        bool blocking,
        string reason,
        string confirmation,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>Points a definition at one of its recorded versions, recording what it replaced.</summary>
    Task<OgResult<OgAdminWorkspace>> ActivateVersionAsync(
        string definitionId,
        string versionId,
        string reason,
        string confirmation,
        string actor,
        CancellationToken cancellationToken = default);

    Task<OgResult<OgAdminWorkspace>> AddDelegationAsync(
        string delegator,
        string delegateUser,
        DateTime fromUtc,
        DateTime toUtc,
        decimal limit,
        string currency,
        string reason,
        string confirmation,
        string actor,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// WFL-06 over the owner's <c>WORKFLOW_VERSION</c>, <c>WORKFLOW_DEPENDENCY</c>, <c>BUSINESS_EVENT_TRIGGER</c>, <c>SOD_RULE</c>,
/// <c>DELEGATION_OF_AUTHORITY</c> and <c>COMPENSATING_CONTROL</c>, plus the <c>WORKFLOW_CONFIGURATION_CHANGE</c> record this
/// feature added.
/// </summary>
/// <remarks>
/// Every write records actor, reason and the value it replaced, because the owner's configuration tables keep no history of their
/// own. A change is refused without a reason and without the confirmation that names the affected definitions, and a dependency
/// that would close a cycle is refused before anything is written.
/// </remarks>
public sealed class OgWorkflowAdminService : IOgWorkflowAdmin
{
    private readonly Func<IDataSource?> _source;
    private readonly IFailureReporter _failures;

    public OgWorkflowAdminService(Func<IDataSource?> source, IFailureReporter failures)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _failures = failures ?? throw new ArgumentNullException(nameof(failures));
    }

    public async Task<OgResult<OgAdminWorkspace>> LoadAsync(
        DateTime asOf,
        CancellationToken cancellationToken = default)
    {
        var source = _source();

        if (source is null)
        {
            return OgResult<OgAdminWorkspace>.Failed(
                "No connection is open, so the workflow configuration cannot be read.",
                WorkflowAdminRules.CodeNoConnection);
        }

        try
        {
            return await ReadAsync(source, asOf, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, "reading the workflow configuration", "the configuration is not shown", FailureSeverity.Error);

            return OgResult<OgAdminWorkspace>.Failed(
                "The workflow configuration could not be read" +
                (string.IsNullOrWhiteSpace(reference) ? "." : " (reference " + reference + ")."),
                reference ?? WorkflowAdminRules.CodeReadFailed);
        }
    }

    public OgSodTest TestSod(OgAdminWorkspace workspace, string permissionA, string permissionB)
    {
        ArgumentNullException.ThrowIfNull(workspace);

        return WorkflowAdminRules.Test(workspace.SodRules, permissionA, permissionB);
    }

    public Task<OgResult<OgAdminWorkspace>> AddDependencyAsync(
        string dependent,
        string prerequisite,
        string type,
        string? condition,
        string reason,
        string confirmation,
        string actor,
        CancellationToken cancellationToken = default) =>
        RunAsync(async (source, snapshot) =>
        {
            // AC-1: the cycle is refused before anything is written.
            if (WorkflowAdminRules.ValidateDependency(snapshot.Dependencies, dependent, prerequisite) is { } refusal)
            {
                return OgResult<OgAdminWorkspace>.Failed(refusal.Message, refusal.Code);
            }

            var unit = OgUow.For<WORKFLOW_DEPENDENCY>(source);
            unit.PrimaryKey = nameof(WORKFLOW_DEPENDENCY.DEPENDENCY_ID);

            unit.Add(new WORKFLOW_DEPENDENCY
            {
                DEPENDENCY_ID = Guid.NewGuid().ToString("N"),
                DEPENDENT_PROCESS_DEF_ID = dependent.Trim(),
                PREREQUISITE_PROCESS_DEF_ID = prerequisite.Trim(),
                DEPENDENCY_TYPE = string.IsNullOrWhiteSpace(type) ? "REQUIRES" : type.Trim().ToUpperInvariant(),
                CONDITION_EXPRESSION = string.IsNullOrWhiteSpace(condition) ? null : condition.Trim(),
            });

            var committed = await unit.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(committed))
            {
                return OgResult<OgAdminWorkspace>.Failed(
                    OgUow.Message(committed, "The dependency was not saved."), WorkflowAdminRules.CodeWriteFailed);
            }

            await AuditAsync(source, "DEPENDENCY", dependent.Trim(), prerequisite.Trim(), actor, reason, null,
                "{\"dependent\":\"" + dependent.Trim() + "\",\"prerequisite\":\"" + prerequisite.Trim() + "\"}",
                cancellationToken).ConfigureAwait(false);

            return null;
        }, reason, confirmation, actor, cancellationToken);

    public Task<OgResult<OgAdminWorkspace>> SetTriggerActiveAsync(
        string triggerId,
        bool active,
        string reason,
        string confirmation,
        string actor,
        CancellationToken cancellationToken = default) =>
        RunAsync(async (source, snapshot) =>
        {
            var existing = snapshot.Triggers.FirstOrDefault(trigger =>
                string.Equals(trigger.Id, triggerId, StringComparison.OrdinalIgnoreCase));

            if (existing is null)
            {
                return OgResult<OgAdminWorkspace>.Failed(
                    "Trigger '" + triggerId + "' is not recorded.", WorkflowAdminRules.CodeUnknownTarget);
            }

            if (existing.Active == active)
            {
                return OgResult<OgAdminWorkspace>.Failed(
                    existing.Name + " is already " + existing.StateText + ".",
                    WorkflowAdminRules.CodeAlreadyInThatState);
            }

            var unit = OgUow.For<BUSINESS_EVENT_TRIGGER>(source);
            unit.PrimaryKey = nameof(BUSINESS_EVENT_TRIGGER.TRIGGER_ID);

            var rows = await OgUow.ReadAsync(
                unit,
                [OgUow.Equal(nameof(BUSINESS_EVENT_TRIGGER.TRIGGER_ID), existing.Id)],
                cancellationToken).ConfigureAwait(false);

            var row = rows.FirstOrDefault();

            if (row is null)
            {
                return OgResult<OgAdminWorkspace>.Failed(
                    "Trigger '" + triggerId + "' is not recorded.", WorkflowAdminRules.CodeUnknownTarget);
            }

            row.IS_ACTIVE = active ? "Y" : "N";

            var updated = unit.Update(row);

            if (!OgUow.IsOk(updated))
            {
                return OgResult<OgAdminWorkspace>.Failed(
                    OgUow.Message(updated, "The trigger was not saved."), WorkflowAdminRules.CodeWriteFailed);
            }

            var committed = await unit.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(committed))
            {
                return OgResult<OgAdminWorkspace>.Failed(
                    OgUow.Message(committed, "The trigger was not saved."), WorkflowAdminRules.CodeWriteFailed);
            }

            await AuditAsync(source, "TRIGGER", existing.Id, null, actor, reason,
                "{\"active\":" + (existing.Active ? "true" : "false") + "}",
                "{\"active\":" + (active ? "true" : "false") + "}", cancellationToken).ConfigureAwait(false);

            return null;
        }, reason, confirmation, actor, cancellationToken);

    public Task<OgResult<OgAdminWorkspace>> SetSodRuleBlockingAsync(
        string ruleId,
        bool blocking,
        string reason,
        string confirmation,
        string actor,
        CancellationToken cancellationToken = default) =>
        RunAsync(async (source, snapshot) =>
        {
            var existing = snapshot.SodRules.FirstOrDefault(rule =>
                string.Equals(rule.Id, ruleId, StringComparison.OrdinalIgnoreCase));

            if (existing is null)
            {
                return OgResult<OgAdminWorkspace>.Failed(
                    "Segregation rule '" + ruleId + "' is not recorded.", WorkflowAdminRules.CodeUnknownTarget);
            }

            if (existing.Blocking == blocking)
            {
                return OgResult<OgAdminWorkspace>.Failed(
                    existing.Name + " is already " + existing.StateText + ".",
                    WorkflowAdminRules.CodeAlreadyInThatState);
            }

            var unit = OgUow.For<SOD_RULE>(source);
            unit.PrimaryKey = nameof(SOD_RULE.SOD_RULE_ID);

            var rows = await OgUow.ReadAsync(
                unit,
                [OgUow.Equal(nameof(SOD_RULE.SOD_RULE_ID), existing.Id)],
                cancellationToken).ConfigureAwait(false);

            var row = rows.FirstOrDefault();

            if (row is null)
            {
                return OgResult<OgAdminWorkspace>.Failed(
                    "Segregation rule '" + ruleId + "' is not recorded.", WorkflowAdminRules.CodeUnknownTarget);
            }

            row.IS_BLOCKING = blocking ? "Y" : "N";

            var updated = unit.Update(row);

            if (!OgUow.IsOk(updated))
            {
                return OgResult<OgAdminWorkspace>.Failed(
                    OgUow.Message(updated, "The rule was not saved."), WorkflowAdminRules.CodeWriteFailed);
            }

            var committed = await unit.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(committed))
            {
                return OgResult<OgAdminWorkspace>.Failed(
                    OgUow.Message(committed, "The rule was not saved."), WorkflowAdminRules.CodeWriteFailed);
            }

            await AuditAsync(source, "SOD_RULE", existing.Id, null, actor, reason,
                "{\"blocking\":" + (existing.Blocking ? "true" : "false") + "}",
                "{\"blocking\":" + (blocking ? "true" : "false") + "}", cancellationToken).ConfigureAwait(false);

            return null;
        }, reason, confirmation, actor, cancellationToken);

    public Task<OgResult<OgAdminWorkspace>> ActivateVersionAsync(
        string definitionId,
        string versionId,
        string reason,
        string confirmation,
        string actor,
        CancellationToken cancellationToken = default) =>
        RunAsync(async (source, snapshot) =>
        {
            var version = snapshot.Versions.FirstOrDefault(candidate =>
                string.Equals(candidate.VersionId, versionId, StringComparison.OrdinalIgnoreCase));

            if (version is null)
            {
                return OgResult<OgAdminWorkspace>.Failed(
                    "Version '" + versionId + "' is not recorded.", WorkflowAdminRules.CodeUnknownTarget);
            }

            if (!version.HasSnapshot)
            {
                return OgResult<OgAdminWorkspace>.Failed(
                    version.Describe() + " records no configuration snapshot, so there is nothing to activate.",
                    WorkflowAdminRules.CodeNoSnapshot);
            }

            var target = string.IsNullOrWhiteSpace(definitionId) ? version.DefinitionId : definitionId.Trim();

            var versions = await OgUow.ReadAllAsync<WORKFLOW_VERSION>(source).ConfigureAwait(false);

            var stored = versions.FirstOrDefault(candidate =>
                string.Equals(candidate.VERSION_ID, version.VersionId, StringComparison.OrdinalIgnoreCase));

            if (stored is null || string.IsNullOrWhiteSpace(stored.PROCESS_CONFIG_SNAPSHOT))
            {
                return OgResult<OgAdminWorkspace>.Failed(
                    version.Describe() + " records no configuration snapshot, so there is nothing to activate.",
                    WorkflowAdminRules.CodeNoSnapshot);
            }

            var unit = OgUow.For<PROCESS_DEFINITION>(source);
            unit.PrimaryKey = nameof(PROCESS_DEFINITION.PROCESS_DEFINITION_ID);

            var rows = await OgUow.ReadAsync(
                unit,
                [OgUow.Equal(nameof(PROCESS_DEFINITION.PROCESS_DEFINITION_ID), target)],
                cancellationToken).ConfigureAwait(false);

            var definition = rows.FirstOrDefault();

            if (definition is null)
            {
                return OgResult<OgAdminWorkspace>.Failed(
                    "Definition '" + target + "' is not recorded.", WorkflowAdminRules.CodeUnknownTarget);
            }

            var previous = "{\"version\":\"" + (definition.VERSION ?? string.Empty) +
                           "\",\"configLength\":" + (definition.PROCESS_CONFIG_JSON?.Length ?? 0) + "}";

            definition.VERSION = version.Number;
            definition.PROCESS_CONFIG_JSON = stored.PROCESS_CONFIG_SNAPSHOT;

            var updated = unit.Update(definition);

            if (!OgUow.IsOk(updated))
            {
                return OgResult<OgAdminWorkspace>.Failed(
                    OgUow.Message(updated, "The version was not activated."), WorkflowAdminRules.CodeWriteFailed);
            }

            var committed = await unit.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(committed))
            {
                return OgResult<OgAdminWorkspace>.Failed(
                    OgUow.Message(committed, "The version was not activated."), WorkflowAdminRules.CodeWriteFailed);
            }

            await AuditAsync(source, "VERSION", target, version.Number, actor, reason, previous,
                "{\"version\":\"" + version.Number + "\"}", cancellationToken).ConfigureAwait(false);

            return null;
        }, reason, confirmation, actor, cancellationToken);

    public Task<OgResult<OgAdminWorkspace>> AddDelegationAsync(
        string delegator,
        string delegateUser,
        DateTime fromUtc,
        DateTime toUtc,
        decimal limit,
        string currency,
        string reason,
        string confirmation,
        string actor,
        CancellationToken cancellationToken = default) =>
        RunAsync(async (source, snapshot) =>
        {
            if (WorkflowAdminRules.ValidateDelegation(delegator, delegateUser, fromUtc, toUtc, limit) is { } refusal)
            {
                return OgResult<OgAdminWorkspace>.Failed(refusal.Message, refusal.Code);
            }

            var unit = OgUow.For<DELEGATION_OF_AUTHORITY>(source);
            unit.PrimaryKey = nameof(DELEGATION_OF_AUTHORITY.DOA_ID);

            var id = Guid.NewGuid().ToString("N");

            unit.Add(new DELEGATION_OF_AUTHORITY
            {
                DOA_ID = id,
                DOA_NAME = delegator.Trim() + " delegates to " + delegateUser.Trim(),
                // The owner's threshold columns describe the authority ladder. This row is a person delegation, so they record
                // that explicitly rather than being left blank.
                ENTITY_TYPE = "USER",
                FIELD_NAME = "NONE",
                COMPARISON_OPERATOR = "EQ",
                THRESHOLD_VALUE = limit,
                THRESHOLD_VALUE_MAX = limit,
                CURRENCY_CODE = string.IsNullOrWhiteSpace(currency) ? null : currency.Trim().ToUpperInvariant(),
                APPROVAL_LEVEL = "DELEGATE",
                REQUIRED_ROLE = "DELEGATE",
                APPROVAL_SEQUENCE = 0,
                REQUIRES_UNANIMOUS = "N",
                PROCESS_TYPE = "ALL",
                NOTES = reason.Trim(),
                DELEGATOR_USER_ID = delegator.Trim(),
                DELEGATE_USER_ID = delegateUser.Trim(),
                VALID_FROM_UTC = fromUtc,
                VALID_TO_UTC = toUtc,
                IS_ACTIVE = "Y",
            });

            var committed = await unit.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(committed))
            {
                return OgResult<OgAdminWorkspace>.Failed(
                    OgUow.Message(committed, "The delegation was not saved."), WorkflowAdminRules.CodeWriteFailed);
            }

            await AuditAsync(source, "DELEGATION", id, delegateUser.Trim(), actor, reason, null,
                "{\"delegator\":\"" + delegator.Trim() + "\",\"delegate\":\"" + delegateUser.Trim() + "\"}",
                cancellationToken).ConfigureAwait(false);

            _ = snapshot;

            return null;
        }, reason, confirmation, actor, cancellationToken);

    // ------------------------------------------------------------------ the write helpers

    private async Task<OgResult<OgAdminWorkspace>> RunAsync(
        Func<IDataSource, OgAdminWorkspace, Task<OgResult<OgAdminWorkspace>?>> body,
        string reason,
        string confirmation,
        string actor,
        CancellationToken cancellationToken)
    {
        // AC-2 and the state table: a change needs its actor, its reason and the confirmation that names what it affects.
        if (WorkflowAdminRules.ValidateChange(actor, reason, confirmation) is { } refusal)
        {
            return OgResult<OgAdminWorkspace>.Failed(refusal.Message, refusal.Code);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgAdminWorkspace>.Failed(
                "No connection is open, so the configuration cannot be changed.",
                WorkflowAdminRules.CodeNoConnection);
        }

        try
        {
            var read = await ReadAsync(source, DateTime.UtcNow, cancellationToken).ConfigureAwait(false);

            if (!read.Success || read.Value is null)
            {
                return read;
            }

            var outcome = await body(source, read.Value).ConfigureAwait(false);

            if (outcome is not null)
            {
                return outcome;
            }

            return await ReadAsync(source, DateTime.UtcNow, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, "changing the workflow configuration", "the change was not saved", FailureSeverity.Error);

            return OgResult<OgAdminWorkspace>.Failed(
                "The change could not be saved" +
                (string.IsNullOrWhiteSpace(reference) ? "." : " (reference " + reference + ")."),
                reference ?? WorkflowAdminRules.CodeWriteFailed);
        }
    }

    /// <summary>AC-2: actor, reason and the value it replaced, kept where the change can be read back.</summary>
    private async Task AuditAsync(
        IDataSource source,
        string area,
        string targetId,
        string? detail,
        string actor,
        string reason,
        string? previous,
        string? newValue,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        var unit = OgUow.For<WORKFLOW_CONFIGURATION_CHANGE>(source);
        unit.PrimaryKey = nameof(WORKFLOW_CONFIGURATION_CHANGE.WORKFLOW_CONFIGURATION_CHANGE_ID);

        unit.Add(new WORKFLOW_CONFIGURATION_CHANGE
        {
            WORKFLOW_CONFIGURATION_CHANGE_ID = Guid.NewGuid().ToString("N"),
            AREA = area.ToUpperInvariant(),
            TARGET_ID = targetId,
            TARGET_DETAIL = detail,
            ACTOR = actor.Trim(),
            REASON = reason.Trim(),
            PREVIOUS_VALUE = previous,
            NEW_VALUE = newValue,
            CHANGED_AT_UTC = now,
            ACTIVE_IND = "Y",
            ROW_CREATED_BY = actor.Trim(),
            ROW_CREATED_DATE = now,
            ROW_CHANGED_BY = actor.Trim(),
            ROW_CHANGED_DATE = now,
        });

        var committed = await unit.Commit().ConfigureAwait(false);

        if (!OgUow.IsOk(committed))
        {
            _failures.ReportHandled(
                new InvalidOperationException(OgUow.Message(committed, "The change record was not written.")),
                "recording the configuration change to " + targetId,
                "the change stands; its actor, reason and previous value are not recorded",
                FailureSeverity.Degraded);
        }

        cancellationToken.ThrowIfCancellationRequested();
    }

    // ------------------------------------------------------------------ the read model

    private async Task<OgResult<OgAdminWorkspace>> ReadAsync(
        IDataSource source,
        DateTime asOf,
        CancellationToken cancellationToken)
    {
        var versions = await OgUow.ReadAllAsync<WORKFLOW_VERSION>(source).ConfigureAwait(false);
        var definitions = await OgUow.ReadAllAsync<PROCESS_DEFINITION>(source).ConfigureAwait(false);
        var dependencies = await OgUow.ReadAllAsync<WORKFLOW_DEPENDENCY>(source).ConfigureAwait(false);
        var triggers = await OgUow.ReadAllAsync<BUSINESS_EVENT_TRIGGER>(source).ConfigureAwait(false);
        var sodRules = await OgUow.ReadAllAsync<SOD_RULE>(source).ConfigureAwait(false);
        var delegations = await OgUow.ReadAllAsync<DELEGATION_OF_AUTHORITY>(source).ConfigureAwait(false);
        var controls = await OgUow.ReadAllAsync<COMPENSATING_CONTROL>(source).ConfigureAwait(false);
        var changes = await OgUow.ReadAllAsync<WORKFLOW_CONFIGURATION_CHANGE>(source).ConfigureAwait(false);
        var instances = await OgUow.ReadAllAsync<PROCESS_INSTANCE>(source).ConfigureAwait(false);
        var steps = await OgUow.ReadAllAsync<PROCESS_STEP_INSTANCE>(source).ConfigureAwait(false);
        var handoffs = await OgUow.ReadAllAsync<ROLE_HANDOFF_CONTRACT>(source).ConfigureAwait(false);

        cancellationToken.ThrowIfCancellationRequested();

        var names = definitions
            .GroupBy(row => row.PROCESS_DEFINITION_ID, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key,
                group => string.IsNullOrWhiteSpace(group.First().PROCESS_NAME) ? group.Key : group.First().PROCESS_NAME!,
                StringComparer.OrdinalIgnoreCase);

        var live = definitions
            .GroupBy(row => row.PROCESS_DEFINITION_ID, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.First().VERSION ?? string.Empty,
                StringComparer.OrdinalIgnoreCase);

        var versionRows = versions
            .OrderBy(row => row.PROCESS_DEFINITION_ID, StringComparer.Ordinal)
            .ThenBy(row => row.VERSION_NUMBER, StringComparer.Ordinal)
            .Select(row => new OgVersionRow(
                row.VERSION_ID,
                row.PROCESS_DEFINITION_ID ?? string.Empty,
                names.TryGetValue(row.PROCESS_DEFINITION_ID ?? string.Empty, out var name)
                    ? name
                    : row.PROCESS_DEFINITION_ID ?? string.Empty,
                row.VERSION_NUMBER ?? string.Empty,
                row.CHANGE_DESCRIPTION ?? string.Empty,
                !string.IsNullOrWhiteSpace(row.PROCESS_CONFIG_SNAPSHOT),
                live.TryGetValue(row.PROCESS_DEFINITION_ID ?? string.Empty, out var number) &&
                string.Equals(number, row.VERSION_NUMBER, StringComparison.Ordinal)))
            .ToList();

        var dependencyRows = dependencies
            .Select(row => new OgDependencyRow(
                row.DEPENDENCY_ID,
                row.DEPENDENT_PROCESS_DEF_ID,
                row.PREREQUISITE_PROCESS_DEF_ID,
                row.DEPENDENCY_TYPE ?? string.Empty,
                row.CONDITION_EXPRESSION))
            .OrderBy(row => row.From, StringComparer.Ordinal)
            .ThenBy(row => row.To, StringComparer.Ordinal)
            .ToList();

        var triggerRows = triggers
            .Select(row => new OgTriggerRow(
                row.TRIGGER_ID,
                row.TRIGGER_NAME,
                row.EVENT_TYPE,
                row.ENTITY_TYPE,
                row.TARGET_PROCESS_DEF_ID,
                string.Equals(row.IS_ACTIVE?.Trim(), "Y", StringComparison.OrdinalIgnoreCase),
                row.PRIORITY,
                row.DESCRIPTION))
            .OrderByDescending(row => row.Priority)
            .ThenBy(row => row.Name, StringComparer.Ordinal)
            .ToList();

        var sodRows = sodRules
            .Select(row => new OgSodRuleRow(
                row.SOD_RULE_ID,
                row.RULE_NAME,
                row.CONFLICTING_PERMISSION_A,
                row.CONFLICTING_PERMISSION_B,
                row.SEVERITY ?? string.Empty,
                string.Equals(row.IS_BLOCKING?.Trim(), "Y", StringComparison.OrdinalIgnoreCase),
                !string.Equals(row.ACTIVE_IND?.Trim(), "N", StringComparison.OrdinalIgnoreCase),
                row.SCOPE_TYPE ?? string.Empty,
                row.CONFLICT_DESCRIPTION,
                row.MITIGATION_GUIDANCE))
            .OrderBy(row => row.Name, StringComparer.Ordinal)
            .ToList();

        var delegationRows = delegations
            .Select(row =>
            {
                var (active, state) = WorkflowAdminRules.DelegationState(
                    row.IS_ACTIVE, row.VALID_FROM_UTC, row.VALID_TO_UTC, asOf);

                return new OgDelegationRow(
                    row.DOA_ID,
                    row.DOA_NAME,
                    row.DELEGATOR_USER_ID,
                    row.DELEGATE_USER_ID,
                    row.VALID_FROM_UTC,
                    row.VALID_TO_UTC,
                    row.THRESHOLD_VALUE,
                    row.CURRENCY_CODE,
                    active,
                    state);
            })
            .OrderBy(row => row.Name, StringComparer.Ordinal)
            .ToList();

        var controlRows = controls
            .Select(row => new OgCompensatingControlRow(
                row.CONTROL_ID,
                row.SOD_CONFLICT_ID,
                row.USER_ID,
                row.CONTROL_TYPE,
                row.CONTROL_DESCRIPTION,
                row.STATUS,
                row.EFFECTIVE_FROM,
                row.EFFECTIVE_TO))
            .OrderBy(row => row.UserId, StringComparer.Ordinal)
            .ToList();

        // AC-5: what is orphaned or broken, with the cause where it is known.
        var diagnostics = new List<OgAdminDiagnostic>();

        foreach (var instance in instances.Where(row => row.COMPLETED_DATE is null))
        {
            if (string.IsNullOrWhiteSpace(instance.PROCESS_DEFINITION_ID) ||
                !names.ContainsKey(instance.PROCESS_DEFINITION_ID!))
            {
                diagnostics.Add(new OgAdminDiagnostic(
                    "orphaned instance",
                    instance.PROCESS_INSTANCE_ID,
                    "no process definition named '" + (instance.PROCESS_DEFINITION_ID ?? "(none)") + "' is recorded"));
                continue;
            }

            if (!steps.Any(step => string.Equals(step.PROCESS_INSTANCE_ID, instance.PROCESS_INSTANCE_ID,
                    StringComparison.OrdinalIgnoreCase)))
            {
                diagnostics.Add(new OgAdminDiagnostic(
                    "instance without steps",
                    instance.PROCESS_INSTANCE_ID,
                    "no step instance is recorded, so its state cannot advance"));
            }
        }

        var stepKeys = steps
            .GroupBy(row => row.PROCESS_INSTANCE_ID, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key,
                group => group.Select(row => row.STEP_ID).ToHashSet(StringComparer.OrdinalIgnoreCase),
                StringComparer.OrdinalIgnoreCase);

        foreach (var handoff in handoffs)
        {
            var key = instances
                .FirstOrDefault(instance => string.Equals(instance.PROCESS_DEFINITION_ID, handoff.PROCESS_DEFINITION_ID,
                    StringComparison.OrdinalIgnoreCase))?.PROCESS_INSTANCE_ID;

            if (key is null || !stepKeys.TryGetValue(key, out var known))
            {
                continue;
            }

            if (!known.Contains(handoff.FROM_STEP_ID) || !known.Contains(handoff.TO_STEP_ID))
            {
                diagnostics.Add(new OgAdminDiagnostic(
                    "broken handoff",
                    handoff.HANDOFF_CONTRACT_ID,
                    "it names a step this instance does not record"));
            }
        }

        var changeRows = changes
            .OrderByDescending(row => row.CHANGED_AT_UTC)
            .Select(row => new OgConfigChange(
                row.AREA,
                row.TARGET_ID,
                row.TARGET_DETAIL,
                row.ACTOR,
                row.REASON,
                row.PREVIOUS_VALUE,
                row.NEW_VALUE,
                row.CHANGED_AT_UTC))
            .ToList();

        return OgResult<OgAdminWorkspace>.Ok(new OgAdminWorkspace(
            versionRows,
            dependencyRows,
            triggerRows,
            sodRows,
            delegationRows,
            controlRows,
            diagnostics,
            changeRows,
            asOf,
            DateTime.UtcNow));
    }
}
