using System.Text.Json;
using Beep.OilandGas.Models.Processes;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Workflow;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// WFL-02: the ordered step list with its roles, the cycle detection, the version differences, the read-only guarantee and the
/// graph's agreement with the step list - the parts that need no database. The read over real rows is proven in the gateway test.
/// </summary>
public sealed class DefinitionCatalogueTests
{
    private const string PageFileName = "DefinitionsPageView.cs";
    private const string ServiceFileName = "OgDefinitionCatalogue.cs";

    [Fact]
    public void Steps_are_ordered_and_each_names_its_required_role()
    {
        // AC-1: the order comes from the recorded sequence, not from the file's order.
        var (definition, error) = DefinitionCatalogueRules.ParseConfig(
            Config(
                Step("S-3", "Approve", 3, roles: ["ReservoirEngineer"], next: ""),
                Step("S-1", "Submit", 1, roles: ["DrillingEngineer"], next: "S-2"),
                Step("S-2", "Review", 2, roles: ["PeerReviewer", "TeamLead"], next: "S-3")),
            new Quiet(),
            "test");

        Assert.Null(error);
        Assert.NotNull(definition);

        var steps = DefinitionCatalogueRules.OrderSteps(definition!);

        Assert.Equal(["S-1", "S-2", "S-3"], steps.Select(step => step.Key));
        Assert.Equal("DrillingEngineer", steps[0].RequiresRole);
        Assert.Equal("PeerReviewer, TeamLead", steps[1].RequiresRole);
        Assert.Empty(steps[2].NextStepKeys);

        // A step with no role says so rather than showing an empty cell as though a role had been recorded.
        Assert.Contains("no role recorded", (steps[2] with { RequiresRole = string.Empty }).Describe(),
            StringComparison.Ordinal);
    }

    [Fact]
    public void A_cycle_in_the_steps_is_shown_with_its_path()
    {
        // AC-2.
        var (definition, _) = DefinitionCatalogueRules.ParseConfig(
            Config(
                Step("S-1", "Submit", 1, next: "S-2"),
                Step("S-2", "Review", 2, next: "S-3"),
                Step("S-3", "Approve", 3, next: "S-2")),
            new Quiet(),
            "test");

        var cycles = DefinitionCatalogueRules.StepCycles(
            DefinitionCatalogueRules.OrderSteps(definition!));

        var cycle = Assert.Single(cycles);

        Assert.Equal("step", cycle.Kind);
        Assert.Equal(["S-2", "S-3", "S-2"], cycle.Path);
        Assert.Contains("S-2 → S-3 → S-2", cycle.Describe(), StringComparison.Ordinal);

        // A straight line records no cycle.
        var (linear, _) = DefinitionCatalogueRules.ParseConfig(
            Config(
                Step("S-1", "Submit", 1, next: "S-2"),
                Step("S-2", "Review", 2, next: "")),
            new Quiet(),
            "test");

        Assert.Empty(DefinitionCatalogueRules.StepCycles(
            DefinitionCatalogueRules.OrderSteps(linear!)));
    }

    [Fact]
    public void A_dependency_cycle_between_definitions_is_shown_too()
    {
        // AC-2 across definitions.
        var dependencies = new List<OgWorkflowDependency>
        {
            new("PD-1", null, "PD-2", null, "REQUIRES", null),
            new("PD-2", null, "PD-1", null, "REQUIRES", null),
        };

        var cycles = DefinitionCatalogueRules.DependencyCycles(dependencies);

        var cycle = Assert.Single(cycles);

        Assert.Equal("dependency", cycle.Kind);
        Assert.Equal(["PD-1", "PD-2", "PD-1"], cycle.Path);

        // A one-way dependency is an ordering, not a cycle.
        Assert.Empty(DefinitionCatalogueRules.DependencyCycles([dependencies[0]]));
    }

    [Fact]
    public void Versions_show_the_steps_they_added_and_removed()
    {
        // AC-3: the difference comes from the two snapshots, not from the description.
        var previous = Config(
            Step("S-1", "Submit", 1, next: "S-2"),
            Step("S-2", "Review", 2, next: ""));

        var current = Config(
            Step("S-1", "Submit", 1, next: "S-2"),
            Step("S-2", "Review", 2, next: "S-3"),
            Step("S-3", "Approve", 3, next: ""));

        var (added, removed) = DefinitionCatalogueRules.Difference(current, previous, new Quiet(), "test");

        Assert.Equal(["S-3"], added);
        Assert.Empty(removed);

        var version = new OgWorkflowVersion("V-2", "2", "Added approval", "V-1", "sara", true, added, removed);

        Assert.Contains("added S-3", version.Describe(), StringComparison.Ordinal);
        Assert.Contains("superseded", version.Describe(), StringComparison.Ordinal);

        var (_, removedOnly) = DefinitionCatalogueRules.Difference(previous, current, new Quiet(), "test");

        Assert.Equal(["S-3"], removedOnly);

        // A version with no snapshot says so rather than claiming no change.
        var bare = new OgWorkflowVersion("V-3", "3", "Retired", "V-2", "sara", false, [], []);

        Assert.Contains("no configuration snapshot", bare.Describe(), StringComparison.Ordinal);
    }

