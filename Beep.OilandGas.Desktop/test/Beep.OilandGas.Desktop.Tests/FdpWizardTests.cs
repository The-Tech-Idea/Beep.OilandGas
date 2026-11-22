using TheTechIdea.Beep.OilandGas.Desktop.Data.Development;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// DEV-02: the missing-step rule, the editable/terminal states, the review validation and the step names - the parts
/// that do not need a database. The draft, submit, review, baseline and revision are proven in the gateway test.
/// </summary>
public sealed class FdpWizardTests
{
    private static readonly DateTime AsOf = new(2026, 6, 1);

    [Fact]
    public void Missing_steps_start_with_the_first_unfinished_step()
    {
        var scopeOnly = Draft(scope: "Alpha", reservoir: null, wells: 0, facility: null, cost: 0m, currency: null, risk: null);
        var missing = OgFdpWizardService.MissingSteps(scopeOnly, hasSchedule: true);

        Assert.Equal(new[] { 2, 4, 5, 6 }, missing);
        Assert.Equal("Reservoirs and well count", OgFdpWizardService.StepName(missing[0]));

        Assert.Contains(3, OgFdpWizardService.MissingSteps(scopeOnly, hasSchedule: false));
    }

    [Fact]
    public void A_complete_draft_has_no_missing_step()
    {
        var complete = Draft("Alpha", "Upper sand", 4, "Central facility", 1200m, "USD", "Water disposal");

        Assert.Empty(OgFdpWizardService.MissingSteps(complete, hasSchedule: true));
        Assert.Contains(3, OgFdpWizardService.MissingSteps(complete, hasSchedule: false));
    }

    [Fact]
    public void A_draft_or_returned_plan_is_editable_and_a_submitted_or_terminal_one_is_not()
    {
        Assert.True(DraftWithState(OgFdpDraft.Draft).IsEditable);
        Assert.True(DraftWithState(OgFdpDraft.Returned).IsEditable);
        Assert.False(DraftWithState(OgFdpDraft.Submitted).IsEditable);
        Assert.False(DraftWithState(OgFdpDraft.Approved).IsEditable);
        Assert.False(DraftWithState(OgFdpDraft.Rejected).IsEditable);
        Assert.True(DraftWithState(OgFdpDraft.Approved).IsTerminal);
    }

    [Fact]
    public void A_return_names_the_step_to_correct()
    {
        var returned = DraftWithState(OgFdpDraft.Returned) with
        {
            ReturnedStep = 4,
            ReturnReason = "capacity is not justified",
        };

        Assert.Contains("Facilities and capacity", returned.ReturnText!, StringComparison.Ordinal);
        Assert.Contains("capacity is not justified", returned.ReturnText!, StringComparison.Ordinal);
    }

    [Fact]
    public async Task A_review_needs_a_reviewer_a_known_decision_a_rationale_a_return_step_and_conditions()
    {
        var wizard = new OgFdpWizardService(() => null, new Quiet());

        Assert.Equal(
            OgFdpWizardService.CodeActorRequired,
            (await wizard.ReviewAsync("D-1", new OgFdpDecision("APPROVE", "ok", null, [], " "))).Reference);

        Assert.Equal(
            OgFdpWizardService.CodeDecisionRequired,
            (await wizard.ReviewAsync("D-1", new OgFdpDecision("BOGUS", "ok", null, [], "sara"))).Reference);

        Assert.Equal(
            OgFdpWizardService.CodeRationaleRequired,
            (await wizard.ReviewAsync("D-1", new OgFdpDecision("APPROVE", " ", null, [], "sara"))).Reference);

        Assert.Equal(
            OgFdpWizardService.CodeConditionsRequired,
            (await wizard.ReviewAsync(
                "D-1", new OgFdpDecision("APPROVE_WITH_CONDITIONS", "ok", null, [], "sara"))).Reference);

        Assert.Equal(
            OgFdpWizardService.CodeReturnStepRequired,
            (await wizard.ReviewAsync("D-1", new OgFdpDecision("RETURN", "fix it", null, [], "sara"))).Reference);
    }

    [Fact]
    public async Task A_cost_cannot_be_negative_and_needs_a_currency()
    {
        var wizard = new OgFdpWizardService(() => null, new Quiet());

        Assert.Equal(
            OgFdpWizardService.CodeActorRequired,
            (await wizard.SaveDraftAsync("D-1", Values(-1m), " ")).Reference);

        Assert.Equal(
            OgFdpWizardService.CodeCostInvalid,
            (await wizard.SaveDraftAsync("D-1", Values(-1m), "sara")).Reference);

        Assert.Equal(
            OgFdpWizardService.CodeCurrencyRequired,
            (await wizard.SaveDraftAsync("D-1", Values(10m, currency: " "), "sara")).Reference);
    }

    [Fact]
    public async Task No_connection_is_a_failure_not_an_empty_workspace()
    {
        var wizard = new OgFdpWizardService(() => null, new Quiet());

        var loaded = await wizard.LoadAsync("F-1", "Alpha");

        Assert.False(loaded.Success);
        Assert.Equal(OgFdpWizardService.CodeNoConnection, loaded.Reference);
        Assert.Null(loaded.Value);
    }

    [Fact]
    public void The_step_names_are_the_seven_wizard_steps()
    {
        Assert.Equal("Scope", OgFdpWizardService.StepName(1));
        Assert.Equal("Well schedule", OgFdpWizardService.StepName(3));
        Assert.Equal("Review and submit", OgFdpWizardService.StepName(7));
        Assert.Contains("99", OgFdpWizardService.StepName(99), StringComparison.Ordinal);
    }

    private static OgFdpDraft Draft(
        string? scope,
        string? reservoir,
        int wells,
        string? facility,
        decimal cost,
        string? currency,
        string? risk) =>
        new(
            "D-1",
            "FDP-1",
            "F-1",
            1,
            OgFdpDraft.Draft,
            1,
            "sara",
            scope,
            reservoir,
            wells,
            facility,
            cost,
            currency,
            risk,
            null,
            null,
            null);

    private static OgFdpDraft DraftWithState(string state) =>
        Draft("Alpha", "Upper sand", 4, "Central", 1200m, "USD", "Water disposal") with { State = state };

    private static OgFdpDraftValues Values(decimal cost, string? currency = "USD") =>
        new("Alpha", "Upper sand", 4, "Central", cost, currency, "Water disposal", 7);

    private sealed class Quiet : IFailureReporter
    {
        public string ReportHandled(
            Exception exception,
            string operation,
            string consequence,
            FailureSeverity severity = FailureSeverity.Error) => FailureReferenceFormat.New();
    }
}
