# Phase 8 Pass A — Domain Boundary and Project Ownership

## Objective

Freeze the ownership model for operational workflows across the entire Beep.OilandGas solution before vertical-slice completion work starts.

---

## Whole-Solution Workstreams

| Workstream | Project Groups | Output |
|------------|----------------|--------|
| Presentation boundary map | Web, ApiService, Client, Models | page-to-client-to-controller ownership map |
| Persistence and contract map | PPDM39, PPDM39.DataManagement, PPDM.Models, DataManager | field-scoped persistence/service ownership map |
| Operational domain ownership | ProspectIdentification, DevelopmentPlanning, ProductionOperations, LifeCycle, LeaseAcquisition, EnhancedRecovery, Decommissioning, DrillingAndConstruction | one owning domain service per workflow area |
| Supporting domain alignment | Accounting, ProductionAccounting, PermitsAndApplications, PDF rendering, UserManagement, Branchs, Drawing | support-role map and dependency list |
| Engineering support alignment | all calculation/engineering modules | which modules remain supporting-only vs which surface directly in operational flows |

---

## Required Outputs

1. Domain-specific typed client map.
2. Field-scoped API ownership matrix.
3. Workflow ownership list for exploration, development, production, lease, EOR, and decommissioning.
4. Support-domain dependency map for finance, permits, security, and visualization.
5. Engineering-support map showing which calculations participate in operational handoffs.

---

## Exit Gate

No operational page or controller should have ambiguous ownership after this pass. Every flow should name its page owner, client owner, controller owner, and domain-service owner.

---

## Current Checkpoint

