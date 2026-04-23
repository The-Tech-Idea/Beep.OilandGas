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
- **Field Maps**: Asset maps with facilities, wells, seismic context, surface networks, and HSE overlays
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
├── Measurements/
│   ├── SceneMeasurementService.cs # Scene-aware distance and area tools
│   └── SceneMeasurementResult.cs  # Typed measurement results
└── Export/
    ├── ImageExporter.cs          # Image export
    ├── GeoReferencedImageExporter.cs # PNG + world-file export
    ├── GeoJsonExporter.cs        # GeoJSON export
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

## Export Quick Start

Use the export helpers when you need publication-ready raster, vector, or document output from the same drawing engine instance:

```csharp
using Beep.OilandGas.Drawing.Builders;
using Beep.OilandGas.Drawing.Export;

var engine = FieldMapBuilder.Create()
    .WithSize(2200, 1500)
    .Build();

ImageExporter.ExportToPng(engine, @"C:\output\field-map.png", quality: 100);
SvgExporter.Export(engine, @"C:\output\field-map.svg");
PdfExporter.Export(engine, @"C:\output\field-map.pdf");
```

`PdfExporter` and `SvgExporter` render through the same layer pipeline as raster output, so scene composition, ordering, and viewport behavior stay consistent across formats.

Use the geospatial exporters when you need exchange-ready vector output or a georeferenced raster sidecar for map scenes that already carry CRS and world bounds:

```csharp
using Beep.OilandGas.Drawing.Builders;
using Beep.OilandGas.Drawing.CoordinateSystems;
using Beep.OilandGas.Drawing.DataLoaders.Models;
using Beep.OilandGas.Drawing.Export;
using Beep.OilandGas.Drawing.Visualizations.Reservoir;

var fieldMap = new FieldMapData
{
    MapName = "North Lease",
    CoordinateReferenceSystem = CoordinateReferenceSystem.CreateProjected("EPSG:26915", "NAD83 / UTM zone 15N")
};

fieldMap.PointAssets.Add(new FieldMapPointAsset
{
    AssetId = "well-a01",
    AssetName = "A-01",
    AssetKind = FieldMapAssetKind.Well,
    Location = new Point3D { X = 1250, Y = 640, Z = 0 }
});

GeoJsonExporter.ExportFieldMap(fieldMap, @"C:\output\field-map.geojson");

var engine = FieldMapBuilder.Create()
    .WithFieldMapData(fieldMap)
    .WithSize(2200, 1500)
    .Build();

GeoReferencedImageExporter.ExportToPng(engine, @"C:\output\field-map.png");
```

`GeoReferencedImageExporter` writes the image plus a companion world file and `.crs.json` sidecar. `GeoJsonExporter` currently supports typed field-map assets and reservoir-map exports built from contour surfaces, fault traces, and well overlays, and the sample gallery now exposes those map-ready artifacts plus typed log, section, and schematic payloads through scene-declared supplemental export actions.

## Measurement Quick Start

Use the scene-aware measurement helpers when you need distance or area values from screen clicks while preserving the active scene CRS and units:

```csharp
using Beep.OilandGas.Drawing.Measurements;
using Beep.OilandGas.Drawing.Samples;
using SkiaSharp;

var engine = DrawingSampleGallery.CreateFieldMapAssetNetworkScene();

var startScreen = engine.WorldToScreen(new SKPoint(250, 300));
var endScreen = engine.WorldToScreen(new SKPoint(850, 500));

SceneMeasurementResult distance = engine.MeasureDistance(startScreen, endScreen);
SceneMeasurementResult area = engine.MeasureArea(new[]
{
    engine.WorldToScreen(new SKPoint(0, 0)),
    engine.WorldToScreen(new SKPoint(1000, 0)),
    engine.WorldToScreen(new SKPoint(1000, 800)),
    engine.WorldToScreen(new SKPoint(0, 800))
});
```

