# Phase 4 — Blazor Web UI: Business Process Pages
## Business Process Branch — Detailed Implementation Plan

> **Status**: 🔲 Not Started  
> **Depends on**: Phase 3 (API Layer)  
> **Owner**: Web / UI Team  
> **UI Framework**: Blazor Server + MudBlazor latest check online 8.0

---

## 4.1 — Objective

Build the Blazor pages and components that allow users to:
- Navigate and search all process definitions
- Start, monitor, and advance process instances
- Execute gate reviews with role-based approval
- Report and manage HSE incidents
- Submit and track compliance reports
- View a field-level process dashboard

---

## 4.2 — Page Inventory

All pages live under `Beep.OilandGas.Web/Pages/PPDM39/BusinessProcess/`:

| Page File | Route | Purpose | Auth |
|---|---|---|---|
| `ProcessDashboard.razor` | `/ppdm39/process` | Field-level overview: active instances, overdue gates, open HSE | All users |
| `ProcessDefinitions.razor` | `/ppdm39/process/definitions` | Browse / search all 96 definitions; filter by category + jurisdiction | All users |
| `ProcessDefinitionDetail.razor` | `/ppdm39/process/definitions/{id}` | View definition steps, PPDM tables, standards references | All users |
| `ProcessInstances.razor` | `/ppdm39/process/instances` | List active + completed instances | All users |
| `ProcessInstanceDetail.razor` | `/ppdm39/process/instances/{id}` | Step-by-step progress; execute transitions; attach docs | All users |
| `GateReviews.razor` | `/ppdm39/process/gates` | List pending gates; submit / approve / reject | GateApprover |
| `GateReviewDetail.razor` | `/ppdm39/process/gates/{id}` | Gate checklist; document upload; decision form | GateApprover |
| `HSEIncidents.razor` | `/ppdm39/hse/incidents` | Report + track HSE incidents | All users |
| `HSEIncidentDetail.razor` | `/ppdm39/hse/incidents/{id}` | RCA form; corrective actions; close-out | SafetyOfficer |
| `ComplianceObligations.razor` | `/ppdm39/compliance` | Obligations calendar; overdue list | All users |
| `ComplianceReport.razor` | `/ppdm39/compliance/report` | Submit jurisdiction report; remediation | Compliance |

---

## 4.3 — Component Inventory

Reusable components under `Beep.OilandGas.Web/Components/BusinessProcess/`:

| Component | Purpose | Used By |
|---|---|---|
| `ProcessStatusBadge.razor` | Color-coded `MudChip` showing process status | All pages |
| `ProcessStepTimeline.razor` | `MudTimeline` showing step sequence + current position | `ProcessInstanceDetail` |
| `StateTransitionPanel.razor` | Available transitions for current state (buttons + reason input) | `ProcessInstanceDetail` |
| `GateChecklistTable.razor` | Required documents checklist (MudDataGrid) | `GateReviewDetail` |
| `HSEIncidentSeverityBadge.razor` | API RP 754 Tier 1-4 badge | `HSEIncidents`, `ProcessDashboard` |
| `ObligationCalendar.razor` | MudCalendar view of compliance due dates | `ComplianceObligations` |
| `JurisdictionFilterChips.razor` | USA / Canada / International filter chips | `ProcessDefinitions` |
| `PPDMTableLink.razor` | Renders PPDM table name as a hyperlink into PPDM39 branch | `ProcessDefinitionDetail` |

---

## 4.4 — PPDM Tables Displayed Per Page

| Page | Data Source (PPDM Tables via API) |
|---|---|
| `ProcessDashboard` | Aggregated from `PROJECT_STEP`, `HSE_INCIDENT`, `OBLIGATION` |
| `ProcessDefinitions` | `PROJECT_PLAN`, `PROJECT_STEP` (definition metadata) |
| `ProcessInstanceDetail` | `PROJECT_STEP`, `PROJECT_STEP_CONDITION`, `PPDM_AUDIT_HISTORY` |
| `GateReviewDetail` | `PROJECT_STEP_BA`, `RM_INFORMATION_ITEM`, `BA_AUTHORITY` |
| `HSEIncidentDetail` | `HSE_INCIDENT`, `HSE_INCIDENT_CAUSE`, `HSE_INCIDENT_DETAIL`, `HSE_INCIDENT_BA` |
| `ComplianceObligations` | `OBLIGATION`, `OBLIG_CALC`, `OBLIG_PAYMENT` |
| `ComplianceReport` | `NOTIFICATION`, `PDEN_VOL_SUMMARY` (for volume-based reports) |

---

## 4.5 — Responsibility Matrix (RACI)

