using System.Globalization;
using System.Text.Json;
using Beep.OilandGas.LifeCycle.Data.Tables;
using Beep.OilandGas.Models.Processes;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Data.Workflow;

/// <summary>One step of a process definition, as the definition records it (WFL-02's <c>OgProcessStep</c>).</summary>
/// <param name="Decisions">The ways out of the step: its next step, its conditional next steps and, for an approval, the decision itself.</param>
public sealed record OgProcessStep(
    string Key,
    string Name,
    string RequiresRole,
    IReadOnlyList<string> Decisions,
    IReadOnlyList<string> NextStepKeys,
    int Sequence,
    int? SlaHours,
    bool RequiresApproval,
    bool IsRequired)
{
    public string Describe() =>
        Sequence.ToString(CultureInfo.InvariantCulture) + ". " + Name +
        (string.IsNullOrWhiteSpace(RequiresRole) ? " · no role recorded" : " · " + RequiresRole) +
        (RequiresApproval ? " · approval" : string.Empty) +
        (SlaHours is { } sla ? " · " + sla.ToString(CultureInfo.InvariantCulture) + "h SLA" : " · no SLA") +
        (NextStepKeys.Count == 0 ? " · terminal" : " → " + string.Join(", ", NextStepKeys));
}

/// <summary>A dependency between two definitions (WFL-02, and a cycle when it closes).</summary>
public sealed record OgWorkflowDependency(
    string DependentDefinitionId,
    string? DependentStepId,
    string PrerequisiteDefinitionId,
    string? PrerequisiteStepId,
    string Type,
    string? Condition)
{
    public string Describe() =>
        DependentDefinitionId + (string.IsNullOrWhiteSpace(DependentStepId) ? string.Empty : "/" + DependentStepId) +
        " " + Type.ToLowerInvariant() + " " + PrerequisiteDefinitionId +
        (string.IsNullOrWhiteSpace(PrerequisiteStepId) ? string.Empty : "/" + PrerequisiteStepId) +
        (string.IsNullOrWhiteSpace(Condition) ? string.Empty : " when " + Condition);
}

/// <summary>One recorded version of a definition (WFL-02, AC-3).</summary>
public sealed record OgWorkflowVersion(
    string VersionId,
    string Number,
    string Change,
    string? PreviousVersionId,
    string? CreatedBy,
    bool HasSnapshot,
    IReadOnlyList<string> AddedSteps,
    IReadOnlyList<string> RemovedSteps)
{
    public bool Superseded => PreviousVersionId is not null;

    public string DifferenceText => !HasSnapshot
        ? "no configuration snapshot is recorded, so only the stated change is known"
        : AddedSteps.Count == 0 && RemovedSteps.Count == 0
            ? "no step was added or removed against the previous version"
            : (AddedSteps.Count == 0 ? string.Empty : "added " + string.Join(", ", AddedSteps)) +
              (AddedSteps.Count > 0 && RemovedSteps.Count > 0 ? "; " : string.Empty) +
              (RemovedSteps.Count == 0 ? string.Empty : "removed " + string.Join(", ", RemovedSteps));

    public string Describe() =>
        "version " + Number + " · " + (Superseded ? "superseded (read-only)" : "current") +
        (string.IsNullOrWhiteSpace(Change) ? " · no change described" : " · " + Change) +
        " · " + DifferenceText;
}

