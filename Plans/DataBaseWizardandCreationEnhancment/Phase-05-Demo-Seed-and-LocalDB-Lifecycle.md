# Phase 05 - Demo, Seed, And LocalDB Lifecycle

## Objective

Bring demo database creation, dummy data generation, reference data seeding, and local file lifecycle under the same setup foundation as canonical connection and schema creation.

## Current Gaps

- `DemoDatabaseService` still creates schema by running all scripts rather than using the migration pipeline.
- dummy-data generation is an endpoint-specific flow rather than a first-class setup capability.
- seed operations are idempotent, but they are still modeled as a separate concern instead of a governed stage in the setup workflow.
- local file creation, delete, copy, and recreate are handled across several paths rather than one local database lifecycle policy.

## Best-Practice Drivers

- `migration` skill: demo and non-demo schema creation should share the migration lifecycle.
- `localdb` skill: close file handles before copy, delete, or recreate operations.
- `environmentservice` skill: keep demo storage and cleanup in standard app folders.
- `defaults` skill: if setup operation metadata or seeded setup entities are persisted, default audit and identity values should come from the defaults pipeline rather than duplicated helper logic.

## Implementation Scope

### 1. Move demo schema creation onto migration orchestration

Replace script-list driven schema creation in `DemoDatabaseService` with the same plan-first migration flow used by setup.

### 2. Model seed stages as setup capabilities

Treat these as typed stages in the setup orchestration:

- reference data seed
- well-status facet seed
- optional enum or category seed
- dummy-data generation

### 3. Define local DB lifecycle rules

For local databases, enforce:

- environment-aware folder placement
- explicit file extension handling
- close-before-copy and close-before-delete rules
- cleanup and retention semantics for demo databases
- safe rollback or cleanup after failed setup

### 4. Make demo data status a reusable service capability

Expose dummy-data and demo-database status through shared contracts so the setup and admin pages can reason about whether seeded demo content already exists.

## Deliverables

- demo database creation moved to migration orchestrator
- typed seed and dummy-data service contracts
- local DB lifecycle policy and cleanup rules
- cleanup and retention documentation for demo stores

## Validation And Exit Criteria

- demo databases are created with the same schema creation foundation as normal setup
- failed demo setup cleans up partial local artifacts safely
- seed and dummy-data stages can be rerun idempotently or reported as already complete
- cleanup service works against tracked demo metadata and closed file handles

## Dependencies

- `DemoDatabaseService`
- `DemoDatabaseCleanupService`
- seed services and facet seeder
- migration orchestrator from Phase 03

## Target Files

- `Beep.OilandGas.PPDM39.DataManagement/Services/DemoDatabaseService.cs`
- `Beep.OilandGas.PPDM39.DataManagement/Services/DemoDatabaseCleanupService.cs`
- `Beep.OilandGas.PPDM39.DataManagement/SeedData/PPDMReferenceDataSeeder.cs`
- `Beep.OilandGas.PPDM39.DataManagement/SeedData/PPDMDemoDataSeeder.cs`
- `Beep.OilandGas.PPDM39.DataManagement/SeedData/WellStatusFacetSeeder.cs`

## Execution Checklist

- [x] Remove script-list schema creation from demo flow; call migration orchestration.
- [x] Define seed stage catalog with required vs optional stages.
- [x] Require schema-complete state before any seed or dummy stage.
- [x] Enforce localdb close-before-copy/delete/recreate rules.
- [x] Add retention and cleanup policies for failed/cancelled demo operations.
- [x] Publish typed status contract for demo and dummy-data completion state.

## Seed Stage Catalog (Required Baseline)

- [x] Reference data seed
- [x] Well status facet seed
- [x] Required LOV seed packs
- [x] Optional demo/dummy datasets only after required stages pass

## Acceptance Criteria

- Demo and non-demo schema creation use the same migration orchestration.
- Seed stages are resumable and idempotent with clear completion markers.
- Local file lifecycle operations are safe and policy-driven.
- Cleanup can remove partial demo artifacts without breaking active connections.