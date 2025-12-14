# Beep.HeatMap - Advanced Heat Map Visualization Library

A comprehensive .NET library for creating interactive, high-performance heat maps for oil and gas spatial data visualization using SkiaSharp.

## Features

### Core Functionality
- **10+ Predefined Color Schemes**: Viridis, Plasma, Inferno, Magma, Cividis, and more
- **Custom Color Gradients**: Create any color scheme from your own colors
- **Color Scale Legend**: Professional legend display with ticks and labels
- **Data Interpolation**: IDW, Kriging, and grid-based interpolation
- **Contour Generation**: Automatic contour line generation

### Performance
- **Spatial Indexing**: QuadTree for O(log n) queries
- **Viewport Culling**: Only render visible points
- **Level-of-Detail (LOD)**: Adaptive point reduction for large datasets
- **Optimized Rendering**: Handles 10,000+ points smoothly

### Interaction
- **Point Selection**: Click to select, multi-select support
- **Tooltips**: Hover to display point information
- **Zoom to Selection**: Auto-calculate zoom/pan to fit selection
- **Events**: PointClicked, PointSelected, SelectionCleared

### Visual Elements
- **Grid Lines**: Configurable spacing and style
- **Axis Labels**: X and Y axis labels with rotation
- **Scale Bar**: Real-world distance representation
- **North Arrow**: Standard north indicator
- **Coordinate System Display**: Show CRS information

### Advanced Features
- **Clustering**: Grid-based, distance-based, and K-means clustering
- **Multi-Layer Support**: Overlay multiple datasets with opacity control
- **Statistical Analysis**: Outliers, confidence intervals, contours
- **Data Filtering**: Value range, spatial bounds, label pattern filters
- **Real-time Updates**: Thread-safe streaming data support
- **Time-Series Animation**: Frame-based animation with playback controls

### Export
- **Image Export**: PNG, JPEG, WebP with quality control
- **Vector Export**: SVG export
- **Data Export**: CSV with coordinates

## Quick Start

### Basic Usage

```csharp
using Beep.HeatMap;
using Beep.HeatMap.ColorSchemes;
using SkiaSharp;

// Create data points
var dataPoints = new List<HeatMapDataPoint>
{
    new HeatMapDataPoint(100, 200, 50.0, "Point 1"),
    new HeatMapDataPoint(200, 300, 75.0, "Point 2"),
    // ... more points
};

// Create heat map generator
var generator = new HeatMapGenerator(dataPoints, 800, 600, SKColors.White);

// Render
using (var surface = SKSurface.Create(new SKImageInfo(800, 600)))
{
    generator.Render(surface.Canvas);
    // Export or display
}
```

### With Color Schemes

```csharp
using Beep.HeatMap.ColorSchemes;

// Get color scheme
var colors = ColorScheme.GetColorScheme(ColorSchemeType.Viridis, 256);

// Create legend
var legend = new ColorScaleLegend(colors, minValue, maxValue)
{
    X = 10,
    Y = 10,
    Title = "Production Rate"
};

// Render legend
legend.Draw(canvas);
```

### With Performance Optimizations

```csharp
using Beep.HeatMap.Performance;

// Create spatial index
var spatialIndex = new SpatialIndex(dataPoints);

// Viewport culling
var viewportBounds = ViewportCulling.CalculateViewportBounds(
    width, height, zoom, panOffset);
var visiblePoints = ViewportCulling.CullToViewportWithIndex(
    spatialIndex, viewportBounds);

// Apply LOD
var filteredPoints = LevelOfDetail.ApplyAdaptiveLOD(
    visiblePoints, zoom, viewportArea, maxPoints: 1000);
```

### With Interaction

```csharp
using Beep.HeatMap.Interaction;

var interaction = new HeatMapInteraction
{
    MultiSelectEnabled = true,
    TooltipsEnabled = true
};

// Handle mouse events
void OnMouseClick(float x, float y)
{
    interaction.HandleClick(x, y, dataPoints, zoom, panOffset);
}

void OnMouseMove(float x, float y)
{
    interaction.HandleHover(x, y, dataPoints, zoom, panOffset);
}

// Render
interaction.DrawSelection(canvas, zoom, panOffset);
interaction.DrawTooltips(canvas);
```

### With Filtering and Analysis

```csharp
using Beep.HeatMap.Filtering;
using Beep.HeatMap.Analysis;

// Create filter manager
var filterManager = new FilterManager();
filterManager.AddFilter(FilterManager.CreateValueRangeFilter(10, 90));

// Apply filters
var filteredPoints = filterManager.ApplyFilters(dataPoints);

// Calculate statistics
var summary = DataAnalysis.CalculateSummary(filteredPoints);
Console.WriteLine(summary.GetFormattedSummary());
```

### With Real-time Updates

```csharp
using Beep.HeatMap.Realtime;

var realtimeManager = new RealtimeDataManager()
{
    HighlightNewPoints = true
};

// Subscribe to events
realtimeManager.PointsAdded += (sender, points) =>
{
    InvalidateSurface(); // Trigger redraw
};

// Add new points
realtimeManager.AddPoints(newPoints);
```

## Documentation

- **PHASE1_SUMMARY.md**: Color schemes, interpolation, performance
- **PHASE2_SUMMARY.md**: Interaction, visual elements, export
- **PHASE3_SUMMARY.md**: Clustering, multi-layer, statistics, animation
- **PHASE4_SUMMARY.md**: Data analysis, filtering, real-time updates
- **FINAL_IMPLEMENTATION_SUMMARY.md**: Complete feature overview

## Requirements

- .NET 6.0 or later
- SkiaSharp 2.88.8 or later

## Performance

- **Small datasets (< 100 points)**: < 10ms rendering
- **Medium datasets (100-1000 points)**: 10-50ms rendering
- **Large datasets (1000-10000 points)**: 50-200ms with optimizations
- **Very large datasets (10000+ points)**: 200-500ms with all optimizations

## License

MIT License

## Contributing

Contributions are welcome! Please ensure all code follows the existing style and includes appropriate documentation.

