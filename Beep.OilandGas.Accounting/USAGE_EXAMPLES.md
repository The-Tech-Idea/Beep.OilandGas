# Beep.OilandGas.Accounting - Usage Examples

## Successful Efforts Accounting

### Basic Property Acquisition and Exploration

```csharp
using Beep.OilandGas.Accounting;
using Beep.OilandGas.Accounting.Models;
using Beep.OilandGas.Accounting.SuccessfulEfforts;

// Create accounting instance
var accounting = new SuccessfulEffortsAccounting();

// Record acquisition of unproved property
var property = new UnprovedProperty
{
    PropertyId = "Lease-A-001",
    PropertyName = "Lease A",
    AcquisitionCost = 60000m,
    AcquisitionDate = new DateTime(2023, 1, 1),
    WorkingInterest = 1.0m,
    NetRevenueInterest = 0.875m
};

accounting.RecordAcquisition(property);

// Record G&G costs (expensed as incurred)
var ggCosts = new ExplorationCosts
{
    PropertyId = "Lease-A-001",
    GeologicalGeophysicalCosts = 50000m,
    CostDate = new DateTime(2023, 2, 1)
};

accounting.RecordExplorationCosts(ggCosts);
// G&G costs are automatically expensed

// Record exploratory drilling costs (capitalized until success determined)
var drillingCosts = new ExplorationCosts
{
    PropertyId = "Lease-A-001",
    WellId = "Well-001",
    ExploratoryDrillingCosts = 300000m, // IDC
    ExploratoryWellEquipment = 20000m,
    CostDate = new DateTime(2023, 3, 1)
};

accounting.RecordExplorationCosts(drillingCosts);
```

### Dry Hole Expense

```csharp
// If well is dry, expense all costs
var dryHoleCosts = new ExplorationCosts
{
    PropertyId = "Lease-A-001",
    WellId = "Well-002",
    ExploratoryDrillingCosts = 250000m,
    ExploratoryWellEquipment = 15000m,
    CostDate = new DateTime(2023, 4, 1),
    IsDryHole = true
};

accounting.RecordDryHole(dryHoleCosts);
// All costs are expensed

// Get total dry hole costs expensed
decimal totalDryHoleExpense = accounting.GetTotalDryHoleCostsExpensed("Lease-A-001");
```

### Successful Well - Classify as Proved

```csharp
// When well finds proved reserves, classify property as proved
var reserves = new ProvedReserves
{
    PropertyId = "Lease-A-001",
    ProvedDevelopedOilReserves = 500000m, // barrels
    ProvedUndevelopedOilReserves = 500000m,
    ProvedDevelopedGasReserves = 2000000m, // MCF
    ProvedUndevelopedGasReserves = 3000000m,
    ReserveDate = new DateTime(2023, 12, 31),
    OilPrice = 70m, // $/barrel
    GasPrice = 3.50m // $/MCF
};

accounting.ClassifyAsProved(property, reserves);
```

### Development Costs

```csharp
// All development costs are capitalized
var developmentCosts = new DevelopmentCosts
{
    PropertyId = "Lease-A-001",
    DevelopmentWellDrillingCosts = 400000m,
    DevelopmentWellEquipment = 50000m,
    SupportEquipmentAndFacilities = 200000m,
    ServiceWellCosts = 100000m,
    CostDate = new DateTime(2024, 1, 15)
};

accounting.RecordDevelopmentCosts(developmentCosts);
```

### Amortization Calculation

```csharp
// Get proved property
var provedProperty = accounting.GetProvedProperties().First();

// Production data
var production = new ProductionData
{
    PropertyId = "Lease-A-001",
    ProductionPeriod = new DateTime(2024, 6, 30),
    OilProduction = 50000m, // barrels
    GasProduction = 200000m // MCF
};

// Calculate amortization
decimal amortization = accounting.CalculateAmortization(
    provedProperty,
    reserves,
    production
);

Console.WriteLine($"Amortization: ${amortization:N2}");
```

### Interest Capitalization

```csharp
// Calculate interest capitalization
var interestData = new InterestCapitalizationData
{
    PropertyId = "Lease-A-001",
    BeginningAccumulatedExpenditures = 60000m, // Acquisition cost
    EndingAccumulatedExpenditures = 360000m, // Acquisition + drilling
    InterestRate = 0.10m, // 10%
    CapitalizationPeriodMonths = 12m,
    ActualInterestCosts = 40000m
};

decimal capitalizedInterest = accounting.CalculateInterestCapitalization(interestData);
Console.WriteLine($"Capitalized Interest: ${capitalizedInterest:N2}");
```

## Full Cost Accounting

### Cost Center Management