`SceneMeasurementService` also works directly on world-space vertices when a host already manages its own pointer state. Projected maps and section views use planar scene units, depth scenes measure along the depth axis, and geographic scenes fall back to approximate geodesic meters and square meters.

Use the same engine instance for hover or selection lookups against interactive map layers:

```csharp
using Beep.OilandGas.Drawing.Interaction;
using SkiaSharp;

var pointerScreenPoint = new SKPoint(320, 240);
LayerHitResult hit = engine.HitTest(pointerScreenPoint, screenTolerance: 8f);

if (hit != null)
{
    Console.WriteLine($"{hit.FeatureKind}: {hit.FeatureLabel}");
}
```

`HitTest` currently resolves features from `FieldMapLayer` and `ReservoirMapOverlayLayer`, which gives an interactive host a stable path for hover labels, selection state, and click-driven measurements without inspecting renderer internals.

Persist those interaction artifacts back into the active scene when a host wants replayable selection or measurement state:

```csharp
var hit = engine.HitTest(pointerScreenPoint, screenTolerance: 8f);
if (hit != null)
{
    engine.RecordSelection(hit, replaceExisting: true);
}

var measurement = engine.MeasureDistance(startScreen, endScreen);
engine.RecordMeasurement(measurement, new[] { startScreen, endScreen }, label: "Offset Check");
```

Selections and measurements are stored under `DrawingScene.InteractionState`, so a host can keep scene-level interaction artifacts alongside CRS, bounds, viewport state, and export metadata.

## Sample Gallery Quick Start

Use the built-in sample gallery when you want deterministic engineering scenes for onboarding, demos, documentation, or export smoke tests:

```csharp
using System.IO;
using Beep.OilandGas.Drawing.Export;
using Beep.OilandGas.Drawing.Samples;

Directory.CreateDirectory(@"C:\output\gallery");

foreach (var scene in DrawingSampleGallery.GetStandardScenes())
{
    var engine = scene.CreateEngine();
    var basePath = Path.Combine(@"C:\output\gallery", scene.Name);

    ImageExporter.ExportToPng(engine, basePath + ".png", quality: 100);
    SvgExporter.Export(engine, basePath + ".svg");
    PdfExporter.Export(engine, basePath + ".pdf");
}
```

The built-in gallery currently exposes five canonical scenes covering field maps, petrophysical logs, reservoir contours, reservoir cross-sections, and well schematics.

An integrated web-host workbench now lives in the main application at `/ppdm39/data-management/drawing-samples`, where the canonical gallery scenes can be inspected with selection, distance, area, drag-based pan, and targeted persisted-annotation cleanup workflows backed by the same scene APIs used in the library.

The same workbench now also exports the current engine state to PNG, SVG, and PDF, and can surface explicit scene-declared exports such as Field Map GeoJSON, Field Map georeferenced PNG bundles, Reservoir Contour GeoJSON, Reservoir Contour georeferenced PNG bundles, Reservoir Cross Section JSON, Well Schematic JSON, and Well Log JSON directly from the live workflow surface.

## Well Log Quick Start

Use the fluent builder directly when you already have a loader or `LogData` instance:

```csharp
using Beep.OilandGas.Drawing.Builders;
using Beep.OilandGas.Drawing.DataLoaders.Models;
using Beep.OilandGas.Drawing.Rendering;

var engine = WellLogBuilder.Create()
    .WithFile(@"C:\logs\well-01.las", wellIdentifier: "Well-01")
    .WithConfiguration(new LogRendererConfiguration
    {
        UseStandardTrackTemplates = true,
        RenderDepthScaleAsTrack = true,
        ShowDensityNeutronCrossoverShading = true,
        ShowLogDecadeGridLines = true
    })
    .WithSize(1400, 1800)
    .Build();
```

Use the gallery helper when you want a project-native standard petrophysical scene and optional export path from a real source file:

