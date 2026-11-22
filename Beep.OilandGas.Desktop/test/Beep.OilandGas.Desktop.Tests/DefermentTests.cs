using TheTechIdea.Beep.OilandGas.Desktop.Data.Production;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// PRD-03: the reason-code rule, the overlap rule, the volume-unit rule, the totals rule and the ongoing-versus-closed
/// rule - the parts that do not need a database. The PPDM status read, the saved deferment and the failure path are
/// proven in the gateway test.
/// </summary>
public sealed class DefermentTests
{
    private static readonly DateTime Day = new(2026, 10, 1);
    private static readonly string[] Reasons = ["EQ-FAIL", "WEATHER", "PLANNED"];

    [Fact]
    public void A_deferment_cannot_be_saved_without_a_reference_reason_code()
    {
        // No code at all.
        Assert.Equal(
            DefermentRules.CodeReasonRequired,
            DefermentRules.Validate(Values(reason: null), Reasons)!.Value.Code);

        // A code that is not in the set, and the set is offered in the message.
        var unknown = DefermentRules.Validate(Values(reason: "MADE-UP"), Reasons)!.Value;

        Assert.Equal(DefermentRules.CodeReasonUnknown, unknown.Code);
        Assert.Contains("EQ-FAIL", unknown.Message, StringComparison.Ordinal);

        // An empty reference set is its own refusal, not a free-text acceptance.
        Assert.Equal(
            DefermentRules.CodeReasonReferenceMissing,
            DefermentRules.Validate(Values(reason: "EQ-FAIL"), [])!.Value.Code);

        Assert.Null(DefermentRules.Validate(Values(reason: "eq-fail"), Reasons));
    }

    [Fact]
    public void A_deferment_needs_a_well_a_start_and_a_reporter()
    {
        Assert.Equal(
            DefermentRules.CodeWellRequired,
            DefermentRules.Validate(Values(well: " "), Reasons)!.Value.Code);

        Assert.Equal(
            DefermentRules.CodeStartRequired,
            DefermentRules.Validate(Values() with { Start = null }, Reasons)!.Value.Code);

        Assert.Equal(
            DefermentRules.CodeEndBeforeStart,
            DefermentRules.Validate(Values(start: Day, end: Day.AddHours(-1)), Reasons)!.Value.Code);

        Assert.Equal(
            DefermentRules.CodeReporterRequired,
            DefermentRules.Validate(Values(reporter: " "), Reasons)!.Value.Code);

        Assert.Null(DefermentRules.Validate(Values(), Reasons));
    }

    [Fact]
    public void A_deferred_volume_carries_its_unit()
    {
        Assert.Equal(
            DefermentRules.CodeVolumeUnitRequired,
            DefermentRules.Validate(Values(volume: 120m, unit: null), Reasons)!.Value.Code);

        Assert.Equal(
            DefermentRules.CodeVolumeInvalid,
            DefermentRules.Validate(Values(volume: -1m, unit: "bbl"), Reasons)!.Value.Code);

        Assert.Null(DefermentRules.Validate(Values(volume: 120m, unit: "bbl"), Reasons));

        // No volume at all is allowed: the deferment may only know its hours.
        Assert.Null(DefermentRules.Validate(Values(volume: null, unit: null), Reasons));
    }

    [Fact]
    public void Overlapping_deferments_are_detected_including_an_open_ended_one()
    {
        var morning = (Start: Day.AddHours(6), End: (DateTime?)Day.AddHours(12));
        var afternoon = (Start: Day.AddHours(12), End: (DateTime?)Day.AddHours(18));
        var later = (Start: Day.AddDays(2), End: (DateTime?)Day.AddDays(3));

        Assert.True(DefermentRules.Overlaps(morning.Start, morning.End, afternoon.Start, afternoon.End));
        Assert.True(DefermentRules.Overlaps(afternoon.Start, afternoon.End, morning.Start, morning.End));
        Assert.False(DefermentRules.Overlaps(morning.Start, morning.End, later.Start, later.End));

        // An ongoing deferment is open-ended, so anything after its start overlaps it.
        Assert.True(DefermentRules.Overlaps(
            Day.AddHours(6),
            null,
            Day.AddDays(5),
            Day.AddDays(6)));

        // Two closed deferments that only touch at one instant still overlap (they are not merged, they are flagged).
        Assert.True(DefermentRules.Overlaps(
            Day.AddHours(6),
            Day.AddHours(12),
            Day.AddHours(12),
            Day.AddHours(18)));
    }

