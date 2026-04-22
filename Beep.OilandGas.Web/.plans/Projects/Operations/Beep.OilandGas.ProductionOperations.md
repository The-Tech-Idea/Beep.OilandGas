# Beep.OilandGas.ProductionOperations

## Snapshot

- Category: Operations
- Scan depth: Medium
- Current role: production-operations domain slice
- Maturity signal: thin service layer with API and web presence

## Observed Structure

- Top-level folders: `Services`
- Service set is small: `ProductionOperationsService`, `ProductionManagementService`, and related interfaces
- The project does not show the same breadth of models and helpers seen in stronger domain slices

## Representative Evidence

- Services: `Services/ProductionOperationsService.cs`, `Services/ProductionManagementService.cs`, `Services/IProductionOperationsService.cs`
- API surfacing: `Beep.OilandGas.ApiService/Controllers/Production/ProductionOperationsController.cs`
- Web surfacing: `Beep.OilandGas.Web/Pages/PPDM39/Production/ProductionDashboard.razor`, `InterventionDecisions.razor`, `WellPerformanceOptimizer.razor`

## Planning Notes

- This project needs maturity work in phase 8, not just connection cleanup.
- Workflows like interventions, deferments, and late-life triggers should not be assumed complete based only on the web/API presence.
- The web/API slice now covers two real downstream seams: intervention -> work order -> AFE and intervention -> decommissioning / P&A execution, which reduces the remaining Phase 8 gap to deeper domain maturity rather than missing workflow launch points.
- The generic production client compatibility seam is now partially hardened as well: `ProductionOperationsController` exposes repository-backed `PRODUCTION_COSTS` create/get/update routes that match `Beep.OilandGas.Client`, and the `/api/production/operations/optimize` endpoint now derives recommendations from live `PDEN_VOL_SUMMARY` history instead of returning a canned choke-adjustment record.
- That same compatibility surface now also includes absolute-route shims for `/api/production/data/{wellId}`, `/api/production/history/{wellId}`, and `/api/production/record`; they map the client's `PRODUCTION_ALLOCATION` contract onto the existing `ProductionData`/`PDEN_VOL_SUMMARY` seam so the client no longer points at missing generic production routes.
- The older generic `OperationsService.CreateProductionOperationAsync` path is now live too: `/api/productionoperations/create` is a compatibility route over `ProductionManagementService` that persists a real `PDEN` operation and maps the legacy `ProductionOperation` response back from that row instead of trying to reuse the separate `PRODUCTION_COSTS` seam.
