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

### Production Operations (`/api/productionoperations`)
- `GET /api/productionoperations/data` - Get production data
- `POST /api/productionoperations/data` - Record production data

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
- `POST /api/accounting/royalty/calculate` - Calculate royalties
- `GET /api/accounting/royalty/calculations` - Get royalty calculations

### Cost Allocation (`/api/accounting/cost-allocation`)
- `POST /api/accounting/cost-allocation/allocate` - Allocate costs
- `GET /api/accounting/cost-allocation/allocations` - Get cost allocations

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

