using TheTechIdea.Beep.OilandGas.Desktop.Data.Interop;
using TheTechIdea.OilGas.Mapping;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// INT-07: the rules the page states - the method names (AC-1), a projection's round trip (AC-2), the interval the engine chooses
/// (AC-5) and the refusals. The map itself is built in the opt-in test, because it reads wells from a database.
/// </summary>
public sealed class MapTests
{
    private const string PageFileName = "MapPageView.cs";

    [Fact]
    public void The_gridding_methods_are_the_engines_three_and_an_alias_is_accepted()
    {
        // AC-1: the three methods the engine offers, each resolvable by its own name and by the short form a person writes.
        Assert.Equal(GriddingMethod.InverseDistance, OgMapRules.MethodOf("InverseDistance"));
        Assert.Equal(GriddingMethod.InverseDistance, OgMapRules.MethodOf("idw"));
        Assert.Equal(GriddingMethod.InverseDistance, OgMapRules.MethodOf("inverse distance"));

        Assert.Equal(GriddingMethod.MinimumCurvature, OgMapRules.MethodOf("MinimumCurvature"));
        Assert.Equal(GriddingMethod.MinimumCurvature, OgMapRules.MethodOf("mincurv"));

        Assert.Equal(GriddingMethod.OrdinaryKriging, OgMapRules.MethodOf("OrdinaryKriging"));
        Assert.Equal(GriddingMethod.OrdinaryKriging, OgMapRules.MethodOf("kriging"));

        Assert.Null(OgMapRules.MethodOf("triangulation"));
        Assert.Null(OgMapRules.MethodOf(null));
        Assert.Equal(["InverseDistance", "MinimumCurvature", "OrdinaryKriging"], OgMapRules.Methods);
    }

    [Fact]
    public void The_contour_interval_is_the_engines_own_unless_one_is_set()
    {
        // AC-5: a requested interval is used exactly; with none, the engine's round interval decides, giving a handful of lines.
        var overridden = OgMapRules.LevelsOf(1000d, 1100d, 25d, "m", out var usedOverride, out var niceOverride);

        Assert.False(niceOverride);
        Assert.Equal(25d, usedOverride, 9);
        Assert.Equal([1025d, 1050d, 1075d], overridden);
        Assert.All(overridden, level => Assert.True(level > 1000d && level < 1100d));

        var automatic = OgMapRules.LevelsOf(1000d, 1100d, null, "m", out var usedAuto, out var niceAuto);

        Assert.True(niceAuto);
        Assert.True(usedAuto > 0d);
        Assert.NotEmpty(automatic);
        Assert.Equal(usedAuto, Contouring.Interval(100d, "m"), 9);

        // Every level is a multiple of the interval strictly inside the range, which is what the engine promises.
        Assert.All(automatic, level =>
        {
            Assert.True(level > 1000d && level < 1100d);
            Assert.Equal(0d, level % usedAuto, 6);
        });

        // A range with nothing in it gives no lines rather than one at the boundary.
        Assert.Empty(OgMapRules.LevelsOf(1000d, 1000d, null, "m", out _, out _));
        Assert.Empty(OgMapRules.LevelsOf(1000d, 900d, null, "m", out _, out _));

        // A zero or negative interval is not an interval, so the engine chooses instead of refusing.
        var zero = OgMapRules.LevelsOf(0d, 100d, 0d, "m", out var usedZero, out var niceZero);

        Assert.True(niceZero);
        Assert.True(usedZero > 0d);
        Assert.NotEmpty(zero);
    }

    [Fact]
    public void A_projection_returns_the_point_it_was_given()
    {
        // AC-2: the round trip is measured in metres at the point's own latitude, and a UTM zone the wells sit inside returns them.
        var norway = new List<(double Latitude, double Longitude)> { (58.5d, 2.1d), (58.6d, 2.2d) };
        var utm31 = CoordinateSystems.Utm(31, north: true);

        // The round trip is measured in metres: a projection that is right for the field returns the point it was given to within
        // the tolerance, which is what makes a map's grid coordinates trustworthy.
        var error = OgMapRules.RoundTripMetres(utm31, norway);

        Assert.True(error < OgMapRules.RoundTripToleranceMetres,
            "the utm:31N round trip is " + OgMapRules.Figure(error) + " m");

        // The same zone south of the equator is just as exact: nothing about the false northing changes the return.
        var utm31South = CoordinateSystems.Utm(31, north: false);

        Assert.True(OgMapRules.RoundTripMetres(utm31South, norway) < OgMapRules.RoundTripToleranceMetres);

        // No point is no error, which is the honest answer for an empty list rather than a number that reads as a measurement.
        Assert.Equal(0d, OgMapRules.RoundTripMetres(utm31, []));
    }

