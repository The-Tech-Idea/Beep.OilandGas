# Facility Management Module — Implementation Plan

## Overview

Comprehensive facility lifecycle management for oil & gas facilities (gathering centers, processing plants, compressor stations, injection facilities, storage terminals). Covers design → construction → commissioning → operation → maintenance → decommissioning.

## PPDM39 Tables Used (25+ tables)

### Core Facility Tables
| Table | Purpose |
|-------|---------|
| `FACILITY` | Master facility record (type, location, dates, operator) |
| `FACILITY_ALIAS` | Alternate names/IDs for the facility |
| `FACILITY_AREA` | Geographic area assignment |
| `FACILITY_CLASS` | Facility classification (gathering, processing, compression, etc.) |
| `FACILITY_COMPONENT` | Sub-components/subsystems within the facility |
| `FACILITY_DESCRIPTION` | Detailed facility descriptions |
| `FACILITY_FIELD` | Link to parent field |
| `FACILITY_VERSION` | Version history for facility changes |
| `FACILITY_XREF` | Cross-references to other entities |

### Facility Status & Lifecycle
| Table | Purpose |
|-------|---------|
| `FACILITY_STATUS` | Current operational status (active, shut-in, abandoned, etc.) |
| `FACILITY_MAINTAIN` | Maintenance records for the facility |
| `FACILITY_MAINT_STATUS` | Maintenance status reference |
| `FACILITY_RATE` | Production/processing rate limits and actuals |
| `FACILITY_RESTRICTION` | Operational restrictions and constraints |

### Facility Licensing & Compliance
| Table | Purpose |
|-------|---------|
| `FACILITY_LICENSE` | Operating licenses and permits |
| `FACILITY_LIC_ALIAS` | License alternate identifiers |
| `FACILITY_LIC_AREA` | Licensed area coverage |
| `FACILITY_LIC_COND` | License conditions and requirements |
| `FACILITY_LIC_REMARK` | License notes and comments |
| `FACILITY_LIC_STATUS` | License status tracking |
| `FACILITY_LIC_TYPE` | License type classification |
| `FACILITY_LIC_VIOLATION` | License violation records |

### Facility Equipment & Substances
| Table | Purpose |
|-------|---------|
| `FACILITY_EQUIPMENT` | Equipment installed at the facility |
| `FACILITY_SUBSTANCE` | Substances handled/processed at the facility |
| `FACILITY_BA_SERVICE` | Business associates providing services to the facility |

### Equipment Tables (shared with Equipment module)
| Table | Purpose |
|-------|---------|
| `EQUIPMENT` | Individual equipment items (pumps, compressors, separators, etc.) |
| `EQUIPMENT_STATUS` | Equipment operational status |
| `EQUIPMENT_MAINTAIN` | Equipment maintenance records |
| `EQUIPMENT_MAINT_STATUS` | Maintenance status reference |
| `EQUIPMENT_MAINT_TYPE` | Maintenance type classification |
| `EQUIPMENT_SPEC` | Equipment specifications |
| `EQUIPMENT_SPEC_SET` | Equipment specification sets |
| `EQUIPMENT_SPEC_SET_SPEC` | Specification set details |
| `EQUIPMENT_ALIAS` | Equipment alternate identifiers |
| `EQUIPMENT_BA` | Equipment business associate assignments |
| `EQUIPMENT_COMPONENT` | Equipment sub-components |
| `EQUIPMENT_USE_STAT` | Equipment usage statistics |
| `EQUIPMENT_XREF` | Equipment cross-references |
| `CAT_EQUIPMENT` | Equipment catalog/master data |
| `CAT_EQUIP_ALIAS` | Catalog equipment aliases |
| `CAT_EQUIP_SPEC` | Catalog equipment specifications |

### Production Reporting (facility-level)
| Table | Purpose |
|-------|---------|
| `PDEN` | Production declaration entity (facility-level reporting) |
| `PDEN_VOL_SUMMARY` | Facility production volume summaries |
| `PDEN_VOL_DISPOSITION` | How facility production is disposed/allocated |
| `PDEN_FLOW_MEASUREMENT` | Flow measurement data at facility |
| `PDEN_FACILITY` | PDEN-facility linkage |
| `PDEN_OPER_HIST` | Facility operational history |
| `PDEN_STATUS_HIST` | Facility status history |

### Work Orders & Maintenance
| Table | Purpose |
|-------|---------|
| `WORK_ORDER` | Work orders for facility maintenance/operations |
| `WORK_ORDER_INSTRUCTION` | Work order instructions |
| `WORK_ORDER_DELIVERY` | Work order delivery tracking |
| `WORK_ORDER_BA` | Work order business associate assignments |
| `WORK_ORDER_COMPONENT` | Work order components |
| `WORK_ORDER_CONDITION` | Work order conditions |
| `WORK_ORDER_XREF` | Work order cross-references |

