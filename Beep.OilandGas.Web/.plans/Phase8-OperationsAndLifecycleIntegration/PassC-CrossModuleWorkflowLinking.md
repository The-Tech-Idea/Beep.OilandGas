# Phase 8 Pass C — Cross-Module Workflow Linking

## Objective

Link the operational vertical slices so business handoffs become first-class workflows instead of manual page-to-page transitions.

---

## Handoff Map

| Handoff | Participating Projects | Result |
|---------|------------------------|--------|
| Prospect -> development concept -> economics | ProspectIdentification, DevelopmentPlanning, EconomicAnalysis, LifeCycle, Web, ApiService | screened prospects become development candidates with economics context |
| Development plan -> permits -> drilling/construction | DevelopmentPlanning, PermitsAndApplications, DrillingAndConstruction, ApiService, Web | development planning can open compliance/permit planning and drilling workflows directly |
| Production issue -> engineering analysis -> intervention -> work order | ProductionOperations, NodalAnalysis, WellTestAnalysis, DCA, GasLift, Pump modules, LifeCycle, ApiService, Web | operational issues can trigger engineering recommendations and actionable work |
| EOR screening -> pilot proposal -> economics and approvals | EnhancedRecovery, EconomicAnalysis, Accounting, LifeCycle, Web, ApiService | EOR candidates become reviewable projects with approval context |
| Late-life trigger -> decommissioning planning -> compliance and accounting | ProductionOperations, Decommissioning, PermitsAndApplications, ProductionAccounting, Accounting, LifeCycle | decommissioning starts from operational state and lands in governed financial/compliance flows |
| Workflow state -> dashboards and navigation | LifeCycle, Branchs, Web, Drawing, ApiService | dashboards and navigation reflect real lifecycle transitions |

---

## Required Outputs

1. Explicit workflow transition actions between slices.
2. Stable handoff payloads across modules.
3. Lifecycle-aware dashboards and navigation.
4. Documented exceptions where a handoff remains manual for now.

---

## Current Progress

- Intervention -> work order -> AFE is live through the field production controller, work-order controller, accounting linkage service, and the intervention/work-order web pages.
- Lease acquisition -> compliance -> development planning is live through prefilled obligation intake, obligation-detail continuation, and FDP regulatory prefill.
- The lease create seam inside that flow is no longer placeholder-grade: `LeaseAcquisition.razor` now opens a real create dialog and links the new acquisition to the active field instead of inventing a GUID-backed property id and posting canned lease defaults.
- The remaining lease backend seam is also closed for create/list/status: `LeaseAcquisitionService.ModelsCoreImpl.cs` now writes `LAND_RIGHT` / `LAND_AGREEMENT` / `LAND_STATUS`, returns summaries from saved PPDM lease fields, and keeps lease active indicators plus status history aligned when the workflow updates status.
- The lease read surface is now aligned with that backend seam instead of staying global/minimal: `LeaseAcquisition.razor` loads by the active field id, surfaces persisted lease type/date/term/working-interest metadata, and collects the create inputs that this PPDM-backed slice can actually store.
- Development planning -> drilling/construction is now live through `WellDesign.razor` handing off into both `ConstructionProgress.razor` and `DrillingOperations.razor`; construction now preserves well handoff context and can launch drilling with nested return navigation back through construction to the selected well design.
- The drilling execution slice now also runs on a field-scoped API seam: `DrillingServiceClient` targets `Controllers/Field/DrillingController.cs`, and the drilling service validates the handed-off well against `ASSIGNED_FIELD` when the active-field route is used.
- The drilling persistence seam is no longer status-only: operation updates now round-trip current depth and completion state on `WELL`, create flow can persist handed-off well name plus rig/contractor metadata, and the web page has dropped the unsupported create-time daily-cost field instead of collecting data that this slice could not store.
- The drilling page itself is no longer placeholder-grade for its surfaced actions: operation status updates run through a dialog-backed update path, drilling reports open in-page for review, and new report logging persists remarks/dates through the drilling service instead of stopping at a count-only snackbar.
- The generic drilling controller is now a deliberate compatibility seam rather than an accidental duplicate: the shared client package translates its older `DrillingOperation` contract onto `/api/drillingoperation/operations`, while the active web workflow continues to use the field-scoped drilling client/controller path.
- EOR screening -> pilot economics -> economic evaluation is now live through the shared EOR economics contract, the enhanced-recovery API/client seam, pilot-economics KPIs on the EOR page, and a prefilled economic-evaluation handoff with return navigation.
- The remaining thin EOR placeholder seam was removed: recovery-factor execution now requires a real analyzed EOR operation and posts that `OperationId` through the enhanced-recovery contract instead of fabricating a project GUID in the page.
- The EOR injection action is now on the same honest boundary: `EnhancedRecoveryService` persists the injection operation on `PDEN`, round-trips its rate through `PDEN_FLOW_MEASUREMENT`, the controller/client expose a stored injection read path, and `EnhancedRecovery.razor` surfaces the saved injection state instead of treating injection as a write-only snackbar action.
- The production optimizer now sits on the same honest field seam: `WellPerformanceOptimizer.razor` pulls findings, recommendation, and history from a dedicated field-analysis route backed by `PPDMProductionService`, and its deviation action now persists a `PERFORMANCE_DEVIATION` `WELL_ACTIVITY` row that can flow into downstream intervention review instead of stopping at a local toast.
- Production intervention -> decommissioning / P&A execution is now live through the field production controller handoff endpoint, production transfer markers, and query-based navigation into the decommissioning P&A and abandonment pages.
- Decommissioning / P&A execution -> compliance and closeout finance intake is now live through prefilled compliance-obligation intake, prefilled AFE creation, and return navigation back into the active decommissioning workflow.
- The late-life trigger chain now carries through production -> decommissioning -> compliance / finance follow-on, and the previously thin development -> drilling seam now runs through a real construction-progress checkpoint as well; remaining Pass C work is limited to optional hardening inside thinner operational pages.

---

## Exit Gate

Operational flows should now cross module boundaries intentionally, with clear state transfer and traceable ownership.
