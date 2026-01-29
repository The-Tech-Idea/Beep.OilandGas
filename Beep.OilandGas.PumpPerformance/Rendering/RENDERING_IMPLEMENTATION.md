# Pump Performance SkiaSharp Rendering Implementation

## Overview

This document describes the SkiaSharp-based rendering system for pump performance curves, following the same methodology used in DCA and HeatMap projects.

## Architecture

### Components

1. **PumpPerformanceRendererConfiguration** - Comprehensive styling and configuration
2. **PumpPerformanceRenderer** - Main renderer for all curve types
3. **PumpPerformanceInteractionHandler** - User interaction handling (zoom, pan, selection)

## Features

### Supported Curve Types

1. **H-Q (Head-Quantity) Curves**
   - Primary pump performance curve
   - Smooth curve rendering with optional point markers
   - Configurable colors and line widths

2. **P-Q (Power-Quantity) Curves**
   - Secondary axis for power consumption
   - Overlaid on H-Q plot
   - Separate color and styling

3. ** EFFICIENCY Curves**
   -  EFFICIENCY vs. flow rate
   - Secondary axis support
   - Best  EFFICIENCY Point (BEP) highlighting

4. **System Curves**
   - System resistance curves
   - Dashed line style
   - Operating point intersection visualization

5. **Multi-Pump Configurations**
   - Series and parallel pump curves
   - Multiple colors for different pumps
   - Individual pump labels

6. **Affinity Law Curves**
   - Speed variation curves
   - Impeller diameter change curves
   - Dashed line style for differentiation

7. **NPSH Curves**
   - NPSH Required curve
   - NPSH Available curve
   - Margin visualization

### Visualization Features

- **Operating Point**
  - Highlighted with distinct color
  - Label showing flow rate and head
  - Configurable size and style

- **Best  EFFICIENCY Point (BEP)**
  - Special marker
  - Label with efficiency percentage
  - Automatic detection and display

- **Grid and Axes**
  - Configurable grid lines
  - Major/minor tick support
  - Customizable axis labels

- **Legend**
  - Automatic legend generation
  - Multiple position options (TopLeft, TopRight, BottomLeft, BottomRight)
  - Color-coded entries

- **Curve Smoothing**
  - Optional spline interpolation
  - Configurable smoothing factor
  - Smooth bezier curves

## Usage Example

```csharp
using Beep.PumpPerformance.Rendering;
using Beep.PumpPerformance.Calculations;
using Beep.PumpPerformance.Interaction;
using SkiaSharp;

// Create configuration
var config = new PumpPerformanceRendererConfiguration
{
    Title = "Pump Performance Analysis",
    ShowGrid = true,
    ShowLegend = true,
    HQCurveColor = SKColors.Blue,
    SystemCurveColor = SKColors.Purple,
    OperatingPointColor = SKColors.Red
};

// Create renderer
var renderer = new PumpPerformanceRenderer(config);

// Set H-Q curve data
var flowRates = new double[] { 100, 200, 300, 400, 500 };
var heads = new double[] { 100, 90, 75, 60, 45 };
var powers = new double[] { 80, 150, 230, 310, 380 };
var hqCurve = HeadQuantityCalculations.GenerateHQCurve(flowRates, heads, powers);
renderer.SetHQCurve(hqCurve);

// Set system curve
var systemCurve = SystemCurveCalculations.CalculateSystemCurve(
    staticHead: 50,
    systemResistanceCoefficient: 0.001,
    flowRates: flowRates
);
renderer.SetSystemCurve(systemCurve);

// Find and set operating point
var operatingPoint = SystemCurveCalculations.FindOperatingPoint(hqCurve, systemCurve);
if (operatingPoint.HasValue)
{
    renderer.SetOperatingPoint(operatingPoint.Value.flowRate, operatingPoint.Value.head);
}

// Find and set BEP
var bep = HeadQuantityCalculations.FindBestEfficiencyPoint(hqCurve);
renderer.SetBEP(bep);

// Enable additional curves
renderer.ShowPQCurve(true);
renderer.ShowEfficiencyCurve(true);

// Render to canvas
using (var surface = SKSurface.Create(new SKImageInfo(1200, 800)))
{
    var canvas = surface.Canvas;
    renderer.Render(canvas, 1200, 800);
    
    // Export to PNG
    renderer.ExportToPng("pump_performance.png", 1200, 800);
}
```

## Interaction Handler

```csharp
// Create interaction handler
var interactionHandler = new PumpPerformanceInteractionHandler(renderer);
interactionHandler.EnablePan = true;
interactionHandler.EnableZoom = true;
interactionHandler.ZoomSensitivity = 0.1;

// Set coordinate transforms (if needed for point selection)
interactionHandler.ScreenToDataTransform = (x, y) =>
{
    // Convert screen coordinates to data coordinates
    // Implementation depends on your coordinate system
    return (flowRate, head);
};

// Handle mouse events
interactionHandler.OnMouseDown(mouseX, mouseY);
interactionHandler.OnMouseMove(mouseX, mouseY);
interactionHandler.OnMouseUp(mouseX, mouseY);
interactionHandler.OnMouseWheel(mouseX, mouseY, wheelDelta);

// Handle point selection
interactionHandler.PointSelected += (sender, e) =>
{
    Console.WriteLine($"Selected: Flow={e.FlowRate} GPM, Head={e.Head} ft");
};

// Reset view
interactionHandler.ResetView();
interactionHandler.ZoomToFit();
```

