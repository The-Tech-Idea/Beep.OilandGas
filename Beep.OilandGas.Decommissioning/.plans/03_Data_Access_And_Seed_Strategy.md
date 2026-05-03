# Decommissioning Data Access and Seed Strategy

## PPDM-First Matrix
- Reuse directly:
  - `WELL`, `WELL_ABANDONMENT`, `WELL_STATUS`, `WELL_ACTIVITY`
  - `FACILITY`, `FACILITY_STATUS`
  - `R_WELL_STATUS`, `R_WELL_STATUS_TYPE`
- Optional metadata-dependent:
  - `FACILITY_DECOMMISSIONING`, `ENVIRONMENTAL_RESTORATION`, `DECOMMISSIONING_COST`
- Module-local:
  - `DECOMMISSIONING_STATUS`, `ABANDONMENT_STATUS`
  - `R_DECOMMISSIONING_REFERENCE_CODE` (module reference-set catalog)

## WELL_STATUS Contract
- Required keys: `UWI`, `STATUS_TYPE`, `STATUS_ID`, `EFFECTIVE_DATE`
- Required fields: `STATUS`, `ACTIVE_IND`, audit columns
- Validation:
  - `STATUS_TYPE` exists in `R_WELL_STATUS_TYPE`
  - (`STATUS_TYPE`, `STATUS`) exists in `R_WELL_STATUS`

## Seed Strategy
- Add deterministic reference seeds by `(REFERENCE_SET, REFERENCE_CODE)`.
- Use idempotent insert-if-missing semantics in `DecommissioningModule.SeedAsync`.
- Include status families for abandonment/decommissioning/restoration/cost lifecycle and WELL_STATUS alignment.