    [Fact]
    public void A_definition_that_cannot_be_read_is_invalid_with_its_reason()
    {
        // The "definition has no steps" state, and a configuration that is not a definition at all.
        var (none, empty) = DefinitionCatalogueRules.ParseConfig(null, new Quiet(), "test");

        Assert.Null(none);
        Assert.Contains("no configuration", empty!, StringComparison.Ordinal);

        var (unparsable, error) = DefinitionCatalogueRules.ParseConfig("{ not json", new Quiet(), "test");

        Assert.Null(unparsable);
        Assert.Contains("not a process definition", error!, StringComparison.Ordinal);

        var view = new OgProcessDefinitionView("PD-1", "Drilling approval", "APPROVAL", "1.0", true, [],
            Instances: 0, InvalidBecause: empty);

        Assert.False(view.IsValid);
        Assert.Contains("invalid", view.StateText, StringComparison.Ordinal);
        Assert.Contains("No step is recorded", view.StepsText, StringComparison.Ordinal);
    }

    [Fact]
    public void The_graph_is_derived_from_the_step_list_so_it_cannot_disagree()
    {
        // AC-6.
        var (definition, _) = DefinitionCatalogueRules.ParseConfig(
            Config(
                Step("S-1", "Submit", 1, next: "S-2"),
                Step("S-2", "Review", 2, next: "S-3"),
                Step("S-3", "Approve", 3, next: "")),
            new Quiet(),
            "test");

        var view = new OgProcessDefinitionView("PD-1", "Drilling approval", "APPROVAL", "1.0", true,
            DefinitionCatalogueRules.OrderSteps(definition!), 4, null);

        Assert.Equal(["S-1", "S-2", "S-3"], view.GraphNodes);
        Assert.Equal(2, view.GraphEdges.Count);
        Assert.Equal(("S-1", "S-2"), view.GraphEdges[0]);
        Assert.Equal(("S-2", "S-3"), view.GraphEdges[1]);
        Assert.Contains("nodes: S-1, S-2, S-3", view.GraphText, StringComparison.Ordinal);
    }

    [Fact]
    public void The_feature_writes_nothing()
    {
        // AC-4: definitions are code and configuration, so the catalogue has no write call at all.
        var service = File.ReadAllText(
            SourceRoot.AllSources().Single(source => source.EndsWith(ServiceFileName, StringComparison.Ordinal)));

        // No unit of work is opened at all, which is what makes every write shape impossible rather than merely absent.
        Assert.DoesNotContain("OgUow.For<", service, StringComparison.Ordinal);
        Assert.DoesNotContain("OgUow.Open", service, StringComparison.Ordinal);
        Assert.DoesNotContain(".Update(", service, StringComparison.Ordinal);
        Assert.DoesNotContain(".Commit(", service, StringComparison.Ordinal);
        Assert.DoesNotContain(".Delete(", service, StringComparison.Ordinal);

        // And the page states it, rather than leaving the reader to discover it.
        var page = File.ReadAllText(
            SourceRoot.AllSources().Single(source => source.EndsWith(PageFileName, StringComparison.Ordinal)));

        Assert.Contains("ReadOnlyNotice", page, StringComparison.Ordinal);
        Assert.Contains("is read-only", OgDefinitionCatalogue.ReadOnlyNotice, StringComparison.Ordinal);
    }

    private static string Config(params ProcessStepDefinition[] steps) =>
        JsonSerializer.Serialize(new ProcessDefinition { Steps = steps.ToList() });

    private static ProcessStepDefinition Step(
        string id,
        string name,
        int sequence,
        string[]? roles = null,
        string next = "") =>
        new()
        {
            StepId = id,
            StepName = name,
            SequenceNumber = sequence,
            StepType = "APPROVAL",
            RequiredRoles = roles?.ToList() ?? [],
            NextStepId = next,
        };

    private sealed class Quiet : IFailureReporter
    {
        public string ReportHandled(
            Exception exception,
            string operation,
            string consequence,
            FailureSeverity severity = FailureSeverity.Error) => FailureReferenceFormat.New();
    }
}
