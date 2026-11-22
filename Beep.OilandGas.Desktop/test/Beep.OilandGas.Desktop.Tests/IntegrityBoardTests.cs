using TheTechIdea.Beep.OilandGas.Desktop.Data.Engineering;
using TheTechIdea.OilGas.Integrity;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// ENG-05: the unassessed label (AC-1), the owner's ordering (AC-2), the counts (AC-3), the CSV (AC-5) and the failed read - the
/// parts that need no database. The categories themselves come from the owner's assessment and are proven in the gateway test.
/// </summary>
public sealed class IntegrityBoardTests
{
    private const string PageFileName = "IntegrityBoardPageView.cs";

    [Fact]
    public void An_unassessed_well_says_so_before_its_category()
    {
        // AC-1: "unassessed" is a different state from green, and never reads as one.
        var unassessed = IntegrityBoardRules.StateOf("Green", unassessed: true, stale: false, ageDays: null, missing: []);

        Assert.StartsWith("unassessed", unassessed, StringComparison.Ordinal);
        Assert.DoesNotContain("green", unassessed, StringComparison.OrdinalIgnoreCase);
        Assert.Equal("unassessed", IntegrityBoardRules.UnassessedState);

        var assessed = IntegrityBoardRules.StateOf("Green", unassessed: false, stale: false, ageDays: 12.4, missing: []);

        Assert.Contains("green", assessed, StringComparison.Ordinal);
        Assert.Contains("assessed 12.4 day(s) ago", assessed, StringComparison.Ordinal);
        Assert.DoesNotContain("unassessed", assessed, StringComparison.Ordinal);
    }

    [Fact]
    public void A_stale_and_partial_well_says_both()
    {
        var state = IntegrityBoardRules.StateOf("Red", unassessed: false, stale: true, ageDays: 91, missing: ["no caliper result"]);

        Assert.Contains("red", state, StringComparison.Ordinal);
        Assert.Contains("assessed 91 day(s) ago", state, StringComparison.Ordinal);
        Assert.Contains("stale", state, StringComparison.Ordinal);
        Assert.Contains("partial inputs", state, StringComparison.Ordinal);
    }

    [Fact]
    public void Only_the_four_categories_are_accepted()
    {
        Assert.Equal("Red", IntegrityBoardRules.NormaliseCategory("red"));
        Assert.Equal("Orange", IntegrityBoardRules.NormaliseCategory(" Orange "));
        Assert.Null(IntegrityBoardRules.NormaliseCategory("Purple"));
        Assert.Null(IntegrityBoardRules.NormaliseCategory(" "));
        Assert.Null(IntegrityBoardRules.NormaliseCategory(null));

        Assert.Equal(["Green", "Yellow", "Orange", "Red"], IntegrityBoardRules.Categories);
    }

    [Fact]
    public void The_counts_are_the_owner_counts_over_the_wells()
    {
        // AC-3: one count per category, and they add up to the wells given.
        var wells = new List<WellIntegrity>
        {
            Well("W-1", IntegrityCategory.Green),
            Well("W-2", IntegrityCategory.Orange),
            Well("W-3", IntegrityCategory.Red),
            Well("W-4", IntegrityCategory.Green),
            Well("W-5", IntegrityCategory.Red),
        };

        var counts = IntegrityBoardRules.CountsOf(wells);

        Assert.Equal(4, counts.Count);
        Assert.Equal(2, counts.Single(count => count.Category == "Green").Count);
        Assert.Equal(0, counts.Single(count => count.Category == "Yellow").Count);
        Assert.Equal(1, counts.Single(count => count.Category == "Orange").Count);
        Assert.Equal(2, counts.Single(count => count.Category == "Red").Count);
        Assert.Equal(wells.Count, counts.Sum(count => count.Count));
    }

