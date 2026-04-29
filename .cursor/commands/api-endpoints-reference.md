# API Endpoints Reference

This document provides a quick reference for all API endpoints in the Beep.OilandGas.ApiService project.

## Authentication
All endpoints require authentication via `[Authorize]` attribute unless otherwise noted.

## Base URL
All endpoints are prefixed with `/api/`

---

## Calculation Services

### Gas Lift (`/api/gaslift`)
- `POST /api/gaslift/analyze-potential` - Analyze gas lift potential
- `POST /api/gaslift/design-valves` - Design gas lift valves
- `POST /api/gaslift/design` - Save gas lift design
- `GET /api/gaslift/performance/{wellUWI}` - Get gas lift performance

### Nodal Analysis (`/api/nodalanalysis`)
- `POST /api/nodalanalysis/analyze` - Perform nodal analysis
- `POST /api/nodalanalysis/optimize` - Optimize system
- `POST /api/nodalanalysis/result` - Save analysis result
- `GET /api/nodalanalysis/history/{wellUWI}` - Get analysis history

### Production Forecasting (`/api/productionforecasting`)
- `POST /api/productionforecasting/generate` - Generate forecast
- `POST /api/productionforecasting/decline-curve` - Perform decline curve analysis
- `POST /api/productionforecasting/forecast` - Save forecast

### Pipeline Analysis (`/api/pipelineanalysis`)
- `POST /api/pipelineanalysis/analyze-flow` - Analyze pipeline flow
- `POST /api/pipelineanalysis/pressure-drop` - Calculate pressure drop
- `POST /api/pipelineanalysis/result` - Save analysis result

### Economic Analysis (`/api/economicanalysis`)
- `POST /api/economicanalysis/npv` - Calculate NPV
- `POST /api/economicanalysis/irr` - Calculate IRR
- `POST /api/economicanalysis/analyze` - Comprehensive analysis
- `POST /api/economicanalysis/npv-profile` - Generate NPV profile
- `POST /api/economicanalysis/result` - Save analysis result
- `GET /api/economicanalysis/result/{analysisId}` - Get analysis result

### Flash Calculations (`/api/flashcalculation`)
- `POST /api/flashcalculation/isothermal` - Perform isothermal flash
- `POST /api/flashcalculation/multi-stage` - Perform multi-stage flash
- `POST /api/flashcalculation/result` - Save flash result
- `GET /api/flashcalculation/history` - Get flash history

---

## Properties Services

### Gas Properties (`/api/gasproperties`)
- `POST /api/gasproperties/calculate` - Calculate gas properties
- `POST /api/gasproperties/composition` - Save gas composition
- `GET /api/gasproperties/composition/{compositionId}` - Get gas composition
- `GET /api/gasproperties/history/{compositionId}` - Get property history

### Oil Properties (`/api/oilproperties`)
- `POST /api/oilproperties/calculate` - Calculate oil properties
- `POST /api/oilproperties/composition` - Save oil composition
- `GET /api/oilproperties/composition/{compositionId}` - Get oil composition
- `GET /api/oilproperties/history/{compositionId}` - Get property history

### Heat Map (`/api/heatmap`)
- `POST /api/heatmap/generate` - Generate heat map
- `POST /api/heatmap/configuration` - Save heat map configuration
- `GET /api/heatmap/configuration/{heatMapId}` - Get heat map configuration
- `POST /api/heatmap/production` - Generate production heat map

---

## Operations Services

### Prospect Identification (`/api/prospectidentification`)
- `POST /api/prospectidentification/evaluate/{prospectId}` - Evaluate prospect
- `GET /api/prospectidentification` - Get prospects (with optional filters)
- `POST /api/prospectidentification` - Create prospect
- `POST /api/prospectidentification/rank` - Rank prospects

### Enhanced Recovery (`/api/enhancedrecovery`)
- `POST /api/enhancedrecovery/analyze-eor` - Analyze EOR potential
- `POST /api/enhancedrecovery/recovery-factor` - Calculate recovery factor
- `POST /api/enhancedrecovery/injection` - Manage injection

### Lease Acquisition (`/api/leaseacquisition`)
- `GET /api/leaseacquisition/evaluate/{leaseId}` - Evaluate lease
- `GET /api/leaseacquisition/available` - Get available leases
- `POST /api/leaseacquisition` - Create lease acquisition
- `PUT /api/leaseacquisition/{leaseId}/status` - Update lease status

### Drilling Operation (`/api/drillingoperation`)
- `GET /api/drillingoperation/operations` - Get drilling operations
- `GET /api/drillingoperation/operations/{operationId}` - Get drilling operation
- `POST /api/drillingoperation/operations` - Create drilling operation
- `PUT /api/drillingoperation/operations/{operationId}` - Update drilling operation
- `GET /api/drillingoperation/operations/{operationId}/reports` - Get drilling reports
- `POST /api/drillingoperation/operations/{operationId}/reports` - Create drilling report

