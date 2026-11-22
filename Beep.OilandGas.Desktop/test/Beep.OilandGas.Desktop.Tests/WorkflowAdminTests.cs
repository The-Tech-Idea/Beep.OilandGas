using TheTechIdea.Beep.OilandGas.Desktop.Data.Workflow;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// WFL-06: the cycle refusal, the mandatory reason and confirmation, the delegation window, the segregation tester and the
/// administrator gate - the parts that need no database. The writes and the diagnostics are proven in the gateway test.
/// </summary>
public sealed class WorkflowAdminTests
{
    private const string PageFileName = "WorkflowAdminPageView.cs";

    private static readonly DateTime AsOf = new(2026, 11, 8, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void A_dependency_that_closes_a_cycle_is_refused_with_its_path()
    {
        // AC-1: the refusal comes before anything is written, and it names the cycle.
        var dependencies = new List<OgDependencyRow>
        {
            new("D-1", "PD-1", "PD-2", "REQUIRES", null),
            new("D-2", "PD-2", "PD-3", "REQUIRES", null),
        };

        var cycle = WorkflowAdminRules.ValidateDependency(dependencies, "PD-3", "PD-1");

        Assert.Equal(WorkflowAdminRules.CodeDependencyCycle, cycle!.Value.Code);
        Assert.Contains("would close a cycle", cycle.Value.Message, StringComparison.Ordinal);
        Assert.Contains("PD-1", cycle.Value.Message, StringComparison.Ordinal);
        Assert.Contains("PD-3", cycle.Value.Message, StringComparison.Ordinal);

        // A dependency that does not close one is accepted.
        Assert.Null(WorkflowAdminRules.ValidateDependency(dependencies, "PD-3", "PD-9"));

        // A definition depending on itself is a cycle of one.
        var self = WorkflowAdminRules.ValidateDependency(dependencies, "PD-1", "PD-1");

        Assert.Equal(WorkflowAdminRules.CodeSelfDependency, self!.Value.Code);

        // Both ends are required.
        Assert.Equal(WorkflowAdminRules.CodeUnknownTarget,
            WorkflowAdminRules.ValidateDependency(dependencies, " ", "PD-1")!.Value.Code);
    }

    [Fact]
    public void A_change_needs_an_actor_a_reason_and_a_confirmation()
    {
        // AC-2 and the state table: "change without a reason" is refused, and the confirmation names what it affects.
        Assert.Null(WorkflowAdminRules.ValidateChange("sara", "Retire the old ladder", "PD-1, PD-2"));

        Assert.Equal(WorkflowAdminRules.CodeActorRequired,
            WorkflowAdminRules.ValidateChange(" ", "why", "PD-1")!.Value.Code);

        var noReason = WorkflowAdminRules.ValidateChange("sara", "  ", "PD-1");

        Assert.Equal(WorkflowAdminRules.CodeReasonRequired, noReason!.Value.Code);
        Assert.Contains("affects every approval", noReason.Value.Message, StringComparison.Ordinal);

        var noConfirmation = WorkflowAdminRules.ValidateChange("sara", "why", " ");

        Assert.Equal(WorkflowAdminRules.CodeReasonRequired, noConfirmation!.Value.Code);
        Assert.Contains("confirmation", noConfirmation.Value.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void A_delegation_outside_its_window_is_inactive_and_marked()
    {
        // AC-3.
        var inside = WorkflowAdminRules.DelegationState("Y", AsOf.AddDays(-1), AsOf.AddDays(1), AsOf);

        Assert.True(inside.Active);
        Assert.Contains("inside its window", inside.StateText, StringComparison.Ordinal);

        var before = WorkflowAdminRules.DelegationState("Y", AsOf.AddDays(1), AsOf.AddDays(2), AsOf);

        Assert.False(before.Active);
        Assert.Contains("outside its window", before.StateText, StringComparison.Ordinal);
        Assert.Contains("starts", before.StateText, StringComparison.Ordinal);

        var after = WorkflowAdminRules.DelegationState("Y", AsOf.AddDays(-2), AsOf.AddDays(-1), AsOf);

        Assert.False(after.Active);
        Assert.Contains("ended", after.StateText, StringComparison.Ordinal);

        var withdrawn = WorkflowAdminRules.DelegationState("N", AsOf.AddDays(-1), AsOf.AddDays(1), AsOf);

        Assert.False(withdrawn.Active);
        Assert.Contains("withdrawn", withdrawn.StateText, StringComparison.Ordinal);

        // No recorded window is not treated as expired; it is reported as exactly that.
        Assert.Contains("no window recorded",
            WorkflowAdminRules.DelegationState("Y", null, null, AsOf).StateText, StringComparison.Ordinal);
    }

    [Fact]
    public void A_delegation_needs_both_names_a_window_and_a_sane_limit()
    {
        Assert.Null(WorkflowAdminRules.ValidateDelegation("sara", "nadia", AsOf, AsOf.AddDays(7), 50_000m));

        Assert.Equal(WorkflowAdminRules.CodeDelegationInvalid,
            WorkflowAdminRules.ValidateDelegation("sara", " ", AsOf, AsOf.AddDays(7), 0m)!.Value.Code);
        Assert.Equal(WorkflowAdminRules.CodeDelegationInvalid,
            WorkflowAdminRules.ValidateDelegation("sara", "sara", AsOf, AsOf.AddDays(7), 0m)!.Value.Code);
        Assert.Equal(WorkflowAdminRules.CodeWindowInvalid,
            WorkflowAdminRules.ValidateDelegation("sara", "nadia", null, AsOf, 0m)!.Value.Code);
        Assert.Equal(WorkflowAdminRules.CodeWindowInvalid,
            WorkflowAdminRules.ValidateDelegation("sara", "nadia", AsOf.AddDays(7), AsOf, 0m)!.Value.Code);
        Assert.Equal(WorkflowAdminRules.CodeDelegationInvalid,
            WorkflowAdminRules.ValidateDelegation("sara", "nadia", AsOf, AsOf.AddDays(7), -1m)!.Value.Code);
    }

    [Fact]
    public void The_sod_tester_answers_pass_or_fail_and_names_the_rule()
    {
        // AC-4.
        var rules = new List<OgSodRuleRow>
        {
            new("R-1", "Creator cannot approve", "reserves.create", "reserves.approve", "CRITICAL", true, true, "PROCESS", null, null),
            new("R-2", "Advisory pairing", "reserves.view", "reserves.approve", "HIGH", false, true, "PROCESS", null, null),
            new("R-3", "Retired rule", "reserves.create", "reserves.delete", "CRITICAL", true, false, "PROCESS", null, null),
        };

        var failed = WorkflowAdminRules.Test(rules, "reserves.create", "reserves.approve");

        Assert.True(failed.Failed);
        Assert.Contains("'Creator cannot approve'", failed.Sentence, StringComparison.Ordinal);
        Assert.Equal(["R-1"], failed.Rules);

        // The order of the pair does not matter.
        Assert.True(WorkflowAdminRules.Test(rules, "reserves.approve", "reserves.create").Failed);

        // A non-blocking match is reported as advisory, not as a pass that hides it.
        var advisory = WorkflowAdminRules.Test(rules, "reserves.view", "reserves.approve");

        Assert.False(advisory.Failed);
        Assert.Contains("advisory rather than blocking", advisory.Sentence, StringComparison.Ordinal);

        // An inactive rule is not applied, and an unrelated pair passes.
        Assert.False(WorkflowAdminRules.Test(rules, "reserves.create", "reserves.delete").Failed);
        Assert.False(WorkflowAdminRules.Test(rules, "reserves.export", "reserves.approve").Failed);
        Assert.Contains("pass:", WorkflowAdminRules.Test(rules, "a", "b").Describe(), StringComparison.Ordinal);
        Assert.Contains("fail:", failed.Describe(), StringComparison.Ordinal);
    }

    [Fact]
    public void Only_administrators_reach_the_page()
    {
        // AC-6: the catalogue gates the page on the two administrator roles rather than on everyone.
        var page = OgModuleCatalog.Pages.Single(definition => definition.Route == "workflowadmin");

        Assert.Contains(OgRoleNames.SystemAdmin, page.RequiredRoles);
        Assert.Contains(OgRoleNames.TenantAdmin, page.RequiredRoles);
        Assert.DoesNotContain(OgRoleNames.Viewer, page.RequiredRoles);
        Assert.DoesNotContain(OgRoleNames.ProductionEngineer, page.RequiredRoles);

        // And the page shows the recorded change history, which is what makes a change reviewable.
        var view = File.ReadAllText(
            SourceRoot.AllSources().Single(source => source.EndsWith(PageFileName, StringComparison.Ordinal)));

        Assert.Contains("workspace.Changes", view, StringComparison.Ordinal);
        Assert.Contains("workspace.DiagnosticsText", view, StringComparison.Ordinal);
        Assert.Contains("workspace.SodRules", view, StringComparison.Ordinal);
    }

    [Fact]
    public void An_orphaned_instance_is_reported_with_its_cause()
    {
        var finding = new OgAdminDiagnostic(
            "orphaned instance", "PI-9", "no process definition named 'PD-9' is recorded");

        Assert.Contains("orphaned instance", finding.Describe(), StringComparison.Ordinal);
        Assert.Contains("PD-9", finding.Describe(), StringComparison.Ordinal);

        var workspace = new OgAdminWorkspace([], [], [], [], [], [], [finding], [], AsOf, DateTime.UtcNow);

        Assert.Contains("✗ orphaned instance", workspace.DiagnosticsText, StringComparison.Ordinal);
        Assert.Contains("1 diagnostic(s)", workspace.CountsText, StringComparison.Ordinal);
        Assert.Contains("No configuration change is recorded", workspace.ChangesText, StringComparison.Ordinal);
    }
}
