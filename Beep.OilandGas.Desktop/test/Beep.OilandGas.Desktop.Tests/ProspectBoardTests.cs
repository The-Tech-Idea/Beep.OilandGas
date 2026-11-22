using TheTechIdea.Beep.OilandGas.Desktop.Data.Exploration;
using TheTechIdea.Beep.OilandGas.Desktop.Views;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// EXP-02: the rules that hold without a database - columns follow the recorded stage sequence, an unclassified
/// prospect keeps its own column, an incomplete risk says "incomplete" instead of a number, the advance command is
/// refused with a stated reason, the drop path is the same command, and a failed read keeps the board.
/// </summary>
public sealed class ProspectBoardTests
{
    [Fact]
    public void Columns_follow_the_recorded_stage_sequence_not_the_alphabet()
    {
        var cards = new[]
        {
            Card("P-1", "Alpha", "SCREENING", 1),
            Card("P-2", "Bravo", "APPRAISAL", 2),
            Card("P-3", "Charlie", "DRILLING", 3),
        };

        var columns = OgProspectBoard.BuildColumns(cards, cards);

        // AC-1: alphabetical would be APPRAISAL, DRILLING, SCREENING.
        Assert.Equal(["SCREENING", "APPRAISAL", "DRILLING"], columns.Select(column => column.Stage));
        Assert.Equal([1, 2, 3], columns.Select(column => column.Order));
        Assert.All(columns, column => Assert.Equal(1, column.Count));
    }

    [Fact]
    public void A_prospect_with_no_stage_is_visible_in_its_own_column_and_never_dropped()
    {
        var cards = new[]
        {
            Card("P-1", "Alpha", "APPRAISAL", 2),
            Card("P-2", "Bravo", null, int.MaxValue),
            Card("P-3", "Charlie", null, int.MaxValue),
        };

        var columns = OgProspectBoard.BuildColumns(cards, cards);

        // AC-2: the unclassified column exists, is last, and holds both prospects.
        var unclassified = Assert.Single(columns, column => column.IsUnclassified);
        Assert.Equal(OgProspectCard.UnclassifiedStage, unclassified.Stage);
        Assert.Equal(2, unclassified.Count);
        Assert.Same(unclassified, columns[^1]);
        Assert.Equal(3, columns.Sum(column => column.Count));
    }

    [Fact]
    public void A_filter_hides_cards_but_never_hides_a_stage_column()
    {
        var all = new[]
        {
            Card("P-1", "Alpha", "SCREENING", 1, chance: 60m),
            Card("P-2", "Bravo", "APPRAISAL", 2, chance: 20m),
        };

        var shown = new[] { all[0] };
        var columns = OgProspectBoard.BuildColumns(all, shown);

        Assert.Equal(2, columns.Count);
        Assert.Equal(1, columns[0].Count);
        Assert.Equal(0, columns[1].Count);
        Assert.Null(columns[1].AverageChancePercent);
    }

    [Fact]
    public void An_incomplete_risk_says_incomplete_and_does_not_invent_a_chance()
    {
        var incomplete = Card("P-1", "Alpha", "APPRAISAL", 2, chance: null, complete: false);
        var complete = Card("P-2", "Bravo", "APPRAISAL", 2, chance: 75m);

        // AC-3
        Assert.Equal("incomplete", incomplete.ChanceText);
        Assert.Equal("75%", complete.ChanceText);

        var noOverall = complete with { ChancePercent = null };
        Assert.Equal("no chance recorded", noOverall.ChanceText);
    }

    [Fact]
    public void Chance_is_the_product_of_the_element_success_probabilities()
    {
        var complete = new Dictionary<string, decimal?>(StringComparer.OrdinalIgnoreCase)
        {
            ["TRAP"] = 0.1m,
            ["RESERVOIR"] = 0.2m,
            ["SEAL"] = 0.1m,
            ["SOURCE"] = 0.1m,
            ["TIMING"] = 0.1m,
        };

        // 0.9 * 0.8 * 0.9 * 0.9 * 0.9 = 0.52488 -> 52%
        Assert.Equal(52m, OgProspectChance.Of(complete));

        // A missing element produces no number, whichever one it is.
        foreach (var element in OgProspectChance.RequiredElements)
        {
            var partial = new Dictionary<string, decimal?>(complete, StringComparer.OrdinalIgnoreCase)
            {
                [element] = null,
            };

            Assert.Null(OgProspectChance.Of(partial));
            Assert.False(OgProspectChance.IsComplete(partial));
        }

        // An out-of-range probability is a data error, not a chance.
        Assert.Null(OgProspectChance.Of(
            new Dictionary<string, decimal?>(complete, StringComparer.OrdinalIgnoreCase) { ["TRAP"] = 1.4m }));

        var certain = OgProspectChance.RequiredElements.ToDictionary(element => element, _ => (decimal?)0m);
        var impossible = OgProspectChance.RequiredElements.ToDictionary(element => element, _ => (decimal?)1m);

        Assert.Equal(100m, OgProspectChance.Of(certain));
        Assert.Equal(0m, OgProspectChance.Of(impossible));
        Assert.Equal("incomplete", OgProspectChance.Text(null, complete: false));
        Assert.Equal("52%", OgProspectChance.Text(52m, complete: true));
    }
    [Fact]
    public void Completeness_needs_all_five_elements()
    {
        var complete = new OgRisk(0.1m, 0.2m, 0.1m, 0.1m, 0.1m, 0.4m);

        Assert.True(OgProspectBoard.IsComplete(complete));

        foreach (var missing in new[]
                 {
                     complete with { Trap = null },
                     complete with { Reservoir = null },
                     complete with { Seal = null },
                     complete with { Source = null },
                     complete with { Timing = null },
                 })
        {
            // AC-3: any missing element makes the assessment incomplete.
            Assert.False(OgProspectBoard.IsComplete(missing));
        }

        Assert.Equal(5, OgProspectBoard.RiskElements.Count);
    }

