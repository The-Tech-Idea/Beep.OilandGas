# Business Process Branch — Master Implementation Plan

> **Scope**: End-to-end implementation of the `BusinessProcess` genre branch in
> `Beep.OilandGas.Branchs`, the supporting workflow/service layer in
> `Beep.OilandGas.LifeCycle`, API controllers in `Beep.OilandGas.ApiService`,
> and Blazor UI pages in `Beep.OilandGas.Web`.
>
> **Goal**: Allow users to navigate, start, monitor, and manage oil-and-gas
> business processes/workflows directly from the Beep tree view — aligned with
> international best practices (USA: SPE/API/BSEE/ONRR, Canada: AER/NEB/CER,
> International: ISO/IOGP/OSPAR/NORSOK).
>
> **Detailed Phase Plans** (read per-phase for full PPDM table mapping, RACI matrices, and todo trackers):
>
> | Phase | File | Status |
> |---|---|---|
> | Phase 1 — Branch Infrastructure | *(this file)* | ✅ Complete |
> | Phase 2 — Service Layer Enhancements | [Phase2_ServiceLayer.md](Phase2_ServiceLayer.md) | 🔲 Not Started |
> | Phase 3 — API Layer | [Phase3_APILayer.md](Phase3_APILayer.md) | 🔲 Not Started |
> | Phase 4 — Blazor Web UI | [Phase4_BlazorUI.md](Phase4_BlazorUI.md) | 🔲 Not Started |
> | Phases 5–10 — Analytics, WO Engine, HSE, Compliance, Integrations, Rollout | [Phases5to10_AdvancedPlan.md](Phases5to10_AdvancedPlan.md) | 🔲 Not Started |

---

## Architecture Overview

```
Tree View
└── Business Processes  (BusinessProcessRootNode – genre)
    ├── Exploration Workflows         (BusinessProcessCategoryNode)
    │   ├── Lead-to-Prospect Assessment      (BusinessProcessNode – leaf)
    │   ├── Prospect-to-Discovery Evaluation
    │   └── ...
    ├── Development Workflows
    ├── Production Workflows
    ├── Decommissioning Workflows
    ├── Work Order Workflows
    ├── Approval & Gate Reviews
    ├── HSE & Safety Workflows
    ├── Compliance & Regulatory
    ├── Well Lifecycle Workflows
    ├── Facility Lifecycle Workflows
    ├── Reservoir Management Workflows
    └── Pipeline & Infrastructure
```

**Data flow per leaf node action**
```
BusinessProcessNode.ExecuteBranchAction("Start New Process Instance")
  → API: POST /api/process/instances
      → PPDMProcessService.StartProcessAsync()
          → ProcessStateMachine drives step transitions
              → Blazor SignalR push → ProgressTrackingClient
```

---

## Phase Overview

| Phase | Name                             | Status        | Target |
|-------|----------------------------------|---------------|--------|
| 1     | Branch Infrastructure            | ✅ Complete   | —      |
| 2     | Service Layer Enhancements       | 🔲 Not Started | —      |
| 3     | API Controller Layer             | 🔲 Not Started | —      |
| 4     | Process Definition Seeding       | 🔲 Not Started | —      |
| 5     | Web UI — Process Explorer        | 🔲 Not Started | —      |
| 6     | Web UI — Process Instance Mgmt   | 🔲 Not Started | —      |
| 7     | Approval & Gate-Review Workflows | 🔲 Not Started | —      |
| 8     | HSE & Compliance Workflows       | 🔲 Not Started | —      |
| 9     | Reporting & Analytics            | 🔲 Not Started | —      |
| 10    | Integration & End-to-End Testing | 🔲 Not Started | —      |

---

## Phase 1 — Branch Infrastructure

> **Status: ✅ COMPLETE**

### Deliverables
- [x] `BusinessProcess/BusinessProcessCategories.cs` — 12 category static registry
- [x] `BusinessProcess/BusinessProcessRootNode.cs` — genre root node
- [x] `BusinessProcess/BusinessProcessCategoryNode.cs` — category node with per-category process list
- [x] `BusinessProcess/BusinessProcessNode.cs` — leaf node with 5 context-menu actions
- [x] 14 new SVG icons in `GFX/`
- [x] `.csproj` updated with `<EmbeddedResource>` entries for all new SVGs
- [x] Build verified: `Build succeeded` (0 errors)

### Registered Process Workflows (96 total)

