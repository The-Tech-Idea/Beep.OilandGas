# Beep.OilandGas.FlashCalculations

A comprehensive library for phase equilibrium and flash calculations in oil and gas operations.

## Module setup (`IModuleSetup`)

**`FlashCalculationsModule`** (`Beep.OilandGas.FlashCalculations.Modules`) inherits **`ModuleSetupBase`**, **`ModuleId`** **`FLASH_CALCULATIONS`**, **`Order`** **73** (before ProductionForecasting / Nodal). **`EntityTypes`** includes the extension table **`R_FLASH_CALCULATION_REFERENCE_CODE`** (`Beep.OilandGas.FlashCalculations.Data`).

**`SeedAsync`** loads LOVs for:

- **EOS models** (`FLASH_EOS_MODEL`): PR, SRK, SRK_MODIFIED, IDEAL_K  
- **Calculation categories** (`FLASH_CALC_CATEGORY`): isothermal, multistage, phase envelope, rigorous  
- **Solver presets** (`FLASH_SOLVER_PRESET`): DEFAULT (ties to **`FlashConstants`** iterations/tolerance), STRICT, FAST  
- **Specifications** (`FLASH_SPECIFICATION`): PT_SPECIFIED, PH_SPECIFIED (reserved), TP_SPECIFIED  
- **Phase states** (`FLASH_PHASE_STATE`): OVERALL, VAPOR, LIQUID, AQUEOUS  
- **Property kinds** (`FLASH_PROPERTY_KIND`): Z, molar volume, fugacity coefficient, K-value  

DDL for **`R_FLASH_CALCULATION_REFERENCE_CODE`** is driven by entity/metadata tooling — see root **`CLAUDE.md`** (*Schema for extension tables*). Definitions live in **`Data/Constants/FlashReferenceSets.cs`** and **`FlashReferenceCodeSeed.cs`**.

Seeded **`FLASH_EOS_MODEL`** codes (**PR**, **SRK**, **SRK_MODIFIED**, **IDEAL_K**) align with **`FlashCalculationOptions.EquationOfState`** on **`FlashCalculationRequest`** (`Beep.OilandGas.Models.Data.Calculations`), so picklists and orchestration stay consistent with persisted flash options.

### Screening vs full PVT

| In this library | Outside / downstream |
|-----------------|----------------------|
| **Wilson K** + **Rachford–Rice** isothermal flash; vapor-fraction and **xi/yi** screening | Tuned **BIPs**, **multi-phase** (water/oil), **enthalpy** specs, or **regulatory** compositional runs in a dedicated PVT package |
| **`CalculateVaporProperties` / `CalculateLiquidProperties`** — simplified MW and density estimates | Full **EOS** volume translations, **Z** from rigorous iterations, lab **DL** / **CCE** matching |

Use **`FLASH_SOLVER_PRESET`** (**`FAST`** vs **`DEFAULT`** / **`STRICT`**) and EOS picklists per **[`.plans/07_PVT_Best_Practices_And_Reference.md`](.plans/07_PVT_Best_Practices_And_Reference.md)**. Examples below assume **psia** and **Rankine** for **`PRESSURE`** and **`TEMPERATURE`**.

---

## Overview

`Beep.OilandGas.FlashCalculations` provides calculations for:
- Isothermal flash calculations (pressure and temperature specified)
- Phase equilibrium calculations
- K-value calculations
- Phase composition calculations
- Phase property calculations

## Features

### ✅ Flash Calculations
- Isothermal flash (P-T flash)
- Rachford-Rice equation solver
- Wilson correlation for K-values
- Newton-Raphson iteration
- Convergence monitoring

### ✅ Phase Equilibrium
- Vapor-liquid equilibrium
- K-value calculations
- Phase composition calculations
- Phase property calculations

### ✅ Integration
- **`Calculations/AdvancedEOS`** — cubic **PR/SRK** and **Z**-factor helpers used by rigorous / EOS-backed paths
- **`Beep.OilandGas.GasProperties`** — project reference for stack alignment with other packages (the **Wilson + `PerformIsothermalFlash`** path does not call into GasProperties today)

## Installation

```bash
dotnet add package Beep.OilandGas.FlashCalculations
```

## Usage Examples

Shared wire types live in **`Beep.OilandGas.Models.Data.FlashCalculations`** (`FLASH_CONDITIONS`, **`FLASH_COMPONENT`**, thin subclass **`Component`**). **`FlashCalculator`** and **`FlashValidator`** are in this assembly.

### Basic Flash Calculation

