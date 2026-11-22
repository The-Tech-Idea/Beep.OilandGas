using System.Windows;
using System.Windows.Automation.Peers;
using SkiaSharp;
using TheTechIdea.Beep.OilandGas.Desktop.WellTools.Hosting;
using TheTechIdea.OilGas.Wellbore;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// VSB-01 closing items: pinch and two-finger pan reach the surface, the canvas is exposed to assistive
/// technology, and <c>Invalidated</c> causes exactly one redraw.
/// </summary>
public sealed class ViewSurfaceInteractionTests
{
    [Fact]
    public void A_pinch_reaches_the_surface_with_its_scale()
    {
        using var renderer = new ViewSurfaceRenderer { Surface = new InteractiveSurface() };
        renderer.Render(200, 120, scale: 2f);

        Assert.True(renderer.Pinch(40, 30, 1.25f));

        // 40/2 = 20, 30/2 = 15; the scale factor is passed through unchanged.
        Assert.Contains("pinch:20,15:1.25", ((InteractiveSurface)renderer.Surface).Inputs);
    }

    [Fact]
    public void A_two_finger_pan_is_converted_to_surface_units()
    {
        using var renderer = new ViewSurfaceRenderer { Surface = new InteractiveSurface() };
        renderer.Render(200, 120, scale: 2f);

        Assert.True(renderer.Pan(20, 10));

        Assert.Contains("pan:10,5", ((InteractiveSurface)renderer.Surface).Inputs);
    }

    [Fact]
    public void The_host_exposes_the_surfaces_accessible_name_and_children()
    {
        RunOnSta(() =>
        {
            using var host = new SkiaViewSurfaceHost { Surface = new InteractiveSurface() };

            var peer = UIElementAutomationPeer.CreatePeerForElement(host);

            Assert.NotNull(peer);
            Assert.Equal("Probe view", peer!.GetName());

            var children = peer.GetChildren();
            Assert.NotNull(children);
            Assert.Equal(2, children!.Count);
            Assert.Equal("Depth", children[0].GetName());
            Assert.Equal("Gamma ray", children[1].GetName());
        });
    }

    [Fact]
    public void An_invalidated_surface_causes_exactly_one_redraw()
    {
        RunOnSta(() =>
        {
            var surface = new InteractiveSurface();
            using var host = new SkiaViewSurfaceHost { Surface = surface };

            host.Measure(new Size(200, 150));
            host.Arrange(new Rect(0, 0, 200, 150));
            host.Redraw();

            var before = host.RenderCount;
            Assert.True(before > 0, "The host did not render before the invalidation.");

            surface.RaiseInvalidated();

            Assert.Equal(before + 1, host.RenderCount);
        });
    }

    [Fact]
    public void A_render_failure_is_reported_and_contained()
    {
        RunOnSta(() =>
        {
            var reporter = new RecordingFailureReporter();
            using var host = new SkiaViewSurfaceHost
            {
                Surface = new ThrowingSurface(),
                FailureReporter = reporter,
            };

            host.Measure(new Size(120, 90));
            host.Arrange(new Rect(0, 0, 120, 90));

            // The failure is contained: no exception escapes the redraw.
            Assert.False(host.Redraw());
            Assert.Single(reporter.Failures);
            Assert.Equal(0, host.RenderCount);
        });
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

    /// <summary>Records what the host reported, so a failure cannot disappear.</summary>
    private sealed class RecordingFailureReporter : TheTechIdeaWeb.Diagnostics.IFailureReporter
    {
        public List<string> Failures { get; } = [];

        public string ReportHandled(
            Exception exception,
            string operation,
            string consequence,
            TheTechIdeaWeb.Diagnostics.FailureSeverity severity = TheTechIdeaWeb.Diagnostics.FailureSeverity.Error)
        {
            Failures.Add($"{severity}: {operation} — {consequence}");
            return TheTechIdeaWeb.Diagnostics.FailureReferenceFormat.New();
        }
    }

    /// <summary>A surface whose drawing always fails.</summary>
    private sealed class ThrowingSurface : ViewSurface
    {
        protected override void DrawContent(SKCanvas canvas) =>
            throw new InvalidOperationException("probe: drawing failed");
    }

    /// <summary>A surface with two accessible children that records pinch/pan and can invalidate itself.</summary>
    private sealed class InteractiveSurface : ViewSurface
    {
        public InteractiveSurface()
        {
            Name = "Probe view";
        }

        public List<string> Inputs { get; } = [];

        public override string AccessibleName => "Probe view";

        public override string AccessibleDescription => "A probe surface for tests";

        public override IReadOnlyList<AccessibleItem> AccessibleChildren { get; } =
        [
            new AccessibleItem("Depth", "depth track", "Depth axis"),
            new AccessibleItem("Gamma ray", "curve", "GR", selected: true),
        ];

        public void RaiseInvalidated() => InvalidateVisual();

        protected override void DrawContent(SKCanvas canvas) => canvas.Clear(new SKColor(0x0B, 0x6E, 0x4F));

        public override bool HandlePinch(SKPoint center, float scale)
        {
            Inputs.Add($"pinch:{center.X:0},{center.Y:0}:{scale:0.###}");
            return true;
        }

        public override bool HandleTwoFingerPan(SKPoint delta)
        {
            Inputs.Add($"pan:{delta.X:0},{delta.Y:0}");
            return true;
        }
    }
}
