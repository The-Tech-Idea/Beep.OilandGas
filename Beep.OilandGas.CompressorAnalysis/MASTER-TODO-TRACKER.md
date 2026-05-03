# MASTER-TODO-TRACKER — Beep.OilandGas.CompressorAnalysis

Single rollup for **planning and execution**. Detailed steps live under [.plans/](.plans/README.md).

## Phase rollup

| Phase | Document | Status |
|-------|----------|--------|
| 0 | [.plans/00_CompressorAnalysis_Overview_And_Baseline.md](.plans/00_CompressorAnalysis_Overview_And_Baseline.md) | Doc baseline |
| 1 | [.plans/01_Phase_Service_Contracts_And_Models.md](.plans/01_Phase_Service_Contracts_And_Models.md) | Done — **`CompressorAnalysis.Core.Interfaces`**, **`CompressorAnalysis.Data`** tables, **`Models.Data.Calculations`** wire types; **`FormatIdForTable`** via **`IPPDM39DefaultsRepository`** |
| 2 | [.plans/02_Phase_Calculation_And_Validation.md](.plans/02_Phase_Calculation_And_Validation.md) | In progress — golden vectors, **`CompressorValidator`** suite, curve/surge regression (**no SkiaSharp**) |
| 3 | [.plans/03_Phase_PPDM_ModuleSetup_And_Data.md](.plans/03_Phase_PPDM_ModuleSetup_And_Data.md) | Done — **`CompressorAnalysisModule`** |
| 4 | [.plans/04_Phase_API_And_Orchestration.md](.plans/04_Phase_API_And_Orchestration.md) | Done — **`CompressorController`** → service; **`PerformCompressorAnalysisAsync`** **`POWER`** vs **`PRESSURE`** (`AnalysisType`) |
| 5 | [.plans/05_Phase_Tests_And_Verification.md](.plans/05_Phase_Tests_And_Verification.md) | In progress — **`CompressorAnalysis.Tests`** **19** (incl. **`CompressorAnalysisReferenceSeedCatalogTests`** ×3); **`PPDMCalculationServiceCompressorTests`** **12**; **`PPDMCalculationServiceFacilitiesTests`** **1**; **`CompressorControllerTests`** **1** |
| 6 | [.plans/06_Phase_Packaging_Docs_And_Backlog.md](.plans/06_Phase_Packaging_Docs_And_Backlog.md) | Planned |
| Reference | [.plans/07_Scenarios_Best_Practices_And_Reference.md](.plans/07_Scenarios_Best_Practices_And_Reference.md) | Doc baseline |
| Checklist | [.plans/08_Consolidated_Execution_Checklist.md](.plans/08_Consolidated_Execution_Checklist.md) | Doc baseline |

Update the **Status** column when a phase’s exit criteria are satisfied in the codebase.

## Repository standards (always)

- [CLAUDE.md](../../CLAUDE.md) — three-layer architecture, **extension tables**: entities + **`IModuleSetup.EntityTypes`** + tooling (no hand-written `Models/Scripts` DDL per feature).
- Most cross-library APIs live in **`Beep.OilandGas.Models.Core.Interfaces`**; **`ICompressorAnalysisService`** is defined in **`Beep.OilandGas.CompressorAnalysis.Core.Interfaces`** (feature-owned; depends on **`CompressorAnalysis.Data`** table types).
- **`PPDM39.DataManagement`** must not gain a project reference **to** CompressorAnalysis (domain stays out of infrastructure).

## Consolidated next actions (editable)