---

## Pump Services

### Hydraulic Pump (`/api/hydraulicpump`)
- `POST /api/hydraulicpump/design` - Design pump system
- `POST /api/hydraulicpump/analyze-performance` - Analyze pump performance
- `POST /api/hydraulicpump/design/save` - Save pump design
- `GET /api/hydraulicpump/performance-history/{pumpId}` - Get performance history

### Plunger Lift (`/api/plungerlift`)
- `POST /api/plungerlift/design` - Design plunger lift system
- `POST /api/plungerlift/analyze-performance` - Analyze performance
- `POST /api/plungerlift/design/save` - Save plunger lift design

### Sucker Rod Pumping (`/api/suckerrodpumping`)
- `POST /api/suckerrodpumping/design` - Design pump system
- `POST /api/suckerrodpumping/analyze-performance` - Analyze performance
- `POST /api/suckerrodpumping/design/save` - Save pump design

---

## Production Services

### Production Operations (`/api/production/operations`)
Active contract routes (implemented service members):
- `POST /api/production/operations/create` - Create production operation cost record
- `GET /api/production/operations/{operationId}` - Get production operation cost record
- `PUT /api/production/operations/{operationId}` - Update production operation cost record
- `GET /api/production/operations/data` - Get production data (well/field scoped query)
- `POST /api/production/operations/data` - Record production data
- `POST /api/production/operations/optimize` - Generate optimization recommendations (heuristic)

Compatibility routes (legacy adapter surface):
- `POST /api/productionoperations/create` - Legacy create operation route (mapped to management service)
- `GET /api/production/data/{wellId}` - Legacy production data route
- `POST /api/production/history/{wellId}` - Legacy production history route
- `POST /api/production/record` - Legacy production record route

Staged local interface members are intentionally not exposed as active API routes until implementation is completed.

### Facility Monitoring (`/api/facility/{facilityId}/monitoring`)
Vertical facility/equipment tracking for measurements (for example tank levels) and installation lifecycle history.
- `GET /api/facility/{facilityId}/monitoring/measurements` - List facility/equipment measurements (optional filters: `facilityType`, `equipmentId`, `measurementType`, `startDate`, `endDate`)
- `POST /api/facility/{facilityId}/monitoring/measurements` - Record a facility/equipment measurement (`FACILITY_MEASUREMENT`)
- `GET /api/facility/{facilityId}/monitoring/equipment/{equipmentId}/activity` - List equipment install/uninstall/move/replace history
- `POST /api/facility/{facilityId}/monitoring/equipment/{equipmentId}/activity` - Record equipment lifecycle activity (`FACILITY_EQUIPMENT_ACTIVITY`)

---

## Lifecycle Services

### Field Orchestrator (`/api/field`)
- `GET /api/field/fields` - Get all fields
- `GET /api/field/current` - Get current active field
- `POST /api/field/set-active` - Set active field
- `GET /api/field/current/dashboard` - Get field dashboard
- `GET /api/field/current/summary` - Get field lifecycle summary
- `GET /api/field/current/wells` - Get field wells
- `GET /api/field/current/statistics` - Get field statistics
- `GET /api/field/current/timeline` - Get field timeline

### Field Exploration — current field (`/api/field/current/exploration`)
Requires active field (`FieldOrchestrator`). Workflow bodies use `ExplorationWorkflowStepRequest` (`InstanceId`, `UserId`, optional `StepData`) where noted. Step routes return `200 OK` with a boolean payload (`true`/`false`) for service result. **Exploration workflow** maturation POSTs return **409 Conflict** if a prior seeded step is still **PENDING** (out-of-order call); body includes `InstanceId`, `attemptedStep`, `prerequisiteStep`.

**Lead → prospect**
- `POST /api/field/current/exploration/workflows/lead-to-prospect` - Start workflow
- `POST /api/field/current/exploration/workflows/evaluate-lead` - `LEAD_EVALUATION`
- `POST /api/field/current/exploration/workflows/approve-lead` - `LEAD_APPROVAL` (approve)
- `POST /api/field/current/exploration/workflows/reject-lead` - `LEAD_APPROVAL` (reject)
- `POST /api/field/current/exploration/workflows/promote-lead-to-prospect` - `PROSPECT_CREATION` + `ILeadExplorationService` persistence

