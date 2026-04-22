# Phase 7 Pass C — Advanced Operations Composition

## Objective

Move from isolated calculation pages to compound engineering workflows that support real production and intervention decisions.

---

## Modules In Scope

- Choke Analysis
- Compressor Analysis
- multi-tool artificial-lift recommendations
- production optimization panels that combine DCA, nodal, well test, and properties

---

## Workstreams

| Workstream | Scope | Result |
|------------|-------|--------|
| Advanced API surfaces | choke, compressor, composite scenario endpoints | higher-order engineering APIs |
| Shared comparison UX | scenario compare, result overlays, action recommendations | reusable advanced workbench components |
| Workflow handoffs | work orders, AFEs, intervention actions | engineering results drive business actions |
| Cross-tool consistency | calculation naming, units, save/export contracts | stable multi-tool user experience |

---

## Exit Gate

The calculation stack should now support both standalone engineering analysis and embedded operational decision support.

---

## Current Milestone

Implemented first Pass C lift-composition slice:

- `Components/Shared/LiftRecommendationCard.razor` now provides a reusable recommendation shell for artificial-lift workflow handoff.
- `Pages/PPDM39/Calculations/PumpPerformance.razor` now hands selected wells into `WellPerformanceOptimizer` and surfaces an operations-facing ESP recommendation.
- `Pages/PPDM39/Calculations/GasLift.razor` now stores and surfaces the returned gas-lift potential/design results instead of dropping them, and can hand the selected well into `WellPerformanceOptimizer`.
- `Pages/PPDM39/Production/WellPerformanceOptimizer.razor` now uses the shared recommendation shell and can route directly into the ESP or Gas Lift workbench based on live well findings.

Implemented follow-on lift-workbench coverage:

- `Pages/PPDM39/Pumps/SuckerRodPumping.razor` now accepts query-selected well context, carries design output forward into performance validation, and surfaces a shared SRP recommendation card with optimizer handoff.
- `Pages/PPDM39/Pumps/HydraulicPump.razor` now loads active-field wells, prefers the query-selected well when present, carries design output into performance validation, and surfaces a shared hydraulic-pump recommendation card with optimizer handoff.
- `Pages/PPDM39/Pumps/PlungerLift.razor` now accepts query-selected well context, keeps the performance well aligned with the design well, and surfaces a shared plunger-lift recommendation card with optimizer handoff.

Implemented operations follow-up intake handoff:

- `Pages/PPDM39/Production/WellPerformanceOptimizer.razor` now opens prefilled intervention, work-order, and AFE intake routes when a recommendation has material uplift.
- `Pages/PPDM39/WorkOrder/WorkOrderDashboard.razor` now accepts query-supplied well, subtype, title, and description context so recommendation-driven work orders land in the live create dialog instead of a blank dashboard.
- `Pages/PPDM39/Economics/AFEManagement.razor` now accepts query-supplied well, description, and budget context so recommendation-driven AFE creation opens with the relevant well already attached.
- `Pages/PPDM39/Production/InterventionDecisions.razor` now routes manual intervention intake into the live work-order create path instead of a snackbar stub.

Implemented lifecycle follow-through:

- `Pages/PPDM39/Production/WellPerformanceOptimizer.razor` now tags recommendation-driven work-order intake so the resulting work order can immediately attempt AFE linkage through the lifecycle API.
- `Pages/PPDM39/WorkOrder/WorkOrderDashboard.razor` now calls `ILifeCycleService.CreateOrLinkAFEAsync` after creating recommendation-driven work orders and routes users straight into the created work-order detail page.
- `Pages/PPDM39/WorkOrder/WorkOrderDetail.razor` now loads linked AFE state, allows on-demand create/link refresh, and can open the linked AFE in the accounting view.
- `Pages/PPDM39/Economics/AFEManagement.razor` now accepts an `afeNumber` query and opens directly to an existing linked AFE.

Implemented return-flow polish:

- `Pages/PPDM39/Production/WellPerformanceOptimizer.razor` now stamps intervention, work-order, and AFE handoffs with a `returnTo` route back into the selected optimizer context.
- `Pages/PPDM39/Production/InterventionDecisions.razor`, `Pages/PPDM39/WorkOrder/WorkOrderDashboard.razor`, `Pages/PPDM39/WorkOrder/WorkOrderDetail.razor`, and `Pages/PPDM39/Economics/AFEManagement.razor` now expose a direct return path back to the optimizer instead of leaving users in a dead-end operational branch.

Implemented comparison/export completion slice:

- `Services/ClientFileExportService.cs` now provides a shared client-side JSON/text download path over the existing `wwwroot/js/fileDownload.js` helper.
- `Pages/PPDM39/Production/WellPerformanceOptimizer.razor` now exports the active well-performance recommendation package, including findings, recommended actions, and workflow follow-up routes.
- `Pages/PPDM39/Production/ProductionForecasting.razor`, `Pages/PPDM39/Economics/EconomicEvaluation.razor`, and `Pages/PPDM39/CompareWells.razor` now export structured comparison packages for scenario and well-comparison review.

Pass C is complete for the current Phase 7 scope.
