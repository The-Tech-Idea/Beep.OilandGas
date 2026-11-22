using System.Windows;
using SkiaSharp;
using TheTechIdea.Beep.OilandGas.Desktop.WellTools.Hosting;
using TheTechIdea.OilGas.Wellbore;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// VSB-01: the SkiaSharp host. Proves a <see cref="ViewSurface"/> is drawn into a frame, that pointer and
/// key input reach it in surface coordinates, and that the WPF host presents the frame — all without
/// <c>SkiaSharp.Views.WPF</c> (ADR-002).
/// </summary>
public sealed class ViewSurfaceHostTests
{
    [Fact]
    public void Rendering_a_surface_produces_a_bgra_frame()
    {
        using var renderer = new ViewSurfaceRenderer();
        renderer.Surface = new RecordingSurface();

        var rendered = renderer.Render(200, 120);

        Assert.True(rendered);
        Assert.True(renderer.HasFrame);
        Assert.Equal(200, renderer.PixelWidth);
        Assert.Equal(120, renderer.PixelHeight);
        Assert.Equal(200 * 4, renderer.FrameStride);
        Assert.Equal(1, renderer.RenderCount);
        Assert.Equal(1, ((RecordingSurface)renderer.Surface).Draws);
    }

    [Fact]
    public void Rendering_applies_the_device_scale_to_the_bitmap_but_not_to_the_surface()
    {
        using var renderer = new ViewSurfaceRenderer();
        var surface = new RecordingSurface();
        renderer.Surface = surface;

        renderer.Render(100, 50, scale: 1.5f);

        Assert.Equal(150, renderer.PixelWidth);
        Assert.Equal(75, renderer.PixelHeight);
        Assert.Equal(100f, surface.Width);
        Assert.Equal(50f, surface.Height);
    }

    [Fact]
    public void A_pointer_event_reaches_the_surface_in_surface_coordinates()
    {
        using var renderer = new ViewSurfaceRenderer();
        var surface = new RecordingSurface();
        renderer.Surface = surface;
        renderer.Render(200, 120, scale: 2f);

        Assert.True(renderer.MouseDown(40, 30));

        // 40 device-independent units at scale 2 is logical 20.
        Assert.Contains("down:20,15", surface.Inputs);
    }

    [Fact]
    public void Move_up_and_wheel_are_forwarded()
    {
        using var renderer = new ViewSurfaceRenderer();
        var surface = new RecordingSurface();
        renderer.Surface = surface;
        renderer.Render(200, 120);

        renderer.MouseMove(5, 6);
        renderer.MouseUp(7, 8);
        renderer.MouseWheel(9, 10, 120);

        Assert.Contains("move:5,6", surface.Inputs);
        Assert.Contains("up:7,8", surface.Inputs);
        Assert.Contains("wheel:9,10:120", surface.Inputs);
    }

    [Fact]
    public void A_key_event_reaches_the_surface_with_its_modifiers()
    {
        using var renderer = new ViewSurfaceRenderer();
        var surface = new RecordingSurface();
        renderer.Surface = surface;
        renderer.Render(200, 120);

        Assert.True(renderer.Key(ViewKey.Enter));
        Assert.True(renderer.Key(ViewKey.Z, ViewModifiers.Control));

        Assert.Contains("key:Enter:None", surface.Inputs);
        Assert.Contains("key:Z:Control", surface.Inputs);
    }

    [Fact]
    public void Nothing_is_drawn_without_a_surface_or_a_size()
    {
        using var renderer = new ViewSurfaceRenderer();

        Assert.False(renderer.Render(200, 120));
        Assert.False(renderer.HasFrame);

        renderer.Surface = new RecordingSurface();
        Assert.False(renderer.Render(0, 120));
        Assert.False(renderer.HasFrame);
    }

    [Fact]
    public void Every_documented_key_maps_to_a_surface_key()
    {
        Assert.True(ViewKeyMap.TryMap(System.Windows.Input.Key.Up, out var up));
        Assert.Equal(ViewKey.Up, up);
        Assert.True(ViewKeyMap.TryMap(System.Windows.Input.Key.OemPlus, out var plus));
        Assert.Equal(ViewKey.Plus, plus);
        Assert.True(ViewKeyMap.TryMap(System.Windows.Input.Key.OemMinus, out var minus));
        Assert.Equal(ViewKey.Minus, minus);

        // A key with no surface meaning is not invented.
        Assert.False(ViewKeyMap.TryMap(System.Windows.Input.Key.F5, out _));

        Assert.Equal(
            ViewModifiers.Shift | ViewModifiers.Control,
            ViewKeyMap.Modifiers(System.Windows.Input.ModifierKeys.Shift | System.Windows.Input.ModifierKeys.Control));
    }

    [Fact]
    public void The_wpf_host_presents_the_rendered_frame()
    {
        // WPF controls need an STA thread; the test host runs MTA.
        RunOnSta(() =>
        {
            using var host = new SkiaViewSurfaceHost();
            var surface = new RecordingSurface();
            host.Surface = surface;

            host.Measure(new Size(160, 90));
            host.Arrange(new Rect(0, 0, 160, 90));

            Assert.True(host.Redraw(), "The host did not render a frame.");
            Assert.NotNull(host.Frame);
            Assert.Equal(1, host.RenderCount);
            Assert.Equal(1, surface.Draws);
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

    /// <summary>A surface that renders a solid colour and records what it received.</summary>
    private sealed class RecordingSurface : ViewSurface
    {
        public int Draws { get; private set; }

        public List<string> Inputs { get; } = [];

        protected override void DrawContent(SKCanvas canvas)
        {
            Draws++;
            canvas.Clear(new SKColor(0x0B, 0x6E, 0x4F));
        }

        protected override bool OnMouseDown(SKPoint point)
        {
            Inputs.Add($"down:{point.X:0},{point.Y:0}");
            return true;
        }

        protected override bool OnMouseMove(SKPoint point)
        {
            Inputs.Add($"move:{point.X:0},{point.Y:0}");
            return true;
        }

        protected override bool OnMouseUp(SKPoint point)
        {
            Inputs.Add($"up:{point.X:0},{point.Y:0}");
            return true;
        }

        public override bool HandleMouseWheel(SKPoint point, float delta)
        {
            Inputs.Add($"wheel:{point.X:0},{point.Y:0}:{delta:0}");
            return true;
        }

        public override bool HandleKey(ViewKey key, ViewModifiers modifiers = ViewModifiers.None)
        {
            Inputs.Add($"key:{key}:{modifiers}");
            return true;
        }
    }
}
