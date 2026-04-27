# ProductionAccounting Data Access and Seed Strategy

## Canonical Data Access Direction

- Use `PPDMGenericRepository` with metadata resolution and `AppFilter` for production-accounting table operations.
- Keep audit/default handling centralized via `ICommonColumnHandler` and `IPPDM39DefaultsRepository`.
- Eliminate avoidable sync-over-async repository calls in compatibility code over time.

## Service Area Strategy Matrix

- **Orchestrator (`ProductionAccountingService`)**
  - Strategy: PPDM repository + domain service orchestration.
  - Status: active and mostly consistent.
- **Compatibility managers (`ControllerFacade`)**
  - Strategy: compatibility-only wrappers; convert to async-safe repository access where persistent behavior is required.
  - Status: mixed; includes in-memory fallback/state.
- **Module setup**
  - Strategy: keep broad entity registration and add explicit seed sets for reference values.
  - Status: implemented with project-owned gap-fill reference table + idempotent seeding.

## Reference Value Policy (`R_` / `RA_`)

- Prefer PPDM-native `R_`/`RA_` tables whenever suitable.
- If no suitable PPDM reference table exists, define project-owned reference table(s) and seed through module setup.
- Candidate seed domains to formalize:
  - run ticket disposition/status
  - allocation methods/types
  - royalty payment status/types
  - period close status/reasons
  - revenue and deduction classification codes

## Seed Scope Block (implemented in module setup)

- **Tables**: production-accounting operational tables and custom reference tables.
- **Projections**: none seeded (computed/read models only).
- **Core**: reference-code dictionaries and default domain constants consumed by services.

## Verification Criteria

- Seed is idempotent and safely re-runnable.
- Reference values are queryable via PPDM/production-accounting services after setup.
- No new direct SQL string building is introduced for standard CRUD/query paths.