    [Fact]
    public async Task Advancing_is_refused_with_a_stated_reason_and_changes_nothing()
    {
        var board = new OgProspectBoard(() => null!, new Quiet());
        var card = Card("P-1", "Alpha", "APPRAISAL", 2);

        var refused = await board.AdvanceAsync(card, "DRILLING", "fahad");

        // AC-4
        Assert.False(refused.Success);
        Assert.Equal(OgProspectBoard.CodeAdvanceNotWired, refused.Reference);
        Assert.Contains("refused", refused.Message!, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("STG-14", refused.Message!, StringComparison.Ordinal);
        Assert.Contains("Nothing was written", refused.Message!, StringComparison.Ordinal);
    }

    [Fact]
    public async Task The_advance_command_checks_its_inputs_before_it_refuses()
    {
        var board = new OgProspectBoard(() => null!, new Quiet());
        var card = Card("P-1", "Alpha", "APPRAISAL", 2);

        Assert.Equal(
            OgProspectBoard.CodeActorRequired,
            (await board.AdvanceAsync(card, "DRILLING", " ")).Reference);

        Assert.Equal(
            OgProspectBoard.CodeTargetRequired,
            (await board.AdvanceAsync(card, null, "fahad")).Reference);

        Assert.Equal(
            OgProspectBoard.CodeUnchanged,
            (await board.AdvanceAsync(card, "appraisal", "fahad")).Reference);
    }

    [Fact]
    public async Task A_drop_runs_the_same_check_as_the_menu_command()
    {
        var board = new OgProspectBoard(() => null!, new Quiet());
        var card = Card("P-1", "Alpha", "APPRAISAL", 2);

        var fromMenu = await board.AdvanceAsync(card, "DRILLING", "fahad");
        var fromDrop = await board.MoveAsync(card, "DRILLING", "fahad");

        // AC-5
        Assert.Equal(fromMenu.Reference, fromDrop.Reference);
        Assert.Equal(fromMenu.Message, fromDrop.Message);
        Assert.Equal(fromMenu.Success, fromDrop.Success);
    }

    [Fact]
    public void A_failed_read_keeps_the_board_instead_of_clearing_it()
    {
        var current = new OgProspectBoardSnapshot(
            "North",
            [],
            [Card("P-1", "Alpha", "APPRAISAL", 2)],
            [],
            [],
            [],
            new DateTime(2026, 3, 1));

        var selected = current.Cards[0];

        // AC-6
        var kept = ProspectBoardPageView.AfterRead(current, selected, success: false, loaded: null);

        Assert.Same(current, kept.Snapshot);
        Assert.Same(selected, kept.Selected);
        Assert.True(kept.Failed);

        // A successful read replaces the snapshot and drops the selection.
        var fresh = current with { FieldName = "South" };
        var replaced = ProspectBoardPageView.AfterRead(current, selected, success: true, loaded: fresh);

        Assert.Same(fresh, replaced.Snapshot);
        Assert.Null(replaced.Selected);
        Assert.False(replaced.Failed);
    }

    private static OgProspectCard Card(
        string id,
        string name,
        string? stage,
        int order,
        decimal? chance = null,
        bool complete = true) =>
        new(
            id,
            name,
            Play: "Upper sand",
            Basin: "BASIN-1",
            Owner: "BA-1",
            Stage: stage,
            StageOrder: order,
            StageEnteredAt: new DateTime(2026, 2, 1),
            StageAgeDays: 28,
            ChancePercent: chance,
            RiskComplete: complete,
            History: stage is null
                ? []
                : [new OgProspectStageRow(stage, order, new DateTime(2026, 2, 1), null, null)]);

    private sealed class Quiet : IFailureReporter
    {
        public string ReportHandled(
            Exception exception,
            string operation,
            string consequence,
            FailureSeverity severity = FailureSeverity.Error) => FailureReferenceFormat.New();
    }
}