```csharp
using Beep.OilandGas.Drawing.Visualizations.Logs;

var engine = WellLogGallery.CreateStandardPetrophysicalSceneFromFile(
    @"C:\logs\well-01.las",
    wellIdentifier: "Well-01");

WellLogGallery.ExportStandardPetrophysicalSceneToPng(
    @"C:\logs\well-01.las",
    @"C:\output\well-01-log.png",
    wellIdentifier: "Well-01");

WellLogGallery.ExportStandardPetrophysicalSceneToSvg(
    @"C:\logs\well-01.las",
    @"C:\output\well-01-log.svg",
    wellIdentifier: "Well-01");

WellLogGallery.ExportStandardPetrophysicalSceneToPdf(
    @"C:\logs\well-01.las",
    @"C:\output\well-01-log.pdf",
    wellIdentifier: "Well-01");
```

## Well Correlation Quick Start

Use the correlation builder to place multiple wells on a shared depth window and align explicit or zone-derived markers across panels:

```csharp
using Beep.OilandGas.Drawing.Builders;
using Beep.OilandGas.Drawing.Visualizations.Logs;

var engine = WellCorrelationBuilder.Create()
    .AddFile(
        @"C:\logs\well-01.las",
        panelName: "Well-01",
        wellIdentifier: "Well-01",
        markers: new[]
        {
            new WellCorrelationMarker { Label = "Top A", Depth = 8125 },
            new WellCorrelationMarker { Label = "Top B", Depth = 8460 }
        })
    .AddFile(
        @"C:\logs\well-02.las",
        panelName: "Well-02",
        wellIdentifier: "Well-02",
        markers: new[]
        {
            new WellCorrelationMarker { Label = "Top A", Depth = 8040 },
            new WellCorrelationMarker { Label = "Top B", Depth = 8395 }
        })
    .WithSize(2600, 1800)
    .Build();
```

## Reservoir Map Quick Start

Use the reservoir map builder to combine typed contour surfaces with fault overlays from RESQML and explicit well map points:

```csharp
using Beep.OilandGas.Drawing.Builders;
using Beep.OilandGas.Drawing.DataLoaders.Models;
using Beep.OilandGas.Drawing.Visualizations.Reservoir;

var engine = ReservoirMapBuilder.Create()
    .WithDataSource(@"C:\models\reservoir.epc", DataLoaderType.RESQML)
    .SelectSurface("Top Reservoir")
    .WithWells(new[]
    {
        new ReservoirWellMapPoint
        {
            WellName = "A-12",
            Uwi = "100012345678W500",
            SurfaceLocation = new Point3D { X = 1250, Y = 640, Z = 0 }
        },
        new ReservoirWellMapPoint
        {
            WellName = "B-08H",
            Uwi = "100076543210W500",
            SurfaceLocation = new Point3D { X = 1690, Y = 980, Z = 0 },
            TrajectoryPoints =
            {
                new Point3D { X = 1690, Y = 980, Z = 0 },
                new Point3D { X = 1850, Y = 1120, Z = 9850 }
            }
        }
    })
    .WithConfiguration(new ReservoirMapConfiguration
    {
        ShowFaultLabels = true,
        ContourConfiguration = new ReservoirContourConfiguration
        {
            ContourInterval = 25,
            ShowContourLabels = true
        }
    })
    .WithSize(2000, 1400)
    .Build();
```

## Field Map Quick Start

Use the field map builder to combine field and lease boundaries, surface facilities, wells, seismic context, surface-network links, and HSE overlays in a single plan-view scene:

