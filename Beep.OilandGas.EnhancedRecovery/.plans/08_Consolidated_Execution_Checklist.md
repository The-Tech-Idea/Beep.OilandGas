# Consolidated execution checklist — EnhancedRecovery

Use before merging any enhancement PR.

## Architecture & layering

- [ ] **`CLAUDE.md`** three-layer rules respected; **no** `DTO` namespaces
- [ ] **`PPDM39.DataManagement`** does **not** reference **`Beep.OilandGas.EnhancedRecovery`**
- [ ] **`IEnhancedRecoveryService`** (**`Models.Core.Interfaces`**) remains the **public API contract** unless versioned bump + migration notes
- [ ] Well mutations use **`WellServices`** when touching **`WELL`/`WELL_STATUS`/`WELL_XREF`**

## Data & persistence

- [ ] **`PDEN`** / **`PDEN_FLOW_MEASUREMENT`** writes preserve **audit** columns consistently (**`ROW_CREATED_BY`** / **`SYSTEM`** vs authenticated user policy)
- [ ] **`ACTIVE_IND`** semantics documented for soft-delete vs historical measurements
- [ ] New tables (if any): **`ModelEntityBase`**, **`ModuleSetup.EntityTypes`**, **no** hand-written per-feature **`Models/Scripts`** DDL

## Calculations & API

- [ ] Units documented for any **new** endpoint
- [ ] Screening outputs labeled **non-simulation** where applicable
- [ ] **`OperationCanceledException`** propagates from long-running analysis paths
- [ ] Swagger / Web client routes **aligned** (**`/api/EnhancedRecovery`** vs **`/api/field/current/...`**)

## Quality gates

- [ ] `dotnet build Beep.OilandGas.EnhancedRecovery`
- [ ] `dotnet build Beep.OilandGas.ApiService`
- [ ] `dotnet test` on **`EnhancedRecovery.Tests`** (once created) + targeted **`ApiService.Tests`**

## Documentation

- [ ] **`README.md`** updated if behavior or assumptions change
- [ ] **[07_EOR_Best_Practices_And_Scenario_Matrix.md](07_EOR_Best_Practices_And_Scenario_Matrix.md)** scenario table updated if scope shifts
