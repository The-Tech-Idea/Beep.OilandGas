# MASTER-TODO-TRACKER — Beep.OilandGas.GasLift

Rollup for **gas lift** enhancement. Details: **[`.plans/README.md`](.plans/README.md)**.

## Phase rollup

| Phase | Document | Status |
|-------|----------|--------|
| 0 | [.plans/00_GasLift_Overview_And_Baseline.md](.plans/00_GasLift_Overview_And_Baseline.md) | Baseline |
| 1 | [.plans/01_ModuleSetup_And_Reference_LOV.md](.plans/01_ModuleSetup_And_Reference_LOV.md) | Done — **`GasLiftModule`** + **`R_GAS_LIFT_REFERENCE_CODE`** seed |
| 2 | [.plans/02_Tests_And_Verification.md](.plans/02_Tests_And_Verification.md) | In progress — **`GasLift.Tests`** (4) catalog + module contract |
| 3 | [.plans/03_Documentation_And_Packaging.md](.plans/03_Documentation_And_Packaging.md) | Done — README + **`PackageReadmeFile`** |
| Run | [.plans/08_Consolidated_Execution_Checklist.md](.plans/08_Consolidated_Execution_Checklist.md) | Active |

## Consolidated next actions

| Priority | Action |
|----------|--------|
| Done | **`GasLiftModule`** — **`ModuleId` GAS_LIFT**, **`Order` 74**, **`SeedAsync`** for **`R_GAS_LIFT_REFERENCE_CODE`** |
| Done | LOVs: port sizes (from **`GasLiftConstants`**), operating mode, design method, valve service, injection source, design-limit keys |
| P1 | Golden vectors for **valve design** / **spacing** vs trusted reference |
| P2 | Optional **`IGasLiftService`** async + cancellation parity (align with API) |

## Verification

```bash
dotnet build Beep.OilandGas.GasLift/Beep.OilandGas.GasLift.csproj
dotnet test Beep.OilandGas.GasLift.Tests/Beep.OilandGas.GasLift.Tests.csproj
```
