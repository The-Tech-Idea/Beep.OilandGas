# Beep.OilandGas.Drawing Framework Overview

## Introduction

Beep.OilandGas.Drawing is a unified, industry-standard drawing framework for oil and gas visualizations. It provides a single, extensible platform for creating professional visualizations following best practices from leading oil and gas software platforms.

## Key Features

### Unified Architecture
- **Single Rendering Engine**: One engine for all visualization types
- **Layer System**: Multi-layer support with z-ordering and visibility
- **Coordinate Systems**: Support for depth, time, geographic, and custom systems
- **Viewport Management**: Zoom, pan, and coordinate transformations

### Visualization Types
- **Well Schematics**: Vertical and deviated wellbores, casing, tubing, equipment
- **Log Displays**: Wireline logs, production logs, composite logs
- **Reservoir Visualization**: Structure maps, cross-sections, property maps
- **Production Charts**: Production curves, decline curves, performance plots

### Industry Standards
- **Color Palettes**: Industry-standard colors for logs, reservoirs, production
- **Symbol Libraries**: Standard equipment and symbol libraries
- **Format Support**: WITSML, LAS, and other industry formats
- **API Compliance**: Follows API recommended practices

## Architecture

### Core Components

```
Beep.OilandGas.Drawing/
├── Core/
│   ├── DrawingEngine.cs          # Main rendering engine
│   └── Viewport.cs               # Viewport and camera control
├── Layers/
│   ├── ILayer.cs                 # Layer interface
│   └── LayerBase.cs              # Base layer implementation
├── Visualizations/
│   ├── WellSchematic/            # Well schematic renderer
│   ├── LogDisplay/               # Log display renderer (planned)
│   ├── ReservoirMap/             # Reservoir visualization (planned)
│   └── ProductionChart/          # Production charts (planned)
├── Styling/
│   ├── ColorPalette.cs          # Industry color palettes
│   └── Theme.cs                  # Visual themes
├── CoordinateSystems/
│   └── CoordinateSystem.cs       # Coordinate transformations
├── Export/
│   └── ImageExporter.cs          # Image export
└── Builders/
    └── WellSchematicBuilder.cs   # Fluent API builders
```

## Usage Examples

### Basic Well Schematic

```csharp
using Beep.OilandGas.Drawing;
using Beep.OilandGas.Drawing.Builders;
using Beep.OilandGas.Drawing.Styling;

// Create well schematic using builder
var engine = WellSchematicBuilder.Create()
    .WithWellData(wellData)
    .WithTheme(Theme.Standard)
    .WithZoom(1.5f)
    .WithAnnotations(true)
    .WithSize(1200, 800)
    .Build();

// Render to image
using (var image = engine.RenderToImage())
{
    // Use image...
}

// Export to file
ImageExporter.ExportToPng(engine, "well_schematic.png", quality: 100);
```

### Multi-Layer Visualization

```csharp
using Beep.OilandGas.Drawing;
using Beep.OilandGas.Drawing.Layers;
using Beep.OilandGas.Drawing.Visualizations.WellSchematic;

// Create engine
var engine = new DrawingEngine(1200, 800);

// Add well schematic layer
var wellLayer = new WellSchematicLayer(wellData, Theme.Standard);
engine.AddLayer(wellLayer);

// Add log display layer (when implemented)
// var logLayer = new LogDisplayLayer(logData, Theme.Standard);
// engine.AddLayer(logLayer);

// Render
using (var surface = engine.Render())
{
    // Use surface...
}
```

### Custom Theme

```csharp
using Beep.OilandGas.Drawing.Styling;

// Create custom theme
var customTheme = new Theme
{
    BackgroundColor = SKColors.White,
    ForegroundColor = SKColors.Black,
    GridColor = SKColors.LightGray,
    CustomColors = new Dictionary<string, SKColor>
    {
        ["Wellbore"] = SKColors.DarkBlue,
        ["Casing"] = SKColors.Gray,
        ["Tubing"] = SKColors.LightBlue
    }
};

var engine = WellSchematicBuilder.Create()
    .WithWellData(wellData)
    .WithTheme(customTheme)
    .Build();
```

### Coordinate System Usage

```csharp
using Beep.OilandGas.Drawing.CoordinateSystems;

// Create depth coordinate system
var depthSystem = CoordinateSystem.CreateDepthSystem(
    minDepth: 0,
    maxDepth: 10000,
    unit: "feet");

// Convert depth to screen coordinates
float screenY = depthSystem.ToScreenY(5000, canvasHeight: 800);

// Convert screen coordinates to depth
double depth = depthSystem.FromScreenY(screenY, canvasHeight: 800);
```

## Industry Best Practices

### Color Standards
- **Well Schematics**: Black (wellbore), Gray (casing), Blue (tubing), Red (perforation)
- **Logs**: Black (gamma ray), Red (resistivity), Blue (porosity)
- **Reservoir**: Black (oil), Red (gas), Blue (water)

### Rendering Practices
- Depth increases downward (inverted Y-axis)
- Standard equipment symbols
- Consistent scaling and annotation
- Professional appearance

### Performance
- Viewport culling for large datasets
- Level-of-detail rendering
- Efficient coordinate transformations
- Caching strategies

## Extensibility

### Adding New Visualization Types

1. Create a new layer class inheriting from `LayerBase`
2. Implement `RenderContent` method
3. Implement `GetBounds` method
4. Add to appropriate namespace

Example:
```csharp
public class CustomVisualizationLayer : LayerBase
{
    protected override void RenderContent(SKCanvas canvas, Viewport viewport)
    {
        // Custom rendering logic
    }

    public override SKRect GetBounds()
    {
        // Return bounding rectangle
    }
}
```

## Integration with Existing Projects

- **Beep.WellSchematics**: Can migrate to use this framework
- **Beep.DCA**: Can use for decline curve visualization
- **Beep.HeatMap**: Can integrate spatial visualization
- **Beep.PumpPerformance**: Can use for performance curves

## Roadmap

### Phase 1: Foundation ✅
- Core engine, layers, styling, export

### Phase 2: Well Schematics (In Progress)
- Enhanced well schematic rendering
- Equipment symbols
- Annotations

### Phase 3: Log Displays (Planned)
- Log track rendering
- Multiple log types
- LAS/WITSML support

### Phase 4: Reservoir Visualization (Planned)
- Structure maps
- Cross-sections
- Property maps

### Phase 5: Production Charts (Planned)
- Production curves
- Decline curves
- Performance plots

## Benefits

1. **Unified Platform**: Single framework for all visualizations
2. **Industry Standards**: Follows best practices from leading software
3. **Extensible**: Easy to add new visualization types
4. **Performance**: Optimized for large datasets
5. **Maintainable**: Clean architecture and comprehensive documentation

