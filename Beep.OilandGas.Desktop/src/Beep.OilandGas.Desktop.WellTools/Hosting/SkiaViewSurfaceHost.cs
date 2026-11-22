using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using SkiaSharp;
using TheTechIdea.OilGas.Wellbore;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.WellTools.Hosting;

/// <summary>A pointer position in host units and in surface units.</summary>
public sealed record OgSurfacePointer(double X, double Y, SKPoint SurfacePoint);

/// <summary>
/// Hosts an <see cref="ViewSurface"/> in WPF: one <see cref="Image"/> showing the surface's frame, input
/// forwarded, and a redraw whenever the surface reports <c>Invalidated</c>.
/// </summary>
/// <remarks>
/// ADR-002: pixels travel SKSurface → SKBitmap → <see cref="WriteableBitmap"/>, so no .NET Framework
/// SkiaSharp view assembly is loaded. A render failure is reported through
/// <see cref="FailureReporter"/> and leaves the previous frame visible; with no reporter it rethrows so
/// the defect cannot hide.
/// </remarks>
public sealed class SkiaViewSurfaceHost : ContentControl, IDisposable
{
    public static readonly DependencyProperty SurfaceProperty =
        DependencyProperty.Register(
            nameof(Surface),
            typeof(ViewSurface),
            typeof(SkiaViewSurfaceHost),
            new PropertyMetadata(null, OnSurfaceChanged));

    private readonly Image _image = new()
    {
        Stretch = Stretch.None,
        HorizontalAlignment = HorizontalAlignment.Left,
        VerticalAlignment = VerticalAlignment.Top,
        SnapsToDevicePixels = true,
    };

    private ViewSurfaceRenderer _renderer = new();
    private bool _disposed;

    public SkiaViewSurfaceHost()
    {
        Content = _image;
        Focusable = true;
        IsTabStop = true;
        ClipToBounds = true;
        IsManipulationEnabled = true;
        SizeChanged += (_, _) => Redraw();
        Loaded += (_, _) => Redraw();
        Background = Brushes.Transparent;

        // REL-03 AC-2: a canvas must show where the keyboard is. The ring is a border on the host, so it is
        // drawn around the image rather than over it, and its colour is the theme accent rather than a literal.
        BorderThickness = new Thickness(2);
        BorderBrush = Brushes.Transparent;
        Padding = new Thickness(0);
        Style = BuildFocusRing();
    }

    /// <summary>True when the focus ring is currently shown; the tests inspect this rather than a screenshot.</summary>
    public bool IsFocusRingVisible => IsKeyboardFocused;

    private static Style BuildFocusRing()
    {
        var style = new Style(typeof(SkiaViewSurfaceHost));
        var focused = new Trigger { Property = IsKeyboardFocusedProperty, Value = true };

        focused.Setters.Add(new Setter(BorderBrushProperty, new DynamicResourceExtension("BeepAccentBrush")));
        style.Triggers.Add(focused);

        return style;
    }

    /// <summary>Where a render failure is recorded. Optional: without it the exception propagates.</summary>
    public IFailureReporter? FailureReporter { get; set; }

    /// <summary>
    /// Drawn over the surface after it paints (surface coordinates), then presented. Setting it does not
    /// redraw; call <see cref="Redraw"/>.
    /// </summary>
    public Action<SkiaSharp.SKCanvas>? Overlay
    {
        get => _renderer.Overlay;
        set => _renderer.Overlay = value;
    }

    /// <summary>Raised after the pointer moves, in host and surface coordinates.</summary>
    public event EventHandler<OgSurfacePointer>? PointerMoved;

    /// <summary>Raised after a left click, so a page can add a marker or an annotation at the cursor.</summary>
    public event EventHandler<OgSurfacePointer>? Clicked;

    /// <summary>Converts a host point to the surface coordinates a <c>ViewSurface</c> understands.</summary>
    public OgSurfacePointer ToSurfacePointer(double x, double y) =>
        new(x, y, _renderer.ToSurfacePoint(x, y));

