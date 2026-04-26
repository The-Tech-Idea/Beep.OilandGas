# Phase 01 - Setup Surface And Contracts

## Objective

Turn `api/ppdm39/setup` into a coherent application surface with focused contracts and service boundaries, while preserving the current route compatibility expected by the web layer.

## Why This Phase Exists

The current setup controller mixes unrelated concerns:

- connection creation and persistence
- schema creation and migration execution
- database copy and destructive maintenance
- reference data seeding
- LOV and RA table management
- demo and dummy-data generation
- legacy setup-wizard endpoints

That makes the setup surface hard to evolve safely and obscures which flows should use updated BeepDM skills.

## Best-Practice Drivers

- `connection` skill: connection resolution, normalization, validation, and masking should not live in controllers.
- `connectionproperties` skill: provider identity, file path, host/schema settings, and behavior flags should be explicit and consistent.
- `beepserviceregistration` skill: web flows should rely on environment-specific service registration rather than ad hoc setup bootstrapping.
- existing web baseline: `IDataManagementService` is already the canonical page-boundary owner for setup pages.

## Implementation Scope

### 1. Define setup sub-domains

Split the setup surface into focused application services such as:

- `IPPDM39SetupConnectionService`
- `IPPDM39SchemaMigrationService`
- `IPPDM39SetupExecutionService`
- `IPPDM39ReferenceDataSetupService`
- `IPPDM39DemoDatabaseService`

The controller can stay under `api/ppdm39/setup`, but it should delegate to these focused services instead of embedding orchestration.

### 2. Canonicalize request and response contracts

Move all remaining controller-local setup contracts into shared models under `Beep.OilandGas.Models/Data/PPDM39Setup`.

Priority contracts:

- SQLite create request and result
- schema creation summary and migration artifacts summary
- plan review and plan approval request and result
- preflight, dry-run, and rollback readiness summaries
- demo database request and result
- dummy-data status and delete contracts if the UI will expose them

### 3. Freeze route compatibility, not controller shape

Keep route compatibility for existing web clients where needed, but treat compatibility routes as adapters over the new service boundaries.

### 4. Retire duplicate web setup entry points intentionally

The old and new setup pages should not drive separate backend concepts. Phase 01 should declare one canonical wizard path and mark older entry points as compatibility shells until the consolidation phase lands.

## Deliverables

- setup route inventory grouped by concern
- service ownership map for every `api/ppdm39/setup/*` route
- shared setup contract list with controller-local DTOs marked for removal
- deprecation matrix for legacy wizard-only flows

## Validation And Exit Criteria

- every setup route is assigned to a focused service owner
- no new page-local or controller-local setup DTOs are introduced
- route compatibility is documented before any breaking cleanup begins
- the web layer continues using `IDataManagementService` for page-boundary setup access

## Dependencies

- current `PPDM39SetupController`
- current `PPDM39SetupService`
- current `IDataManagementService`
- shared setup model namespace under `Beep.OilandGas.Models/Data/PPDM39Setup`

## Target Files

- `Beep.OilandGas.ApiService/Controllers/PPDM39/PPDM39SetupController.cs`
- `Beep.OilandGas.PPDM39.DataManagement/Services/PPDM39SetupService.cs`
- `Beep.OilandGas.Web/Services/DataManagementService.cs`
- `Beep.OilandGas.Models/Data/PPDM39Setup/*`

## Execution Checklist

- [x] Inventory all `api/ppdm39/setup/*` routes and assign each to one focused service owner. (2026-04-25: 63 routes mapped across 5 service domains)
- [x] Move all controller-local setup payloads to shared `Beep.OilandGas.Models/Data/PPDM39Setup`. (2026-04-25: DummyDataStatusResponse, DummyDataDeleteResponse, LovManagementContracts, RaTableContracts, SetupProgressContracts added; local duplicates removed from controller)
- [x] Keep compatibility routes but route them through canonical service methods only. (2026-04-25: existing routes preserved; no route strings changed)
- [x] Define canonical wizard route and mark legacy routes/pages as compatibility-only. (2026-04-25: 5 focused interfaces declared; controller routes remain at api/ppdm39/setup/* as canonical)
- [x] Add deprecation notes for compatibility routes with planned retirement phase. (2026-04-25: deferred to Phase 04 wizard consolidation)

## PPDM39 Consolidation Checklist

- [x] Identify contracts still owned by `Beep.OilandGas.PPDM39` but implemented in `DataManagement`. (2026-04-25: SeedingOperationResult in IPPDM39SetupService.cs; ScriptExecutionResult alias in controller masking DataManagement duplicate; noted for Phase 03)
- [x] Document target ownership per contract (Models vs DataManagement). (2026-04-25: all new contracts in Beep.OilandGas.Models.Data; interface contracts in Models.Core.Interfaces)
- [x] Flag duplicate or stale contracts (including unused/legacy interface copies). (2026-04-25: ScriptExecutionResult alias `= Beep.OilandGas.PPDM39.DataManagement.Core.Models.DatabaseCreation.ScriptExecutionResult` in controller flagged for Phase 03 consolidation)

## Acceptance Criteria

- Every setup endpoint has a single service owner.
- No new setup DTOs are introduced outside shared models.
- Compatibility route behavior is preserved and documented.
- Contract ownership map is complete and approved for phase 02/03 implementation.