```csharp
using System.Collections.Generic;
using Beep.OilandGas.Models.Data.FlashCalculations;
using Beep.OilandGas.FlashCalculations.Calculations;
using Beep.OilandGas.FlashCalculations.Validation;

// Define components (validation uses NAME; Rachford–Rice labeling uses COMPONENT_NAME — keep both in sync)
var methane = new Component
{
    NAME = "Methane",
    COMPONENT_NAME = "Methane",
    MOLE_FRACTION = 0.5m,
    CRITICAL_TEMPERATURE = 343.0m, // Rankine
    CRITICAL_PRESSURE = 667.8m,    // psia
    ACENTRIC_FACTOR = 0.008m,
    MOLECULAR_WEIGHT = 16.04m
};

var ethane = new Component
{
    NAME = "Ethane",
    COMPONENT_NAME = "Ethane",
    MOLE_FRACTION = 0.3m,
    CRITICAL_TEMPERATURE = 549.6m,
    CRITICAL_PRESSURE = 707.8m,
    ACENTRIC_FACTOR = 0.098m,
    MOLECULAR_WEIGHT = 30.07m
};

var propane = new Component
{
    NAME = "Propane",
    COMPONENT_NAME = "Propane",
    MOLE_FRACTION = 0.2m,
    CRITICAL_TEMPERATURE = 665.7m,
    CRITICAL_PRESSURE = 616.3m,
    ACENTRIC_FACTOR = 0.152m,
    MOLECULAR_WEIGHT = 44.10m
};

var conditions = new FLASH_CONDITIONS
{
    PRESSURE = 500m,   // psia
    TEMPERATURE = 540m, // Rankine
    FEED_COMPOSITION = new List<FLASH_COMPONENT> { methane, ethane, propane }
};

FlashValidator.ValidateFlashConditions(conditions);

var flashResult = FlashCalculator.PerformIsothermalFlash(conditions);

Console.WriteLine($"Vapor Fraction: {flashResult.VaporFraction:F4}");
Console.WriteLine($"Liquid Fraction: {flashResult.LiquidFraction:F4}");
Console.WriteLine($"Converged: {flashResult.Converged}");
Console.WriteLine($"Iterations: {flashResult.Iterations}");

Console.WriteLine("\nVapor Composition:");
foreach (var row in flashResult.VaporComposition)
    Console.WriteLine($"  {row.ComponentName}: {row.MoleFraction:F4}");

Console.WriteLine("\nLiquid Composition:");
foreach (var row in flashResult.LiquidComposition)
    Console.WriteLine($"  {row.ComponentName}: {row.MoleFraction:F4}");

Console.WriteLine("\nK-Values:");
foreach (var row in flashResult.KValues)
    Console.WriteLine($"  {row.ComponentName}: {row.KValue:F4}");
```

### Phase Property Calculations

```csharp
using Beep.OilandGas.Models.Data.Calculations;
using Beep.OilandGas.FlashCalculations.Calculations;

// Calculate vapor / liquid summaries (screening estimates in current implementation)
var vaporProperties = FlashCalculator.CalculateVaporProperties(flashResult, conditions);
var liquidProperties = FlashCalculator.CalculateLiquidProperties(flashResult, conditions);

Console.WriteLine($"Vapor Molecular Weight: {vaporProperties.MolecularWeight:F2} lb/lbmol");
Console.WriteLine($"Vapor Density: {vaporProperties.Density:F4} lb/ft³");

Console.WriteLine($"Liquid Molecular Weight: {liquidProperties.MolecularWeight:F2} lb/lbmol");
Console.WriteLine($"Liquid Density: {liquidProperties.Density:F4} lb/ft³");
```

### Multi-Component Mixture

```csharp
using System.Collections.Generic;
using Beep.OilandGas.Models.Data.FlashCalculations;
using Beep.OilandGas.FlashCalculations.Calculations;
using Beep.OilandGas.FlashCalculations.Validation;

var components = new List<FLASH_COMPONENT>
{
    new Component { NAME = "C1", COMPONENT_NAME = "C1", MOLE_FRACTION = 0.4m,
        CRITICAL_TEMPERATURE = 343.0m, CRITICAL_PRESSURE = 667.8m, ACENTRIC_FACTOR = 0.008m, MOLECULAR_WEIGHT = 16.04m },
    new Component { NAME = "C2", COMPONENT_NAME = "C2", MOLE_FRACTION = 0.25m,
        CRITICAL_TEMPERATURE = 549.6m, CRITICAL_PRESSURE = 707.8m, ACENTRIC_FACTOR = 0.098m, MOLECULAR_WEIGHT = 30.07m },
    new Component { NAME = "C3", COMPONENT_NAME = "C3", MOLE_FRACTION = 0.15m,
        CRITICAL_TEMPERATURE = 665.7m, CRITICAL_PRESSURE = 616.3m, ACENTRIC_FACTOR = 0.152m, MOLECULAR_WEIGHT = 44.10m },
    new Component { NAME = "iC4", COMPONENT_NAME = "iC4", MOLE_FRACTION = 0.1m,
        CRITICAL_TEMPERATURE = 734.1m, CRITICAL_PRESSURE = 527.9m, ACENTRIC_FACTOR = 0.176m, MOLECULAR_WEIGHT = 58.12m },
    new Component { NAME = "nC4", COMPONENT_NAME = "nC4", MOLE_FRACTION = 0.1m,
        CRITICAL_TEMPERATURE = 765.3m, CRITICAL_PRESSURE = 550.7m, ACENTRIC_FACTOR = 0.193m, MOLECULAR_WEIGHT = 58.12m }
};

var flashConditions = new FLASH_CONDITIONS
{
    PRESSURE = 1000m,
    TEMPERATURE = 560m,
    FEED_COMPOSITION = components
};

var result = FlashCalculator.PerformIsothermalFlash(flashConditions);
FlashValidator.ValidateFlashResult(result);
```

