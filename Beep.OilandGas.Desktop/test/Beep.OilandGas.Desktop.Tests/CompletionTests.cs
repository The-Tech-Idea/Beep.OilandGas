using TheTechIdea.Beep.OilandGas.Desktop.Data.Development;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// DEV-07: the test value rules, the submission gaps, the decision rules (no passing test, author cannot accept,
/// conditions, handover recipient) and the handover record - the parts that do not need a database. The persistence,
/// the foreign-well evidence refusal and the lost acknowledgement are proven in the gateway test.
/// </summary>
public sealed class CompletionTests
{
    private static readonly DateTime Day = new(2026, 9, 1);

    [Fact]
    public void A_test_needs_a_type_a_valid_status_a_real_interval_and_a_result_unit()
    {
        Assert.Equal(
            CompletionRules.CodeTestTypeRequired,
            CompletionRules.ValidateTest(Values(type: " "))!.Value.Code);

        Assert.Equal(
            CompletionRules.CodeStatusInvalid,
            CompletionRules.ValidateTest(Values(status: "MAYBE"))!.Value.Code);

        Assert.Equal(
            CompletionRules.CodeIntervalInvalid,
            CompletionRules.ValidateTest(Values(top: 900m, bottom: 500m))!.Value.Code);

        Assert.Equal(
            CompletionRules.CodeIntervalInvalid,
            CompletionRules.ValidateTest(Values(top: -1m, bottom: 500m))!.Value.Code);

        Assert.Equal(
            CompletionRules.CodeUnitRequired,
            CompletionRules.ValidateTest(Values(depthUnit: null))!.Value.Code);

        // AC-5: every test states the unit of its result.
        Assert.Equal(
            CompletionRules.CodeUnitRequired,
            CompletionRules.ValidateTest(Values(resultUnit: null))!.Value.Code);

        Assert.Null(CompletionRules.ValidateTest(Values()));
        Assert.Null(CompletionRules.ValidateTest(Values(status: "pass")));
    }

    [Fact]
    public void A_submission_needs_a_test_and_an_evidence_item()
    {
        var test = Test("PASS");
        var evidence = Evidence("WELL-1");

        Assert.Equal(
            new[] { "no completion test is recorded", "no evidence item is attached" },
            CompletionRules.SubmissionGaps(0, 0));

        Assert.Equal(
            new[] { "no evidence item is attached" },
            CompletionRules.SubmissionGaps(1, 0));

        Assert.Equal(
            new[] { "no completion test is recorded" },
            CompletionRules.SubmissionGaps(0, 1));

        Assert.Empty(CompletionRules.SubmissionGaps(1, 1));
        Assert.Empty(CompletionRules.SubmissionGaps([test], [evidence]));
    }

    [Fact]
    public void Acceptance_needs_a_passed_test_and_a_handover_recipient()
    {
        var submitted = Submission();

        Assert.Equal(
            CompletionRules.CodeNoPassingTest,
            CompletionRules.ValidateDecision(
                submitted, new OgCompletionDecision(OgCompletionDecision.Accept, null, [], "production"), "reviewer", false)!
                .Value.Code);

        Assert.Equal(
            CompletionRules.CodeHandoverRequired,
            CompletionRules.ValidateDecision(
                submitted, new OgCompletionDecision(OgCompletionDecision.Accept, null, [], " "), "reviewer", true)!
                .Value.Code);

        Assert.Null(CompletionRules.ValidateDecision(
            submitted, new OgCompletionDecision(OgCompletionDecision.Accept, null, [], "production"), "reviewer", true));

        // A failing test only blocks the acceptance; a return needs no passing test.
        Assert.Null(CompletionRules.ValidateDecision(
            submitted,
            new OgCompletionDecision(OgCompletionDecision.Return, "the CBL is missing", [], null),
            "reviewer",
            false));
    }

    [Fact]
    public void The_author_cannot_decide_their_own_completion()
    {
        var submitted = Submission(author: "sara");

        Assert.Equal(
            CompletionRules.CodeAuthorCannotAccept,
            CompletionRules.ValidateDecision(
                submitted, new OgCompletionDecision(OgCompletionDecision.Accept, null, [], "production"), "sara", true)!
                .Value.Code);

        // Even a return is refused: the author does not review their own work.
        Assert.Equal(
            CompletionRules.CodeAuthorCannotAccept,
            CompletionRules.ValidateDecision(
                submitted,
                new OgCompletionDecision(OgCompletionDecision.Return, "fix it", [], null),
                "SARA",
                true)!.Value.Code);
    }

