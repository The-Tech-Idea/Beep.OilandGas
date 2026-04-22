# Phase 7 — Calculation Surface Integration

> Status: In Progress  
> Depends On: Phase 6  
> Goal: Surface all engineering and calculation libraries through consistent API-backed workbenches in the web project.
> Detailed subplans: `Phase7-CalculationSurfaceIntegration/README.md`

---

## Objective

Make the calculation stack feel like one platform instead of a mix of isolated pages and direct library calls.

Every calculation vertical should follow this shape:

```
Page -> typed web client -> ApiService controller -> domain calculation service -> result persistence / PPDM linkage
```

Before implementing or normalizing any calculation workbench UI in this phase, read `../MudBlazor_Docs/README.md` and the matching local docs for `Grid`, `Tabs`, `Stepper`, `Dialog`, `DataGrid`, `Button`, and `Progress` patterns.

---

## Calculation Workbench Pattern

Each workbench should contain:

1. field/well/asset selector
2. validated input form
3. run action with snackbar + progress state
4. result summary cards
5. charts / tables / diagnostics tabs
6. save/export/attach-to-workflow actions

Reusable components to standardize during this phase:

- `CalculationInputPanel`
- `CalculationResultSummary`
- `ScenarioCompareGrid`
- `OptimizationDeltaSummary`
- `LiftRecommendationCard`
- `FieldOrWellSelector`
- `SaveResultDialog`

Implemented result-display layer in the current pass:

- `CalculationKpiSection`
- `CalculationFactGrid`
- `CalculationResultTabs`
- `ComparisonGridCard`
- `OptimizationDeltaSummary`
- `LiftRecommendationCard`

---

## Pass Plan

### Pass A — Standardize Existing Surfaces

Focus on modules that already have both API controllers and page presence:

- Economic Analysis
- Production Forecasting
- Nodal Analysis
- Gas Lift
- Pipeline Analysis
- Heat Map
- Sucker Rod Pumping
- Hydraulic Pumps
- Plunger Lift

Actions:

- migrate any direct `BeepApp` calls to typed web clients
- normalize page layout and shared input/result patterns
- standardize save and history actions
- tie pages back to active field / active well context

### Pass B — Add Missing First-Class Vertical Slices

Add missing API client + page combinations for the remaining incomplete Phase 7 slices at the time of initial planning.

Actions:

- add controller endpoints where none exist
- expand `ICalculationServiceClient`, `IPropertiesServiceClient`, and `IPumpServiceClient`
- create workbench pages and result components
- define result persistence models where needed

### Pass C — Advanced Operations Calculations

Add higher-order engineering tools and cross-tool composition. Choke Analysis and Compressor Analysis are now live first-class slices, so the remaining Pass C work is composition and workflow integration:

- Choke Analysis
- Compressor Analysis
- multi-tool artificial lift decision panels
- production optimisation pages that combine DCA + nodal + well test + properties

Actions:

- create advanced workbench routes
- connect recommended actions to work orders, AFEs, and production interventions
- add scenario comparison and cross-tool export support

---

## Module Placement Notes

| Module | Primary Web Location | Secondary Use |
|--------|----------------------|---------------|
| DCA | `/production/forecasting` and `/production/well-performance` | economics assumptions and reserves maturity |
| Economic Analysis | `/economics/evaluation` | AFE, intervention, development concept ranking |
| Production Forecasting | `/production/forecasting` | economics and reserves support |
| Nodal Analysis | `/production/well-performance` and calculation workbench | intervention candidate evaluation |
| Gas Lift | calculation workbench and artificial-lift recommendations | production optimisation |
| Pipeline Analysis | operations / flow assurance page | facility and constraint analysis |
| Flash / PVT | `/properties/*` and facility process panels | inputs to gas/oil properties and economics |
| Well Test Analysis | `/production/well-tests` and diagnostics tabs | intervention justification |
| Pump modules | `/pumps/*` and production diagnostics | artificial lift selection |

---

## Todo

| ID | Task | Status |
|----|------|--------|
| 7.1 | Replace remaining direct `BeepApp.Calculations` usage with `ICalculationServiceClient` | Done |
| 7.2 | Standardize layout for existing calculation pages | Done |
| 7.3 | Add DCA API/client/page slice | Done |
| 7.4 | Add Flash Calculations API/client/page slice | Done |
| 7.5 | Expand properties client for gas and oil property pages | Done |
| 7.6 | Add Well Test Analysis API/client/page slice | Done |
| 7.7 | Add Pump Performance API/client/page slice | Done |
| 7.8 | Add Choke Analysis and Compressor Analysis vertical slices | Done |
| 7.9 | Add shared calculation result and scenario components | Done |
| 7.10 | Start multi-tool artificial-lift composition | Done |
| 7.11 | Extend lift-composition recommendations across remaining pump workbenches | Done |
| 7.12 | Connect optimizer recommendations into intervention, work-order, and AFE intake | Done |
| 7.13 | Deepen work-order and AFE lifecycle linkage | Done |
| 7.14 | Add optimizer return-flow polish | Done |
| 7.15 | Add scenario comparison and analysis export support | Done |

Current 7.9 checkpoint:

- shared result-display components are live on the calculation workbench pages
- shared comparison-grid card is live on `EconomicEvaluation`, `ProductionForecasting`, and `CompareWells`
- shared optimization summary is live on `PumpPerformance`
- shared lift recommendation card is live on `PumpPerformance`, `GasLift`, `WellPerformanceOptimizer`, `SuckerRodPumping`, `HydraulicPump`, and `PlungerLift`
- `WellPerformanceOptimizer` now opens prefilled intervention, work-order, and AFE intake surfaces, and `InterventionDecisions` now routes its manual intake into the live work-order dashboard
- recommendation-driven work-order creation now calls the lifecycle AFE-link endpoint, `WorkOrderDetail` surfaces linked AFE state and actions, and `AFEManagement` can open directly to an existing linked AFE
- intervention, work-order, and AFE pages now carry `returnTo` context so users can move back into the originating optimisation surface after taking action
- shared JSON export packages are now live on `WellPerformanceOptimizer`, `ProductionForecasting`, `EconomicEvaluation`, and `CompareWells` via the client-side download helper and scoped export service

---

## Exit Criteria

- every calculation page uses a typed web client
- missing engineering modules have a first-class web integration path or an explicit defer decision
- calculation workbenches share a common interaction model
- calculation results can be saved, recalled, and linked to field/well workflows
