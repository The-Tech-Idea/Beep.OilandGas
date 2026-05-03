# Beep.OilandGas.EnhancedRecovery — phased plans

Module-local execution plans for **Enhanced Oil Recovery (EOR)**: PDEN-centric operations and injection surveillance, **advanced screening / performance analytics** (`EnhancedRecoveryService.Advanced.cs`), economics (**`EOREconomicAnalysis`**), API/Web alignment, and optional **extension schema** where PPDM core tables are insufficient.

## Non-negotiable solution standards

Apply on every phase (see [CLAUDE.md](../../CLAUDE.md) and [.cursor/commands](../../.cursor/commands/) for full detail):

- **Three layers**: Web → API → Data (`IDMEEditor`, **`PPDMGenericRepository`** / **`UnitOfWork`** where used today); keep **shared** contracts in **`Beep.OilandGas.Models.Core.Interfaces`** (**`IEnhancedRecoveryService`** is already here).
- **Data shapes**: **Table** classes extend **`ModelEntityBase`** and stay **scalar-only**; **projections** (**`EnhancedRecoveryOperation`**, **`WaterfloodPerformanceAnalysis`**, etc.) live under **`Beep.OilandGas.Models.Data`** / **`Models.Data.Calculations`** — never pass rich graphs to **`InsertAsync`** / **`UpdateAsync`**.
- **PPDM alignment**: Today **`EnhancedRecoveryService`** maps EOR/injection to **`PDEN`** + **`PDEN_FLOW_MEASUREMENT`**. Do **not** invent parallel business tables without a **ModuleSetup** / metadata story; prefer extending **reference codes** and **filters** before new DDL.
- **Wells**: Any change that creates or mutates **`WELL`** / **`WELL_STATUS`** / **`WELL_XREF`** must go through **`WellServices`** (see **CLAUDE.md**).
- **DI**: Register in **`Beep.OilandGas.ApiService/Program.cs`** with factory **`AddScoped<…>(sp => new …)`**; read **`Program.cs`** early lines when changing registration order (**`AddBeepServices`** before consumers of **`IDMEEditor`**).
- **`PPDM39.DataManagement`** must **not** take a project reference **to** **`Beep.OilandGas.EnhancedRecovery`** (domain stays out of infrastructure).

## Phase documents

| Phase | File | Focus |
|-------|------|--------|
| 0 | [00_EnhancedRecovery_Overview_And_Baseline.md](00_EnhancedRecovery_Overview_And_Baseline.md) | Inventory, PDEN mapping, gaps vs API surface |
| 1 | [01_Phase_Contracts_PDEN_Reference_And_Data_Fidelity.md](01_Phase_Contracts_PDEN_Reference_And_Data_Fidelity.md) | **`IEnhancedRecoveryService`**, PDEN/flow conventions, ID & LOV alignment |
| 2 | [02_Phase_EOR_Scenarios_Calculations_And_Validation.md](02_Phase_EOR_Scenarios_Calculations_And_Validation.md) | Water/gas/chemical/thermal scenarios, units, validation, replace placeholders |
| 3 | [03_Phase_ModuleSetup_And_Optional_Extensions.md](03_Phase_ModuleSetup_And_Optional_Extensions.md) | **`IModuleSetup`**, seeds, when to add extension tables |
| 4 | [04_Phase_API_Web_And_Lifecycle_Orchestration.md](04_Phase_API_Web_And_Lifecycle_Orchestration.md) | Controllers, field routes, **`PPDMProductionService`**, clients |
| 5 | [05_Phase_Tests_And_Verification.md](05_Phase_Tests_And_Verification.md) | Unit/API tests, regression vectors, surveillance KPIs |
| 6 | [06_Phase_Docs_Packaging_And_Governance.md](06_Phase_Docs_Packaging_And_Governance.md) | README, economics assumptions, package metadata |
| — | [07_EOR_Best_Practices_And_Scenario_Matrix.md](07_EOR_Best_Practices_And_Scenario_Matrix.md) | Industry practices (screening, pilots, surveillance) |
| — | [08_Consolidated_Execution_Checklist.md](08_Consolidated_Execution_Checklist.md) | Single runbook before merge |

**Master tracker:** [../MASTER-TODO-TRACKER.md](../MASTER-TODO-TRACKER.md)

## Default verification

```bash
dotnet build Beep.OilandGas.EnhancedRecovery/Beep.OilandGas.EnhancedRecovery.csproj
dotnet build Beep.OilandGas.ApiService/Beep.OilandGas.ApiService.csproj
```
