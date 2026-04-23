# Phase 6 — Web Architecture Consolidation

> Status: Done  
> Depends On: Existing web project state  
> Goal: Remove structural ambiguity in the web project before broader domain integration work starts.
> Detailed subplans: `Phase6-WebArchitectureConsolidation/README.md`

---

## Objective

Establish one predictable web architecture for `Beep.OilandGas.Web`:

- `Pages/` owns routes
- `Components/` owns reusable UI
- typed clients own service access
- direct domain calls from pages are treated as legacy

Before making UI changes in this phase, read `../MudBlazor_Docs/README.md` and the matching local MudBlazor component docs for any shell, nav, grid, dialog, tab, stepper, or shared-component changes.

---

## Problems To Resolve In This Phase

1. App shell duplication: root `App.razor`, `Components/App.razor`, and `Components/Routes.razor`
2. Shared component duplication: `Components/Shared/KpiCard.razor` and `Components/Dashboard/KpiCard.razor`
3. Route ownership drift: custom routed content lives in both `Pages/` and `Components/`
4. Legacy vs canonical admin pages: `Pages/Data/*` and `Pages/PPDM39/Data/*`
5. Mixed integration style: `IBeepOilandGasApp` side-by-side with typed web clients
6. Direct project references in the web project that should move behind API seams over time

---

## Target Decisions

| Concern | Canonical Outcome |
|---------|-------------------|
| App shell | Root `App.razor` + `Components/Routes.razor` remain; `Components/App.razor` is retired or made internal-only if still required by framework conventions |
| KPI card | `Components/Shared/KpiCard.razor` becomes the single KPI component |
| Routed custom pages | Live under `Pages/` only |
| Identity routes | `Components/Account/Pages/*` remains the only allowed `Components/` route exception |
| Data/admin pages | One route tree for setup, quality, audit, versioning, and schema tasks |
| Integration style | Page code should inject typed web clients, not `IBeepOilandGasApp` |

---

## Pass Plan

### Pass A — Inventory and Decision Freeze

- classify every routed Razor file as `canonical`, `legacy`, or `identity exception`
- produce a route manifest for all custom pages
- mark the canonical app shell and canonical shared component set
- identify which project references are actually needed for page compilation versus legacy convenience

### Pass B — Consolidate Structure

- unify duplicate shell and shared components
- move any custom routed page out of `Components/` into `Pages/`
- choose one canonical route tree for data/setup/admin surfaces
- standardize page imports around shared components and typed clients

### Pass C — Dependency and DI Cleanup

- remove page-level dependence on `IBeepOilandGasApp`
- update `Program.cs` so typed clients are the default integration path
- begin trimming direct project references from `Beep.OilandGas.Web.csproj`
- verify that routes, layouts, and auth redirects still behave correctly

---

## Todo

| ID | Task | Status |
|----|------|--------|
| 6.1 | Create a route ownership manifest for `Pages/` and routed files under `Components/` | Done |
| 6.2 | Designate the single canonical app shell (`App.razor` + `Routes.razor`) | Done |
| 6.3 | Consolidate `KpiCard` into one shared implementation | Done |
| 6.4 | Classify `Pages/Data/*` vs `Pages/PPDM39/Data/*` as canonical vs legacy | Done |
| 6.5 | Move or retire custom routed files under `Components/Pages/*` | Done |
| 6.6 | Remove page-level `IBeepOilandGasApp` usage in favor of typed clients | Done |
| 6.7 | Define the allowed long-term project references for the web project | Done |

---

## Exit Criteria

- there is one documented app shell
- there is one shared KPI card
- custom routable surfaces live under `Pages/`
- typed clients are the page-level default integration style
- legacy routes and components are explicitly marked for retirement or compatibility
