using System.Windows;
using TheTechIdea.Beep.OilandGas.Desktop.WellTools.Bridging;
using TheTechIdea.Beep.OilandGas.Desktop.WellTools.Environment;
using TheTechIdea.Beep.OilandGas.Desktop.WellTools.Hosting;
using TheTechIdea.OilGas.Wellbore;
using TheTechIdea.OilGas.Wellbore.Data;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// VSB-03: a <c>WellboreDocument</c> is produced by the mapper and drawn through the host — the demo
/// document now, a canonical <c>WellModel</c> as soon as PPDM can supply one.
/// </summary>
public sealed class WellboreDocumentTests
{
    [Fact]
    public void The_triple_combo_demo_document_has_tracks_and_a_depth_range()
    {
        var document = OgDocumentMapper.TripleComboDemo();
        var summary = OgDocumentMapper.Summarise(document);

        Assert.True(summary.TrackCount > 1, $"Only {summary.TrackCount} track(s) in the demo document.");
        Assert.True(summary.Bottom > summary.Top, "The demo document has no depth range.");
        Assert.False(string.IsNullOrWhiteSpace(summary.DepthUnit));
    }

    [Fact]
    public void The_mapper_builds_a_dispatchable_document_from_a_well_model()
    {
        var well = OgDocumentMapper.BuildDemoWell("MAPPED-1");

        var document = OgDocumentMapper.FromWellModel(well);
        var summary = OgDocumentMapper.Summarise(document);

        Assert.Equal("MAPPED-1", summary.WellName);
        Assert.True(summary.TrackCount >= 2, $"Only {summary.TrackCount} track(s) were built from the model.");
        Assert.True(summary.MarkerCount >= 0);
        Assert.True(summary.Bottom > summary.Top, "The mapped document has no depth range.");
    }

    [Fact]
    public void A_well_model_without_depth_logs_produces_an_empty_document_not_an_exception()
    {
        var empty = new WellModel();

        var document = OgDocumentMapper.FromWellModel(empty);
        var summary = OgDocumentMapper.Summarise(document);

        // The page must be able to say "no depth data" rather than draw an empty plot as if it were the well.
        Assert.Equal(0, summary.TrackCount);
    }

    [Fact]
    public void Rendering_the_demo_document_produces_non_empty_pixels()
    {
        UseDefaultEnvironment();

        using var renderer = new ViewSurfaceRenderer
        {
            Surface = new WellboreView { Document = OgDocumentMapper.TripleComboDemo() },
        };

        Assert.True(renderer.Render(400, 300));
        Assert.True(renderer.HasFrame);

        var painted = renderer.Frame!.Count(value => value != 0);
        Assert.True(painted > 0, "The host produced a fully transparent frame.");
    }

    [Fact]
    public void A_device_scaled_render_keeps_input_in_surface_coordinates()
    {
        UseDefaultEnvironment();

        using var renderer = new ViewSurfaceRenderer
        {
            Surface = new WellboreView { Document = OgDocumentMapper.TripleComboDemo() },
        };

        Assert.True(renderer.Render(200, 150, scale: 2f));
        Assert.Equal(400, renderer.PixelWidth);
        Assert.Equal(300, renderer.PixelHeight);

        // A pointer at (40,30) on a 2x display is logical (20,15) inside the surface.
        var point = renderer.ToSurfacePoint(40, 30);
        Assert.Equal(20f, point.X, 3);
        Assert.Equal(15f, point.Y, 3);

        // And the surface was told its logical size, not the bitmap size.
        Assert.Equal(200f, renderer.Surface!.Width);
        Assert.Equal(150f, renderer.Surface.Height);
    }

    [Fact]
    public void The_wpf_host_draws_the_demo_document()
    {
        UseDefaultEnvironment();

        RunOnSta(() =>
        {
            using var host = new SkiaViewSurfaceHost
            {
                Surface = new WellboreView { Document = OgDocumentMapper.TripleComboDemo() },
            };

            host.Measure(new Size(360, 320));
            host.Arrange(new Rect(0, 0, 360, 320));

            Assert.True(host.Redraw(), "The host did not render the document.");
            Assert.NotNull(host.Frame);
            Assert.Contains(host.Frame!, value => value != 0);
        });
    }

    private static void UseDefaultEnvironment()
    {
        // Surfaces read the environment when they are constructed, so it must exist first (VSB-02).
        OgToolboxBootstrap.RegisterAll();
        WellboreServices.Global = new WellboreServices();
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
}
