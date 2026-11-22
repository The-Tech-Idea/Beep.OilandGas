using System.Threading;
using TheTechIdea.Beep.OilandGas.Desktop.WellTools.Bridging;
using TheTechIdea.OilGas.Wellbore;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// VSB-06 AC-3 and AC-4: a cross-section is built from real surface positions, and the horizon builder's
/// pinch-out terminations are reported in the checks rather than left inside the drawing.
/// </summary>
public sealed class CorrelationSectionTests
{
    private static IReadOnlyList<(string Name, TheTechIdea.OilGas.Wellbore.WellboreDocument Document)> Wells() =>
        OgCorrelation.DemoWells();

    [Fact]
    public void A_section_is_built_from_the_wells_real_positions()
    {
        var section = OgCorrelation.BuildSection(Wells(), OgCorrelation.DemoPositions());

        Assert.True(section.Success, section.Message);
        var panel = section.Value!;

        // AC-4: three wells, each at its own easting, and a line through the outermost two.
        Assert.Equal(3, panel.Wells.Count);
        Assert.Equal([0d, 1500d, 3200d], panel.Wells.Select(well => well.X));
        Assert.Equal(2, panel.Line.Count);
        Assert.Equal((0d, 0d), panel.Line[0]);
        Assert.Equal((3200d, 540d), panel.Line[1]);

        // The distances along the line are the real ones, not an index.
        var paths = CrossSectionLayout.Paths(panel);
        var along = paths.OrderBy(path => path.Points[0].s).Select(path => Math.Round(path.Points[0].s, 1)).ToList();
        Assert.Equal(3, along.Count);
        Assert.True(along[0] < along[1] && along[1] < along[2]);
        Assert.InRange(along[2] - along[0], 3_200, 3_300);
    }

    [Fact]
    public void A_well_without_a_position_is_refused_instead_of_drawn_at_zero()
    {
        var positions = OgCorrelation.DemoPositions()
            .Where(position => position.Well != "DEMO-2")
            .ToList();

        var section = OgCorrelation.BuildSection(Wells(), positions);

        Assert.False(section.Success);
        Assert.Equal(OgCorrelation.CodeNoCoordinates, section.Reference);
        Assert.Contains("DEMO-2", section.Message!, StringComparison.Ordinal);
    }

    [Fact]
    public void The_checks_report_every_pinch_out_with_both_wells()
    {
        var panel = OgCorrelation.BuildSection(Wells(), OgCorrelation.DemoPositions()).Value!;
        var checks = OgCorrelation.SectionChecks(panel);

        var pinchOuts = checks.Where(check => check.Code == OgCorrelation.CodePinchOut).ToList();

        // AC-3: the demo set is missing Top B in the third well and Top A in the second, so at least one
        // horizon must taper out rather than merely vanish.
        Assert.NotEmpty(pinchOuts);
        Assert.All(pinchOuts, check =>
        {
            Assert.Equal("Info", check.Severity);
            Assert.Contains("pinches out", check.Message, StringComparison.Ordinal);
            Assert.Contains("DEMO-", check.Message, StringComparison.Ordinal);
        });

        // Top C is in every well, so it cannot pinch out.
        Assert.DoesNotContain(pinchOuts, check => check.Message.Contains("'Top C'", StringComparison.Ordinal));
    }

    [Fact]
    public void A_set_where_every_well_carries_every_top_has_no_pinch_out()
    {
        var wells = OgCorrelation.DemoWells();
        var complete = wells.Select(well => (well.Name, Document: WithAllTops(well.Document))).ToList();

        var panel = OgCorrelation.BuildSection(complete, OgCorrelation.DemoPositions()).Value!;
        var checks = OgCorrelation.SectionChecks(panel);

        // The check is not always-true: complete wells produce no pinch-out.
        Assert.DoesNotContain(checks, check => check.Code == OgCorrelation.CodePinchOut);
    }

