# Phase 2 — Exploration Business Process UX

> **Status**: 🔲 Not Started  
> **Depends on**: Phase 1 (Foundation & Navigation)  
> **Duration**: ~2 weeks  
> **Reference Process**: `Plans/BusinessProcessesPlan/PetroleumEngineerBusinessProcesses.md` — Part 2  
> **Goal**: Build the exploration section as a petroleum geoscientist's daily work environment.

---

## Objective

A geoscientist or exploration engineer opens the Exploration section and sees:
- **Where are my prospects in the maturation funnel?**
- **What decisions are pending?**
- **What seismic data is available?**
- **What well programs need approval?**

They should be able to advance a prospect, request a well program, and track seismic — all without touching raw CRUD tables.

---

## Pages to Build

### 2.1 Exploration Dashboard

**Route**: `/exploration`  
**File**: `Pages/PPDM39/Exploration/Dashboard.razor`  
**Process**: Entry point for all exploration activity

**Layout**:
```
┌──────────────────────────────────────────────────────────────┐
│  Exploration  ●  [Active Field Name]                        │
├────────┬────────┬────────┬────────┐                          │
│Leads   │Prospects│Drill   │Active  │  ← Prospect funnel KPIs │
│  8     │  5     │Candidates│Wells  │                         │
│        │        │  2     │  12    │                         │
├────────┴────────┴────────┴────────┤                          │
│  Pending Decisions (gate reviews) │  Upcoming Well Programs  │
│  3 prospects await ranking        │  2 programs pending AFE  │
│  1 well program awaiting approval │                         │
├───────────────────────────────────┴──────────────────────────┤
│  [→ View Prospect Board]  [→ New Well Program]               │
└──────────────────────────────────────────────────────────────┘
```

**KPIs** (use `KpiCard` component):
1. Active Leads count
2. Prospects count  
3. Drill Candidates count
4. Exploratory Wells Drilled (YTD)
5. Success Rate (%) — discoveries / wells drilled

**Quick Actions**:
- `[Open Prospect Board]` → `/exploration/prospect-board`
- `[Submit Well Program]` → `/exploration/well-program/new`

### Todo

| ID | Task | Status |
|----|------|--------|
| 2.1.a | Create `Dashboard.razor` page skeleton with route | 🔲 |
| 2.1.b | Add 5 KpiCard components with static data | 🔲 |
| 2.1.c | Add Pending Decisions panel | 🔲 |
| 2.1.d | Add Quick Actions buttons | 🔲 |
| 2.1.e | Wire to API: `GET /api/exploration/dashboard-summary` | 🔲 |

---

### 2.2 Prospect Maturation Board

**Route**: `/exploration/prospect-board`  
**File**: `Pages/PPDM39/Exploration/ProspectBoard.razor`  
**Process**: Prospect lifecycle from lead to drill decision  
**Pattern**: Kanban board (see guidelines §3.3)

**Columns** (stages):
1. **Leads** — Raw opportunities identified
2. **Screening** — Being evaluated against min criteria
3. **Prospect** — Passed screening, being matured
4. **Ranking** — In portfolio ranking process
5. **Drill Candidate** — Approved for well program
6. **Drilled** — Well has been executed

**Prospect Card** shows:
- Prospect name
- Estimated gross resource (MMBO/BCF)
- Risk (Pg%) or chance of success
- Age in current stage
- Status badge (Active/On-hold/At-risk)

**Actions from board**:
- Click card → Open Prospect Detail page
- Drag card to next stage → Triggers state transition dialog
  - Confirm advance, capture decision rationale, record date
- Stage header menu: `[+ Add Lead]`

**Filters** (top bar):
- Filter by play type (clastic/carbonate/unconventional)
- Filter by basin
- Show/hide dropped prospects (archived)

### Todo

| ID | Task | Status |
|----|------|--------|
| 2.2.a | Create `ProspectBoard.razor` page | 🔲 |
| 2.2.b | Build kanban column layout (MudGrid columns) | 🔲 |
| 2.2.c | Create `ProspectCard.razor` component | 🔲 |
| 2.2.d | Add stage-advance dialog (confirm + reason) | 🔲 |
| 2.2.e | Add filter bar (play type, basin, show archived) | 🔲 |
| 2.2.f | Wire to API: `GET /api/exploration/prospects?fieldId=` | 🔲 |
| 2.2.g | Wire stage advance to API: `POST /api/exploration/prospects/{id}/advance` | 🔲 |

---

### 2.3 Prospect Detail & Screening Page

**Route**: `/exploration/prospect-board/{id}`  
**File**: `Pages/PPDM39/Exploration/ProspectDetail.razor`  
**Process**: Detailed prospect review and screening decision

**Layout** (two-column):
```
LEFT (2/3):                            RIGHT (1/3):
┌─────────────────────┐                ┌─────────────────────┐
│ Prospect Header     │                │ Stage & Status      │
│ Name, Basin, Play   │                │ ProcessTimeline     │
├─────────────────────┤                │ component           │
│ Resource Estimate   │                ├─────────────────────┤
│ P90 / P50 / P10     │                │ Pending Actions     │
│ (MudChart — pie)    │                │ [Advance to Next]   │
├─────────────────────┤                │ [Request Well Prog] │
│ Screening Criteria  │                │ [Drop Prospect]     │
│ Checklist (table)   │                └─────────────────────┘
├─────────────────────┤
│ Well History        │
│ (related wells)     │
└─────────────────────┘
```