- Exploration is the first frozen boundary: `Beep.OilandGas.Web/Services/IExplorationServiceClient.cs` now owns the web client seam for the exploration dashboard, prospect board, prospect list, and prospect detail pages.
- Exploration now extends that boundary across the remaining exploration surfaces: `SeismicTracker.razor`, `SeismicSurveys.razor`, `SeismicLines.razor`, and `WellProgramApproval.razor` now consume the exploration client rather than page-level exploration API wiring.
- Development is now the second frozen boundary: `Beep.OilandGas.Web/Services/IDevelopmentServiceClient.cs` owns the web client seam for `DevDashboard.razor`, `ConstructionProgress.razor`, `FDPWizard.razor`, `WellDesign.razor`, `Pools.razor`, and `Facilities.razor`.
- Production is now the third frozen workflow boundary: `Beep.OilandGas.Web/Services/IProductionServiceClient.cs` owns the web client seam for `ProductionDashboard.razor`, `InterventionDecisions.razor`, `WellPerformanceOptimizer.razor`, `AllocationWorkbench.razor`, and `WellTests.razor`.
- Production now extends that boundary across the browse/list surfaces as well: `Fields.razor`, `Pools.razor`, `Reserves.razor`, `Reporting.razor`, and `Forecasts.razor` now use the same typed production client instead of page-local production endpoint calls and reflection-based PPDM rendering.
- `ProductionForecasting.razor` is now frozen as a calculations-owned support workbench rather than a production-operations page: it generates and saves through `ICalculationServiceClient`, while `IDevelopmentServiceClient` and `IProductionServiceClient` only provide supporting field/well context.
- Lease acquisition is now the fourth frozen boundary: `Beep.OilandGas.Web/Services/ILeaseServiceClient.cs` owns the web client seam for `LeaseAcquisition.razor` instead of routing that page through the shared operations client.
- Enhanced recovery is now the fifth frozen boundary: `Beep.OilandGas.Web/Services/IEnhancedRecoveryServiceClient.cs` owns the web client seam for `EnhancedRecovery.razor` instead of routing that page through the shared operations client.
- Decommissioning is now the sixth frozen boundary: `Beep.OilandGas.Web/Services/IDecommissioningServiceClient.cs` owns the field-scoped web client seam for `WellPAWorkflow.razor` and `FacilityDecommissioning.razor`.
- Work order execution is now the seventh frozen boundary: `Beep.OilandGas.Web/Services/IWorkOrderServiceClient.cs` owns the web client seam for `WorkOrderDashboard.razor`, `WorkOrderDetail.razor`, and `WorkOrderCalendar.razor` instead of leaving those pages split between raw work-order routes and the lifecycle service.
- `Beep.OilandGas.ApiService/Controllers/Field/ExplorationController.cs` now exposes real field-scoped prospect create/update/delete/list/detail routes plus seismic survey create/list, seismic line list, and prospect-linked AFE line routes, so exploration pages no longer depend on missing update/delete routes, a missing seismic create route, or a mixed raw-API/operations-client pattern for core exploration actions.
- `Beep.OilandGas.ApiService/Controllers/Field/DevelopmentController.cs` now exposes the missing field-scoped pool list/create/update and facility create/update routes, closing the gap where development pages were already trying to call routes the controller did not actually own.
- `Beep.OilandGas.ApiService/Controllers/Field/ProductionController.cs` now exposes the missing field-scoped well-tests route and now returns real rate data from well-test history on the well-performance route, which closes the gap where production workflow pages were hitting zeroed metrics or falling back to the non-field production controller.
- `Beep.OilandGas.LifeCycle/Services/Exploration/PPDMExplorationService.cs` now links created seismic surveys back to the active field through `AREA_ID` + `AREA_TYPE`, matching the existing read path.
- The exploration update seam is now real end to end: board-stage advancement and prospect edits flow through the exploration client into the field-scoped controller and merge onto existing `PROSPECT` rows rather than replacing them with partial DTO data.
- The development update seam is now also real end to end: pool and facility edits flow through the development client into the field-scoped controller and merge onto existing `POOL` and `FACILITY` rows rather than replacing them with partial DTO data.
- The production update seam is now real end to end for the core workflow pages: dashboard loads, intervention decisions, allocation patches, optimizer analysis, and well-test review all flow through the production client into the field-scoped controller instead of page-level production API calls.
- The production forecasting generate path now uses the typed production-forecasting request contract with a `ForecastType` enum, eliminating the earlier web-to-controller request-shape mismatch.
- The lease and EOR update seams are now explicit at the page boundary: `LeaseAcquisition.razor` and `EnhancedRecovery.razor` call dedicated domain clients even though their existing API routes remain under the operations controllers.
- The lease boundary now also has an explicit downstream workflow seam: `LeaseAcquisition.razor` hands users into `HSE/ComplianceCalendar.razor`, `ObligationDetail.razor` owns the continue/back navigation, and `FDPWizard.razor` receives the regulatory prefill instead of sending the workflow into the dead permits route.
- The decommissioning update seam is now explicit at the page boundary as well: the decommissioning pages load shared response contracts through the typed decommissioning client, and the P&A approval action now calls the field-scoped approval route using the abandonment record id rather than the displayed well id.
- The work-order ownership split is now explicit end to end: `Controllers/WorkOrder/WorkOrderController.cs` owns the field-scoped AFE link endpoints used by the work-order pages, and `WorkOrderAccountingService` now resolves and persists the `AFE_ID:` marker across both legacy `WORK_ORDER` rows and the PROJECT-backed field work orders used by the active work-order workflow, so create/link and lookup use the same source of truth.
- The first Phase 8 Pass C handoff is now live on the production seam: approved intervention decisions create a planned corrective work order, optionally link or create an AFE, persist those downstream identifiers into the decision activity remark, and return them to `InterventionDecisions.razor` for direct navigation into the next workflow step.
- The well-program approval boundary now also covers cost-estimate intake: the exploration client can resolve live AFE line items for the selected prospect through the field-scoped controller, which aggregates prospect-linked accounting records without reintroducing page-level accounting API calls.
- The development field-link rules are now explicit at the service boundary: `WELL` reads use `ASSIGNED_FIELD` + `CURRENT_CLASS`, and `FACILITY` reads use `PRIMARY_FIELD_ID`, which removes the prior mismatch between PPDM ownership and the web/API contract.
