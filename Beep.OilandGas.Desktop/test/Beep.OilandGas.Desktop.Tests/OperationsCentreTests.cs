using TheTechIdea.Beep.OilandGas.Desktop.Data.Production;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// PRD-01: the unit rule, the water cut rule, "not yet reported" against zero, the dominant-unit rule and the
/// top-mover history requirement - the parts that do not need a database. The PPDM reads, the amendment rule, the
/// completeness count and the failure path are proven in the gateway test.
/// </summary>
public sealed class OperationsCentreTests
{
    private static readonly DateTime Day = new(2026, 10, 1);

    [Fact]
    public void Not_yet_reported_is_a_state_and_never_zero()
    {
        var missing = new OgProductionDay(Day, null, null, null, null, null, null, 0, 0);
        var zero = new OgProductionDay(Day, 0m, 0m, 0m, "bbl", "MMscf", "bbl", 1, 1);

        Assert.False(missing.Reported);
        Assert.Equal("not yet reported", missing.StateText);
        Assert.Equal("not yet reported", missing.OilText);
        Assert.Contains("not yet reported", missing.Describe(), StringComparison.Ordinal);

        Assert.True(zero.Reported);
        Assert.Equal("reported", zero.StateText);
        Assert.Equal("0 bbl", zero.OilText);
        Assert.NotEqual(missing.OilText, zero.OilText);
    }

    [Fact]
    public void A_volume_always_carries_its_unit_or_says_it_was_not_recorded()
    {
        Assert.Equal("1,250 bbl", OgProductionDay.Format(1250m, "bbl"));
        Assert.Equal("0.5 MMscf", OgProductionDay.Format(0.5m, "MMscf"));
        Assert.Equal("12 (unit not recorded)", OgProductionDay.Format(12m, " "));
        Assert.Equal("not recorded", OgProductionDay.Format(null, "bbl"));
    }

    [Fact]
    public void A_water_cut_is_computed_only_when_oil_and_water_share_a_unit()
    {
        Assert.Equal(0.25m, OperationsCentreRules.WaterCut(750m, 250m, "bbl", "bbl"));
        Assert.Equal(0.25m, OperationsCentreRules.WaterCut(750m, 250m, "bbl", "BBL"));

        Assert.Null(OperationsCentreRules.WaterCut(750m, 250m, "bbl", "m3"));
        Assert.Null(OperationsCentreRules.WaterCut(750m, 250m, "bbl", null));
        Assert.Null(OperationsCentreRules.WaterCut(null, 250m, "bbl", "bbl"));
        Assert.Null(OperationsCentreRules.WaterCut(0m, 0m, "bbl", "bbl"));

        Assert.Equal(
            "not computable: oil is in bbl and water is in m3",
            OperationsCentreRules.WaterCutText(
                OperationsCentreRules.WaterCut(750m, 250m, "bbl", "m3"), "bbl", "m3"));

        Assert.Equal(
            "25%",
            OperationsCentreRules.WaterCutText(
                OperationsCentreRules.WaterCut(750m, 250m, "bbl", "bbl"), "bbl", "bbl"));
    }

    [Fact]
    public void The_dominant_unit_is_the_unit_most_rows_use()
    {
        Assert.Equal("bbl", OperationsCentreRules.DominantUnit(["bbl", "bbl", "m3"]));
        Assert.Equal("MMscf", OperationsCentreRules.DominantUnit([null, " ", "MMscf"]));
        Assert.Null(OperationsCentreRules.DominantUnit([null, "  "]));
        Assert.Null(OperationsCentreRules.DominantUnit([]));
    }

    [Fact]
    public void An_injection_row_is_not_a_production_row()
    {
        Assert.True(OperationsCentreRules.IsProductionRow("PRODUCTION"));
        Assert.True(OperationsCentreRules.IsProductionRow(null));
        Assert.True(OperationsCentreRules.IsProductionRow(" "));
        Assert.False(OperationsCentreRules.IsProductionRow("INJECTION"));
        Assert.False(OperationsCentreRules.IsProductionRow("injection"));
    }

    [Fact]
    public void Top_movers_need_seven_days_of_history()
    {
        Assert.Equal(7, OperationsCentreRules.RequiredHistoryDays);
        Assert.Equal(30, OperationsCentreRules.WindowDays);
    }

    [Fact]
    public void The_day_state_says_which_of_three_answers_applies()
    {
        var reported = Centre(
            today: new OgProductionDay(Day, 100m, 10m, 20m, "bbl", "MMscf", "bbl", 2, 2),
            days: [new OgProductionDay(Day, 100m, 10m, 20m, "bbl", "MMscf", "bbl", 2, 2)]);

        var notYet = Centre(
            today: new OgProductionDay(Day, null, null, null, null, null, null, 0, 0),
            days:
            [
                new OgProductionDay(Day.AddDays(-1), 90m, 9m, 18m, "bbl", "MMscf", "bbl", 2, 2),
                new OgProductionDay(Day, null, null, null, null, null, null, 0, 0),
            ]);

        var nothing = Centre(
            today: new OgProductionDay(Day, null, null, null, null, null, null, 0, 0),
            days: [new OgProductionDay(Day, null, null, null, null, null, null, 0, 0)]);

        Assert.StartsWith("reported for", reported.DayStateText, StringComparison.Ordinal);
        Assert.StartsWith("not yet reported for", notYet.DayStateText, StringComparison.Ordinal);
        Assert.Contains("no production is recorded", nothing.DayStateText, StringComparison.Ordinal);

        Assert.True(notYet.HasData);
        Assert.False(nothing.HasData);
    }