```csharp
using Beep.OilandGas.Drawing.Builders;
using Beep.OilandGas.Drawing.DataLoaders.Models;
using Beep.OilandGas.Drawing.Primitives;
using Beep.OilandGas.Drawing.Visualizations.FieldMap;

var engine = FieldMapBuilder.Create()
    .AddField(new FieldMapAreaAsset
    {
        AssetId = "FIELD-01",
        AssetName = "North Dome",
        BoundaryPoints =
        {
            new Point3D { X = 0, Y = 0 },
            new Point3D { X = 3200, Y = 0 },
            new Point3D { X = 3200, Y = 2200 },
            new Point3D { X = 0, Y = 2200 }
        }
    })
    .AddLease(new FieldMapAreaAsset
    {
        AssetId = "LEASE-A",
        AssetName = "Lease A",
        BoundaryPoints =
        {
            new Point3D { X = 250, Y = 250 },
            new Point3D { X = 1550, Y = 250 },
            new Point3D { X = 1550, Y = 1450 },
            new Point3D { X = 250, Y = 1450 }
        }
    })
    .AddSeismicFootprint(new FieldMapAreaAsset
    {
        AssetId = "SEIS-3D-01",
        AssetName = "Phase A 3D",
        BoundaryPoints =
        {
            new Point3D { X = 900, Y = 700 },
            new Point3D { X = 2900, Y = 700 },
            new Point3D { X = 2900, Y = 1900 },
            new Point3D { X = 900, Y = 1900 }
        }
    })
    .AddLandRight(new FieldMapAreaAsset
    {
        AssetId = "ROW-07",
        AssetName = "Pipeline ROW",
        BoundaryPoints =
        {
            new Point3D { X = 1700, Y = 300 },
            new Point3D { X = 1850, Y = 300 },
            new Point3D { X = 1850, Y = 2100 },
            new Point3D { X = 1700, Y = 2100 }
        }
    })
    .AddHazardZone(new FieldMapAreaAsset
    {
        AssetId = "HZ-12",
        AssetName = "Sour Gas Buffer",
        BoundaryPoints =
        {
            new Point3D { X = 2100, Y = 1200 },
            new Point3D { X = 2600, Y = 1200 },
            new Point3D { X = 2600, Y = 1700 },
            new Point3D { X = 2100, Y = 1700 }
        }
    })
    .AddFacility(new FieldMapPointAsset
    {
        AssetId = "CPF-1",
        AssetName = "Central Facility",
        Location = new Point3D { X = 2050, Y = 980, Z = 0 }
    })
    .AddWell(new FieldMapPointAsset
    {
        AssetId = "WELL-A12",
        AssetName = "A-12H",
        Location = new Point3D { X = 840, Y = 860, Z = 0 }
    })
    .AddSurfaceSystem(new FieldMapPointAsset
    {
        AssetId = "MAN-03",
        AssetName = "Test Manifold",
        Location = new Point3D { X = 1380, Y = 930, Z = 0 }
    })
    .AddFlowline(new FieldMapConnectionAsset
    {
        ConnectionId = "FL-001",
        ConnectionName = "A Pad Flowline",
        FromAssetId = "WELL-A12",
        ToAssetId = "MAN-03",
        Vertices =
        {
            new Point3D { X = 840, Y = 860, Z = 0 },
            new Point3D { X = 1120, Y = 900, Z = 0 },
            new Point3D { X = 1380, Y = 930, Z = 0 }
        }
    })
    .AddGatheringLine(new FieldMapConnectionAsset
    {
        ConnectionId = "GL-010",
        ConnectionName = "Gathering Spur",
        FromAssetId = "MAN-03",
        ToAssetId = "CPF-1",
        Vertices =
        {
            new Point3D { X = 1380, Y = 930, Z = 0 },
            new Point3D { X = 1710, Y = 955, Z = 0 },
            new Point3D { X = 2050, Y = 980, Z = 0 }
        }
    })
    .WithConfiguration(new FieldMapConfiguration
    {
        LegendAnchor = OverlayAnchor.BottomRight,
        ShowConnectionLabels = true,
        ShowAreaLabels = true
    })
    .WithSize(2200, 1500)
    .Build();
```

## Reservoir Cross-Section Quick Start

Use the cross-section builder to cut a section through typed reservoir surfaces and project nearby wells onto the section line:

