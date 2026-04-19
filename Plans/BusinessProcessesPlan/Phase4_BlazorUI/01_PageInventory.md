# Phase 4 — Page Inventory
## All 11 Blazor Pages: Routes, Injection, and Data Contracts

---

## 1. ProcessDashboard.razor

```razor
@page "/ppdm39/process"
@attribute [Authorize]
@inject ApiClient ApiClient
@inject IFieldOrchestrator FieldOrchestrator
@inject NavigationManager Nav
```

**Data loaded**: `GET /api/field/current/process/instances` — filtered by current field ID.  
**Components used**: `ProcessStateChip`, `ProcessFilterBar`, `MudDataGrid`  
**Key interactions**:
- Click row → navigate to `/ppdm39/process/{instanceId}`
- "Start Process" button → navigate to `/ppdm39/process/start`
- Filter bar updates `?state=` and `?category=` query params; grid re-fetches

---

## 2. ProcessCatalog.razor

```razor
@page "/ppdm39/process/catalog"
@attribute [Authorize]
@inject ApiClient ApiClient
@inject NavigationManager Nav
```

**Data loaded**: `GET /api/process/definitions` — all 96 definitions.  
**Components used**: `ProcessCategoryCard`, `MudDataGrid`, `MudChipSet` (type filter)  
**Key interactions**:
- Click definition row → navigate to `/ppdm39/process/definition/{processId}`
- MudChipSet filters by `ProcessType` (WORK_ORDER, GATE_REVIEW, etc.)

---

## 3. ProcessDetail.razor

```razor
@page "/ppdm39/process/{InstanceId}"
@attribute [Authorize]
@inject ApiClient ApiClient
@inject IFieldOrchestrator FieldOrchestrator
```

**Route param**: `[Parameter] public string InstanceId { get; set; }`  
**Data loaded**:
- `GET /api/field/current/process/instances/{InstanceId}`
- `GET /api/field/current/process/instances/{InstanceId}/transitions`

**Components used**: `TransitionPanel`, `ProcessTimeline` (last 5 events), `ProcessStateChip`, `JurisdictionBadge`  
**Key interactions**: TransitionPanel fires and refreshes instance on 200; shows `GuardFailureDialog` on 422.

---

## 4. StartProcess.razor

```razor
@page "/ppdm39/process/start"
@attribute [Authorize(Roles = "ProcessOperator,Manager,Admin")]
@inject ApiClient ApiClient
@inject NavigationManager Nav
```

**Data loaded**: `GET /api/process/definitions` (for step 1 picklist only).  
**Components used**: `ProcessStartStepper` (owns internal state)  
**Key interactions**: Stepper completion → POST `/api/field/current/process/start` → navigate to ProcessDetail of new instance.

---

## 5. ProcessAudit.razor

```razor
@page "/ppdm39/process/{InstanceId}/audit"
@attribute [Authorize(Roles = "Auditor,Manager,Admin")]
@inject ApiClient ApiClient
```

**Route param**: `[Parameter] public string InstanceId { get; set; }`  
**Data loaded**: `GET /api/field/current/process/instances/{InstanceId}/audit`  
**Components used**: `ProcessTimeline` (full history mode), `MudTable`  
**Export**: "Download CSV" button serializes audit rows client-side.

---

## 6. DefinitionDetail.razor

```razor
@page "/ppdm39/process/definition/{ProcessId}"
@attribute [Authorize]
@inject ApiClient ApiClient
```

**Route param**: `[Parameter] public string ProcessId { get; set; }`  
**Data loaded**: `GET /api/process/definitions/{ProcessId}`  
**Components used**: `MudTimeline` (step sequence), `JurisdictionBadge`, `MudChip` (required documents)  
**Purpose**: Read-only reference page; no transitions.

---

## 7–11. Category-Filtered Dashboards

All five are thin wrappers over ProcessDashboard with a pre-applied category filter:

```razor
// ExplorationProcesses.razor
@page "/ppdm39/process/category/exploration"
@inherits FilteredProcessBase
@code { protected override string CategoryFilter => "Exploration"; }
```

Base class `FilteredProcessBase` inherits the same `ProcessDashboard` logic but passes `?category={CategoryFilter}` on load.

| Page | Route | CategoryFilter value |
|---|---|---|
| ExplorationProcesses | `/ppdm39/process/category/exploration` | `"Exploration"` |
| DevelopmentProcesses | `/ppdm39/process/category/development` | `"Development"` |
| ProductionProcesses | `/ppdm39/process/category/production` | `"Production"` |
| HSEProcesses | `/ppdm39/process/category/hse` | `"HSE"` |
| GateReviews | `/ppdm39/process/category/gate-reviews` | `"GateReview"` |

---

## Navigation Registration

Add these items to `NavMenu.razor` sidebar:

```razor
<MudNavGroup Title="Business Processes" Icon="@Icons.Material.Filled.AccountTree" Expanded="false">
    <MudNavLink Href="/ppdm39/process" Icon="@Icons.Material.Filled.Dashboard">Dashboard</MudNavLink>
    <MudNavLink Href="/ppdm39/process/catalog" Icon="@Icons.Material.Filled.LibraryBooks">Definition Catalog</MudNavLink>
    <MudNavLink Href="/ppdm39/process/category/exploration">Exploration</MudNavLink>
    <MudNavLink Href="/ppdm39/process/category/development">Development</MudNavLink>
    <MudNavLink Href="/ppdm39/process/category/production">Production</MudNavLink>
    <MudNavLink Href="/ppdm39/process/category/hse">HSE</MudNavLink>
    <MudNavLink Href="/ppdm39/process/category/gate-reviews">Gate Reviews</MudNavLink>
</MudNavGroup>
```