**Exploration workflow**
- `POST /api/field/current/exploration/workflows/prospect-to-discovery` - Start workflow (prospect must be in current field)
- `POST /api/field/current/exploration/workflows/prospect-to-discovery/prospect-readiness` - First seed step `PROSPECT_CREATION` (prospect-anchored only; no lead hook; can return `200 false` if execution/completion is declined)
- `POST /api/field/current/exploration/workflows/prospect-to-discovery/risk-assessment` - `RISK_ASSESSMENT`
- `POST /api/field/current/exploration/workflows/prospect-to-discovery/volume-estimation` - `VOLUME_ESTIMATION`
- `POST /api/field/current/exploration/workflows/prospect-to-discovery/economic-evaluation` - `ECONOMIC_EVALUATION`
- `POST /api/field/current/exploration/workflows/prospect-to-discovery/drilling-decision` - `DRILLING_DECISION`
- `POST /api/field/current/exploration/workflows/prospect-to-discovery/discovery-recording` - `DISCOVERY_RECORDING`

**Discovery → development**
- `POST /api/field/current/exploration/workflows/discovery-to-development` - Start workflow
- `POST /api/field/current/exploration/workflows/discovery-to-development/appraisal` - `APPRAISAL`
- `POST /api/field/current/exploration/workflows/discovery-to-development/reserve-estimation` - `RESERVE_ESTIMATION`
- `POST /api/field/current/exploration/workflows/discovery-to-development/economic-analysis` - `ECONOMIC_ANALYSIS`
- `POST /api/field/current/exploration/workflows/discovery-to-development/approve` - `DEVELOPMENT_APPROVAL`

### Work Orders (`/api/lifecycle/workorders`)
- `GET /api/lifecycle/workorders` - Get work orders
- `POST /api/lifecycle/workorders` - Create work order
- `GET /api/lifecycle/workorders/{workOrderId}` - Get work order
- `PUT /api/lifecycle/workorders/{workOrderId}` - Update work order
- `POST /api/lifecycle/workorders/{workOrderId}/afe` - Create or link AFE
- `POST /api/lifecycle/workorders/{workOrderId}/costs` - Record work order cost
- `GET /api/lifecycle/workorders/{workOrderId}/afe` - Get AFE for work order

---

## Accounting Services

### Royalty (`/api/accounting/royalty`)
Active service-backed routes:
- `POST /api/accounting/royalty/service/calculate` - Calculate royalties through `IRoyaltyService`
- `GET /api/accounting/royalty/service/calculations/{id}` - Get royalty calculation by id through `IRoyaltyService`
- `GET /api/accounting/royalty/service/calculations/by-allocation/{allocationId}` - Get royalty calculations by allocation
- `POST /api/accounting/royalty/service/payments` - Record royalty payment from an existing calculation

Compatibility/legacy routes:
- `POST /api/accounting/royalty/calculate` - Legacy royalty calculation route
- `GET /api/accounting/royalty/calculations` - Legacy royalty listing/query route

### Cost Allocation (`/api/accounting/cost-allocation`)
Active service-backed routes:
- `POST /api/accounting/cost-allocation/service/allocate` - Allocate production through `IAllocationService`
- `GET /api/accounting/cost-allocation/service/{id}` - Get allocation result by id
- `GET /api/accounting/cost-allocation/service/{id}/details` - Get allocation details by result id
- `POST /api/accounting/cost-allocation/service/{id}/reverse` - Reverse an allocation result

Compatibility/legacy routes:
- `POST /api/accounting/cost-allocation/allocate` - Legacy allocation route
- `GET /api/accounting/cost-allocation/allocations` - Legacy allocation listing route

### Reporting (`/api/accounting/reporting`)
Active service-backed routes:
- `POST /api/accounting/reporting/operational` - Generate operational report through `IReportingService`
- `POST /api/accounting/reporting/financial` - Generate financial report through `IReportingService`
- `POST /api/accounting/reporting/royalty-statement` - Generate royalty statement through `IReportingService`
- `POST /api/accounting/reporting/jib-statement` - Generate JIB statement through `IReportingService`
- `POST /api/accounting/reporting/schedule` - Create report schedule through `IReportingService`
- `GET /api/accounting/reporting/schedules` - List report schedules
- `POST /api/accounting/reporting/distribute` - Distribute report through `IReportingService`
- `GET /api/accounting/reporting/history` - Report generation/distribution history

Compatibility/legacy routes:
- Existing lease-report manager paths remain for backward compatibility and staged migration.

### AFE (`/api/accounting/afe`)
Active service-backed routes:
- `POST /api/accounting/afe` - Create AFE through `IAfeService`
- `POST /api/accounting/afe/{id}/approve` - Approve AFE through `IAfeService`
- `POST /api/accounting/afe/{id}/line-items` - Add AFE line item
- `GET /api/accounting/afe/{id}/line-items` - List AFE line items
- `POST /api/accounting/afe/{id}/costs` - Record AFE cost
- `POST /api/accounting/afe/{id}/variance-report` - Generate variance report
- `GET /api/accounting/afe/variance-reports` - Query variance reports

