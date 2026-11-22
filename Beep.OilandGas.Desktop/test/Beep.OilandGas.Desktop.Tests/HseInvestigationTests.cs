using Beep.OilandGas.Models.Constants;
using Beep.OilandGas.Models.Data.HSE;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Hse;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// HSE-03: the assignment retention rule, the action's owner and due date, the root-cause rule, the closure checklist and the
/// records' own state - the parts that need no database. The commands and their rows are proven in the gateway test.
/// </summary>
public sealed class HseInvestigationTests
{
    private const string PageFileName = "InvestigationPageView.cs";

    [Fact]
    public void An_action_needs_its_owner_and_its_due_date()
    {
        // AC-6.
        Assert.Null(InvestigationRules.ValidateAction("Replace the seal", "omar", new DateTime(2026, 12, 1)));

        Assert.Equal(InvestigationRules.CodeInputRequired,
            InvestigationRules.ValidateAction(null, "omar", new DateTime(2026, 12, 1))!.Value.Code);
        Assert.Equal(InvestigationRules.CodeInputRequired,
            InvestigationRules.ValidateAction("Replace the seal", " ", new DateTime(2026, 12, 1))!.Value.Code);
        Assert.Equal(InvestigationRules.CodeInputRequired,
            InvestigationRules.ValidateAction("Replace the seal", "omar", null)!.Value.Code);
    }

    [Fact]
    public void An_assignment_needs_a_participant_a_role_and_a_rationale()
    {
        Assert.Null(InvestigationRules.ValidateAssignment("omar", "INVESTIGATOR", "He knows the separator train."));

        Assert.Equal(InvestigationRules.CodeInputRequired,
            InvestigationRules.ValidateAssignment(null, "INVESTIGATOR", "why")!.Value.Code);
        Assert.Equal(InvestigationRules.CodeInputRequired,
            InvestigationRules.ValidateAssignment("omar", "", "why")!.Value.Code);
        Assert.Equal(InvestigationRules.CodeInputRequired,
            InvestigationRules.ValidateAssignment("omar", "INVESTIGATOR", " ")!.Value.Code);
    }

