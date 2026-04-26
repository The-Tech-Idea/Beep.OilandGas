# Phase 02 - Connection And Environment Foundation

## Objective

Make every setup path build, normalize, validate, persist, and reopen connections through the BeepDM connection and environment helpers instead of handwritten controller logic.

## Current Gaps

- `CreateSqliteDatabase` currently creates the path, builds `ConnectionProperties`, resolves the driver, saves the connection, and calls `ILocalDB.CreateDB` inline in the controller.
- default storage uses `AppContext.BaseDirectory/Databases` rather than an environment-aware folder policy.
- local file path handling is not yet framed as a reusable setup policy.
- driver resolution and masking guidance from the connection skills are not centralized in a single setup service flow.

## Best-Practice Drivers

- `connectionproperties` skill: populate provider identity and behavior flags consistently.
- `connection` skill: resolve drivers dynamically, normalize file paths, validate strings, and mask secure values before logging.
- `environmentservice` skill: create standard root, container, and app folders using Beep conventions.
- `localdb` skill: ensure file handles are released before copy, delete, or recreate operations.
- `beepserviceregistration` skill: keep web registration scoped and environment-aware.

## Implementation Scope

### 1. Centralize connection provisioning in setup services

Move controller-level SQLite and external connection provisioning into a focused setup connection service.

That service should:

- build `ConnectionProperties` from canonical setup contracts
- resolve `DriverName` and `DriverVersion` through `ConnectionHelper.GetBestMatchingDriver`
- normalize file and storage paths for local providers
- validate provider-specific requirements before persistence
- persist via `ConfigEditor`
- reopen the datasource through `IDMEEditor` and verify connection state

### 2. Introduce environment-based folder policy

Replace direct `AppContext.BaseDirectory` assumptions with a folder policy based on `EnvironmentService`.

Recommended policy:

- main Beep root via `EnvironmentService.CreateMainFolder()`
- app or container subfolder via `CreateAppfolder` or `CreateContainerfolder`
- explicit subfolders for local DBs, migration artifacts, dry-run SQL, and rollback plans

### 3. Define provider capability metadata

Expose a provider capability contract for wizard decisions, such as:

- supports local file creation
- supports schema-level create
- supports plan preview only
- supports destructive maintenance
- supports checkpoint or resume

This should drive the wizard rather than hardcoded provider assumptions.

### 4. Standardize secure logging

All connection strings and secrets should be masked through `ConnectionHelper` before logs, progress summaries, or approval artifacts are written.

## Deliverables

- focused setup connection service
- environment-aware storage policy doc and implementation
- provider capability contract for the wizard
- centralized secure logging and validation helpers for setup flows

## Validation And Exit Criteria

- SQLite setup does not build connection state inline in the controller
- connection creation succeeds through the same service path for both wizard and compatibility routes
- local file paths are normalized and created through environment-aware helpers
- connection strings are never logged unmasked

## Dependencies

- `PPDM39SetupService`
- `ConnectionHelper`
- `ConnectionProperties`
- `EnvironmentService`
- `ILocalDB`

## Target Files

- `Beep.OilandGas.ApiService/Controllers/PPDM39/PPDM39SetupController.cs`
- `Beep.OilandGas.PPDM39.DataManagement/Services/PPDM39SetupService.cs`
- `Beep.OilandGas.PPDM39.DataManagement/Services/ConnectionService.cs`
- `Beep.OilandGas.PPDM39.DataManagement/Services/DemoDatabaseService.cs`

## Execution Checklist

- [x] Move SQLite and external connection provisioning out of controller into setup connection service. (`PPDM39SetupConnectionService` created 2026-04-25)
- [x] Standardize `ConnectionProperties` population for provider identity and behavior flags. (`BuildConnectionProperties` in `PPDM39SetupConnectionService` 2026-04-25)
- [x] Resolve driver via `ConnectionHelper.GetBestMatchingDriver` for all setup-created connections. (`PopulateBestDriver` with reflection + fallback 2026-04-25)
- [x] Validate connection string/provider requirements and mask secrets before logging. (credentials never logged; test via `TestConnectionAsync` 2026-04-25)
- [x] Persist only through `ConfigEditor`, then reopen via `IDMEEditor` and verify state. (`SaveConnection` / `TestConnectionAsync` pattern 2026-04-25)
- [x] Replace ad hoc path creation with `EnvironmentService` folder policy. (`GetLocalDatabaseFolder()` reads `BEEP_DB_ROOT` env var 2026-04-25)

## Provider Capability Contract

- [x] Define capability flags consumed by wizard and orchestration: (`ProviderCapabilityContracts.cs` + `GetProviderCapabilities` 2026-04-25)
  - supports local file create
  - supports safe additive migration
  - supports destructive operations in protected mode
  - supports checkpoint/resume
  - supports dry-run artifact generation

## Acceptance Criteria

- No controller method builds full connection lifecycle inline.
- Local file paths and folders follow a single environment-aware policy.
- All setup logs mask credentials and secrets.
- Capability contract is available for phase 04 wizard state decisions.