```csharp
using Beep.OilandGas.Drawing.Builders;
using Beep.OilandGas.Drawing.DataLoaders.Models;
using Beep.OilandGas.Drawing.Visualizations.Reservoir;

var engine = ReservoirCrossSectionBuilder.Create()
    .WithDataSource(@"C:\models\reservoir.epc", DataLoaderType.RESQML)
    .WithSectionLine(new ReservoirSectionLine
    {
        SectionName = "A-A'",
        Start = new Point3D { X = 900, Y = 750, Z = 0 },
        End = new Point3D { X = 2200, Y = 750, Z = 0 }
    })
    .WithWells(new[]
    {
        new ReservoirWellMapPoint
        {
            WellName = "A-12",
            SurfaceLocation = new Point3D { X = 1250, Y = 640, Z = 0 },
            TrajectoryPoints =
            {
                new Point3D { X = 1250, Y = 640, Z = 0 },
                new Point3D { X = 1310, Y = 760, Z = 10125 }
            }
        }
    })
    .WithConfiguration(new ReservoirCrossSectionConfiguration
    {
        ShowFluidContacts = true,
        MaximumWellOffsetFromSection = 300
    })
    .WithSize(2200, 1400)
    .Build();
```

## Regression Workflow

The canonical scenes in `DrawingSampleGallery` are the same scene constructors used by `Beep.OilandGas.Drawing.Tests`, so documentation, demos, and golden-image regression coverage all stay aligned:

```csharp
using Beep.OilandGas.Drawing.Export;
using Beep.OilandGas.Drawing.Samples;

var scene = DrawingSampleGallery.GetStandardScene("ReservoirContour_StructureMap");
var engine = scene.CreateEngine();

ImageExporter.ExportToPng(engine, @"C:\output\regression-preview.png", quality: 100);
```

When you add or modify a canonical sample scene, update the matching golden-image hash in `Beep.OilandGas.Drawing.Tests` so the regression harness continues to validate the public gallery surface instead of a private test-only scene catalog.

## Benchmark Suite

Use the in-process benchmark harness when you want repeatable baseline timings for canonical logs, maps, sections, and schematic scenes without creating a separate private workload catalog:

```powershell
dotnet run --project Beep.OilandGas.Drawing.Benchmarks -- --iterations 5 --warmup 1
```

Scope the run to a specific scene or operation when you are investigating a local regression:

```powershell
dotnet run --project Beep.OilandGas.Drawing.Benchmarks -- --scene WellLog_Petrophysical --operation build-scene,render-png
```

Use the vector-export operations when you want SVG or PDF timings instead of raster-only baselines:

```powershell
dotnet run --project Beep.OilandGas.Drawing.Benchmarks -- --scene FieldMap_AssetNetwork,WellLog_Petrophysical --operation render-svg,render-pdf
```

Use the interaction-focused operations when you need timings for persisted-annotation rendering or typed feature resolution instead of plain scene rasterization:

```powershell
dotnet run --project Beep.OilandGas.Drawing.Benchmarks -- --scene FieldMap_AssetNetwork,WellLog_Petrophysical --operation render-png-annotated,hit-test
```

Use the annotated vector-export operations when you need the cost of persisted selections and measurements on SVG or PDF output instead of only on raster export:

```powershell
dotnet run --project Beep.OilandGas.Drawing.Benchmarks -- --scene FieldMap_AssetNetwork,WellLog_Petrophysical --operation render-svg-annotated,render-pdf-annotated
```

The benchmark harness writes a JSON report under `BenchmarkResults` by default and uses the same `DrawingSampleGallery` scenes that power the README examples and the golden-image regression suite.

## Industry Best Practices

- **Performance**: Optimized for large datasets (10,000+ data points)
- **Scalability**: Handles complex wells and reservoirs
- **Standards Compliance**: Follows API, ISO, and industry standards
- **Extensibility**: Easy to add new visualization types
- **Maintainability**: Clean architecture and comprehensive documentation

## License

MIT License