### Financial
| Table | Purpose |
|-------|---------|
| `FINANCE` | Facility financial records |
| `FIN_COMPONENT` | Financial components |
| `FIN_COST_SUMMARY` | Cost summaries |
| `FIN_XREF` | Financial cross-references |

---

## Facility Lifecycle Phases

### Phase 1: Facility Planning & Design
- **Tables**: `FACILITY`, `FACILITY_CLASS`, `FACILITY_DESCRIPTION`, `FACILITY_AREA`, `FACILITY_FIELD`
- **Process**:
  1. Create facility master record with type, location, field assignment
  2. Define facility classification (gathering center, processing plant, etc.)
  3. Link to parent field and geographic area
  4. Record design specifications and capacity ratings
  5. Create facility components/subsystems hierarchy

### Phase 2: Licensing & Permits
- **Tables**: `FACILITY_LICENSE`, `FACILITY_LIC_*`, `APPLICATION`, `APPLIC_*`
- **Process**:
  1. Create operating license records
  2. Track license conditions and compliance requirements
  3. Monitor license expiry and renewal dates
  4. Record any license violations
  5. Link to permit applications if applicable

### Phase 3: Construction & Commissioning
- **Tables**: `FACILITY`, `FACILITY_VERSION`, `WORK_ORDER`, `EQUIPMENT`, `EQUIPMENT_SPEC`
- **Process**:
  1. Track construction progress via work orders
  2. Register equipment as installed
  3. Record equipment specifications and ratings
  4. Create facility version records for as-built state
  5. Commissioning checklist and sign-off

### Phase 4: Operations & Production
- **Tables**: `FACILITY_STATUS`, `FACILITY_RATE`, `PDEN`, `PDEN_VOL_SUMMARY`, `PDEN_FLOW_MEASUREMENT`
- **Process**:
  1. Set facility to "Active" status
  2. Record daily production volumes (oil, gas, water)
  3. Track flow measurements at facility inlet/outlet
  4. Monitor production rates against capacity limits
  5. Record operational restrictions
  6. Track facility uptime/downtime

### Phase 5: Maintenance & Reliability
- **Tables**: `FACILITY_MAINTAIN`, `EQUIPMENT_MAINTAIN`, `WORK_ORDER`, `EQUIPMENT_STATUS`
- **Process**:
  1. Schedule preventive maintenance via work orders
  2. Record corrective maintenance events
  3. Track equipment status changes
  4. Monitor maintenance costs
  5. Calculate equipment reliability metrics (MTBF, MTTR)
  6. Track facility availability

### Phase 6: Modification & Upgrade
- **Tables**: `FACILITY_VERSION`, `FACILITY_COMPONENT`, `EQUIPMENT`, `WORK_ORDER`
- **Process**:
  1. Create facility version for modification
  2. Add/remove/modify components
  3. Install new equipment or upgrade existing
  4. Update capacity ratings and specifications
  5. Re-commission modified systems

### Phase 7: Decommissioning & Abandonment
- **Tables**: `FACILITY_STATUS`, `FACILITY_VERSION`, `WORK_ORDER`, `FACILITY_LICENSE`
- **Process**:
  1. Set facility to "Shut-in" then "Abandoned" status
  2. Execute decommissioning work orders
  3. Remove equipment records
  4. Close out licenses
  5. Record final facility version (as-abandoned)

---

## Service Architecture

### `IFacilityManagementService` (PPDM39 entities only)

Facility workflows persist and return **`Beep.OilandGas.PPDM39.Models`** types (`FACILITY`, `FACILITY_STATUS`, `PDEN_VOL_SUMMARY`, …). There is **no parallel facility DTO tree** in Models; API layers may map to view models as needed.

```
Beep.OilandGas.ProductionOperations/Services/
├── IFacilityManagementService.cs
├── FacilityManagementService.cs           — Core facility, status, classes, components, rates, equipment, licenses
└── FacilityManagementService.Operations.cs — PDEN / PDEN_FACILITY, volumes, maintenance, work orders, reliability tuple
```

**Dependency injection (ApiService):** register `IFacilityManagementService` **before** `IProductionOperationsService`. `ProductionOperationsService` delegates facility production volumes and `FACILITY_STATUS` writes to `IFacilityManagementService` (PDEN linkage via `EnsureFacilityPdenAsync`, license checks on operational status).

See also: [Production operations service model](ProductionOperations_Service_Model.md).