| Category | Count | Sample workflows |
|---|---|---|
| Exploration | 8 | Lead-to-Prospect, G&G Study, Exploration License Renewal |
| Development | 8 | Pool Definition, FDP Approval, First-Oil Readiness |
| Production | 8 | Well Start-up, Workover, Emergency Shutdown Response |
| Decommissioning | 6 | Well P&A, Site Remediation, Regulatory Notification |
| Work Orders | 6 | Preventive/Corrective/Inspection/Turnaround WOs |
| Approvals | 8 | Stage-Gate 0-5, MOC, AFE |
| HSE & Safety | 8 | HAZOP, SIMOPS, Incident Investigation |
| Compliance | 8 | Permit Application, Royalty Reporting, GHG Emissions |
| Well Lifecycle | 8 | Spud Authorization through P&A |
| Facility Lifecycle | 8 | Concept through Decommissioning |
| Reservoir Mgmt | 8 | Annual Review, IOR/EOR Screening, Infill Drilling |
| Pipeline | 8 | Integrity Assessment, Pigging, Abandonment |

---

## Phase 2 — Service Layer Enhancements

> **Status: 🔲 Not Started**

### Goal
Extend `PPDMProcessService` and `ProcessDefinitionInitializer` to cover **all
12 categories** with seeded `ProcessDefinition` objects and wired state machines.

### Todo Tracker

- [ ] **2.1** Extend `ProcessDefinitionInitializer.InitializeDefaultProcessDefinitionsAsync()`
  - Add seeder methods for all 12 categories (currently only Exploration,
    Development, Production, Decommissioning are wired)
  - Missing: Work Orders, Approvals/Gate Reviews, HSE, Compliance, Well
    Lifecycle, Facility Lifecycle, Reservoir Management, Pipeline
  - File: `LifeCycle/Services/Processes/ProcessDefinitionInitializer.cs`

- [ ] **2.2** Wire Gate-Review state machine  
  - States: `PENDING → UNDER_REVIEW → APPROVED | REJECTED | DEFERRED`
  - Transitions: `submit`, `approve`, `reject`, `defer`, `resubmit`
  - `ProcessStateMachine` registration in `PPDMProcessService`

- [ ] **2.3** Wire Work Order state machine  
  - States: `DRAFT → PLANNED → IN_PROGRESS → ON_HOLD → COMPLETED | CANCELLED`
  - Transitions: `plan`, `start`, `hold`, `resume`, `complete`, `cancel`

- [ ] **2.4** Wire HSE Workflow state machine  
  - States: `REPORTED → INVESTIGATING → ROOT_CAUSE_ANALYSIS → ACTIONS_OPEN → CLOSED`

- [ ] **2.5** Wire Compliance Workflow state machine  
  - States: `DRAFT → SUBMITTED → UNDER_REVIEW → COMPLIANT | NON_COMPLIANT → REMEDIATION → CLOSED`

- [ ] **2.6** Wire Well/Facility/Reservoir/Pipeline lifecycle state machines  
  - Reuse existing `WellLifecycleService` and `FacilityManagement` transitions
    where possible; create adapters into `IProcessService` if needed.

- [ ] **2.7** Add `GetProcessDefinitionByNameAsync(string processName)` helper  
  - Used by `BusinessProcessNode.ExecuteBranchAction("View Process Definition")`  
  - File: `IProcessService.cs` + `ProcessServiceBase.cs`

- [ ] **2.8** Unit-test new state machines  
  - File: `Beep.OilandGas.Tests/Services/Processes/`  
  - Cover: valid transitions, invalid transitions, guard conditions

---

## Phase 3 — API Controller Layer

> **Status: 🔲 Not Started**

### Goal
Expose all process operations through REST endpoints under
`/api/process/` and integrate with the existing `FieldOrchestrator` context.

### Todo Tracker

- [ ] **3.1** Create `BusinessProcessController`  
  - Route: `[Route("api/process")]`  
  - File: `ApiService/Controllers/Process/BusinessProcessController.cs`
  - Endpoints:
    - `GET  /api/process/categories` — list all 12 categories
    - `GET  /api/process/definitions?category={cat}` — list `ProcessDefinition` by category
    - `GET  /api/process/definitions/{id}` — single definition
    - `POST /api/process/instances` — start new process instance
    - `GET  /api/process/instances/{id}` — get instance state
    - `GET  /api/process/instances?entityId={}&entityType={}` — all for entity
    - `PUT  /api/process/instances/{id}/steps/{stepId}/complete`
    - `PUT  /api/process/instances/{id}/steps/{stepId}/skip`
    - `PUT  /api/process/instances/{id}/cancel`
    - `GET  /api/process/instances/{id}/history`
    - `GET  /api/process/instances/{id}/transitions` — available next states