    /// <summary>
    /// Forwards a move by host coordinates and raises <see cref="PointerMoved"/>. The mouse override calls
    /// this, so the page and a test see exactly the same path.
    /// </summary>
    public void ForwardPointerMove(double x, double y)
    {
        _renderer.MouseMove(x, y);
        PointerMoved?.Invoke(this, ToSurfacePointer(x, y));
    }

    /// <summary>Forwards a left click by host coordinates and raises <see cref="Clicked"/>.</summary>
    public void ForwardClick(double x, double y)
    {
        _renderer.MouseUp(x, y);
        Clicked?.Invoke(this, ToSurfacePointer(x, y));
    }

    /// <summary>The surface being hosted.</summary>
    public ViewSurface? Surface
    {
        get => (ViewSurface?)GetValue(SurfaceProperty);
        set => SetValue(SurfaceProperty, value);
    }

    /// <summary>How many frames have been produced. Used by the smoke run.</summary>
    public int RenderCount => _renderer.RenderCount;

    /// <summary>The last rendered frame, for tests.</summary>
    public byte[]? Frame => _renderer.Frame;

    /// <summary>Draws the surface and presents it. Returns false when there is nothing to draw.</summary>
    public bool Redraw()
    {
        if (Surface is null || ActualWidth <= 0 || ActualHeight <= 0)
        {
            return false;
        }

        var scale = VisualTreeHelper.GetDpi(this).DpiScaleX;
        if (scale <= 0)
        {
            scale = 1f;
        }

        try
        {
            _renderer.Surface = Surface;

            if (!_renderer.Render((int)Math.Ceiling(ActualWidth), (int)Math.Ceiling(ActualHeight), (float)scale))
            {
                return false;
            }

            Present();
            RefreshAutomation();
            return true;
        }
        catch (Exception ex)
        {
            if (FailureReporter is null)
            {
                throw;
            }

            FailureReporter.ReportHandled(
                ex,
                "drawing the well view",
                "the previous frame is still shown",
                FailureSeverity.Error);

            return false;
        }
    }

    private void Present()
    {
        var frame = _renderer.Frame;
        if (frame is null)
        {
            return;
        }

        if (_image.Source is not WriteableBitmap bitmap
            || bitmap.PixelWidth != _renderer.PixelWidth
            || bitmap.PixelHeight != _renderer.PixelHeight)
        {
            bitmap = new WriteableBitmap(
                _renderer.PixelWidth,
                _renderer.PixelHeight,
                96 * _renderer.Scale,
                96 * _renderer.Scale,
                PixelFormats.Pbgra32,
                null);

            _image.Source = bitmap;
        }

        bitmap.WritePixels(
            new Int32Rect(0, 0, _renderer.PixelWidth, _renderer.PixelHeight),
            frame,
            _renderer.FrameStride,
            0);
    }

    private static void OnSurfaceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var host = (SkiaViewSurfaceHost)d;

        if (e.OldValue is ViewSurface oldSurface)
        {
            oldSurface.Invalidated -= host.OnSurfaceInvalidated;
        }

        if (e.NewValue is ViewSurface newSurface)
        {
            newSurface.Invalidated += host.OnSurfaceInvalidated;
        }