    [Fact]
    public void A_recorded_assignment_is_retained_and_a_second_one_is_blocked()
    {
        // AC-1: nothing recorded yet is assignable; the same participant is a retry; a different one is blocked.
        Assert.Null(InvestigationRules.CheckAssignment(null, "omar", "INC-1"));
        Assert.Null(InvestigationRules.CheckAssignment("omar", "omar", "INC-1"));

        var blocked = InvestigationRules.CheckAssignment("omar", "nadia", "INC-1");

        Assert.Equal(InvestigationRules.CodeAssignmentRecorded, blocked!.Value.Code);
        Assert.Contains("INC-1", blocked.Value.Message, StringComparison.Ordinal);
        Assert.Contains("omar", blocked.Value.Message, StringComparison.Ordinal);
        Assert.Contains("original is retained", blocked.Value.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void A_root_cause_is_a_root_row_of_at_least_twenty_characters()
    {
        // The owner's IsRCACompleteAsync rule.
        Assert.False(InvestigationRules.HasRootCause([]));
        Assert.False(InvestigationRules.HasRootCause([Cause(1, CauseType.Root, "Too short")]));
        Assert.True(InvestigationRules.HasRootCause(
            [Cause(1, CauseType.Root, "The seal failed because the pump ran dry for six hours")]));

        // An immediate cause of the same length is not a root cause.
        Assert.False(InvestigationRules.HasRootCause(
            [Cause(1, CauseType.Immediate, "The seal failed because the pump ran dry for six hours")]));
    }

    [Fact]
    public void The_closure_checklist_reports_every_blocking_reason()
    {
        // AC-4 and AC-5, at the rule rather than the command.
        var open = Action("S-1", HseReferenceCodes.CorrectiveAction.Open, verifiedBy: null);
        var done = Action("S-2", HseReferenceCodes.CorrectiveAction.Completed, verifiedBy: "nadia");

        var workspace = new OgInvestigationWorkspace(
            "INC-1", IncidentState.Reported, "sara", "INVESTIGATOR",
            [Cause(1, CauseType.Root, "The seal failed because the pump ran dry for six hours")],
            [open, done], "P-1", "Plan", "CAP-1", DateTime.UtcNow);

        Assert.False(workspace.AllActionsVerified);
        Assert.Single(workspace.Unverified);

        // The closer is the investigator and one action is unverified: both are refused, not just the first.
        var blocks = InvestigationRules.ClosureBlocks(workspace, "sara");

        Assert.Contains(blocks, block => block.Code == InvestigationRules.CodeClosureBlocked);
        Assert.Contains(blocks, block => block.Code == InvestigationRules.CodeCloserIsInvestigator);

        // A different closer with every action verified and a root cause passes.
        var ready = workspace with
        {
            Actions =
            [
                done with { StepId = "S-1" },
            ],
        };

        Assert.True(ready.AllActionsVerified);
        Assert.Empty(InvestigationRules.ClosureBlocks(ready, "omar"));

        // A closed incident is refused as well, and so is a missing root cause.
        Assert.Contains(InvestigationRules.ClosureBlocks(ready with { IncidentState = IncidentState.Closed }, "omar"),
            block => block.Code == InvestigationRules.CodeAlreadyClosed);

        Assert.Contains(InvestigationRules.ClosureBlocks(ready with { Causes = [] }, "omar"),
            block => block.Code == InvestigationRules.CodeClosureBlocked);
    }

    [Fact]
    public void An_action_is_closed_out_only_when_it_is_completed_and_verified()
    {
        var open = Action("S-1", HseReferenceCodes.CorrectiveAction.Open, verifiedBy: null);
        var completed = Action("S-1", HseReferenceCodes.CorrectiveAction.Completed, verifiedBy: null);
        var verified = Action("S-1", HseReferenceCodes.CorrectiveAction.Completed, verifiedBy: "nadia");

        Assert.False(open.IsClosedOut);
        Assert.False(completed.IsClosedOut);
        Assert.True(verified.IsClosedOut);

        Assert.Contains("verified by nadia", verified.Describe(), StringComparison.Ordinal);
        Assert.Contains("not verified", completed.Describe(), StringComparison.Ordinal);
    }

    [Fact]
    public void An_overdue_action_says_so_and_a_completed_one_does_not()
    {
        var overdue = Action("S-1", HseReferenceCodes.CorrectiveAction.Open, verifiedBy: null) with
        {
            DueDate = new DateTime(2026, 1, 1),
            Overdue = true,
        };

        Assert.Contains("(overdue)", overdue.DueText, StringComparison.Ordinal);

        var completed = overdue with { State = HseReferenceCodes.CorrectiveAction.Completed, Overdue = false };

        Assert.DoesNotContain("(overdue)", completed.DueText, StringComparison.Ordinal);
        Assert.Contains("no due date", (completed with { DueDate = null }).DueText, StringComparison.Ordinal);
    }

    [Fact]
    public void A_cause_type_is_one_of_the_owners_three()
    {
        Assert.Null(InvestigationRules.ValidateCause(CauseType.Immediate, "Pump ran dry"));
        Assert.Null(InvestigationRules.ValidateCause(CauseType.Root, "A description"));

        Assert.Equal(InvestigationRules.CodeInputInvalid,
            InvestigationRules.ValidateCause("PROBABLE", "Pump ran dry")!.Value.Code);
        Assert.Equal(InvestigationRules.CodeInputRequired,
            InvestigationRules.ValidateCause(CauseType.Root, "  ")!.Value.Code);

        Assert.Equal([CauseType.Immediate, CauseType.Contributing, CauseType.Root], InvestigationRules.CauseTypes);
    }

    [Fact]
    public void The_page_binds_the_checklist_and_the_actions()
    {
        var page = File.ReadAllText(
            SourceRoot.AllSources().Single(source => source.EndsWith(PageFileName, StringComparison.Ordinal)));

        Assert.Contains("workspace.ChecklistText", page, StringComparison.Ordinal);
        Assert.Contains("workspace.Actions", page, StringComparison.Ordinal);
        Assert.Contains("workspace.Causes", page, StringComparison.Ordinal);
        Assert.Contains("workspace.InvestigatorText", page, StringComparison.Ordinal);

        // The complete and verify commands are offered only from the selected action's own state.
        Assert.Contains("IsCompleted: false", page, StringComparison.Ordinal);
        Assert.Contains("IsCompleted: true, IsVerified: false", page, StringComparison.Ordinal);
    }

    private static OgCause Cause(int seq, string type, string description) =>
        new(seq, type, CauseCategory.Human, description, null);

    private static OgCorrectiveAction Action(string stepId, string state, string? verifiedBy) =>
        new("P-1", stepId, "Replace the seal", "omar", new DateTime(2026, 12, 1), state, verifiedBy,
            verifiedBy is null ? null : new DateTime(2026, 11, 1), false);
}
