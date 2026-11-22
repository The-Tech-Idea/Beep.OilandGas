using Beep.OilandGas.LifeCycle.Constants;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Economics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// ECO-03: the decision codes and their authorisation, the decision and proposal validation, the record's staleness and
/// economics rules, and the page's guard - the parts that need no database. The queue, the decision, the stale refusal and
/// the single-AFE handoff are proven in the gateway test.
/// </summary>
public sealed class InvestmentReviewTests
{
    private const string PageFileName = "InvestmentReviewPageView.cs";

    [Fact]
    public void Only_an_approving_decision_authorises_money()
    {
        // AC-4's precondition: the handoff is allowed only after a decision that authorises it.
        Assert.Equal(["APPROVED", "CONDITIONAL", "RETURNED", "REJECTED"], InvestmentDecisionCodes.Decisions);

        Assert.True(InvestmentDecisionCodes.Authorises("APPROVED"));
        Assert.True(InvestmentDecisionCodes.Authorises("conditional"));
        Assert.False(InvestmentDecisionCodes.Authorises("RETURNED"));
        Assert.False(InvestmentDecisionCodes.Authorises("REJECTED"));
        Assert.False(InvestmentDecisionCodes.Authorises(null));

        Assert.Equal(["APPROVED", "CONDITIONAL"], InvestmentDecisionCodes.Authorising);

        // AC-5's precondition: a conditional approval has to say what the conditions are.
        Assert.True(InvestmentDecisionCodes.NeedsConditions("CONDITIONAL"));
        Assert.False(InvestmentDecisionCodes.NeedsConditions("APPROVED"));

        // The queue states, in order.
        Assert.Equal(["QUEUED", "DECIDED", "HANDED_OFF"], InvestmentDecisionCodes.States);

        Assert.Equal("approved with conditions", InvestmentDecisionCodes.Describe("CONDITIONAL"));
        Assert.Equal("returned to the author", InvestmentDecisionCodes.Describe("RETURNED"));
    }

    [Fact]
    public void Conditions_are_stored_one_per_line_and_come_back_the_same()
    {
        // AC-5: the conditions the reviewer writes are the conditions the AFE receives.
        var conditions = InvestmentDecisionCodes.Split(
            "  Third-party audit before spud  \n\n Monitor overburden pressure \n");

        Assert.Equal(["Third-party audit before spud", "Monitor overburden pressure"], conditions);

        var stored = InvestmentDecisionCodes.Join(conditions);

        Assert.Equal("Third-party audit before spud\nMonitor overburden pressure", stored);
        Assert.Equal(conditions, InvestmentDecisionCodes.Split(stored));

        Assert.Null(InvestmentDecisionCodes.Join([]));
        Assert.Empty(InvestmentDecisionCodes.Split(null));
    }

    [Fact]
    public void A_decision_needs_a_known_decision_a_rationale_and_its_conditions()
    {
        // AC-1 and the state table's decision rules, in one place.
        Assert.Null(InvestmentReviewRules.ValidateDecision("APPROVED", "Bring forward value at 2.1 years payback", []));

        var unknown = InvestmentReviewRules.ValidateDecision("MAYBE", "because", []);

        Assert.Equal(InvestmentReviewCodes.InputInvalid, unknown!.Value.Code);
        Assert.Contains("APPROVED", unknown.Value.Message, StringComparison.Ordinal);

        var noReason = InvestmentReviewRules.ValidateDecision("APPROVED", "   ", []);

        Assert.Equal(InvestmentReviewCodes.InputRequired, noReason!.Value.Code);
        Assert.Contains("rationale", noReason.Value.Message, StringComparison.Ordinal);

        var noConditions = InvestmentReviewRules.ValidateDecision("CONDITIONAL", "approve with strings", []);

        Assert.Equal(InvestmentReviewCodes.ConditionsRequired, noConditions!.Value.Code);
        Assert.Contains("travel to the AFE", noConditions.Value.Message, StringComparison.Ordinal);

        // A conditional approval with a condition is fine.
        Assert.Null(InvestmentReviewRules.ValidateDecision("CONDITIONAL", "approve with strings", ["audit first"]));
    }