/// <summary>A definition with its ordered steps (WFL-02's <c>OgProcessDefinition</c>).</summary>
public sealed record OgProcessDefinitionView(
    string Id,
    string Name,
    string Category,
    string Version,
    bool Active,
    IReadOnlyList<OgProcessStep> Steps,
    int Instances,
    string? InvalidBecause)
{
    public bool IsValid => InvalidBecause is null && Steps.Count > 0;

    /// <summary>AC-6: the graph's nodes are exactly the step list.</summary>
    public IReadOnlyList<string> GraphNodes => Steps.Select(step => step.Key).ToList();

    /// <summary>AC-6: the graph's edges are exactly the transitions the steps record.</summary>
    public IReadOnlyList<(string From, string To)> GraphEdges => Steps
        .SelectMany(step => step.NextStepKeys.Select(next => (From: step.Key, To: next)))
        .ToList();

    public string StateText => !IsValid
        ? "invalid: " + (InvalidBecause ?? "the definition records no step")
        : Active
            ? "active"
            : "not active";

    public string Describe() =>
        Name + " · " + Category + " · version " + (string.IsNullOrWhiteSpace(Version) ? "(none)" : Version) +
        " · " + Steps.Count.ToString(CultureInfo.InvariantCulture) + " step(s) · " +
        Instances.ToString(CultureInfo.InvariantCulture) + " instance(s) · " + StateText;

    public string StepsText => Steps.Count == 0
        ? "No step is recorded, so the definition is invalid."
        : string.Join(Environment.NewLine, Steps.Select(step => step.Describe()));

    public string GraphText
    {
        get
        {
            var nodes = string.Join(", ", GraphNodes);

            var edges = GraphEdges.Count == 0
                ? "no transition is recorded"
                : string.Join(", ", GraphEdges.Select(edge => edge.From + " → " + edge.To));

            return "nodes: " + (string.IsNullOrWhiteSpace(nodes) ? "(none)" : nodes) + Environment.NewLine +
                   "edges: " + edges;
        }
    }
}

/// <summary>A cycle found in the definition graph or between definitions (WFL-02, AC-2).</summary>
public sealed record OgDefinitionCycle(string Kind, IReadOnlyList<string> Path)
{
    public string Describe() => Kind + " cycle: " + string.Join(" → ", Path);
}

/// <summary>The definition catalogue for one field and one selected definition (WFL-02).</summary>
public sealed record OgDefinitionCatalogue(
    string? FieldId,
    IReadOnlyList<OgProcessDefinitionView> Definitions,
    OgProcessDefinitionView? Selected,
    IReadOnlyList<OgWorkflowVersion> Versions,
    IReadOnlyList<OgWorkflowDependency> Dependencies,
    IReadOnlyList<OgDefinitionCycle> Cycles,
    DateTime ReadAt)
{
    public int Invalid => Definitions.Count(definition => !definition.IsValid);

    public int TotalSteps => Definitions.Sum(definition => definition.Steps.Count);

    /// <summary>AC-5: the same instance counts WFL-01 reports, per definition.</summary>
    public int Instances => Definitions.Sum(definition => definition.Instances);

    public string CountsText =>
        Definitions.Count.ToString(CultureInfo.InvariantCulture) + " definition(s) · " +
        TotalSteps.ToString(CultureInfo.InvariantCulture) + " step(s) · " +
        Instances.ToString(CultureInfo.InvariantCulture) + " instance(s) · " +
        Invalid.ToString(CultureInfo.InvariantCulture) + " invalid · " +
        Cycles.Count.ToString(CultureInfo.InvariantCulture) + " cycle(s)";

    public string ListText => Definitions.Count == 0
        ? "No process definition is recorded."
        : string.Join(Environment.NewLine, Definitions.Select(definition => definition.Describe()));

    public string VersionsText => Versions.Count == 0
        ? "No version is recorded for this definition."
        : string.Join(Environment.NewLine, Versions.Select(version => version.Describe()));

    public string DependenciesText => Dependencies.Count == 0
        ? "No dependency is recorded."
        : string.Join(Environment.NewLine, Dependencies.Select(dependency => dependency.Describe()));

    public string CyclesText => Cycles.Count == 0
        ? "No cycle is recorded in the steps or the dependencies."
        : string.Join(Environment.NewLine, Cycles.Select(cycle => cycle.Describe()));

    /// <summary>
    /// The feature's own open question, stated in the UI: definitions are code/config, and this build edits neither.
    /// </summary>
    public const string ReadOnlyNotice =
        "Definitions are code and configuration in their owning project, so this screen is read-only: it shows what a " +
        "definition does, and editing it is workflow administration rather than a viewer. No definition is written from here.";

    public string ReadText =>
        "read at " + ReadAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
}

