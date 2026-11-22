using System.Windows;
using TheTechIdea.Beep.OilandGas.Desktop.WellTools.Bridging;
using TheTechIdea.Beep.OilandGas.Desktop.WellTools.Environment;
using TheTechIdea.Beep.OilandGas.Desktop.WellTools.Hosting;
using TheTechIdea.OilGas.Wellbore;
using TheTechIdea.OilGas.Wellbore.Data;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// VSB-03 closing items: the track-set selector, marker and annotation editing through the view's own
/// API, the cursor readout, and click forwarding from the host.
/// </summary>
public sealed class WellboreEditingTests
{
    [Fact]
    public void Every_track_template_produces_a_distinct_document()
    {
        var triple = OgDocumentMapper.Summarise(OgDocumentMapper.Create(OgTrackTemplate.TripleComboDemo));
        var model = OgDocumentMapper.Summarise(OgDocumentMapper.Create(OgTrackTemplate.ModelLogSet));
        var depthOnly = OgDocumentMapper.Summarise(OgDocumentMapper.Create(OgTrackTemplate.DepthOnly));

        Assert.True(triple.TrackCount > model.TrackCount, $"{triple.TrackCount} vs {model.TrackCount}");
        Assert.True(model.TrackCount > depthOnly.TrackCount, $"{model.TrackCount} vs {depthOnly.TrackCount}");
        Assert.Equal(1, depthOnly.TrackCount);
        Assert.Equal(3, OgDocumentMapper.Templates.Count);
    }

    [Fact]
    public void A_marker_is_added_at_the_cursor_depth_and_undone()
    {
        UseDefaultEnvironment();
        var view = new WellboreView { Document = OgDocumentMapper.Create(OgTrackTemplate.TripleComboDemo) };
        var before = view.Document!.Markers.Count;

        view.View.CursorDepth = 8250f;
        var marker = view.AddMarker("TEST-1", view.View.CursorDepth!.Value);

        Assert.Equal(before + 1, view.Document.Markers.Count);
        Assert.Equal(8250f, marker.Depth, 3);
        Assert.Contains(view.Document.Markers, item => ReferenceEquals(item, marker));

        Assert.True(view.UndoMarker());
        Assert.Equal(before, view.Document.Markers.Count);
    }

    [Fact]
    public void An_annotation_is_added_and_removed()
    {
        UseDefaultEnvironment();
        var view = new WellboreView { Document = OgDocumentMapper.Create(OgTrackTemplate.DepthOnly) };
        var before = view.Document!.Annotations.Count;

        var annotation = new WellboreAnnotation
        {
            Kind = AnnotationKind.Note,
            Depth = 2100f,
            Text = "Note 1",
        };

        view.AddAnnotation(annotation);
        Assert.Equal(before + 1, view.Document.Annotations.Count);

        Assert.True(view.RemoveAnnotation(annotation.Id));
        Assert.Equal(before, view.Document.Annotations.Count);
    }

    [Fact]
    public void The_readout_reports_the_depth_and_the_element_under_the_pointer()
    {
        UseDefaultEnvironment();
        var view = new WellboreView { Document = OgDocumentMapper.Create(OgTrackTemplate.TripleComboDemo) };

        // Hit testing needs the renderer's last contexts, so draw once first.
        using var renderer = new ViewSurfaceRenderer { Surface = view };
        Assert.True(renderer.Render(400, 300));

        view.View.CursorDepth = 8260f;
        var readout = OgCursorReadout.At(view, 200, 150);

        Assert.Equal(8260f, readout.Depth);
        Assert.Equal("ft", readout.Unit);
        Assert.Contains("8260.0", readout.Describe());
        Assert.Contains("ft", readout.Describe());
    }

    [Fact]
    public void The_readout_says_so_when_there_is_no_cursor()
    {
        UseDefaultEnvironment();
        var view = new WellboreView { Document = OgDocumentMapper.Create(OgTrackTemplate.DepthOnly) };

        var readout = OgCursorReadout.At(view, 10, 10);

        Assert.Null(readout.Depth);
        Assert.StartsWith("No cursor", readout.Describe());
    }

    [Fact]
    public void A_click_on_the_host_is_forwarded_with_surface_coordinates()
    {
        RunOnSta(() =>
        {
            UseDefaultEnvironment();

            using var host = new SkiaViewSurfaceHost
            {
                Surface = new WellboreView { Document = OgDocumentMapper.Create(OgTrackTemplate.DepthOnly) },
            };

            host.Measure(new Size(400, 300));
            host.Arrange(new Rect(0, 0, 400, 300));

            OgSurfacePointer? clicked = null;
            OgSurfacePointer? moved = null;
            host.Clicked += (_, pointer) => clicked = pointer;
            host.PointerMoved += (_, pointer) => moved = pointer;

            host.ForwardPointerMove(40, 30);
            host.ForwardClick(40, 30);

            Assert.NotNull(moved);
            Assert.Equal(40, moved!.X, 3);
            Assert.NotNull(clicked);
            Assert.Equal(40, clicked!.X, 3);
            Assert.Equal(moved.SurfacePoint.X, clicked.SurfacePoint.X, 5);
        });
    }

    private static void UseDefaultEnvironment()
    {
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