```csharp
using Beep.OilandGas.Accounting.FullCost;

var fullCost = new FullCostAccounting();

// Create cost center
var costCenter = fullCost.GetOrCreateCostCenter("USA-Onshore", "USA Onshore Cost Center");

// Record all costs to cost center (all capitalized)
fullCost.RecordAcquisitionCosts("USA-Onshore", property);
fullCost.RecordExplorationCosts("USA-Onshore", drillingCosts);
fullCost.RecordDevelopmentCosts("USA-Onshore", developmentCosts);

// Calculate total capitalized costs
decimal totalCosts = fullCost.CalculateTotalCapitalizedCosts("USA-Onshore");
```

### Full Cost Amortization

```csharp
// Amortize over total proved reserves in cost center
decimal amortization = fullCost.CalculateAmortization(
    "USA-Onshore",
    reserves,
    production
);

fullCost.RecordAmortization("USA-Onshore", amortization);
```

### Ceiling Test

```csharp
// Perform ceiling test
var ceilingResult = fullCost.PerformCeilingTest(
    "USA-Onshore",
    reserves,
    discountRate: 0.10m
);

if (ceilingResult.ImpairmentNeeded)
{
    Console.WriteLine($"Impairment needed: ${ceilingResult.ImpairmentAmount:N2}");
}
```

## Rendering with SkiaSharp

### Cost Trend Visualization

```csharp
using Beep.OilandGas.Accounting.Rendering;
using SkiaSharp;

var config = new AccountingRendererConfiguration
{
    Title = "Oil and Gas Accounting - Cost Trend",
    PlotType = AccountingPlotType.CostTrend,
    ShowLegend = true
};

var renderer = new AccountingRenderer(config);
renderer.SetAccountingData(accounting);

using (var surface = SKSurface.Create(new SKImageInfo(1200, 800)))
{
    var canvas = surface.Canvas;
    renderer.Render(canvas, 1200, 800);
    renderer.ExportToPng("accounting_costs.png", 1200, 800);
}
```

### Amortization Schedule

```csharp
// Create amortization schedule data
var amortizationSchedule = new List<(DateTime period, decimal amortization)>
{
    (new DateTime(2024, 1, 31), 50000m),
    (new DateTime(2024, 2, 29), 52000m),
    (new DateTime(2024, 3, 31), 48000m),
    (new DateTime(2024, 4, 30), 51000m)
};

config.PlotType = AccountingPlotType.AmortizationSchedule;
renderer.SetAmortizationData(amortizationSchedule);
renderer.Render(canvas, 1200, 800);
```

### Cost Breakdown Pie Chart

```csharp
config.PlotType = AccountingPlotType.CostBreakdown;
renderer.Render(canvas, 1200, 800);
renderer.ExportToPng("cost_breakdown.png", 1200, 800);
```

## Interactive Plot

```csharp
using Beep.OilandGas.Accounting.Interaction;

var interactionHandler = new AccountingInteractionHandler(renderer);
interactionHandler.EnablePan = true;
interactionHandler.EnableZoom = true;

// Handle mouse events
interactionHandler.OnMouseWheel(x, y, delta);
interactionHandler.OnMouseDown(x, y);
interactionHandler.OnMouseMove(x, y);
interactionHandler.ResetView();
```

## Complete Example

```csharp
// Complete accounting workflow
var accounting = new SuccessfulEffortsAccounting();

// 1. Acquire property
var property = new UnprovedProperty
{
    PropertyId = "Lease-001",
    AcquisitionCost = 100000m,
    AcquisitionDate = new DateTime(2023, 1, 1)
};
accounting.RecordAcquisition(property);

// 2. G&G costs (expensed)
var ggCosts = new ExplorationCosts
{
    PropertyId = "Lease-001",
    GeologicalGeophysicalCosts = 75000m
};
accounting.RecordExplorationCosts(ggCosts);

// 3. Exploratory drilling (capitalized)
var drillingCosts = new ExplorationCosts
{
    PropertyId = "Lease-001",
    ExploratoryDrillingCosts = 500000m,
    ExploratoryWellEquipment = 50000m
};
accounting.RecordExplorationCosts(drillingCosts);

// 4. Find reserves - classify as proved
var reserves = new ProvedReserves
{
    PropertyId = "Lease-001",
    ProvedDevelopedOilReserves = 1000000m,
    ProvedDevelopedGasReserves = 5000000m,
    OilPrice = 70m,
    GasPrice = 3.50m
};
accounting.ClassifyAsProved(property, reserves);

// 5. Development costs (all capitalized)
var devCosts = new DevelopmentCosts
{
    PropertyId = "Lease-001",
    DevelopmentWellDrillingCosts = 800000m,
    DevelopmentWellEquipment = 100000m
};
accounting.RecordDevelopmentCosts(devCosts);

// 6. Production and amortization
var production = new ProductionData
{
    PropertyId = "Lease-001",
    OilProduction = 100000m,
    GasProduction = 500000m
};

var provedProperty = accounting.GetProvedProperties().First();
decimal amortization = accounting.CalculateAmortization(provedProperty, reserves, production);
Console.WriteLine($"Period Amortization: ${amortization:N2}");
```

