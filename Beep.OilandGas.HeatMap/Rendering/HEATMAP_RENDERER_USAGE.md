# HeatMapRenderer Usage Guide

## Overview

The `HeatMapRenderer` class provides enhanced SkiaSharp-based rendering for heat maps with support for:
- Multiple color schemes (Viridis, Plasma, Inferno, etc.)
- Interpolation methods (IDW, Kriging)
- Visual elements (grid, axis labels, legend, scale bar, north arrow)
- Performance optimizations (LOD, viewport culling)
- Customizable configuration

## Basic Usage

```csharp
using Beep.HeatMap.Rendering;
using Beep.HeatMap.Configuration;
using Beep.HeatMap.ColorSchemes;
using SkiaSharp;

// Create data points
var dataPoints = new List<HeatMapDataPoint>
{
    new HeatMapDataPoint(100, 200, 0.5, "Point 1"),
    new HeatMapDataPoint(150, 250, 0.8, "Point 2"),
    new HeatMapDataPoint(200, 300, 0.3, "Point 3")
};

// Create configuration
var config = new HeatMapConfiguration
{
    ColorSchemeType = ColorSchemeType.Viridis,
    ShowLegend = true,
    ShowGrid = true,
    ShowAxisLabels = true,
    UseInterpolation = true,
    InterpolationMethod = InterpolationMethodType.InverseDistanceWeighting
};

// Create renderer
var renderer = new HeatMapRenderer(dataPoints, config);

// Create SkiaSharp surface and canvas
var imageInfo = new SKImageInfo(800, 600);
using (var surface = SKSurface.Create(imageInfo))
{
    var canvas = surface.Canvas;
    
    // Render the heat map
    renderer.Render(canvas, 800, 600);
    
    // Get the image
    using (var image = surface.Snapshot())
    {
        // Save or use the image
        using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
        {
            File.WriteAllBytes("heatmap.png", data.ToArray());
        }
    }
}
```

## Advanced Configuration

```csharp
var config = new HeatMapConfiguration
{
    // Color scheme
    ColorSchemeType = ColorSchemeType.Plasma,
    ColorSteps = 512, // Higher quality gradient
    
    // Point rendering
    MinPointRadius = 3f,
    MaxPointRadius = 25f,
    ShowLabels = true,
    LabelFontSize = 10f,
    
    // Interpolation
    UseInterpolation = true,
    InterpolationMethod = InterpolationMethodType.InverseDistanceWeighting,
    InterpolationCellSize = 5.0,
    IdwPower = 2.5,
    
    // Visual elements
    ShowGrid = true,
    GridSpacing = 50f,
    GridColor = SKColors.LightGray,
    ShowAxisLabels = true,
    XAxisLabel = "Easting (m)",
    YAxisLabel = "Northing (m)",
    ShowLegend = true,
    LegendX = 10f,
    LegendY = 10f,
    ShowScaleBar = true,
    ShowNorthArrow = true,
    CoordinateSystem = "UTM Zone 15N",
    
    // Performance
    UseLOD = true,
    MaxRenderPoints = 2000,
    MaxDensity = 15.0,
    UseViewportCulling = true
};
```

## Custom Color Scheme

```csharp
var customColors = new SKColor[]
{
    SKColors.Blue,
    SKColors.Cyan,
    SKColors.Green,
    SKColors.Yellow,
    SKColors.Red
};

var config = new HeatMapConfiguration
{
    ColorSchemeType = ColorSchemeType.Custom,
    CustomColors = customColors,
    ColorSteps = 256
};
```

## Updating Data Points

```csharp
// Add new data points
var newPoints = new List<HeatMapDataPoint> { /* ... */ };
renderer.UpdateDataPoints(newPoints);

// The renderer will automatically recompute interpolation grid if enabled
```

## Updating Configuration

```csharp
var newConfig = new HeatMapConfiguration
{
    ColorSchemeType = ColorSchemeType.Inferno,
    UseInterpolation = false
};

renderer.UpdateConfiguration(newConfig);
```

## Getting Value and Spatial Bounds

```csharp
var (minValue, maxValue) = renderer.GetValueRange();
var (minX, maxX, minY, maxY) = renderer.GetSpatialBounds();

Console.WriteLine($"Value range: {minValue} to {maxValue}");
Console.WriteLine($"Spatial bounds: X[{minX}, {maxX}], Y[{minY}, {maxY}]");
```

## Integration with Existing HeatMapGenerator

The new `HeatMapRenderer` can be used alongside or as a replacement for the existing `HeatMapGenerator`:

```csharp
// Option 1: Use HeatMapRenderer directly
var renderer = new HeatMapRenderer(dataPoints, config);
renderer.Render(canvas, width, height);

// Option 2: Use existing HeatMapGenerator for basic rendering
var generator = new HeatMapGenerator(dataPoints, width, height, SKColors.Black);
generator.Render(canvas);
```

## Performance Considerations

- **Interpolation**: Enables smooth heat maps but requires grid computation (slower for large datasets)
- **LOD**: Automatically reduces point count for better performance with large datasets
- **Viewport Culling**: Only renders visible points (requires viewport bounds calculation)
- **Spatial Indexing**: Use with `SpatialIndex` class for very large datasets (10,000+ points)

## Best Practices

1. **For small datasets (< 1000 points)**: 
   - Disable LOD and viewport culling
   - Use interpolation for smooth visualization

2. **For medium datasets (1000-10,000 points)**:
   - Enable LOD with `MaxRenderPoints = 2000`
   - Consider disabling interpolation for faster rendering

3. **For large datasets (> 10,000 points)**:
   - Enable all performance optimizations
   - Use spatial indexing
   - Consider clustering before rendering

4. **Color Scheme Selection**:
   - Use Viridis/Plasma/Inferno for colorblind-friendly visualization
   - Use RedToBlue/RedToGreen for traditional heat maps
   - Use Grayscale for printing

