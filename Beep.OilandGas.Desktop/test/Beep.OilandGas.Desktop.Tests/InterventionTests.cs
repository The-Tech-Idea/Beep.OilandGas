using TheTechIdea.Beep.OilandGas.Desktop.Data.Production;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// PRD-06: the diagnosis rules (insufficient data, conflicts, confidence), the option rules (basis, gain unit, method in
/// the reference set) and the decision rules (rationale, owner, target, chosen option) - the parts that do not need a
/// database. The PPDM status/rate/deferment/variance reads, the saved decision and the lost work order are proven in the
/// gateway test.
/// </summary>
public sealed class InterventionTests
{
    private static readonly DateTime Day = new(2026, 10, 1);
    private static readonly string[] Methods = ["GAS_LIFT", "ESP", "ACIDIZE"];

    [Fact]
    public void A_diagnosis_without_evidence_says_insufficient_data_and_names_what_is_missing()
    {
        var bare = Well(
            evidence: [],
            missing:
            [
                "no measured volume for the well",
                "no open deferment",
                "no material variance this month",
                "no status recorded",
            ]);

        Assert.True(bare.IsInsufficient);
        Assert.Contains("insufficient data", bare.DiagnosisText, StringComparison.Ordinal);
        Assert.Contains("no measured volume for the well", bare.DiagnosisText, StringComparison.Ordinal);
        Assert.Contains("no status recorded", bare.DiagnosisText, StringComparison.Ordinal);
        Assert.Equal(0m, bare.Confidence);
        Assert.False(bare.IsUnderperforming);

        // One evidenced check is no longer "insufficient data", and it is diagnosed on that evidence.
        var evidenced = Well(
            evidence: [new OgInterventionEvidence("STATUS", "the recorded status is producing", OgInterventionEvidence.Positive)],
            missing: ["no open deferment"]);

        Assert.False(evidenced.IsInsufficient);
        Assert.Equal("no underperformance is evidenced", evidenced.DiagnosisText);
        Assert.Empty(evidenced.Flags);
    }

    [Fact]
    public void Each_evidenced_check_adds_a_quarter_of_confidence_and_a_conflict_caps_it()
    {
        var one = Well(evidence: [Evidence("RATE")], missing: []);
        var four = Well(
            evidence: [Evidence("RATE"), Evidence("DEFERMENT"), Evidence("VARIANCE"), Evidence("STATUS")],
            missing: []);

        Assert.Equal(0.25m, one.Confidence);
        Assert.Equal("25%", one.ConfidenceText);
        Assert.Equal(1m, four.Confidence);
        Assert.Equal("100%", four.ConfidenceText);

        // The state table's conflicting evidence: both shown, nothing resolved, and the confidence is capped.
        var conflicting = Well(
            evidence: [Evidence("RATE", OgInterventionEvidence.Positive), Evidence("DEFERMENT")],
            missing: [],
            conflicts: ["the well is recorded as down but measured above its own average; both are shown"]);

        Assert.True(conflicting.HasConflicts);
        Assert.Equal(0.5m, conflicting.Confidence);
        Assert.Contains("capped", conflicting.ConfidenceText, StringComparison.Ordinal);
        Assert.Contains("conflicting evidence", conflicting.DiagnosisText, StringComparison.Ordinal);
        Assert.Contains("measured above its own average", conflicting.DiagnosisText, StringComparison.Ordinal);
    }

    [Fact]
    public void An_underperforming_well_is_flagged_by_what_the_earlier_features_recorded()
    {
        var declining = Well(
            evidence: [Evidence("RATE")],
            missing: [],
            flags: ["the rate fell 40 bbl below its own average"],
            change: -40m);

        Assert.True(declining.IsUnderperforming);
        Assert.Contains("the rate fell", declining.FlagsText, StringComparison.Ordinal);
        Assert.Equal("-40 bbl/d", declining.ChangeText);
        Assert.Equal("100 bbl/d (7 day(s))", declining.AverageText);

        Assert.Equal("not computable", Well(evidence: [], missing: [], change: null).ChangeText);
    }

