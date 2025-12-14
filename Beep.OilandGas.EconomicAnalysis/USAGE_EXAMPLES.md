# Beep.EconomicAnalysis - Usage Examples

## Basic Economic Analysis

```csharp
using Beep.EconomicAnalysis;
using Beep.EconomicAnalysis.Models;
using Beep.EconomicAnalysis.Rendering;
using SkiaSharp;

// Define project cash flows
var cashFlows = new CashFlow[]
{
    new CashFlow(0, -1000000, "Initial Investment"),
    new CashFlow(1, 300000, "Year 1 Revenue"),
    new CashFlow(2, 400000, "Year 2 Revenue"),
    new CashFlow(3, 500000, "Year 3 Revenue"),
    new CashFlow(4, 300000, "Year 4 Revenue"),
    new CashFlow(5, 200000, "Year 5 Revenue")
};

// Perform comprehensive analysis
double discountRate = 0.10; // 10%
var result = EconomicAnalyzer.Analyze(cashFlows, discountRate);

Console.WriteLine($"NPV: ${result.NPV:N2}");
Console.WriteLine($"IRR: {result.IRR:P2}");
Console.WriteLine($"MIRR: {result.MIRR:P2}");
Console.WriteLine($"Profitability Index: {result.ProfitabilityIndex:F2}");
Console.WriteLine($"Payback Period: {result.PaybackPeriod:F2} years");
Console.WriteLine($"Discounted Payback: {result.DiscountedPaybackPeriod:F2} years");
Console.WriteLine($"ROI: {result.ROI:F2}%");
```

## Individual Metrics

```csharp
// Calculate individual metrics
double npv = EconomicAnalyzer.CalculateNPV(cashFlows, discountRate: 0.10);
double irr = EconomicAnalyzer.CalculateIRR(cashFlows);

// Calculate payback period
double payback = EconomicCalculator.CalculatePaybackPeriod(cashFlows);

// Calculate profitability index
double pi = EconomicCalculator.CalculateProfitabilityIndex(cashFlows, discountRate: 0.10);
```

## NPV Profile

```csharp
// Generate NPV profile (NPV vs discount rate)
var npvProfile = EconomicAnalyzer.GenerateNPVProfile(
    cashFlows,
    minRate: 0.0,
    maxRate: 0.5, // 0% to 50%
    points: 50
);

// Find break-even discount rate (where NPV = 0)
var breakEvenRate = npvProfile
    .Where(p => Math.Abs(p.NPV) < 100)
    .FirstOrDefault()?.DiscountRate;
```

## Rendering with SkiaSharp

```csharp
// Create renderer for cash flow chart
var config = new EconomicRendererConfiguration
{
    Title = "Project Economics",
    PlotType = EconomicPlotType.CashFlow,
    PositiveCashFlowColor = SKColors.Green,
    NegativeCashFlowColor = SKColors.Red
};

var renderer = new EconomicRenderer(config);
renderer.SetCashFlows(cashFlows);
renderer.SetEconomicResult(result);

// Render cash flow chart
using (var surface = SKSurface.Create(new SKImageInfo(1200, 800)))
{
    var canvas = surface.Canvas;
    renderer.Render(canvas, 1200, 800);
    renderer.ExportToPng("cash_flow_chart.png", 1200, 800);
}

// Render NPV profile
config.PlotType = EconomicPlotType.NPVProfile;
renderer.SetNPVProfile(npvProfile);
renderer.Render(canvas, 1200, 800);
renderer.ExportToPng("npv_profile.png", 1200, 800);
```

## Cumulative Cash Flow

```csharp
// Render cumulative cash flow
config.PlotType = EconomicPlotType.CumulativeCashFlow;
renderer.Render(canvas, 1200, 800);
```

## MIRR Calculation

```csharp
// Calculate MIRR with different finance and reinvest rates
double financeRate = 0.08; // Cost of capital
double reinvestRate = 0.12; // Reinvestment rate
double mirr = EconomicCalculator.CalculateMIRR(cashFlows, financeRate, reinvestRate);
```

## Interactive Plot

```csharp
using Beep.EconomicAnalysis.Interaction;

var interactionHandler = new EconomicInteractionHandler(renderer);
interactionHandler.EnablePan = true;
interactionHandler.EnableZoom = true;

// Handle interactions
interactionHandler.OnMouseWheel(x, y, delta);
interactionHandler.ResetView();
```

