# Permits and Applications Data Model Audit

## Boundary

The PPDM39 foundation owns standard PPDM tables. The Permits and Applications module should not create replacement schemas for PPDM-owned tables.

PPDM-owned tables used by this project include:

- `APPLICATION`
- `APPLICATION_COMPONENT`
- `APPLIC_BA`
- `APPLIC_DESC`
- `APPLIC_REMARK`
- `BA_PERMIT`
- `FACILITY_LICENSE`
- `WELL_LICENSE`
- `WELL_PERMIT_TYPE`

The optional `PERMITS` module owns only permits-specific extension tables and setup data, such as:

- `PERMIT_APPLICATION`
- `APPLICATION_AREA`
- `APPLICATION_ATTACHMENT`
- `PERMIT_STATUS_HISTORY`
- `DRILLING_PERMIT_APPLICATION`
- `ENVIRONMENTAL_PERMIT_APPLICATION`
- `INJECTION_PERMIT_APPLICATION`
- `JURISDICTION_REQUIREMENTS`
- `MIT_RESULT`
- `REQUIRED_FORM`

## Current Cleanup

- `PermitsModule` no longer declares PPDM-owned duplicate tables as module schema entities.
- `PERMIT_APPLICATION` table class no longer contains collection/object graph properties.
- `APPLICATION_COMPONENT` table class no longer contains projection-style object aliases.

## Process Rules

- Persisted table classes under `Data/PermitsAndApplications/Tables` must remain scalar-only.
- Rich workflow objects with nested attachments, areas, or components belong under `Data/PermitsAndApplications/Projections`.
- Service writes to standard PPDM tables should use PPDM39 model classes or explicit mapper methods, not project-local duplicate table classes with different column names.
- Optional module setup should install only permits extension tables; PPDM39 standard tables are installed by the required foundation flow.

## Remaining Review Targets

The project still contains local classes with names matching PPDM-owned tables. They are retained for compatibility in this pass, but should be migrated toward either PPDM39 model usage or renamed projection classes:

- `APPLIC_BA`
- `APPLIC_DESC`
- `APPLIC_REMARK`
- `APPLICATION_COMPONENT`
- `BA_PERMIT`
- `FACILITY_LICENSE`
- `WELL_PERMIT_TYPE`