### API Controllers (implemented)

All routes require authorization; pass `CancellationToken` via standard ASP.NET request abortion (bound on actions).

| File | Base route | Notes |
|------|------------|--------|
| `FacilityController.cs` | `GET/POST/PUT api/facility` | List/create/update facility; `GET .../pden/facility-subtype` lists facility PDENs via `IProductionManagementService`; nested `classes`, `components`, `status`, `rates` |
| `FacilityEquipmentController.cs` | `api/facility/{facilityId}/equipment` | List; `POST .../{equipmentId}` links equipment |
| `FacilityMaintenanceController.cs` | `api/facility/{facilityId}/maintenance` | List (optional date range); create |
| `FacilityProductionController.cs` | `api/facility/{facilityId}/production` | Ensure PDEN; list/record volumes; reliability metrics |
| `FacilityLicenseController.cs` | `api/facility/{facilityId}/licenses` | List; create; `GET .../active` |
| `FacilityWorkOrderController.cs` | `api/facility/{facilityId}/work-orders` | List; create (`facilityType` query required) |

Payloads and responses use **`Beep.OilandGas.PPDM39.Models`** entity types (no parallel DTO layer).

---

## Implementation Priority

### Sprint 1: Core Facility Management
- [ ] `FACILITY` CRUD (create, read, update, soft-delete)
- [ ] `FACILITY_STATUS` tracking (status changes over time)
- [ ] `FACILITY_CLASS` classification
- [ ] `FACILITY_COMPONENT` hierarchy
- [ ] `FACILITY_DESCRIPTION` management
- [ ] Facility search and filtering

### Sprint 2: Equipment Management
- [ ] `EQUIPMENT` CRUD at facility level
- [ ] `EQUIPMENT_STATUS` tracking
- [ ] `EQUIPMENT_SPEC` specifications
- [ ] `CAT_EQUIPMENT` catalog integration
- [ ] Equipment-to-facility assignment
- [ ] Equipment hierarchy (parent/child)

### Sprint 3: Maintenance & Work Orders
- [ ] `FACILITY_MAINTAIN` records
- [ ] `EQUIPMENT_MAINTAIN` records
- [ ] `WORK_ORDER` creation and tracking
- [ ] `WORK_ORDER_INSTRUCTION` details
- [ ] Preventive maintenance scheduling
- [ ] Maintenance cost tracking

### Sprint 4: Production Reporting
- [ ] `PDEN` facility-level records
- [ ] `PDEN_VOL_SUMMARY` daily volumes
- [ ] `PDEN_FLOW_MEASUREMENT` flow data
- [ ] `FACILITY_RATE` capacity tracking
- [ ] Production dashboards and reports

### Sprint 5: Licensing & Compliance
- [ ] `FACILITY_LICENSE` management
- [ ] `FACILITY_LIC_COND` conditions tracking
- [ ] `FACILITY_LIC_STATUS` status monitoring
- [ ] `FACILITY_LIC_VIOLATION` violation records
- [ ] License expiry alerts

### Sprint 6: Analytics & Reporting
- [ ] Facility uptime/downtime calculations
- [ ] Equipment reliability metrics (MTBF, MTTR)
- [ ] Production efficiency reports
- [ ] Maintenance cost analysis
- [ ] Facility capacity utilization

---

## Key Business Rules

1. **Every facility must have a `FACILITY_CLASS`** (gathering, processing, compression, injection, storage, etc.)
2. **Every facility must have at least one `FACILITY_STATUS` record** with effective date
3. **Equipment must be linked to a facility** via `FACILITY_EQUIPMENT`
4. **Production volumes are reported via `PDEN`** with `PDEN_SUBTYPE = 'FACILITY'`
5. **Maintenance records link to both facility and equipment** for full traceability
6. **Licenses must be active** for a facility to be in "Active" status
7. **Facility versions track all significant changes** (modifications, upgrades, status changes)
8. **Work orders are the primary mechanism** for scheduling and tracking maintenance

---

## Integration Points

- **FieldOrchestrator**: All facility operations scoped to current active field
- **HSE Project**: Safety incidents at facilities (cross-reference via `HSE_INCIDENT_EQUIP`)
- **PipelineManagement**: Pipeline facilities (FACILITY_CLASS = 'PIPELINE')
- **ProductionOperations**: `ProductionOperationsService` delegates facility PDEN volumes and `FACILITY_STATUS` to **`IFacilityManagementService`**; well-level `PDEN_VOL_SUMMARY` remains in production operations where appropriate
- **Accounting**: Facility costs flow to `FINANCE` and `FIN_COST_SUMMARY`