Compatibility/legacy routes:
- Existing AFE list/get and compatibility routes remain available while clients migrate.

### Production Tax (`/api/accounting/tax/production`)
Active service-backed routes:
- `POST /api/accounting/tax/production/calculate` - Calculate production taxes through `IProductionTaxService`

### Run Tickets / Production Accounting (`/api/accounting/runticket`)
Active service-backed routes:
- `POST /api/accounting/runticket/service/process-cycle` - Run orchestration workflow through `IProductionAccountingService`
- `GET /api/accounting/runticket/service/accounting-status/{fieldId}` - Get accounting status
- `GET /api/accounting/runticket/service/revenue-transactions/{fieldId}` - Get revenue transactions

Compatibility/legacy routes:
- Existing run ticket CRUD and manager-based accounting routes remain available.

### Revenue (`/api/accounting/revenue`)
Active service-backed routes:
- `POST /api/accounting/revenue/service/recognize` - Recognize revenue through `IRevenueService`
- `POST /api/accounting/revenue/service/validate` - Validate revenue allocation through `IRevenueService`

Compatibility/legacy routes:
- Existing revenue creation route remains available for staged migration.

### Inventory (Traditional) (`/api/accounting/traditional/inventory`)
Active service-backed routes:
- `POST /api/accounting/traditional/inventory/service/{tankId}/update` - Update tank inventory
- `GET /api/accounting/traditional/inventory/service/{tankId}` - Get tank inventory
- `POST /api/accounting/traditional/inventory/service/validate` - Validate inventory payload
- `POST /api/accounting/traditional/inventory/service/{inventoryItemId}/valuation` - Calculate inventory valuation
- `POST /api/accounting/traditional/inventory/service/{inventoryItemId}/reconciliation-report` - Generate reconciliation report

Compatibility/legacy routes:
- Existing manager-backed traditional inventory routes remain available.

### Storage (`/api/accounting/storage`)
Active service-backed routes:
- `POST /api/accounting/storage/service/tanks/{tankId}/update` - Update storage tank inventory
- `GET /api/accounting/storage/service/tanks/{tankId}` - Get storage tank inventory
- `POST /api/accounting/storage/service/inventory/{inventoryItemId}/valuation` - Calculate storage inventory valuation
- `POST /api/accounting/storage/service/inventory/{inventoryItemId}/reconciliation-report` - Generate storage reconciliation report

Compatibility/legacy routes:
- Existing storage facility manager routes remain available.

### Royalty Disputes (`/api/accounting/royalty-dispute`)
Active service-backed routes:
- `POST /api/accounting/royalty-dispute/create` - Create royalty dispute through `IRoyaltyDisputeService`
- `POST /api/accounting/royalty-dispute/{id}/resolve` - Resolve royalty dispute through `IRoyaltyDisputeService`
- `GET /api/accounting/royalty-dispute` - List royalty disputes

Staged note:
- Service-backed accounting routes are the preferred contract for new clients.
- Compatibility/legacy routes remain intentionally available during staged client migration.

### Volume Reconciliation (`/api/accounting/volume-reconciliation`)
- `POST /api/accounting/volume-reconciliation/reconcile` - Reconcile volumes

---

## Data Management Services

### Connection Management (`/api/connection`)
- `GET /api/connection/connections` - Get available connections
- `POST /api/connection/test` - Test connection
- `POST /api/connection/set-active` - Set active connection

### PPDM39 Data Management (`/api/ppdm39`)
- Various endpoints for PPDM39 data operations, validation, versioning, etc.

### Demo Database (`/api/demodatabase`)
- `POST /api/demodatabase/create` - Create demo database
- `POST /api/demodatabase/seed` - Seed demo database

---

## Common Request/Response Patterns

### Request Headers
- `Authorization: Bearer {token}` - Required for authenticated endpoints
- `Content-Type: application/json` - For POST/PUT requests

### Response Format
- Success: `200 OK` with JSON body
- Created: `201 Created` with JSON body
- Error: `400 Bad Request`, `404 Not Found`, `500 Internal Server Error` with error object:
  ```json
  {
    "error": "Error message",
    "details": { ... }
  }
  ```

### User ID Parameter
Many endpoints accept an optional `userId` query parameter. If not provided, the system extracts it from the authenticated user's claims.

---

## Notes

- All endpoints use camelCase for JSON properties
- All date/time values should be in ISO 8601 format
- All decimal values should use appropriate precision for oil & gas calculations
- Well UWIs should follow standard industry formats
- Field IDs and other identifiers are typically GUIDs or custom identifiers

