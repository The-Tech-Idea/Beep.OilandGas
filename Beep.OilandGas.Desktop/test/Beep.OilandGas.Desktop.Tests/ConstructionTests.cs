using TheTechIdea.Beep.OilandGas.Desktop.Data.Development;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// DEV-08: the completion rule, the overdue arithmetic, the unresolved-submission rule, the handover rules, the
/// equipment wording and the version revalidation - the parts that do not need a database. The persistence, the
/// foreign facility and the after-write revalidation are proven in the gateway test.
/// </summary>
public sealed class ConstructionTests
{
    private static readonly DateTime AsOf = new(2026, 10, 1);

    [Fact]
    public void A_milestone_cannot_complete_without_evidence()
    {
        var noEvidence = Milestone("COMMISSIONING", state: OgConstructionMilestone.InProgress, evidence: []);
        var rejected = Milestone(
            "COMMISSIONING",
            evidence: [Evidence("REJECTED", "commissioning report")]);
        var accepted = Milestone(
            "COMMISSIONING",
            evidence: [Evidence("ACCEPTED", "commissioning report")]);

        Assert.False(noEvidence.CanComplete);
        Assert.Contains("no evidence is recorded", Assert.Single(noEvidence.CompletionGaps), StringComparison.Ordinal);

        Assert.False(rejected.CanComplete);
        Assert.Contains("was rejected", Assert.Single(rejected.CompletionGaps), StringComparison.Ordinal);

        Assert.True(accepted.CanComplete);
        Assert.Empty(accepted.CompletionGaps);
        Assert.Equal("ready to complete", accepted.CompletionText);
    }

    [Fact]
    public void The_newest_evidence_decides_even_when_an_older_item_was_accepted()
    {
        var milestone = Milestone(
            "INSTALLATION",
            evidence:
            [
                Evidence("ACCEPTED", "as-built drawing", day: AsOf.AddDays(-10)),
                Evidence("REJECTED", "re-inspection required", day: AsOf.AddDays(-1)),
            ]);

        Assert.True(milestone.LatestEvidence!.IsRejected);
        Assert.False(milestone.CanComplete);
        Assert.Contains("re-inspection required", Assert.Single(milestone.CompletionGaps), StringComparison.Ordinal);
    }

    [Fact]
    public void Overdue_is_computed_from_the_planned_date_and_never_stored()
    {
        var late = Milestone("FABRICATION", planned: AsOf.AddDays(-5), state: OgConstructionMilestone.InProgress);
        var doneLate = Milestone("FABRICATION", planned: AsOf.AddDays(-5), state: OgConstructionMilestone.Complete);
        var future = Milestone("FABRICATION", planned: AsOf.AddDays(5));
        var none = Milestone("FABRICATION", planned: null);

        Assert.True(late.IsOverdue(AsOf));
        Assert.Equal(5, late.DaysOverdue(AsOf));
        Assert.Contains("5 d overdue", late.PlanText(AsOf), StringComparison.Ordinal);

        Assert.False(doneLate.IsOverdue(AsOf));
        Assert.Null(doneLate.DaysOverdue(AsOf));

        Assert.False(future.IsOverdue(AsOf));
        Assert.False(none.IsOverdue(AsOf));

        // The same milestone is overdue on a later date and not on an earlier one.
        var onTime = Milestone("INSTALLATION", planned: AsOf.AddDays(3));
        Assert.False(onTime.IsOverdue(AsOf));
        Assert.True(onTime.IsOverdue(AsOf.AddDays(10)));
    }

