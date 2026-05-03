# MASTER-TODO-TRACKER — Beep.OilandGas.WellTestAnalysis

Rollup for **PTA**, **build-up / drawdown**, **derivatives**, **gas m(p)**, **type curves**, **Skia** plots, and **`IWellTestAnalysisService`**. Phased detail: **[`.plans/README.md`](.plans/README.md)**.

## Phase rollup

| Phase | Document | Status |
|-------|----------|--------|
| 0 | [.plans/00_WellTestAnalysis_Overview_And_Baseline.md](.plans/00_WellTestAnalysis_Overview_And_Baseline.md) | **Drafted** |
| 1 | [.plans/01_Calculations_Validation_Services_And_PPDM_Alignment.md](.plans/01_Calculations_Validation_Services_And_PPDM_Alignment.md) | **In progress** — radial-flow limits drafted; **flow-regime** + smoothing literals on **`WellTestConstants`**; default **`WellTestAnalysisService`** stubs explicit **`NotImplementedException`** |
| 2 | [.plans/02_Tests_Verification_And_Golden_Vectors.md](.plans/02_Tests_Verification_And_Golden_Vectors.md) | **In progress** — **20** tests (analyzer, service, validator, **`IdentifyModel`**); extend golden vectors as needed |
| 3 | [.plans/03_Documentation_Packaging_And_Consumers.md](.plans/03_Documentation_Packaging_And_Consumers.md) | **In progress** — README, **`API.md`**, **`PackageReadmeFile`**, **`ENHANCEMENT_PLAN`** reconciliation |
| 4 | [.plans/04_Industry_Scenarios_Oil_Gas_WellTest_Best_Practices.md](.plans/04_Industry_Scenarios_Oil_Gas_WellTest_Best_Practices.md) | **Drafted** |
| Run | [.plans/08_Consolidated_Execution_Checklist.md](.plans/08_Consolidated_Execution_Checklist.md) | **Active** |

## Consolidated next actions

| Priority | Action |
|----------|--------|
| **P0** | ~~Fix **README** / examples~~ — quick start uses **`WELL_TEST_DATA`** / **`WellTestAnalyzer`**; **`API.md`** added |
| **P1** | Host **`IWellTestAnalysisService`** when required; default **`WellTestAnalysisService`** sets **`CalculationWellUwi`** from **`wellUWI`** when empty; **`WellTestAnalysisCalculationRequest.AnalysisType`** optional — **`PPDMCalculationService`** infers **BUILDUP**/**DRAWDOWN** from PPDM **`WELL_TEST.TEST_TYPE`** when omitted (**`DataFlowService`** path); **`LifeCycle`** well-test (null request + null-safe failure); **DRAWDOWN** rejects **MDH**; PPDM duration / press series / **OBS_NO** sort; **`WellTestAnalysis.razor`** / **`ResolveEffectiveFieldId`**; **`WELL_TEST_DATA`**: **`CalculationWellUwi`** vs **`AREA_ID`**; **`Client`** legacy **`AREA_ID`** UWI fallback; **`DataFlowService`** optional **`fieldId`** |
| **P2** | Phase **2** — more **`IdentifyFlowRegimes`** / **`IdentifyModel`** cases; integration tests with real repository when API lands |
| **P3** | ~~**Package readme** / **`.csproj`**~~ — **`PackageReadmeFile`** + packed README |
| **P4** | Scenario-tagged tests + applicability notes (phase **4**) |

## Verification

```bash
dotnet build Beep.OilandGas.WellTestAnalysis/Beep.OilandGas.WellTestAnalysis.csproj
dotnet test Beep.OilandGas.WellTestAnalysis.Tests/Beep.OilandGas.WellTestAnalysis.Tests.csproj
```