## Multi-Pump Configuration

```csharp
// Create multiple pump curves
var pump1Curve = HeadQuantityCalculations.GenerateHQCurve(flowRates1, heads1, powers1);
var pump2Curve = HeadQuantityCalculations.GenerateHQCurve(flowRates2, heads2, powers2);
var pump3Curve = HeadQuantityCalculations.GenerateHQCurve(flowRates3, heads3, powers3);

// Set multi-pump curves
renderer.SetMultiPumpCurves(
    new List<List<HeadQuantityPoint>> { pump1Curve, pump2Curve, pump3Curve },
    new List<string> { "Pump A", "Pump B", "Pump C" }
);
```

## Affinity Laws

```csharp
// Scale curve to new speed
var originalCurve = HeadQuantityCalculations.GenerateHQCurve(flowRates, heads, powers);
var scaledCurve = AffinityLaws.ScaleCurveToNewSpeed(
    originalCurve,
    originalSpeed: 1750,
    newSpeed: 2000
);

renderer.SetAffinityLawCurve(scaledCurve, "2000 RPM");
```

## NPSH Curves

```csharp
// Calculate NPSH curves
var npshRequired = new List<(double flowRate, double npsh)>
{
    (100, 10), (200, 12), (300, 15), (400, 18), (500, 22)
};

var npshAvailable = new List<(double flowRate, double npsh)>
{
    (100, 25), (200, 24), (300, 23), (400, 22), (500, 21)
};

renderer.SetNPSHCurves(npshRequired, npshAvailable);
```

## Configuration Options

### Colors
- `HQCurveColor` - H-Q curve color
- `PQCurveColor` - P-Q curve color
- `EfficiencyCurveColor` -  EFFICIENCY curve color
- `SystemCurveColor` - System curve color
- `OperatingPointColor` - Operating point color
- `BEPPointColor` - BEP point color
- `MultiPumpColors` - Array of colors for multiple pumps

### Styling
- `HQCurveLineWidth` - Line width for H-Q curve
- `SystemCurveDashed` - Whether system curve is dashed
- `ShowHQPoints` - Show data points on H-Q curve
- `PointSize` - Size of data points
- `UseCurveSmoothing` - Enable curve smoothing
- `SmoothingFactor` - Smoothing intensity (0-1)

### Layout
- `LeftMargin`, `RightMargin`, `TopMargin`, `BottomMargin` - Margins
- `XAxisTickCount`, `YAxisTickCount` - Number of axis ticks
- `XAxisLabel`, `YAxisLabel` - Axis labels
- `Title` - Plot title

### Legend
- `ShowLegend` - Enable/disable legend
- `LegendPosition` - Legend position (TopLeft, TopRight, BottomLeft, BottomRight)
- `LegendBackgroundColor` - Legend background color
- `LegendBorderColor` - Legend border color

### Zoom and Pan
- `MinZoom`, `MaxZoom` - Zoom limits
- `DefaultZoom` - Initial zoom level
- Zoom and pan handled by `PumpPerformanceInteractionHandler`

## Export

The renderer supports exporting to PNG:

```csharp
renderer.ExportToPng("output.png", width: 1200, height: 800);
```

The export uses the configured `ExportDPI` (default: 300) for high-quality output.

## Integration with UI Frameworks

### WPF
```csharp
// In your WPF control
private void OnPaint(object sender, PaintEventArgs e)
{
    using (var surface = SKSurface.Create(info.Width, info.Height, 
        SKColorType.Rgba8888, SKAlphaType.Premul))
    {
        var canvas = surface.Canvas;
        renderer.Render(canvas, info.Width, info.Height);
        
        using (var image = surface.Snapshot())
        {
            // Display image in WPF control
        }
    }
}
```

### WinForms
```csharp
// In your WinForms control
protected override void OnPaint(PaintEventArgs e)
{
    using (var surface = SKSurface.Create(Width, Height, 
        SKColorType.Rgba8888, SKAlphaType.Premul))
    {
        var canvas = surface.Canvas;
        renderer.Render(canvas, Width, Height);
        
        // Convert to bitmap and draw
    }
}
```

### MAUI / Xamarin
```csharp
// In your MAUI control
private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
{
    var canvas = e.Surface.Canvas;
    renderer.Render(canvas, e.Info.Width, e.Info.Height);
}
```

## Best Practices

1. **Update bounds after setting data** - The renderer automatically updates bounds, but you can call `UpdateBounds()` manually if needed.

2. **Use appropriate colors** - Follow industry standards (blue for H-Q, purple for system, red for operating point).

3. **Enable smoothing for smooth curves** - Set `UseCurveSmoothing = true` for better visual appearance.

4. **Configure margins appropriately** - Ensure enough space for axis labels and legend.

5. **Use interaction handler for interactive plots** - Connect mouse/touch events to the interaction handler.

6. **Export at high DPI** - Use `ExportDPI = 300` for publication-quality images.

## Performance Considerations

- Curve smoothing adds computational overhead - disable if rendering many curves
- Multi-pump configurations with many pumps may require optimization
- High DPI exports use more memory - consider lower DPI for large images
- Zoom/pan operations are efficient - no re-rendering of data, only viewport transformation

## Future Enhancements

Potential future enhancements:
- SVG export
- PDF export
- Animation support
- Real-time data updates
- Custom curve styles (dotted, dash-dot, etc.)
- Secondary Y-axis for P-Q and efficiency curves
- Zoom rectangle selection
- Data point tooltips
- Curve comparison overlays