**Sections** (MudTabs):
1. **Summary** — Key metrics, resource estimate
2. **Geology** — Play description, structural map link, seismic coverage
3. **Risk** — Chance of success breakdown (trap/reservoir/seal/charge)
4. **History** — Stage transitions, decisions, documents
5. **Related Wells** — Offset/control wells

**Actions** (context-dependent on stage):
- `[Advance Stage]` — opens transition dialog
- `[Request Well Program]` — starts well program wizard pre-filled with this prospect
- `[Drop Prospect]` — requires reason (soft delete to archived)

### Todo

| ID | Task | Status |
|----|------|--------|
| 2.3.a | Create `ProspectDetail.razor` page | 🔲 |
| 2.3.b | Build 2-column layout with `ProcessTimeline` on right | 🔲 |
| 2.3.c | Add 5-tab content sections | 🔲 |
| 2.3.d | Add resource estimate chart (P90/P50/P10 bar) | 🔲 |
| 2.3.e | Add risk breakdown table | 🔲 |
| 2.3.f | Add context-aware action buttons (change by stage) | 🔲 |
| 2.3.g | Wire to API: `GET /api/exploration/prospects/{id}` | 🔲 |

---

### 2.4 Well Program Approval Wizard

**Route**: `/exploration/well-program/new` and `/exploration/well-program/{id}`  
**File**: `Pages/PPDM39/Exploration/WellProgramApproval.razor`  
**Process**: Multi-step gate review to authorize an exploratory well  
**Pattern**: MudStepper (5 steps)

**Steps**:

| Step | Name | Content | Gate Question |
|------|------|---------|---------------|
| 1 | Well Objectives | Prospect link, target zone, expected result | Are the objectives clearly defined? |
| 2 | Technical Summary | Well design overview, depth, trajectory, expected formations | Is the technical plan adequate? |
| 3 | Risk Assessment | Technical risks (geological + operational), HSE hazards | Are risks acceptable? |
| 4 | Cost Estimate (AFE) | Detailed AFE line items, contingency, comparison to analogues | Is the budget approved? |
| 5 | Approval Sign-off | Summary card, approver sign-off button | Final go/no-go decision |

**At step 5**: Shows complete summary. Approver buttons:
- `[Approve Well Program]` → sets status to "Approved", issues drill order
- `[Defer to Next Cycle]` → requires reason and next review date
- `[Reject]` → requires reason, returns to engineer with comments

### Todo

| ID | Task | Status |
|----|------|--------|
| 2.4.a | Create `WellProgramApproval.razor` page with MudStepper | 🔲 |
| 2.4.b | Step 1: Objectives form (prospect selector, zone, objectives text) | 🔲 |
| 2.4.c | Step 2: Technical summary form | 🔲 |
| 2.4.d | Step 3: Risk assessment matrix | 🔲 |
| 2.4.e | Step 4: AFE line-item table with totals | 🔲 |
| 2.4.f | Step 5: Summary card + Approve/Defer/Reject buttons | 🔲 |
| 2.4.g | Wire to API: `POST /api/exploration/well-programs` | 🔲 |
| 2.4.h | Wire approval action to API: `POST /api/exploration/well-programs/{id}/approve` | 🔲 |

---

### 2.5 Seismic Survey Tracker

**Route**: `/exploration/seismic`  
**File**: `Pages/PPDM39/Exploration/SeismicTracker.razor`  
**Process**: Track seismic acquisition, processing, and interpretation status

**Layout**:
- Top: Summary KPIs (surveys active, coverage km², surveys complete this year)
- Main: `MudDataGrid` of surveys with columns:
  - Survey Name, Type (2D/3D), Status, Area (km²), Vintage, Linked Prospects
- Row expand: show processing/interpretation stages as `ProcessTimeline`
- Actions per row: `[View Details]`, `[Link to Prospect]`, `[Upload Interpretation]`

### Todo

| ID | Task | Status |
|----|------|--------|
| 2.5.a | Create `SeismicTracker.razor` page | 🔲 |
| 2.5.b | Add summary KPIs at top | 🔲 |
| 2.5.c | Add MudDataGrid with expandable rows | 🔲 |
| 2.5.d | Add row-level `ProcessTimeline` for survey stages | 🔲 |
| 2.5.e | Wire to API: `GET /api/exploration/seismic-surveys?fieldId=` | 🔲 |

---

## Definition of Done — Phase 2

- [ ] All 5 Exploration pages exist and build cleanly
- [ ] Prospect Board shows kanban layout with stage columns
- [ ] Prospect card shows resource estimate and risk metrics
- [ ] Well Program Approval wizard has all 5 steps
- [ ] No inline `style=""` in any new file
- [ ] All routes work from the updated NavMenu
- [ ] `KpiCard` and `StatusBadge` components used throughout
- [ ] `dotnet build` passes with 0 errors
