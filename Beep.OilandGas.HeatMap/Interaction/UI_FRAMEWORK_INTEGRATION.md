# SkiaSharp Heatmap UI Framework Integration Guide

## Overview

SkiaSharp is a **rendering library only** - it doesn't handle mouse events, touch events, or user interaction. You need to integrate it with a UI framework that provides event handling.

## How It Works

1. **SkiaSharp renders** to a canvas/surface
2. **UI framework** (WinForms, WPF, MAUI, etc.) handles mouse/touch events
3. **InteractionHandler** bridges UI events to heatmap tools
4. **HeatMapRenderer** updates and re-renders

## Integration Examples

### WinForms Integration

```csharp
using System;
using System.Windows.Forms;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using Beep.HeatMap;
using Beep.HeatMap.Interaction;

public class HeatMapControl : SKControl
{
    private HeatMapRenderer renderer;
    private InteractionHandler interactionHandler;

    public HeatMapControl()
    {
        // Initialize renderer
        var dataPoints = GetDataPoints();
        var config = new HeatMapConfiguration();
        renderer = new HeatMapRenderer(dataPoints, config);
        
        // Initialize interaction handler
        interactionHandler = new InteractionHandler(renderer);
        interactionHandler.SetupCoordinateTransforms(Width, Height);
        
        // Subscribe to viewport changes
        interactionHandler.ViewportChanged += (s, e) => Invalidate();
    }

    protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        canvas.Clear(SKColors.White);
        
        // Render heatmap
        renderer.Render(canvas, e.Info.Width, e.Info.Height);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        interactionHandler.HandleMouseDown(e.X, e.Y, (int)e.Button);
        Invalidate();
        base.OnMouseDown(e);
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        interactionHandler.HandleMouseMove(e.X, e.Y);
        Invalidate();
        base.OnMouseMove(e);
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        interactionHandler.HandleMouseUp(e.X, e.Y, (int)e.Button);
        Invalidate();
        base.OnMouseUp(e);
    }

    protected override void OnMouseWheel(MouseEventArgs e)
    {
        interactionHandler.HandleMouseWheel(e.X, e.Y, e.Delta);
        Invalidate();
        base.OnMouseWheel(e);
    }

    protected override void OnDoubleClick(EventArgs e)
    {
        var mousePos = PointToClient(Control.MousePosition);
        interactionHandler.HandleDoubleClick(mousePos.X, mousePos.Y);
        Invalidate();
        base.OnDoubleClick(e);
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        interactionHandler.SetupCoordinateTransforms(Width, Height);
        Invalidate();
    }
}
```

### WPF Integration

```csharp
using System;
using System.Windows;
using System.Windows.Input;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using SkiaSharp.Views.WPF;
using Beep.HeatMap;
using Beep.HeatMap.Interaction;

public class HeatMapControl : SKElement
{
    private HeatMapRenderer renderer;
    private InteractionHandler interactionHandler;

    public HeatMapControl()
    {
        // Initialize renderer
        var dataPoints = GetDataPoints();
        var config = new HeatMapConfiguration();
        renderer = new HeatMapRenderer(dataPoints, config);
        
        // Initialize interaction handler
        interactionHandler = new InteractionHandler(renderer);
        
        // Wire up events
        MouseDown += OnMouseDown;
        MouseMove += OnMouseMove;
        MouseUp += OnMouseUp;
        MouseWheel += OnMouseWheel;
        MouseDoubleClick += OnMouseDoubleClick;
        SizeChanged += OnSizeChanged;
    }

    protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        canvas.Clear(SKColors.White);
        
        // Update coordinate transforms
        interactionHandler.SetupCoordinateTransforms(
            (float)ActualWidth, (float)ActualHeight);
        
        // Render heatmap
        renderer.Render(canvas, e.Info.Width, e.Info.Height);
    }

    private void OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        var pos = e.GetPosition(this);
        int button = e.ChangedButton == MouseButton.Left ? 0 : 
                    e.ChangedButton == MouseButton.Right ? 1 : 2;
        interactionHandler.HandleMouseDown((float)pos.X, (float)pos.Y, button);
        InvalidateVisual();
    }

    private void OnMouseMove(object sender, MouseEventArgs e)
    {
        var pos = e.GetPosition(this);
        interactionHandler.HandleMouseMove((float)pos.X, (float)pos.Y);
        InvalidateVisual();
    }

    private void OnMouseUp(object sender, MouseButtonEventArgs e)
    {
        var pos = e.GetPosition(this);
        int button = e.ChangedButton == MouseButton.Left ? 0 : 
                    e.ChangedButton == MouseButton.Right ? 1 : 2;
        interactionHandler.HandleMouseUp((float)pos.X, (float)pos.Y, button);
        InvalidateVisual();
    }

    private void OnMouseWheel(object sender, MouseWheelEventArgs e)
    {
        var pos = e.GetPosition(this);
        interactionHandler.HandleMouseWheel((float)pos.X, (float)pos.Y, e.Delta);
        InvalidateVisual();
    }

    private void OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        var pos = e.GetPosition(this);
        interactionHandler.HandleDoubleClick((float)pos.X, (float)pos.Y);
        InvalidateVisual();
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        interactionHandler.SetupCoordinateTransforms(
            (float)ActualWidth, (float)ActualHeight);
        InvalidateVisual();
    }
}
```

### .NET MAUI Integration

