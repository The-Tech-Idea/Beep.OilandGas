using Beep.OilandGas.LifeCycle.Data.Tables;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Workflow;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// WFL-04: the decision rule, the checklist that a gate cannot satisfy itself, the segregation check that honours the owner's
/// blocking flag, the delegation rule and the single-decision rule - the parts that need no database. The writes are proven in
/// the gateway test.
/// </summary>
public sealed class GateReviewTests
{
    private const string PageFileName = "GatesPageView.cs";

    [Fact]
    public void Reject_and_return_require_a_rationale_and_conditions_require_their_text()
    {
        // AC-2 and SG-14-4.
        Assert.Null(GateRules.ValidateDecision(GateRules.Approve, null, null));
        Assert.Null(GateRules.ValidateDecision(GateRules.Approve, "Looks right", null));

        var reject = GateRules.ValidateDecision(GateRules.Reject, "  ", null);

        Assert.Equal(GateRules.CodeRationaleRequired, reject!.Value.Code);
        Assert.Contains("needs its rationale", reject.Value.Message, StringComparison.Ordinal);

        Assert.Equal(GateRules.CodeRationaleRequired,
            GateRules.ValidateDecision(GateRules.Return, null, "some conditions")!.Value.Code);

        Assert.Equal(GateRules.CodeConditionsRequired,
            GateRules.ValidateDecision(GateRules.ApproveWithConditions, "Fine", " ")!.Value.Code);

        Assert.Null(GateRules.ValidateDecision(GateRules.ApproveWithConditions, "Fine", "Monthly pressure test"));

        var unknown = GateRules.ValidateDecision("MAYBE", null, null);

        Assert.Equal(GateRules.CodeKindInvalid, unknown!.Value.Code);
        Assert.Contains("APPROVE_WITH_CONDITIONS", unknown.Value.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void The_checklist_is_the_other_approvals_and_never_the_gate_itself()
    {
        // AC-1: a gate cannot block itself, and another approval that is not approved does block it.
        var approvals = new List<PROCESS_APPROVAL>
        {
            new() { PROCESS_APPROVAL_ID = "PA-1", PROCESS_STEP_INSTANCE_ID = "PS-1", REQUIRED_ACTION = "GATE", APPROVER_SEQUENCE = 1 },
            new() { PROCESS_APPROVAL_ID = "PA-2", PROCESS_STEP_INSTANCE_ID = "PS-1", REQUIRED_ACTION = "TECHNICAL", APPROVER_SEQUENCE = 2, APPROVAL_STATUS = "APPROVED", APPROVED_BY = "omar" },
            new() { PROCESS_APPROVAL_ID = "PA-3", PROCESS_STEP_INSTANCE_ID = "PS-1", REQUIRED_ACTION = "SAFETY", APPROVER_SEQUENCE = 3, APPROVAL_STATUS = "PENDING" },
        };

        var checklist = GateRules.ChecklistOf(approvals, "PS-1", "PA-1");

        Assert.Equal(2, checklist.Count);
        Assert.DoesNotContain(checklist, item => item.Key.Contains("#1", StringComparison.Ordinal));

        var technical = checklist.Single(item => item.Key.StartsWith("TECHNICAL", StringComparison.Ordinal));

        Assert.True(technical.Satisfied);
        Assert.Equal("omar", technical.EvidenceRef);

        var safety = checklist.Single(item => item.Key.StartsWith("SAFETY", StringComparison.Ordinal));

        Assert.False(safety.Satisfied);
        Assert.Contains("pending", safety.Why, StringComparison.Ordinal);
        Assert.Null(safety.EvidenceRef);

        // Another step's approvals are not this gate's checklist.
        Assert.Empty(GateRules.ChecklistOf(approvals, "PS-9", "PA-1"));
    }

    [Fact]
    public void A_blocking_conflict_stops_the_approval_and_a_non_blocking_one_does_not()
    {
        // SG-14-3: the owner records IS_BLOCKING, and that is respected rather than every conflict being treated alike.
        var rules = new List<SOD_RULE>
        {
            new() { SOD_RULE_ID = "R-1", RULE_NAME = "Creator cannot approve", IS_BLOCKING = "Y", SEVERITY = "CRITICAL" },
            new() { SOD_RULE_ID = "R-2", RULE_NAME = "Advisory only", IS_BLOCKING = "N", SEVERITY = "HIGH" },
        };

        var conflicts = new List<SOD_CONFLICT>
        {
            new() { SOD_CONFLICT_ID = "C-1", SOD_RULE_ID = "R-1", RULE_NAME = "Creator cannot approve", USER_ID = "nadia", ROLE_A = "ReservoirEngineer", ROLE_B = "Approver" },
            new() { SOD_CONFLICT_ID = "C-2", SOD_RULE_ID = "R-2", RULE_NAME = "Advisory only", USER_ID = "nadia" },
        };

        var blocks = GateRules.SodBlocks("nadia", conflicts, rules);

        var block = Assert.Single(blocks);

        Assert.Contains("Creator cannot approve", block, StringComparison.Ordinal);
        Assert.Contains("ReservoirEngineer", block, StringComparison.Ordinal);
        Assert.DoesNotContain("Advisory only", block, StringComparison.Ordinal);

        // Somebody else is untouched, and a resolved conflict no longer blocks.
        Assert.Empty(GateRules.SodBlocks("sara", conflicts, rules));

        var resolved = new List<SOD_CONFLICT>
        {
            new() { SOD_CONFLICT_ID = "C-1", SOD_RULE_ID = "R-1", RULE_NAME = "Creator cannot approve", USER_ID = "nadia", CONFLICT_STATUS = "RESOLVED", RESOLVED_DATE = DateTime.UtcNow },
        };

        Assert.Empty(GateRules.SodBlocks("nadia", resolved, rules));
    }

    [Fact]
    public void The_author_and_the_requester_cannot_decide_their_own_gate()
    {
        // AC-6.
        var author = GateRules.CheckAuthor("sara", null, "sara");

        Assert.Equal(GateRules.CodeAuthorCannotApprove, author!.Value.Code);
        Assert.Contains("authored this subject", author.Value.Message, StringComparison.Ordinal);

        var requester = GateRules.CheckAuthor("sara", "omar", "omar");

        Assert.Equal(GateRules.CodeAuthorCannotApprove, requester!.Value.Code);
        Assert.Contains("requested this approval", requester.Value.Message, StringComparison.Ordinal);

        Assert.Null(GateRules.CheckAuthor("sara", "omar", "nadia"));
        Assert.Equal(GateRules.CodeActorRequired, GateRules.CheckAuthor("sara", "omar", " ")!.Value.Code);
    }

    [Fact]
    public void A_delegated_gate_is_decided_by_its_delegate_and_shows_both_names()
    {
        // AC-3 and SG-14-7.
        Assert.Equal(GateRules.CodeDelegatedToAnother,
            GateRules.CheckDelegation("sara", "nadia", "omar")!.Value.Code);
        Assert.Null(GateRules.CheckDelegation("sara", "nadia", "nadia"));

        // A delegation with no delegate cannot be held by anybody.
        var orphan = GateRules.CheckDelegation("sara", null, "nadia");

        Assert.Equal(GateRules.CodeDelegatedToAnother, orphan!.Value.Code);
        Assert.Contains("no delegate", orphan.Value.Message, StringComparison.Ordinal);

        // An undelegated gate is not restricted.
        Assert.Null(GateRules.CheckDelegation(null, null, "omar"));

        var gate = Gate(
            delegatedFrom: "sara",
            delegatedTo: "nadia",
            blocks: []);

        Assert.True(gate.IsDelegated);
        Assert.Contains("Acting for sara", gate.DelegationText, StringComparison.Ordinal);
        Assert.Contains("nadia", gate.DelegationText, StringComparison.Ordinal);

        Assert.Contains("Not delegated", Gate(delegatedFrom: null, delegatedTo: null, blocks: []).DelegationText,
            StringComparison.Ordinal);
    }

    [Fact]
    public void A_decided_gate_is_read_only_and_the_original_is_retained()
    {
        // AC-4.
        Assert.Null(GateRules.CheckUndecided(null));
        Assert.Null(GateRules.CheckUndecided("PENDING"));

        var decided = GateRules.CheckUndecided("APPROVED");

        Assert.Equal(GateRules.CodeAlreadyDecided, decided!.Value.Code);
        Assert.Contains("read-only", decided.Value.Message, StringComparison.Ordinal);
        Assert.Contains("original decision is retained", decided.Value.Message, StringComparison.Ordinal);

        Assert.Contains("read-only", Gate(decided: "APPROVED", blocks: []).DecisionText, StringComparison.Ordinal);
        Assert.Equal("APPROVED", GateRules.StatusFor(GateRules.ApproveWithConditions));
        Assert.Equal("RETURNED", GateRules.StatusFor(GateRules.Return));
        Assert.Equal("REJECTED", GateRules.StatusFor(GateRules.Reject));
    }

    [Fact]
    public void Approve_is_offered_only_when_nothing_blocks_it()
    {
        // AC-1 with the block list: the same answer feeds the checklist panel, the blocks panel and the button.
        var blocked = Gate(blocks: ["TECHNICAL #2 is not satisfied: it is pending."]);

        Assert.False(blocked.ApproveAllowed);
        Assert.Contains("✗ TECHNICAL #2", blocked.BlocksText, StringComparison.Ordinal);

        var ready = Gate(blocks: []);

        Assert.True(ready.ApproveAllowed);
        Assert.Contains("Nothing blocks an approval", ready.BlocksText, StringComparison.Ordinal);

        // A decided gate is never approving again, however clean its list.
        Assert.False(Gate(decided: "REJECTED", blocks: []).ApproveAllowed);

        var page = File.ReadAllText(
            SourceRoot.AllSources().Single(source => source.EndsWith(PageFileName, StringComparison.Ordinal)));

        Assert.Contains("gate.ApproveAllowed", page, StringComparison.Ordinal);
        Assert.Contains("gate.DelegationText", page, StringComparison.Ordinal);
        Assert.Contains("gate.ChecklistText", page, StringComparison.Ordinal);
        Assert.Contains("gate.BlocksText", page, StringComparison.Ordinal);
    }

    private static OgGateReview Gate(
        string? delegatedFrom = null,
        string? delegatedTo = null,
        IReadOnlyList<string>? blocks = null,
        string decided = GateRules.Pending) =>
        new(
            "PA-1",
            "PI-1",
            "S-2",
            "Peer review",
            "Drilling approval",
            "WELL",
            "W-1",
            "GATE",
            "omar",
            new DateTime(2026, 11, 1, 8, 0, 0, DateTimeKind.Utc),
            7m,
            "2026-11-10 08:00",
            delegatedFrom,
            delegatedTo,
            decided,
            [],
            [],
            "sara",
            blocks ?? []);
}
