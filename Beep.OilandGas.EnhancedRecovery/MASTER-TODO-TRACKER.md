# MASTER-TODO-TRACKER — Beep.OilandGas.EnhancedRecovery

Single rollup for **planning and execution**. Detailed steps live under **[.plans/](.plans/README.md)**.

## Phase rollup

| Phase | Document | Status |
|-------|----------|--------|
| 0 | [.plans/00_EnhancedRecovery_Overview_And_Baseline.md](.plans/00_EnhancedRecovery_Overview_And_Baseline.md) | Doc baseline — inventory + gaps |
| 1 | [.plans/01_Phase_Contracts_PDEN_Reference_And_Data_Fidelity.md](.plans/01_Phase_Contracts_PDEN_Reference_And_Data_Fidelity.md) | Planned — rename feature **`IEnhancedRecoveryService`**, RF semantics, PDEN fidelity |
| 2 | [.plans/02_Phase_EOR_Scenarios_Calculations_And_Validation.md](.plans/02_Phase_EOR_Scenarios_Calculations_And_Validation.md) | Planned — scenario catalog, validation, de-magic screening math |
| 3 | [.plans/03_Phase_ModuleSetup_And_Optional_Extensions.md](.plans/03_Phase_ModuleSetup_And_Optional_Extensions.md) | Done — **`EnhancedRecoveryModule`** + **`R_ENHANCED_RECOVERY_REFERENCE_CODE`** seed |
| 4 | [.plans/04_Phase_API_Web_And_Lifecycle_Orchestration.md](.plans/04_Phase_API_Web_And_Lifecycle_Orchestration.md) | Planned — expose **Advanced** analytics; field routes; OCE |
| 5 | [.plans/05_Phase_Tests_And_Verification.md](.plans/05_Phase_Tests_And_Verification.md) | In progress — **`EnhancedRecovery.Tests`** (13) — LOV catalog, module contract, screening RF |
| 6 | [.plans/06_Phase_Docs_Packaging_And_Governance.md](.plans/06_Phase_Docs_Packaging_And_Governance.md) | Planned — README, pack metadata, assumptions |
| Ref | [.plans/07_EOR_Best_Practices_And_Scenario_Matrix.md](.plans/07_EOR_Best_Practices_And_Scenario_Matrix.md) | Reference — screening / surveillance matrix |
| Runbook | [.plans/08_Consolidated_Execution_Checklist.md](.plans/08_Consolidated_Execution_Checklist.md) | Active checklist |

Update the **Status** column when a phase’s exit criteria are met in the codebase.

## Repository standards (always)

- [CLAUDE.md](../../CLAUDE.md) — three-layer architecture; **table vs projection** rules; **WellServices** for well tables.
- **`Beep.OilandGas.Models.Core.Interfaces.IEnhancedRecoveryService`** — shared HTTP/LifeCycle contract.
- **`PDEN`** + **`PDEN_FLOW_MEASUREMENT`** — current persistence backbone for schemes and injection rates.

## Consolidated next actions (editable)

| Priority | Action | Phase |
|----------|--------|--------|
| Done | Rename feature interface → **`IEnhancedRecoveryOperationsService`** ( **`Models.Core.Interfaces.IEnhancedRecoveryService`** unchanged for DI ) | 1 |
| Done | **`CalculateRecoveryFactorAsync`** — screening RF % on **`Efficiency`** + **`Remarks`**; XML docs on Models interface | 1–2 |
| Done | ApiService DI injects **`ILogger<EnhancedRecoveryService>`**; two-arg ctor chains to **`NullLogger`** for tests/tools | 1 |
| P1 | Expose **Advanced** analysis via API + Web with explicit **screening** disclaimers | 4 |
| Done | **`EnhancedRecovery.Tests`** — LOV catalog (×3), module contract, screening RF theory (**13** total); economics vectors backlog | 5 |
| Done | **`README.md`** — full module overview, PPDM mapping, APIs, Web clients, limits, build (**[`README.md`](README.md)**); **`PackageReadmeFile`** in **`.csproj`** | 6 |
| Done | **`EnhancedRecoveryModule`** + **`R_ENHANCED_RECOVERY_REFERENCE_CODE`** + **`EnhancedRecoveryReferenceCodeSeed`** | 3 |

## Verification

```bash
dotnet build Beep.OilandGas.EnhancedRecovery/Beep.OilandGas.EnhancedRecovery.csproj
dotnet build Beep.OilandGas.ApiService/Beep.OilandGas.ApiService.csproj
```

```bash
dotnet test Beep.OilandGas.EnhancedRecovery.Tests/Beep.OilandGas.EnhancedRecovery.Tests.csproj
```