    [Fact]
    public void An_option_without_a_basis_is_refused_and_so_is_an_unknown_basis()
    {
        // AC-2 and the state table's "option without basis is not selectable".
        Assert.Equal(
            InterventionRules.CodeBasisRequired,
            InterventionRules.Validate(Decision(options: [Option(basis: null)]), Methods)!.Value.Code);

        Assert.Equal(
            InterventionRules.CodeBasisInvalid,
            InterventionRules.Validate(Decision(options: [Option(basis: "GUESS")]), Methods)!.Value.Code);

        Assert.Null(InterventionRules.Validate(
            Decision(options: [Option(basis: InterventionRules.BasisAnalogue)]), Methods));
    }

    [Fact]
    public void An_estimated_gain_carries_its_unit()
    {
        Assert.Equal(
            InterventionRules.CodeGainUnitRequired,
            InterventionRules.Validate(Decision(options: [Option(gain: 120m, gainUnit: null)]), Methods)!.Value.Code);

        Assert.Equal(
            InterventionRules.CodeGainInvalid,
            InterventionRules.Validate(Decision(options: [Option(gain: -1m, gainUnit: "bbl/d")]), Methods)!.Value.Code);

        Assert.Null(InterventionRules.Validate(
            Decision(options: [Option(gain: 120m, gainUnit: "bbl/d")]), Methods));

        // No gain at all is allowed: the option may only know its basis.
        Assert.Null(InterventionRules.Validate(Decision(options: [Option(gain: null, gainUnit: null)]), Methods));
    }

    [Fact]
    public void An_option_method_comes_from_the_reference_set_and_is_not_repeated()
    {
        Assert.Equal(
            InterventionRules.CodeMethodRequired,
            InterventionRules.Validate(Decision(options: [Option(method: " ")]), Methods)!.Value.Code);

        Assert.Equal(
            InterventionRules.CodeMethodUnknown,
            InterventionRules.Validate(Decision(options: [Option(method: "TELEPORT")]), Methods)!.Value.Code);

        // An empty reference set is its own refusal: the code book is the operator's data.
        Assert.Equal(
            InterventionRules.CodeMethodReferenceMissing,
            InterventionRules.Validate(Decision(options: [Option()]), [])!.Value.Code);

        Assert.Equal(
            InterventionRules.CodeMethodDuplicate,
            InterventionRules.Validate(
                Decision(options: [Option(method: "ESP"), Option(method: "esp")]), Methods)!.Value.Code);

        Assert.Equal(
            InterventionRules.CodeNoOptions,
            InterventionRules.Validate(Decision(options: []), Methods)!.Value.Code);
    }

    [Fact]
    public void A_decision_records_its_rationale_owner_target_and_chosen_option()
    {
        // AC-4.
        Assert.Equal(
            InterventionRules.CodeRationaleRequired,
            InterventionRules.Validate(Decision(rationale: " "), Methods)!.Value.Code);

        Assert.Equal(
            InterventionRules.CodeOwnerRequired,
            InterventionRules.Validate(Decision(owner: null), Methods)!.Value.Code);

        Assert.Equal(
            InterventionRules.CodeTargetRequired,
            InterventionRules.Validate(Decision() with { TargetDate = null }, Methods)!.Value.Code);

        Assert.Equal(
            InterventionRules.CodeWellRequired,
            InterventionRules.Validate(Decision(well: " "), Methods)!.Value.Code);

        Assert.Equal(
            InterventionRules.CodeChosenRequired,
            InterventionRules.Validate(Decision(chosen: null), Methods)!.Value.Code);

        Assert.Equal(
            InterventionRules.CodeChosenUnknown,
            InterventionRules.Validate(Decision(chosen: "ACIDIZE"), Methods)!.Value.Code);

        Assert.Null(InterventionRules.Validate(Decision(chosen: "GAS_LIFT"), Methods));
    }