    [Fact]
    public void A_conditional_acceptance_needs_at_least_one_condition()
    {
        var submitted = Submission();

        Assert.Equal(
            CompletionRules.CodeConditionsRequired,
            CompletionRules.ValidateDecision(
                submitted,
                new OgCompletionDecision(OgCompletionDecision.AcceptWithConditions, null, ["  ", ""], "production"),
                "reviewer",
                true)!.Value.Code);

        Assert.Null(CompletionRules.ValidateDecision(
            submitted,
            new OgCompletionDecision(
                OgCompletionDecision.AcceptWithConditions,
                null,
                ["Install the flow meter within 30 days"],
                "production"),
            "reviewer",
            true));
    }

    [Fact]
    public void A_return_or_rejection_needs_a_rationale_and_the_decision_must_be_known()
    {
        var submitted = Submission();

        Assert.Equal(
            CompletionRules.CodeDecisionInvalid,
            CompletionRules.ValidateDecision(
                submitted, new OgCompletionDecision("BOGUS", "x", [], null), "reviewer", true)!.Value.Code);

        Assert.Equal(
            CompletionRules.CodeRationaleRequired,
            CompletionRules.ValidateDecision(
                submitted, new OgCompletionDecision(OgCompletionDecision.Return, " ", [], null), "reviewer", true)!
                .Value.Code);

        Assert.Equal(
            CompletionRules.CodeRationaleRequired,
            CompletionRules.ValidateDecision(
                submitted, new OgCompletionDecision(OgCompletionDecision.Reject, null, [], null), "reviewer", true)!
                .Value.Code);

        Assert.Equal(
            "completion-reviewer-required",
            CompletionRules.ValidateDecision(
                submitted, new OgCompletionDecision(OgCompletionDecision.Accept, null, [], "production"), "  ", true)!
                .Value.Code);
    }

    [Fact]
    public void Only_a_submitted_completion_can_be_decided()
    {
        var draft = Submission(state: OgCompletionSubmission.Draft);
        var decided = Submission(state: OgCompletionSubmission.Accepted);

        Assert.Equal(
            "completion-not-submitted",
            CompletionRules.ValidateDecision(
                draft, new OgCompletionDecision(OgCompletionDecision.Accept, null, [], "production"), "reviewer", true)!
                .Value.Code);

        Assert.Equal(
            "completion-not-submitted",
            CompletionRules.ValidateDecision(
                decided, new OgCompletionDecision(OgCompletionDecision.Accept, null, [], "production"), "reviewer", true)!
                .Value.Code);
    }

    [Fact]
    public void The_handover_record_carries_the_conditions_and_the_recipient()
    {
        var accepted = Submission(
            state: OgCompletionSubmission.AcceptedWithConditions,
            acceptedBy: "reviewer",
            conditions: ["Install the flow meter within 30 days", "Run a second CBL in 6 months"],
            handoverTo: "production");

        Assert.True(accepted.IsAccepted);
        Assert.False(accepted.IsEditable);

        var handover = accepted.Handover;

        Assert.NotNull(handover);
        Assert.Equal("WELL-1", handover!.WellId);
        Assert.Equal("reviewer", handover.AcceptedBy);
        Assert.Equal("production", handover.HandoverTo);
        Assert.Equal(2, handover.Conditions.Count);
        Assert.Contains("Install the flow meter", handover.ConditionsText, StringComparison.Ordinal);
        Assert.Contains("production", handover.Describe(), StringComparison.Ordinal);

        // A submission that was not accepted has no handover to read.
        Assert.Null(Submission(state: OgCompletionSubmission.Submitted).Handover);
        Assert.Null(Submission(state: OgCompletionSubmission.Rejected).Handover);
    }

    [Fact]
    public void A_test_shows_its_result_with_its_unit_or_says_it_is_missing()
    {
        var test = Test("PASS", result: "3500", resultUnit: "psi");
        var noUnit = Test("FAIL", result: "3500", resultUnit: null);

        Assert.True(test.IsPassed);
        Assert.Equal("pass", test.StatusText);
        Assert.Equal("3500 psi", test.ResultText);
        Assert.Contains("evidence CBL-1", test.Describe(), StringComparison.Ordinal);

        Assert.False(noUnit.IsPassed);
        Assert.Contains("(unit not recorded)", noUnit.ResultText, StringComparison.Ordinal);
    }

