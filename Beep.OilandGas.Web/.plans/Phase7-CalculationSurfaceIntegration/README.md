# Phase 7 Folder Plan — Calculation Surface Integration

> Detailed execution plan for Phase 7  
> Companion overview: `../Phase7-CalculationSurfaceIntegration.md`

---

## Purpose

This folder turns the calculation integration strategy into three controlled passes so existing engineering surfaces can be normalized before missing vertical slices are introduced.

Before executing any workbench, result-grid, dialog, tabs, or stepper changes in this phase, read `../../MudBlazor_Docs/README.md` and then the matching local component docs.

---

## Pass Set

| Pass | Focus | Document |
|------|-------|----------|
| A | Normalize calculation modules that already have web and API presence | `PassA-StandardizeExistingSurfaces.md` |
| B | Add missing first-class API/client/page slices | `PassB-AddMissingVerticalSlices.md` |
| C | Compose advanced engineering tools into production and operations workflows | `PassC-AdvancedOperationsComposition.md` |

---

## Phase Coverage

### Existing integrated modules

- Economic Analysis
- Production Forecasting
- Nodal Analysis
- Gas Lift
- Pipeline Analysis
- Heat Map
- Sucker Rod Pumping
- Hydraulic Pumps
- Plunger Lift

### Current Phase 7 status

- cross-tool composition work tracked in Pass C is complete for the current slice set
- comparison and export support is live on the optimizer, production forecasting, economic evaluation, and compare-wells surfaces

---

## Deliverables

- typed-client-first calculation pages
- shared calculation workbench components
- reusable result-display components for KPI bands, summary fact grids, and summary/input tabs
- reusable comparison-grid card shell for scenario and comparison views
- reusable optimization-summary component for baseline-vs-optimized workbenches
- reusable lift-recommendation card for production-to-lift workflow handoff
- API/client/page coverage for missing engineering modules
- consistent save/history/export patterns
- workflow handoffs from calculations into operations, work orders, and AFEs