    [Fact]
    public void An_ongoing_deferment_is_distinct_from_a_closed_one()
    {
        var ongoing = Deferment(end: null);
        var closed = Deferment(end: Day.AddHours(8));

        Assert.True(ongoing.IsOngoing);
        Assert.False(closed.IsOngoing);

        Assert.Equal("ongoing", ongoing.EndText);
        Assert.Equal("2026-10-01 08:00", closed.EndText);

        Assert.Equal(12m, ongoing.ElapsedHours(Day.AddHours(12)));
        Assert.Equal(8m, closed.ElapsedHours(Day.AddHours(12)));

        Assert.Contains("ongoing", ongoing.DurationText(Day.AddHours(12)), StringComparison.Ordinal);
        Assert.Contains("12 h so far", ongoing.DurationText(Day.AddHours(12)), StringComparison.Ordinal);
        Assert.DoesNotContain("ongoing", closed.DurationText(Day.AddHours(12)), StringComparison.Ordinal);

        // Elapsed hours never go negative, whatever order the dates arrive in.
        Assert.Equal(0m, DefermentRules.ElapsedHours(Day, Day.AddHours(-4), Day));
    }

    [Fact]
    public void The_totals_by_reason_equal_the_sum_of_the_rows()
    {
        var rows = new[]
        {
            Deferment("EQ-FAIL", volume: 100m),
            Deferment("EQ-FAIL", volume: 50m),
            Deferment("WEATHER", volume: 25m),
        };

        var totals = DefermentRules.Totals(rows);

        Assert.Equal(2, totals.Count);

        var equipment = totals.Single(total => total.Code == "EQ-FAIL");

        Assert.Equal(2, equipment.Count);
        Assert.Equal(150m, equipment.Volume);
        Assert.Equal("bbl", equipment.Unit);
        Assert.Contains("150 bbl", equipment.Text, StringComparison.Ordinal);

        Assert.Equal(25m, totals.Single(total => total.Code == "WEATHER").Volume);

        // The totals are the rows' own sum, not a second computation.
        Assert.Equal(rows.Sum(row => row.Volume ?? 0m), totals.Sum(total => total.Volume));

        var (volume, unit, mixed) = DefermentRules.TotalVolume(rows);

        Assert.Equal(175m, volume);
        Assert.Equal("bbl", unit);
        Assert.Empty(mixed);
    }

    [Fact]
    public void Two_volume_units_are_named_rather_than_added()
    {
        var rows = new[]
        {
            Deferment("EQ-FAIL", volume: 100m, unit: "bbl"),
            Deferment("EQ-FAIL", volume: 2m, unit: "MMscf"),
        };

        var total = Assert.Single(DefermentRules.Totals(rows));

        Assert.True(total.HasMixedUnits);
        Assert.Null(total.Volume);
        Assert.Equal(["bbl", "MMscf"], total.MixedUnits);
        Assert.Equal("mixed units, so no total", total.VolumeText);

        var (volume, unit, mixed) = DefermentRules.TotalVolume(rows);

        Assert.Null(volume);
        Assert.Null(unit);
        Assert.Equal(2, mixed.Count);
    }

