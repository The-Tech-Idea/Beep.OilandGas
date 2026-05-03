# DevelopmentPlanning Migration Notes

## Canonical Persistence Shift
- Development-plan CRUD now persists against `FIELD_DEVELOPMENT_PLAN` instead of `APPLICATION`.
- Plan-linked reads now source module-local canonical tables:
  - `DEVELOPMENT_WELL_SCHEDULE`
  - `FACILITY_INVESTMENT`
  - `DEVELOPMENT_COSTS`

## New Planning Tables
- `WELL_MAINTENANCE_PLAN` for maintenance scheduling windows and lifecycle.
- `WELL_SERVICE_JOB` for service-company job planning and assignment.
- `R_DEVELOPMENT_PLANNING_REFERENCE_CODE` for module-owned fallback reference sets.

## Integration Notes
- API DI now wires `IDevelopmentPlanService` using factory-based registration in `ApiService`.
- API surface added under `api/field/current/development-planning` for canonical plan CRUD and scheduling.
- Permit linkage remains lifecycle-owned; no duplicate permit persistence introduced in DevelopmentPlanning.
- Full-solution migration validation is now complete (`dotnet build Beep.OilandGas.sln` green).
