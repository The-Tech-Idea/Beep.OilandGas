using Beep.OilandGas.Models.Constants;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Hse;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// HSE-04: the action's node-and-deviation rule, the guide-word and deviation validation, the legal transitions, the
/// unconfirmed acknowledgement and the study's own counts - the parts that need no database. The study, its rows and the
/// scope guard are proven in the gateway test.
/// </summary>
public sealed class HseHazopTests
{
    private const string PageFileName = "HazopPageView.cs";

    [Fact]
    public void An_action_needs_its_node_and_its_deviation()
    {
        // AC-4 and SG-11-5.
        Assert.Equal(HazopRules.CodeActionNeedsNodeAndDeviation,
            HazopRules.ValidateAction(null, 1, "Fit a guard", "omar", new DateTime(2026, 12, 1))!.Value.Code);
        Assert.Equal(HazopRules.CodeActionNeedsNodeAndDeviation,
            HazopRules.ValidateAction("S-1-NODE-001", null, "Fit a guard", "omar", new DateTime(2026, 12, 1))!.Value.Code);
        Assert.Equal(HazopRules.CodeActionNeedsNodeAndDeviation,
            HazopRules.ValidateAction("S-1-NODE-001", 0, "Fit a guard", "omar", new DateTime(2026, 12, 1))!.Value.Code);

        // With both, the rest of the contract applies.
        Assert.Null(HazopRules.ValidateAction("S-1-NODE-001", 1, "Fit a guard", "omar", new DateTime(2026, 12, 1)));
        Assert.Equal(HazopRules.CodeInputRequired,
            HazopRules.ValidateAction("S-1-NODE-001", 1, "Fit a guard", " ", new DateTime(2026, 12, 1))!.Value.Code);
        Assert.Equal(HazopRules.CodeInputRequired,
            HazopRules.ValidateAction("S-1-NODE-001", 1, "Fit a guard", "omar", null)!.Value.Code);
    }

    [Fact]
    public void A_deviation_needs_a_guide_word_a_parameter_and_a_consequence()
    {
        Assert.Null(HazopRules.ValidateDeviation("NO", "flow", "No flow to the separator"));

        Assert.Equal(HazopRules.CodeInputInvalid,
            HazopRules.ValidateDeviation("MAYBE", "flow", "No flow")!.Value.Code);
        Assert.Equal(HazopRules.CodeInputRequired,
            HazopRules.ValidateDeviation("NO", " ", "No flow")!.Value.Code);
        Assert.Equal(HazopRules.CodeInputRequired,
            HazopRules.ValidateDeviation("NO", "flow", " ")!.Value.Code);

        Assert.Contains("MORE", HazopRules.GuideWords);
        Assert.Contains("AS_WELL_AS", HazopRules.GuideWords);
    }

    [Fact]
    public void Only_legal_transitions_are_offered()
    {
        // AC-6: the study lifecycle DRAFT -> OPEN -> CLOSED, and nothing backwards.
        Assert.Null(HazopRules.CheckTransition(HseReferenceCodes.Hazop.Draft, HseReferenceCodes.Hazop.Open));
        Assert.Null(HazopRules.CheckTransition(HseReferenceCodes.Hazop.Open, HseReferenceCodes.Hazop.Closed));

        var backwards = HazopRules.CheckTransition(HseReferenceCodes.Hazop.Open, HseReferenceCodes.Hazop.Draft);

        Assert.Equal(HazopRules.CodeTransitionIllegal, backwards!.Value.Code);
        Assert.Contains(HseReferenceCodes.Hazop.Closed, backwards.Value.Message, StringComparison.Ordinal);

        Assert.Equal(HazopRules.CodeTransitionIllegal,
            HazopRules.CheckTransition(HseReferenceCodes.Hazop.Closed, HseReferenceCodes.Hazop.Open)!.Value.Code);

        Assert.Equal([HseReferenceCodes.Hazop.Open], HazopRules.NextStates(HseReferenceCodes.Hazop.Draft));
        Assert.Empty(HazopRules.NextStates(HseReferenceCodes.Hazop.Closed));

        // The deviation lifecycle is OPEN -> ACTION_REQUIRED -> CLOSED.
        Assert.Equal(
            [HseReferenceCodes.Hazop.ActionRequired, HseReferenceCodes.Hazop.Closed],
            HazopRules.NextDeviationStates(HseReferenceCodes.Hazop.Open));
        Assert.Equal([HseReferenceCodes.Hazop.Closed],
            HazopRules.NextDeviationStates(HseReferenceCodes.Hazop.ActionRequired));
        Assert.Empty(HazopRules.NextDeviationStates(HseReferenceCodes.Hazop.Closed));
    }

