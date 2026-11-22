using TheTechIdea.Beep.OilandGas.Desktop.Data.Interop;
using TheTechIdea.OilGas.Wellbore.Setup;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// INT-05: the rules the wizard states - the engine's three states in the plan's vocabulary (AC-1's state reporting), the value a
/// setting is written as, and the outstanding-check list the hand-over depends on (AC-5). The engine itself is exercised in the
/// opt-in test, because it works on a real company folder.
/// </summary>
public sealed class WellboreSetupTests
{
    private const string PageFileName = "WellboreSetupPageView.cs";

    [Fact]
    public void The_engines_three_states_are_named_in_the_plans_vocabulary()
    {
        // The engine judges a step rather than tracking a cursor: complete is Done, something to resolve is Blocked with the
        // engine's own detail as the reason, and nothing to judge yet is NotStarted.
        Assert.Equal(OgSetupRules.Done, OgSetupRules.StateOf(SetupStepState.Done));
        Assert.Equal(OgSetupRules.Blocked, OgSetupRules.StateOf(SetupStepState.NeedsAttention));
        Assert.Equal(OgSetupRules.NotStarted, OgSetupRules.StateOf(SetupStepState.NotStarted));

        // The plan names four states; the fourth is the page's, not the engine's, and is still available to a screen by name.
        Assert.Equal(["NotStarted", "InProgress", "Done", "Blocked"], OgSetupRules.States);
        Assert.DoesNotContain(OgSetupRules.InProgress,
            new[] { SetupStepState.Done, SetupStepState.NeedsAttention, SetupStepState.NotStarted }
                .Select(OgSetupRules.StateOf));
    }

    [Fact]
    public void A_written_value_is_json_when_it_is_shaped_like_json_and_text_when_it_is_not()
    {
        // The shape decides, so an ordinary company name never goes near the parser.
        Assert.False(OgSetupRules.LooksLikeJson("Probe Energy"));
        Assert.False(OgSetupRules.LooksLikeJson("en-GB"));
        Assert.True(OgSetupRules.LooksLikeJson("72"));
        Assert.True(OgSetupRules.LooksLikeJson("true"));
        Assert.True(OgSetupRules.LooksLikeJson("""{ "a": 1 }"""));
        Assert.True(OgSetupRules.LooksLikeJson("[1,2]"));
        Assert.True(OgSetupRules.LooksLikeJson("\"quoted\""));
        Assert.False(OgSetupRules.LooksLikeJson(string.Empty));
    }

    [Fact]
    public void An_outstanding_check_is_named_by_the_hand_over_refusal()
    {
        // AC-5: the failing cases, with the reason each gave, are what the refusal lists.
        var verification = new List<OgVerificationRow>
        {
            new("Trajectory from LAS", "dataSource", Passed: true, null, []),
            new("Dip reference", "dip", Passed: false, "no dip curves in the source", []),
            new("Projection", "grid", Passed: false, null, []),
        };

        var outstanding = OgSetupRules.Outstanding(verification);

        Assert.Equal(2, outstanding.Count);
        Assert.Equal("Dip reference (no dip curves in the source)", outstanding[0]);
        Assert.Equal("Projection", outstanding[1]);

        // With nothing failing, nothing is outstanding - which is the only state a hand-over may proceed in.
        Assert.Empty(OgSetupRules.Outstanding(verification.Where(row => row.Passed).ToList()));
        Assert.Empty(OgSetupRules.Outstanding([]));
    }

    [Fact]
    public void A_coverage_that_cannot_be_computed_says_unknown_rather_than_zero_problems()
    {
        // AC-4's wording: unknown carries its reason, and an unknown report cannot be mistaken for a clean one.
        var unknown = new OgCoverageReport("Wells", false, 0, [], [], [], "the folder does not exist");

        Assert.False(unknown.Known);
        Assert.Contains("coverage unknown", unknown.Describe(), StringComparison.Ordinal);
        Assert.Contains("the folder does not exist", unknown.Describe(), StringComparison.Ordinal);

        var known = new OgCoverageReport("Wells", true, 12, ["codes 40"], ["codes · component.type · 'PACKR' · no symbol"],
            [], "computed");

        Assert.True(known.Known);
        Assert.Contains("12 well(s) scanned", known.Describe(), StringComparison.Ordinal);
        Assert.Contains("1 unresolved value(s)", known.Describe(), StringComparison.Ordinal);
    }

    [Fact]
    public void The_page_gates_the_hand_over_and_never_applies_a_proposal_by_itself()
    {
        var page = File.ReadAllText(
            SourceRoot.AllSources().Single(source => source.EndsWith(PageFileName, StringComparison.Ordinal)));

        // AC-5: the command is disabled until the engine reports no outstanding case.
        Assert.Contains("_handoverButton.IsEnabled = workspace.CanHandOver", page, StringComparison.Ordinal);

        // AC-3: accepting is a command of its own, and it needs a selected proposal.
        Assert.Contains("Select the proposal to accept", page, StringComparison.Ordinal);
        Assert.Contains("suggestion.Area", page, StringComparison.Ordinal);
        Assert.Contains("suggestion.Target", page, StringComparison.Ordinal);

        // The wizard edits the file the application loads, so it asks the host to re-read it.
        Assert.Contains("_application.ReloadWellboreEnvironment()", page, StringComparison.Ordinal);

        // A refused command clears the engine's view rather than leaving the last one on screen.
        Assert.Contains("_stepGrid.ItemsSource = null", page, StringComparison.Ordinal);
        Assert.Contains("_suggestionGrid.ItemsSource = null", page, StringComparison.Ordinal);
    }
}
