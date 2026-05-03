# LeaseAcquisition Master Tracker

## Phase Rollup
- [x] Phase 0 - Context baseline and architecture map
- [x] Phase 1 - Behavior and ownership boundary map
- [x] Phase 2 - Interface status and canonical contract map
- [x] Phase 3 - Data access and seed strategy definition
- [x] Phase 4 - Execution-ready phased implementation plan
- [x] Phase 5 - Verification gates and migration notes

## Active TODOs
- [x] Enforce canonical interface boundary (models-core vs advanced/staged via `ILeaseAcquisitionAdvancedService`).
- [x] Normalize lease canonical persistence paths — Option A documented; `LAND_*` writes only.
- [x] Introduce `R_LEASE_REFERENCE_CODE` with idempotent module seeding.
- [x] Harden lease acquisition controller validation/error mapping (`Operations` + optional field routes).
- [x] Add focused controller + seed catalog + interface contract tests.
- [x] Execute build/test verification gates and close migration checklist.

## Verification Criteria
- Canonical PPDM table strategy is documented and implemented (Option A).
- No table/projection contract violations in write paths.
- Seed reruns are deterministic with no duplicate natural keys `(REFERENCE_SET, REFERENCE_CODE)`.
- API contract boundaries (canonical vs advanced) are explicit and test-covered.
- LeaseAcquisition project and ApiService build green.

## Exit Criteria
- All `.plans` documents are present and consistent.
- Execution backlog has concrete target files and acceptance criteria.
- Verification/migration gates are executable and complete.