    [Fact]
    public void The_completeness_indicator_counts_the_wells_that_did_not_report()
    {
        var complete = Centre(
            today: new OgProductionDay(Day, 100m, null, null, "bbl", null, null, 2, 2),
            alerts: []);

        var partial = Centre(
            today: new OgProductionDay(Day, 100m, null, null, "bbl", null, null, 1, 1),
            alerts:
            [
                new OgProductionAlert("WELL-2", "WELL-2", "no production measurement for the day"),
                new OgProductionAlert("WELL-3", null, "no production measurement for the day"),
            ]);

        Assert.Contains("complete: every active well reported", complete.CompletenessText, StringComparison.Ordinal);
        Assert.Contains("2 of 3 active well(s) have no measurement", partial.CompletenessText, StringComparison.Ordinal);
        Assert.Equal(2, partial.MissingWellCount);
        Assert.Contains("WELL-2: no production measurement for the day", partial.Alerts[0].Describe(), StringComparison.Ordinal);
        Assert.Contains("WELL-3", partial.Alerts[1].Describe(), StringComparison.Ordinal);
    }

    [Fact]
    public void The_strip_names_an_unreported_day_instead_of_drawing_zero()
    {
        var centre = Centre(
            today: new OgProductionDay(Day, 100m, null, null, "bbl", null, null, 1, 1),
            days:
            [
                new OgProductionDay(Day.AddDays(-1), null, null, null, null, null, null, 0, 0),
                new OgProductionDay(Day, 100m, null, null, "bbl", null, null, 1, 1),
            ]);

        Assert.Contains("not reported", centre.StripText, StringComparison.Ordinal);
        Assert.Contains("100 bbl", centre.StripText, StringComparison.Ordinal);
        Assert.Contains("no day in the window", Centre(
            today: new OgProductionDay(Day, null, null, null, null, null, null, 0, 0),
            days: []).StripText, StringComparison.Ordinal);
    }

    [Fact]
    public void Deferment_and_allocation_are_labelled_rather_than_shown_as_zero()
    {
        var centre = Centre(
            today: new OgProductionDay(Day, 100m, null, null, "bbl", null, null, 1, 1),
            alerts: []);

        Assert.False(centre.DeferredAvailable);
        Assert.Contains("PRD-03", centre.DeferredText, StringComparison.Ordinal);
        Assert.False(centre.AllocationAvailable);
        Assert.Contains("PRD-04", centre.AllocationText, StringComparison.Ordinal);
        Assert.Contains("PRD-03", centre.DowntimeText, StringComparison.Ordinal);
    }

    [Fact]
    public void A_unit_that_does_not_match_is_named_and_never_converted()
    {
        var centre = Centre(
            today: new OgProductionDay(Day, 100m, null, null, "bbl", null, null, 1, 1),
            alerts: [],
            notComparable: ["1 oil row(s) not in bbl"]);

        Assert.Contains("1 oil row(s) not in bbl", centre.NotComparableText, StringComparison.Ordinal);
        Assert.Contains("never", "never converted", StringComparison.Ordinal);

        Assert.Equal(
            "every volume is stated in one unit per fluid",
            Centre(
                today: new OgProductionDay(Day, 100m, null, null, "bbl", null, null, 1, 1),
                alerts: []).NotComparableText);
    }

    [Fact]
    public void A_mover_shows_its_change_with_its_unit_and_its_history()
    {
        var gainer = new OgMover("WELL-1", "WELL-1", 150m, 100m, 50m, "bbl", 7);
        var decliner = new OgMover("WELL-2", null, 50m, 100m, -50m, "bbl", 7);

        Assert.True(gainer.Rose);
        Assert.Equal("+50 bbl", gainer.ChangeText);
        Assert.Contains("today 150 bbl", gainer.Describe(), StringComparison.Ordinal);
        Assert.Contains("7 day(s)", gainer.Describe(), StringComparison.Ordinal);

        Assert.False(decliner.Rose);
        Assert.Equal("-50 bbl", decliner.ChangeText);
    }

    [Fact]
    public void The_excluded_rows_are_counted_and_named()
    {
        var centre = Centre(
            today: new OgProductionDay(Day, 1m, null, null, "bbl", null, null, 1, 1),
            alerts: [],
            injection: 2,
            undated: 1,
            superseded: 3);

        Assert.Contains("2 injection", centre.ExcludedText, StringComparison.Ordinal);
        Assert.Contains("1 without a volume date", centre.ExcludedText, StringComparison.Ordinal);
        Assert.Contains("3 superseded amendment", centre.ExcludedText, StringComparison.Ordinal);
    }

    [Fact]
    public async Task No_connection_is_a_failure_not_a_page_of_zeros()
    {
        var service = new OgOperationsCentreService(() => null, new Quiet());

        var loaded = await service.LoadAsync("F-1", "Alpha");

        Assert.False(loaded.Success);
        Assert.Equal(OgOperationsCentreService.CodeNoConnection, loaded.Reference);
        Assert.Null(loaded.Value);
    }

    private static OgOperationsCentre Centre(
        OgProductionDay today,
        IReadOnlyList<OgProductionDay>? days = null,
        IReadOnlyList<OgProductionAlert>? alerts = null,
        IReadOnlyList<string>? notComparable = null,
        int injection = 0,
        int undated = 0,
        int superseded = 0) =>
        new(
            "Alpha field",
            "F-1",
            Day,
            today,
            days ?? [today],
            [],
            [],
            [],
            false,
            "hidden: no well has 7 day(s) of history before this day",
            alerts ?? [],
            3,
            4,
            notComparable ?? [],
            injection,
            undated,
            superseded);

    private sealed class Quiet : IFailureReporter
    {
        public string ReportHandled(
            Exception exception,
            string operation,
            string consequence,
            FailureSeverity severity = FailureSeverity.Error) => FailureReferenceFormat.New();
    }
}