| Priority | Action | Owner phase |
|----------|--------|----------------|
| Done | Align `.csproj` with sibling analysis libs: explicit **`Models`**, **`PPDM39`**, **`PPDM39.DataManagement`**, **`PackageReadmeFile`**, **nullable** | 6 |
| Done | **`ICompressorAnalysisService`** + ApiService DI; **`CompressorController`** async delegation | 1 / 4 |
| Done | **`CompressorAnalysisModule`** (`EntityTypes` ×6 incl. **`R_COMPRESSOR_ANALYSIS_REFERENCE_CODE`**, **`SeedAsync`** LOV seed) | 3 |
| Done | **`PerformCompressorAnalysisAsync`** → **`ICompressorAnalysisService.CalculateRequiredPressureAsync`** | 4 |
| Done | **`Beep.OilandGas.CompressorAnalysis.Tests`** — module + service + golden vectors + **`CompressorValidatorTests`** + **`CompressorCurveRegressionTests`** (**16 tests**) | 5 |
| Done | **`FormatIdForTable`** uses **`IPPDM39DefaultsRepository`** when registered in ApiService | 1 |
| Done | Compressor **table** entities + **`ICompressorAnalysisService`** live under **`Beep.OilandGas.CompressorAnalysis`** (`Data/Tables`, `Core/Interfaces`); **`Models`** keeps wire DTOs in **`Data/Calculations`** only | 1–3 |
| Done | **`ApiService.Tests`** — **`PPDMCalculationServiceCompressorTests`** (orchestration branches + **`AdditionalParameters`** + service exception → **`CalculationRunStatus.Failed`**) | 4 / 5 |
| Done | **`AdditionalParameters`** — geometry/speed, **`MaxDriverHorsepower`**, effective **`AnalysisType`**/**`CompressorType`** from options when top-level unset (see **`PPDMCalculationService.Facilities`**) + tests | 2 / 5 |
| Done | **`PerformCompressorAnalysisAsync`** — **`ArgumentNullException`** on null request; **`POST compressor`** rethrows **`OperationCanceledException`** (aligned with choke); tests | 4 / 5 |
| Done | **`PerformPipelineAnalysisAsync`** null guard; **`CompressorController`** rethrows **`OperationCanceledException`**; **`CalculationServiceClient.PerformCompressorAnalysisAsync`** null guard (safe logging); **`PPDMCalculationServiceFacilitiesTests`** + **`CompressorControllerTests`** | 4 / 5 |
| Done | **`PerformCompressorAnalysisAsync`** / **`PerformPipelineAnalysisAsync`** — rethrow **`OperationCanceledException`** (do not map to **`CalculationRunStatus.Failed`**); orchestration test | 4 / 5 |
| — | SkiaSharp performance maps — backlog / separate package boundary | 6 |

## Verification

```bash
dotnet build Beep.OilandGas.CompressorAnalysis/Beep.OilandGas.CompressorAnalysis.csproj
dotnet build Beep.OilandGas.ApiService/Beep.OilandGas.ApiService.csproj
dotnet test Beep.OilandGas.CompressorAnalysis.Tests
dotnet test Beep.OilandGas.ApiService.Tests --filter "FullyQualifiedName~PPDMCalculationServiceChokeTests"
dotnet test Beep.OilandGas.ApiService.Tests --filter "FullyQualifiedName~PPDMCalculationServiceCompressorTests"
dotnet test Beep.OilandGas.ApiService.Tests --filter "FullyQualifiedName~CompressorCalculationsControllerTests"
dotnet test Beep.OilandGas.ApiService.Tests --filter "FullyQualifiedName~PPDMCalculationServiceFacilitiesTests"
dotnet test Beep.OilandGas.ApiService.Tests --filter "FullyQualifiedName~CompressorControllerTests"
```

**Last run (local):** `CompressorAnalysis.Tests` — 19/19; `Beep.OilandGas.Client` — build OK (stale **`client_build_output.txt`** removed); `PPDMCalculationServiceChokeTests` — 6/6; `PPDMCalculationServiceCompressorTests` — 12/12; `CompressorCalculationsControllerTests` — 4/4; `PPDMCalculationServiceFacilitiesTests` — 1/1; `CompressorControllerTests` — 1/1 (combined compressor/facilities API filter **18**/18).

### Consumer note

Clients that need **pressure-only** facility analysis must send **`AnalysisType`** matching **`CompressorAnalysisWellKnown.AnalysisType.Pressure`** (`"PRESSURE"`, case-insensitive after trim). Default **`POWER`** runs the full power path (recip vs centrifugal from **`CompressorType`**). Prefer **`CompressorAnalysisWellKnown`** in code instead of string literals.

Result **`Status`** uses **`CalculationRunStatus.Success`** / **`CalculationRunStatus.Failed`** (same wire strings as before). **`CompressorAnalysis.razor`** exposes optional **`AdditionalParameters`** (HP cap, speed, recip geometry) in an Advanced panel.