    [Fact]
    public void An_unresolved_submission_hides_the_create_and_edit_controls()
    {
        var rejected = Milestone("PROCUREMENT", evidence: [Evidence("REJECTED", "FAT certificate")]);
        var facility = Facility(milestones: [rejected, Milestone("DESIGN", state: OgConstructionMilestone.Complete)]);

        Assert.False(facility.CanEdit);
        Assert.NotNull(facility.UnresolvedReason);
        Assert.Contains("unresolved", facility.UnresolvedReason!, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("FAT certificate", facility.UnresolvedReason!, StringComparison.Ordinal);

        var resolved = Facility(milestones:
        [
            Milestone("DESIGN", evidence: [Evidence("ACCEPTED", "signed drawing")]),
            Milestone("PROCUREMENT", evidence: [Evidence("ACCEPTED_WITH_NOTES", "FAT certificate")]),
        ]);

        Assert.True(resolved.CanEdit);
        Assert.Null(resolved.UnresolvedReason);
    }

    [Fact]
    public void A_handed_over_facility_cannot_be_edited_any_more()
    {
        var facility = Facility(
            milestones: [Milestone("HANDOVER", state: OgConstructionMilestone.Complete, evidence: [Evidence("ACCEPTED", "handover pack")])],
            handover: Handover(conditions: []));

        Assert.True(facility.IsHandedOver);
        Assert.False(facility.CanEdit);
        Assert.False(facility.CanHandover);
        Assert.Contains("already handed over", Assert.Single(facility.HandoverGaps), StringComparison.Ordinal);
    }

    [Fact]
    public void Handover_needs_every_milestone_complete_and_its_signature()
    {
        var incomplete = Facility(milestones:
        [
            Milestone("DESIGN", state: OgConstructionMilestone.Complete),
            Milestone("INSTALLATION", state: OgConstructionMilestone.InProgress),
        ]);

        Assert.False(incomplete.CanHandover);
        Assert.Contains("not complete: installation", Assert.Single(incomplete.HandoverGaps), StringComparison.Ordinal);

        var complete = Facility(milestones:
        [
            Milestone("DESIGN", state: OgConstructionMilestone.Complete),
            Milestone("INSTALLATION", state: OgConstructionMilestone.Complete),
        ]);

        Assert.True(complete.CanHandover);

        // AC-4: the handover records recipient, date, conditions and the signature it was signed under.
        Assert.Equal(
            ConstructionRules.CodeRecipientRequired,
            ConstructionRules.ValidateHandover(complete, Values() with { HandedOverTo = " " })!.Value.Code);

        Assert.Equal(
            ConstructionRules.CodeHandoverDateRequired,
            ConstructionRules.ValidateHandover(complete, Values() with { HandedOverAt = null })!.Value.Code);

        Assert.Equal(
            ConstructionRules.CodeSignatureRequired,
            ConstructionRules.ValidateHandover(complete, Values() with { Signature = " " })!.Value.Code);

        Assert.Null(ConstructionRules.ValidateHandover(complete, Values()));

        Assert.Equal(
            ConstructionRules.CodeMilestonesIncomplete,
            ConstructionRules.ValidateHandover(incomplete, Values())!.Value.Code);

        var record = Handover(conditions: ["Connect the flow line within 14 days", "Hand over the spare parts list"]);

        Assert.Equal(2, record.Conditions.Count);
        Assert.Contains("Connect the flow line within 14 days", record.Describe(), StringComparison.Ordinal);
        Assert.Contains("operations", record.Describe(), StringComparison.Ordinal);
        Assert.True(record.Accepted);
    }

    [Fact]
    public void An_unconditional_handover_says_it_carries_no_conditions()
    {
        Assert.Equal("no conditions", Handover(conditions: []).ConditionsText);
        Assert.Equal(
            "a; b",
            Handover(conditions: ["a", "b"]).ConditionsText);
    }

    [Fact]
    public void Recorded_equipment_is_shown_with_the_claim_it_does_not_make()
    {
        Assert.Contains("not evidence", ConstructionRules.EquipmentSentence, StringComparison.Ordinal);
        Assert.Contains("tested", ConstructionRules.EquipmentSentence, StringComparison.Ordinal);
        Assert.Contains("accepted", ConstructionRules.EquipmentSentence, StringComparison.Ordinal);

        var withEquipment = Facility(equipment: [new OgEquipmentRecord("EQ-1", true, AsOf.AddDays(-30), null, "16\" gate valve", null)]);
        var without = Facility(equipment: []);

        Assert.Contains("1 equipment item(s) recorded", withEquipment.EquipmentText, StringComparison.Ordinal);
        Assert.Contains(ConstructionRules.EquipmentSentence, withEquipment.EquipmentText, StringComparison.Ordinal);
        Assert.Contains("No equipment is recorded", without.EquipmentText, StringComparison.Ordinal);
        Assert.Contains(ConstructionRules.EquipmentSentence, without.EquipmentText, StringComparison.Ordinal);

        var item = new OgEquipmentRecord("EQ-1", true, AsOf.AddDays(-30), null, "16\" gate valve", "spare");
        Assert.Equal("active", item.StateText);
        Assert.Equal("2026-09-01", item.InstallText);
        Assert.Equal("(not removed)", item.RemoveText);
        Assert.Contains("16\" gate valve", item.Describe(), StringComparison.Ordinal);
    }

    [Fact]
    public void A_milestone_name_and_evidence_are_validated_against_the_plan_s_own_lists()
    {
        Assert.Equal(
            ConstructionRules.CodeMilestoneNameRequired,
            ConstructionRules.ValidateMilestone(new OgMilestoneValues(" ", AsOf))!.Value.Code);

        Assert.Equal(
            ConstructionRules.CodeMilestoneNameInvalid,
            ConstructionRules.ValidateMilestone(new OgMilestoneValues("DEMOLITION", AsOf))!.Value.Code);

        Assert.Null(ConstructionRules.ValidateMilestone(new OgMilestoneValues("commissioning", AsOf)));

        Assert.Equal(6, ConstructionRules.MilestoneNames.Count);
        Assert.Equal(1, ConstructionRules.SequenceOf("DESIGN"));
        Assert.Equal(6, ConstructionRules.SequenceOf("HANDOVER"));
        Assert.Equal(0, ConstructionRules.SequenceOf("NOPE"));

        Assert.Equal(
            ConstructionRules.CodeEvidenceRequired,
            ConstructionRules.ValidateEvidence(new OgMilestoneEvidenceValues(" ", null, AsOf, "sara", "ACCEPTED"))!.Value.Code);

        Assert.Equal(
            ConstructionRules.CodeOutcomeInvalid,
            ConstructionRules.ValidateEvidence(new OgMilestoneEvidenceValues("FAT", null, AsOf, "sara", "MAYBE"))!.Value.Code);

        Assert.Equal(
            ConstructionRules.CodeReviewerRequired,
            ConstructionRules.ValidateEvidence(new OgMilestoneEvidenceValues("FAT", null, AsOf, " ", "ACCEPTED"))!.Value.Code);

        Assert.Null(ConstructionRules.ValidateEvidence(new OgMilestoneEvidenceValues(null, "doc-1", AsOf, "sara", "ACCEPTED")));
    }

    [Fact]
    public void The_facility_state_is_the_first_unfinished_milestone_until_it_is_handed_over()
    {
        var notTracked = Facility(milestones: []);
        var inProgress = Facility(milestones:
        [
            Milestone("DESIGN", sequence: 1, state: OgConstructionMilestone.Complete),
            Milestone("PROCUREMENT", sequence: 2, state: OgConstructionMilestone.InProgress),
        ]);
        var ready = Facility(milestones:
        [
            Milestone("DESIGN", sequence: 1, state: OgConstructionMilestone.Complete),
            Milestone("PROCUREMENT", sequence: 2, state: OgConstructionMilestone.Complete),
        ]);

        Assert.Equal("not tracked", notTracked.StateText);
        Assert.Equal("procurement", inProgress.StateText);
        Assert.Equal("1 of 2 milestone(s) complete", inProgress.CompletionText);
        Assert.Equal("ready for handover", ready.StateText);
        Assert.Equal("handed over", Facility(
            milestones: [Milestone("DESIGN", state: OgConstructionMilestone.Complete)],
            handover: Handover(conditions: [])).StateText);
    }

    [Fact]
    public void Spend_is_compared_with_the_budget_and_states_when_it_cannot_be()
    {
        var over = Facility(budget: 80m, actual: 95m, currency: "USD");
        var under = Facility(budget: 80m, actual: 75m, currency: "USD");
        var none = Facility(budget: 80m, actual: null, currency: "USD");
        var noBudget = Facility(budget: null, actual: 10m, currency: "USD");

        Assert.Equal("95 USD", over.SpendText);
        Assert.Contains("15 over budget", over.VarianceText, StringComparison.Ordinal);
        Assert.Contains("5 under budget", under.VarianceText, StringComparison.Ordinal);
        Assert.Equal("no actual spend recorded", none.SpendText);
        Assert.Equal("variance not computable", none.VarianceText);
        Assert.Equal("no budget recorded", noBudget.BudgetText);
    }

    [Fact]
    public void The_workspace_says_how_many_facilities_and_which_are_overdue()
    {
        var overdue = Facility(
            milestones: [Milestone("FABRICATION", planned: AsOf.AddDays(-2))]);
        var onTime = Facility(milestones: [Milestone("FABRICATION", planned: AsOf.AddDays(9))]);

        var workspace = new OgConstructionWorkspace(
            "Alpha field",
            [overdue, onTime],
            overdue,
            [],
            AsOf);

        Assert.Equal(1, workspace.OverdueFacilities);
        Assert.Contains("2 facility/facilities", workspace.Summary, StringComparison.Ordinal);
        Assert.Contains("1 with an overdue milestone", workspace.Summary, StringComparison.Ordinal);
        Assert.Contains("well completion has been accepted", workspace.UpstreamText, StringComparison.Ordinal);
    }

    [Fact]
    public void The_upstream_well_handover_is_shown_with_its_conditions()
    {
        var workspace = new OgConstructionWorkspace(
            "Alpha field",
            [],
            null,
            [new OgUpstreamHandover("WELL-1", "reviewer", AsOf, ["Install the flow meter within 30 days"])],
            AsOf);

        Assert.Contains("WELL-1 accepted by reviewer", workspace.UpstreamText, StringComparison.Ordinal);
        Assert.Contains("Install the flow meter within 30 days", workspace.UpstreamText, StringComparison.Ordinal);
    }

    [Fact]
    public void A_facility_that_moved_on_is_refused_before_it_is_written_to()
    {
        Assert.Null(ConstructionRules.VersionRefusal(3, 3));
        Assert.Null(ConstructionRules.VersionRefusal(3, null));

        var refusal = ConstructionRules.VersionRefusal(4, 3);

        Assert.NotNull(refusal);
        Assert.Contains("plan version 3", refusal!, StringComparison.Ordinal);
        Assert.Contains("to 4", refusal!, StringComparison.Ordinal);
    }

    [Fact]
    public async Task No_connection_is_a_failure_not_an_empty_facility_list()
    {
        var service = new OgConstructionService(() => null, new Quiet());

        var loaded = await service.LoadAsync("F-1", "Alpha");

        Assert.False(loaded.Success);
        Assert.Equal(OgConstructionService.CodeNoConnection, loaded.Reference);
        Assert.Null(loaded.Value);
    }

    [Fact]
    public async Task Every_write_needs_an_actor_and_a_facility()
    {
        var service = new OgConstructionService(() => null, new Quiet());

        Assert.Equal(
            OgConstructionService.CodeActorRequired,
            (await service.CreateMilestoneAsync("FI-1", new OgMilestoneValues("DESIGN", AsOf), " ")).Reference);

        Assert.Equal(
            OgConstructionService.CodeFacilityRequired,
            (await service.CreateMilestoneAsync(" ", new OgMilestoneValues("DESIGN", AsOf), "sara")).Reference);

        Assert.Equal(
            OgConstructionService.CodeActorRequired,
            (await service.HandoverAsync("FI-1", Values(), " ")).Reference);

        Assert.Equal(
            OgConstructionService.CodeFacilityRequired,
            (await service.HandoverAsync(" ", Values(), "sara")).Reference);

        Assert.Equal(
            ConstructionRules.CodeMilestoneNameInvalid,
            (await service.CreateMilestoneAsync("FI-1", new OgMilestoneValues("DEMOLITION", AsOf), "sara")).Reference);
    }

    private static OgMilestoneEvidence Evidence(string outcome, string document, DateTime? day = null) =>
        new("E-1", "M-1", document, "ref-1", day ?? AsOf.AddDays(-3), "reviewer", outcome, "sara", AsOf.AddDays(-3));

    private static OgConstructionMilestone Milestone(
        string name,
        int sequence = 1,
        DateTime? planned = null,
        string state = OgConstructionMilestone.NotStarted,
        IReadOnlyList<OgMilestoneEvidence>? evidence = null) =>
        new(
            "M-" + name,
            "FI-1",
            "F-1",
            name,
            sequence,
            planned,
            state == OgConstructionMilestone.Complete ? AsOf.AddDays(-1) : null,
            state,
            null,
            evidence ?? []);

    private static OgFacilityHandover Handover(IReadOnlyList<string> conditions) =>
        new(
            "H-1",
            "FI-1",
            "F-1",
            "operations",
            AsOf,
            conditions,
            "signed-2026-10-01",
            "reviewer",
            AsOf);

    private static OgConstructionFacility Facility(
        IReadOnlyList<OgConstructionMilestone>? milestones = null,
        OgFacilityHandover? handover = null,
        IReadOnlyList<OgEquipmentRecord>? equipment = null,
        decimal? budget = null,
        decimal? actual = null,
        string? currency = null) =>
        new(
            "FI-1",
            "F-1",
            "F-1",
            "Central processing facility",
            "CPF",
            "CONSTRUCTION",
            "FDP-1",
            3,
            1000m,
            "BOPD",
            budget,
            actual,
            currency,
            AsOf.AddMonths(-6),
            AsOf.AddMonths(6),
            milestones ?? [],
            handover,
            equipment ?? []);

    private static OgFacilityHandoverValues Values() =>
        new("operations", AsOf, ["one condition"], "signed-2026-10-01", "reviewer");

    private sealed class Quiet : IFailureReporter
    {
        public string ReportHandled(
            Exception exception,
            string operation,
            string consequence,
            FailureSeverity severity = FailureSeverity.Error) => FailureReferenceFormat.New();
    }
}