/// <summary>The definition-catalogue rules WFL-02 states once.</summary>
public static class DefinitionCatalogueRules
{
    public const string CodeNoConnection = "definitions-no-connection";
    public const string CodeReadFailed = "definitions-read-failed";

    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    /// <summary>
    /// Parses a definition's configuration with the owner's own contract (<c>ProcessDefinition</c>), so the catalogue and the
    /// engine cannot disagree about what a step is.
    /// </summary>
    public static (ProcessDefinition? Definition, string? Error) ParseConfig(
        string? json,
        IFailureReporter failures,
        string operation)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return (null, "the definition records no configuration, so it has no steps");
        }

        try
        {
            var parsed = JsonSerializer.Deserialize<ProcessDefinition>(json, Options);

            return parsed is null
                ? (null, "the recorded configuration is empty")
                : (parsed, null);
        }
        catch (JsonException ex)
        {
            // A malformed configuration is a data defect this screen reports and shows as the definition's invalid reason,
            // not an exception for the page (BEEP0003: report it, with what the person sees).
            var reference = failures.ReportHandled(
                ex, operation,
                "the definition is shown as invalid rather than being dropped from the catalogue",
                FailureSeverity.Degraded);

            return (null, "the recorded configuration is not a process definition" +
                          (string.IsNullOrWhiteSpace(reference) ? string.Empty : " (reference " + reference + ")"));
        }
    }

    /// <summary>AC-1: the steps in sequence order, each naming its required role.</summary>
    public static IReadOnlyList<OgProcessStep> OrderSteps(ProcessDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);

        return definition.Steps
            .OrderBy(step => step.SequenceNumber)
            .ThenBy(step => step.StepId, StringComparer.Ordinal)
            .Select(step =>
            {
                var next = new List<string>();

                if (!string.IsNullOrWhiteSpace(step.NextStepId))
                {
                    next.Add(step.NextStepId.Trim());
                }

                next.AddRange(step.ConditionalNextSteps
                    .Where(id => !string.IsNullOrWhiteSpace(id))
                    .Select(id => id.Trim()));

                var decisions = new List<string>();

                if (!string.IsNullOrWhiteSpace(step.StepType))
                {
                    decisions.Add(step.StepType.Trim().ToUpperInvariant());
                }

                if (step.RequiresApproval)
                {
                    decisions.Add("APPROVE");
                    decisions.Add("RETURN");
                }

                decisions.AddRange(next.Select(id => "NEXT:" + id));

                return new OgProcessStep(
                    step.StepId,
                    string.IsNullOrWhiteSpace(step.StepName) ? step.StepId : step.StepName,
                    string.Join(", ", step.RequiredRoles.Where(role => !string.IsNullOrWhiteSpace(role))),
                    decisions,
                    next.Distinct(StringComparer.Ordinal).ToList(),
                    step.SequenceNumber,
                    step.SlaHours,
                    step.RequiresApproval,
                    step.IsRequired);
            })
            .ToList();
    }

    /// <summary>
    /// AC-2: the cycles the step graph records. A step that points back to an earlier one is shown rather than hidden, and the
    /// path is named so the reader can see exactly where it closes.
    /// </summary>
    public static IReadOnlyList<OgDefinitionCycle> StepCycles(IReadOnlyList<OgProcessStep> steps)
    {
        ArgumentNullException.ThrowIfNull(steps);

        var edges = steps.ToDictionary(
            step => step.Key,
            step => step.NextStepKeys,
            StringComparer.Ordinal);

        var cycles = new List<OgDefinitionCycle>();
        var finished = new HashSet<string>(StringComparer.Ordinal);
        var path = new List<string>();
        var onPath = new HashSet<string>(StringComparer.Ordinal);

        void Walk(string key)
        {
            if (!edges.ContainsKey(key) || finished.Contains(key))
            {
                return;
            }

            if (!onPath.Add(key))
            {
                var start = path.IndexOf(key);
                cycles.Add(new OgDefinitionCycle("step", [.. path.Skip(start), key]));
                return;
            }

            path.Add(key);

            foreach (var next in edges[key])
            {
                Walk(next);
            }

            path.RemoveAt(path.Count - 1);
            onPath.Remove(key);
            finished.Add(key);
        }

        foreach (var key in edges.Keys.OrderBy(key => key, StringComparer.Ordinal))
        {
            Walk(key);
        }

        return cycles;
    }

    /// <summary>AC-2 across definitions: a dependency that closes on itself is a cycle, not an ordering.</summary>
    public static IReadOnlyList<OgDefinitionCycle> DependencyCycles(IReadOnlyList<OgWorkflowDependency> dependencies)
    {
        ArgumentNullException.ThrowIfNull(dependencies);

        var edges = dependencies
            .Where(dependency => !string.IsNullOrWhiteSpace(dependency.DependentDefinitionId) &&
                                 !string.IsNullOrWhiteSpace(dependency.PrerequisiteDefinitionId))
            .GroupBy(dependency => dependency.DependentDefinitionId, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                group => group.Key,
                group => group.Select(dependency => dependency.PrerequisiteDefinitionId)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList(),
                StringComparer.OrdinalIgnoreCase);

        var cycles = new List<OgDefinitionCycle>();
        var finished = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var path = new List<string>();

        void Walk(string key)
        {
            if (!edges.TryGetValue(key, out var next) || finished.Contains(key))
            {
                return;
            }

            var index = path.FindIndex(candidate => string.Equals(candidate, key, StringComparison.OrdinalIgnoreCase));

            if (index >= 0)
            {
                cycles.Add(new OgDefinitionCycle("dependency", [.. path.Skip(index), key]));
                return;
            }

            path.Add(key);

            foreach (var target in next)
            {
                Walk(target);
            }

            path.RemoveAt(path.Count - 1);
            finished.Add(key);
        }

        foreach (var key in edges.Keys.OrderBy(key => key, StringComparer.OrdinalIgnoreCase))
        {
            Walk(key);
        }

        return cycles;
    }

    /// <summary>AC-3: the steps a version added and removed, from its own snapshot against the one before it.</summary>
    public static (IReadOnlyList<string> Added, IReadOnlyList<string> Removed) Difference(
        string? snapshotJson,
        string? previousSnapshotJson,
        IFailureReporter failures,
        string operation)
    {
        var current = Keys(snapshotJson, failures, operation);
        var previous = Keys(previousSnapshotJson, failures, operation);

        if (current is null || previous is null)
        {
            return ([], []);
        }

        return (current.Except(previous, StringComparer.Ordinal).OrderBy(key => key, StringComparer.Ordinal).ToList(),
                previous.Except(current, StringComparer.Ordinal).OrderBy(key => key, StringComparer.Ordinal).ToList());
    }

    private static List<string>? Keys(string? json, IFailureReporter failures, string operation)
    {
        var (definition, error) = ParseConfig(json, failures, operation);

        return definition is null
            ? error is null ? [] : null
            : definition.Steps.Select(step => step.StepId).Where(id => !string.IsNullOrWhiteSpace(id)).ToList();
    }
}

