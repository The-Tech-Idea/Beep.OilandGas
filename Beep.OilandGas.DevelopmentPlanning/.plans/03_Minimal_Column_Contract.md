# DevelopmentPlanning Minimal Column Contract

## PPDM Reuse Contracts

### WELL_ACTIVITY (mandatory scheduling anchor)
- Required for scheduling: `UWI`, `ACTIVITY_TYPE`, `ACTIVITY_DATE`, `ACTIVE_IND`
- Required for operational linkage: activity identifier columns and `ROW_CREATED_*`/`ROW_CHANGED_*` audit fields
- Recommended for assignment context: BA/service linkage columns when available in deployed schema

### WELL_ACTIVITY_COMPONENT (when activity decomposition required)
- Required: activity key fields, `COMPONENT_TYPE`, `ACTIVE_IND`

### BUSINESS_ASSOCIATE + BA_SERVICE
- Required for service-company assignment: BA identifier + service type code + active indicator

## Module-Local Contracts

### FIELD_DEVELOPMENT_PLAN
- Required: `FDP_ID`, `FIELD_ID`, `FDP_NAME`, `FDP_STATUS`, `ACTIVE_IND`

### DEVELOPMENT_WELL_SCHEDULE
- Required: `SCHEDULE_ID`, `FDP_ID`, `PLANNED_WELL_ID`, `SCHEDULE_STATUS`, `ACTIVE_IND`

### FACILITY_INVESTMENT
- Required: `FACILITY_INV_ID`, `FDP_ID`, `FACILITY_TYPE`, `INVESTMENT_PHASE`, `ACTIVE_IND`

### DEVELOPMENT_COSTS
- Required: `DEVELOPMENT_COST_ID`, `FIELD_ID`, `FDP_ID`, `COST_YEAR`, `COST_CATEGORY`, `COST_TYPE`, `COST_CURRENCY`, `ACTIVE_IND`

### WELL_MAINTENANCE_PLAN
- Required: `MAINT_PLAN_ID`, `FDP_ID`, `UWI`, `MAINTENANCE_TYPE`, `MAINTENANCE_STATUS`, `PLANNED_START_DATE`, `ACTIVE_IND`

### WELL_SERVICE_JOB
- Required: `JOB_ID`, `FDP_ID`, `UWI`, `JOB_TYPE`, `JOB_STATUS`, `SERVICE_BA_ID`, `PLANNED_START_DATE`, `ACTIVE_IND`
