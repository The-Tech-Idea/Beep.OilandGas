# Phase 6 Pass A — Inventory and Decision Freeze

## Objective

Create the evidence set required to make structural decisions once and stop re-litigating page/component ownership during later edits.

---

## Workstreams

| Workstream | Scope | Output |
|------------|-------|--------|
| Route inventory | All routed Razor files under `Pages/` and `Components/` | route manifest with `canonical`, `legacy`, and `identity exception` labels |
| App shell audit | `App.razor`, `Components/App.razor`, `Components/Routes.razor` | single shell decision record |
| Shared component audit | `KpiCard`, status badges, dashboard widgets, selectors | duplicate-component list with canonical owner |
| Data/admin route audit | `Pages/Data/*`, `Pages/PPDM39/Data/*`, setup pages | canonical admin route tree proposal |
| Dependency audit | `Program.cs`, `Beep.OilandGas.Web.csproj`, page injections | typed-client vs direct-domain dependency map |

---

## Required Outputs

1. Route ownership manifest.
2. App shell ownership note.
3. Shared component duplication list.
4. Data/admin route duplication list.
5. Page-level `IBeepOilandGasApp` usage inventory.
6. Web project reference classification: `required now`, `candidate for API boundary`, `legacy convenience`.

---

## Exit Gate

This pass is complete only when the later consolidation pass can be run without needing fresh architecture debates.