- [ ] **3.2** Create `ProcessApprovalController`  
  - Route: `[Route("api/process/approvals")]`  
  - Endpoints:
    - `POST /api/process/approvals/request`
    - `PUT  /api/process/approvals/{id}/approve`
    - `PUT  /api/process/approvals/{id}/reject`
    - `GET  /api/process/approvals/pending?userId={}`  — approvals waiting for user

- [ ] **3.3** Register services in `Program.cs`  
  - Add `BusinessProcessController`-required DI registrations  
  - Follow the factory-pattern template from `CLAUDE.md`

- [ ] **3.4** Add Swagger annotations / XML docs to all new endpoints

---

## Phase 4 — Process Definition Seeding

> **Status: 🔲 Not Started**

### Goal
Seed all 96 canonical workflow process definitions into the application
database so they are immediately usable on first run.

### Todo Tracker

- [ ] **4.1** Create `BusinessProcessSeeder` class  
  - File: `PPDM39.DataManagement/Services/Process/BusinessProcessSeeder.cs`
  - Idempotent: check `GetProcessDefinitionsByTypeAsync` before insert

- [ ] **4.2** Seed Exploration workflows (8 definitions)

- [ ] **4.3** Seed Development workflows (8 definitions)

- [ ] **4.4** Seed Production workflows (8 definitions)

- [ ] **4.5** Seed Decommissioning workflows (6 definitions)

- [ ] **4.6** Seed Work Order workflows (6 definitions)

- [ ] **4.7** Seed Approval & Gate Review workflows (8 definitions)  
  - Each Stage-Gate definition must embed the `ApprovalStep` with  
    `approvalType = STAGE_GATE_{n}` and required approvers list

- [ ] **4.8** Seed HSE & Safety workflows (8 definitions)

- [ ] **4.9** Seed Compliance & Regulatory workflows (8 definitions)

- [ ] **4.10** Seed Well / Facility / Reservoir / Pipeline lifecycle workflows (32 definitions)

- [ ] **4.11** Wire seeder into `DatabaseCreator` / startup migrations  
  - Call `BusinessProcessSeeder.SeedAllAsync(userId)` from `Program.cs`
    startup pipeline (after DB creation step)

---

## Phase 5 — Web UI: Process Explorer Page

> **Status: 🔲 Not Started**

### Goal
A Blazor page where users can browser processes by category, see definition
details, and launch new instances — mirroring the tree-view experience in the UI.

### Todo Tracker

- [ ] **5.1** Create `ProcessExplorer.razor`  
  - Route: `/ppdm39/processes`  
  - File: `Web/Pages/PPDM39/Processes/ProcessExplorer.razor`  
  - Left panel: MudTreeView with 12 categories populated from API  
  - Right panel: process list for selected category  
  - Uses `ApiClient.GetAsync<List<ProcessCategory>>("/api/process/categories")`

- [ ] **5.2** Create `ProcessDefinitionCard` component  
  - File: `Web/Components/Process/ProcessDefinitionCard.razor`  
  - Shows: Name, Description, EntityType, Step count, Active Instances badge  
  - Action buttons: "Start New Instance", "View Instances", "View Definition"

- [ ] **5.3** Create `ProcessDefinitionDetail.razor` modal / side-panel  
  - Shows full step list, transition diagram (text-based), config metadata  
  - File: `Web/Components/Process/ProcessDefinitionDetail.razor`

- [ ] **5.4** Wire "Start New Process Instance" dialog  
  - Input: EntityId, EntityType  
  - Calls `POST /api/process/instances`  
  - Redirects to instance detail page on success

- [ ] **5.5** Add route to main nav drawer  
  - File: `Web/Components/Layout/NavMenu.razor`
  - Icon: `Icons.Material.Filled.AccountTree`

---

## Phase 6 — Web UI: Process Instance Management

> **Status: 🔲 Not Started**

### Goal
Real-time monitoring and management of active and historical process instances.

### Todo Tracker

- [ ] **6.1** Create `ProcessInstanceDashboard.razor`  
  - Route: `/ppdm39/processes/instances`  
  - MudDataGrid showing all instances for current field  
  - Columns: Process Name, Entity, Status, Current Step, Started By, Started Date, % Complete  
  - Filter chips: All / In Progress / Completed / Failed / Cancelled