        host.RefreshAutomation();
        host.Redraw();
    }

    /// <summary>
    /// REL-03 AC-5: a canvas is exposed to UI Automation as a named element with a text alternative — the
    /// surface's description plus its accessible children's names. Full per-pixel exposure is not attempted.
    /// </summary>
    public void RefreshAutomation()
    {
        if (Surface is not { } surface)
        {
            return;
        }

        AutomationProperties.SetName(this, surface.AccessibleName);

        var description = surface.AccessibleDescription;

        if (surface.AccessibleChildren.Count > 0)
        {
            description = string.Join(
                " ",
                new[] { description }
                    .Concat(surface.AccessibleChildren.Select(child => $"{child.Name} ({child.Role}).")));
        }

        AutomationProperties.SetHelpText(this, description);
    }

    private void OnSurfaceInvalidated(object? sender, EventArgs e)
    {
        if (_disposed)
        {
            return;
        }

        if (Dispatcher.CheckAccess())
        {
            Redraw();
        }
        else
        {
            Dispatcher.BeginInvoke(Redraw);
        }
    }

    // ── Input forwarding ─────────────────────────────────────────────────────

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);
        Focus();
        var point = e.GetPosition(this);
        _renderer.MouseDown(point.X, point.Y);
        e.Handled = true;
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        var point = e.GetPosition(this);
        ForwardPointerMove(point.X, point.Y);
    }

    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonUp(e);
        var point = e.GetPosition(this);
        ForwardClick(point.X, point.Y);
        e.Handled = true;
    }

    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
        base.OnMouseWheel(e);
        var point = e.GetPosition(this);
        _renderer.MouseWheel(point.X, point.Y, e.Delta);
        e.Handled = true;
    }

    protected override void OnMouseLeave(MouseEventArgs e)
    {
        base.OnMouseLeave(e);
        Surface?.HandleMouseLeave();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (ViewKeyMap.TryMap(e.Key, out var key))
        {
            var handled = _renderer.Key(key, ViewKeyMap.Modifiers(Keyboard.Modifiers));
            e.Handled = handled;
        }
    }

    // ── Touch: pinch and two-finger pan ──────────────────────────────────────

    protected override void OnManipulationStarting(ManipulationStartingEventArgs e)
    {
        base.OnManipulationStarting(e);
        e.Mode = ManipulationModes.Scale | ManipulationModes.Translate;
        e.Handled = true;
    }

    protected override void OnManipulationDelta(ManipulationDeltaEventArgs e)
    {
        base.OnManipulationDelta(e);

        if (Surface is null)
        {
            return;
        }

        var origin = e.ManipulationOrigin;
        var scale = e.DeltaManipulation.Scale;
        var factor = (float)((scale.X + scale.Y) / 2.0);

        if (float.IsFinite(factor) && Math.Abs(factor - 1f) > 0.0005f)
        {
            _renderer.Pinch(origin.X, origin.Y, factor);
        }

        var translation = e.DeltaManipulation.Translation;
        if (translation.X != 0 || translation.Y != 0)
        {
            _renderer.Pan(translation.X, translation.Y);
        }

        e.Handled = true;
    }

    // ── Accessibility ────────────────────────────────────────────────────────

    /// <summary>
    /// Exposes the surface to assistive technology: the view's own accessible name and description, and
    /// one child per <c>ViewSurface.AccessibleChildren</c> entry (tracks, wells, annotations).
    /// </summary>
    protected override System.Windows.Automation.Peers.AutomationPeer OnCreateAutomationPeer() =>
        new SkiaViewSurfaceHostAutomationPeer(this);
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        if (Surface is { } surface)
        {
            surface.Invalidated -= OnSurfaceInvalidated;
        }

        _renderer.Dispose();
    }
}

/// <summary>Maps WPF keys onto the surface's <see cref="ViewKey"/> vocabulary.</summary>
public static class ViewKeyMap
{
    // A map, not a switch: a switch over Key would need an arm for every Windows key or a default
    // that the BEEP0002 analyzer (rightly) refuses.
    private static readonly IReadOnlyDictionary<Key, ViewKey> Keys = new Dictionary<Key, ViewKey>
    {
        [Key.Up] = ViewKey.Up,
        [Key.Down] = ViewKey.Down,
        [Key.Left] = ViewKey.Left,
        [Key.Right] = ViewKey.Right,
        [Key.PageUp] = ViewKey.PageUp,
        [Key.PageDown] = ViewKey.PageDown,
        [Key.Home] = ViewKey.Home,
        [Key.End] = ViewKey.End,
        [Key.OemPlus] = ViewKey.Plus,
        [Key.Add] = ViewKey.Plus,
        [Key.OemMinus] = ViewKey.Minus,
        [Key.Subtract] = ViewKey.Minus,
        [Key.Tab] = ViewKey.Tab,
        [Key.Enter] = ViewKey.Enter,
        [Key.Escape] = ViewKey.Escape,
        [Key.Delete] = ViewKey.Delete,
        [Key.Z] = ViewKey.Z,
        [Key.Y] = ViewKey.Y,
    };

