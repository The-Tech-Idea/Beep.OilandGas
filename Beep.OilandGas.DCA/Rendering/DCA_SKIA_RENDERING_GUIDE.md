# DCA SkiaSharp Rendering Guide

## Overview

The DCA SkiaSharp rendering system provides professional-grade visualization for decline curve analysis plots with interactive features, zoom, pan, and high-quality rendering.

## Features

✅ **SkiaSharp-based rendering** - High-performance, hardware-accelerated rendering
✅ **Interactive plots** - Zoom, pan, point selection
✅ **Multiple plot types** - Decline curves, intervals, residuals, comparisons
✅ **Professional styling** - Customizable colors, fonts, margins
✅ **Prediction intervals** - Visualize uncertainty bounds
✅ **Logarithmic scales** - Support for log-scale Y-axis
✅ **Export capabilities** - Render to images

## Quick Start

### Basic Usage

```csharp
using Beep.DCA.Visualization;
using Beep.DCA.Rendering;
using SkiaSharp;

// Create a decline curve plot from DCA results
var plot = DeclineCurvePlot.FromFitResult(result, timeData, forecastDays: 365);

// Create renderer configuration
var config = new DCARendererConfiguration
{
    BackgroundColor = SKColors.White,
    ShowGrid = true,
    ShowLegend = true,
    ShowPredictedCurve = true,
    ShowForecastCurve = true,
    ShowObservedPoints = true
};

// Create renderer
var renderer = new DCARenderer(plot, config);

// Render to canvas
var surface = SKSurface.Create(new SKImageInfo(800, 600));
renderer.Render(surface.Canvas, 800, 600);

// Export to image
using (var image = surface.Snapshot())
using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
using (var stream = File.OpenWrite("dca_plot.png"))
{
    data.SaveTo(stream);
}
```

### With Prediction Intervals

```csharp
// Create plot with intervals
var plotWithIntervals = PlotWithIntervals.FromFitResult(
    result, 
    timeData, 
    forecastDays: 365,
    confidenceLevel: 0.95);

// Configure to show intervals
var config = new DCARendererConfiguration
{
    ShowPredictionIntervals = true,
    ConfidenceIntervalColor = new SKColor(255, 87, 34, 32), // Red with transparency
    ConfidenceIntervalBorderColor = new SKColor(255, 87, 34, 128)
};

var renderer = new DCARenderer(plotWithIntervals, config);
renderer.Render(canvas, 800, 600);
```

### Interactive Plot with UI Handler

```csharp
using Beep.DCA.Interaction;

// Create renderer
var renderer = new DCARenderer(plot, config);

// Create interaction handler
var handler = new DCAInteractionHandler(renderer);
handler.SetupCoordinateTransforms(800, 600);
handler.EnablePan = true;
handler.EnableZoom = true;

// Wire up UI events (example for WinForms)
control.MouseDown += (s, e) => {
    handler.HandleMouseDown(e.X, e.Y, e.Button == MouseButtons.Left ? 0 : 1);
    control.Invalidate();
};

control.MouseMove += (s, e) => {
    handler.HandleMouseMove(e.X, e.Y);
    control.Invalidate();
};

control.MouseUp += (s, e) => {
    handler.HandleMouseUp(e.X, e.Y);
    control.Invalidate();
};

control.MouseWheel += (s, e) => {
    handler.HandleMouseWheel(e.X, e.Y, e.Delta);
    control.Invalidate();
};

control.MouseDoubleClick += (s, e) => {
    handler.HandleDoubleClick(e.X, e.Y);
    control.Invalidate();
};

// Render in Paint event
control.Paint += (s, e) => {
    var surface = SKSurface.Create(new SKImageInfo(control.Width, control.Height));
    renderer.Render(surface.Canvas, control.Width, control.Height);
    // ... display surface
};
```

## Configuration Options

### Colors

```csharp
var config = new DCARendererConfiguration
{
    BackgroundColor = SKColors.White,
    PlotAreaBackgroundColor = SKColors.White,
    ObservedColor = SKColors.Blue,
    PredictedColor = SKColors.Red,
    ForecastColor = SKColors.Orange,
    ConfidenceIntervalColor = new SKColor(255, 87, 34, 32),
    GridColor = new SKColor(224, 224, 224),
    TextColor = SKColors.Black
};
```

