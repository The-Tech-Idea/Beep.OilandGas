# MASTER-TODO-TRACKER — Beep.OilandGas.OilProperties

Rollup for **black-oil PVT correlations**, **`IOilPropertiesService`** / **`OilPropertiesService`**, **validation**, **tests**, and **industry scenario coverage**. Phased detail: **[`.plans/README.md`](.plans/README.md)**.

## Phase rollup

| Phase | Document | Status |
|-------|----------|--------|
| 0 | [.plans/00_OilProperties_Overview_And_Baseline.md](.plans/00_OilProperties_Overview_And_Baseline.md) | **Baseline updated** — Rankine/°F path documented in code |
| 1 | [.plans/01_Calculators_Correlations_Service_And_Units.md](.plans/01_Calculators_Correlations_Service_And_Units.md) | **In progress** — °R→°F, FVF via calculator, optional γg on FVF, validator on Advanced |
| 2 | [.plans/02_Tests_And_Verification.md](.plans/02_Tests_And_Verification.md) | **Started** — `Beep.OilandGas.OilProperties.Tests` (calculator, units, validator) |
| 3 | [.plans/03_Documentation_Packaging_And_Contracts.md](.plans/03_Documentation_Packaging_And_Contracts.md) | **Started** — README + package readme |
| 4 | [.plans/04_Industry_Black_Oil_PVT_Scenarios.md](.plans/04_Industry_Black_Oil_PVT_Scenarios.md) | **Drafted** |
| Run | [.plans/08_Consolidated_Execution_Checklist.md](.plans/08_Consolidated_Execution_Checklist.md) | **Active** |

## Consolidated next actions

| Priority | Action |
|----------|--------|
| **P0** | ~~Fix **temperature unit contract**~~ — **`OilPropertyUnits`**, Advanced path, service FVF/viscosity use **°R** input |
| **P1** | Undersaturated **Bo/μ**, **`CalculateBlackOilPropertiesAsync`** branch audit, correlation applicability warnings |
| **P2** | Extend tests: **`CalculateFormationVolumeFactor`** vs calculator, **`CalculateBlackOilPropertiesAsync`** (Moq DI), non-Standing correlation policy |
| **P3** | **`IMPLEMENTATION_SUMMARY`**; **`CalculateFVFRequest.GasSpecificGravity`** + API clamp — **Done** |
| **P4** | Applicability warnings + scenario-tagged tests (phase **4**) |

## Verification

```bash
dotnet build Beep.OilandGas.OilProperties/Beep.OilandGas.OilProperties.csproj
dotnet test Beep.OilandGas.OilProperties.Tests/Beep.OilandGas.OilProperties.Tests.csproj
```

*(Test command applies after the test project is added in phase 2.)*
