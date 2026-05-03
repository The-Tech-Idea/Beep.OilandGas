# MASTER-TODO-TRACKER — Beep.OilandGas.HydraulicPumps

Rollup for **hydraulic jet / piston pump** calculations and **`HydraulicPumpService`**. Plans: **[`.plans/README.md`](.plans/README.md)**.

## Phase rollup

| Phase | Document | Status |
|-------|----------|--------|
| 0 | [.plans/00_HydraulicPumps_Overview_And_Baseline.md](.plans/00_HydraulicPumps_Overview_And_Baseline.md) | Baseline drafted |
| Tests | `Beep.OilandGas.HydraulicPumps.Tests` | **Extended** — jet/piston regression, `validateInputs` guard tests, validator, `HydraulicPumpCalculator`, jet null guard |
| Refactor | Shared calculations + constants | **Done** — `HydraulicPumpSharedCalculations`, `HydraulicPumpConstants`; jet/piston use shared discharge / density; dead `GasProperties` ref removed from project |

## Next actions

| Priority | Action |
|----------|--------|
| P1 | Reconcile **`IHydraulicPumpService`** (Models narrow vs feature-wide); split contracts or merge DTOs when product settles |
| P2 | Extend tests (`HydraulicPumpCalculator`, service smoke with Moq, persistence paths; `AnalyzeJetPumpPerformanceAsync` HP assertions if API stabilizes) |
| P3 | Optional **`R_HYDRAULIC_PUMP_REFERENCE_CODE`** + **`HydraulicPumpsModule`** if product needs DB-driven pump LOVs |

## Verification

```bash
dotnet build Beep.OilandGas.HydraulicPumps/Beep.OilandGas.HydraulicPumps.csproj
dotnet test Beep.OilandGas.HydraulicPumps.Tests/Beep.OilandGas.HydraulicPumps.Tests.csproj
```
