# HSE PPDM Refactor Tracker

- [x] Validate actual PPDM HSE entity coverage from generated model classes.
- [x] Confirm seeded HSE process definitions already exist in lifecycle initialization.
- [x] Create phase-by-phase implementation plan in `Beep.OilandGas.HSE/.plans`.
- [x] Refactor incident service to persist header, detail, component, BA, and response rows through PPDM classes.
- [x] Refactor RCA service to use `HSE_INCIDENT_CAUSE` real columns.
- [x] Refactor barrier service to use `HSE_INCIDENT_COMPONENT` real columns.
- [x] Refactor KPI service to derive field scope from component rows.
- [x] Review corrective-action and HAZOP services against PPDM project tables and keep them on PPDM `PROJECT*` tables where no dedicated HSE PPDM table was verified.
- [x] Add a field-scoped HSE lifecycle service and expose it from `IFieldOrchestrator`.
- [x] Rewire HSE API controllers through lifecycle orchestration.
- [x] Remove placeholder web submission and close-out behavior.
- [x] Build touched projects and resolve resulting integration errors.