    [Fact]
    public void The_order_is_worst_first_and_stable_between_equal_categories()
    {
        // AC-2: category descending, then the well id in ordinal order - the owner's rule, documented on the board.
        var wells = new List<WellIntegrity>
        {
            Well("B-2", IntegrityCategory.Green),
            Well("A-1", IntegrityCategory.Red),
            Well("C-3", IntegrityCategory.Orange),
            Well("A-2", IntegrityCategory.Red),
            Well("B-1", IntegrityCategory.Orange),
        };

        var ordered = FieldIntegrity.WorstFirst(wells);

        Assert.Equal(["A-1", "A-2", "B-1", "C-3", "B-2"], ordered.Select(well => well.Well));

        // Running it twice gives the same answer, so the board does not reshuffle on a reload.
        Assert.Equal(ordered.Select(well => well.Well), FieldIntegrity.WorstFirst(wells).Select(well => well.Well));

        Assert.Contains("Worst first", OgIntegrityBoard.OrderingText, StringComparison.Ordinal);
        Assert.Contains("ordinal order", OgIntegrityBoard.OrderingText, StringComparison.Ordinal);
    }

    [Fact]
    public void The_csv_is_the_owner_export_over_the_same_wells()
    {
        // AC-5: the export is built from the board's own wells, worst first.
        var wells = new List<WellIntegrity>
        {
            Well("W-GREEN", IntegrityCategory.Green),
            Well("W-RED", IntegrityCategory.Red, "a leak to surface, and no barrier set"),
        };

        var lines = FieldIntegrity.ToCsv(wells)
            .Split('\n', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

        Assert.Equal("well,category,compliant,reasons", lines[0]);
        Assert.Equal(3, lines.Length);
        Assert.StartsWith("W-RED,Red,no,", lines[1], StringComparison.Ordinal);
        Assert.StartsWith("W-GREEN,Green,yes,", lines[2], StringComparison.Ordinal);

        // A reason with a comma is quoted rather than breaking the row.
        Assert.Contains("\"", lines[1], StringComparison.Ordinal);
        Assert.Contains("a leak to surface, and no barrier set", lines[1], StringComparison.Ordinal);
    }

    [Fact]
    public async Task A_failed_read_returns_no_board_and_never_a_green_one()
    {
        // AC-6: with no connection there is no board at all, so the page has nothing to paint green.
        var offline = new OgIntegrityBoardService(() => null, new Quiet());

        var loaded = await offline.LoadAsync("North", null, null);

        Assert.False(loaded.Success);
        Assert.Null(loaded.Value);
        Assert.Contains("No connection", loaded.Message!, StringComparison.Ordinal);

        var recorded = await offline.RecordAsync("North", null, null, " ");

        Assert.False(recorded.Success);
        Assert.Equal(IntegrityBoardRules.CodeActorRequired, recorded.Reference);
        Assert.Null(recorded.Value);
    }

    [Fact]
    public void The_page_draws_the_owner_board_and_clears_it_on_a_failed_read()
    {
        var page = File.ReadAllText(
            SourceRoot.AllSources().Single(source => source.EndsWith(PageFileName, StringComparison.Ordinal)));

        // The feature's open question: the board is the owner's view inside the shared Skia host.
        Assert.Contains("IntegrityBoardView", page, StringComparison.Ordinal);
        Assert.Contains("SkiaViewSurfaceHost", page, StringComparison.Ordinal);
        Assert.Contains("_board.Wells.AddRange(board.Board)", page, StringComparison.Ordinal);
        Assert.Contains("_host.Redraw()", page, StringComparison.Ordinal);

        Assert.Contains("board.Csv()", page, StringComparison.Ordinal);
        Assert.Contains("OgIntegrityBoard.OrderingText", page, StringComparison.Ordinal);

        // AC-6: the failed branch empties the board.
        Assert.Contains("_board.Wells.Clear()", page, StringComparison.Ordinal);
    }

    private static WellIntegrity Well(string id, IntegrityCategory category, string? reason = null)
    {
        var well = new WellIntegrity { Well = id, Category = category };
        well.Reasons.Add(reason ?? category + ": from the owner's assessment");
        return well;
    }

    private sealed class Quiet : IFailureReporter
    {
        public string ReportHandled(
            Exception exception,
            string operation,
            string consequence,
            FailureSeverity severity = FailureSeverity.Error) => FailureReferenceFormat.New();
    }
}