- [ ] **6.2** Create `ProcessInstanceDetail.razor`  
  - Route: `/ppdm39/processes/instances/{instanceId}`  
  - Stepper showing completed / active / pending steps  
  - Live SignalR updates via `ProgressTrackingClient`  
  - Action panel: "Complete Step", "Skip Step", "Cancel Process"

- [ ] **6.3** Implement `ProcessStepActionDialog`  
  - Collects step-data fields before submitting step completion  
  - Calls `PUT /api/process/instances/{id}/steps/{stepId}/complete`

- [ ] **6.4** Implement process history timeline  
  - Reads `GET /api/process/instances/{id}/history`  
  - Chronological event list with user, action, timestamp, notes

- [ ] **6.5** SignalR integration  
  - Join hub group on instance detail page load  
  - Handle `ProcessStepCompleted`, `ProcessStateChanged`, `ProcessFailed` events  
  - Show MudSnackbar on status change

---

## Phase 7 — Approval & Gate-Review Workflows

> **Status: 🔲 Not Started**

### Goal
Full approval workflow support — request, route, approve/reject/defer — with
notification integration.

### Todo Tracker

- [ ] **7.1** Create `ApprovalInbox.razor`  
  - Route: `/ppdm39/processes/approvals`  
  - Shows pending approvals for the logged-in user  
  - Grouped by: Stage-Gate, MOC, AFE, Other

- [ ] **7.2** Create `ApprovalDetail.razor`  
  - Context: process instance + step data + history of prior approvals  
  - Action buttons: Approve, Reject, Request More Info, Defer

- [ ] **7.3** Implement approval notification service  
  - When `RequestApprovalAsync` is called, push in-app notification  
  - File: `LifeCycle/Services/Processes/ApprovalNotificationService.cs`

- [ ] **7.4** Add approval badge to nav menu  
  - Shows count of pending approvals for current user

- [ ] **7.5** Wire Stage-Gate 0-5 definitions with correct approver roles  
  - Gate 0 → Exploration Manager  
  - Gate 2 → VP Technology  
  - Gate 3 → VP Engineering + CFO (dual approval)  
  - Gate 5 → Operations Director

---

## Phase 8 — HSE & Compliance Workflows

> **Status: 🔲 Not Started**

### Todo Tracker

- [ ] **8.1** Create `HSEWorkflowPage.razor`  
  - Route: `/ppdm39/processes/hse`  
  - List active HSE process instances with severity coloring

- [ ] **8.2** Create Incident Investigation process definition  
  - 5-step process: Report → Investigate → Root Cause → Actions → Close  
  - Each step captures required data fields

- [ ] **8.3** Create `ComplianceCalendar.razor`  
  - Shows regulatory submission deadlines as a monthly calendar  
  - Sourced from active Compliance workflow instances

- [ ] **8.4** Permit tracking integration  
  - Link Compliance workflow instances to `PermitsAndApplications` project records

- [ ] **8.5** GHG/Emissions reporting workflow  
  - Step outputs feed into `ProductionAccounting` emission factors

---

## Phase 9 — Reporting & Analytics

> **Status: 🔲 Not Started**

### Todo Tracker

- [ ] **9.1** Create `ProcessAnalyticsPage.razor`  
  - Route: `/ppdm39/processes/analytics`  
  - KPIs: Avg cycle time per process type, on-time completion %, bottleneck steps

- [ ] **9.2** Process Gantt chart  
  - Shows multi-step processes across time for a field  
  - Uses MudTimeline or custom SVG Gantt component

- [ ] **9.3** Stage-Gate funnel chart  
  - Opportunities at each gate; conversion rates between gates

- [ ] **9.4** Overdue processes alert panel  
  - Processes where current step exceeded estimated duration  
  - Drills down to instance detail

- [ ] **9.5** Export process history to CSV/Excel  
  - Uses `DataManagementService.ExportAsync(...)`

---

## Phase 10 — Integration & End-to-End Testing

> **Status: 🔲 Not Started**

### Todo Tracker

- [ ] **10.1** Integration test: full Exploration process lifecycle  
  - Start → complete all steps → verify `ProcessStatus.COMPLETED`

- [ ] **10.2** Integration test: Stage-Gate approval flow  
  - Request approval → approve → verify state transition

- [ ] **10.3** Integration test: process cancellation mid-flight  
  - Start → complete 2 steps → cancel → verify all step instances closed

