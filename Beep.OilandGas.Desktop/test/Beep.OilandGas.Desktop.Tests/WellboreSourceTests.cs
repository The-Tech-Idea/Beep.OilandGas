using TheTechIdea.Beep.OilandGas.Desktop.Data.Wells;
using TheTechIdea.OilGas.Wellbore;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// VSB-03: the axis the database well is drawn on, the interval track its completions produce, the warnings it must state, and
/// the readout at a depth - the parts that need no database. The read over real rows is proven in the gateway test.
/// </summary>
public sealed class WellboreSourceTests
{
    private const string PageFileName = "WellboreDemoView.cs";

    [Fact]
    public void A_well_with_completions_draws_a_depth_track_and_a_data_track()
    {
        // AC-1: the depth track alone is not enough; a completion interval track is the data the schema records.
        var (document, warnings, tracks) = WellboreSourceRules.Build(
            "W-1",
            "North 1",
            2500m,
            "m",
            [
                (1000m, 1010m, "PERFORATED", "m"),
                (1500m, 1520m, "PERFORATED", "m"),
            ]);

        Assert.Equal(2, tracks.Count);
        Assert.True(tracks.Any(track => track.Kind == "depth"));
        Assert.Single(tracks, track => track.Kind == "intervals");

        Assert.Equal(0f, document.Axis.Top);
        Assert.Equal(2500f, document.Axis.Bottom);
        Assert.Equal(DepthUnit.Meters, document.Axis.Unit);

        var intervals = document.Tracks.OfType<IntervalTrack>().Single();

        Assert.Equal(2, intervals.Intervals.Count);
        Assert.Equal(1500f, intervals.Intervals[1].Top);
        Assert.Equal("PERFORATED", intervals.Intervals[1].Text);

        // The honest warning is always present: this schema has no measured values to plot.
        Assert.Contains(warnings, warning => warning.Contains("no measured curve values", StringComparison.Ordinal));

        var view = new OgWellboreView("W-1", "North 1", 0f, 2500f, "m", tracks, warnings, document);

        Assert.True(view.HasDepthAndDataTrack);
        Assert.Contains("0 to 2500 m", view.RangeText, StringComparison.Ordinal);
    }

    [Fact]
    public void A_well_without_a_total_depth_says_where_its_axis_ends()
    {
        // The axis is never passed off as the well's depth when the wellbore records none.
        var (document, warnings, _) = WellboreSourceRules.Build(
            "W-2",
            "North 2",
            null,
            "ft",
            [(2000m, 2100m, "OPEN_HOLE", "ft")]);

        Assert.Equal(2100f, document.Axis.Bottom);
        Assert.Equal(DepthUnit.Feet, document.Axis.Unit);
        Assert.Contains(warnings, warning => warning.Contains("no total depth", StringComparison.Ordinal));

        var (empty, emptyWarnings, _) = WellboreSourceRules.Build("W-3", "North 3", null, "m", []);

        Assert.Equal(0f, empty.Axis.Bottom);
        Assert.Contains(emptyWarnings, warning => warning.Contains("no depth to draw", StringComparison.Ordinal));
    }

    [Fact]
    public void A_zero_thickness_interval_is_refused_and_a_unit_mismatch_is_stated()
    {
        var (document, warnings, tracks) = WellboreSourceRules.Build(
            "W-4",
            "North 4",
            1000m,
            "m",
            [
                (500m, 500m, "PERFORATED", "m"),
                (600m, 610m, "PERFORATED", "ft"),
            ]);

        // The empty interval is not drawn; the mismatched one is drawn and warned about.
        Assert.Equal(1, document.Tracks.OfType<IntervalTrack>().Single().Intervals.Count);
        Assert.Contains(warnings, warning => warning.Contains("no thickness", StringComparison.Ordinal));
        Assert.Contains(warnings, warning => warning.Contains("differs from the well's", StringComparison.Ordinal));
        Assert.Single(tracks, track => track.Kind == "intervals");
    }

    [Fact]
    public void The_readout_names_the_interval_at_the_depth_or_says_there_is_none()
    {
        var (document, warnings, tracks) = WellboreSourceRules.Build(
            "W-5",
            "North 5",
            2000m,
            "m",
            [(1000m, 1010m, "PERFORATED", "m")]);

        var view = new OgWellboreView("W-5", "North 5", 0f, 2000f, "m", tracks, warnings, document);

        var inside = view.ReadoutAt(1005f);

        Assert.Contains("1005 m", inside, StringComparison.Ordinal);
        Assert.Contains("Completion", inside, StringComparison.Ordinal);
        Assert.Contains("PERFORATED", inside, StringComparison.Ordinal);

        var outside = view.ReadoutAt(500f);

        Assert.Contains("nothing is recorded at this depth", outside, StringComparison.Ordinal);
    }

    [Fact]
    public void A_well_with_no_completion_has_no_data_track()
    {
        var (document, warnings, tracks) = WellboreSourceRules.Build("W-6", "North 6", 3000m, "m", []);

        Assert.Single(tracks);
        Assert.False(new OgWellboreView("W-6", "North 6", 0f, 3000f, "m", tracks, warnings, document)
            .HasDepthAndDataTrack);
        Assert.Contains(warnings, warning => warning.Contains("no data track", StringComparison.Ordinal));
    }

    [Fact]
    public void The_unit_comes_from_the_recorded_uom()
    {
        Assert.Equal(DepthUnit.Feet, WellboreSourceRules.UnitOf("ft"));
        Assert.Equal(DepthUnit.Feet, WellboreSourceRules.UnitOf("F"));
        Assert.Equal(DepthUnit.Meters, WellboreSourceRules.UnitOf("m"));
        Assert.Equal(DepthUnit.Meters, WellboreSourceRules.UnitOf(null));

        Assert.Equal("ft", WellboreSourceRules.Symbol(DepthUnit.Feet));
        Assert.Equal("m", WellboreSourceRules.Symbol(DepthUnit.Meters));
    }

    [Fact]
    public void The_page_loads_a_well_from_the_database_and_states_its_warnings()
    {
        var page = File.ReadAllText(
            SourceRoot.AllSources().Single(source => source.EndsWith(PageFileName, StringComparison.Ordinal)));

        Assert.Contains("WellboreSource.LoadAsync", page, StringComparison.Ordinal);
        Assert.Contains("view.WarningsText", page, StringComparison.Ordinal);
        Assert.Contains("view.Document", page, StringComparison.Ordinal);

        // A refused read states the reason and leaves the template on screen.
        Assert.Contains("The template stays on screen", page, StringComparison.Ordinal);
    }
}
