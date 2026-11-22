using TheTechIdea.Beep.OilandGas.Desktop.Data.Exploration;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// EXP-05: the gate rules that hold without a database - the author cannot decide, an incomplete prospect risk
/// blocks the gate, a rationale is required except for a plain approval, conditions belong to an approval with
/// conditions, and a program that is not pending cannot be decided again.
/// </summary>
public sealed class WellProgramApprovalTests
{
    private static readonly OgWellProgramApprovals Gate = new(() => null!, new Quiet());

    [Fact]
    public void The_author_cannot_decide_their_own_program()
    {
        var program = Program(author: "fahad");

        var refusal = OgWellProgramApprovals.Validate(
            program,
            new OgWellProgramDecision(OgWellProgramDecision.Approve, null, [], "fahad"));

        // AC-3
        Assert.NotNull(refusal);
        Assert.Equal(OgWellProgramApprovals.CodeAuthorCannotDecide, refusal!.Value.Code);
        Assert.Contains("cannot decide", refusal.Value.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void An_incomplete_prospect_risk_blocks_the_gate()
    {
        var program = Program(author: "author", riskComplete: false);

        // AC-2
        foreach (var decision in OgWellProgramDecision.Decisions)
        {
            var refusal = OgWellProgramApprovals.Validate(
                program,
                new OgWellProgramDecision(decision, "a rationale", ["a condition"], "approver"));

            Assert.NotNull(refusal);
            Assert.Equal(OgWellProgramApprovals.CodeRiskIncomplete, refusal!.Value.Code);
        }
    }

    [Fact]
    public void A_plain_approval_needs_no_rationale_but_every_other_decision_does()
    {
        var program = Program(author: "author");

        // AC-1: the plan makes the rationale mandatory except for approve.
        Assert.Null(OgWellProgramApprovals.Validate(
            program,
            new OgWellProgramDecision(OgWellProgramDecision.Approve, null, [], "approver")));

        foreach (var decision in new[] { OgWellProgramDecision.Return, OgWellProgramDecision.Reject })
        {
            var refusal = OgWellProgramApprovals.Validate(
                program,
                new OgWellProgramDecision(decision, "  ", [], "approver"));

            Assert.NotNull(refusal);
            Assert.Equal(OgWellProgramApprovals.CodeRationaleRequired, refusal!.Value.Code);
            Assert.Null(OgWellProgramApprovals.Validate(
                program,
                new OgWellProgramDecision(decision, "not worth the spend", [], "approver")));
        }
    }

    [Fact]
    public void Conditions_belong_to_an_approval_with_conditions()
    {
        var program = Program(author: "author");

        // AC-5: an approval with conditions needs at least one.
        var missing = OgWellProgramApprovals.Validate(
            program,
            new OgWellProgramDecision(OgWellProgramDecision.ApproveWithConditions, "fine with conditions", [], "approver"));

        Assert.Equal(OgWellProgramApprovals.CodeConditionsRequired, missing!.Value.Code);

        // A plain approval does not carry conditions.
        var notAllowed = OgWellProgramApprovals.Validate(
            program,
            new OgWellProgramDecision(OgWellProgramDecision.Approve, null, ["drill with a liner"], "approver"));

        Assert.Equal(OgWellProgramApprovals.CodeConditionsNotAllowed, notAllowed!.Value.Code);

        // Return and reject do not carry them either.
        var onReject = OgWellProgramApprovals.Validate(
            program,
            new OgWellProgramDecision(OgWellProgramDecision.Reject, "no", ["a condition"], "approver"));

        Assert.Equal(OgWellProgramApprovals.CodeConditionsNotAllowed, onReject!.Value.Code);

        Assert.Null(OgWellProgramApprovals.Validate(
            program,
            new OgWellProgramDecision(
                OgWellProgramDecision.ApproveWithConditions,
                "approved with conditions",
                ["drill with a liner", "  ", "no night operations"],
                "approver")));
    }

    [Fact]
    public void A_decision_must_be_recorded_by_somebody_and_must_be_a_known_decision()
    {
        var program = Program(author: "author");

        Assert.Equal(
            OgWellProgramApprovals.CodeActorRequired,
            OgWellProgramApprovals.Validate(
                program,
                new OgWellProgramDecision(OgWellProgramDecision.Approve, null, [], " "))!.Value.Code);

        Assert.Equal(
            OgWellProgramApprovals.CodeDecisionRequired,
            OgWellProgramApprovals.Validate(
                program,
                new OgWellProgramDecision("MAYBE", null, [], "approver"))!.Value.Code);
    }

    [Fact]
    public void A_program_that_is_not_pending_cannot_be_decided()
    {
        var decided = Program(author: "author") with { State = "APPROVED" };

        // AC-4
        var refusal = OgWellProgramApprovals.Validate(
            decided,
            new OgWellProgramDecision(OgWellProgramDecision.Reject, "changed my mind", [], "approver"));

        Assert.NotNull(refusal);
        Assert.Equal(OgWellProgramApprovals.CodeNotPending, refusal!.Value.Code);
    }

    [Fact]
    public void A_decision_maps_to_the_state_it_produces()
    {
        Assert.Equal("APPROVED", new OgWellProgramDecision("approve", null, [], "a").ResultingState);
        Assert.Equal(
            "APPROVED_WITH_CONDITIONS",
            new OgWellProgramDecision("approve_with_conditions", null, [], "a").ResultingState);
        Assert.Equal("RETURNED", new OgWellProgramDecision("return", null, [], "a").ResultingState);
        Assert.Equal("REJECTED", new OgWellProgramDecision("reject", null, [], "a").ResultingState);
        Assert.Equal(string.Empty, new OgWellProgramDecision("maybe", null, [], "a").ResultingState);
    }

    [Fact]
    public async Task A_retry_with_nothing_pending_is_refused()
    {
        var retried = await Gate.RetryAsync(DateTime.Today);

        // AC-6
        Assert.False(retried.Success);
        Assert.Equal(OgWellProgramApprovals.CodeNothingPending, retried.Reference);
        Assert.Null(Gate.Pending);
    }

    [Fact]
    public async Task A_rule_refusal_is_reported_before_a_connection_is_needed()
    {
        // The validation runs first, so a rule refusal is not masked by a missing connection.
        var refused = await Gate.DecideAsync(
            Program(author: "fahad"),
            new OgWellProgramDecision(OgWellProgramDecision.Approve, null, [], "fahad"));

        Assert.Equal(OgWellProgramApprovals.CodeAuthorCannotDecide, refused.Reference);
        Assert.Null(Gate.Pending);

        // With the rules satisfied, the missing connection is the reason.
        var noConnection = await Gate.DecideAsync(
            Program(author: "author"),
            new OgWellProgramDecision(OgWellProgramDecision.Approve, null, [], "approver"));

        Assert.Equal(OgWellProgramApprovals.CodeNoConnection, noConnection.Reference);
        Assert.Null(Gate.Pending);
    }

    private static OgWellProgram Program(string? author, bool riskComplete = true) =>
        new(
            "WP-1",
            "P-1",
            "Alpha",
            "Alpha exploration well",
            "Test the Alpha closure",
            3200m,
            "m",
            12_000_000m,
            "USD",
            OgWellProgram.StatePending,
            author,
            new DateTimeOffset(2026, 3, 1, 0, 0, 0, TimeSpan.Zero),
            10,
            null,
            null,
            null,
            [],
            riskComplete,
            1,
            []);

    private sealed class Quiet : IFailureReporter
    {
        public string ReportHandled(
            Exception exception,
            string operation,
            string consequence,
            FailureSeverity severity = FailureSeverity.Error) => FailureReferenceFormat.New();
    }
}