    [Fact]
    public void A_risk_level_is_one_of_three_and_a_cost_cannot_be_negative()
    {
        Assert.Equal(
            InterventionRules.CodeRiskInvalid,
            InterventionRules.Validate(Decision(options: [Option(risk: "MAYBE")]), Methods)!.Value.Code);

        Assert.Equal(
            InterventionRules.CodeCostInvalid,
            InterventionRules.Validate(Decision(options: [Option(cost: -5m)]), Methods)!.Value.Code);

        Assert.Null(InterventionRules.Validate(Decision(options: [Option(risk: "high")]), Methods));
    }

    [Fact]
    public void A_decision_shows_the_work_order_it_carries_or_says_it_has_none()
    {
        var open = DecisionRecord();
        var acted = DecisionRecord(workOrderRef: "WO-1234", state: OgInterventionDecision.ActedState);

        Assert.False(open.HasWorkOrder);
        Assert.Equal("no work order", open.WorkOrderLink);

        // AC-5: the work order reference is read together with the decision it was raised from.
        Assert.True(acted.HasWorkOrder);
        Assert.True(acted.IsActed);
        Assert.Contains("decision D-1", acted.WorkOrderLink, StringComparison.Ordinal);
        Assert.Contains("WO-1234", acted.WorkOrderLink, StringComparison.Ordinal);
        Assert.Equal("acted", acted.StateText);
    }

    [Fact]
    public void An_option_says_its_basis_with_the_evidence_and_the_method_by_name()
    {
        var option = OptionRecord();

        Assert.True(option.HasBasis);
        Assert.Contains("analogue", option.BasisDescription, StringComparison.Ordinal);
        Assert.Contains("WELL-7", option.BasisDescription, StringComparison.Ordinal);
        Assert.Contains("Gas lift", option.MethodText, StringComparison.Ordinal);
        Assert.Contains("chosen", option.Describe(), StringComparison.Ordinal);
        Assert.Equal("120 bbl/d", option.GainText);
        Assert.Contains("USD", option.CostText, StringComparison.Ordinal);
    }

    [Fact]
    public void The_workspace_counts_the_flagged_insufficient_and_conflicting_wells()
    {
        var workspace = new OgInterventionWorkspace(
            "Alpha field",
            "F-1",
            Day,
            [
                Well("WELL-1", flags: ["a deferment is open"], evidence: [Evidence("DEFERMENT")], missing: []),
                Well("WELL-2", evidence: [], missing: ["no status recorded"]),
                Well("WELL-3", evidence: [Evidence("RATE")], missing: [],
                    conflicts: ["recorded as down but measured above its average"]),
            ],
            null,
            [DecisionRecord()],
            Methods,
            false,
            InterventionRules.Risks);

        Assert.Equal(1, workspace.UnderperformingCount);
        Assert.Equal(1, workspace.InsufficientCount);
        Assert.Equal(1, workspace.ConflictCount);
        Assert.Equal(1, workspace.OpenDecisionCount);
        Assert.Contains("3 well(s)", workspace.Summary, StringComparison.Ordinal);
        Assert.Contains("1 flagged", workspace.Summary, StringComparison.Ordinal);
        Assert.Contains("GAS_LIFT", workspace.MethodsText, StringComparison.Ordinal);
    }

    [Fact]
    public void A_lift_method_says_the_lift_design_belongs_to_LFT_01()
    {
        var sentence = InterventionRules.LiftSentence("GAS_LIFT");

        Assert.Contains("LFT-01", sentence, StringComparison.Ordinal);
        Assert.Contains("does not compute a lift design", sentence, StringComparison.Ordinal);
    }

    [Fact]
    public void A_workspace_with_no_method_says_which_table_to_fill()
    {
        var workspace = new OgInterventionWorkspace(
            "Alpha field", "F-1", Day, [], null, [], [], true, InterventionRules.Risks);

        Assert.True(workspace.MethodReferenceMissing);
        Assert.Contains("R_INTERVENTION_METHOD", workspace.MethodsText, StringComparison.Ordinal);
    }

