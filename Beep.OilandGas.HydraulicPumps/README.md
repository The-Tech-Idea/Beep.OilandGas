# Beep.OilandGas.HydraulicPumps

Library for **hydraulic jet** and **hydraulic piston** pump performance calculations used with PPDM-aligned well and pump entities.

## Planning

Phased notes: **[`.plans/README.md`](.plans/README.md)**. Rollup: **[`MASTER-TODO-TRACKER.md`](MASTER-TODO-TRACKER.md)**.

## Overview

- **Jet pump** — `HydraulicJetPumpCalculator.CalculatePerformance`
- **Piston pump** — `HydraulicPistonPumpCalculator.CalculatePerformance`
- **Shared screening math** — `HydraulicPumpSharedCalculations` (liquid density, tubing friction, discharge pressure); physical constants in `HydraulicPumpConstants`
- Optional **`validateInputs: true`** on both calculators runs `HydraulicPumpValidator` before numbers (throws `InvalidWellPropertiesException` / `InvalidPumpPropertiesException` when out of range)
- **System efficiency** on jet/piston results is **ratio of hydraulic HP to power-fluid HP**, clamped to **\[0, 1\]** so screening outputs stay in a physical reporting range (simplified correlations can otherwise exceed unity).
- **Validation** — `HydraulicPumpValidator`
- **Orchestration** — `HydraulicPumpService` (partial; DI via `Program.cs` registers **`Beep.OilandGas.Models.Core.Interfaces.IHydraulicPumpService`**)

## Data types (where they live)

Wire and table-shaped types are in **`Beep.OilandGas.Models.Data.HydraulicPumps`** (e.g. **`HYDRAULIC_PUMP_WELL_PROPERTIES`**, **`HYDRAULIC_JET_PUMP_PROPERTIES`**, **`HYDRAULIC_JET_PUMP_RESULT`**, **`HYDRAULIC_PISTON_PUMP_PROPERTIES`**, **`HYDRAULIC_PISTON_PUMP_RESULT`**). Physical DDL is under **`Beep.OilandGas.Models/Scripts/**/Hydraulicpumps/`**.

There is **no** `Beep.OilandGas.HydraulicPumps.Models` namespace in this repo — use **`Models.Data.HydraulicPumps`** from the **`Beep.OilandGas.Models`** assembly.

## Dependencies

- **`Beep.OilandGas.Models`**, **`Beep.OilandGas.PPDM39.DataManagement`** — entities and persistence helpers for **`HydraulicPumpService`**
- **SkiaSharp** (and related packages) — reserved for optional visualization; not required for core math

## Quick start (jet pump)

```csharp
using Beep.OilandGas.HydraulicPumps.Calculations;
using Beep.OilandGas.Models.Data.HydraulicPumps;

var well = new HYDRAULIC_PUMP_WELL_PROPERTIES
{
    WELL_DEPTH = 8000m,
    TUBING_DIAMETER = 2.875m,
    CASING_DIAMETER = 7m,
    WELLHEAD_PRESSURE = 50m,
    BOTTOM_HOLE_PRESSURE = 500m,
    WELLHEAD_TEMPERATURE = 520m,
    BOTTOM_HOLE_TEMPERATURE = 580m,
    OIL_GRAVITY = 35m,
    WATER_CUT = 0.2m,
    GAS_OIL_RATIO = 200m,
    GAS_SPECIFIC_GRAVITY = 0.65m,
    DESIRED_PRODUCTION_RATE = 500m,
    PUMP_DEPTH = 7500m
};

var jet = new HYDRAULIC_JET_PUMP_PROPERTIES
{
    NOZZLE_DIAMETER = 0.5m,
    THROAT_DIAMETER = 1.0m,
    DIFFUSER_DIAMETER = 1.5m,
    POWER_FLUID_PRESSURE = 2000m,
    POWER_FLUID_RATE = 300m,
    POWER_FLUID_SPECIFIC_GRAVITY = 1.0m
};

var result = HydraulicJetPumpCalculator.CalculatePerformance(well, jet, validateInputs: true);
// result.PRODUCTION_RATE, result.TOTAL_FLOW_RATE, result.PUMP_EFFICIENCY, ...
```

## Tests

```bash
dotnet test Beep.OilandGas.HydraulicPumps.Tests/Beep.OilandGas.HydraulicPumps.Tests.csproj
```

## License

MIT
