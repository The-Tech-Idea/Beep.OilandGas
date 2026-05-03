# Phase 0 — Hydraulic pumps overview and baseline

## Scope

- **Calculations:** `HydraulicJetPumpCalculator`, `HydraulicPistonPumpCalculator`, `HydraulicPumpCalculator`.
- **Validation:** `HydraulicPumpValidator`.
- **Services:** `HydraulicPumpService` (partial), wide surface on `Beep.OilandGas.HydraulicPumps.Services.IHydraulicPumpService`; **Models** contract `Beep.OilandGas.Models.Core.Interfaces.IHydraulicPumpService` (narrow) implemented explicitly on the same partial class.

## Data model

- Table-style entities and projections live under **`Beep.OilandGas.Models.Data.HydraulicPumps`** (e.g. `HYDRAULIC_PUMP_WELL_PROPERTIES`, `HYDRAULIC_JET_PUMP_PROPERTIES`, `HYDRAULIC_JET_PUMP_RESULT`).
- DDL scripts ship under **`Beep.OilandGas.Models/Scripts/**/Hydraulicpumps/`** (PPDM-aligned naming, not a separate `R_*` extension module in this assembly).

## Dependencies

- **`Beep.OilandGas.GasProperties`** — Z-factor / gas property support in calculators.
- **`Beep.OilandGas.PPDM39.DataManagement`** — service persistence patterns.

## Module setup

- **No** `ModuleSetupBase` in this project today: there is **no** feature-local `R_HYDRAULIC_*` extension LOV table analogous to **`GasLiftModule`** unless product adds one later.

## Verification

```bash
dotnet test Beep.OilandGas.HydraulicPumps.Tests/Beep.OilandGas.HydraulicPumps.Tests.csproj
```
