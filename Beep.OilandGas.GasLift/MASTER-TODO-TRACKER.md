# MASTER-TODO-TRACKER — Beep.OilandGas.GasLift

Rollup for **gas lift** enhancement. Details: **[`.plans/README.md`](.plans/README.md)**.

## Phase rollup

| Phase | Document | Status |
|-------|----------|--------|
| 0 | [.plans/00_GasLift_Overview_And_Baseline.md](.plans/00_GasLift_Overview_And_Baseline.md) | Baseline |
| 1 | [.plans/01_ModuleSetup_And_Reference_LOV.md](.plans/01_ModuleSetup_And_Reference_LOV.md) | Done — **`GasLiftModule`** + **`R_GAS_LIFT_REFERENCE_CODE`** seed |
| 2 | [.plans/02_Tests_And_Verification.md](.plans/02_Tests_And_Verification.md) | Done — catalog + module + **`GasLiftSpacingAndDesignRegressionTests`** |
| 3 | [.plans/03_Documentation_And_Packaging.md](.plans/03_Documentation_And_Packaging.md) | Done — README + **`PackageReadmeFile`** |
| Run | [.plans/08_Consolidated_Execution_Checklist.md](.plans/08_Consolidated_Execution_Checklist.md) | Active |

## Consolidated next actions

| Priority | Action |
|----------|--------|
| Done | **`GasLiftModule`** — **`ModuleId` GAS_LIFT**, **`Order` 74**, **`SeedAsync`** for **`R_GAS_LIFT_REFERENCE_CODE`** |
| Done | LOVs: port sizes (from **`GasLiftConstants`**), operating mode, design method, valve service, injection source, design-limit keys |
| Done | Regression / golden-style tests — **`GasLiftSpacingAndDesignRegressionTests`** (equal-depth depths, spacing determinism, **`DesignValvesUS`** invariants) |
| Done | **`OptimizeValveSpacingAsync`** accepts **`CancellationToken`** (optional); **`DesignValves`** calls **`GasLiftValidator.ValidateCalculationParameters`** |
| Done | **`/api/gaslift/analyze-potential`** returns **`GAS_LIFT_POTENTIAL_RESULT`**; **`IGasLiftService`** async + cancellation; **`GasLiftPotentialWireMapper`** + **`GasLiftPotentialWireMapperTests`**; **`AnalysisService.GasLift`** deserializes potential result directly |
| Done | **`GasLiftController`** rejects min/max injection inversion before service call; **`GasLiftPotentialCalculator`** guards points/range; **`GasLiftController`** maps **`ArgumentException`** / **`GasLiftException`** to **400**; **`ICalculationServiceClient`** gas-lift methods accept **`CancellationToken`**; **`GasLift.razor`** clamps inputs |
| Done | **`GasLiftService`** analyze paths call **`ValidateWellProperties`**; **`GasLift.razor`** screening stub fills PVT/ratings for API + valve design; **`AnalyzeGasLiftPotentialRequest`** injection **50–10000** range; **`SaveDesign`** maps **`ArgumentException`** / **`GasLiftException`** to **400** |

## Verification

```bash
dotnet build Beep.OilandGas.GasLift/Beep.OilandGas.GasLift.csproj
dotnet test Beep.OilandGas.GasLift.Tests/Beep.OilandGas.GasLift.Tests.csproj
```
