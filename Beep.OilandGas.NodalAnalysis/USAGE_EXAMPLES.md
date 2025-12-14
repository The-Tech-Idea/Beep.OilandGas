# Beep.NodalAnalysis - Usage Examples

## Basic Nodal Analysis

```csharp
using Beep.NodalAnalysis;
using Beep.NodalAnalysis.Models;
using Beep.NodalAnalysis.Rendering;
using SkiaSharp;

// Define reservoir properties
var reservoir = new ReservoirProperties
{
    ReservoirPressure = 3000, // psi
    BubblePointPressure = 2500, // psi
    ProductivityIndex = 2.5, // BPD/psi
    WaterCut = 0.1, // fraction
    GasOilRatio = 500, // SCF/STB
    OilGravity = 35.0, // API
    FormationVolumeFactor = 1.2, // RB/STB
    OilViscosity = 1.5 // cp
};

// Generate IPR curve using Vogel method
var iprCurve = NodalAnalyzer.GenerateVogelIPR(reservoir, maxFlowRate: 5000, points: 50);

// Define wellbore properties
var wellbore = new WellboreProperties
{
    TubingDiameter = 2.875, // inches
    TubingLength = 8000, // feet
    WellheadPressure = 500, // psi
    WaterCut = 0.1,
    GasOilRatio = 500, // SCF/STB
    OilGravity = 35.0, // API
    GasSpecificGravity = 0.65,
    WellheadTemperature = 100.0, // F
    BottomholeTemperature = 200.0 // F
};

// Generate flow rates for VLP
var flowRates = Enumerable.Range(0, 50)
    .Select(i => (double)i * 100).ToArray(); // 0 to 4900 BPD

// Generate VLP curve
var vlpCurve = NodalAnalyzer.GenerateVLP(wellbore, flowRates);

// Find operating point
var operatingPoint = NodalAnalyzer.FindOperatingPoint(iprCurve, vlpCurve);

Console.WriteLine($"Operating Flow Rate: {operatingPoint.FlowRate:F2} BPD");
Console.WriteLine($"Operating Bottomhole Pressure: {operatingPoint.BottomholePressure:F2} psi");
```

## Rendering with SkiaSharp

```csharp
// Create renderer
var config = new NodalRendererConfiguration
{
    Title = "Nodal Analysis",
    IPRCurveColor = SKColors.Blue,
    VLPCurveColor = SKColors.Orange,
    OperatingPointColor = SKColors.Red,
    ShowOperatingPointLabel = true
};

var renderer = new NodalRenderer(config);
renderer.SetIPRCurve(iprCurve);
renderer.SetVLPCurve(vlpCurve);
renderer.SetOperatingPoint(operatingPoint);

// Render to canvas
using (var surface = SKSurface.Create(new SKImageInfo(1200, 800)))
{
    var canvas = surface.Canvas;
    renderer.Render(canvas, 1200, 800);
    
    // Export to PNG
    renderer.ExportToPng("nodal_analysis.png", 1200, 800);
}
```

## Fetkovich IPR Method

```csharp
// Use Fetkovich method with test points
var testPoints = new List<(double flowRate, double pressure)>
{
    (500, 2500),
    (1000, 2000),
    (1500, 1500),
    (2000, 1000)
};

var fetkovichIPR = NodalAnalyzer.GenerateFetkovichIPR(
    reservoir, 
    testPoints, 
    maxFlowRate: 5000, 
    points: 50
);
```

## Gas Well IPR

```csharp
// Generate gas well IPR
var gasIPR = IPRCalculator.GenerateGasWellIPR(
    reservoirPressure: 3000,
    aof: 10000, // Absolute Open Flow
    backpressureExponent: 0.5,
    maxFlowRate: 15000,
    points: 50
);
```

## Interactive Plot

```csharp
using Beep.NodalAnalysis.Interaction;

var interactionHandler = new NodalInteractionHandler(renderer);
interactionHandler.EnablePan = true;
interactionHandler.EnableZoom = true;

// Handle interactions
interactionHandler.OnMouseWheel(x, y, delta);
interactionHandler.ResetView();
```