```csharp
using Microsoft.Maui.Controls;
using SkiaSharp.Views.Maui.Controls;
using Beep.HeatMap;
using Beep.HeatMap.Interaction;

public class HeatMapView : SKCanvasView
{
    private HeatMapRenderer renderer;
    private InteractionHandler interactionHandler;

    public HeatMapView()
    {
        // Initialize renderer
        var dataPoints = GetDataPoints();
        var config = new HeatMapConfiguration();
        renderer = new HeatMapRenderer(dataPoints, config);
        
        // Initialize interaction handler
        interactionHandler = new InteractionHandler(renderer);
        
        // Wire up paint event
        PaintSurface += OnPaintSurface;
    }

    private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        canvas.Clear(SKColors.White);
        
        // Update coordinate transforms
        interactionHandler.SetupCoordinateTransforms(
            (float)Width, (float)Height);
        
        // Render heatmap
        renderer.Render(canvas, e.Info.Width, e.Info.Height);
    }

    // Handle touch events (platform-specific)
    // For iOS/Android, use platform-specific touch handlers
    // For Windows, use mouse events
}
```

### Avalonia Integration

```csharp
using Avalonia.Controls;
using Avalonia.Input;
using SkiaSharp;
using SkiaSharp.Views.Avalonia;
using Beep.HeatMap;
using Beep.HeatMap.Interaction;

public class HeatMapControl : SKCanvas
{
    private HeatMapRenderer renderer;
    private InteractionHandler interactionHandler;

    public HeatMapControl()
    {
        // Initialize renderer
        var dataPoints = GetDataPoints();
        var config = new HeatMapConfiguration();
        renderer = new HeatMapRenderer(dataPoints, config);
        
        // Initialize interaction handler
        interactionHandler = new InteractionHandler(renderer);
        
        // Wire up events
        PointerPressed += OnPointerPressed;
        PointerMoved += OnPointerMoved;
        PointerReleased += OnPointerReleased;
        PointerWheelChanged += OnPointerWheelChanged;
        DoubleTapped += OnDoubleTapped;
    }

    protected override void OnPaint(SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        canvas.Clear(SKColors.White);
        
        // Update coordinate transforms
        interactionHandler.SetupCoordinateTransforms(
            (float)Bounds.Width, (float)Bounds.Height);
        
        // Render heatmap
        renderer.Render(canvas, e.Info.Width, e.Info.Height);
    }

    private void OnPointerPressed(object sender, PointerPressedEventArgs e)
    {
        var pos = e.GetPosition(this);
        var props = e.GetCurrentPoint(this).Properties;
        int button = props.IsLeftButtonPressed ? 0 : 
                    props.IsRightButtonPressed ? 1 : 2;
        interactionHandler.HandleMouseDown((float)pos.X, (float)pos.Y, button);
        InvalidateVisual();
    }

    private void OnPointerMoved(object sender, PointerEventArgs e)
    {
        var pos = e.GetPosition(this);
        interactionHandler.HandleMouseMove((float)pos.X, (float)pos.Y);
        InvalidateVisual();
    }

    private void OnPointerReleased(object sender, PointerReleasedEventArgs e)
    {
        var pos = e.GetPosition(this);
        var props = e.GetCurrentPoint(this).Properties;
        int button = props.IsLeftButtonPressed ? 0 : 
                    props.IsRightButtonPressed ? 1 : 2;
        interactionHandler.HandleMouseUp((float)pos.X, (float)pos.Y, button);
        InvalidateVisual();
    }

    private void OnPointerWheelChanged(object sender, PointerWheelEventArgs e)
    {
        var pos = e.GetPosition(this);
        interactionHandler.HandleMouseWheel((float)pos.X, (float)pos.Y, (float)e.Delta.Y);
        InvalidateVisual();
    }

    private void OnDoubleTapped(object sender, TappedEventArgs e)
    {
        var pos = e.GetPosition(this);
        interactionHandler.HandleDoubleClick((float)pos.X, (float)pos.Y);
        InvalidateVisual();
    }
}
```

## Interaction Modes

```csharp
// Switch between interaction modes
interactionHandler.Mode = InteractionMode.Pan;           // Drag to pan
interactionHandler.Mode = InteractionMode.BrushSelection;  // Select area
interactionHandler.Mode = InteractionMode.Measurement;     // Measure distances
interactionHandler.Mode = InteractionMode.PointSelection; // Click to select points

// Configure brush selection
renderer.BrushSelection.SelectionType = BrushSelectionType.Rectangle;
renderer.BrushSelection.SelectionColor = new SKColor(0, 100, 255, 100);

// Configure measurement
renderer.MeasurementTools.CurrentType = MeasurementType.Distance;
renderer.MeasurementTools.CoordinateScale = 1.0; // meters per pixel
renderer.MeasurementTools.UnitLabel = "m";
```

## Key Points

1. **SkiaSharp doesn't handle events** - UI framework does
2. **InteractionHandler bridges** UI events to heatmap tools
3. **Coordinate transformation** is crucial for accurate interaction
4. **Invalidate/Refresh** after each interaction to update display
5. **SetupCoordinateTransforms** must be called when size changes

## Complete Example

```csharp
// 1. Create renderer
var renderer = new HeatMapRenderer(dataPoints, config);

// 2. Create interaction handler
var handler = new InteractionHandler(renderer);
handler.SetupCoordinateTransforms(canvasWidth, canvasHeight);

// 3. Wire up UI events (example for WinForms)
control.MouseDown += (s, e) => {
    handler.HandleMouseDown(e.X, e.Y, (int)e.Button);
    control.Invalidate();
};

control.MouseMove += (s, e) => {
    handler.HandleMouseMove(e.X, e.Y);
    control.Invalidate();
};

// 4. Render in Paint event
control.PaintSurface += (s, e) => {
    renderer.Render(e.Surface.Canvas, e.Info.Width, e.Info.Height);
};
```

This architecture allows the heatmap to work with any UI framework that supports SkiaSharp!

