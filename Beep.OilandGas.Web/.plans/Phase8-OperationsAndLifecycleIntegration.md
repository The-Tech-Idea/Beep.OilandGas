# Phase 8 — Operations and Lifecycle Integration

> Status: In Progress  
> Depends On: Phases 6 and 7  
> Goal: Integrate the operational domain projects into coherent field-scoped workflows across exploration, development, production, lease, EOR, and decommissioning.
> Detailed subplans: `Phase8-OperationsAndLifecycleIntegration/README.md`

---

## Objective

Connect the business-process pages in the web project to the actual domain services that own those workflows.

This phase is about vertical slices, not only pages:

```
Field context -> page -> typed client -> field-scoped API controller -> domain service -> PPDM39 / process state
```

---

## Domain Groups In Scope

| Domain Project | Web Areas |
|----------------|-----------|
| `ProspectIdentification` | Exploration dashboard, prospect board, prospect detail, seismic tracker |
| `DevelopmentPlanning` | FDP, well design, construction progress, facilities |
| `ProductionOperations` | Daily ops, well performance, intervention decisions, deferments |
| `LifeCycle` | Field dashboard, work orders, process dashboards, field timeline |
| `LeaseAcquisition` | Lease opportunity and due-diligence pages |
| `EnhancedRecovery` | EOR screening and pilot workflows |
| `Decommissioning` | Well abandonment, P&A workflow, facility decommissioning |

---

## Pass Plan

### Pass A — Typed Client Boundary By Domain

- split oversized clients where needed
- introduce clearer domain-specific clients such as `IExplorationServiceClient`, `IDevelopmentServiceClient`, `IDrillingServiceClient`, `IProductionServiceClient`, `IDecommissioningServiceClient`, `ILeaseServiceClient`, and `IEnhancedRecoveryServiceClient`
- keep `ILifeCycleService` for field and orchestration concerns, not every domain action

### Pass B — Vertical Slice Completion

- ensure each key page has a single owning domain client
- tie page actions to real API endpoints, not temporary placeholders or generic data calls
- standardize field context propagation and active-field display

### Pass C — Cross-Module Workflow Linking

Add the business hand-offs that make the platform coherent:

- prospect -> well program -> FDP
- well performance recommendation -> intervention -> AFE -> work order
- EOR screening -> pilot proposal -> economics
- lease acquisition -> permit / compliance -> development planning
- production operations -> decommissioning trigger / lifecycle state

---

## Todo

| ID | Task | Status |
|----|------|--------|
| 8.1 | Define domain-specific web clients for exploration, development, drilling, production, lease, enhanced recovery, and decommissioning | Done |
| 8.2 | Refactor exploration pages to use prospect-identification domain APIs consistently | Done |
| 8.3 | Refactor development pages to use development-planning APIs consistently | Done |
| 8.4 | Refactor production pages to use production-operations APIs consistently | Done |
| 8.5 | Wire lease acquisition and enhanced recovery pages to dedicated clients | Done |
| 8.6 | Wire decommissioning pages to field-scoped decommissioning client | Done |
| 8.7 | Connect cross-domain actions to AFEs, work orders, and business-process state transitions | In Progress |

### Current Checkpoint

