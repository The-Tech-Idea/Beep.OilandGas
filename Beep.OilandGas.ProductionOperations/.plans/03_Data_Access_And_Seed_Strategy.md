# ProductionOperations Data Access and Seed Strategy

## Canonical Data Access Policy
- `ProductionOperationsService` and `FacilityManagementService` use `PPDMGenericRepository` + `AppFilter` as the canonical path.
- `ProductionManagementService` remains a compatibility-oriented surface and must not become a second canonical write path for the same workflows.
- Facility monitoring extensions are bounded to:
  - `FACILITY_MEASUREMENT`
  - `FACILITY_EQUIPMENT_ACTIVITY`
  - `R_FACILITY_MONITORING_CODE`

## PPDM-First Mapping Matrix
- Canonical PPDM entities:
  - `PDEN`, `PDEN_FACILITY`, `PDEN_VOL_SUMMARY`
  - `FACILITY`, `FACILITY_STATUS`, `FACILITY_COMPONENT`, `FACILITY_RATE`
  - `FACILITY_EQUIPMENT`, `FACILITY_MAINTAIN`, `FACILITY_LICENSE`
  - `WORK_ORDER`, `WORK_ORDER_COMPONENT`, `CAT_EQUIPMENT`, `PRODUCTION_COSTS`
- Extension entities:
  - `FACILITY_MEASUREMENT`, `FACILITY_EQUIPMENT_ACTIVITY`, `R_FACILITY_MONITORING_CODE`

## Seeding Strategy
- Deterministic keying for reference rows:
  - `CAT_EQUIPMENT`: by `CATALOGUE_EQUIP_ID`
  - `R_FACILITY_MONITORING_CODE`: by `(REFERENCE_SET, REFERENCE_CODE)`
- Insert-if-missing only.
- Cancellation-aware loops with per-row error capture.
