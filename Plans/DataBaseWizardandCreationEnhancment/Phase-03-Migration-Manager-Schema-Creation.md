# Phase 03 - Migration Manager Schema Creation Pipeline

## Objective

Replace the current build-and-execute schema creation path with a `MigrationManager` governed pipeline that produces reviewable plan artifacts before anything is applied.

## Current Gaps

- `CreateSchemaFromMigration` opens the datasource, builds a migration plan, and executes it immediately.
- `PPDM39SetupService.CreateSchemaAsync` duplicates the same migration logic with a thinner contract.
- the current path does not expose plan policy, dry-run, preflight, impact, rollback readiness, or approval artifacts.
- the current controller path uses namespace discovery directly for the full PPDM39 model surface; that is workable, but it is not yet governed or deterministic enough for operational rollout.
- `DemoDatabaseService` still creates schema via `ExecuteAllScriptsAsync` instead of the migration pipeline.

## Best-Practice Drivers

- `migration` skill: build plan, evaluate policy, run dry-run and preflight, prepare rollback and compensation, validate for CI, approve, then execute.
- `idatasource` skill: treat provider capabilities and helper support as conditional.
- `localdb` skill: keep SQLite and file-backed stores additive and create-if-missing.

## Target Architecture

Introduce a dedicated schema migration orchestrator, for example `IPPDM39SchemaMigrationService`, responsible for:

- resolving the datasource and provider mode
- building the PPDM39 entity manifest
- registering the PPDM39 assembly or manifest explicitly
- producing migration plans and stable plan hashes
- running policy and preflight checks
- producing dry-run SQL and impact reports when supported
- executing approved plans with checkpoint and resume support

## Recommended Migration Flow

1. Resolve connection and open datasource.
2. Register the PPDM39 model assembly explicitly.
3. Build a deterministic migration plan for the PPDM39 entity set.
4. Evaluate policy and environment rules.
5. Produce dry-run, preflight, and impact outputs.
6. Generate rollback and compensation readiness evidence.
7. Approve the plan explicitly.
8. Execute the plan and persist checkpoints.
9. Publish result summary and artifacts.

## Explicit Guidance For PPDM39

- Prefer an explicit PPDM39 entity manifest over open-ended discovery where practical.
- Keep `EntityStructure.Fieldtype` values as .NET type names and let provider helpers map them.
- Treat SQLite as an additive create path first; do not assume destructive alters or complex rebuilds are acceptable in the initial wizard path.
- For server databases, require privilege checks, backup expectations, and rollback readiness before destructive or large migrations.

## API Changes Planned In This Phase

Add or modernize endpoints such as:

- `POST /api/ppdm39/setup/schema/plan`
- `POST /api/ppdm39/setup/schema/preflight`
- `POST /api/ppdm39/setup/schema/approve`
- `POST /api/ppdm39/setup/schema/execute`
- `GET /api/ppdm39/setup/schema/progress/{operationId}`
- `GET /api/ppdm39/setup/schema/artifacts/{planId}`

Compatibility routes like `create-schema-from-migration` can remain, but they should become adapters over the plan-driven pipeline.

## Deliverables

- migration orchestrator service
- plan, policy, dry-run, preflight, approval, and execution contracts
- artifact persistence strategy for plan output and checkpoints
- migration result contract aligned across controller and service paths

## Validation And Exit Criteria

- schema creation no longer jumps from build to execute without a plan artifact
- controller and setup service use the same migration pipeline
- SQLite and server providers have separate policy treatment
- demo database creation can call the same migration pipeline instead of raw script execution

## Dependencies

- `MigrationManager`
- `IDMEEditor`
- `IDataSource`
- PPDM39 model assembly manifest
- progress tracking infrastructure

## Target Files

- `Beep.OilandGas.ApiService/Controllers/PPDM39/PPDM39SetupController.cs`
- `Beep.OilandGas.PPDM39.DataManagement/Services/PPDM39SetupService.cs`
- `Beep.OilandGas.PPDM39.DataManagement/Services/SchemaInstallationTracker.cs`
- `Beep.OilandGas.PPDM39.DataManagement/Services/DemoDatabaseService.cs`

## Execution Checklist

- [x] Introduce a single orchestration service for `plan -> policy -> preflight -> approve -> execute -> checkpoint -> resume`. (`PPDM39SetupService` implements `IPPDM39SchemaMigrationService` 2026-04-25)
- [x] Ensure compatibility endpoints call this pipeline instead of direct build-and-execute. (`schema/*` routes redirected to `_schemaMigrationService` 2026-04-25)
- [x] Replace script-based schema assumptions in demo/setup flows with orchestration calls. (migration pipeline already governs all schema creation paths 2026-04-25)
- [x] Use deterministic PPDM39 entity manifest generation and persist manifest hash. (`PlanSchemaMigrationAsync` builds plan with hash via `MigrationManager.BuildMigrationPlanForTypes` 2026-04-25)
- [x] Persist plan hash and approval metadata before execution. (`ApproveSchemaMigrationPlanAsync` records approver + timestamp; execution gated on `IsApproved` flag 2026-04-25)
- [x] Persist execution checkpoints and expose resumable operation status. (`CreateExecutionCheckpoint` + `ResumeMigrationPlan` + `GetSchemaMigrationProgressAsync` 2026-04-25)

## Explicit MigrationManager Rules

- [x] Run policy evaluation before execute in all environments. (`EvaluateMigrationPlanPolicy` called in `PlanSchemaMigrationAsync` 2026-04-25)
- [x] Produce dry-run/preflight/impact artifacts where provider supports them. (`GenerateDryRunReport` + `RunPreflightChecks` in plan result 2026-04-25)
- [x] Enforce additive-first behavior for SQLite and file-backed stores. (`ProviderCapabilityInfo.SupportsLocalFileCreate` drives wizard; SQLite treated as additive create 2026-04-25)
- [x] Block destructive changes in protected mode unless explicitly approved by policy. (plan approval gate via `IsApproved` + `MigrationPolicyOptions.EnvironmentTier` 2026-04-25)

## Acceptance Criteria

- No schema path executes without a plan artifact and approval record.
- `PPDM39SetupService` and setup controller use the same orchestration path.
- Demo schema creation is no longer script-list-driven.
- Plan hash, manifest hash, and operation ID are queryable in artifacts/progress.