    [Fact]
    public async Task No_connection_is_a_failure_and_nothing_is_pending()
    {
        var service = new OgInterventionService(() => null, new Quiet());

        var loaded = await service.LoadAsync("F-1", "Alpha");

        Assert.False(loaded.Success);
        Assert.Equal(InterventionRules.CodeNoConnection, loaded.Reference);
        Assert.Null(loaded.Value);
        Assert.Null(service.Pending);

        Assert.Equal(
            InterventionRules.CodeNothingPending,
            (await service.RetryWorkOrderAsync("WO-1", "sara")).Reference);
    }

    [Fact]
    public async Task A_decision_and_a_work_order_both_need_an_actor()
    {
        var service = new OgInterventionService(() => null, new Quiet());

        Assert.Equal(
            InterventionRules.CodeActorRequired,
            (await service.RecordDecisionAsync(Decision(), " ")).Reference);

        Assert.Equal(
            InterventionRules.CodeActorRequired,
            (await service.CreateWorkOrderAsync("D-1", "WO-1", " ")).Reference);

        Assert.Equal(
            InterventionRules.CodeWorkOrderRequired,
            (await service.CreateWorkOrderAsync("D-1", " ", "sara")).Reference);
    }

    // ------------------------------------------------------------------ helpers

    private static OgInterventionEvidence Evidence(string kind, string direction = OgInterventionEvidence.Negative) =>
        new(kind, $"{kind.ToLowerInvariant()} evidence", direction);

    private static OgInterventionWell Well(
        string wellId = "WELL-1",
        IReadOnlyList<OgInterventionEvidence>? evidence = null,
        IReadOnlyList<string>? missing = null,
        IReadOnlyList<string>? conflicts = null,
        IReadOnlyList<string>? flags = null,
        decimal? change = null) =>
        new(
            wellId,
            wellId,
            "PRODUCING",
            Day.AddMonths(-3),
            60m,
            100m,
            "bbl/d",
            change,
            7,
            evidence ?? [],
            missing ?? [],
            conflicts ?? [],
            InterventionRules.Confidence((evidence ?? []).Count, (conflicts ?? []).Count > 0),
            flags ?? [],
            0);

    private static OgInterventionOptionValues Option(
        string? method = "GAS_LIFT",
        decimal? gain = null,
        string? gainUnit = null,
        decimal? cost = null,
        string? risk = "MEDIUM",
        string? basis = "TEST") =>
        new(method, gain, gainUnit, cost, "USD", risk, basis, "gas lift valve test on WELL-7");

    private static OgInterventionDecisionValues Decision(
        string? well = "WELL-1",
        IReadOnlyList<OgInterventionOptionValues>? options = null,
        string? chosen = "GAS_LIFT",
        string? rationale = "the rate fell 40% and the valve test is positive",
        string? owner = "sara",
        DateTime? target = null) =>
        new(
            "F-1",
            well,
            options ?? [Option()],
            chosen,
            rationale,
            owner,
            target ?? Day.AddDays(30));

    private static OgInterventionOption OptionRecord() =>
        new(
            "O-1",
            "GAS_LIFT",
            "Gas lift",
            120m,
            "bbl/d",
            250_000m,
            "USD",
            "LOW",
            InterventionRules.BasisAnalogue,
            "the same completion on WELL-7 gained 120 bbl/d",
            true,
            true);

    private static OgInterventionDecision DecisionRecord(
        string state = OgInterventionDecision.DecidedState,
        string? workOrderRef = null) =>
        new(
            "D-1",
            "WELL-1",
            "Well one",
            state,
            "the rate fell 40% and the valve test is positive",
            "sara",
            Day.AddDays(30),
            workOrderRef,
            workOrderRef is null ? null : Day,
            workOrderRef is null ? null : "sara",
            [OptionRecord()],
            Day);

    private sealed class Quiet : IFailureReporter
    {
        public string ReportHandled(
            Exception exception,
            string operation,
            string consequence,
            FailureSeverity severity = FailureSeverity.Error) => FailureReferenceFormat.New();
    }
}
