using TheTechIdea.Beep.OilandGas.Desktop.WellTools.Bridging;
using TheTechIdea.Beep.OilandGas.Desktop.WellTools.Environment;
using TheTechIdea.Beep.OilandGas.Desktop.WellTools.Hosting;
using TheTechIdea.OilGas.Wellbore;
using TheTechIdea.OilGas.Wellbore.Data;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// VSB-07: the survey stations come from the tools' <c>Trajectory</c>, plan and section use the same
/// stations, the ladder and cylinder come from <c>AntiCollision.Scan</c>, and a vertical well states its
/// section assumption.
/// </summary>
public sealed class WellPathViewTests
{
    [Fact]
    public void A_vertical_leg_maps_md_to_the_same_tvd()
    {
        UseDefaultEnvironment();

        var stations = OgWellPath.Stations();

        // AC-1: in a vertical section TVD equals MD exactly — the tools' own conversion.
        var vertical = stations.Where(station => station.Inclination == 0 && station.Md <= 500).ToList();
        Assert.NotEmpty(vertical);
        Assert.All(vertical, station => Assert.Equal(station.Md, station.Tvd, 3));

        // And below the build the TVD is shorter than the MD, because the well is deviated.
        var last = stations[^1];
        Assert.True(last.Tvd < last.Md, $"TVD {last.Tvd} should be less than MD {last.Md} for a deviated well.");
        Assert.True(last.North > 0);
    }

    [Fact]
    public void Plan_and_section_use_the_same_stations()
    {
        UseDefaultEnvironment();

        var stations = OgWellPath.Stations();
        var (azimuth, _, _) = OgWellPath.SectionAzimuth();

        var plan = OgWellPath.PlanPath(stations);
        var section = OgWellPath.SectionPath(stations, azimuth);

        // AC-2: one station list, two projections — same count, same MD order.
        Assert.Equal(stations.Count, plan.Count);
        Assert.Equal(stations.Count, section.Count);
        Assert.Equal(stations.Select(station => station.Md), plan.Select(point => point.Md));
        Assert.Equal(plan.Select(point => point.Md), section.Select(point => point.Md));
    }

    [Fact]
    public void The_ladder_and_cylinder_come_from_the_tools_separation_scan()
    {
        UseDefaultEnvironment();

        var (result, _) = OgWellPath.Collision();

        // AC-3: the result is the tools' AntiCollisionResult over the demo offset well.
        Assert.NotEmpty(result.Offsets);
        Assert.All(result.Offsets, offset => Assert.NotEmpty(offset.Points));

        var minimum = OgWellPath.Minimum(result);
        Assert.NotNull(minimum);
        Assert.True(minimum!.Value.Factor > 0);
        Assert.False(string.IsNullOrWhiteSpace(minimum.Value.Offset));

        var ladder = OgWellPath.LadderFrame(result);
        var cylinder = OgWellPath.CylinderFrame(result);

        Assert.NotNull(ladder);
        Assert.NotNull(cylinder);
        Assert.Contains(ladder!, value => value != 0);
        Assert.Contains(cylinder!, value => value != 0);
    }

    [Fact]
    public void The_uncertainty_statement_matches_whether_ellipses_are_available()
    {
        UseDefaultEnvironment();

        var uncertainty = OgWellPath.Uncertainty();

        Assert.False(string.IsNullOrWhiteSpace(uncertainty.Statement));

        // AC-4: availability and the list agree, and the statement says which it is.
        Assert.Equal(uncertainty.Available, uncertainty.Ellipses.Count > 0);

        if (uncertainty.Available)
        {
            Assert.Contains("error model", uncertainty.Statement, StringComparison.OrdinalIgnoreCase);
            Assert.All(uncertainty.Ellipses, ellipse => Assert.True(ellipse.SemiMajor > 0));
        }
        else
        {
            Assert.Contains("No uncertainty model", uncertainty.Statement, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void Ellipses_are_drawn_only_when_they_are_supplied()
    {
        UseDefaultEnvironment();

        var stations = OgWellPath.Stations();
        var plan = OgWellPath.PlanPath(stations);

        var surface = new OgPathSurface { Title = "Plan", Points = plan };
        using var renderer = new ViewSurfaceRenderer { Surface = surface };

        Assert.True(renderer.Render(420, 320));
        var without = renderer.Frame!.ToArray();

        // An explicit ellipse changes the picture…
        surface.Ellipses =
        [
            new OgStationEllipse(stations[^1].North, stations[^1].East, 40, 18, 30),
        ];

        Assert.True(renderer.Render(420, 320));
        Assert.False(
            without.AsSpan().SequenceEqual(renderer.Frame!),
            "An ellipse was supplied but the frame did not change.");

        // …and removing it restores the previous frame exactly, so the difference came from the ellipse.
        surface.Ellipses = [];
        Assert.True(renderer.Render(420, 320));
        Assert.True(without.AsSpan().SequenceEqual(renderer.Frame!), "Clearing the ellipses did not restore the frame.");
    }

    [Fact]
    public void A_vertical_well_states_the_section_assumption()
    {
        UseDefaultEnvironment();

        var vertical = Trajectory.Compute([(0d, 0d, 0d), (500d, 0d, 0d), (1000d, 0d, 0d)], "m");
        var (azimuth, assumed, note) = OgWellPath.SectionAzimuth(vertical);

        // AC-5: 0° is used, and the page says why rather than presenting it as measured.
        Assert.Equal(0, azimuth);
        Assert.True(assumed);
        Assert.NotNull(note);
        Assert.Contains("vertical", note!, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("0°", note, StringComparison.Ordinal);
    }

    [Fact]
    public void A_deviated_well_has_a_measured_section_azimuth()
    {
        UseDefaultEnvironment();

        var (azimuth, assumed, note) = OgWellPath.SectionAzimuth();

        Assert.False(assumed);
        Assert.Null(note);

        // The demo builds towards 45°, so the section bearing is in that neighbourhood.
        Assert.InRange(azimuth, 30, 60);
    }

    [Fact]
    public void The_plan_and_section_surfaces_render_a_path()
    {
        UseDefaultEnvironment();

        var stations = OgWellPath.Stations();
        var (azimuth, _, _) = OgWellPath.SectionAzimuth();

        using var renderer = new ViewSurfaceRenderer
        {
            Surface = new OgPathSurface
            {
                Title = "Plan",
                Points = OgWellPath.PlanPath(stations),
            },
        };

        Assert.True(renderer.Render(420, 320));
        Assert.Contains(renderer.Frame!, value => value != 0);

        renderer.Surface = new OgPathSurface
        {
            Title = "Section",
            Points = OgWellPath.SectionPath(stations, azimuth),
        };

        Assert.True(renderer.Render(420, 320));
        Assert.Contains(renderer.Frame!, value => value != 0);
    }

    [Fact]
    public void An_empty_path_surface_says_so_instead_of_drawing_nothing()
    {
        UseDefaultEnvironment();

        using var renderer = new ViewSurfaceRenderer { Surface = new OgPathSurface { Title = "Plan" } };

        Assert.True(renderer.Render(320, 240));
        Assert.Contains(renderer.Frame!, value => value != 0);
    }

    private static void UseDefaultEnvironment()
    {
        OgToolboxBootstrap.RegisterAll();
        WellboreServices.Global = new WellboreServices();
    }
}