    [Fact]
    public void A_proposal_needs_its_title_entity_amount_currency_and_class()
    {
        Assert.Equal(
            ["title", "well or entity", "amount", "currency", "class"],
            InvestmentReviewRules.Missing(new OgInvestmentEntry(null, null, null, null, null, null, null, null, null)));

        Assert.Null(InvestmentReviewRules.Validate(Entry()));

        Assert.Equal(
            InvestmentReviewCodes.InputInvalid,
            InvestmentReviewRules.Validate(Entry() with { Class = "PICNIC" })!.Value.Code);

        Assert.Equal(
            InvestmentReviewCodes.InputInvalid,
            InvestmentReviewRules.Validate(Entry() with { Currency = "XYZ" })!.Value.Code);

        Assert.Equal(
            InvestmentReviewCodes.InputInvalid,
            InvestmentReviewRules.Validate(Entry() with { Amount = 0m })!.Value.Code);

        Assert.Contains("DRILL", InvestmentReviewRules.Classes);
        Assert.Equal(InvestmentDecisionCodes.Decisions, InvestmentReviewRules.Decisions);
    }

    [Fact]
    public void A_proposal_reports_missing_economics_and_stale_economics()
    {
        // The state table's "economics missing - decision disabled with a link to ECO-01", and AC-2's staleness.
        var without = Proposal() with { EconomicsId = null, EconomicsNpv = null };

        Assert.False(without.HasEconomics);
        Assert.Contains("disabled", without.EconomicsText, StringComparison.Ordinal);
        Assert.Contains("ECO-01", without.EconomicsText, StringComparison.Ordinal);

        var stale = Proposal() with { Amount = 3_000_000m, ReviewedAmount = 2_000_000m };

        Assert.True(stale.HasEconomics);
        Assert.True(stale.EconomicsStale);
        Assert.Contains("re-review is required", stale.EconomicsText, StringComparison.Ordinal);
        Assert.Contains("2,000,000.00 reviewed", stale.EconomicsText, StringComparison.Ordinal);

        // A proposal reviewed at its own amount is not stale.
        var fresh = Proposal() with { Amount = 3_000_000m, ReviewedAmount = 3_000_000m };

        Assert.False(fresh.EconomicsStale);

        // The decision sentence carries actor, date and the amount decided on (AC-1, SG-09-5).
        var decided = fresh with
        {
            State = InvestmentDecisionCodes.Decided,
            Decision = InvestmentDecisionCodes.Approved,
            DecidedBy = "omar",
            DecidedAt = new DateTime(2026, 6, 2, 9, 30, 0, DateTimeKind.Utc),
            DecisionAmount = 3_000_000m,
            Rationale = "best value",
        };

        Assert.False(decided.IsQueued);
        Assert.Contains("approved by omar", decided.DecisionText, StringComparison.Ordinal);
        Assert.Contains("2026-06-02", decided.DecisionText, StringComparison.Ordinal);
        Assert.Contains("best value", decided.RationaleText, StringComparison.Ordinal);
        Assert.Contains("ready to hand off", decided.HandoffText, StringComparison.Ordinal);

        // A returned proposal is not ready to hand off.
        var returned = decided with { Decision = InvestmentDecisionCodes.Returned };

        Assert.False(returned.Authorises);
        Assert.Contains("no handoff", returned.HandoffText, StringComparison.Ordinal);
    }

    [Fact]
    public void The_page_offers_only_the_owner_decisions_and_gates_its_buttons()
    {
        // The decision list and the classes come from the workspace; the page holds no decision literal of its own.
        var page = File.ReadAllText(
            SourceRoot.AllSources().Single(source => Path.GetFileName(source) == PageFileName));

        Assert.Contains("workspace.Decisions", page, StringComparison.Ordinal);
        Assert.Contains("workspace.Classes", page, StringComparison.Ordinal);
        Assert.Contains("workspace.Currencies", page, StringComparison.Ordinal);
        Assert.Contains("proposal.HasEconomics", page, StringComparison.Ordinal);
        Assert.Contains("proposal.EconomicsStale", page, StringComparison.Ordinal);

        Assert.DoesNotContain("\"CONDITIONAL\"", page, StringComparison.Ordinal);
        Assert.DoesNotContain("\"HANDED_OFF\"", page, StringComparison.Ordinal);
    }

    // ------------------------------------------------------------------ helpers

    private static OgInvestmentEntry Entry() =>
        new(
            "Woodford infill programme",
            "WELL-1",
            2_000_000m,
            "USD",
            "DRILL",
            new DateTime(2026, 7, 1),
            "ECO-EVAL-1",
            4_500_000m,
            "Water disposal capacity is the main risk");

    private static OgInvestmentProposal Proposal() =>
        new(
            "PROP-1",
            "Woodford infill programme",
            "WELL-1",
            2_000_000m,
            "USD",
            "DRILL",
            "sara",
            new DateTime(2026, 5, 1),
            new DateTime(2026, 7, 1),
            InvestmentDecisionCodes.Queued,
            null,
            null,
            [],
            null,
            null,
            null,
            "ECO-EVAL-1",
            4_500_000m,
            2_000_000m,
            new DateTime(2026, 5, 1),
            null,
            null,
            null,
            null);
}