    [Fact]
    public void A_missing_or_mismatched_acknowledgement_is_unconfirmed()
    {
        // AC-2: the two cases are told apart from an illegal transition.
        var missing = HazopRules.CheckAcknowledged(null, HseReferenceCodes.Hazop.Draft, "study");

        Assert.Equal(HazopRules.CodeUnconfirmed, missing!.Value.Code);
        Assert.Contains("missing", missing.Value.Message, StringComparison.Ordinal);
        Assert.Contains("not applied", missing.Value.Message, StringComparison.Ordinal);

        var mismatch = HazopRules.CheckAcknowledged(
            HseReferenceCodes.Hazop.Draft, HseReferenceCodes.Hazop.Open, "study");

        Assert.Equal(HazopRules.CodeUnconfirmed, mismatch!.Value.Code);
        Assert.Contains(HseReferenceCodes.Hazop.Draft, mismatch.Value.Message, StringComparison.Ordinal);
        Assert.Contains(HseReferenceCodes.Hazop.Open, mismatch.Value.Message, StringComparison.Ordinal);

        Assert.Null(HazopRules.CheckAcknowledged(
            HseReferenceCodes.Hazop.Open, HseReferenceCodes.Hazop.Open, "study"));
    }

    [Fact]
    public void The_status_ids_are_the_owners_own_format()
    {
        // Matching these is what lets a status row be found and updated rather than duplicated.
        Assert.Equal("S-1-NODE-001-DEV-003", HazopRules.DeviationStatusId("S-1-NODE-001", 3m));
        Assert.Equal("S-1-STUDY", HazopRules.StudyStatusId("S-1"));
    }

    [Fact]
    public void The_study_says_what_it_holds()
    {
        var node = new OgHazopNode(
            "S-1-NODE-001", 1, "Separator inlet", "Take gas from the HP separator",
            [
                new OgHazopDeviation(1, "NO", "flow", "Blockage", "No flow to the separator", "Strainer",
                    "HIGH", HseReferenceCodes.Hazop.Open, null),
                new OgHazopDeviation(2, "MORE", "pressure", "Upset", "Overpressure", "PSV", "HIGH",
                    HseReferenceCodes.Hazop.ActionRequired,
                    new OgHazopAction("A-1", "Check the PSV", "omar", new DateTime(2026, 1, 1),
                        HseReferenceCodes.CorrectiveAction.Open, true)),
            ]);

        var study = new OgHazopStudy("S-1", "Separator HAZOP", "HP separator", "F-1", "FAC-1",
            HseReferenceCodes.Hazop.Open, new DateTime(2026, 10, 1), [node]);

        Assert.Equal(1, study.NodeCount);
        Assert.Equal(2, study.DeviationCount);
        Assert.Equal(1, study.OpenActions);
        Assert.Contains("Separator HAZOP", study.HeaderText, StringComparison.Ordinal);
        Assert.Contains("2 deviation(s)", study.HeaderText, StringComparison.Ordinal);
        Assert.Contains("NO flow", study.DeviationsText, StringComparison.Ordinal);
        Assert.Contains("(overdue)", study.DeviationsText, StringComparison.Ordinal);
    }

    [Fact]
    public void The_page_offers_only_the_legal_next_states()
    {
        var page = File.ReadAllText(
            SourceRoot.AllSources().Single(source => source.EndsWith(PageFileName, StringComparison.Ordinal)));

        // AC-6: the picker is bound from the workspace's legal set, and the transition sends the state it reviewed.
        Assert.Contains("workspace.LegalNextStates", page, StringComparison.Ordinal);
        Assert.Contains("workspace.Study.State", page, StringComparison.Ordinal);

        // AC-1: the study is opened from the id field, not from a list or a "latest".
        Assert.Contains("Enter the recorded study id", page, StringComparison.Ordinal);

        // AC-4: the action command is enabled only with a node and a deviation selected.
        Assert.Contains("_node is not null && _deviation is not null", page, StringComparison.Ordinal);
    }

    private static string SourceOf(string file) =>
        SourceRoot.AllSources().Single(source => source.EndsWith(file, StringComparison.Ordinal));
}
