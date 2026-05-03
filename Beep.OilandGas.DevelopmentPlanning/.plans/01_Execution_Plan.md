# DevelopmentPlanning Canonical Execution

## Scope
- Canonicalize DevelopmentPlanning persistence onto module-owned PPDM-style table classes.
- Reuse PPDM well, maintenance, business associate, and reference tables wherever feasible.
- Keep schema generation class-first; do not add SQL scripts in this phase.

## Phase 0 - Baseline and Governance
- [x] Create local `.plans` and `MASTER-TODO-TRACKER.md`.
- [ ] Maintain per-phase status and verification notes in this folder.

## Phase 1 - Canonical Model Contracts
- [ ] Finalize plan header/detail relationships:
  - `FIELD_DEVELOPMENT_PLAN` -> `DEVELOPMENT_WELL_SCHEDULE`
  - `FIELD_DEVELOPMENT_PLAN` -> `FACILITY_INVESTMENT`
  - `FIELD_DEVELOPMENT_PLAN` -> `DEVELOPMENT_COSTS`
- [ ] Define maintenance/service-job planning links for well activities and service companies.
- [ ] Publish PPDM-first concept mapping (reuse first, local extension second).

## Phase 2 - PPDM-Style Table Modeling (Class-First)
- [ ] Add/adjust only table classes (no SQL scripts).
- [ ] Add module-local planning tables only when PPDM operational tables do not represent planning metadata.
- [ ] Ensure all table classes are scalar-only and repository-safe.

## Phase 3 - Seeding
- [ ] Add `DevelopmentPlanningReferenceCodes` and `DevelopmentPlanningReferenceCodeSeed`.
- [ ] Seed required development/maintenance/job families idempotently via module setup.
- [ ] Add seed integrity tests.

## Phase 4 - Service Canonicalization
- [ ] Refactor `DevelopmentPlanService` from `APPLICATION` persistence to `FIELD_DEVELOPMENT_PLAN`.
- [ ] Bind well/facility/cost outputs by plan identity using canonical detail tables.
- [ ] Add well activity + service-company job scheduling flows linked to PPDM BA and well tables.

## Phase 5 - API and DI
- [ ] Register canonical development planning service in API DI using factory pattern.
- [ ] Add/align API endpoints for plan CRUD and maintenance/job scheduling views.

## Phase 6 - Tests and Hardening
- [ ] Add focused service/API tests for canonical behavior and idempotent setup.
- [ ] Add failure-path and validation tests for reference/table linkage constraints.

## Phase 7 - Exit
- [ ] Reconcile tracker and `.plans` state.
- [ ] Capture runtime verification notes and remaining backlog.
