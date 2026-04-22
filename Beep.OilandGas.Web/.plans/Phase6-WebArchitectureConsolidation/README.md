# Phase 6 Folder Plan — Web Architecture Consolidation

> Detailed execution plan for Phase 6  
> Companion overview: `../Phase6-WebArchitectureConsolidation.md`

---

## Purpose

The top-level phase file states the target state. This folder breaks that phase into execution passes so the cleanup can be run in controlled steps instead of one broad refactor.

Before executing any UI change in this phase, read `../../MudBlazor_Docs/README.md` and then the matching local MudBlazor docs for the components being changed.

---

## Pass Set

| Pass | Focus | Document |
|------|-------|----------|
| A | Freeze route, shell, and dependency decisions from current state | `PassA-InventoryAndDecisionFreeze.md` |
| B | Consolidate duplicated structures and normalize page/component ownership | `PassB-ConsolidateStructure.md` |
| C | Remove legacy page-side integration patterns and reduce direct dependencies | `PassC-DependencyAndDiCleanup.md` |

---

## Target Deliverables

- route ownership manifest for all routed Razor files
- app shell decision record
- canonical shared-component set, including one KPI card
- canonical data/admin route tree decision
- typed-client-first integration rule for pages
- first pass at trimming direct web project references

---

## Phase Rules

1. Freeze ownership decisions before moving files.
2. Do not combine route moves and dependency trimming in the same pass unless the route is low-risk.
3. Preserve the Identity route exception explicitly; do not mix it with custom business pages.
4. Treat `IBeepOilandGasApp` in pages as legacy compatibility, not a template for new work.