    [Fact]
    public void The_utm_zone_of_a_field_is_derived_from_where_it_is()
    {
        // A longitude is one of sixty six-degree bands: 2.1 E is zone 31, 5.5 W is zone 30, 179 E is zone 60.
        Assert.Equal(["utm:31N"], OgMapRules.ZonesOf([(58.5d, 2.1d)]));
        Assert.Equal(["utm:30N"], OgMapRules.ZonesOf([(50.0d, -5.5d)]));
        Assert.Equal(["utm:60N"], OgMapRules.ZonesOf([(60.0d, 179.0d)]));

        // The southern hemisphere is named as the engine names it.
        Assert.Equal(["utm:31S"], OgMapRules.ZonesOf([(-33.9d, 2.1d)]));

        // A field straddling a boundary is offered both zones, so neither half is mapped in the other's zone.
        Assert.Equal(["utm:31N", "utm:32N"], OgMapRules.ZonesOf([(58.5d, 2.1d), (58.6d, 7.5d)]));

        Assert.Empty(OgMapRules.ZonesOf([]));
    }

    [Fact]
    public void A_coordinate_system_is_known_by_definition_or_by_utms_own_shape()
    {
        // The environment defines none by default, so a UTM zone is what makes a map projectable at all.
        Assert.True(OgMapRules.IsKnownProjection("utm:31N"));
        Assert.True(OgMapRules.IsKnownProjection("UTM:12S"));
        Assert.True(OgMapRules.IsKnownProjection("geographic"));
        Assert.True(OgMapRules.IsKnownProjection("geographic:WGS 84"));

        Assert.False(OgMapRules.IsKnownProjection("EPSG:32631"));
        Assert.False(OgMapRules.IsKnownProjection("utm:99N"));
        Assert.False(OgMapRules.IsKnownProjection("nonsense"));
        Assert.False(OgMapRules.IsKnownProjection(" "));
        Assert.False(OgMapRules.IsKnownProjection(null));
    }

    [Fact]
    public async Task A_map_is_refused_without_a_projection_a_top_a_known_method_or_a_connection()
    {
        var offline = new OgMapService(() => null, new Quiet());

        var noProjection = await offline.BuildAsync(new OgMapRequest(null, "Hod", null, " ", "InverseDistance", null, 100));

        Assert.False(noProjection.Success);
        Assert.Equal(OgMapRules.CodeProjectionRequired, noProjection.Reference);
        Assert.Contains("projection is required", noProjection.Message!, StringComparison.Ordinal);

        var noTop = await offline.BuildAsync(new OgMapRequest(null, "  ", null, "utm:31N", "InverseDistance", null, 100));

        Assert.Equal(OgMapRules.CodeTopRequired, noTop.Reference);
        Assert.Contains("name the formation top", noTop.Message!, StringComparison.Ordinal);

        var badMethod = await offline.BuildAsync(new OgMapRequest(null, "Hod", null, "utm:31N", "triangulation", null, 100));

        Assert.Equal(OgMapRules.CodeMethodUnknown, badMethod.Reference);
        Assert.Contains("use InverseDistance, MinimumCurvature, OrdinaryKriging", badMethod.Message!, StringComparison.Ordinal);

        // The projection is refused before a connection is asked for, because the refusal is about the name.
        var badProjection = await offline.BuildAsync(new OgMapRequest(null, "Hod", null, "EPSG:32631", "InverseDistance", null, 100));

        Assert.Equal(OgMapRules.CodeProjectionUnknown, badProjection.Reference);
        Assert.Contains("utm:<zone><N|S> needs no definition", badProjection.Message!, StringComparison.Ordinal);

        var noConnection = await offline.BuildAsync(new OgMapRequest(null, "Hod", null, "utm:31N", "InverseDistance", null, 100));

        Assert.Equal(OgMapRules.CodeNoConnection, noConnection.Reference);

        // An export needs something to export, a path, and a format the engine writes.
        var empty = new OgMapWorkspace("utm:31N", "en", "Hod", "m", true, null, [], 0, 0, null, null, [], [], null, 0, 0,
            DateTime.UtcNow);

        Assert.Equal(OgMapRules.CodeExportFailed, (await offline.ExportAsync(empty, "pdf", "map.pdf")).Reference);
        Assert.Equal(OgMapRules.CodeExportPath, (await offline.ExportAsync(empty with { View = new ContourMapView() }, "pdf", " ")).Reference);
        Assert.Equal(OgMapRules.CodeFormatUnknown,
            (await offline.ExportAsync(empty with { View = new ContourMapView() }, "dxf", "map.dxf")).Reference);
    }

