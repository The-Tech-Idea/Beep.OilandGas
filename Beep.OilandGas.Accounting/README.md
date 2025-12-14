# Beep.OilandGas.Accounting - Oil and Gas Accounting Library

A comprehensive .NET library for oil and gas accounting calculations, supporting both **Successful Efforts** and **Full Cost** accounting methods as defined by FASB Statement No. 19.

## Features

### Accounting Methods
- **Successful Efforts Accounting** - Capitalize only costs directly related to discovered reserves
- **Full Cost Accounting** - Capitalize all exploration and development costs
- **Cost Categorization** - Proper classification of exploration, development, and production costs

### Cost Categories (FASB No. 19)
1. **Acquisition Costs** - Costs of acquiring mineral interests in properties
2. **Exploration Costs** - Geological and geophysical (G&G) costs, exploratory drilling
3. **Development Costs** - Development wells, support equipment and facilities
4. **Production Costs** - Lifting costs, operating costs

### Key Calculations
- **Amortization** - Reserve-based amortization of capitalized costs
- **Interest Capitalization** - Capitalizing interest on qualifying assets
- **Dry Hole Expense** - Expensing unsuccessful exploration costs
- **Impairment** - Loss recognition for unproved properties
- **Deferred Classification** - Handling exploratory wells with deferred classification

## Quick Start

### Successful Efforts Accounting

```csharp
using Beep.OilandGas.Accounting;
using Beep.OilandGas.Accounting.Models;
using Beep.OilandGas.Accounting.SuccessfulEfforts;

// Create accounting data
var property = new UnprovedProperty
{
    PropertyId = "Lease-A-001",
    AcquisitionCost = 60000,
    AcquisitionDate = new DateTime(2023, 1, 1)
};

// Record exploration costs
var explorationCosts = new ExplorationCosts
{
    GeologicalGeophysicalCosts = 50000,
    ExploratoryDrillingCosts = 300000,
    PropertyId = "Lease-A-001"
};

// Perform accounting
var accounting = new SuccessfulEffortsAccounting();
accounting.RecordAcquisition(property);
accounting.RecordExplorationCosts(explorationCosts);

// If well is successful
var provedReserves = new ProvedReserves
{
    PropertyId = "Lease-A-001",
    OilReserves = 1000000, // barrels
    GasReserves = 5000000, // MCF
    ReserveDate = new DateTime(2023, 12, 31)
};

accounting.ClassifyAsProved(property, provedReserves);

// Calculate amortization
var amortization = accounting.CalculateAmortization(property, provedReserves, production: 100000);
```

### Full Cost Accounting

```csharp
using Beep.OilandGas.Accounting.FullCost;

var fullCostAccounting = new FullCostAccounting();
fullCostAccounting.RecordExplorationCosts(explorationCosts);
fullCostAccounting.RecordDevelopmentCosts(developmentCosts);

// Calculate amortization based on total reserves
var amortization = fullCostAccounting.CalculateAmortization(
    totalCapitalizedCosts: 5000000,
    totalProvedReserves: 10000000,
    production: 500000
);
```

## Installation

```bash
dotnet add package Beep.OilandGas.Accounting
```

## Documentation

See the [API Documentation](API.md) for detailed information about all classes and methods.

## License

MIT License


