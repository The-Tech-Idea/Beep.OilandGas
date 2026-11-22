using SkiaSharp;
using TheTechIdea.OilGas.Wellbore;

namespace TheTechIdea.Beep.OilandGas.Desktop.WellTools.Hosting;

/// <summary>
/// Renders an <see cref="ViewSurface"/> into a BGRA bitmap and forwards input to it.
/// </summary>
/// <remarks>
/// <para>
/// ADR-002: the desktop does not use <c>SkiaSharp.Views.WPF</c> — that package ships .NET Framework
/// assets only. This renderer produces raw pixels, and the WPF host copies them into a
/// <c>WriteableBitmap</c>. Keeping the Skia work here means the whole path is testable without a
/// dispatcher.
/// </para>
/// <para>
/// Coordinates: the surface is told its size in logical units and draws at <c>Width × Height</c>;
/// the bitmap is produced at <c>width × scale</c>. Input is converted back by dividing by the scale,
/// so a pointer always lands on the same logical point whatever the display DPI.
/// </para>
/// </remarks>
public sealed class ViewSurfaceRenderer : IDisposable
{
    private SKSurface? _surface;
    private bool _disposed;

    /// <summary>The surface to draw. Setting it does not render.</summary>
    public ViewSurface? Surface { get; set; }

    /// <summary>
    /// Drawn over the surface after it has painted, in surface coordinates. Used for marks the surface
    /// itself cannot make — VSB-05's unverified-barrier hatching, for example.
    /// </summary>
    public Action<SKCanvas>? Overlay { get; set; }

    /// <summary>The last frame, BGRA premultiplied, top-down, ready for a <c>Pbgra32</c> bitmap.</summary>
    public byte[]? Frame { get; private set; }

    public int PixelWidth { get; private set; }

    public int PixelHeight { get; private set; }

    public int FrameStride => PixelWidth * 4;

    /// <summary>The device scale last rendered at. 1 means one bitmap pixel per logical unit.</summary>
    public float Scale { get; private set; } = 1f;

    /// <summary>How many frames have been produced. Used by the smoke run and tests.</summary>
    public int RenderCount { get; private set; }

    public bool HasFrame => Frame is not null && PixelWidth > 0 && PixelHeight > 0;

    /// <summary>
    /// Draws the surface. Returns false when there is nothing to draw (no surface, or no size).
    /// </summary>
    public bool Render(int width, int height, float scale = 1f)
    {
        if (Surface is null || width <= 0 || height <= 0)
        {
            return false;
        }

        scale = scale <= 0 ? 1f : scale;

        var pixelWidth = Math.Max(1, (int)Math.Round(width * scale));
        var pixelHeight = Math.Max(1, (int)Math.Round(height * scale));

        EnsureSurface(pixelWidth, pixelHeight);

        Surface.Width = width;
        Surface.Height = height;

        var canvas = _surface!.Canvas;
        canvas.Clear(SKColors.Transparent);
        Surface.Draw(canvas);

        // Over the surface, before the snapshot: a mark the surface itself does not make.
        Overlay?.Invoke(canvas);

        canvas.Flush();

        using var image = _surface.Snapshot();
        using var bitmap = SKBitmap.FromImage(image);

        var frame = new byte[pixelWidth * pixelHeight * 4];
        bitmap.GetPixelSpan().CopyTo(frame);

        Frame = frame;
        PixelWidth = pixelWidth;
        PixelHeight = pixelHeight;
        Scale = scale;
        RenderCount++;

        return true;
    }

    /// <summary>Converts a host coordinate (device-independent) into surface space.</summary>
    public SKPoint ToSurfacePoint(double x, double y) =>
        new((float)(x / Scale), (float)(y / Scale));

    public bool MouseDown(double x, double y) => Surface?.HandleMouseDown(ToSurfacePoint(x, y)) ?? false;

    public bool MouseMove(double x, double y) => Surface?.HandleMouseMove(ToSurfacePoint(x, y)) ?? false;

    public bool MouseUp(double x, double y) => Surface?.HandleMouseUp(ToSurfacePoint(x, y)) ?? false;

    public bool MouseWheel(double x, double y, float delta) =>
        Surface?.HandleMouseWheel(ToSurfacePoint(x, y), delta) ?? false;

    public bool Key(ViewKey key, ViewModifiers modifiers = ViewModifiers.None) =>
        Surface?.HandleKey(key, modifiers) ?? false;

    /// <summary>Two-finger pinch at a host point. The scale is the per-event factor, not the total.</summary>
    public bool Pinch(double x, double y, float scale) =>
        Surface?.HandlePinch(ToSurfacePoint(x, y), scale) ?? false;

    /// <summary>Two-finger pan by a host-space delta, converted to surface units.</summary>
    public bool Pan(double deltaX, double deltaY) =>
        Surface?.HandleTwoFingerPan(new SKPoint((float)(deltaX / Scale), (float)(deltaY / Scale))) ?? false;

    private void EnsureSurface(int pixelWidth, int pixelHeight)
    {
        if (_surface is not null && _surface.Canvas.DeviceClipBounds.Width == pixelWidth
                                 && _surface.Canvas.DeviceClipBounds.Height == pixelHeight)
        {
            return;
        }

        _surface?.Dispose();
        _surface = SKSurface.Create(new SKImageInfo(pixelWidth, pixelHeight, SKColorType.Bgra8888, SKAlphaType.Premul));
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _surface?.Dispose();
        _surface = null;
    }
}