    [Fact]
    public void A_render_observes_the_token_between_wells()
    {
        var wells = ManyWells(6);
        using var cancellation = new CancellationTokenSource();

        var progressed = 0;

        // AC-6: the cancel lands after the first well; the next per-well check must observe it.
        Assert.Throws<OperationCanceledException>(() => OgCorrelation.RenderChunked(
            wells,
            OgCorrelation.DemoDatum,
            600,
            400,
            cancellation.Token,
            (done, _) =>
            {
                progressed = done;
                if (done == 1)
                {
                    cancellation.Cancel();
                }
            }));

        Assert.Equal(1, progressed);
    }

    [Fact]
    public void A_cancelled_render_produces_no_frame()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        var progressed = 0;

        // AC-6: a cancel before the render starts means no work and no frame at all.
        Assert.Throws<OperationCanceledException>(() => OgCorrelation.RenderChunked(
            ManyWells(3),
            OgCorrelation.DemoDatum,
            600,
            400,
            cancellation.Token,
            (_, _) => progressed++));

        Assert.Equal(0, progressed);
    }

    [Fact]
    public void Changing_the_well_set_cancels_the_render_in_flight_and_the_latest_wins()
    {
        RunOnSta(() =>
        {
            using var application = new TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping.OgBootstrapper().Create();
            var page = new TheTechIdea.Beep.OilandGas.Desktop.Views.CorrelationPageView(application);

            // AC-6: a large set is still rendering when the set is replaced.
            var first = page.RenderAsync(ManyWells(120), OgCorrelation.DemoDatum);
            var second = page.RenderAsync(OgCorrelation.DemoWells(), OgCorrelation.DemoDatum);

            var cancelled = first.GetAwaiter().GetResult();
            var latest = second.GetAwaiter().GetResult();

            // The superseded render returns nothing, so no stale frame can be applied.
            Assert.Null(cancelled);
            Assert.True(page.LastRenderCancelled);

            Assert.NotNull(latest);
            Assert.Equal(3, latest!.Panel.Wells.Count);
        });
    }

    [Fact]
    public void A_completed_render_carries_the_panel_its_columns_the_checks_and_a_frame()
    {
        var render = OgCorrelation.RenderChunked(
            OgCorrelation.DemoWells(),
            OgCorrelation.DemoDatum,
            600,
            400,
            CancellationToken.None);

        Assert.Equal(3, render.Panel.Wells.Count);
        Assert.Equal(3, render.Columns.Count);
        Assert.NotEmpty(render.Issues);
        Assert.NotNull(render.Frame);
        Assert.Contains(render.Frame!, value => value != 0);
    }

    [Fact]
    public void The_section_renders_a_frame_with_content()
    {
        var panel = OgCorrelation.BuildSection(Wells(), OgCorrelation.DemoPositions()).Value!;
        var frame = OgCorrelation.RenderSection(panel, 800, 500);

        Assert.NotNull(frame);
        Assert.Equal(800 * 500 * 4, frame!.Length);
        Assert.Contains(frame, value => value != 0);
    }

    private static void RunOnSta(Action action)
    {
        Exception? failure = null;

        var thread = new Thread(() =>
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                failure = ex;
            }
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        if (failure is not null)
        {
            throw failure;
        }
    }

    /// <summary>N wells built from the three demo documents, for a render long enough to cancel.</summary>
    private static IReadOnlyList<(string Name, TheTechIdea.OilGas.Wellbore.WellboreDocument Document)> ManyWells(int count)
    {
        var demo = OgCorrelation.DemoWells();

        return Enumerable.Range(1, count)
            .Select(index => ($"W{index}", demo[index % demo.Count].Document))
            .ToList();
    }

    private static TheTechIdea.OilGas.Wellbore.WellboreDocument WithAllTops(
        TheTechIdea.OilGas.Wellbore.WellboreDocument document)
    {
        foreach (var (name, depth) in new[] { ("Top A", 8_250f), ("Top B", 8_250f), ("Top C", 8_400f) })
        {
            if (MarkerMatching.Find(document.Markers, name) is null)
            {
                document.Markers.Add(new DepthMarker { Name = name, Depth = depth, Kind = MarkerKind.Top });
            }
        }

        return document;
    }
}