### Styling

```csharp
var config = new DCARendererConfiguration
{
    PointSize = 5f,
    LineWidth = 2.5f,
    FontSize = 12f,
    TitleFontSize = 18f,
    AxisLabelFontSize = 14f,
    TickLabelFontSize = 10f
};
```

### Margins

```csharp
var config = new DCARendererConfiguration
{
    TopMargin = 60f,
    BottomMargin = 60f,
    LeftMargin = 80f,
    RightMargin = 50f
};
```

### Grid and Axes

```csharp
var config = new DCARendererConfiguration
{
    ShowGrid = true,
    GridLineWidth = 1f,
    ShowMajorGridOnly = false,
    XAxisTickCount = 10,
    YAxisTickCount = 10,
    ShowTicks = true,
    ShowAxisLabels = true
};
```

### Legend

```csharp
var config = new DCARendererConfiguration
{
    ShowLegend = true,
    LegendPosition = LegendPosition.TopRight // TopLeft, TopRight, BottomLeft, BottomRight
};
```

### Logarithmic Scale

```csharp
var config = new DCARendererConfiguration
{
    UseLogScale = true // Enables logarithmic Y-axis
};
```

### Zoom and Pan

```csharp
var config = new DCARendererConfiguration
{
    EnableZoom = true,
    EnablePan = true,
    MinZoom = 0.1,
    MaxZoom = 10.0,
    ZoomSensitivity = 0.1
};
```

## Interaction Modes

### Pan Mode

```csharp
handler.Mode = DCAInteractionMode.Pan;
handler.EnablePan = true;
```

### Zoom Mode

```csharp
handler.Mode = DCAInteractionMode.Zoom;
handler.EnableZoom = true;
handler.ZoomSensitivity = 0.15; // Adjust zoom speed
```

### Point Selection Mode

```csharp
handler.Mode = DCAInteractionMode.PointSelection;
handler.PointSelected += (s, e) => {
    Console.WriteLine($"Selected: Time={e.Time}, Rate={e.Rate}");
};
```

## Programmatic Control

### Zoom Operations

```csharp
// Zoom to fit
handler.ZoomToFit();

// Zoom by factor
handler.Zoom(1.5, centerX, centerY); // 1.5x zoom at center point

// Set zoom level directly
renderer.Zoom = 2.0;
```

### Pan Operations

```csharp
// Pan by delta
handler.Pan(deltaX, deltaY);

// Set pan offset directly
renderer.PanOffset = new SKPoint(100, 50);
```

## Export to Images

```csharp
// PNG
using (var image = surface.Snapshot())
using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
{
    data.SaveTo(File.OpenWrite("plot.png"));
}

// JPEG
using (var image = surface.Snapshot())
using (var data = image.Encode(SKEncodedImageFormat.Jpeg, 90))
{
    data.SaveTo(File.OpenWrite("plot.jpg"));
}

// SVG (requires SkiaSharp.Svg)
// ... SVG export implementation
```

## Complete Example

```csharp
using System;
using System.Collections.Generic;
using Beep.DCA.Visualization;
using Beep.DCA.Rendering;
using Beep.DCA.Interaction;
using SkiaSharp;

// 1. Create plot from DCA results
var plot = DeclineCurvePlot.FromFitResult(result, timeData, forecastDays: 365);
plot.Title = "Well A - Production Decline";
plot.XAxisLabel = "Time (days)";
plot.YAxisLabel = "Production Rate (bbl/day)";

// 2. Configure renderer
var config = new DCARendererConfiguration
{
    BackgroundColor = SKColors.White,
    ShowGrid = true,
    ShowLegend = true,
    ShowPredictedCurve = true,
    ShowForecastCurve = true,
    ShowObservedPoints = true,
    PointSize = 5f,
    LineWidth = 2.5f,
    LegendPosition = LegendPosition.TopRight,
    UseLogScale = false
};

// 3. Create renderer
var renderer = new DCARenderer(plot, config);

// 4. Create interaction handler
var handler = new DCAInteractionHandler(renderer);
handler.SetupCoordinateTransforms(800, 600);
handler.EnablePan = true;
handler.EnableZoom = true;
handler.Mode = DCAInteractionMode.Pan;

// 5. Wire up events (UI framework specific)
// ... event handlers ...

// 6. Render
var surface = SKSurface.Create(new SKImageInfo(800, 600));
renderer.Render(surface.Canvas, 800, 600);
```