    [Fact]
    public void A_well_the_engine_could_not_place_is_listed_with_its_reason()
    {
        // AC-3: the map's own table carries the verdict, and the page shows the excluded wells rather than dropping them.
        var workspace = new OgMapWorkspace(
            "utm:31N", "en", "Hod (TVDSS)", "m", true, null,
            [
                new OgMapWell("W-1", true, null, 500000, 6480000, 2050, 1200, 2050, []),
                new OgMapWell("W-2", false, "no top named 'Hod'", 0, 0, null, null, null, []),
            ],
            1, 1, null, null, [], [], null, 0, 0, DateTime.UtcNow);

        Assert.Contains("1 well(s) on the map", workspace.SummaryText, StringComparison.Ordinal);
        Assert.Contains("1 left out", workspace.SummaryText, StringComparison.Ordinal);

        Assert.Contains("W-1", workspace.WellsText, StringComparison.Ordinal);
        Assert.Contains("W-2", workspace.WellsText, StringComparison.Ordinal);

        Assert.Contains("✗ W-2: no top named 'Hod'", workspace.ExcludedText, StringComparison.Ordinal);
        Assert.DoesNotContain("W-1", workspace.ExcludedText, StringComparison.Ordinal);

        // A field where everything was placed says so instead of showing an empty list of failures.
        var allPlaced = workspace with { Excluded = 0 };

        Assert.Contains("Every well in scope was placed", allPlaced.ExcludedText, StringComparison.Ordinal);
    }

    [Fact]
    public void A_map_that_could_not_be_built_states_why_rather_than_showing_an_empty_one()
    {
        var refused = new OgMapWorkspace(
            "utm:31N", "en", "Hod (TVDSS)", "m", MapPossible: false,
            "a surface needs at least 3 placed wells and only 2 could be placed",
            [], 2, 0, null, null, [], [], null, 0, 0, DateTime.UtcNow);

        Assert.Contains("No map: a surface needs at least 3 placed wells", refused.SummaryText, StringComparison.Ordinal);
        Assert.Contains("nothing to check", refused.QcText, StringComparison.Ordinal);
    }

    [Fact]
    public void The_quality_check_and_the_export_report_what_they_found()
    {
        // AC-4: the gridding result's own figures, and the worst residuals named by well.
        var qc = new OgMapQc(12, 1, 40, 350d, "m", 3.5d, 1.25d, OvershootsData: true, ["W-3 3.5", "W-1 -2.0"]);

        Assert.Contains("12 point(s)", qc.Describe(), StringComparison.Ordinal);
        Assert.Contains("farthest cell 350 m from a datum", qc.Describe(), StringComparison.Ordinal);
        Assert.Contains("largest residual 3.5", qc.Describe(), StringComparison.Ordinal);
        Assert.Contains("goes beyond the data's range", qc.Describe(), StringComparison.Ordinal);

        // AC-6: what was written, where, and how large - so a person can see it was written.
        var exported = new OgMapExport("pdf", @"C:\out\map.pdf", 240_000, true, "PDF written: 240000 bytes");

        Assert.Contains("PDF", exported.Summary, StringComparison.Ordinal);
        Assert.Contains("234.4 KB", exported.Summary, StringComparison.Ordinal);
        Assert.Contains(@"C:\out\map.pdf", exported.Summary, StringComparison.Ordinal);
        Assert.DoesNotContain("not written", exported.Summary, StringComparison.Ordinal);

        Assert.Contains("(not written)", (exported with { Written = false }).Summary, StringComparison.Ordinal);
    }

    [Fact]
    public void The_page_hosts_the_owners_view_and_clears_it_when_a_build_fails()
    {
        var page = File.ReadAllText(
            SourceRoot.AllSources().Single(source => source.EndsWith(PageFileName, StringComparison.Ordinal)));

        // The map is the owner's view, hosted rather than redrawn: the surface comes from the workspace.
        Assert.Contains("_host.Surface = workspace.View", page, StringComparison.Ordinal);
        Assert.Contains("_host.Redraw()", page, StringComparison.Ordinal);
        Assert.Contains("_projectionGrid.ItemsSource = _projections", page, StringComparison.Ordinal);
        Assert.Contains("workspace.ExcludedText", page, StringComparison.Ordinal);
        Assert.Contains("workspace.QcText", page, StringComparison.Ordinal);

        // A failed build takes the last surface off the screen.
        Assert.Contains("_host.Surface = null", page, StringComparison.Ordinal);

        // AC-6: the export goes through the owner's writer and its result is shown.
        Assert.Contains("exported.Value.Summary", page, StringComparison.Ordinal);
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
