# Phase 6 Pass B — Consolidate Structure

## Objective

Apply the decisions from Pass A and remove the most obvious structural duplication in the web project.

---

## Workstreams

| Workstream | Scope | Result |
|------------|-------|--------|
| Shell consolidation | root `App.razor`, `Components/App.razor`, `Components/Routes.razor` | one canonical shell path |
| Shared component consolidation | `KpiCard` and other duplicated shared widgets | one canonical implementation per shared concept |
| Routed page normalization | custom routed pages under `Components/` | custom routes moved to `Pages/` or marked compatibility-only |
| Data/admin route cleanup | setup, schema, quality, and validation pages | one canonical route tree |
| Namespace/import cleanup | `_Imports.razor`, page imports, component references | stable page/component references after moves |

---

## Required Outputs

1. One canonical app shell.
2. One canonical KPI card.
3. Explicit list of legacy routes kept temporarily.
4. Consolidated admin/data route tree.
5. Updated page/component ownership guidance reflected in the code layout.

---

## Exit Gate

No custom business page should still live under `Components/` unless it is intentionally retained as a compatibility path and documented as such.