- [ ] **10.4** Integration test: Work Order process  
  - DRAFT → PLANNED → IN_PROGRESS → COMPLETED

- [ ] **10.5** UI smoke test: BusinessProcessNode branch renders correctly  
  - All 12 categories visible in tree  
  - All 96 leaf nodes present per category

- [ ] **10.6** Performance test: listing 100+ active instances  
  - Verify `ProcessInstanceDashboard` paginates correctly

- [ ] **10.7** Security test: approval endpoints  
  - Verify non-approver role cannot call `PUT /approvals/{id}/approve`  
  - Verify JWT claims are checked before state transitions

---

## File Creation Checklist Summary

### Phase 1 (Complete)
- [x] `Beep.OilandGas.Branchs/BusinessProcess/BusinessProcessCategories.cs`
- [x] `Beep.OilandGas.Branchs/BusinessProcess/BusinessProcessRootNode.cs`
- [x] `Beep.OilandGas.Branchs/BusinessProcess/BusinessProcessCategoryNode.cs`
- [x] `Beep.OilandGas.Branchs/BusinessProcess/BusinessProcessNode.cs`
- [x] 14 SVG icons in `Beep.OilandGas.Branchs/GFX/`
- [x] `Beep.OilandGas.Branchs.csproj` SVG entries

### Phase 2
- [ ] `Beep.OilandGas.LifeCycle/Services/Processes/ProcessDefinitionInitializer.cs` (extend)
- [ ] `Beep.OilandGas.LifeCycle/Services/Processes/IProcessService.cs` (extend)
- [ ] `Beep.OilandGas.LifeCycle/Services/Processes/ProcessServiceBase.cs` (extend)

### Phase 3
- [ ] `Beep.OilandGas.ApiService/Controllers/Process/BusinessProcessController.cs`
- [ ] `Beep.OilandGas.ApiService/Controllers/Process/ProcessApprovalController.cs`
- [ ] `Beep.OilandGas.ApiService/Program.cs` (DI registrations)

### Phase 4
- [ ] `Beep.OilandGas.PPDM39.DataManagement/Services/Process/BusinessProcessSeeder.cs`

### Phase 5
- [ ] `Beep.OilandGas.Web/Pages/PPDM39/Processes/ProcessExplorer.razor`
- [ ] `Beep.OilandGas.Web/Components/Process/ProcessDefinitionCard.razor`
- [ ] `Beep.OilandGas.Web/Components/Process/ProcessDefinitionDetail.razor`

### Phase 6
- [ ] `Beep.OilandGas.Web/Pages/PPDM39/Processes/ProcessInstanceDashboard.razor`
- [ ] `Beep.OilandGas.Web/Pages/PPDM39/Processes/ProcessInstanceDetail.razor`
- [ ] `Beep.OilandGas.Web/Components/Process/ProcessStepActionDialog.razor`

### Phase 7
- [ ] `Beep.OilandGas.Web/Pages/PPDM39/Processes/ApprovalInbox.razor`
- [ ] `Beep.OilandGas.Web/Pages/PPDM39/Processes/ApprovalDetail.razor`
- [ ] `Beep.OilandGas.LifeCycle/Services/Processes/ApprovalNotificationService.cs`

### Phase 8
- [ ] `Beep.OilandGas.Web/Pages/PPDM39/Processes/HSEWorkflowPage.razor`
- [ ] `Beep.OilandGas.Web/Pages/PPDM39/Processes/ComplianceCalendar.razor`

### Phase 9
- [ ] `Beep.OilandGas.Web/Pages/PPDM39/Processes/ProcessAnalyticsPage.razor`

### Phase 10
- [ ] Test files in `Beep.OilandGas.Tests/`

---

## Notes & Decisions

- **No new database tables**: Process data is stored via the existing
  `PPDMGenericRepository` pattern (application DB for process tables,
  PPDM39 for domain data). `MiscStringID` on `BusinessProcessNode` is the
  look-up key for `ProcessDefinition.ProcessName`.
- **96 canonical workflows**: Defined statically in
  `BusinessProcessCategoryNode.GetProcessNamesForCategory()`. These names match
  the seed data that `ProcessDefinitionInitializer` inserts on startup.
- **Leaf node actions** are dispatched by the host tree-view UI; the node logs
  intent via `DMEEditor.AddLogMessage` so the host can intercept and route.
- **Icons**: All process branch icons follow the same embedded-SVG pattern as
  the PPDM39 branch. New icons use `EnumPointType.Genre/Category/Entity` to
  match framework expectations.
