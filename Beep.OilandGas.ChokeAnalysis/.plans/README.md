# Beep.OilandGas.ChokeAnalysis — phased plans

Module-local execution plans for **ChokeAnalysis**: gas choke flow math, `IChokeAnalysisService`, PPDM-aware helpers, and API integration via `ICalculationService` / `CalculationsController`.

## Non-negotiable solution standards

Apply on every phase (see [CLAUDE.md](../../CLAUDE.md) and [.cursor/commands](../../.cursor/commands/) for full text):

- **Three layers**: Web → API → Data (`IDMEEditor`, `PPDMGenericRepository`); this library is primarily **domain/calculation** with optional PPDM entity inputs.
- **Interfaces**: `IChokeAnalysisService` lives in `Beep.OilandGas.Models.Core.Interfaces` only.
- **Shared models**: `CHOKE_PROPERTIES`, `GAS_CHOKE_PROPERTIES`, `CHOKE_FLOW_RESULT`, etc. in `Beep.OilandGas.Models/Data/ChokeAnalysis/` — **no** separate `DTO` namespaces.
- **Table vs projection**: Types persisted via repository must be **scalar table shapes** extending `ModelEntityBase`; rich reports stay projections (see Models under `Data/Calculations/` for comprehensive choke reports).
- **Wells**: Any direct `WELL` / `WELL_STATUS` / `WELL_XREF` access must go through **WellServices** in DataManagement — not raw repositories in feature code (ChokeAnalysis may receive `WELL` from callers; do not add parallel well CRUD here).
- **DI**: New services register in `Beep.OilandGas.ApiService/Program.cs` with **factory** `AddScoped<IFoo>(sp => new Foo(...))` after `AddBeepServices` / metadata consumers order is respected (read Program.cs ~1–120 when touching registration).
- **Data access**: Prefer `AppFilter` + `PPDMGenericRepository`; no `ExecuteSql` for app SELECT paths.
- **NuGet**: Packable projects use `PackageReadmeFile` + include `README.md` (see project `.csproj`).

## Phase documents

| Phase | File | Focus |
|-------|------|--------|
| 0 | [00_ChokeAnalysis_Overview_And_Baseline.md](00_ChokeAnalysis_Overview_And_Baseline.md) | Scope, baseline inventory, risks |
| 1 | [01_Phase_Service_Contracts.md](01_Phase_Service_Contracts.md) | `IChokeAnalysisService`, Models alignment |
| 2 | [02_Phase_Calculation_And_Validation_Surface.md](02_Phase_Calculation_And_Validation_Surface.md) | Calculators, validators, numerical edge cases |
| 3 | [03_Phase_PPDM_And_Data_Paths.md](03_Phase_PPDM_And_Data_Paths.md) | `WELL`/`WELL_TEST_*` mapping, ValueRetrievers |
| 4 | [04_Phase_API_And_Orchestration_Integration.md](04_Phase_API_And_Orchestration_Integration.md) | `ICalculationService`, controller, field context |
| 5 | [05_Phase_Tests_And_Verification.md](05_Phase_Tests_And_Verification.md) | Unit/API regression, build gates |
| 6 | [06_Phase_Packaging_Docs_And_Backlog.md](06_Phase_Packaging_Docs_And_Backlog.md) | README, nupkg, future enhancements |
| — | [07_Scenarios_Best_Practices_And_Industry_Reference.md](07_Scenarios_Best_Practices_And_Industry_Reference.md) | **Field scenarios**, choke types, fluids/reservoirs, model selection, parameters, external references |
| — | [08_Consolidated_Execution_Checklist.md](08_Consolidated_Execution_Checklist.md) | **Single runbook** aggregating checks from phases 0–6 |
| — | [09_Interface_Surface_Canonical_vs_Extended.md](09_Interface_Surface_Canonical_vs_Extended.md) | **`IChokeAnalysisService`** vs extended `ChokeAnalysisService` methods |

Read **07** when defining tests, correlation coverage, or documentation; it consolidates oil-and-gas choke **best practices** (critical/subcritical, multiphase families, \(C_D\) bands, operational contexts) with traceability to phases 0–6.

**Master tracker:** [../MASTER-TODO-TRACKER.md](../MASTER-TODO-TRACKER.md) — phase rollup and prioritized next actions for this module.

## Default verification

```bash
dotnet build Beep.OilandGas.ChokeAnalysis/Beep.OilandGas.ChokeAnalysis.csproj
dotnet build Beep.OilandGas.ApiService/Beep.OilandGas.ApiService.csproj
```

When changing HTTP contracts or calculation outputs touched by tests:

```bash
dotnet test Beep.OilandGas.ApiService.Tests --filter "FullyQualifiedName~Choke" --no-build
```

(Adjust filter if choke-specific tests are named differently.)
