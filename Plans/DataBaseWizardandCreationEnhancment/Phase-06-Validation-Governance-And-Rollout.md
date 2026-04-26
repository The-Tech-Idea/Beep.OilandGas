# Phase 06 - Validation, Governance, And Rollout

## Objective

Add the operational safety, CI evidence, and rollout controls needed to treat setup and schema creation as a production-grade subsystem rather than a developer-only bootstrap tool.

## Best-Practice Drivers

- `migration` skill: use CI validation, policy decisions, rollout governance, compensation plans, and hard-stop thresholds.
- `connection` skill: log masked connection information only.
- `beepserviceregistration` skill: validate environment-specific registration assumptions in web and Blazor hosts.

## Implementation Scope

### 1. Test strategy

Create a layered test set covering:

- connection normalization and driver resolution
- environment folder policy
- SQLite create and reopen flow
- migration plan generation for PPDM39 entity sets
- preflight and policy evaluation behavior
- execution checkpoint and resume behavior
- seed and dummy-data idempotency
- demo database cleanup and retention

### 2. Migration artifacts and approval evidence

Persist and expose:

- plan hashes
- dry-run SQL or provider-specific summaries
- impact summaries
- preflight and policy results
- approval metadata
- execution logs and checkpoint summaries

### 3. Observability

Add structured logs and progress payloads for:

- operation ID
- connection name and provider type
- masked connection information
- plan ID and plan hash
- migration phase and current step
- warnings, blocked policy decisions, and rollback readiness

### 4. Rollout strategy

Adopt staged rollout gates:

- development and local SQLite validation
- integration environment with server RDBMS targets
- protected environment dry-run only
- production approval and promoted execution

### 5. Operational runbooks

Document:

- how to create or repair a connection
- how to inspect a blocked migration plan
- how to resume a failed schema execution
- how to clean up demo databases safely
- how to review setup artifacts during support incidents

## Deliverables

- automated validation suite for setup and schema creation
- migration artifact retention model
- rollout checklist and support runbook
- provider-specific readiness matrix for SQLite and server engines

## Validation And Exit Criteria

- CI can reject unsafe schema changes before merge
- setup operations produce auditable artifacts and masked logs
- rollout rules differ correctly for local, non-production, and protected environments
- support documentation exists for resume, rollback, and cleanup scenarios

## Dependencies

- migration pipeline from Phase 03
- wizard orchestration from Phase 04
- demo and seed lifecycle work from Phase 05

## Target Files

- `Beep.OilandGas.ApiService/Controllers/PPDM39/PPDM39SetupController.cs`
- `Beep.OilandGas.PPDM39.DataManagement/Services/PPDM39SetupService.cs`
- `Beep.OilandGas.PPDM39.DataManagement/Services/SchemaInstallationTracker.cs`
- `Beep.OilandGas.Web/Services/DataManagementService.cs`

## Execution Checklist

- [x] Add CI checks for migration plan validity and blocked-policy detection. (COMPLETED 2026-04-25)
- [x] Persist and validate manifest hash and plan hash for every execution. (COMPLETED 2026-04-25)
- [x] Require approval provenance before execute in protected environments. (COMPLETED 2026-04-25)
- [x] Enforce masked logging for connection/security fields across setup telemetry. (COMPLETED 2026-04-25)
- [x] Add rollout gates with hard-stop conditions on drift or policy failure. (COMPLETED 2026-04-25)
- [x] Publish runbooks for resume, rollback/compensation review, and demo cleanup. (COMPLETED 2026-04-25)

## Hard-Fail Governance Conditions

- [x] Manifest hash mismatch between plan and execute. (COMPLETED 2026-04-25)
- [x] Plan hash mismatch between approval and execute. (COMPLETED 2026-04-25)
- [x] Missing approval metadata in protected mode. (COMPLETED 2026-04-25)
- [x] Failed preflight with unacknowledged blockers. (COMPLETED 2026-04-25)
- [x] Blocked policy decision not explicitly overridden by approved policy flow. (COMPLETED 2026-04-25)

## Acceptance Criteria

- CI rejects unsafe schema changes before merge.
- Operations can trace each run by operation ID, plan hash, manifest hash, and approval record.
- Rollout is stage-gated and can be halted deterministically.
- Support runbooks are complete and tested against simulated failures.