| Task | UI Dev | UX/Design | Backend Dev | QA | Domain SME | Compliance |
|---|:---:|:---:|:---:|:---:|:---:|:---:|
| 4.1 — ProcessDashboard | R/A | C | C | C | I | I |
| 4.2 — ProcessDefinitions + Detail | R/A | C | I | C | I | I |
| 4.3 — ProcessInstanceDetail + StateTransitionPanel | R | C | A | C | C | I |
| 4.4 — GateReviews + GateReviewDetail | R | C | C | C | C | R/A |
| 4.5 — HSEIncidents + HSEIncidentDetail | R | C | C | C | A | C |
| 4.6 — ComplianceObligations + Report | R | C | C | C | I | R/A |
| 4.7 — Reusable components | R/A | C | I | C | I | I |
| 4.8 — MudBlazor theme / layout integration | R/A | A | I | C | I | I |
| 4.9 — SignalR progress for long process ops | C | I | R/A | C | I | I |
| 4.10 — E2E tests | C | I | C | R/A | I | I |

---

## 4.6 — Todo Tracker

| ID | Task | Priority | File | Status | Notes |
|---|---|---|---|---|---|
| 4.1 | Create `ProcessDashboard.razor` | HIGH | `Web/Pages/PPDM39/BusinessProcess/` | 🔲 | KPI cards: active instances, overdue gates, open HSE; uses `MudGrid` |
| 4.2 | Create `ProcessDefinitions.razor` | MEDIUM | same | 🔲 | `MudDataGrid` with category + jurisdiction filter chips |
| 4.3 | Create `ProcessDefinitionDetail.razor` | MEDIUM | same | 🔲 | Step list with `PPDMTableLink` component per step |
| 4.4 | Create `ProcessInstances.razor` | MEDIUM | same | 🔲 | Filter by status + date range |
| 4.5 | Create `ProcessInstanceDetail.razor` | HIGH | same | 🔲 | `ProcessStepTimeline` + `StateTransitionPanel` |
| 4.6 | Create `GateReviews.razor` | HIGH | same | 🔲 | Pending gates matrix; colour coded by overdue state |
| 4.7 | Create `GateReviewDetail.razor` | HIGH | same | 🔲 | `GateChecklistTable` + approve/reject `MudDialog` |
| 4.8 | Create `HSEIncidents.razor` | HIGH | same | 🔲 | Quick-report FAB; IOGP Tier severity filter |
| 4.9 | Create `HSEIncidentDetail.razor` | HIGH | same | 🔲 | RCA form; corrective action `MudDataGrid` with add/close actions |
| 4.10 | Create `ComplianceObligations.razor` | HIGH | same | 🔲 | `ObligationCalendar` + overdue badge list |
| 4.11 | Create `ComplianceReport.razor` | HIGH | same | 🔲 | Jurisdiction select (USA/Canada/Intl) drives form fields |
| 4.12 | Create `ProcessStatusBadge.razor` component | HIGH | `Web/Components/BusinessProcess/` | 🔲 | |
| 4.13 | Create `ProcessStepTimeline.razor` component | HIGH | same | 🔲 | Wrap `MudTimeline` |
| 4.14 | Create `StateTransitionPanel.razor` component | HIGH | same | 🔲 | Calls POST `/instances/{id}/transitions`; shows confirm `MudDialog` |
| 4.15 | Create `GateChecklistTable.razor` component | MEDIUM | same | 🔲 | Show `RM_INFORMATION_ITEM` docs as rows; red if missing |
| 4.16 | Create `HSEIncidentSeverityBadge.razor` component | MEDIUM | same | 🔲 | Tier 1 = red, Tier 2 = orange, Tier 3 = yellow, Tier 4 = green |
| 4.17 | Create `ObligationCalendar.razor` component | MEDIUM | same | 🔲 | Wraps MudCalendar or custom calendar |
| 4.18 | Create `JurisdictionFilterChips.razor` component | MEDIUM | same | 🔲 | |
| 4.19 | Create `PPDMTableLink.razor` component | LOW | same | 🔲 | Navigates to PPDM39 branch node |
| 4.20 | Register all pages in `NavMenu.razor` under "Business Processes" section | MEDIUM | `Web/Shared/NavMenu.razor` | 🔲 | Collapse group; icon: `business-process.svg` |
| 4.21 | E2E test: start process → advance 2 steps → complete | MEDIUM | `Tests/E2E/ProcessFlowE2ETests.cs` | 🔲 | Playwright or Selenium |
| 4.22 | E2E test: gate submission + approval flow | MEDIUM | same | 🔲 | |

---

## 4.7 — Definition of Done (Phase 4)

- [ ] All 11 pages render without errors in Blazor Server
- [ ] `ProcessDashboard` shows live data from API
- [ ] `StateTransitionPanel` only shows valid transitions for current state
- [ ] Gate approval / rejection triggers `NOTIFICATION` write visible in API
- [ ] HSE incident report maps to `HSE_INCIDENT` row (verified via DB inspection)
- [ ] All pages have loading skeletons (MudSkeleton) and error fallback UI
- [ ] NavMenu updated with "Business Processes" section and sub-links
- [ ] E2E tests green