    public static bool TryMap(Key key, out ViewKey mapped) => Keys.TryGetValue(key, out mapped);

    public static ViewModifiers Modifiers(ModifierKeys modifiers)
    {
        var result = ViewModifiers.None;

        if (modifiers.HasFlag(ModifierKeys.Shift))
        {
            result |= ViewModifiers.Shift;
        }

        if (modifiers.HasFlag(ModifierKeys.Control))
        {
            result |= ViewModifiers.Control;
        }

        if (modifiers.HasFlag(ModifierKeys.Alt))
        {
            result |= ViewModifiers.Alt;
        }

        return result;
    }
}

/// <summary>
/// Exposes the hosted surface to assistive technology. The canvas is a single control, so the surface's
/// own accessible name and description are reported, and each <c>AccessibleChildren</c> entry becomes a
/// child element with its role as the class name.
/// </summary>
internal sealed class SkiaViewSurfaceHostAutomationPeer : FrameworkElementAutomationPeer
{
    public SkiaViewSurfaceHostAutomationPeer(SkiaViewSurfaceHost owner)
        : base(owner)
    {
    }

    protected override string GetClassNameCore() => nameof(SkiaViewSurfaceHost);

    protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.Pane;

    protected override string GetNameCore()
    {
        var name = base.GetNameCore();
        if (!string.IsNullOrWhiteSpace(name))
        {
            return name;
        }

        var surface = ((SkiaViewSurfaceHost)Owner).Surface;
        return surface is null ? base.GetNameCore() : surface.AccessibleName;
    }

    protected override string GetHelpTextCore()
    {
        var surface = ((SkiaViewSurfaceHost)Owner).Surface;
        return surface is null ? string.Empty : surface.AccessibleDescription;
    }

    protected override List<AutomationPeer>? GetChildrenCore()
    {
        var surface = ((SkiaViewSurfaceHost)Owner).Surface;
        if (surface is null)
        {
            return null;
        }

        var children = surface.AccessibleChildren;
        if (children.Count == 0)
        {
            return null;
        }

        return children.Select(item => (AutomationPeer)new SurfaceAccessibleItemPeer(item)).ToList();
    }
}

/// <summary>One element inside the surface (a track, a well, an annotation), as a text element.</summary>
internal sealed class SurfaceAccessibleItemPeer : AutomationPeer
{
    private readonly AccessibleItem _item;

    public SurfaceAccessibleItemPeer(AccessibleItem item)
    {
        _item = item ?? throw new ArgumentNullException(nameof(item));
    }

    protected override string GetAcceleratorKeyCore() => string.Empty;

    protected override string GetAccessKeyCore() => string.Empty;

    protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.Text;

    protected override string GetAutomationIdCore() => string.Empty;

    protected override Rect GetBoundingRectangleCore() => Rect.Empty;

    protected override List<AutomationPeer>? GetChildrenCore() => null;

    protected override string GetClassNameCore() => _item.Role;

    protected override Point GetClickablePointCore() => new(double.NaN, double.NaN);

    protected override string GetHelpTextCore() => _item.Description ?? string.Empty;

    protected override string GetItemStatusCore() => _item.Selected ? "selected" : string.Empty;

    protected override string GetItemTypeCore() => _item.Role;

    protected override AutomationPeer? GetLabeledByCore() => null;

    protected override string GetNameCore() => _item.Name;

    protected override AutomationOrientation GetOrientationCore() => AutomationOrientation.None;

    protected override bool HasKeyboardFocusCore() => _item.Focused;

    protected override bool IsContentElementCore() => true;

    protected override bool IsControlElementCore() => true;

    protected override bool IsEnabledCore() => true;

    protected override bool IsKeyboardFocusableCore() => false;

    protected override bool IsOffscreenCore() => false;

    protected override bool IsPasswordCore() => false;

    protected override bool IsRequiredForFormCore() => false;

    protected override void SetFocusCore()
    {
        // A surface child is informational; focus stays on the canvas.
    }

    public override object? GetPattern(PatternInterface patternInterface) => null;
}
