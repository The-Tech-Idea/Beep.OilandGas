# Facility Monitoring and Equipment Tracking

## Purpose

Adds a PPDM-style vertical monitoring capability for facility operations that supports:

- time-series measurements (for example tank level, pressure, temperature)
- equipment install/uninstall/move/replace lifecycle history
- unlimited monitoring and activity types via table-backed reference codes

## Data Model

## 1) Transaction tables (project-owned)

- `FACILITY_MEASUREMENT`
  - tracks measured values over time
  - key fields: `MEASUREMENT_ID`, `FACILITY_ID`, `FACILITY_TYPE`, optional `EQUIPMENT_ID`
  - semantic fields: `MEASUREMENT_TYPE`, `MEASURED_VALUE`, `MEASURED_UOM`, `MEASURED_DATE`, `QUALITY_CODE`
  - extensibility fields: `SOURCE_SYSTEM`, `DETAILS_JSON`

- `FACILITY_EQUIPMENT_ACTIVITY`
  - tracks install/uninstall/move/replace lifecycle events
  - key fields: `ACTIVITY_ID`, `FACILITY_ID`, `FACILITY_TYPE`, `EQUIPMENT_ID`
  - semantic fields: `ACTIVITY_TYPE`, `ACTIVITY_DATE`, `END_DATE`, `INSTALL_SEQUENCE`
  - location/context fields: `LOCATION_DESC`, `POSITION_DESC`, `REASON_TEXT`
  - extensibility fields: `SOURCE_SYSTEM`, `DETAILS_JSON`

## 2) Reference table (project-owned)

- `R_FACILITY_MONITORING_CODE`
  - generic set/code catalog for monitoring features when standard PPDM `R_*` sets do not cover project-specific values
  - columns: `REFERENCE_SET`, `REFERENCE_CODE`, `LONG_NAME`, `SHORT_NAME`

## Service Layer

`IFacilityManagementService` new operations:

- `ListFacilityMeasurementsAsync(...)`
- `RecordFacilityMeasurementAsync(...)`
- `ListEquipmentActivityAsync(...)`
- `RecordEquipmentActivityAsync(...)`

Implemented in `FacilityManagementService.Operations.cs` with `PPDMGenericRepository`.

Behavior:

- resolves facility identity (`facilityId`, optional `facilityType`) before write/read
- auto-generates IDs when missing
- sets defaults for source and timestamps
- preserves audit columns through `ICommonColumnHandler`

## API Layer

Controller: `Beep.OilandGas.ApiService/Controllers/Facility/FacilityMonitoringController.cs`

Routes:

- `GET /api/facility/{facilityId}/monitoring/measurements`
- `POST /api/facility/{facilityId}/monitoring/measurements`
- `GET /api/facility/{facilityId}/monitoring/equipment/{equipmentId}/activity`
- `POST /api/facility/{facilityId}/monitoring/equipment/{equipmentId}/activity`

## Module Setup and Seeding

`FacilityManagementModuleSetup` now includes:

- entity registration for:
  - `FACILITY_MEASUREMENT`
  - `FACILITY_EQUIPMENT_ACTIVITY`
  - `R_FACILITY_MONITORING_CODE`
- idempotent seed rows for:
  - `EQUIPMENT_ACTIVITY_TYPE` (`INSTALL`, `UNINSTALL`, `MOVE`, `REPLACE`)
  - `MEASUREMENT_TYPE` (tank level and common operational measurements)
  - `MEASUREMENT_QUALITY` (`MEASURED`, `ESTIMATED`, `IMPUTED`)
  - `MEASUREMENT_UOM` (common units)

## Practical Usage Examples

## Tank level history

1. POST measurement with:
   - `MEASUREMENT_TYPE = "TANK_LEVEL"`
   - `MEASURED_VALUE = <numeric>`
   - `MEASURED_UOM = "PERCENT"` or `"FT"`/`"M"`
2. Query trend by date range via `GET .../measurements?measurementType=TANK_LEVEL&startDate=...&endDate=...`

## Equipment install/uninstall tracking

1. POST equipment activity with:
   - `ACTIVITY_TYPE = "INSTALL"` / `"UNINSTALL"` / `"MOVE"` / `"REPLACE"`
   - optional `LOCATION_DESC`, `POSITION_DESC`, `REASON_TEXT`
2. Query lifecycle history via `GET .../equipment/{equipmentId}/activity`

## Notes

- This slice complements (does not replace) existing PPDM tables such as `FACILITY_EQUIPMENT`, `FACILITY_MAINTAIN`, and `WORK_ORDER`.
- For high-frequency telemetry ingestion, batching and retention policy should be added in a later phase.

