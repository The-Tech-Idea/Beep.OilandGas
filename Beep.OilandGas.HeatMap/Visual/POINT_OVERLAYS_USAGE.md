# Point Overlays Usage Guide

## Overview
The heatmap now supports adding SVG images and charts (pie charts, bar charts) at specific point locations, providing rich visual annotations.

## Features

### 1. **Labels** ✅ (Already Available)
- Text labels on data points
- Value annotations
- Customizable font size and color
- Configurable visibility

### 2. **SVG Image Overlays** ✅ (NEW)
- Load SVG images from file or content string
- Position at any coordinate
- Rotate and scale
- Control opacity
- Offset positioning

### 3. **Pie Chart Overlays** ✅ (NEW)
- Multi-segment pie charts
- Custom colors per segment
- Segment labels
- Border styling
- Configurable size and position

### 4. **Bar Chart Overlays** ✅ (NEW)
- Vertical or horizontal bar charts
- Multiple bars per chart
- Custom colors per bar
- Bar labels
- Configurable size and position

## Usage Examples

### Labels (Existing Feature)

```csharp
var point = new HeatMapDataPoint(100, 200, 0.5, "Well-001");
// Label is automatically rendered if ShowLabels = true
```

### SVG Image Overlay

```csharp
// Add SVG image from file
var svgOverlay = new SvgImageOverlay
{
    X = 1500,  // Data coordinate
    Y = 2000,  // Data coordinate
    SvgPathOrContent = "path/to/icon.svg",
    Width = 40f,
    Height = 40f,
    Offset = new SKPoint(0, -20), // Offset above point
    Rotation = 45f, // Rotate 45 degrees
    Opacity = 0.8f
};
renderer.SvgOverlays.Add(svgOverlay);

// Or add SVG from content string
var svgContent = @"<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 100 100'>
    <circle cx='50' cy='50' r='40' fill='red'/>
</svg>";
var svgOverlay2 = new SvgImageOverlay
{
    X = 1500,
    Y = 2000,
    SvgPathOrContent = svgContent,
    IsSvgContent = true, // Indicates it's content, not file path
    Width = 30f,
    Height = 30f
};
renderer.SvgOverlays.Add(svgOverlay2);
```

### Pie Chart Overlay

```csharp
// Create pie chart showing production breakdown
var pieChart = new PieChartOverlay
{
    X = 1500,  // Data coordinate
    Y = 2000,  // Data coordinate
    Radius = 25f,
    Offset = new SKPoint(0, -30), // Position above point
    ShowLabels = true,
    ShowBorder = true,
    BorderColor = SKColors.White,
    Segments = new List<(double, SKColor)>
    {
        (45.0, SKColors.Red),    // Oil: 45%
        (30.0, SKColors.Blue),   // Gas: 30%
        (25.0, SKColors.Green)    // Water: 25%
    }
};
renderer.PieChartOverlays.Add(pieChart);
```

### Bar Chart Overlay

```csharp
// Create vertical bar chart
var barChart = new BarChartOverlay
{
    X = 1500,
    Y = 2000,
    Width = 50f,
    Height = 60f,
    Offset = new SKPoint(30, 0), // Position to the right of point
    Orientation = BarChartOrientation.Vertical,
    ShowLabels = true,
    ShowBorder = true,
    Bars = new List<(double, SKColor, string)>
    {
        (100, SKColors.Red, "Q1"),
        (150, SKColors.Orange, "Q2"),
        (120, SKColors.Yellow, "Q3"),
        (180, SKColors.Green, "Q4")
    }
};
renderer.BarChartOverlays.Add(barChart);

// Create horizontal bar chart
var horizontalBarChart = new BarChartOverlay
{
    X = 1500,
    Y = 2000,
    Width = 80f,
    Height = 40f,
    Orientation = BarChartOrientation.Horizontal,
    Bars = new List<(double, SKColor, string)>
    {
        (75, SKColors.Blue, "Prod"),
        (50, SKColors.Cyan, "Inj")
    }
};
renderer.BarChartOverlays.Add(horizontalBarChart);
```

## Complete Example

```csharp
var config = new HeatMapConfiguration
{
    ShowLabels = true,
    ShowSvgOverlays = true,
    ShowPieChartOverlays = true,
    ShowBarChartOverlays = true
};

var renderer = new HeatMapRenderer(dataPoints, config);

// Add labels (via data points)
var point = new HeatMapDataPoint(1500, 2000, 0.8, "Well-001");

// Add SVG icon
renderer.SvgOverlays.Add(new SvgImageOverlay
{
    X = 1500,
    Y = 2000,
    SvgPathOrContent = "icons/well.svg",
    Width = 30f,
    Height = 30f,
    Offset = new SKPoint(-15, -40)
});

// Add pie chart
renderer.PieChartOverlays.Add(new PieChartOverlay
{
    X = 1500,
    Y = 2000,
    Radius = 20f,
    Offset = new SKPoint(40, 0),
    Segments = new List<(double, SKColor)>
    {
        (60, SKColors.Red),
        (40, SKColors.Blue)
    }
});

// Add bar chart
renderer.BarChartOverlays.Add(new BarChartOverlay
{
    X = 1500,
    Y = 2000,
    Width = 40f,
    Height = 50f,
    Offset = new SKPoint(0, -60),
    Bars = new List<(double, SKColor, string)>
    {
        (100, SKColors.Green, ""),
        (80, SKColors.Yellow, "")
    }
});

// Render
renderer.Render(canvas, 800, 600);
```

## Configuration

```csharp
// Enable/disable overlay types
config.ShowSvgOverlays = true;
config.ShowPieChartOverlays = true;
config.ShowBarChartOverlays = true;
```

## Rendering Order

Overlays are rendered in this order:
1. Data points
2. SVG overlays
3. Pie chart overlays
4. Bar chart overlays
5. Labels and annotations

This ensures charts and images appear above the base heatmap but below labels.

## Tips

1. **SVG Images**: Use vector icons for best quality at any zoom level
2. **Pie Charts**: Best for showing proportions (percentages)
3. **Bar Charts**: Best for comparing multiple values
4. **Offsets**: Use offsets to position overlays relative to points
5. **Visibility**: Set `IsVisible = false` to temporarily hide overlays

## Performance

- SVG rendering uses reflection to avoid direct dependencies
- Charts are rendered efficiently using SkiaSharp primitives
- Overlays are only rendered if `IsVisible = true`
- Consider LOD for large numbers of overlays