/// <summary>WFL-02: the definition catalogue, its ordered steps, its versions and its cycles.</summary>
public interface IOgDefinitionCatalogue
{
    /// <param name="definitionId">The definition whose detail is opened; the first one when omitted.</param>
    Task<OgResult<OgDefinitionCatalogue>> LoadAsync(
        string? fieldId,
        string? definitionId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// WFL-02 over the owner's <c>PROCESS_DEFINITION</c> (its <c>PROCESS_CONFIG_JSON</c> read through the owner's own
/// <c>ProcessDefinition</c> contract), <c>WORKFLOW_VERSION</c>, <c>WORKFLOW_DEPENDENCY</c> and <c>PROCESS_INSTANCE</c>.
/// </summary>
/// <remarks>
/// <para>
/// The catalogue never writes: definitions are code and configuration in their owning project, which the screen states. The
/// instance counts are taken with the same rule WFL-01 uses, so the two screens cannot disagree (AC-5), and the graph is derived
/// from the step list itself, so it cannot show an edge the steps do not record (AC-6).
/// </para>
/// <para>
/// A definition whose configuration does not parse, or that records no step, is shown as invalid with the reason rather than
/// being dropped from the list - that is the state a person needs when a document is stuck.
/// </para>
/// </remarks>
public sealed class OgDefinitionCatalogueService : IOgDefinitionCatalogue
{
    private readonly Func<IDataSource?> _source;
    private readonly IFailureReporter _failures;

    public OgDefinitionCatalogueService(Func<IDataSource?> source, IFailureReporter failures)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _failures = failures ?? throw new ArgumentNullException(nameof(failures));
    }

    public async Task<OgResult<OgDefinitionCatalogue>> LoadAsync(
        string? fieldId,
        string? definitionId,
        CancellationToken cancellationToken = default)
    {
        var source = _source();

        if (source is null)
        {
            return OgResult<OgDefinitionCatalogue>.Failed(
                "No connection is open, so the definition catalogue cannot be read.",
                DefinitionCatalogueRules.CodeNoConnection);
        }

        try
        {
            var definitions = await OgUow.ReadAllAsync<PROCESS_DEFINITION>(source).ConfigureAwait(false);
            var versions = await OgUow.ReadAllAsync<WORKFLOW_VERSION>(source).ConfigureAwait(false);
            var dependencies = await OgUow.ReadAllAsync<WORKFLOW_DEPENDENCY>(source).ConfigureAwait(false);
            var instances = await OgUow.ReadAllAsync<PROCESS_INSTANCE>(source).ConfigureAwait(false);

            cancellationToken.ThrowIfCancellationRequested();

            // The same scope WFL-01 applies, so the instance counts agree.
            var scoped = instances
                .Where(row => string.IsNullOrWhiteSpace(fieldId) ||
                              string.Equals(row.FIELD_ID?.Trim(), fieldId.Trim(), StringComparison.OrdinalIgnoreCase))
                .ToList();

            var instanceCounts = scoped
                .GroupBy(row => row.PROCESS_DEFINITION_ID ?? string.Empty, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(group => group.Key,
                    group => group.Select(row => row.PROCESS_INSTANCE_ID).Distinct(StringComparer.OrdinalIgnoreCase).Count(),
                    StringComparer.OrdinalIgnoreCase);

            var views = definitions
                .OrderBy(row => row.PROCESS_NAME, StringComparer.Ordinal)
                .ThenBy(row => row.PROCESS_DEFINITION_ID, StringComparer.Ordinal)
                .Select(row =>
                {
                    var (parsed, error) = DefinitionCatalogueRules.ParseConfig(
                        row.PROCESS_CONFIG_JSON, _failures, "reading definition " + row.PROCESS_DEFINITION_ID);

                    var steps = parsed is null
                        ? []
                        : DefinitionCatalogueRules.OrderSteps(parsed);

                    var because = error ?? (steps.Count == 0 ? "the definition records no step" : null);

                    return new OgProcessDefinitionView(
                        row.PROCESS_DEFINITION_ID,
                        string.IsNullOrWhiteSpace(row.PROCESS_NAME) ? row.PROCESS_DEFINITION_ID : row.PROCESS_NAME!,
                        row.PROCESS_TYPE ?? string.Empty,
                        row.VERSION ?? string.Empty,
                        !string.Equals(row.IS_ACTIVE?.Trim(), "N", StringComparison.OrdinalIgnoreCase),
                        steps,
                        instanceCounts.TryGetValue(row.PROCESS_DEFINITION_ID, out var count) ? count : 0,
                        because);
                })
                .ToList();

            var selected = string.IsNullOrWhiteSpace(definitionId)
                ? views.FirstOrDefault()
                : views.FirstOrDefault(view =>
                    string.Equals(view.Id, definitionId.Trim(), StringComparison.OrdinalIgnoreCase));

            var catalogue = Build(views, selected, versions, dependencies, fieldId);

            return OgResult<OgDefinitionCatalogue>.Ok(catalogue);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, "reading the definition catalogue", "the catalogue is not shown", FailureSeverity.Error);

            return OgResult<OgDefinitionCatalogue>.Failed(
                "The definition catalogue could not be read" +
                (string.IsNullOrWhiteSpace(reference) ? "." : " (reference " + reference + ")."),
                reference ?? DefinitionCatalogueRules.CodeReadFailed);
        }
    }

    private OgDefinitionCatalogue Build(
        IReadOnlyList<OgProcessDefinitionView> views,
        OgProcessDefinitionView? selected,
        IReadOnlyList<WORKFLOW_VERSION> versions,
        IReadOnlyList<WORKFLOW_DEPENDENCY> dependencies,
        string? fieldId)
    {
        var mine = selected is null
            ? []
            : versions
                .Where(row => string.Equals(row.PROCESS_DEFINITION_ID, selected.Id, StringComparison.OrdinalIgnoreCase))
                .OrderBy(row => row.VERSION_NUMBER, StringComparer.Ordinal)
                .ToList();

        var versionViews = new List<OgWorkflowVersion>();

        for (var index = 0; index < mine.Count; index++)
        {
            var row = mine[index];

            var previous = mine.FirstOrDefault(candidate =>
                string.Equals(candidate.VERSION_ID, row.PREVIOUS_VERSION_ID, StringComparison.OrdinalIgnoreCase));

            var (added, removed) = DefinitionCatalogueRules.Difference(
                row.PROCESS_CONFIG_SNAPSHOT,
                previous?.PROCESS_CONFIG_SNAPSHOT,
                _failures,
                "comparing version " + row.VERSION_NUMBER + " of " + row.PROCESS_DEFINITION_ID);

            versionViews.Add(new OgWorkflowVersion(
                row.VERSION_ID,
                row.VERSION_NUMBER ?? string.Empty,
                row.CHANGE_DESCRIPTION ?? string.Empty,
                row.PREVIOUS_VERSION_ID,
                row.CREATED_BY,
                !string.IsNullOrWhiteSpace(row.PROCESS_CONFIG_SNAPSHOT),
                added,
                removed));
        }

        var dependencyViews = dependencies
            .Where(row => selected is null ||
                          string.Equals(row.DEPENDENT_PROCESS_DEF_ID, selected.Id, StringComparison.OrdinalIgnoreCase) ||
                          string.Equals(row.PREREQUISITE_PROCESS_DEF_ID, selected.Id, StringComparison.OrdinalIgnoreCase))
            .Select(row => new OgWorkflowDependency(
                row.DEPENDENT_PROCESS_DEF_ID,
                row.DEPENDENT_STEP_ID,
                row.PREREQUISITE_PROCESS_DEF_ID,
                row.PREREQUISITE_STEP_ID,
                row.DEPENDENCY_TYPE ?? string.Empty,
                row.CONDITION_EXPRESSION))
            .ToList();

        var cycles = new List<OgDefinitionCycle>();

        if (selected is not null)
        {
            cycles.AddRange(DefinitionCatalogueRules.StepCycles(selected.Steps));
        }

        cycles.AddRange(DefinitionCatalogueRules.DependencyCycles(dependencyViews));

        return new OgDefinitionCatalogue(
            string.IsNullOrWhiteSpace(fieldId) ? null : fieldId.Trim(),
            views,
            selected,
            versionViews,
            dependencyViews,
            cycles,
            DateTime.UtcNow);
    }
}
