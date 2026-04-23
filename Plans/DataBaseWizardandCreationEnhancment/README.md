# PPDM39 Setup And Schema Creation Enhancement Plan

## Purpose

This plan upgrades PPDM39 setup so schema creation and updates are governed by MigrationManager, while removing duplicated ownership between `Beep.OilandGas.PPDM39` and `Beep.OilandGas.PPDM39.DataManagement`.

Use this folder as the canonical implementation plan for:
- setup API cleanup (`/api/ppdm39/setup/*`)
- wizard consolidation
- migration-first schema lifecycle
- demo and seed lifecycle integration
- rollout governance and CI evidence

Primary tracker:
- [MASTER TODO TRACKER](./MASTER-TODO-TRACKER.md)

## Code Review Findings Incorporated

- Contracts and DTO ownership is split across `PPDM39` and `DataManagement`, creating drift risk.
- `PPDM39SetupService` uses MigrationManager, but other setup/demo paths still reflect script-era assumptions.
- `DemoDatabaseService` still calls script-oriented setup methods.
- DataManagement has repeated type resolution and repository construction patterns that should be centralized.
- Runtime paths include blocking sync-over-async calls and a metadata refresh design that is not fully resettable.

## Architecture Anchors

1. MigrationManager is the only schema create/update authority.
2. Connection and environment setup follows `ConnectionProperties`, `ConnectionHelper`, `EnvironmentService`, `ConfigEditor`, and `IDataSource` capability rules.
3. Shared setup contracts live in `Beep.OilandGas.Models/Data/PPDM39Setup`.
4. Wizard and compatibility routes call the same orchestration services.
5. Demo/seed/localdb operations are setup stages, not separate schema paths.

## Phase Authority Matrix

- Phase 01 owns route/service boundary and contract placement.
- Phase 02 owns connection provisioning, environment policy, and secure logging.
- Phase 03 owns plan/policy/preflight/approve/execute/resume schema orchestration.
- Phase 04 owns canonical wizard route and UX state model.
- Phase 05 owns demo/seed/localdb stage behavior and cleanup rules.
- Phase 06 owns CI gates, artifact governance, and rollout operations.

## Phase Index

- [Phase 01 - Setup Surface And Contracts](./Phase-01-Setup-Surface-and-Contracts.md)
- [Phase 02 - Connection And Environment Foundation](./Phase-02-Connection-And-Environment-Foundation.md)
- [Phase 03 - Migration Manager Schema Creation](./Phase-03-Migration-Manager-Schema-Creation.md)
- [Phase 04 - Wizard Orchestration And UX](./Phase-04-Wizard-Orchestration-and-UX.md)
- [Phase 05 - Demo, Seed, And LocalDB Lifecycle](./Phase-05-Demo-Seed-and-LocalDB-Lifecycle.md)
- [Phase 06 - Validation, Governance, And Rollout](./Phase-06-Validation-Governance-And-Rollout.md)

## Expected End State

- `PPDM39SetupController` is transport-only and delegates orchestration.
- Schema operations no longer jump from plan to execute without policy and approval artifacts.
- PPDM39 setup, demo, and seed run through one governed pipeline.
- Compatibility routes exist only as adapters and have retirement milestones.
- CI and operations can validate plan hash, approval provenance, and execution checkpoints.