    [Fact]
    public void The_board_counts_the_deferred_ongoing_and_overlapping_wells()
    {
        var card = new OgWellCard(
            "WELL-1", "Well one", "PRODUCING", "Well Status", Day, null, 120m, "bbl",
            "D-1", "EQ-FAIL", "Equipment failure", Day);

        var open = Deferment("EQ-FAIL", volume: 100m) with { OverlapWith = ["D-2"] };

        var board = new OgWellStatusBoard(
            "Alpha field",
            "F-1",
            Day,
            [new OgStatusGroup("PRODUCING", "Well Status", [card])],
            [card],
            [open, Deferment("WEATHER", volume: 25m, end: Day.AddHours(6))],
            DefermentRules.Totals([open, Deferment("WEATHER", volume: 25m, end: Day.AddHours(6))]),
            150m,
            "bbl",
            ["EQ-FAIL", "WEATHER"],
            false,
            []);

        Assert.Equal(1, board.WellCount);
        Assert.Equal(1, board.DeferredWells);
        Assert.Equal(1, board.OngoingCount);
        Assert.Equal(1, board.OverlapCount);
        Assert.Contains("1 well(s)", board.Summary, StringComparison.Ordinal);
        Assert.Contains("1 overlapping", board.Summary, StringComparison.Ordinal);
        Assert.Contains("producing (1)", board.GroupText, StringComparison.Ordinal);
        Assert.Contains("150 bbl", board.TotalText, StringComparison.Ordinal);

        Assert.True(card.IsDeferred);
        Assert.Equal("EQ-FAIL (Equipment failure)", card.ReasonText);
        Assert.Equal("120 bbl", card.RateText);
        Assert.Contains("deferred since", card.DefermentText, StringComparison.Ordinal);

        var notDeferred = card with { OpenDefermentId = null, OpenReasonCode = null, OpenReasonName = null, OpenSince = null };

        Assert.False(notDeferred.IsDeferred);
        Assert.Equal("no open deferment", notDeferred.ReasonText);
    }

    [Fact]
    public void A_board_with_no_reason_code_says_which_table_to_fill()
    {
        var board = new OgWellStatusBoard(
            "Alpha field",
            "F-1",
            Day,
            [],
            [],
            [],
            [],
            null,
            null,
            [],
            true,
            []);

        Assert.True(board.ReasonReferenceMissing);
        Assert.Contains("R_DEFERMENT_REASON", board.ReasonsText, StringComparison.Ordinal);
        Assert.Equal("no deferment is recorded for this field", board.TotalsText);
        Assert.Equal("no well status is recorded for this field", board.GroupText);
        Assert.Equal("not recorded", board.TotalText);
    }

    [Fact]
    public async Task No_connection_is_a_failure_not_an_empty_board()
    {
        var service = new OgWellStatusDefermentService(() => null, new Quiet());

        var loaded = await service.LoadAsync("F-1", "Alpha");

        Assert.False(loaded.Success);
        Assert.Equal(OgWellStatusDefermentService.CodeNoConnection, loaded.Reference);
        Assert.Null(loaded.Value);
    }

    [Fact]
    public async Task Logging_and_closing_both_need_an_actor()
    {
        var service = new OgWellStatusDefermentService(() => null, new Quiet());

        Assert.Equal(
            DefermentRules.CodeActorRequired,
            (await service.LogDefermentAsync(Values(), " ")).Reference);

        Assert.Equal(
            DefermentRules.CodeActorRequired,
            (await service.CloseDefermentAsync("D-1", Day, " ")).Reference);
    }

    private static OgDefermentValues Values(
        string? well = "WELL-1",
        DateTime? start = null,
        DateTime? end = null,
        string? reason = "EQ-FAIL",
        decimal? volume = null,
        string? unit = null,
        string? reporter = "sara") =>
        new(well, start ?? Day, end, reason, volume, unit, null, reporter);

    private static OgDeferment Deferment(
        string reason = "EQ-FAIL",
        decimal? volume = 100m,
        string? unit = "bbl",
        DateTime? end = null,
        IReadOnlyList<string>? overlapWith = null) =>
        new(
            "D-1",
            "WELL-1",
            "Well one",
            "F-1",
            Day,
            end,
            reason,
            "Equipment failure",
            volume,
            unit,
            null,
            "sara",
            overlapWith ?? []);

    private sealed class Quiet : IFailureReporter
    {
        public string ReportHandled(
            Exception exception,
            string operation,
            string consequence,
            FailureSeverity severity = FailureSeverity.Error) => FailureReferenceFormat.New();
    }
}
