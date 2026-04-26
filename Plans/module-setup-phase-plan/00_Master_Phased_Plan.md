# ModuleSetupBase Architecture Revision - Master Phased Plan

## Why DevelopmentModule is currently in DrillingAndConstruction
- It is a legacy placement from earlier implementation where DEVELOPMENT was treated as execution (well/work-order/facility operations).
- DevelopmentPlanning currently owns planning behavior but does not yet expose its own ModuleSetupBase.
- Result: naming says "Development" while actual ownership is drilling/construction execution, which causes confusion.
- Temporary rule until refactor is complete: treat this as an execution module, not planning ownership.

## Target rule for ModuleSetupBase
- ModuleSetupBase in feature/process projects must only register project-owned schema classes.
- Feature ModuleSetupBase must not register PPDM39 table classes from Beep.OilandGas.PPDM39.Models.
- Shared PPDM reference and core PPDM seed infrastructure remains in Beep.OilandGas.PPDM39.DataManagement modules only.
- SeedAsync in feature modules should seed only project-owned reference/workflow bootstrap data.

## What ModuleSetupBase should contain
- Module metadata only: ModuleId, ModuleName, Order.
- EntityTypes: only local project table classes (prefer Data/Tables or project model assembly).
- SeedAsync: idempotent project-local seeding, no cross-domain PPDM table ownership.
- Explicit skip reason when no seed data exists.

## What ModuleSetupBase should not contain
- PPDM39 canonical domain table types owned by shared schema package.
- Cross-project table ownership.
- Non-idempotent seeds.
- Business workflows that belong in runtime services rather than setup.

## Phase Plan

### Phase 1 - Ownership Matrix and Naming
- Define module ownership per process project.
- Split Development into two concepts with explicit IDs:
  - DevelopmentPlanning: planning schema/setup (target ModuleId: DEVELOPMENT_PLANNING).
  - DrillingAndConstruction: execution schema/setup (target ModuleId: DRILLING_EXECUTION).
- Approve naming convention for modules: ExplorationModule, DevelopmentPlanningModule, DrillingExecutionModule, ProductionModule, HseModule, EconomicsModule, DecommissioningModule, PermitsModule.

### Phase 2 - Data Class Classification Audit
- For each process project classify classes into:
  - Data/Tables (persisted schema classes).
  - Data/Contracts (request/command DTOs).
  - Data/Projections (responses, analytics, view models).
- Enforce table-class shape rules (no collections in persisted table classes).

### Phase 3 - ModuleSetupBase Refactor
- Update each feature module EntityTypes to local table classes only.
- Remove PPDM39.Models table registrations from feature modules.
- Keep shared PPDM reference modules in PPDM39.DataManagement unchanged unless explicitly re-owned.

### Phase 4 - Seed Boundary Refactor
- Move feature-domain bootstrap seeding into feature modules.
- Keep shared enum/reference PPDM seeders in PPDM39.DataManagement.
- Ensure all seeds are idempotent and safe to re-run.

### Phase 5 - Integration and Verification
- Build each process project independently.
- Run setup workflow tests from ApiService.Tests that touch module orchestration.
- Validate no schema registration leaks across projects.

### Phase 6 - Rollout and Governance
- Add per-project ownership documents.
- Add PR checklist item: ModuleSetupBase must not include PPDM39.Models table types in feature projects.
- Add static scan check (rg/CI) for forbidden patterns in feature module files.

## Tracker
- [x] Phase 1 complete
- [x] Phase 2 complete
- [x] Phase 3 complete
- [x] Phase 4 complete
- [x] Phase 5 complete — **2025-04-25**: 0 Error(s) 0 Warning(s) full solution build. 16/16 `ModuleSetupOrchestratorTests` pass (order, dedup, fault isolation, abort, cancel, idempotency, selective-run, counters).
- [x] Phase 6 complete — **2025-04-25**: `Scripts/ci-guard-module-ownership.ps1` scans 71 module files — **0 violations** [PASS]. `.github/workflows/ci.yml` wires build → ApiService.Tests → guard as mandatory PR checks.

## Project-by-project table ownership audit (2025-04-25)

Reference: [10_Project_By_Project_Table_Audit.md](10_Project_By_Project_Table_Audit.md)

Summary:
- Completed module coverage for active process projects that currently own persisted table classes.
- Explicitly documented projects that currently have no local persisted table classes and should keep EntityTypes empty.
- Identified next-wave projects that currently should not receive ModuleSetupBase yet (no owned tables).

All blockers resolved:
- ApiService compile errors fixed (RunTicketController BSW columns, ProductionOperationsController ROW_ID, Program.cs ProductionAccountingModuleSetup class name, WellLogLayer.cs created, DrawingSampleGallery WellLogPetrophysical scene added).
- Full solution: **0 Error(s)  0 Warning(s)** as of 2025-04-25.
- Setup orchestration tests: **16/16 PASS** (`Beep.OilandGas.ApiService.Tests/ModuleSetupOrchestratorTests.cs`).
- CI module ownership guard: **[PASS] 0 violations** across 71 feature module files (`Scripts/ci-guard-module-ownership.ps1`).
