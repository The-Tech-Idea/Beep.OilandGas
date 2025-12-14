# Beep.WellTestAnalysis - Usage Examples

## Basic Build-up Analysis

```csharp
using Beep.WellTestAnalysis;
using Beep.WellTestAnalysis.Models;
using Beep.WellTestAnalysis.Rendering;
using SkiaSharp;

// Create well test data
var testData = new WellTestData
{
    Time = new List<double> { 0, 0.1, 0.5, 1, 2, 5, 10, 20, 50, 100 },
    Pressure = new List<double> { 3000, 2950, 2900, 2850, 2800, 2750, 2720, 2700, 2680, 2670 },
    FlowRate = 1000, // BPD
    WellboreRadius = 0.25, // feet
    FormationThickness = 50, // feet
    Porosity = 0.20,
    TotalCompressibility = 1e-5, // psi^-1
    OilViscosity = 1.5, // cp
    OilFormationVolumeFactor = 1.2, // RB/STB
    TestType = WellTestType.BuildUp,
    ProductionTime = 720 // hours
};

// Perform analysis
var result = WellTestAnalyzer.AnalyzeBuildUp(testData);

Console.WriteLine($"Permeability: {result.Permeability:F2} md");
Console.WriteLine($"Skin Factor: {result.SkinFactor:F2}");
Console.WriteLine($"Reservoir Pressure: {result.ReservoirPressure:F0} psi");
Console.WriteLine($"Productivity Index: {result.ProductivityIndex:F2} BPD/psi");
Console.WriteLine($"Flow Efficiency: {result.FlowEfficiency:P2}");
```

## Rendering with SkiaSharp

```csharp
// Create pressure-time points
var pressurePoints = testData.Time.Zip(testData.Pressure, 
    (t, p) => new PressureTimePoint(t, p)).ToList();

// Calculate derivative
var derivativePoints = WellTestAnalyzer.CalculateDerivative(pressurePoints);

// Create renderer
var config = new WellTestRendererConfiguration
{
    Title = "Well Test Analysis - Build-up Test",
    ShowDerivative = true,
    UseLogScaleX = true,
    PressureCurveColor = SKColors.Blue,
    DerivativeCurveColor = SKColors.Orange
};

var renderer = new WellTestRenderer(config);
renderer.SetPressureData(pressurePoints);
renderer.SetDerivativeData(derivativePoints);
renderer.SetAnalysisResult(result);

// Render to canvas
using (var surface = SKSurface.Create(new SKImageInfo(1200, 800)))
{
    var canvas = surface.Canvas;
    renderer.Render(canvas, 1200, 800);
    
    // Export to PNG
    renderer.ExportToPng("well_test_analysis.png", 1200, 800);
}
```

## Interactive Plot

```csharp
using Beep.WellTestAnalysis.Interaction;

var interactionHandler = new WellTestInteractionHandler(renderer);
interactionHandler.EnablePan = true;
interactionHandler.EnableZoom = true;
interactionHandler.ZoomSensitivity = 0.1;

// Handle mouse events (in your UI framework)
interactionHandler.OnMouseDown(mouseX, mouseY);
interactionHandler.OnMouseMove(mouseX, mouseY);
interactionHandler.OnMouseWheel(mouseX, mouseY, wheelDelta);

// Handle point selection
interactionHandler.PointSelected += (sender, e) =>
{
    Console.WriteLine($"Selected: Time={e.Time}h, Pressure={e.Pressure} psi");
};
```

## MDH Analysis

```csharp
// Use MDH method instead of Horner
var mdhResult = WellTestAnalyzer.AnalyzeBuildUpMDH(testData);
```

## Model Identification

```csharp
// Identify reservoir model from derivative
var model = WellTestAnalyzer.IdentifyReservoirModel(derivativePoints);
Console.WriteLine($"Identified Model: {model}");
```

