# Phase 4 — Blazor UI Overview
## MudBlazor Pages and Components for Business Process Engine

> **Status**: Not started  
> **Depends on**: Phase 3 (API contracts)  
> **Blocks**: Phase 5 (analytics dashboards require process instance data)  
> **Owner**: Frontend Dev + UX  
> **MudBlazor**: Latest stable (check https://mudblazor.com for current version)

---

## Document Index

| # | Document | Purpose |
|---|---|---|
| This file | `00_Overview.md` | Goals, page inventory summary, key files |
| [01](01_PageInventory.md) | `01_PageInventory.md` | All 11 page routes, layouts, data requirements |
| [02](02_ComponentCatalog.md) | `02_ComponentCatalog.md` | 8 reusable Blazor components with props/events |
| [03](03_UXFlows.md) | `03_UXFlows.md` | User journey flows, screen wireframe descriptions |
| [04](04_StateManagement.md) | `04_StateManagement.md` | Cascading parameters, event bus, service injection |
| [05](05_MudBlazorPatterns.md) | `05_MudBlazorPatterns.md` | MudBlazor-specific implementation patterns |
| [06](06_TestPlan.md) | `06_TestPlan.md` | bUnit component test cases |
| [07](07_SprintPlan_RACI.md) | `07_SprintPlan_RACI.md` | Sprint stories, RACI matrix, Definition of Done |

---

## Goals

1. Process catalog browser: searchable list of all 96 process definitions grouped by category
2. Process instance dashboard: field-scoped active process list with state badges
3. Process start wizard: multi-step MudStepper to configure and launch a new instance
4. Transition panel: one-click trigger buttons with guard-failure feedback dialog
5. Audit trail viewer: chronological timeline of state changes per instance
6. Jurisdiction-aware rendering: hide/show steps and buttons based on `Jurisdiction` field

---

## Out of Scope (Phase 4)

- Work order scheduling calendar (Phase 6)
- HSE incident forms (Phase 7)
- Compliance filing screens (Phase 8)
- SCADA/sensor data widgets (Phase 9)

---

## Key Files

| File Path | Action |
|---|---|
| `Beep.OilandGas.Web/Pages/PPDM39/Process/ProcessDashboard.razor` | CREATE |
| `Beep.OilandGas.Web/Pages/PPDM39/Process/ProcessCatalog.razor` | CREATE |
| `Beep.OilandGas.Web/Pages/PPDM39/Process/ProcessDetail.razor` | CREATE |
| `Beep.OilandGas.Web/Pages/PPDM39/Process/StartProcess.razor` | CREATE |
| `Beep.OilandGas.Web/Pages/PPDM39/Process/ProcessAudit.razor` | CREATE |
| `Beep.OilandGas.Web/Components/Process/ProcessStateChip.razor` | CREATE |
| `Beep.OilandGas.Web/Components/Process/TransitionPanel.razor` | CREATE |
| `Beep.OilandGas.Web/Components/Process/ProcessTimeline.razor` | CREATE |
| `Beep.OilandGas.Web/Components/Process/ProcessStartStepper.razor` | CREATE |
| `Beep.OilandGas.Web/Components/Process/GuardFailureDialog.razor` | CREATE |
| `Beep.OilandGas.Web/Components/Process/ProcessFilterBar.razor` | CREATE |
| `Beep.OilandGas.Web/Components/Process/JurisdictionBadge.razor` | CREATE |
| `Beep.OilandGas.Web/Components/Process/ProcessCategoryCard.razor` | CREATE |

---

## Milestones

| ID | Target | Acceptance Criteria |
|---|---|---|
| M4-1 | Sprint 4.1 | `ProcessCatalog` lists all 96 definitions; category filter works |
| M4-2 | Sprint 4.2 | `ProcessDashboard` shows live field-scoped instances with state badges |
| M4-3 | Sprint 4.3 | `StartProcess` wizard completes and creates an instance via API |
| M4-4 | Sprint 4.4 | Transition buttons fire and show guard failure feedback inline |
| M4-5 | Sprint 4.5 | All bUnit tests pass; responsive at 1024px and 1440px |

---

## Page Summary

| Page | Route | Primary Component |
|---|---|---|
| Process Dashboard | `/ppdm39/process` | `MudDataGrid` with `ProcessStateChip` |
| Process Catalog | `/ppdm39/process/catalog` | `MudDataGrid` grouped by category |
| Process Detail | `/ppdm39/process/{instanceId}` | `TransitionPanel` + `ProcessTimeline` |
| Start Process | `/ppdm39/process/start` | `ProcessStartStepper` |
| Process Audit | `/ppdm39/process/{instanceId}/audit` | `ProcessTimeline` (full history) |
| Definition Detail | `/ppdm39/process/definition/{processId}` | Step list + document checklist |
| Exploration Processes | `/ppdm39/process/category/exploration` | Filtered dashboard |
| Development Processes | `/ppdm39/process/category/development` | Filtered dashboard |
| Production Processes | `/ppdm39/process/category/production` | Filtered dashboard |
| HSE Processes | `/ppdm39/process/category/hse` | Filtered dashboard |
| Gate Reviews | `/ppdm39/process/category/gate-reviews` | Filtered dashboard |
