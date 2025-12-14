# Beep.OilandGas.Drawing - Unified Drawing Framework

A comprehensive, industry-standard drawing framework for oil and gas visualizations including well schematics, log displays, reservoir maps, cross-sections, and more.

## Overview

Beep.OilandGas.Drawing provides a unified, extensible framework for creating professional oil and gas visualizations. Built on SkiaSharp, it follows industry best practices from leading oil and gas software platforms.

## Key Features

### Core Framework
- **Unified Drawing Engine**: Single rendering engine for all visualization types
- **Layer System**: Multi-layer support with z-ordering and visibility controls
- **Coordinate Systems**: Support for multiple coordinate systems (depth, time, geographic)
- **Scaling & Transformations**: Automatic scaling, zoom, pan, and coordinate transformations
- **Event System**: Comprehensive event handling for interactions

### Visualization Types
- **Well Schematics**: Vertical and deviated wellbores, casing, tubing, equipment
- **Log Displays**: Wireline logs, mud logs, production logs, composite logs
- **Reservoir Visualization**: Structure maps, cross-sections, 3D views
- **Production Charts**: Production curves, decline curves, performance plots
- **Facility Diagrams**: Surface facilities, flow diagrams, P&IDs

### Industry Standards
- **API Standards**: Compliance with API recommended practices
- **WITSML Support**: Import/export WITSML data formats
- **LAS Format**: Support for Log ASCII Standard format
- **Color Standards**: Industry-standard color schemes and palettes
- **Symbol Libraries**: Standard equipment and symbol libraries

## Architecture

### Design Patterns
- **Strategy Pattern**: Different rendering strategies for different visualization types
- **Factory Pattern**: Creation of visualization components
- **Builder Pattern**: Fluent API for building complex visualizations
- **Observer Pattern**: Event-driven architecture
- **Template Method**: Consistent rendering pipeline

### Core Components

```
Beep.OilandGas.Drawing/
├── Core/
│   ├── DrawingEngine.cs          # Main rendering engine
│   ├── LayerManager.cs           # Layer management
│   ├── CoordinateSystem.cs        # Coordinate transformations
│   └── Viewport.cs               # Viewport and camera
├── Visualizations/
│   ├── WellSchematic/            # Well schematic renderer
│   ├── LogDisplay/               # Log display renderer
│   ├── ReservoirMap/             # Reservoir visualization
│   └── ProductionChart/          # Production charts
├── Rendering/
│   ├── RendererBase.cs          # Base renderer class
│   ├── GeometryRenderer.cs       # Geometry rendering
│   ├── TextRenderer.cs           # Text and labels
│   └── SymbolRenderer.cs        # Symbol rendering
├── Styling/
│   ├── StyleManager.cs           # Style management
│   ├── ColorPalette.cs           # Color palettes
│   └── Theme.cs                  # Themes
└── Export/
    ├── ImageExporter.cs          # Image export
    ├── PdfExporter.cs            # PDF export
    └── SvgExporter.cs            # SVG export
```

## Quick Start

```csharp
using Beep.OilandGas.Drawing;
using Beep.OilandGas.Drawing.Visualizations;

// Create drawing engine
var engine = new DrawingEngine(800, 600);

// Create well schematic
var wellSchematic = new WellSchematicRenderer(wellData)
    .WithTheme(Theme.Standard)
    .WithZoom(1.5)
    .WithAnnotations(true);

// Add to engine
engine.AddLayer(wellSchematic);

// Render
var image = engine.Render();
```

## Industry Best Practices

- **Performance**: Optimized for large datasets (10,000+ data points)
- **Scalability**: Handles complex wells and reservoirs
- **Standards Compliance**: Follows API, ISO, and industry standards
- **Extensibility**: Easy to add new visualization types
- **Maintainability**: Clean architecture and comprehensive documentation

## License

MIT License