    [Fact]
    public void The_workspace_counts_the_tests_and_decides_whether_acceptance_is_possible()
    {
        var workspace = Workspace(tests: [Test("PASS"), Test("FAIL"), Test("PASS")], evidence: [Evidence("WELL-1")]);

        Assert.Equal(2, workspace.PassedTests);
        Assert.Equal(1, workspace.FailedTests);
        Assert.True(workspace.HasPassingTest);
        Assert.True(workspace.CanAccept);
        Assert.Empty(workspace.SubmissionGaps);
        Assert.Equal("2 passed · 1 failed", workspace.TestsText);

        var failing = Workspace(tests: [Test("FAIL")], evidence: [Evidence("WELL-1")]);

        Assert.False(failing.HasPassingTest);
        Assert.False(failing.CanAccept);

        var empty = Workspace(tests: [], evidence: []);

        Assert.Equal("no completion test is recorded", empty.TestsText);
        Assert.Equal(2, empty.SubmissionGaps.Count);
    }

    [Fact]
    public async Task No_connection_is_a_failure_not_an_empty_workspace()
    {
        var service = new OgWellCompletionService(() => null, new Quiet());

        var loaded = await service.LoadAsync("F-1", "Alpha");

        Assert.False(loaded.Success);
        Assert.Equal(OgWellCompletionService.CodeNoConnection, loaded.Reference);
        Assert.Null(loaded.Value);
    }

    [Fact]
    public async Task A_test_a_submission_and_a_decision_all_need_an_actor()
    {
        var service = new OgWellCompletionService(() => null, new Quiet());

        Assert.Equal(
            OgWellCompletionService.CodeActorRequired,
            (await service.RecordTestAsync("WELL-1", Values(), " ")).Reference);

        Assert.Equal(
            OgWellCompletionService.CodeWellRequired,
            (await service.RecordTestAsync(" ", Values(), "sara")).Reference);

        Assert.Equal(
            OgWellCompletionService.CodeActorRequired,
            (await service.CreateSubmissionAsync("WELL-1", null, " ")).Reference);

        Assert.Equal(
            OgWellCompletionService.CodeNothingPending,
            (await service.RetryDecisionAsync()).Reference);
    }

    private static OgCompletionTestValues Values(
        string? type = "PRESSURE_TEST",
        decimal? top = 2400m,
        decimal? bottom = 2500m,
        string? depthUnit = "m",
        string? status = "PASS",
        string? result = "3500",
        string? resultUnit = "psi") =>
        new(type, top, bottom, depthUnit, status, result, resultUnit, "sara", Day, "CBL-1");

    private static OgCompletionTest Test(string status, string? result = "3500", string? resultUnit = "psi") =>
        new(
            "T-1",
            "WELL-1",
            "P-1",
            "PRESSURE_TEST",
            2400m,
            2500m,
            "m",
            status,
            result,
            resultUnit,
            "sara",
            Day,
            "CBL-1");

    private static OgCompletionEvidence Evidence(string wellId) =>
        new("E-1", "S-1", wellId, "CBL LOG", "cbl-2026-09-01.pdf", "sara", Day);

    private static OgCompletionSubmission Submission(
        string state = OgCompletionSubmission.Submitted,
        string author = "sara",
        string? acceptedBy = null,
        IReadOnlyList<string>? conditions = null,
        string? handoverTo = null) =>
        new(
            "S-1",
            "WELL-1",
            "P-1",
            "Completion of WELL-1",
            state,
            author,
            Day,
            state == OgCompletionSubmission.Submitted ? null : "reviewer",
            null,
            conditions ?? [],
            acceptedBy is null ? null : Day,
            acceptedBy,
            handoverTo,
            handoverTo is null ? null : Day);

    private static OgCompletionWorkspace Workspace(
        IReadOnlyList<OgCompletionTest> tests,
        IReadOnlyList<OgCompletionEvidence> evidence) =>
        new(
            "Alpha field",
            [new OgCompletionWell("WELL-1", "P-1", "ACTIVE")],
            "WELL-1",
            tests,
            [Submission()],
            Submission(),
            evidence,
            null);

    private sealed class Quiet : IFailureReporter
    {
        public string ReportHandled(
            Exception exception,
            string operation,
            string consequence,
            FailureSeverity severity = FailureSeverity.Error) => FailureReferenceFormat.New();
    }
}
