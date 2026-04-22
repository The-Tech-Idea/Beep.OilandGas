# Phase 7 Pass A — Standardize Existing Surfaces

## Objective

Bring the calculation modules that already exist in both the API and web layers onto one interaction pattern.

---

## Modules In Scope

- Economic Analysis
- Production Forecasting
- Nodal Analysis
- Gas Lift
- Pipeline Analysis
- Heat Map
- Sucker Rod Pumping
- Hydraulic Pumps
- Plunger Lift

---

## Workstreams

| Workstream | Scope | Result |
|------------|-------|--------|
| Page integration cleanup | direct `BeepApp` calls and inconsistent service usage | typed-client-only calculation pages |
| UX normalization | forms, summaries, charts, save actions, history | one calculation workbench interaction model |
| Context propagation | active field, active well, scenario selection | reusable field/well selectors and consistent page state |
| Result persistence | save, reload, compare, export | standardized result lifecycle |

---

## Exit Gate

Existing calculation pages should feel like one product line rather than separate experiments.