- `Beep.OilandGas.Web/Services/IExplorationServiceClient.cs` and `ExplorationServiceClient.cs` now define the first Phase 8 domain-specific web client boundary.
- `Beep.OilandGas.Web/Services/IDevelopmentServiceClient.cs` and `DevelopmentServiceClient.cs` now define the matching development-planning client boundary for the development dashboard, construction progress, FDP wizard, well design, pools, and facilities pages.
- `Beep.OilandGas.Web/Services/IDrillingServiceClient.cs` and `DrillingServiceClient.cs` now define the drilling/construction client boundary for `DrillingOperations.razor`, so drilling actions no longer route through the shared `IOperationsServiceClient`.
- `Beep.OilandGas.Web/Services/IProductionServiceClient.cs` and `ProductionServiceClient.cs` now define the production-operations client boundary for the daily operations dashboard, intervention decisions, well performance optimizer, allocation workbench, and well-tests pages.
- `Beep.OilandGas.Web/Services/IProductionServiceClient.cs` and `ProductionServiceClient.cs` now also own the production field register, pool register, reserves register, reporting register, and forecast-list reads, so those production browse pages no longer call raw production endpoints directly or reflect over typed PPDM entities.
- `Beep.OilandGas.Web/Services/ILeaseServiceClient.cs` and `LeaseServiceClient.cs` now own the lease-acquisition client boundary for `LeaseAcquisition.razor` instead of routing that page through the shared operations client.
- `Beep.OilandGas.Web/Services/IEnhancedRecoveryServiceClient.cs` and `EnhancedRecoveryServiceClient.cs` now own the enhanced-recovery client boundary for `EnhancedRecovery.razor`, leaving the page with only a minimal raw active-field lookup for cosmetic/default field context.
- `Beep.OilandGas.Web/Services/IDecommissioningServiceClient.cs` and `DecommissioningServiceClient.cs` now own the field-scoped decommissioning client boundary for `WellPAWorkflow.razor` and `FacilityDecommissioning.razor`.
- `Beep.OilandGas.Web/Services/IWorkOrderServiceClient.cs` and `WorkOrderServiceClient.cs` now own the work-order client boundary for `WorkOrderDashboard.razor`, `WorkOrderDetail.razor`, and `WorkOrderCalendar.razor` instead of leaving those pages split between raw work-order routes and `ILifeCycleService` AFE-link calls.
- `Beep.OilandGas.ApiService/Controllers/Field/ExplorationController.cs` now owns field-scoped prospect create/update/delete/list/detail routes plus seismic survey create/list, seismic line list, and prospect-linked AFE line routes instead of leaving exploration pages to mix generic CRUD seams and operations-client calls.
- `Beep.OilandGas.ApiService/Controllers/Field/DevelopmentController.cs` now owns field-scoped pool list/create/update and facility create/update routes in addition to the existing development dashboard, well, rig-assignment, construction-progress, and FDP endpoints that the development pages consume.
- `Beep.OilandGas.ApiService/Controllers/Field/DrillingController.cs` now owns the field-scoped drilling operation and drilling-report routes consumed by the web drilling workflow, while the drilling service now filters and validates wells by `ASSIGNED_FIELD` when a field context is supplied.
- `Beep.OilandGas.ApiService/Controllers/Field/ProductionController.cs` now owns the field-scoped dashboard, well-performance, intervention, allocation, and well-tests routes used by the core production workflow pages.
- `ExplorationDashboard.razor`, `ProspectBoard.razor`, `ProspectDetail.razor`, `Prospects.razor`, `SeismicTracker.razor`, `SeismicSurveys.razor`, `SeismicLines.razor`, and `WellProgramApproval.razor` now use the exploration client for their primary exploration data/actions.
- `DevDashboard.razor`, `ConstructionProgress.razor`, `FDPWizard.razor`, `WellDesign.razor`, `Pools.razor`, and `Facilities.razor` now use the development client for their primary development data/actions instead of page-level `ApiClient` calls.
- `ProductionDashboard.razor`, `InterventionDecisions.razor`, `WellPerformanceOptimizer.razor`, `AllocationWorkbench.razor`, and `WellTests.razor` now use the production client for their primary production data/actions, while `AllocationWorkbench.razor` and `WellPerformanceOptimizer.razor` reuse `IDevelopmentServiceClient` for field-scoped well selection.
- `Fields.razor`, `Pools.razor`, `Reserves.razor`, `Reporting.razor`, and `Forecasts.razor` now also use the production client for their primary production data loads, and the PPDM browse pages now bind directly to typed `FIELD`, `POOL`, `RESERVE_ENTITY`, and `PDEN_VOL_SUMMARY` properties instead of runtime reflection helpers.
- `ProductionForecasting.razor` is now explicitly frozen on the calculations forecasting seam: it uses `ICalculationServiceClient` for forecast generate/save, `IDevelopmentServiceClient` for field well selection, and `IProductionServiceClient` for live field production-rate seeding rather than calling raw forecasting endpoints directly.
- `CalculationServiceClient.GenerateForecastAsync` now posts the typed production-forecasting request contract with a real `ForecastType` enum value, which removes the prior string-payload mismatch against `Controllers/Calculations/ProductionForecastingController.cs`.
- `PPDMExplorationService.CreateSeismicSurveyForFieldAsync` now writes the active field linkage through `AREA_ID` + `AREA_TYPE`, which matches the existing field-scoped seismic read path and prevents newly created surveys from disappearing from the active-field view.
- `ProspectBoard.razor` and `Prospects.razor` now persist board-stage transitions and prospect edits through the exploration client instead of mutating local UI state or calling the wrong evaluation action.
- `PPDMExplorationService.UpdateProspectForFieldAsync` now merges request data onto the existing `PROSPECT` row before update, which preserves field linkage and existing values for partial web edits.
- `PPDMDevelopmentService.GetDevelopmentWellsForFieldAsync` and `GetFacilitiesForFieldAsync` now use the correct PPDM field-link columns (`ASSIGNED_FIELD` for `WELL`, `PRIMARY_FIELD_ID` for `FACILITY`), and the pool/facility update seam now merges partial web edits onto existing PPDM rows instead of replacing them with partial request data.
- `Pools.razor` and `Facilities.razor` now render real typed PPDM entities directly instead of treating `POOL` and `FACILITY` rows as dictionary-backed objects, which fixes previously blank `POOL_*` and `FACILITY_*` grid/KPI values on those pages.
- `WellProgramApproval.razor` now refreshes from the selected prospect and resolves live prospect-linked AFE line items through the field-scoped exploration route rather than leaving the cost-estimate step empty.
- `WellTestResponse` now carries PPDM test-rate fields used by the production workflow pages, and the field-scoped well-performance endpoint now derives actual oil, gas, water, and potential-rate values from live well-test history instead of returning zeros.
- `LeaseAcquisition.razor` and `EnhancedRecovery.razor` now use dedicated domain clients rather than the shared operations client, which completes the remaining page-level lease/EOR boundary split without changing their existing API route shape.
- `LeaseAcquisition.razor` now also starts a real downstream handoff into the compliance workflow by deep-linking into `HSE/ComplianceCalendar.razor` with lease context, a permit-review obligation type, and a return path back to the lease workflow.
- `LeaseAcquisition.razor` no longer fabricates a property identifier or posts a hardcoded lease payload when creating a lease; it now requires an active field, opens a real create dialog, and submits only the lease name plus the active field linkage that the current lease-acquisition service actually consumes.
- `LeaseAcquisitionService.ModelsCoreImpl.cs` is no longer an in-memory placeholder seam for create/list/status operations: it now persists `LAND_RIGHT`, `LAND_AGREEMENT`, and initial `LAND_STATUS` rows for new leases, derives `LeaseSummary` values from real PPDM lease metadata instead of agreement type placeholders, and updates both active indicators and status history when lease status changes.
- `LeaseAcquisition.razor`, `LeaseSummary`, and `LeaseAcquisitionService.ModelsCoreImpl.cs` now also round-trip the field-scoped lease metadata already persisted in this slice: the page loads leases by the active field id, the grid shows saved lease type/effective date/expiry/term/working interest, and the create dialog now captures dates, primary term, and working interest that map onto the existing PPDM-backed lease write path.
- `WellPAWorkflow.razor` and `FacilityDecommissioning.razor` now use the shared decommissioning response contracts through the new field-scoped decommissioning client, their KPI cost values now use live or estimated decommissioning data instead of zeros where available, and P&A approval now patches by abandonment record id rather than the well id shown in the grid.
- `Controllers/WorkOrder/WorkOrderController.cs` now owns the field-scoped work-order-to-AFE link routes, and `WorkOrderAccountingService` now persists and resolves the `AFE_ID:` marker across both legacy `WORK_ORDER` rows and the PROJECT-backed field work orders used by the current work-order workflow, which makes work-order AFE linking and lookup consistent regardless of whether the flow starts from optimizer intake, work-order detail, dashboard creation, or intervention approval.
- `Controllers/Field/ProductionController.cs` now turns approved intervention decisions into real downstream workflow records: it creates a corrective work order, advances it to `Planned`, optionally links or creates an AFE through `WorkOrderAccountingService`, records the intervention decision with downstream linkage markers, and returns those identifiers to the web client.
- `Controllers/Field/ProductionController.cs` now also exposes a late-life decommissioning handoff endpoint that creates the starter well-abandonment record, attempts to start the well-abandonment lifecycle process, records the production-side transfer marker, and returns the abandonment/process identifiers to the web client.
- `InterventionDecisions.razor` now submits richer intervention decision context through `IProductionServiceClient`, surfaces any linked downstream work order and AFE on the decision page, and navigates directly into the created work-order detail page with a return path back to the production intervention workflow.
- `InterventionDecisions.razor` now also surfaces any linked P&A programme, can transition a late-life production candidate directly into decommissioning, and deep-links into `WellPAWorkflow.razor` with `wellId`, `abandonmentId`, `processInstanceId`, handoff reason, and return navigation.
- `HSE/ComplianceCalendar.razor` now accepts query-prefilled obligation context, records `CreatedByProcess`, adds a permit / lease regulatory review option, and routes newly created lease obligations into `ObligationDetail.razor` with continuation into development planning.
- `ObligationDetail.razor` now honors `returnTo` and `continueTo` query flow so a created compliance obligation can hand off cleanly into development planning while preserving navigation back to the originating lease workflow.
- `FDPWizard.razor` now accepts regulatory query prefill (`jurisdiction`, `permitList`, `regulatoryNotes`, `returnTo`) and its regulator-tracking action now targets the live compliance page instead of the dead `/ppdm39/permits-applications` route.
- `AnalyzeEOREconomicsRequest`, `IEnhancedRecoveryService`, `EnhancedRecoveryController`, and `EnhancedRecoveryServiceClient` now expose the existing domain EOR economics analysis through a real shared contract and API path instead of leaving pilot economics trapped inside the domain project.
- `EnhancedRecovery.razor` now lets the operator calculate pilot economics from the EOR screen, shows domain economics KPIs, and opens `EconomicEvaluation.razor` through a query-based handoff carrying scenario assumptions and a return path.
- Recovery-factor calculation on `EnhancedRecovery.razor` now reuses the analyzed EOR operation id returned from the domain/API seam instead of generating a fake project GUID; the shared request/client/controller contract accepts `OperationId` and still tolerates the legacy `ProjectId` payload during the transition.
- The remaining EOR injection seam is now persisted and readable instead of write-only: `EnhancedRecoveryService` stores injection operations with the well UWI and field linkage on `PDEN`, persists the current injection rate in `PDEN_FLOW_MEASUREMENT`, the enhanced-recovery controller/client expose stored injection reads, and `EnhancedRecovery.razor` can load and display the saved injection state for the selected well.
- `WellPerformanceOptimizer.razor` no longer owns production diagnosis and deviation handling in Razor alone: the field production controller now exposes a dedicated well-analysis route plus a deviation-log route, `PPDMProductionService` builds findings/recommendation/history from live well tests and `WELL_ACTIVITY`, and logging a deviation now persists a `PERFORMANCE_DEVIATION` activity instead of ending in a snackbar-only action.
- `EconomicEvaluation.razor` now accepts EOR handoff context (`sourceProcess`, `scenarioName`, `eorMethod`, economics assumptions, pilot KPI metrics, `returnTo`), preloads the relevant inputs, and preserves navigation back to the enhanced-recovery workflow.
- `WellPAWorkflow.razor`, `WellAbandonment.razor`, and `WellAbandonmentDialog.razor` now carry production-handoff query context, preserve return navigation into the production decision surface, and bind the abandonment page to real `WellAbandonmentResponse` records instead of deserializing decommissioning responses into `WELL` rows.
- `WellPAWorkflow.razor` and `WellAbandonment.razor` now also launch prefilled closure-compliance intake and closeout AFE creation for the selected abandonment record, while `HSE/ComplianceCalendar.razor` now treats decommissioning as a workflow handoff and routes created obligations into obligation detail with return navigation preserved.
- `WellDesign.razor` now hands off into `/ppdm39/operations/drillingoperations` with prefilled well context and `returnTo`, while `DrillingOperations.razor` now filters by handed-off well UWI, suppresses duplicate starts, and collects real create inputs via a dialog instead of creating placeholder drilling operations.
- `WellDesign.razor` now also routes into `/development/construction` with well handoff context, and `ConstructionProgress.razor` now accepts that query prefill, surfaces the active handoff in-page, and launches drilling with nested `returnTo` navigation back through construction and into the originating well design when present.
- `DrillingServiceClient.cs` now targets the field-scoped drilling controller instead of the generic operations controller, and `DrillingOperationService` now stamps or validates `WELL.ASSIGNED_FIELD` during create/read/update/report flows when the active-field route is used.
- `Beep.OilandGas.Client/App/Services/Operations/OperationsService.cs` now translates its legacy `DrillingOperation` model onto `/api/drillingoperation/operations` and the PPDM drilling contracts, which leaves `Controllers/Operations/DrillingOperationController.cs` as an explicit compatibility seam for shared-client consumers while the web workflow stays on the field-scoped controller.
- `DrillingOperationService` now also persists real operation state back onto PPDM rows instead of only toggling `ACTIVE_IND`: `WELL_NAME`, `CURRENT_STATUS`, `CURRENT_STATUS_DATE`, `DRILL_TD`, and `COMPLETION_DATE` are maintained on `WELL`, while rig/contractor metadata is stored on `WELL_DRILL_REPORT`; the drilling page no longer asks for unsupported create-time daily-cost input that had no backing storage in this slice.
- Drilling write flows no longer stamp repository audit columns as `SYSTEM` when invoked through authenticated controllers; both the field-scoped and legacy drilling controllers now pass the resolved user id into `DrillingOperationService` for create, update, and report insert operations.
- `DrillingOperations.razor` no longer uses toast-only report handling or echo-style edit actions; it now opens dialog-driven status updates and drilling-report list/create flows, and `DrillingOperationService` maps report remarks through the typed contract consistently.
- The domain-client split and work-order ownership cleanup for Phase 8 are complete, and Pass C is now in progress through six implemented linkage slices: development planning -> construction progress -> drilling, intervention -> work order -> AFE, lease acquisition -> compliance -> development planning, EOR screening -> pilot economics -> economic evaluation, production intervention -> decommissioning / P&A execution, and decommissioning / P&A execution -> compliance and closeout finance intake. Remaining unfinished Phase 8 work is now limited to optional polish in thinner operational domains.

---

## Exit Criteria

- each routed operational page has one clear owning domain client
- field-scoped pages consistently use active-field context
- cross-module actions produce real downstream records or workflows
- no operational page depends on ad hoc generic data access when a domain service already exists