## API Reference

### Models (`Beep.OilandGas.Models`)

- **`FLASH_COMPONENT`** — feed row (**`COMPONENT_NAME`**, **`MOLE_FRACTION`**, **`CRITICAL_*`**, **`ACENTRIC_FACTOR`**, **`MOLECULAR_WEIGHT`**)
- **`Component`** — thin subclass of **`FLASH_COMPONENT`** (optional convenience type)
- **`FLASH_CONDITIONS`** — **`PRESSURE`**, **`TEMPERATURE`**, **`FEED_COMPOSITION`**
- **`FlashResult`** — vapor/liquid fractions, iterations, **`VaporComposition`** / **`LiquidComposition`** (**`FlashComponentFraction`**), **`KValues`**
- **`PhasePropertiesData`** (`Beep.OilandGas.Models.Data.Calculations`) — screening MW/density from **`CalculateVaporProperties`** / **`CalculateLiquidProperties`**

### Calculations

- **`FlashCalculator`** — Wilson K, Rachford–Rice, **`PerformIsothermalFlash`**, phase compositions, screening phase properties
- **`AdvancedEOS`** — PR/SRK cubic roots and **Z**-related helpers
- **`MultiComponentFlash`**, **`PhaseEnvelope`** — extended / envelope paths (see XML docs on types)
- **`FlashFeedCatalogMerge`** — builds stage **N+1** feed from liquid fractions + original component catalog (used by **`FlashCalculationService.PerformMultiStageFlash`**)

### Validation

- `FlashValidator` - Input validation

### Constants

- `FlashConstants` - Standard values, convergence parameters

### Exceptions (`Beep.OilandGas.FlashCalculations.Exceptions`)

All types are defined in **`Exceptions/FlashException.cs`**:

- **`FlashException`** — base
- **`InvalidFlashConditionsException`**
- **`InvalidComponentException`**
- **`FlashConvergenceException`** — also used when **`ValidateFlashResult`** rejects fractions

## Algorithm Details

### Rachford-Rice Equation

The Rachford-Rice equation is solved using Newton-Raphson iteration:

```
Σ(zi * (Ki - 1) / (1 + V * (Ki - 1))) = 0
```

Where:
- V = vapor fraction
- zi = feed mole fraction of component i
- Ki = K-value (equilibrium ratio) of component i

### K-Value Initialization

K-values are initialized using the Wilson correlation:

```
Ki = (Pci/P) * exp(5.37 * (1 + ωi) * (1 - Tci/T))
```

Where:
- Pci = critical pressure of component i
- Tci = critical temperature of component i
- ωi = acentric factor of component i
- P = system pressure
- T = system temperature

## Dependencies

- **`Beep.OilandGas.Models`** — shared **`IFlashCalculationService`** contract and **`FlashCalculation*`** / **`FLASH_*`** wire types
- **`Beep.OilandGas.GasProperties`** — referenced alongside the rest of the solution; EOS / **Z** math used here lives primarily in **`AdvancedEOS`**
- **`Beep.OilandGas.PPDM39.DataManagement`** — **`ModuleSetupBase`**, **`FlashCalculationService`** persistence helpers
- **`SkiaSharp`** — graphics (optional future charts)

## Service entry point

For orchestrated flash (including **multi-stage** feeds that reuse **Tc / Pc / ω / MW** from the initial composition), inject **`IFlashCalculationService`** / **`FlashCalculationService`** (`Services/`). **`PerformMultiStageFlash`** builds each stage’s feed from the previous liquid composition merged against the **original** feed catalog.

**`RunRigorousFlashAsync`** (`FlashCalculationRequest`, **`CancellationToken`**) runs the same Wilson + Rachford–Rice path and returns **`FlashCalculationResult`**. In **`Beep.OilandGas.ApiService`**: **`POST /api/FlashCalculation/rigorous`** with a **`FlashCalculationRequest`** body (`Pressure`, `Temperature`, `FeedComposition` as **`Component`** rows); uses **`HttpContext.RequestAborted`** via the action **`CancellationToken`**.

## Documentation & planning

| Resource | Purpose |
|----------|---------|
| **[`.plans/README.md`](.plans/README.md)** | Phased enhancement index |
| **[`MASTER-TODO-TRACKER.md`](MASTER-TODO-TRACKER.md)** | Status rollup |
| **[`IMPLEMENTATION_SUMMARY.md`](IMPLEMENTATION_SUMMARY.md)** | Project layout and build commands |

## Source Files

Based on Petroleum Engineer XLS files:
- `LP - Flash.xls`

## License

MIT License

## Contributing

Contributions are welcome! Please follow the project's coding standards and submit pull requests.