## Integration with UI Frameworks

### WinForms

```csharp
// Use SKControl from SkiaSharp.Views.Desktop.Common
using SkiaSharp.Views.Desktop;

public partial class DCAForm : Form
{
    private DCARenderer renderer;
    private DCAInteractionHandler handler;

    public DCAForm()
    {
        InitializeComponent();
        
        var plot = DeclineCurvePlot.FromFitResult(result, timeData);
        var config = new DCARendererConfiguration();
        renderer = new DCARenderer(plot, config);
        handler = new DCAInteractionHandler(renderer);
        
        skControl.PaintSurface += OnPaintSurface;
        skControl.MouseDown += OnMouseDown;
        skControl.MouseMove += OnMouseMove;
        skControl.MouseUp += OnMouseUp;
        skControl.MouseWheel += OnMouseWheel;
    }

    private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
    {
        handler.SetupCoordinateTransforms(e.Info.Width, e.Info.Height);
        renderer.Render(e.Surface.Canvas, e.Info.Width, e.Info.Height);
    }

    private void OnMouseDown(object sender, MouseEventArgs e)
    {
        handler.HandleMouseDown(e.X, e.Y, e.Button == MouseButtons.Left ? 0 : 1);
        skControl.Invalidate();
    }

    private void OnMouseMove(object sender, MouseEventArgs e)
    {
        handler.HandleMouseMove(e.X, e.Y);
        skControl.Invalidate();
    }

    private void OnMouseUp(object sender, MouseEventArgs e)
    {
        handler.HandleMouseUp(e.X, e.Y);
        skControl.Invalidate();
    }

    private void OnMouseWheel(object sender, MouseEventArgs e)
    {
        handler.HandleMouseWheel(e.X, e.Y, e.Delta);
        skControl.Invalidate();
    }
}
```

### WPF

```csharp
// Use SKElement from SkiaSharp.Views.WPF
using SkiaSharp.Views.WPF;

public partial class DCAControl : UserControl
{
    private DCARenderer renderer;
    private DCAInteractionHandler handler;

    public DCAControl()
    {
        InitializeComponent();
        
        var plot = DeclineCurvePlot.FromFitResult(result, timeData);
        var config = new DCARendererConfiguration();
        renderer = new DCARenderer(plot, config);
        handler = new DCAInteractionHandler(renderer);
        
        skElement.PaintSurface += OnPaintSurface;
        skElement.MouseDown += OnMouseDown;
        skElement.MouseMove += OnMouseMove;
        skElement.MouseUp += OnMouseUp;
        skElement.MouseWheel += OnMouseWheel;
    }

    private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
    {
        handler.SetupCoordinateTransforms((float)e.Info.Width, (float)e.Info.Height);
        renderer.Render(e.Surface.Canvas, (float)e.Info.Width, (float)e.Info.Height);
    }

    // ... event handlers similar to WinForms
}
```

## Best Practices

1. **Performance**: For large datasets, consider downsampling points for display
2. **Memory**: Dispose of SKSurface and SKImage objects properly
3. **Threading**: SkiaSharp operations should be on the UI thread
4. **Coordinate Transforms**: Always call `SetupCoordinateTransforms` after canvas size changes
5. **Zoom Limits**: Set appropriate MinZoom/MaxZoom to prevent excessive zoom

## Summary

The DCA SkiaSharp rendering system provides:
- ✅ Professional visualization
- ✅ Interactive zoom/pan
- ✅ Multiple plot types
- ✅ Customizable styling
- ✅ Easy UI integration
- ✅ High performance

Perfect for building professional DCA analysis applications! 📊

