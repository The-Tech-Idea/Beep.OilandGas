# Beep.OilandGas.CompressorAnalysis — phased plans

Module-local execution plans for **CompressorAnalysis**: centrifugal/reciprocating power and head math, **`COMPRESSOR_*`** extension tables in **`CompressorAnalysis.Data`**, API routes, **`ICalculationService`** integration, and **`IModuleSetup`** registration.

## Non-negotiable solution standards

Apply on every phase (see [CLAUDE.md](../../CLAUDE.md) and [.cursor/commands](../../.cursor/commands/) for full text):

- **Three layers**: Web → API → Data (`IDMEEditor`, `PPDMGenericRepository`); this library is **calculation + service**; **table** entities are **`Beep.OilandGas.CompressorAnalysis.Data`**; **shared wire** DTOs are **`Beep.OilandGas.Models.Data.Calculations`**.
- **Interfaces**: **`ICompressorAnalysisService`** lives in **`Beep.OilandGas.CompressorAnalysis.Core.Interfaces`** (signatures use feature-local **`COMPRESSOR_*`** types). Other cross-cutting contracts remain in **`Beep.OilandGas.Models.Core.Interfaces`**.
- **Shared models**: `CompressorAnalysisRequest` / `CompressorAnalysisResult` / **`CompressorAnalysisWellKnown`** in **`Beep.OilandGas.Models/Data/Calculations/`** — no `DTO` namespaces.
- **Table vs projection**: types passed to **`InsertAsync`/`UpdateAsync`** must be **scalar table shapes** extending **`ModelEntityBase`**. Rich design/performance POCOs returned only by **`CompressorAnalysisService`** must remain projections unless persisted — verify **`CENTRIFUGAL_COMPRESSOR_PROPERTIES`** in **`Beep.OilandGas.CompressorAnalysis.Data`** stays scalar-only (no nested object graphs on table classes).
- **Extension schema**: entity classes + **`ModuleSetupBase.EntityTypes`** + migration/tooling — **not** new hand-written **`Beep.OilandGas.Models/Scripts/**` DDL per [CLAUDE.md](../../CLAUDE.md).
- **DI**: register services in **`Beep.OilandGas.ApiService/Program.cs`** with factory **`AddScoped<IFoo>(sp => new Foo(...))`**; read **`Program.cs`** ~1–120 when touching registration order.
- **`PPDM39.DataManagement`** does **not** reference CompressorAnalysis.

## Phase documents

| Phase | File | Focus |
|-------|------|--------|
| 0 | [00_CompressorAnalysis_Overview_And_Baseline.md](00_CompressorAnalysis_Overview_And_Baseline.md) | Scope, inventory, gaps |
| 1 | [01_Phase_Service_Contracts_And_Models.md](01_Phase_Service_Contracts_And_Models.md) | Done — **`CompressorAnalysis.Core.Interfaces`**, **`CompressorAnalysis.Data`** tables, **`Models.Data.Calculations`** wire types |
| 2 | [02_Phase_Calculation_And_Validation.md](02_Phase_Calculation_And_Validation.md) | Calculators, units, validation, tech debt |
| 3 | [03_Phase_PPDM_ModuleSetup_And_Data.md](03_Phase_PPDM_ModuleSetup_And_Data.md) | **`CompressorAnalysisModule`**, seed, **`FACILITY`/`EQUIPMENT`** story |
| 4 | [04_Phase_API_And_Orchestration.md](04_Phase_API_And_Orchestration.md) | **`CompressorController`**, **`PerformCompressorAnalysisAsync`**, client |
| 5 | [05_Phase_Tests_And_Verification.md](05_Phase_Tests_And_Verification.md) | Unit/API tests, regression gates |
| 6 | [06_Phase_Packaging_Docs_And_Backlog.md](06_Phase_Packaging_Docs_And_Backlog.md) | README, nupkg, SkiaSharp backlog |
| — | [07_Scenarios_Best_Practices_And_Reference.md](07_Scenarios_Best_Practices_And_Reference.md) | Field scenarios, units, machinery references |
| — | [08_Consolidated_Execution_Checklist.md](08_Consolidated_Execution_Checklist.md) | Single runbook |

**Master tracker:** [../MASTER-TODO-TRACKER.md](../MASTER-TODO-TRACKER.md)

## Default verification

```bash
dotnet build Beep.OilandGas.CompressorAnalysis/Beep.OilandGas.CompressorAnalysis.csproj
dotnet build Beep.OilandGas.ApiService/Beep.OilandGas.ApiService.csproj
```
