# DevelopmentPlanning PPDM Usage Matrix

## Reuse First

| Concept | Canonical PPDM Table(s) | Notes |
|---|---|---|
| Well identity/context | `WELL` | Use `UWI` as canonical key where available. |
| Well activities | `WELL_ACTIVITY`, `WELL_ACTIVITY_COMPONENT` | Activity timeline and component-level execution. |
| Well lifecycle status | `WELL_STATUS`, `R_WELL_STATUS`, `R_WELL_STATUS_TYPE` | Validate state transitions with reference families. |
| Well hierarchy/linking | `WELL_XREF` | Use xref types for parent-child links. |
| Service company master | `BUSINESS_ASSOCIATE` | Canonical BA identity. |
| Service capability typing | `BA_SERVICE`, `R_BA_SERVICE_TYPE` | Validate BA-service compatibility. |
| Maintenance operational context | `EQUIPMENT_MAINTAIN`, `FACILITY_MAINTAIN`, `SF_MAINTAIN` | Prefer PPDM operations tables for actual execution records. |

## Module-Local Extension (Only for Planning Gaps)

| Concept | Module Table | Why Local |
|---|---|---|
| FDP plan header/version | `FIELD_DEVELOPMENT_PLAN` | Scenario/version governance not represented as a single PPDM planning header table. |
| Well schedule by FDP | `DEVELOPMENT_WELL_SCHEDULE` | Plan-specific schedule snapshots and sequence. |
| Facility investment by FDP | `FACILITY_INVESTMENT` | Plan-phase CAPEX tracking. |
| Development costs | `DEVELOPMENT_COSTS` | Plan-year cost rollup and category alignment. |
| Well maintenance planning | `WELL_MAINTENANCE_PLAN` | Planning windows and trigger metadata before operational posting. |
| Service job planning | `WELL_SERVICE_JOB` | Vendor/job scheduling tied to plan phase. |
| Module reference codes | `R_DEVELOPMENT_PLANNING_REFERENCE_CODE` | Module-owned fallback reference sets where PPDM references are absent. |
